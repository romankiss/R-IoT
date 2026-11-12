// Core candidate revision: publish_async_v1 - UNVERIFIED; includes filter_address_operand_v1 and emit_event_snapshot_v1
using System;
using System.Collections;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace CodeCraft.Common
{
    // ── Delegate types ──
    public delegate string CommandHandler(string cmd, int msgId, string[] args);
    public delegate void EventHandler(string topic, int msgId, string filter, string[] args);

    // ── Generic delegate for FAN-IN sink ──
    public delegate void ConsumeForwarderX(string line);

    /// <summary>
    /// Describes a subscription from a consumer to this module's events.
    /// </summary>
    public class SubscriptionEntry
    {
        public string TopicPattern;
        public string FilterExpression;
        public string ConsumerAtGroup;
        public string DeliveryMode;      // PUSH | PUSH_PULL | PULL
        public bool Enabled;
        public DateTime SubscribedAt;

        public SubscriptionEntry(string topicPattern, string consumerAtGroup, string deliveryMode)
        {
            TopicPattern = topicPattern;
            FilterExpression = string.Empty;
            ConsumerAtGroup = consumerAtGroup;
            DeliveryMode = string.IsNullOrEmpty(deliveryMode) ? "PUSH" : deliveryMode.ToUpper();
            Enabled = true;
            SubscribedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Enqueued command routed to a module's inbox for async execution.
    /// </summary>
    public class AsyncMessage
    {
        public VirtualModule Module;
        public string Command;
        public int MsgId;
        public string[] Args;

        public AsyncMessage(VirtualModule module, string command, int msgId, string[] args)
        {
            Module = module;
            Command = command;
            MsgId = msgId;
            Args = args;
        }
    }

    /// <summary>
    /// GOLDEN LEVEL 0 RUNTIME CONTRACT — base class for every CodeCraft module.
    /// A module owns its worker, inbox, handlers, subscriptions and telemetry;
    /// modules are loosely coupled and linked by <see cref="ForwardTarget"/>.
    ///
    /// FEATURE INDEX (one line per capability):
    /// - Chain: Publish walks ChainHead → ForwardTarget and routes by AtGroup.
    /// - Commands: exact destination is synchronous; wildcard destination is asynchronous.
    /// - Query fan-out: wildcard SUBLIST is synchronous and returns every result/error in string[].
    /// - Handlers: derived modules register handlers disabled; Start enables them and starts the worker.
    /// - Local events: Subscribe binds an in-process delegate to a local topic pattern.
    /// - Cross events: SUB stores consumer interest on the module that produces the event.
    /// - Delivery: PUSH calls the consumer handler; PULL stores the event; PUSH_PULL does both.
    /// - Topics: emitted local topics start with '/'; handlers receive topicPath = AtGroup + localTopic.
    /// - Topic wildcards: '*', '?', '#' are supported; '/*' is one segment and '/**' is recursive.
    /// - Conditions: SUB receives topic and filter as separate mandatory arguments.
    /// - Filters: space=AND, unescaped '|'=OR, leading '!'=NOT and '\' escapes syntax characters.
    /// - Filter patterns: '*' matches a sequence, '?' one character and '#' zero or more digits; matching ignores case.
    /// - Argument aliases: $argN=pattern targets args[N]; an unqualified pattern targets the last arg.
    /// - Numeric aliases: $argN&gt;number, &gt;=, &lt; and &lt;= perform numeric comparisons; AND composes ranges.
    /// - Address operands: $argN=@pattern strips @ for text matching (also under leading !).
    /// - Address comparisons: $argN&gt;=@5000 parses both operands as unsigned hex.
    /// - Without @, numeric comparisons retain double.TryParse; no radix inference.
    /// - Hex integers must be exactly representable by double (maximum 0x1FFFFFFFFFFFFF).
    /// - Filter references: standalone @scope:name remains reserved (EFILTERREF:UNSUPPORTED).
    /// - Identity: producer + topic interest + consumer endpoint + delivery mode; producer is the owning module.
    /// - Filter update: SUB on an existing identity atomically updates only FilterExpression.
    /// - Hierarchy: broader topic patterns supersede narrower topic identities in the same scope.
    /// - Selectors: SUBLIST and UNSUB select identity by consumer/topic/delivery; delivery defaults to PUSH.
    /// - Context: msgId is a 4-byte token: 7-bit group + 24-bit non-zero ID, rendered as X8.
    /// - Instrumentation: TraceLog sends deterministic checkpoints to ConsumerHook or Debug output.
    /// - Runtime target: implementation avoids APIs unavailable on .NET nanoFramework.
    ///
    /// BUSINESS MODULE AUTHORING API:
    /// - Constructor: pass the human-readable Name and routable AtGroup to base(name, atGroup).
    /// - RegisterHandlers/RegisterHandler: register derived command handlers during Initialize(); Start() enables them.
    /// - DefaultAtCommandHandler: retain base built-ins and optionally provide a derived fallback for other commands.
    /// - DefaultEventHandler/DefaultHandler: receive cross-module event deliveries when no private local delegate applies.
    /// - EmitEvent: publish a local '/TOPIC' with typed string arguments and the current context.
    /// - Subscribe/Unsubscribe: bind or remove private in-process delegates using local topic patterns.
    /// - Publish: enter the module chain for exact synchronous or wildcard asynchronous command routing.
    /// - PublishAsync: queue exactly one endpoint command; return admission receipt, not completion.
    /// - Route/RouteAsync: execute directly on a known module; normally prefer Publish from business code.
    /// - SetTopicSwitch: pause/resume delivery while retaining subscriptions; SetTopicBlock grounds emission early.
    /// - EvalFilter/MatchTopic: reuse the Level 0 filter and topic semantics in business-module policy code.
    /// - MessageFormat: build canonical +OK/+DATA/+ERR/+EVT envelopes with an X8 context token.
    ///
    /// REUSABLE NANOFRAMEWORK HELPERS (Extensions):
    /// - Join/JoinKeys: deterministic delimited rendering for string arrays and Hashtable keys.
    /// - AddRange/CloneList: nanoFramework-safe ArrayList copy helpers; useful before mutation during iteration.
    /// - SplitCount: bounded split that preserves the unsplit remainder as the last element.
    /// - UnwrapTextArg: remove transport quotes only from a properly wrapped comma-bearing text value.
    /// - TextArg: rejoin a transport-split text payload from args[index..] and apply UnwrapTextArg.
    /// - WildcardMatchEscaped: case-insensitive '*', '?', '#' matching with backslash literal escaping.
    /// - WildcardMatch: case-insensitive raw wildcard matching for topics and selectors without escape parsing.
    ///
    /// BUILT-IN COMMAND INDEX:
    /// - SUB consumer,topic,filter,delivery — create a route or update only its filter.
    /// - SUBLIST consumerSelector,topicSelector[,deliverySelector] — list matching identities.
    /// - UNSUB consumerSelector,topicSelector[,deliverySelector] — remove matching identities.
    /// - TOGGLE consumer,topic[|filter],on|off — enable or disable an exact route.
    /// - SWITCH topic,on|off — enable or disable local topic dispatch.
    /// - PULL msgId — consume one pending PULL/PUSH_PULL event.
    ///
    /// ERROR INDEX:
    /// - ECONSUMER/ETOPIC/EDELIVERY — invalid route identity or delivery mode.
    /// - EFILTER:* — invalid filter grammar, alias, duplicate or wildcard hierarchy.
    /// - EFILTERREF:UNSUPPORTED — reserved @scope:name reference is not implemented.
    /// - ESUBHIER — an existing broader topic identity already covers the request.
    /// - ENOTFOUND — selected subscription or pending event does not exist.
    /// - ENOHANDLER/EUNKNOWN — command or destination cannot be resolved.
    ///
    /// PRIMARY TRACE CHECKPOINTS:
    /// - publish_entry/match/skip/nomatch — command FAN-OUT path through the chain.
    /// - route_entry/ok/default/err — synchronous handler decision and result.
    /// - async_enqueue/execute/result/default/err — worker-thread command path.
    /// - crosssub_add/update/remove/clear/list/toggle — subscription state changes and queries.
    /// - emit_event/deliver_event/handler_event — event source, delivery decision and consumer sink.
    /// - deliver_event_local/dispatch_event/handler_event_local — local delegate event path.
    ///
    /// GOLDEN INVARIANTS — generators and derived modules must preserve these rules:
    /// - Do not change CommandHandler, EventHandler, Publish or ConsumerHook delegate signatures.
    /// - Keep ChainHead and ForwardTarget as the only Level 0 module-routing topology.
    /// - Keep subscription ownership on the event-producing module, not on the consumer.
    /// - Keep subscription identity independent of FilterExpression.
    /// - For an existing identity, SUB may mutate only FilterExpression; preserve all identity fields and state.
    /// - Store no-filter as empty string; expose it canonically as '*'.
    /// - In SUBLIST/UNSUB, omitted delivery means PUSH while '*' selects every delivery mode.
    /// - In SUBLIST/UNSUB, topic selector '/*' is the explicit all-topics sentinel.
    /// - Preserve synchronous SUBLIST fan-out; all other wildcard commands remain asynchronous.
    /// - Resolve event interest before logging emit_event so uninterested events remain silent.
    /// - Keep filter text available to the destination handler for FAN-IN correlation/dispatch.
    /// - Context 00000000 means lifecycle/system activity without a correlation context.
    /// - The legacy API parameter name msgId carries the 32-bit context value; AppLog renders it as ctx.
    /// - Preserve locks around mutable Hashtables and queues; never remove during enumeration.
    ///
    /// CORE STATE:
    /// - _handlers: command → CommandHandler.
    /// - _subscriptions: local topicPattern → ArrayList&lt;EventHandler&gt;.
    /// - _crossSubs: topicPattern|consumerAtGroup|deliveryMode → SubscriptionEntry.
    /// - _topicMeta: topic → byte (bit0=switchEnabled, bit1=blocked).
    /// - _pendingEvents: msgId → EventItem for PULL/PUSH_PULL.
    ///
    /// Version: 3.0.2
    /// </summary>
    /*
    APPLOG PRODUCER FORMAT
    AT+PUB=<module>,<cmd>,<msgId>[,args[]]
    AT+PUB is the AppLog producer. The module is an exact or wildcard AtGroup
    destination, cmd is a module/built-in command, msgId is the travel/context ID
    and args are optional.

    APPLOG QUICK USAGE
    Copy the script lines between BEGIN and END into the AppLog script editor.
    The script contains no empty lines because an empty line stops Batch Run.

#+++++++++++++++++++++++++++++
#APPLOG_QUICK_USAGE
#+++++++++++++++++++++++++++++
#BEGIN
/log?clear&size=150
log?purge
.clear
#
#ASYNCHRONOUS_GLOBAL_BROADCAST_CLEANUP
AT+PUB=*,UNSUB,12345,*,/*,*
#
#EXACT_SYNCHRONOUS_MESSAGE
AT+PUB=LORA,INFO,12345
#
#ASYNCHRONOUS_WILDCARD_BROADCAST
AT+PUB=EMPTY?,INFO,12345
#
#CONDITIONAL_EVENT_SUBSCRIPTION
#consumer=EMPTY1 producer=LORA
AT+PUB=LORA,SUB,12345,EMPTY1,/QRX/PACK,$arg0=5000 *Hello*,PUSH
AT+PUB=LORA,SUB,12345,EMPTY1,/QRX/PACK,$arg0=@5000 *Hello*,PUSH
AT+PUB=LORA,SUB,12345,EMPTY0,/QRX/PACK,$arg0>=@5000 $arg0<=@5100 ACK_PING*,PUSH
#NOT example: !$arg0=@5000 ACK_PING*
#
#GENERATE_LORA_ACTIVITY
#delivery occurs only when the received event matches the subscription filter
AT+PUB=LORA,SEND,222,0x5000,"Hello ABBA"
#
#LIST_LORA_SUBSCRIPTIONS
AT+PUB=LORA,SUBLIST,12345,*,/*
#
#SYNCHRONOUS_WILDCARD_QUERY_WITH_RESULT_FAN_IN
AT+PUB=*,SUBLIST,12345,*,/*,*
#
#SELECTIVE_CLEANUP
AT+PUB=LORA,UNSUB,12345,EMPTY*,/QRX/*
#
#GLOBAL_CLEANUP_LORA
AT+PUB=LORA,UNSUB,12345,*,/*,*
#
#ASYNCHRONOUS_GLOBAL_BROADCAST_CLEANUP
AT+PUB=*,UNSUB,12345,*,/*,*
#
#VERIFY_FINAL_STATE_USING_RESULT_FAN_IN
AT+PUB=*,SUBLIST,12345,*,/*,*
#
#END
/log?
    */
    public class VirtualModule
    {
        // --- AppLog tool ---
        public static ConsumeForwarderX ConsumerHook { get; set; }

        // ── Static ──
        /// <summary>Chain head — set by Program after the chain is built</summary>
        public static VirtualModule ChainHead { get; set; }

        private const int MsgIdGroupMask = 0x7F;
        private const int MsgIdValueMask = 0x00FFFFFF;
        private static int _globalMsgIdCounter = 0;
        private static int _msgIdGroup = 0;

        /// <summary>
        /// Context producer group (0..127). Group 0 is reserved for local, legacy
        /// or otherwise unassigned producers; groups 1..127 are assignable.
        /// Configure once during platform initialization before publishing.
        /// </summary>
        public static int MsgIdGroup
        {
            get { return _msgIdGroup; }
            set
            {
                if (value < 0 || value > MsgIdGroupMask)
                    throw new ArgumentException("MsgIdGroup must be between 0 and 127.");
                _msgIdGroup = value;
            }
        }

        /// <summary>
        /// Allocate a non-zero 24-bit context ID inside the configured 7-bit group.
        /// The zero ID is skipped on wrap so 00000000 remains the unassigned token.
        /// </summary>
        public static int NextMsgId()
        {
            int id;
            do
            {
                id = Interlocked.Increment(ref _globalMsgIdCounter) & MsgIdValueMask;
            }
            while (id == 0);

            return (_msgIdGroup << 24) | id;
        }

        // ── Identity ──
        public string Name { get; private set; }
        public string AtGroup { get; private set; }
        public bool IsEnabled { get; private set; }

        // ── Telemetry (read-only externally) ──
        public long LastActivity { get; private set; }
        public int RxCount { get; private set; }

        // ── Chain ──
        /// <summary>Next module in the chain (null = end).</summary>
        public VirtualModule ForwardTarget { get; set; }

        // ── Handlers ──
        protected Hashtable _handlers;           // command → CommandHandler
        protected Hashtable _handlerEnabled;     // command → bool

        // ── Subscriptions ──
        protected Hashtable _subscriptions;      // topicPattern → ArrayList<EventHandler>
        protected Hashtable _crossSubs;          // "topicPattern|consumerAtGroup|deliveryMode" → SubscriptionEntry
        protected Hashtable _topicMeta;          // topic → byte (bit0=switchEnabled, bit1=blocked)
        protected Hashtable _pendingEvents;      // msgId → EventItem

        // ── Worker thread ──
        readonly ArrayList _inbox;                // AsyncMessage or EventItem
        readonly AutoResetEvent _inboxSignal;
        private Thread _workerThread;
        private bool _running;

        private static readonly long WatchdogTicks = Stopwatch.Frequency * 120; // 120 seconds

        // Topic meta flags
        private const byte TOPIC_SWITCH_ENABLED = 0x01;
        private const byte TOPIC_BLOCKED = 0x02;

        // ── FAN-IN hook for tracing ──
        // ConsumerHook is the connector for FAN-IN sink with a delegate signature:
        // public delegate void ConsumeForwarder(string line);
        /// <summary>
        /// Emit one deterministic runtime checkpoint to the configured FAN-IN sink;
        /// fall back to Debug output when no ConsumerHook is attached. AppLog names
        /// the formatted correlation value ctx; msgId remains the legacy API name.
        /// </summary>
        public static void TraceLog(string atGroup, string checkpoint, int msgId, string extra)
        {
            string msg = $"[TRACE] {atGroup}:{checkpoint} ctx={MessageFormat.FormatMsgId(msgId)}" +
                (extra != null ? " " + extra : "");
            if (ConsumerHook == null)
                Debug.WriteLine(msg);
            else
                ConsumerHook.Invoke(msg);
        }

        // ── Constructor ──
        /// <summary>
        /// Create an uninitialized module identity and its private Level 0 state;
        /// call Initialize() and Start() after linking the chain.
        /// </summary>
        public VirtualModule(string name, string atGroup)
        {
            if (string.IsNullOrEmpty(atGroup))
                throw new ArgumentException("AtGroup must not be empty");

            AtGroup = atGroup;
            Name = $"{name}_{atGroup}";

            _handlers = new Hashtable();
            _handlerEnabled = new Hashtable();
            _subscriptions = new Hashtable();
            _crossSubs = new Hashtable();
            _topicMeta = new Hashtable();
            _pendingEvents = new Hashtable();
            _inbox = new ArrayList();
            _inboxSignal = new AutoResetEvent(false);
            _running = false;
            IsEnabled = false;
            LastActivity = 0;
            RxCount = 0;
            ForwardTarget = null;
        }

        /// <summary>Default event handler delegate — set by Program for cross-sub deliveries without local subs.</summary>
        public EventHandler DefaultHandler { get; set; }

        // ── Lifecycle ──

        /// <summary>
        /// Call RegisterHandlers() for derived modules.
        /// No built-in handlers registered — SUB/SUBLIST/UNSUB/TOGGLE/SWITCH/PULL
        /// are handled by DefaultAtCommandHandler.
        /// </summary>
        public void Initialize()
        {
            RegisterHandlers();
        }

        /// <summary>Start the worker thread and enable all handlers.</summary>
        public void Start()
        {
            EnableAllHandlers();
            IsEnabled = true;
            LastActivity = Stopwatch.GetTimestamp();
            _running = true;
            _workerThread = new Thread(WorkerLoop);
            _workerThread.Start();
        }

        /// <summary>Stop the worker thread.</summary>
        public void Stop()
        {
            _running = false;
            _inboxSignal.Set();
            if (_workerThread != null)
            {
                _workerThread.Join();
                _workerThread = null;
            }
        }

        /// <summary>Override in derived modules to register AT command handlers.</summary>
        protected virtual void RegisterHandlers()
        {
            // default: no handlers
        }

        /// <summary>
        /// Default AT command handler — handles built-in SUB/SUBLIST/UNSUB/TOGGLE/SWITCH/PULL
        /// and any unregistered commands. Override in derived modules for custom
        /// fallback or DDR validation. Return null if not handled.
        /// </summary>
        protected virtual string DefaultAtCommandHandler(string command, int msgId, string[] args)
        {
            switch (command)
            {
                case "SUB":
                    return BuiltinSubHandler(msgId, args);
                case "SUBLIST":
                    return BuiltinSubListHandler(msgId, args);
                case "UNSUB":
                    return BuiltinUnsubHandler(msgId, args);
                case "TOGGLE":
                    return BuiltinToggleHandler(msgId, args);
                case "SWITCH":
                    return BuiltinSwitchHandler(msgId, args);
                case "PULL":
                    return BuiltinPullHandler(msgId, args);
                default:
                    return null; // not handled → ENOHANDLER
            }
        }

        /// <summary>
        /// Default event handler — called for events dispatched to this module
        /// when no specific subscription handler exists. Override in derived modules.
        /// </summary>
        protected virtual void DefaultEventHandler(string topic, int msgId, string filter, string[] args)
        {
            // default: no-op
        }

        // ── Worker thread ──

        /// <summary>
        /// Drain asynchronous commands and queued events in batches on this module's
        /// worker thread until Stop() clears the running state.
        /// </summary>
        private void WorkerLoop()
        {
            TraceLog(AtGroup, "WorkerLoop", 0, $"[{Name}] Worker thread started.");

            while (_running)
            {
                bool signaled = _inboxSignal.WaitOne(100_000, false);

                if (!signaled)
                    continue;

                ArrayList batch;
                lock (_inbox)
                {
                    if (_inbox.Count == 0) continue;
                    batch = Extensions.CloneList(_inbox);
                    _inbox.Clear();
                }

                LastActivity = Stopwatch.GetTimestamp();

                for (int i = 0; i < batch.Count; i++)
                {
                    object item = batch[i];

                    if (item is AsyncMessage)
                    {
                        var msg = item as AsyncMessage;
                        ExecuteAsync(msg.Command, msg.MsgId, msg.Args);
                    }
                    else if (item is EventItem)
                    {
                        var evt = item as EventItem;
                        TraceLog(AtGroup, "inbox_dequeue", evt.MsgId,
                            $"topic={evt.TopicPath} filter={FilterForLog(evt.FilterExpression)}");
                        DispatchIfSubscribed(evt.LocalTopic, evt.TopicPath, evt.MsgId,
                            evt.FilterExpression, evt.Args);
                    }
                }
            }

            TraceLog(AtGroup, "WorkerLoop", 0, $"Worker thread stopped.");
        }

        // ── Handler management ──

        /// <summary>Register a command handler (disabled by default).</summary>
        protected void RegisterHandler(string command, CommandHandler handler)
        {
            lock (_handlers)
            {
                _handlers[command] = handler;
                _handlerEnabled[command] = false;
            }
        }

        /// <summary>Remove a previously registered command handler and its enabled state.</summary>
        protected void UnregisterHandler(string command)
        {
            lock (_handlers)
            {
                _handlers.Remove(command);
                _handlerEnabled.Remove(command);
            }
        }

        /// <summary>Enable one registered command handler.</summary>
        protected void EnableHandler(string command)
        {
            lock (_handlerEnabled)
            {
                if (_handlerEnabled.Contains(command))
                    _handlerEnabled[command] = true;
            }
        }

        /// <summary>Disable one registered command handler without unregistering it.</summary>
        protected void DisableHandler(string command)
        {
            lock (_handlerEnabled)
            {
                if (_handlerEnabled.Contains(command))
                    _handlerEnabled[command] = false;
            }
        }

        /// <summary>Enable every registered handler; Start() invokes this after Initialize().</summary>
        protected void EnableAllHandlers()
        {
            lock (_handlerEnabled)
            {
                var keys = new ArrayList();
                keys.AddRange(_handlerEnabled.Keys);
                foreach (string cmd in keys)
                    _handlerEnabled[cmd] = true;
            }
        }

        /// <summary>Return true when a command is registered in the enabled state.</summary>
        protected bool IsHandlerEnabled(string command)
        {
            lock (_handlerEnabled)
            {
                return _handlerEnabled.Contains(command) && (bool)_handlerEnabled[command];
            }
        }

        /// <summary>Return true when a command handler is registered, regardless of enabled state.</summary>
        protected bool HasHandler(string command)
        {
            lock (_handlers)
            {
                return _handlers.Contains(command);
            }
        }

        // ── Local topic subscriptions ──

        /// <summary>Subscribe a handler to a topic pattern on this module.</summary>
        public void Subscribe(string topicPattern, EventHandler handler)
        {
            lock (_subscriptions)
            {
                if (!_subscriptions.Contains(topicPattern))
                    _subscriptions[topicPattern] = new ArrayList();
                var list = (ArrayList)_subscriptions[topicPattern];
                if (!list.Contains(handler))
                    list.Add(handler);
            }
        }

        /// <summary>Remove one local delegate binding and prune its empty topic bucket.</summary>
        public void Unsubscribe(string topicPattern, EventHandler handler)
        {
            lock (_subscriptions)
            {
                if (_subscriptions.Contains(topicPattern))
                {
                    var list = (ArrayList)_subscriptions[topicPattern];
                    list.Remove(handler);
                    if (list.Count == 0)
                        _subscriptions.Remove(topicPattern);
                }
            }
        }

        /// <summary>Return true when any local subscription pattern matches the topic.</summary>
        protected bool IsSubscribedTo(string topic)
        {
            lock (_subscriptions)
            {
                foreach (string pattern in _subscriptions.Keys)
                {
                    if (MatchTopic(topic, pattern))
                        return true;
                }
            }
            return false;
        }

        // ── Subscription management ──

        /// <summary>
        /// Register one consumer route on this event-producing module; validate the
        /// topic/filter, atomically update only the filter of an existing identity,
        /// and preserve topic hierarchy rules for distinct identities.
        /// </summary>
        public string CrossSubscribe(string consumerAtGroup, string topicPattern,
            string filterExpression, string deliveryMode, int msgId)
        {
            if (string.IsNullOrEmpty(consumerAtGroup))
                return MessageFormat.BuildError(this.AtGroup, msgId, "ECONSUMER");

            // SUB uses explicit topic and filter arguments. A pipe belongs only to
            // read/remove selectors (SUBLIST/UNSUB), never to a SUB topic identity.
            if (!string.IsNullOrEmpty(topicPattern) && topicPattern.IndexOf('|') >= 0)
                return MessageFormat.BuildError(this.AtGroup, msgId, "ETOPIC");

            string selectorError = ValidateSubscriptionSelector(topicPattern);
            if (!string.IsNullOrEmpty(selectorError))
                return MessageFormat.BuildError(this.AtGroup, msgId, selectorError);

            deliveryMode = string.IsNullOrEmpty(deliveryMode) ? "PUSH" : deliveryMode.ToUpper();
            if (deliveryMode != "PUSH" && deliveryMode != "PUSH_PULL" && deliveryMode != "PULL")
                return MessageFormat.BuildError(this.AtGroup, msgId, "EDELIVERY");

            string filterError = ValidateFilterExpression(filterExpression, true);
            if (!string.IsNullOrEmpty(filterError))
                return MessageFormat.BuildError(this.AtGroup, msgId, filterError);

            if (filterExpression == "*") filterExpression = string.Empty;
            string subscriptionSelector = topicPattern +
                (string.IsNullOrEmpty(filterExpression) ? "" : "|" + filterExpression);

            string key = BuildSubscriptionIdentity(topicPattern, consumerAtGroup, deliveryMode);

            lock (_crossSubs)
            {
                if (_crossSubs.Contains(key))
                {
                    var existing = (SubscriptionEntry)_crossSubs[key];
                    if (existing.FilterExpression == filterExpression)
                        return MessageFormat.BuildOk(this.AtGroup, msgId, $"sub,{subscriptionSelector},already");

                    string oldFilterExpression = existing.FilterExpression;
                    existing.FilterExpression = filterExpression;
                    TraceLog(AtGroup, "crosssub_update", msgId,
                        $"consumer={existing.ConsumerAtGroup} topic={existing.TopicPattern} " +
                        $"oldFilter={FilterForLog(oldFilterExpression)} " +
                        $"newFilter={FilterForLog(existing.FilterExpression)} mode={existing.DeliveryMode}");
                    return MessageFormat.BuildOk(this.AtGroup, msgId,
                        $"sub,{subscriptionSelector},updated=1");
                }

                var coveredKeys = new ArrayList();
                foreach (string existingKey in _crossSubs.Keys)
                {
                    var existing = (SubscriptionEntry)_crossSubs[existingKey];
                    if (existing.ConsumerAtGroup != consumerAtGroup ||
                        existing.DeliveryMode != deliveryMode)
                        continue;

                    if (TopicPatternCovers(existing.TopicPattern, topicPattern))
                        return MessageFormat.BuildError(this.AtGroup, msgId,
                            "ESUBHIER:" + BuildCanonicalSubscription(existing));

                    if (TopicPatternCovers(topicPattern, existing.TopicPattern))
                        coveredKeys.Add(existingKey);
                }

                for (int i = 0; i < coveredKeys.Count; i++)
                {
                    string coveredKey = (string)coveredKeys[i];
                    var covered = (SubscriptionEntry)_crossSubs[coveredKey];
                    _crossSubs.Remove(coveredKey);
                    TraceLog(AtGroup, "crosssub_remove", msgId,
                        $"consumer={covered.ConsumerAtGroup} topic={covered.TopicPattern} " +
                        $"filter={FilterForLog(covered.FilterExpression)} mode={covered.DeliveryMode} reason=hierarchy");
                }

                var entry = new SubscriptionEntry(topicPattern, consumerAtGroup, deliveryMode);
                entry.FilterExpression = filterExpression;
                _crossSubs[key] = entry;
                TraceLog(AtGroup, "crosssub_add", msgId,
                    $"consumer={consumerAtGroup} topic={topicPattern} filter={FilterForLog(filterExpression)} mode={deliveryMode}");
                return MessageFormat.BuildOk(this.AtGroup, msgId,
                    coveredKeys.Count > 0
                        ? $"sub,{subscriptionSelector},replaced={coveredKeys.Count}"
                        : $"sub,{subscriptionSelector}");
            }
        }

        /// <summary>
        /// Remove routes selected by consumer, topic and delivery identity patterns;
        /// omitted delivery means PUSH and '*' selects every delivery mode.
        /// </summary>
        public string CrossUnsubscribe(string consumerAtGroup, string topicPattern,
            string deliveryMode, int msgId)
        {
            if (string.IsNullOrEmpty(consumerAtGroup))
                return MessageFormat.BuildError(this.AtGroup, msgId, "ECONSUMER");

            string selectorError = ValidateSubscriptionSelector(topicPattern);
            if (!string.IsNullOrEmpty(selectorError))
                return MessageFormat.BuildError(this.AtGroup, msgId, selectorError);

            if (topicPattern.IndexOf('|') >= 0)
                return MessageFormat.BuildError(this.AtGroup, msgId, "ETOPIC");

            deliveryMode = string.IsNullOrEmpty(deliveryMode) ? "PUSH" : deliveryMode.ToUpper();
            if (deliveryMode != "*" && deliveryMode != "PUSH" &&
                deliveryMode != "PUSH_PULL" && deliveryMode != "PULL")
                return MessageFormat.BuildError(this.AtGroup, msgId, "EDELIVERY");

            lock (_crossSubs)
            {
                // In UNSUB selectors, /* is the explicit sentinel for all topics.
                // Keep the fast clear only for the fully unqualified identity selector.
                bool allTopics = topicPattern == "/*";
                if (consumerAtGroup == "*" && allTopics && deliveryMode == "*")
                {
                    var numberOfSubs = _crossSubs.Count;
                    _crossSubs.Clear();
                    TraceLog(AtGroup, "crosssub_clear", msgId,
                        $"all {numberOfSubs} subscriptions removed for consumers and topics");
                    return MessageFormat.BuildOk(this.AtGroup, msgId, "unsub,*,/*,*");
                }

                var toRemove = new ArrayList();
                foreach (string key in _crossSubs.Keys)
                {
                    var entry = (SubscriptionEntry)_crossSubs[key];

                    if (MatchSubscriptionSelector(entry, consumerAtGroup,
                        topicPattern, allTopics, deliveryMode))
                        toRemove.Add(key);
                }
                foreach (string key in toRemove)
                {
                    var entry = (SubscriptionEntry)_crossSubs[key];
                    _crossSubs.Remove(key);
                    TraceLog(AtGroup, "crosssub_remove", msgId,
                        $"consumer={entry.ConsumerAtGroup} topic={entry.TopicPattern} " +
                        $"filter={FilterForLog(entry.FilterExpression)} mode={entry.DeliveryMode}");
                }
                if (toRemove.Count > 0)
                    return MessageFormat.BuildOk(this.AtGroup, msgId,
                        $"unsub,{consumerAtGroup},{topicPattern},{deliveryMode},removed={toRemove.Count}");
            }
            return MessageFormat.BuildError(this.AtGroup, msgId, "ENOTFOUND");
        }

        /// <summary>
        /// Return canonical subscriptions selected with the same non-mutating selector
        /// semantics used by CrossUnsubscribe.
        /// </summary>
        public string CrossSubscriptionList(string consumerAtGroup,
            string topicPattern, string deliveryMode, int msgId)
        {
            if (string.IsNullOrEmpty(consumerAtGroup))
                return MessageFormat.BuildError(this.AtGroup, msgId, "ECONSUMER");

            string selectorError = ValidateSubscriptionSelector(topicPattern);
            if (!string.IsNullOrEmpty(selectorError))
                return MessageFormat.BuildError(this.AtGroup, msgId, selectorError);

            if (topicPattern.IndexOf('|') >= 0)
                return MessageFormat.BuildError(this.AtGroup, msgId, "ETOPIC");

            deliveryMode = string.IsNullOrEmpty(deliveryMode) ? "PUSH" : deliveryMode.ToUpper();
            if (deliveryMode != "*" && deliveryMode != "PUSH" &&
                deliveryMode != "PUSH_PULL" && deliveryMode != "PULL")
                return MessageFormat.BuildError(this.AtGroup, msgId, "EDELIVERY");
            bool allTopics = topicPattern == "/*";

            var subscriptions = new StringBuilder();
            int count = 0;
            lock (_crossSubs)
            {
                foreach (string key in _crossSubs.Keys)
                {
                    var entry = (SubscriptionEntry)_crossSubs[key];
                    if (!MatchSubscriptionSelector(entry, consumerAtGroup,
                        topicPattern, allTopics, deliveryMode))
                        continue;

                    if (subscriptions.Length > 0) subscriptions.Append(",");
                    subscriptions.Append(BuildCanonicalSubscription(entry));
                    count++;
                }
            }

            string listOfSubscriptions = $"count={count} [{subscriptions}]";
            TraceLog(AtGroup, "crosssub_list", msgId,
                $"consumer={consumerAtGroup} topic={topicPattern} " +
                $"mode={deliveryMode} {listOfSubscriptions}");
            return MessageFormat.BuildOk(this.AtGroup, msgId,
                $"sub_list,{consumerAtGroup},{topicPattern},{deliveryMode},{listOfSubscriptions}");
        }

        /// <summary>
        /// Shared SUBLIST/UNSUB predicate over immutable subscription identity fields.
        /// </summary>
        private static bool MatchSubscriptionSelector(SubscriptionEntry entry,
            string consumerSelector, string topicSelector, bool allTopics,
            string deliverySelector)
        {
            bool consumerMatches =
                MatchDestination(consumerSelector, entry.ConsumerAtGroup);
            bool topicMatches = allTopics ||
                MatchTopic(entry.TopicPattern, topicSelector);
            bool deliveryMatches = deliverySelector == "*" ||
                entry.DeliveryMode == deliverySelector;

            return consumerMatches && topicMatches && deliveryMatches;
        }

        /// <summary>Enable or disable an exact stored cross-subscription without deleting it.</summary>
        public string CrossToggle(string consumerAtGroup, string topicPattern, bool enabled, int msgId)
        {
            if (string.IsNullOrEmpty(consumerAtGroup))
                return MessageFormat.BuildError(this.AtGroup, msgId, "ECONSUMER");

            string selectorError = ValidateSubscriptionSelector(topicPattern);
            if (!string.IsNullOrEmpty(selectorError))
                return MessageFormat.BuildError(this.AtGroup, msgId, selectorError);

            bool filterSpecified = topicPattern.IndexOf('|') >= 0;
            string filterExpression = string.Empty;
            int filterDelimiter = topicPattern.IndexOf('|');
            if (filterDelimiter >= 0)
            {
                filterExpression = topicPattern.Substring(filterDelimiter + 1);
                topicPattern = topicPattern.Substring(0, filterDelimiter);
            }
            if (filterExpression == "*") filterExpression = string.Empty;

            lock (_crossSubs)
            {
                bool found = false;
                foreach (string key in _crossSubs.Keys)
                {
                    var entry = (SubscriptionEntry)_crossSubs[key];
                    if (entry.TopicPattern == topicPattern &&
                        entry.ConsumerAtGroup == consumerAtGroup &&
                        ((!filterSpecified && string.IsNullOrEmpty(entry.FilterExpression)) ||
                         (filterSpecified && entry.FilterExpression == filterExpression)))
                    {
                        entry.Enabled = enabled;
                        found = true;
                    }
                }
                if (found)
                {
                    TraceLog(AtGroup, "crosssub_toggle", msgId,
                        $"topic={topicPattern} consumer={consumerAtGroup} enabled={enabled}");
                    return MessageFormat.BuildOk(this.AtGroup, msgId, $"toggle,{topicPattern},{(enabled ? "on" : "off")}");
                }
            }
            return MessageFormat.BuildError(this.AtGroup, msgId, "ENOTFOUND");
        }

        /// <summary>Build the route key; FilterExpression is intentionally excluded.</summary>
        private static string BuildSubscriptionIdentity(string topicPattern,
            string consumerAtGroup, string deliveryMode)
        {
            return $"{topicPattern}|{consumerAtGroup}|{deliveryMode}";
        }

        /// <summary>Serialize a stored subscription as topic|filter|consumer|delivery.</summary>
        private static string BuildCanonicalSubscription(SubscriptionEntry entry)
        {
            return $"{entry.TopicPattern}|{CanonicalFilter(entry.FilterExpression)}|" +
                $"{entry.ConsumerAtGroup}|{entry.DeliveryMode}";
        }

        /// <summary>Expose the internally empty no-filter value as canonical '*'.</summary>
        private static string CanonicalFilter(string filterExpression)
        {
            return string.IsNullOrEmpty(filterExpression) ? "*" : filterExpression;
        }

        /// <summary>Quote and escape a canonical filter for deterministic TraceLog parsing.</summary>
        public static string FilterForLog(string filterExpression)
        {
            string value = CanonicalFilter(filterExpression);
            var result = new StringBuilder();
            result.Append("'");
            for (int i = 0; i < value.Length; i++)
            {
                if (value[i] == '\'')
                {
                    int precedingBackslashes = 0;
                    for (int j = i - 1; j >= 0 && value[j] == '\\'; j--)
                        precedingBackslashes++;
                    if ((precedingBackslashes & 1) == 0) result.Append("\\");
                }
                result.Append(value[i]);
            }
            result.Append("'");
            return result.ToString();
        }

        /// <summary>Determine whether a broader topic interest makes a narrower route redundant.</summary>
        private static bool TopicPatternCovers(string higherPattern, string lowerPattern)
        {
            if (string.IsNullOrEmpty(higherPattern) || string.IsNullOrEmpty(lowerPattern))
                return false;
            if (higherPattern == lowerPattern) return true;
            if (higherPattern == "**" || higherPattern == "/**") return true;

            if (IsExactTopicPattern(lowerPattern))
                return MatchTopic(lowerPattern, higherPattern);

            if (higherPattern.EndsWith("/**"))
            {
                string prefix = higherPattern.Substring(0, higherPattern.Length - 3);
                return lowerPattern == prefix || lowerPattern.StartsWith(prefix + "/");
            }

            if (higherPattern.EndsWith("/*") && lowerPattern.EndsWith("/*"))
            {
                string higherPrefix = higherPattern.Substring(0, higherPattern.Length - 2);
                string lowerPrefix = lowerPattern.Substring(0, lowerPattern.Length - 2);
                return higherPrefix == lowerPrefix;
            }

            return false;
        }

        private static bool IsExactTopicPattern(string pattern)
        {
            return pattern.IndexOf('*') < 0 && pattern.IndexOf('?') < 0 &&
                pattern.IndexOf('#') < 0;
        }

        /// <summary>Validate the structural topic portion before filter parsing.</summary>
        private static string ValidateSubscriptionSelector(string subscriptionSelector)
        {
            if (string.IsNullOrEmpty(subscriptionSelector)) return "ETOPIC";

            int filterDelimiter = subscriptionSelector.IndexOf('|');
            string topicPattern = filterDelimiter >= 0
                ? subscriptionSelector.Substring(0, filterDelimiter)
                : subscriptionSelector;

            if (string.IsNullOrEmpty(topicPattern) || !topicPattern.StartsWith("/"))
                return "ETOPIC";

            return "";
        }

        /// <summary>
        /// Validate filter grammar, aliases, duplicate terms/groups and universal wildcard hierarchy.
        /// </summary>
        private static string ValidateFilterExpression(string filterExpression, bool filterSpecified)
        {
            if (!filterSpecified) return "";
            if (string.IsNullOrEmpty(filterExpression)) return "EFILTER:EMPTY";
            if (filterExpression == "*") return "";

            int trailingBackslashes = 0;
            for (int i = filterExpression.Length - 1;
                i >= 0 && filterExpression[i] == '\\'; i--)
                trailingBackslashes++;
            if ((trailingBackslashes & 1) != 0)
                return "EFILTER:DANGLING_ESCAPE";

            var groups = new ArrayList();
            bool hasUniversalGroup = false;
            int groupStart = 0;

            for (int i = 0; i <= filterExpression.Length; i++)
            {
                if (i == filterExpression.Length ||
                    (filterExpression[i] == '|' && !IsEscaped(filterExpression, i)))
                {
                    if (i == groupStart) return "EFILTER:EMPTY_OR";

                    string group = filterExpression.Substring(groupStart, i - groupStart);
                    var terms = new Hashtable();
                    bool groupHasUniversal = false;
                    int termStart = 0;

                    for (int j = 0; j <= group.Length; j++)
                    {
                        if (j == group.Length ||
                            (group[j] == ' ' && !IsEscaped(group, j)))
                        {
                            if (j == termStart) return "EFILTER:EMPTY_TERM";

                            string term = group.Substring(termStart, j - termStart);
                            string canonicalTerm;
                            bool universalTerm;
                            string termError = ValidateFilterTerm(term,
                                out canonicalTerm, out universalTerm);
                            if (!string.IsNullOrEmpty(termError)) return termError;
                            if (terms.Contains(canonicalTerm)) return "EFILTER:DUPLICATE";

                            terms[canonicalTerm] = true;
                            if (universalTerm) groupHasUniversal = true;
                            termStart = j + 1;
                        }
                    }

                    if (groupHasUniversal && terms.Count > 1)
                        return "EFILTER:HIERARCHY";

                    for (int g = 0; g < groups.Count; g++)
                    {
                        if (SameFilterTerms((Hashtable)groups[g], terms))
                            return "EFILTER:DUPLICATE";
                    }

                    groups.Add(terms);
                    if (groupHasUniversal) hasUniversalGroup = true;
                    if (hasUniversalGroup && groups.Count > 1)
                        return "EFILTER:HIERARCHY";

                    groupStart = i + 1;
                }
            }

            return "";
        }

        /// <summary>Validate and canonicalize one optional-NOT filter term.</summary>
        private static string ValidateFilterTerm(string term,
            out string canonicalTerm, out bool universalTerm)
        {
            canonicalTerm = "";
            universalTerm = false;

            bool negate = false;
            int start = 0;
            while (start < term.Length && term[start] == '!')
            {
                negate = !negate;
                start++;
            }
            if (start >= term.Length) return "EFILTER:NOT";

            string operand = term.Substring(start);
            if (operand.StartsWith("@")) return "EFILTERREF:UNSUPPORTED";

            int argIndex = -1;
            string pattern = operand;
            string filterOperator = "=";
            if (operand.StartsWith("$arg"))
            {
                if (!TryParseAliasTerm(operand, out argIndex,
                    out filterOperator, out pattern))
                    return "EFILTER:ARG";

                if (pattern.Length == 0) return "EFILTER:ARG_PATTERN";

                bool addressOperand = pattern[0] == '@';
                string numericPattern = addressOperand ? pattern.Substring(1) : pattern;
                if (numericPattern.Length == 0) return "EFILTER:ARG_PATTERN";
                if (filterOperator != "=")
                {
                    double numericValue;
                    if (!TryParseFilterNumber(numericPattern, addressOperand, out numericValue))
                        return "EFILTER:NUMBER";
                }
                else if (addressOperand)
                {
                    // =@5000 and =5000 have identical text-match semantics.
                    pattern = numericPattern;
                }
            }

            if (pattern.Length == 0) return "EFILTER:EMPTY_TERM";

            string target = argIndex >= 0 ? "$arg" + argIndex.ToString() : "$last";
            canonicalTerm = (negate ? "!" : "") + target +
                filterOperator + pattern.ToLower();
            universalTerm = !negate && argIndex < 0 && pattern == "*";
            return "";
        }

        /// <summary>
        /// Parse $argN followed by '=', '&gt;', '&gt;=', '&lt;' or '&lt;=' and its operand.
        /// </summary>
        private static bool TryParseAliasTerm(string operand, out int argIndex,
            out string filterOperator, out string value)
        {
            argIndex = -1;
            filterOperator = "";
            value = "";

            if (string.IsNullOrEmpty(operand) || !operand.StartsWith("$arg"))
                return false;

            int operatorIndex = 4;
            while (operatorIndex < operand.Length &&
                operand[operatorIndex] >= '0' && operand[operatorIndex] <= '9')
                operatorIndex++;

            if (operatorIndex == 4 ||
                !int.TryParse(operand.Substring(4, operatorIndex - 4), out argIndex) ||
                argIndex < 0 || operatorIndex >= operand.Length)
                return false;

            char first = operand[operatorIndex];
            if ((first == '>' || first == '<') &&
                operatorIndex + 1 < operand.Length && operand[operatorIndex + 1] == '=')
            {
                filterOperator = operand.Substring(operatorIndex, 2);
                operatorIndex += 2;
            }
            else if (first == '=' || first == '>' || first == '<')
            {
                filterOperator = operand.Substring(operatorIndex, 1);
                operatorIndex++;
            }
            else
            {
                return false;
            }

            if (operatorIndex >= operand.Length) return false;
            value = operand.Substring(operatorIndex);
            return true;
        }

        private static bool SameFilterTerms(Hashtable left, Hashtable right)
        {
            if (left == null || right == null || left.Count != right.Count) return false;
            foreach (object key in left.Keys)
            {
                if (!right.Contains(key)) return false;
            }
            return true;
        }

        // ── Topic meta (merged switches + blocks) ──

        private byte GetTopicMeta(string topic)
        {
            lock (_topicMeta)
            {
                if (_topicMeta.Contains(topic))
                    return (byte)_topicMeta[topic];
            }
            return TOPIC_SWITCH_ENABLED; // default: switch enabled, not blocked
        }

        private void SetTopicMeta(string topic, byte flags)
        {
            lock (_topicMeta)
            {
                _topicMeta[topic] = flags;
            }
        }

        /// <summary>Return true when delivery for a local topic is switched off but not removed.</summary>
        protected bool IsTopicSwitchedOff(string topic)
        {
            return (GetTopicMeta(topic) & TOPIC_SWITCH_ENABLED) == 0;
        }

        /// <summary>Enable or pause delivery for a local topic while preserving subscription state.</summary>
        protected void SetTopicSwitch(string topic, bool enabled)
        {
            byte meta = GetTopicMeta(topic);
            if (enabled)
                meta |= TOPIC_SWITCH_ENABLED;
            else
                meta &= unchecked((byte)~TOPIC_SWITCH_ENABLED);
            SetTopicMeta(topic, meta);
            TraceLog(AtGroup, "switch_topic", 0, $"topic={topic} enabled={enabled}");
        }

        /// <summary>Return true when a local topic is blocked at the producer before emission.</summary>
        protected bool IsTopicBlocked(string topic)
        {
            return (GetTopicMeta(topic) & TOPIC_BLOCKED) != 0;
        }

        /// <summary>Block or unblock a local topic at the producer; blocked events are grounded silently.</summary>
        protected void SetTopicBlock(string topic, bool blocked)
        {
            byte meta = GetTopicMeta(topic);
            if (blocked)
                meta |= TOPIC_BLOCKED;
            else
                meta &= unchecked((byte)~TOPIC_BLOCKED);
            SetTopicMeta(topic, meta);
            TraceLog(AtGroup, "block_topic", 0, $"topic={topic} blocked={blocked}");
        }

        // ── Built-in command handlers (via DefaultAtCommandHandler) ──

        /// <summary>Validate the explicit SUB identity/filter arguments and create or update the route.</summary>
        private string BuiltinSubHandler(int msgId, string[] args)
        {
            if (args == null || args.Length != 4)
                return MessageFormat.BuildError(this.AtGroup, msgId,
                    "EARGS:SUB,consumer,topic,filter,delivery");

            string consumerAtGroup = args[0];
            string topic = args[1].UnwrapTextArg();
            string filter = args[2].UnwrapTextArg();
            string delivery = args[3];

            return CrossSubscribe(consumerAtGroup, topic, filter, delivery, msgId);
        }

        /// <summary>Validate SUBLIST selectors and return a non-mutating canonical list.</summary>
        private string BuiltinSubListHandler(int msgId, string[] args)
        {
            if (args == null || args.Length < 2 || args.Length > 3)
                return MessageFormat.BuildError(this.AtGroup, msgId,
                    "EARGS:SUBLIST,consumer,topic[,delivery]");

            string consumerAtGroup = args[0];
            string topic = args[1].UnwrapTextArg();
            string delivery = args.Length == 3 ? args[2] : "PUSH";
            return CrossSubscriptionList(consumerAtGroup, topic, delivery, msgId);
        }

        /// <summary>Validate UNSUB selectors and delegate route removal to CrossUnsubscribe.</summary>
        private string BuiltinUnsubHandler(int msgId, string[] args)
        {
            if (args == null || args.Length < 2 || args.Length > 3)
                return MessageFormat.BuildError(this.AtGroup, msgId,
                    "EARGS:UNSUB,consumer,topic[,delivery]");

            string consumerAtGroup = args[0];
            string topic = args[1].UnwrapTextArg();
            string delivery = args.Length == 3 ? args[2] : "PUSH";
            return CrossUnsubscribe(consumerAtGroup, topic, delivery, msgId);
        }

        /// <summary>Apply an exact subscription enabled-state change.</summary>
        private string BuiltinToggleHandler(int msgId, string[] args)
        {
            if (args == null || args.Length != 3)
                return MessageFormat.BuildError(this.AtGroup, msgId, "EARGS:TOGGLE,consumer,topic,onoff");

            string consumerAtGroup = args[0];
            string topic = args[1].UnwrapTextArg();
            bool enabled = args[2] == "1" || args[2].ToLower() == "on" || args[2].ToLower() == "true";

            return CrossToggle(consumerAtGroup, topic, enabled, msgId);
        }

        /// <summary>Switch local dispatch for a topic on or off without deleting subscriptions.</summary>
        private string BuiltinSwitchHandler(int msgId, string[] args)
        {
            if (args == null || args.Length < 2)
                return MessageFormat.BuildError(this.AtGroup, msgId, "EARGS:SWITCH,topic,onoff");

            string topic = args[0];
            bool enabled = args[1] == "1" || args[1].ToLower() == "on" || args[1].ToLower() == "true";

            SetTopicSwitch(topic, enabled);
            return MessageFormat.BuildOk(this.AtGroup, msgId, $"switch,{topic},{(enabled ? "on" : "off")}");
        }

        /// <summary>Consume and remove one pending PULL/PUSH_PULL event by msgId.</summary>
        private string BuiltinPullHandler(int msgId, string[] args)
        {
            if (args == null || args.Length < 1)
                return MessageFormat.BuildError(this.AtGroup, msgId, "EARGS:PULL,msgId");

            if (!MessageFormat.TryParseMsgId(args[0], out int pullMsgId))
                return MessageFormat.BuildError(this.AtGroup, msgId, "EBADMSGID");

            lock (_pendingEvents)
            {
                if (_pendingEvents.Contains(pullMsgId))
                {
                    var evt = (EventItem)_pendingEvents[pullMsgId];
                    _pendingEvents.Remove(pullMsgId);
                    TraceLog(AtGroup, "pull_event", msgId,
                        $"topic={evt.TopicPath} filter={FilterForLog(evt.FilterExpression)} " +
                        $"pullCtx={MessageFormat.FormatMsgId(pullMsgId)}");
                    return MessageFormat.BuildEvent(evt.TopicPath, evt.MsgId, evt.Args);
                }
            }
            return MessageFormat.BuildError(this.AtGroup, msgId, "ENOTFOUND");
        }

        // ── Destination matching ──

        /// <summary>Match an AtGroup using '*', '?' and numeric '#' destination wildcards.</summary>
        private static bool MatchDestination(string pattern, string atGroup)
        {
            if (pattern == "*" || pattern == "**")
                return true;
            if (pattern == atGroup)
                return true;

            int pi = 0, gi = 0;
            int pLen = pattern.Length, gLen = atGroup.Length;

            while (pi < pLen)
            {
                char pc = pattern[pi];

                if (pc == '*')
                {
                    if (pi == pLen - 1)
                        return true;
                    pi++;
                    if (pi >= pLen) return true;
                    char next = pattern[pi];
                    while (gi < gLen && atGroup[gi] != next) gi++;
                    if (gi >= gLen) return false;
                }
                else if (pc == '?')
                {
                    if (gi >= gLen) return false;
                    pi++;
                    gi++;
                }
                else if (pc == '#')
                {
                    if (gi >= gLen || atGroup[gi] < '0' || atGroup[gi] > '9') return false;
                    pi++;
                    gi++;
                }
                else
                {
                    if (gi >= gLen || pc != atGroup[gi]) return false;
                    pi++;
                    gi++;
                }
            }

            return gi == gLen;
        }

        // ── Publish ──

        /// <summary>
        /// Deliver a command to one or more modules in the chain.
        /// Exact destination: synchronous Route, break on first match.
        /// SUBLIST wildcard: synchronous read-only fan-out, collect all results.
        /// Other wildcards: RouteAsync to all matching modules.
        /// This is a FAN-OUT entry point with a delegate signature:
        //  public delegate string[] PublishForwarder(string destination, string command, int msgId, params string[] args);
        /// </summary>
        public string[] Publish(string destination, string command, int msgId, params string[] args)
        {
            if (msgId == 0)
                msgId = NextMsgId();

            var responses = new ArrayList();
            bool hasDestinationWildcard = destination.Contains("*")
                       || destination.Contains("?")
                       || destination.Contains("#");
            bool isSync = !hasDestinationWildcard;
            bool isSyncFanout = hasDestinationWildcard && command == "SUBLIST";

            TraceLog(this.AtGroup, "publish_entry", msgId,
                $"dest={destination} cmd={command} args={args.Join()} " +
                $"sync={isSync || isSyncFanout} fanout={isSyncFanout}");

            var module = this;
            bool matched = false;

            while (module != null)
            {
                bool moduleMatches = MatchDestination(destination, module.AtGroup);

                if (moduleMatches)
                {
                    matched = true;
                    TraceLog(this.AtGroup, "publish_match", msgId,
                        $"dest={destination} cmd={command} module={module.AtGroup}");

                    if (isSync)
                    {
                        string result = module.Route(command, msgId, args);
                        responses.Add(result);
                        break;
                    }
                    else if (isSyncFanout)
                    {
                        // SUBLIST is a read-only query. Preserve every matching
                        // module's real result/error in the returned string[].
                        responses.Add(module.Route(command, msgId, args));
                    }
                    else
                    {
                        module.RouteAsync(command, msgId, args);
                        responses.Add(MessageFormat.BuildOk(module.AtGroup, msgId));
                    }
                }
                else
                {
                    TraceLog(this.AtGroup, "publish_skip", msgId,
                        $"dest={destination} cmd={command} module={module.AtGroup}");
                }

                module = module.ForwardTarget;
            }

            if (!matched)
            {
                TraceLog(this.AtGroup, "publish_nomatch", msgId,
                    $"dest={destination} cmd={command}");
                responses.Add(MessageFormat.BuildError(module?.AtGroup ?? "?", msgId, "EUNKNOWN"));
            }

            return (string[])responses.ToArray(typeof(string));
        }

        /// <summary>
        /// Queue one exact endpoint command on its existing worker. The returned
        /// CommandEnqueued receipt is not the execution result; completion uses
        /// existing /RESULT or /ERROR events and the same context.
        /// Wildcard destinations are rejected. Publish behavior is unchanged.
        /// </summary>
        public string[] PublishAsync(string destination, string command, int msgId, params string[] args)
        {
            if (msgId == 0) msgId = NextMsgId();
            if (string.IsNullOrEmpty(destination) || destination.IndexOf('*') >= 0 ||
                destination.IndexOf('?') >= 0 || destination.IndexOf('#') >= 0)
                return new string[] { MessageFormat.BuildError(AtGroup, msgId, "EDEST") };
            if (string.IsNullOrEmpty(command))
                return new string[] { MessageFormat.BuildError(AtGroup, msgId, "EARGS") };

            TraceLog(AtGroup, "publish_async_entry", msgId,
                "dest=" + destination + " cmd=" + command + " args=" + args.Join());
            var module = this;
            while (module != null)
            {
                if (MatchDestination(destination, module.AtGroup))
                {
                    if (!module.IsEnabled || !module._running)
                    {
                        TraceLog(AtGroup, "publish_async_err", msgId,
                            "dest=" + destination + " cmd=" + command + " error=ENOTREADY");
                        return new string[] { MessageFormat.BuildError(module.AtGroup, msgId, "ENOTREADY") };
                    }
                    // Snapshot the caller-owned array before asynchronous execution.
                    string[] queuedArgs = new string[args == null ? 0 : args.Length];
                    for (int i = 0; i < queuedArgs.Length; i++) queuedArgs[i] = args[i];
                    TraceLog(AtGroup, "publish_async_match", msgId,
                        "dest=" + destination + " cmd=" + command + " module=" + module.AtGroup);
                    module.RouteAsync(command, msgId, queuedArgs);
                    return new string[] { MessageFormat.BuildOk(module.AtGroup, msgId, "CommandEnqueued") };
                }
                module = module.ForwardTarget;
            }
            TraceLog(AtGroup, "publish_async_nomatch", msgId, "dest=" + destination + " cmd=" + command);
            return new string[] { MessageFormat.BuildError(AtGroup, msgId, "EUNKNOWN") };
        }

        // ── Route (sync) ──

        /// <summary>
        /// Execute a command handler synchronously on this module's calling thread.
        /// Falls back to DefaultAtCommandHandler if no handler is registered.
        /// </summary>
        public string Route(string command, int msgId, string[] args)
        {
            RxCount++;
            LastActivity = Stopwatch.GetTimestamp();

            TraceLog(AtGroup, "route_entry", msgId,
                $"cmd={command} args={args.Join()}");

            CommandHandler handler = null;
            bool enabled = false;

            lock (_handlers)
            {
                if (_handlers.Contains(command))
                {
                    handler = (CommandHandler)_handlers[command];
                    enabled = IsHandlerEnabled(command);
                }
            }

            if (handler == null || !enabled)
            {
                string fallback = DefaultAtCommandHandler(command, msgId, args);
                if (fallback != null)
                {
                    LastActivity = Stopwatch.GetTimestamp();
                    TraceLog(AtGroup, "route_default", msgId,
                        $"cmd={command} result={fallback}");
                    return fallback;
                }
                TraceLog(AtGroup, "route_err", msgId,
                    $"cmd={command} error=ENOHANDLER");
                return MessageFormat.BuildError(this.AtGroup, msgId, "ENOHANDLER");
            }

            try
            {
                string result = handler(command, msgId, args);
                LastActivity = Stopwatch.GetTimestamp();
                TraceLog(AtGroup, "route_ok", msgId,
                    $"cmd={command} result={result}");
                return result ?? MessageFormat.BuildOk(this.AtGroup, msgId);
            }
            catch (Exception ex)
            {
                TraceLog(AtGroup, "route_err", msgId,
                    $"cmd={command} error={ex.Message}");
                return MessageFormat.BuildError(this.AtGroup, msgId, "EHANDLER:" + ex.Message);
            }
        }

        // ── RouteAsync ──

        /// <summary>Enqueue a command to this module's inbox for async execution.</summary>
        public void RouteAsync(string command, int msgId, string[] args)
        {
            RxCount++;
            LastActivity = Stopwatch.GetTimestamp();

            TraceLog(AtGroup, "async_enqueue", msgId,
                $"cmd={command} args={args.Join()}");

            lock (_inbox)
            {
                _inbox.Add(new AsyncMessage(this, command, msgId, args));
            }
            _inboxSignal.Set();
        }

        // ── ExecuteAsync (called by own worker thread) ──

        /// <summary>
        /// Execute a queued command on the worker and convert its result/error into
        /// /RESULT or /ERROR event flow for asynchronous FAN-IN.
        /// </summary>
        private void ExecuteAsync(string command, int msgId, string[] args)
        {
            TraceLog(AtGroup, "async_execute", msgId,
                $"cmd={command} args={args.Join()}");

            CommandHandler handler = null;
            bool enabled = false;

            lock (_handlers)
            {
                if (_handlers.Contains(command))
                {
                    handler = (CommandHandler)_handlers[command];
                    enabled = IsHandlerEnabled(command);
                }
            }

            if (handler == null || !enabled)
            {
                string fallback = DefaultAtCommandHandler(command, msgId, args);
                if (fallback != null)
                {
                    LastActivity = Stopwatch.GetTimestamp();
                    TraceLog(AtGroup, "async_default", msgId,
                        $"cmd={command} result={fallback}");
                    EmitEvent("/RESULT", msgId, fallback);
                    return;
                }
                TraceLog(AtGroup, "async_err", msgId,
                    $"cmd={command} error=ENOHANDLER");
                EmitEvent("/ERROR", msgId, "ENOHANDLER", command);
                return;
            }

            try
            {
                string result = handler(command, msgId, args);
                LastActivity = Stopwatch.GetTimestamp();
                TraceLog(AtGroup, "async_result", msgId,
                    $"cmd={command} result={result}");
                EmitEvent("/RESULT", msgId, result ?? "");
            }
            catch (Exception ex)
            {
                TraceLog(AtGroup, "async_err", msgId,
                    $"cmd={command} error={ex.Message}");
                EmitEvent("/ERROR", msgId, "EHANDLER", ex.Message);
            }
        }

        // ── EmitEvent ──

        /// <summary>
        /// Emit an event. Takes a local topic (starting with /) and walks
        /// the chain from ChainHead, delivering to every module that has a
        /// matching subscription entry on this module.
        ///
        /// Topic format:
        ///   localTopic = "/ECHO/DONE"   (what module emits, what callers subscribe to)
        ///   topicPath  = "EMPTY0/ECHO/DONE"  (AtGroup + localTopic, delivered to handlers)
        /// </summary>
        public void EmitEvent(string localTopic, int msgId, params string[] args)
        {
            if (string.IsNullOrEmpty(localTopic) || !localTopic.StartsWith("/"))
            {
                TraceLog(Name, "emit_event_error", msgId,
                    $"localTopic must start with '/', got '{localTopic}'");
                return;
            }
            string topicPath = AtGroup + localTopic;
            if (IsTopicBlocked(localTopic)) return;
            if (ChainHead == null) return;
            ArrayList subscriptions = new ArrayList();
            lock (_crossSubs)
            {
                foreach (string key in _crossSubs.Keys)
                {
                    SubscriptionEntry entry = (SubscriptionEntry)_crossSubs[key];
                    SubscriptionEntry snapshot = new SubscriptionEntry(
                        entry.TopicPattern, entry.ConsumerAtGroup, entry.DeliveryMode);
                    snapshot.FilterExpression = entry.FilterExpression;
                    snapshot.Enabled = entry.Enabled;
                    snapshot.SubscribedAt = entry.SubscribedAt;
                    subscriptions.Add(snapshot);
                }
            }
            bool anyInterested = false;
            VirtualModule scan = ChainHead;
            while (scan != null)
            {
                for (int i = 0; i < subscriptions.Count; i++)
                {
                    SubscriptionEntry entry = (SubscriptionEntry)subscriptions[i];
                    if (MatchTopic(localTopic, entry.TopicPattern) &&
                        MatchFilter(args, entry.FilterExpression) && entry.ConsumerAtGroup == scan.AtGroup)
                    { anyInterested = true; break; }
                }
                if (anyInterested) break;
                scan = scan.ForwardTarget;
            }
            if (!anyInterested)
            {
                scan = ChainHead;
                while (scan != null)
                {
                    if (scan.IsSubscribedTo(localTopic)) { anyInterested = true; break; }
                    scan = scan.ForwardTarget;
                }
            }
            if (!anyInterested) return;
            TraceLog(AtGroup, "emit_event", msgId, $"topic={topicPath}");
            VirtualModule m = ChainHead;
            while (m != null)
            {
                for (int i = 0; i < subscriptions.Count; i++)
                {
                    SubscriptionEntry entry = (SubscriptionEntry)subscriptions[i];
                    if (entry.Enabled && MatchTopic(localTopic, entry.TopicPattern) &&
                        MatchFilter(args, entry.FilterExpression) && entry.ConsumerAtGroup == m.AtGroup)
                        DeliverToConsumer(m, entry, localTopic, topicPath, msgId, args);
                }
                if (m.IsSubscribedTo(localTopic))
                {
                    TraceLog(m.AtGroup, "deliver_event_local", msgId, $"topic={topicPath} filter='*'");
                    m.DispatchIfSubscribed(localTopic, topicPath, msgId, string.Empty, args);
                }
                m = m.ForwardTarget;
            }
        }

        /// <summary>Apply PUSH, PULL or PUSH_PULL semantics for one matched cross-subscription.</summary>
        private void DeliverToConsumer(VirtualModule consumer, SubscriptionEntry entry,
            string localTopic, string topicPath, int msgId, string[] args)
        {
            TraceLog(consumer.AtGroup, "deliver_event", msgId,
                $"topic={topicPath} filter={FilterForLog(entry.FilterExpression)} mode={entry.DeliveryMode}");

            switch (entry.DeliveryMode)
            {
                case "PUSH":
                    // Cross-sub PUSH delivers to the module's default handler,
                    // not to local Program Subscribe delegates.
                    if (consumer.DefaultHandler != null)
                    {
                        try
                        {
                            consumer.DefaultHandler(topicPath, msgId,
                            CanonicalFilter(entry.FilterExpression), args);
                        }
                        catch (Exception ex)
                        {
                            TraceLog(consumer.Name, "handler_default_error", msgId,
                                $"reason for {topicPath}: {ex.Message}");
                        }
                    }
                    else
                    {
                        consumer.DefaultEventHandler(topicPath, msgId,
                            CanonicalFilter(entry.FilterExpression), args);
                    }
                    break;

                case "PUSH_PULL":
                    lock (_pendingEvents)
                    {
                        _pendingEvents[msgId] = new EventItem(localTopic, topicPath, msgId,
                            entry.FilterExpression, args);
                    }
                    lock (consumer._inbox)
                    {
                        consumer._inbox.Add(new EventItem(localTopic, topicPath, msgId,
                            entry.FilterExpression,
                            new[] { "PENDING", MessageFormat.FormatMsgId(msgId) }));
                    }
                    TraceLog(consumer.AtGroup, "inbox_enqueue", msgId,
                        $"topic={topicPath} filter={FilterForLog(entry.FilterExpression)} mode=PUSH_PULL pending");
                    consumer._inboxSignal.Set();
                    break;

                case "PULL":
                    lock (_pendingEvents)
                    {
                        _pendingEvents[msgId] = new EventItem(localTopic, topicPath, msgId,
                            entry.FilterExpression, args);
                    }
                    TraceLog(consumer.AtGroup, "event_pending_pull", msgId,
                        $"topic={topicPath} filter={FilterForLog(entry.FilterExpression)} mode=PULL pending");
                    break;

                default:
                    TraceLog(consumer.AtGroup, "deliver_unknown_mode", msgId,
                        $"topic={topicPath} filter={FilterForLog(entry.FilterExpression)} mode={entry.DeliveryMode}");
                    break;
            }
        }

        // ── DispatchIfSubscribed (called by own worker thread) ──

        /// <summary>
        /// Dispatch a delivered event to matching local delegates, Program's default
        /// delegate or the derived module's DefaultEventHandler fallback.
        /// </summary>
        private void DispatchIfSubscribed(string localTopic, string topicPath, int msgId,
            string filterExpression, string[] args)
        {
            if (IsTopicSwitchedOff(localTopic))
            {
                TraceLog(AtGroup, "dispatch_blocked", msgId,
                    $"topic={topicPath} (switched off)");
                return;
            }

            ArrayList handlersToInvoke = new();

            lock (_subscriptions)
            {
                foreach (string subPattern in _subscriptions.Keys)
                {
                    if (MatchTopic(localTopic, subPattern))
                    {
                        TraceLog(AtGroup, "dispatch_event", msgId,
                            $"topic={topicPath} filter={FilterForLog(filterExpression)} pattern={subPattern}");

                        var handlers = (ArrayList)_subscriptions[subPattern];
                        handlersToInvoke.AddRange(handlers);
                    }
                }
            }

            if (handlersToInvoke.Count == 0)
            {
                // Check DefaultHandler delegate first (set by Program)
                if (DefaultHandler != null)
                {
                    try
                    {
                        DefaultHandler(topicPath, msgId,
                            CanonicalFilter(filterExpression), args);
                    }
                    catch (Exception ex)
                    {
                        TraceLog(Name, "handler_default", msgId, $"reason for {topicPath}: {ex.Message}");
                    }
                    return;
                }
                // Fallback: virtual DefaultEventHandler
                DefaultEventHandler(topicPath, msgId, CanonicalFilter(filterExpression), args);
                return;
            }

            foreach (EventHandler h in handlersToInvoke)
            {
                try
                {
                    h(topicPath, msgId, CanonicalFilter(filterExpression), args);
                }
                catch (Exception ex)
                {
                    TraceLog(Name, "handler_event", msgId, $"reason for {topicPath}: {ex.Message}");
                }
            }
        }

        // ── Topic matching ──

        /// <summary>Internal subscription-condition entry point.</summary>
        private static bool MatchFilter(string[] args, string filterExpression)
        {
            return EvalFilter(args, filterExpression);
        }

        /// <summary>
        /// Evaluate a subscription condition over event arguments.
        ///   !      NOT
        ///   space  AND
        ///   |      OR
        ///   \      escape
        ///   $argN=pattern selects a positional argument for wildcard matching
        ///   $argN&gt;number, &gt;=, &lt;, &lt;= perform numeric comparison
        /// An unqualified term evaluates against the last event argument.
        /// </summary>
        public static bool EvalFilter(string[] args, string filterExpression)
        {
            if (string.IsNullOrEmpty(filterExpression) || filterExpression == "*")
                return true;

            int groupStart = 0;
            for (int i = 0; i <= filterExpression.Length; i++)
            {
                if (i == filterExpression.Length ||
                    (filterExpression[i] == '|' && !IsEscaped(filterExpression, i)))
                {
                    string group = filterExpression.Substring(groupStart, i - groupStart);
                    if (EvalAndGroup(args, group)) return true;
                    groupStart = i + 1;
                }
            }
            return false;
        }

        private static bool EvalAndGroup(string[] args, string group)
        {
            bool hasTerm = false;
            int termStart = 0;

            for (int i = 0; i <= group.Length; i++)
            {
                if (i == group.Length || (group[i] == ' ' && !IsEscaped(group, i)))
                {
                    if (i > termStart)
                    {
                        hasTerm = true;
                        if (!EvalFilterTerm(args, group.Substring(termStart, i - termStart)))
                            return false;
                    }
                    termStart = i + 1;
                }
            }
            return hasTerm;
        }

        private static bool EvalFilterTerm(string[] args, string term)
        {
            bool negate = false;
            int start = 0;
            while (start < term.Length && term[start] == '!')
            {
                negate = !negate;
                start++;
            }

            string operand = term.Substring(start);
            int argIndex = -1;
            string pattern = operand;
            string filterOperator = "=";

            if (operand.StartsWith("$arg"))
            {
                if (!TryParseAliasTerm(operand, out argIndex,
                    out filterOperator, out pattern))
                    return false;
            }
            else if (args != null && args.Length > 0)
            {
                argIndex = args.Length - 1;
            }

            bool matched = false;
            if (args != null && argIndex >= 0 && argIndex < args.Length)
            {
                string actual = args[argIndex] ?? "";
                // Only an alias operand's explicit @ selects address semantics.
                bool addressOperand = operand.StartsWith("$arg") &&
                    pattern.Length > 0 && pattern[0] == '@';
                if (addressOperand) pattern = pattern.Substring(1);
                if (pattern.Length == 0) return false;
                if (filterOperator == "=")
                {
                    matched = Extensions.WildcardMatchEscaped(actual, pattern);
                }
                else
                {
                    double actualNumber;
                    double expectedNumber;
                    if (TryParseFilterNumber(actual, addressOperand, out actualNumber) &&
                        TryParseFilterNumber(pattern, addressOperand, out expectedNumber))
                    {
                        if (filterOperator == ">") matched = actualNumber > expectedNumber;
                        else if (filterOperator == ">=") matched = actualNumber >= expectedNumber;
                        else if (filterOperator == "<") matched = actualNumber < expectedNumber;
                        else if (filterOperator == "<=") matched = actualNumber <= expectedNumber;
                    }
                }
            }

            return negate ? !matched : matched;
        }

        /// <summary>
        /// Explicit @ operands select unsigned hex on both sides of a comparison.
        /// Otherwise preserve the original double parser, including decimal fractions.
        /// Event values remain unchanged; this helper has no LoRa/topic dependency.
        /// </summary>
        private static bool TryParseFilterNumber(string value, bool addressOperand, out double number)
        {
            number = 0;
            if (string.IsNullOrEmpty(value)) return false;
            if (!addressOperand) return double.TryParse(value, out number);
            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                int digit;
                if (c >= '0' && c <= '9') digit = c - '0';
                else if (c >= 'a' && c <= 'f') digit = c - 'a' + 10;
                else if (c >= 'A' && c <= 'F') digit = c - 'A' + 10;
                else return false;
                // Reject values beyond the exact integer range of double before arithmetic.
                if (number > (9007199254740991d - digit) / 16d) return false;
                number = number * 16d + digit;
            }
            return true;
        }

        private static bool IsEscaped(string value, int index)
        {
            int backslashes = 0;
            for (int i = index - 1; i >= 0 && value[i] == '\\'; i--)
                backslashes++;
            return (backslashes & 1) != 0;
        }

        /// <summary>
        /// Match a local topic against exact, wildcard, one-segment /* or recursive /** patterns.
        /// </summary>
        public static bool MatchTopic(string topic, string pattern)
        {
            if (string.IsNullOrEmpty(topic) || string.IsNullOrEmpty(pattern))
                return false;

            // Fast exact match
            if (topic == pattern) return true;

            // ** matches everything
            if (pattern == "**") return true;

            // /** matches prefix + any nested segments
            if (pattern.EndsWith("/**"))
            {
                string prefix = pattern.Substring(0, pattern.Length - 3);
                // Fast-path only if prefix has no wildcards; otherwise delegate
                if (prefix.IndexOf('*') < 0 && prefix.IndexOf('?') < 0 && prefix.IndexOf('#') < 0)
                    return topic.StartsWith(prefix + "/") || topic == prefix;
                return Extensions.WildcardMatch(topic, pattern);
            }

            // /* matches exactly one segment (no / in remainder)
            if (pattern.EndsWith("/*"))
            {
                string prefix = pattern.Substring(0, pattern.Length - 2);
                // Fast-path only if prefix has no wildcards; otherwise delegate
                if (prefix.IndexOf('*') < 0 && prefix.IndexOf('?') < 0 && prefix.IndexOf('#') < 0)
                {
                    if (!topic.StartsWith(prefix + "/")) return false;
                    string remainder = topic.Substring(prefix.Length + 1);
                    return !remainder.Contains("/");
                }
                return Extensions.WildcardMatch(topic, pattern);
            }

            // Delegate to WildcardMatch for *, ?, #, and all other patterns
            return Extensions.WildcardMatch(topic, pattern);
        }

        // ── Watchdog ──

        /// <summary>Return true when module activity is newer than the watchdog threshold.</summary>
        public bool IsAlive()
        {
            return (Stopwatch.GetTimestamp() - LastActivity) < WatchdogTicks;
        }
    }


    #region Helpers
    /// <summary>
    /// Event item placed in a module inbox or pending-event store. It retains the
    /// local topic, delivered topic path, matched filter, context and original args.
    /// </summary>
    public class EventItem
    {
        public string LocalTopic { get; set; }
        public string TopicPath { get; set; }
        public string FilterExpression { get; set; }
        public int MsgId { get; set; }
        public string[] Args { get; set; }

        public EventItem(string localTopic, string topicPath, int msgId,
            string filterExpression, params string[] args)
        {
            LocalTopic = localTopic;
            TopicPath = topicPath;
            FilterExpression = filterExpression ?? "";
            MsgId = msgId;
            Args = args ?? new string[0];
        }
    }

    /// <summary>
    /// Runtime response/event envelope used across the module chain.
    /// AppLog producer format: AT+PUB=&lt;module&gt;,&lt;cmd&gt;,&lt;msgId&gt;[,args[]]
    /// Response format: +OK:&lt;contextId&gt;,... +DATA:&lt;contextId&gt;,... +ERR:&lt;contextId&gt;,...
    /// Event format:    +EVT:&lt;contextId&gt;,&lt;topic&gt;[,&lt;args&gt;]
    /// contextId is always rendered as an uppercase eight-character hexadecimal token.
    /// </summary>
    public class MessageFormat
    {
        public string Destination { get; set; }
        public string Command { get; set; }
        public int MsgId { get; set; }
        public string[] Args { get; set; }

        public MessageFormat()
        {
            Destination = string.Empty;
            Command = string.Empty;
            MsgId = 0;
            Args = new string[0];
        }

        // ── Response builders ──
        /// <summary>Render a context as the canonical uppercase eight-character hexadecimal token.</summary>
        public static string FormatMsgId(int msgId)
        {
            return msgId.ToString("X8");
        }

        /// <summary>
        /// Parse 0x-prefixed hex, canonical eight-character hex, or legacy decimal.
        /// Canonical eight-character tokens are always interpreted as hexadecimal.
        /// </summary>
        public static bool TryParseMsgId(string token, out int msgId)
        {
            msgId = 0;
            if (string.IsNullOrEmpty(token)) return false;

            if (token.StartsWith("0x") || token.StartsWith("0X"))
                return TryParseHexMsgId(token.Substring(2), out msgId);

            if (token.Length == 8)
                return TryParseHexMsgId(token, out msgId);

            return int.TryParse(token, out msgId) && msgId >= 0;
        }

        private static bool TryParseHexMsgId(string token, out int msgId)
        {
            msgId = 0;
            if (string.IsNullOrEmpty(token) || token.Length > 8) return false;

            int value = 0;
            for (int i = 0; i < token.Length; i++)
            {
                char c = token[i];
                int digit;
                if (c >= '0' && c <= '9') digit = c - '0';
                else if (c >= 'A' && c <= 'F') digit = c - 'A' + 10;
                else if (c >= 'a' && c <= 'f') digit = c - 'a' + 10;
                else return false;

                // The context group is limited to 0x00..0x7F; reject signed IDs.
                if (value > 0x07FFFFFF) return false;
                value = (value << 4) | digit;
            }

            msgId = value;
            return true;
        }

        /// <summary>Build a canonical payload-free +OK response.</summary>
        public static string BuildOk(string src, int msgId)
        {
            return "+OK:" + FormatMsgId(msgId) + "," + src;
        }

        /// <summary>Build a canonical +OK response with a source-qualified payload.</summary>
        public static string BuildOk(string src, int msgId, string payload)
        {
            return "+OK:" + FormatMsgId(msgId) + "," + src + ":" + payload;
        }

        /// <summary>Build a canonical +DATA response with a source-qualified payload.</summary>
        public static string BuildData(string src, int msgId, string payload)
        {
            return "+DATA:" + FormatMsgId(msgId) + "," + src + ":" + payload;
        }

        /// <summary>Build a canonical +ERR response with its source and error code.</summary>
        public static string BuildError(string src, int msgId, string code)
        {
            return "+ERR:" + FormatMsgId(msgId) + "," + src + "," + code;
        }

        /// <summary>Build a canonical +EVT envelope with context, topic path and ordered arguments.</summary>
        public static string BuildEvent(string topic, int msgId, params string[] args)
        {
            var sb = new StringBuilder();
            sb.Append("+EVT:");
            sb.Append(FormatMsgId(msgId));
            sb.Append(",");
            sb.Append(topic);
            if (args != null)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    sb.Append(",");
                    sb.Append(args[i]);
                }
            }
            return sb.ToString();
        }
    }

    /// <summary>
    /// Reusable nanoFramework compatibility helpers for Level 0 and business modules.
    /// They intentionally avoid desktop-only overloads and preserve the transport's
    /// comma-bearing text convention used by AT command arguments.
    /// </summary>
    public static class Extensions
    {
        /// <summary>Join Hashtable keys using a delimiter; return empty text for null or empty input.</summary>
        public static string JoinKeys(this Hashtable table, string delimiter = ",")
        {
            if (table == null || table.Count == 0)
                return string.Empty;

            var sb = new StringBuilder();

            foreach (DictionaryEntry entry in table)
            {
                if (sb.Length > 0)
                    sb.Append(delimiter);

                sb.Append(entry.Key.ToString());
            }

            return sb.ToString();
        }

        /// <summary>Join a string array without relying on unavailable nanoFramework overloads.</summary>
        public static string Join(this string[] p, string separator = ",")
        {
            if (p == null || p.Length == 0)
                return string.Empty;

            var sb = new StringBuilder();
            for (int i = 0; i < p.Length; i++)
            {
                if (i > 0)
                    sb.Append(separator);
                sb.Append(p[i]);
            }
            return sb.ToString();
        }

        /// <summary>Append every item from an ArrayList; null inputs are ignored.</summary>
        public static void AddRange(this ArrayList list, ArrayList items)
        {
            if (list == null || items == null)
                return;
            for (int i = 0; i < items.Count; i++)
                list.Add(items[i]);
        }

        /// <summary>Append every item from any ICollection; null inputs are ignored.</summary>
        public static void AddRange(this ArrayList list, ICollection items)
        {
            if (list == null || items == null)
                return;
            var enumerator = items.GetEnumerator();
            while (enumerator.MoveNext())
                list.Add(enumerator.Current);
        }

        /// <summary>
        /// Manual clone of ArrayList (nanoFramework lacks Clone()).
        /// </summary>
        public static ArrayList CloneList(this ArrayList list)
        {
            if (list == null) return null;
            var copy = new ArrayList();
            for (int i = 0; i < list.Count; i++)
                copy.Add(list[i]);
            return copy;
        }

        /// <summary>
        /// Split at no more than count delimiters and preserve the remaining text in
        /// the final element (nanoFramework lacks the required bounded Split overload).
        /// </summary>
        public static string[] SplitCount(this string s, char delimiter, int count)
        {
            if (string.IsNullOrEmpty(s)) return new string[0];
            var result = new ArrayList();
            int start = 0;
            int parts = 0;
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == delimiter)
                {
                    parts++;
                    if (parts >= count)
                    {
                        result.Add(s.Substring(start, i - start));
                        result.Add(s.Substring(i + 1));
                        return (string[])result.ToArray(typeof(string));
                    }
                    result.Add(s.Substring(start, i - start));
                    start = i + 1;
                }
            }
            result.Add(s.Substring(start));
            return (string[])result.ToArray(typeof(string));
        }

        /// <summary>
        /// Apply the consumer-side transport rule: remove one outer quote pair only
        /// when the value is properly wrapped and contains a comma. Naked values and
        /// JSON/object braces are preserved unchanged.
        /// </summary>
        public static string UnwrapTextArg(this string arg)
        {
            if (string.IsNullOrEmpty(arg)) return "";
            if (arg.Length >= 2 && arg[0] == '"' && arg[arg.Length - 1] == '"' && arg.IndexOf(',') >= 0)
                return arg.Substring(1, arg.Length - 2);
            return arg;
        }

        /// <summary>
        /// Extract a text payload beginning at index, rejoin comma-split transport
        /// pieces, then apply UnwrapTextArg. Use only when the remaining arguments
        /// semantically form one text field.
        /// </summary>
        public static string TextArg(this string[] args, int index = 1)
        {
            if (args == null || args.Length <= index) return "";
            if (args.Length == index + 1)
                return args[index].UnwrapTextArg();

            var sb = new StringBuilder();
            for (int i = index; i < args.Length; i++)
            {
                if (i > index) sb.Append(',');
                sb.Append(args[i]);
            }
            return sb.ToString().UnwrapTextArg();
        }

        /// <summary>
        /// Case-insensitive wildcard match with backslash escaping.
        /// Unescaped: * = zero or more chars, ? = one char, # = zero or more digits.
        /// Escaped wildcard and expression characters are matched literally. Prefer
        /// this helper for filter expressions and user-authored escaped patterns.
        /// </summary>
        public static bool WildcardMatchEscaped(string text, string pattern)
        {
            if (text == null || pattern == null) return false;

            char[] token = new char[pattern.Length];
            byte[] kind = new byte[pattern.Length]; // 0 literal, 1 ?, 2 #, 3 *
            int tokenCount = 0;

            for (int i = 0; i < pattern.Length; i++)
            {
                char value = pattern[i];
                if (value == '\\' && i + 1 < pattern.Length)
                {
                    token[tokenCount] = pattern[++i];
                    kind[tokenCount++] = 0;
                }
                else if (value == '*')
                {
                    token[tokenCount] = value;
                    kind[tokenCount++] = 3;
                }
                else if (value == '?')
                {
                    token[tokenCount] = value;
                    kind[tokenCount++] = 1;
                }
                else if (value == '#')
                {
                    token[tokenCount] = value;
                    kind[tokenCount++] = 2;
                }
                else
                {
                    token[tokenCount] = value;
                    kind[tokenCount++] = 0;
                }
            }

            int ti = 0;
            int pi = 0;
            int starIndex = -1;
            int starMatch = 0;

            while (ti < text.Length)
            {
                if (pi < tokenCount)
                {
                    if (kind[pi] == 1)
                    {
                        ti++;
                        pi++;
                        continue;
                    }

                    if (kind[pi] == 2)
                    {
                        char digit = text[ti];
                        if (digit >= '0' && digit <= '9')
                            ti++;
                        else
                            pi++;
                        continue;
                    }

                    if (kind[pi] == 3)
                    {
                        starIndex = pi++;
                        starMatch = ti;
                        if (pi >= tokenCount) return true;
                        continue;
                    }

                    char textChar = (char)(text[ti] | 0x20);
                    char patternChar = (char)(token[pi] | 0x20);
                    if (textChar == patternChar)
                    {
                        ti++;
                        pi++;
                        continue;
                    }
                }

                if (starIndex >= 0)
                {
                    pi = starIndex + 1;
                    ti = ++starMatch;
                    continue;
                }

                return false;
            }

            while (pi < tokenCount && (kind[pi] == 2 || kind[pi] == 3))
                pi++;

            return pi == tokenCount;
        }

        /// <summary>
        /// Case-insensitive raw wildcard match: * = zero or more chars, ? = exactly
        /// one char, # = zero or more digits. Prefer this helper for topic and
        /// destination patterns whose backslashes have no escape semantics.
        /// </summary>
        public static bool WildcardMatch(string text, string pattern)
        {
            if (text == null || pattern == null) return false;
            int tLen = text.Length;
            int pLen = pattern.Length;
            if (pLen == 0) return tLen == 0;

            int ti = 0, pi = 0;
            int starIdx = -1, matchIdx = 0;

            while (ti < tLen)
            {
                if (pi < pLen)
                {
                    char pc = pattern[pi];

                    // ? matches exactly one char
                    if (pc == '?')
                    {
                        ti++; pi++;
                        continue;
                    }

                    // # matches zero or more digits
                    if (pc == '#')
                    {
                        char tc = text[ti];
                        if (tc >= '0' && tc <= '9')
                        {
                            ti++;
                        }
                        else
                        {
                            pi++; // zero digits
                        }
                        continue;
                    }

                    // * matches zero or more chars
                    if (pc == '*')
                    {
                        starIdx = pi;
                        matchIdx = ti;
                        pi++;
                        // Trailing *: consume rest of text
                        if (pi >= pLen) return true;
                        continue;
                    }

                    // Fast ASCII case-insensitive compare
                    char tcLower = (char)(text[ti] | 0x20);
                    char pcLower = (char)(pc | 0x20);

                    if (tcLower == pcLower)
                    {
                        ti++; pi++;
                        continue;
                    }
                }

                // Backtrack if we had a *
                if (starIdx != -1)
                {
                    pi = starIdx + 1;
                    matchIdx++;
                    ti = matchIdx;
                    continue;
                }

                return false;
            }

            // Consume trailing * or #
            while (pi < pLen && (pattern[pi] == '*' || pattern[pi] == '#'))
                pi++;

            return pi == pLen;
        }
    }

    #endregion
}
