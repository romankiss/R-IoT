// Core candidate revision: wifiagent_puba_v1 - UNVERIFIED; includes wifiagent_2_4_8_8
﻿/* ---- WifiAgent for PubSub Log Analyzer ------------------------
    written by Roman Kiss, July 23, 2026
    version: 2.4.8.5  09/11/2026  package v2.4.8.5, patch: SplitAtArgs
    version: 2.4.8.6  09/11/2026  package v2.4.8.6, patch: TraceLog adding [AppLog], updating header,
    version: 2.4.8.7  09/20/2026  package v2.4.8.7, patch: msgId decimal or hex 3 bytes,
    version: 2.4.8.8  10/28/2026  package v2.4.8.8, patch: fixing filtering for multiple patterns support,

    REST Console – Log API Usage
    ============================

    Script
        .clear
        .pause <timeInSeconds>
        .goto <lineNumber>[#label] [comment]
        .gosub <lineNumber>[#label] [comment]
        .return

    example:
        #refresh
        AT+PUB=LORA,SEND,222,0x5000,"Hello ABBA"
        /log?-15
        .pause 5
        .goto #refresh

    POST (publish AT command into module logic)
        AT+PUB=<topic>,<cmd>,<msgId>,<payload>
        Example: AT+PUB=EMPTY*,ECHO,1234,READY

    GET (pull-up log messages)
        /log?                 -> full log buffer
        /log?<num>            -> first <num> messages
        /log?-<num>           -> last <num> messages
        /log?clear[&size=num] -> clear buffer, trigger, filter; optional new size
        /log?purge            -> clear only buffer
        /log?info             -> buffer info (size, count, trigger state)
        /log?filter=<pattern> -> filter messages by pattern (multiple patterns supported)

    Trigger mode:
        /log?trigger=start&pattern=<wildcard>[&filter=<pattern>][&num=<xx>]
            -> returns <xx> messages before trigger match + trigger line + post-trigger window


   usage for plug&play:
   --------------------
        // creating a wifi agent
        using (agent = WiFiAgent.Create(Ssid, Password))
            {
                TraceStartupIdentity();

                if (agent != null)
                    VirtualModule.ConsumerHook = agent.ConsumerWorker;

                PlatformInit();
                CreateChain();

                if (agent != null)
                {
                    agent.Forwarder = VirtualModule.ChainHead.Publish;
                    agent.AsyncForwarder = VirtualModule.ChainHead.PublishAsync;
                    agent.Start();
                    agent.TraceLog("[Platform] AppLog is ready.\r\n");
                }
                else
                {
                    Debug.WriteLine("WiFi agent is not running.");
                }

                PlatformPostInit();

                Trace("[Platform] Modules are READY");
                Thread.Sleep(Timeout.Infinite);
            }
*/
#region Namespaces
using nanoFramework.Networking;
using nanoFramework.WebServer;
using System;
using System.Collections;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
#endregion

namespace NFAppLib.device.lib.tool
{
    // delegates for plug and play contracts, biderectional connector
    public delegate void ConsumeForwarder(string line);
    public delegate string[] PublishForwarder(string destination, string command, int msgId, params string[] args);

    public class WiFiAgent : WebServer
    {
        public PublishForwarder Forwarder { get; set; }
        public PublishForwarder AsyncForwarder { get; set; }

        private readonly object _logLock = new();
        private int LogBufferSize = 80;
        private string[] _logBuffer = null;
        private int _logIndex = 0;
        private int _logHead = 0;
        //
        private string _filterPattern = null;
        private string _triggerPattern = null;
        private int _triggerPostCount = 0;

        // using for direct log
        public void TraceLog(string line) => ConsumerWorker(line);

        public string Timestamp => DateTime.UtcNow.ToString("HH:mm:ss.fff");

        public WiFiAgent(string ssid, string password) : base(80, HttpProtocol.Http)
        {
            _logBuffer = new string[LogBufferSize];

            CancellationTokenSource cs = new(90000);
            TraceLog($"[AppLog] Connecting to {ssid} network ...");
            if (WifiNetworkHelper.ScanAndConnectDhcp(ssid, password, requiresDateTime: true, token: cs.Token))
            {
                TraceLog($"[AppLog] Wifi ready - ip:{IpAddress}");
            }
            else
            {
                TraceLog($"Can't connect to the network {ssid}, error: {WifiNetworkHelper.Status}");
                throw new Exception($"[AppLog] Wifi is not connected.");
            }
        }

        /// <summary>
        /// Create WiFi Agent
        /// </summary>
        /// <param name="ssid"></param>
        /// <param name="password"></param>
        /// <param name="publish">delegate delivery contract</param>
        /// <returns></returns>
        public static WiFiAgent Create(string ssid, string password, PublishForwarder publish = null)
        {
            WiFiAgent agent = null;
            try
            {
                agent = new WiFiAgent(ssid, password);
                if (agent != null)
                {
                    agent.Forwarder = publish;
                    agent.CommandReceived += (s, e) =>
                    {
                        try { agent.Agent_CommandReceived(s, e); }
                        catch (Exception ex) { agent.TraceLog($"[AppLog] WiFi Agent at processing of received method failed, reason: {ex.Message}"); }
                    };
                }
            }
            catch (Exception ex) { Debug.WriteLine($"WiFi Agent creating failed, reason: {ex.Message}"); }
            return agent;
        }

        public new void Start()
        {
            Debug.WriteLine($"[{Timestamp}] Waiting for starting ...");
            Thread.Sleep(5000);
            base.Start();
        }

        /// <summary>
        /// FAN-IN Sink
        /// </summary>
        /// <param name="line">message body</param>
        public void ConsumerWorker(string line)
        {
            string logmsg = $"[{Timestamp}] {line}";
            Debug.WriteLine(logmsg);

            // ============================================================
            // TRIGGER STATE MACHINE
            // ============================================================

            if (_triggerPattern != null)
            {
                // 1) LOCK
                if (_triggerPostCount == 0)
                    return;

                // 2) WAIT (negative)
                if (_triggerPostCount < 0)
                {
                    if (!(line.IndexOf("[") == 0))
                    {
                        // WAIT režim → konzolové logy sa zapisujú normálne
                        WriteToRingBuffer(logmsg);
                        return;
                    }
                    if (MatchLogExpression(line, _triggerPattern))
                    {
                        _triggerPostCount *= -1;

                        string tmsg =
                            $"[{Timestamp}] TRIGGER: start='{_triggerPattern}' filter='{_filterPattern ?? "(null)"}' post={_triggerPostCount}";

                        Debug.WriteLine(tmsg);
                        WriteToRingBuffer(tmsg, logmsg);
                        return;
                    }

                    // WAIT → no match → ignore
                    return;
                }

                // 3) CAPTURE (positive)
                if (_triggerPostCount > 0)
                {
                    if (_filterPattern != null)
                    {
                        if (!MatchLogExpression(logmsg, _filterPattern))
                            return;

                        logmsg = "~" + logmsg;
                    }

                    WriteToRingBuffer(logmsg);
                    _triggerPostCount--;
                    return;
                }
            }

            // ============================================================
            // NORMAL FILTER CHECK (trigger OFF)
            // ============================================================

            if (_filterPattern != null)
            {
                if (!MatchLogExpression(logmsg, _filterPattern))
                    return;

                logmsg = "~" + logmsg;
            }

            WriteToRingBuffer(logmsg);
        }

        #region Helpers

        private void Agent_CommandReceived(object obj, WebServerEventArgs e)
        {
            ArgumentNullException.ThrowIfNull(obj);
            var url = e.Context.Request.RawUrl.ToLower();
            var method = e.Context.Request.HttpMethod;
            uint.TryParse(e?.Context?.Request?.Headers["AppLog-line-number"], out uint lineNumber);

            // CORS
            e.Context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
            e.Context.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
            e.Context.Response.Headers.Add("Access-Control-Allow-Headers", "*");

            // Pre-flight OPTIONS
            if (method == "OPTIONS")
            {
                e.Context.Response.StatusCode = 200;
                return;
            }

            // GET handlers
            if (method == "GET")
            {
                // don't included to the circulated buffer
                string rcvcmd = $"[{Timestamp}] [{lineNumber:D5}] Command received: {url}, Method: {method}";
                Debug.WriteLine(rcvcmd);

                if (url.IndexOf("/log") == 0)
                {
                    // -------------------------------
                    // PARAM PARSING
                    // -------------------------------
                    string param = string.Empty;
                    int q = url.IndexOf('?');
                    if (q >= 0)
                        param = q == url.Length - 1 ? string.Empty : url.Substring(q + 1);

                    // -------------------------------
                    // FILTER=<wildcard> or FILTER=
                    // -------------------------------
                    #region Filter
                    if (param.IndexOf("filter=") == 0)
                    {
                        string pattern = DecodeQueryValue(param.Substring(7));

                        if (pattern.Length == 0)
                        {
                            _filterPattern = null;
                            TraceLog($"[AppLog] FILTER: filter cleared");
                        }
                        else
                        {
                            _filterPattern = pattern;
                            TraceLog($"[AppLog] FILTER: filter set to '{pattern}'");
                        }

                        OutputAsStream(e.Context.Response,
                            rcvcmd + "\r\n" + $"+OK: filter={_filterPattern ?? "(null)"}");
                        return;
                    }
                    #endregion

                    // -------------------------------
                    // TRIGGER=start...
                    // -------------------------------
                    #region Trigger
                    if (param.IndexOf("trigger=") == 0)
                    {
                        // stop all activity in the FAN-IN sink
                        lock (_logLock)
                        {
                            _triggerPattern = null;
                            _triggerPostCount = 0;
                            _filterPattern = null;
                        }

                        string cmd = param.Substring(8);

                        if (cmd.Length == 0)
                        {
                            TraceLog("[AppLog] TRIGGER: reset");
                            OutputAsStream(e.Context.Response,
                                rcvcmd + "\r\n" + "+OK: trigger reset");
                            return;
                        }

                        if (!(cmd.IndexOf("start") == 0))
                        {
                            OutputAsStream(e.Context.Response,
                                rcvcmd + "\r\n" + "+ERR: invalid trigger command");
                            return;
                        }

                        string[] parts = param.Split('&');

                        string pattern = null;
                        string filter = null;
                        int postCount = 0;

                        foreach (var p in parts)
                        {
                            if (p.IndexOf("pattern=") == 0)
                                pattern = DecodeQueryValue(p.Substring(8));
                            else if (p.IndexOf("filter=") == 0)
                                filter = DecodeQueryValue(p.Substring(7));
                            else if (p.IndexOf("num=") == 0)
                            {
                                string numStr = p.Substring(4);
                                if (int.TryParse(numStr, out int num))
                                {
                                    if (num < LogBufferSize)
                                        postCount = num;
                                    else
                                        postCount = LogBufferSize / 2;
                                }
                            }
                        }

                        if (string.IsNullOrEmpty(pattern))
                        {
                            OutputAsStream(e.Context.Response,
                                rcvcmd + "\r\n" + "+ERR: missing trigger pattern");
                            return;
                        }

                        _filterPattern = !string.IsNullOrEmpty(filter) ? filter : null;

                        if (postCount == 0)
                            postCount = LogBufferSize / 2;

                        int tpc = (postCount - 2) * -1;

                        TraceLog($"[AppLog] TRIGGER: configured start='{pattern}' filter='{_filterPattern ?? "(null)"}' post={postCount}");

                        _triggerPattern = pattern;
                        _triggerPostCount = tpc;

                        OutputAsStream(e.Context.Response,
                            rcvcmd + "\r\n" +
                            $"+OK: trigger start pattern='{pattern}' filter='{_filterPattern ?? "(null)"}' post={postCount}");
                        return;
                    }
                    #endregion

                    // -------------------------------
                    // CLEAR
                    // -------------------------------
                    #region Clear
                    if (param != null && param.IndexOf("clear") == 0)
                    {
                        int newSize = LogBufferSize;

                        int pos = param.IndexOf("&size=");
                        if (pos >= 0)
                        {
                            string num = param.Substring(pos + 6);
                            if (int.TryParse(num, out int parsed))
                            {
                                if (parsed < 25) parsed = 25;
                                if (parsed > 2000) parsed = 2000;
                                newSize = parsed;
                            }
                        }

                        lock (_logLock)
                        {
                            _filterPattern = null;
                            _triggerPattern = null;
                            _triggerPostCount = 0;

                            LogBufferSize = newSize;
                            _logBuffer = new string[newSize];
                            _logIndex = 0;
                            _logHead = 0;
                        }

                        OutputAsStream(e.Context.Response,
                            rcvcmd + "\r\n" +
                            $"+OK: log cleared, loginfo[size={LogBufferSize}, head=0, index=0]");
                        return;
                    }
                    #endregion

                    // -------------------------------
                    // PURGE Log buffer
                    // -------------------------------
                    #region Purge
                    if (param != null && param.IndexOf("purge") == 0)
                    {
                        lock (_logLock)
                        {
                            _logIndex = 0;
                            _logHead = 0;
                        }

                        OutputAsStream(e.Context.Response,
                            rcvcmd + "\r\n" +
                            $"+OK: log purged, loginfo[size={LogBufferSize}, head=0, index=0]");
                        return;
                    }
                    #endregion

                    // -------------------------------
                    // SNAPSHOT (atomic)
                    // -------------------------------
                    int size, index, head;
                    string[] snap;

                    lock (_logLock)
                    {
                        size = LogBufferSize;
                        index = _logIndex;
                        head = _logHead;
                        snap = (string[])_logBuffer.Clone();
                    }

                    // -------------------------------
                    // COUNT
                    // -------------------------------
                    int count;
                    if (index >= head)
                        count = index - head;
                    else
                        count = size - head + index;

                    // EMPTY
                    if (count == 0)
                    {
                        OutputAsStream(e.Context.Response,
                            rcvcmd + "\r\n" + "Log is Empty\r\n");
                        return;
                    }

                    // TAIL
                    int tail = (index - 1 + size) % size;

                    // -------------------------------
                    // INFO
                    // -------------------------------
                    if (param == "info")
                    {
                        OutputAsStream(e.Context.Response,
                            rcvcmd + "\r\n" +
                            $"+OK: loginfo[size={size}, head={head}, tail={tail}, index={index}, count={count}, filter={_filterPattern ?? "(null)"}, trigger={_triggerPattern ?? "(null)"}, post={_triggerPostCount}]");
                        return;
                    }

                    StringBuilder sb = new(rcvcmd + "\r\n");

                    // -------------------------------
                    // HEAD (first n)
                    // -------------------------------
                    if (int.TryParse(param, out int n) && n > 0)
                    {
                        int limit = n > count ? count : n;

                        int idx = head;
                        for (int i = 0; i < limit; i++)
                        {
                            sb.AppendLine(snap[idx]);
                            idx = (idx + 1) % size;
                        }

                        OutputAsStream(e.Context.Response, sb.ToString());
                        return;
                    }

                    // -------------------------------
                    // TAIL (last n)
                    // -------------------------------
                    if (int.TryParse(param, out n) && n < 0)
                    {
                        int need = -n;
                        if (need > count) need = count;

                        string[] temp = new string[need];

                        int idx = tail;
                        for (int i = 0; i < need; i++)
                        {
                            temp[i] = snap[idx];
                            idx = (idx - 1 + size) % size;
                        }

                        for (int i = need - 1; i >= 0; i--)
                            sb.AppendLine(temp[i]);

                        OutputAsStream(e.Context.Response, sb.ToString());
                        return;
                    }

                    // -------------------------------
                    // DEFAULT: full log
                    // -------------------------------
                    {
                        int idx = head;

                        for (int i = 0; i < count; i++)
                        {
                            sb.AppendLine(snap[idx]);
                            idx = (idx + 1) % size;
                        }

                        OutputAsStream(e.Context.Response, sb.ToString());
                        return;
                    }
                }
            }

            // POST handlers
            if (method == "POST")
            {
                if (url == "/at")
                {
                    #region /at
                    // Read POST body correctly
                    byte[] buffer = new byte[e.Context.Request.ContentLength64];
                    e.Context.Request.InputStream.Read(buffer, 0, buffer.Length);
                    string command = new(Encoding.UTF8.GetChars(buffer));

                    //string atCmd = $"[{Timestamp}] [{lineNumber:D5}] AT Command: {command}";
                    //TraceLog(atCmd.Substring(atCmd.IndexOf("] ") + 2));

                    bool publishAsync = command.IndexOf("AT+PUBA=") == 0;
                    if (publishAsync || command.IndexOf("AT+PUB=") == 0)
                    {
                        string atCmd = $"[{Timestamp}] [{lineNumber:D5}] AT Command: {command}";
                        TraceLog(atCmd.Substring(atCmd.IndexOf("] ") + 2));

                        var raw = command.Substring(publishAsync ? 8 : 7); // remove AT+PUBA= or AT+PUB=
                        //var parts = raw.Split(',');
                        var parts = SplitAtArgs(raw);

                        if (parts.Length < 3)
                        {
                            OutputAsStream(e.Context.Response, "+ERR: Missing required parameters");
                            return;
                        }

                        string destination = parts[0];
                        string cmd = parts[1];

                        if (!TryParseInt(parts[2], out int msgId))  // decimal or hex with prefix 0x
                        {
                            OutputAsStream(e.Context.Response, "+ERR: msgId must be integer");
                            return;
                        }
                        //if (msgId < 0 || msgId > 0xFFFFFF)      // LIMIT: max 3 bytes (0xFFFFFF)
                        //{
                        //    OutputAsStream(e.Context.Response, "+ERR: msgId out of range (0..0xFFFFFF)");
                        //    return;
                        //}

                        // optional args
                        string[] args = new string[parts.Length - 3];
                        for (int i = 3; i < parts.Length; i++)
                            args[i - 3] = parts[i];

                        // invoke the Code-First model
                        PublishForwarder selectedForwarder = publishAsync ? AsyncForwarder : Forwarder;
                        var result = selectedForwarder?.Invoke(destination, cmd, msgId, args) ?? new string[1] { publishAsync ? "+ERR: Missing async publish forwarder" : "+ERR: Missing publish forwarder" };

                        // return response
                        OutputAsStream(e.Context.Response, atCmd + "\r\n" + Join(result, "\n"));
                        return;
                    }
                    else
                    {
                        if(command.StartsWith("---")) TraceLog($"");
                        if(command == "---") return;
                        TraceLog($"[{lineNumber:D5}] {command}");
                    }
                    #endregion
                }
                else
                {
                    OutputHttpCode(e.Context.Response, HttpStatusCode.NotFound);
                    return;
                }
            }
        }

        private void WriteToRingBuffer(params string[] batch)
        {
            lock (_logLock)
            {
                foreach (string msg in batch)
                {
                    _logBuffer[_logIndex] = msg;
                    _logIndex = (_logIndex + 1) % LogBufferSize;

                    if (_logIndex == _logHead)
                    {
                        _logHead = (_logHead + 1) % LogBufferSize;
                    }
                }
            }
        }

        // Decode query values after separating '&' parameters. A literal '+' is
        // preserved; encoded UTF-8 bytes, including %20, %7C and %5C, are decoded.
        private static string DecodeQueryValue(string value)
        {
            if (value == null || value.IndexOf('%') < 0) return value;
            StringBuilder result = new StringBuilder();
            int i = 0;
            while (i < value.Length)
            {
                if (value[i] == '%' && i + 2 < value.Length &&
                    HexDigit(value[i + 1]) >= 0 && HexDigit(value[i + 2]) >= 0)
                {
                    byte[] bytes = new byte[(value.Length - i) / 3];
                    int count = 0;
                    while (i + 2 < value.Length && value[i] == '%' &&
                        HexDigit(value[i + 1]) >= 0 && HexDigit(value[i + 2]) >= 0)
                    {
                        bytes[count++] = (byte)((HexDigit(value[i + 1]) << 4) | HexDigit(value[i + 2]));
                        i += 3;
                    }
                    byte[] decodedBytes = new byte[count];
                    for (int j = 0; j < count; j++) decodedBytes[j] = bytes[j];
                    result.Append(new string(Encoding.UTF8.GetChars(decodedBytes)));
                }
                else result.Append(value[i++]);
            }
            return result.ToString();
        }

        private static int HexDigit(char c)
        {
            if (c >= '0' && c <= '9') return c - '0';
            if (c >= 'a' && c <= 'f') return c - 'a' + 10;
            if (c >= 'A' && c <= 'F') return c - 'A' + 10;
            return -1;
        }

        // Deployment zero patch: expression terms operate on the same log line.
        // Unescaped | = OR, space = AND, leading ! = NOT; backslash escapes.
        // AND has higher precedence. This component remains self-contained.
        private bool MatchLogExpression(string text, string expression)
        {
            if (text == null || expression == null) return false;
            int groupStart = 0;
            for (int i = 0; i <= expression.Length; i++)
            {
                if (i == expression.Length || (expression[i] == '|' && !LogIsEscaped(expression, i)))
                {
                    if (MatchLogAndGroup(text, expression.Substring(groupStart, i - groupStart))) return true;
                    groupStart = i + 1;
                }
            }
            return false;
        }

        private bool MatchLogAndGroup(string text, string group)
        {
            bool hasTerm = false;
            int start = 0;
            for (int i = 0; i <= group.Length; i++)
            {
                if (i == group.Length || (group[i] == ' ' && !LogIsEscaped(group, i)))
                {
                    if (i > start)
                    {
                        hasTerm = true;
                        string term = group.Substring(start, i - start);
                        int pos = 0;
                        bool negate = false;
                        while (pos < term.Length && term[pos] == '!') { negate = !negate; pos++; }
                        if (pos == term.Length) return false;
                        string pattern = term.Substring(pos);
                        bool wildcard = false;
                        for (int j = 0; j < pattern.Length; j++)
                            if ((pattern[j] == '*' || pattern[j] == '?' || pattern[j] == '#') && !LogIsEscaped(pattern, j)) { wildcard = true; break; }
                        // Retain WiFiAgent's implicit substring matching for literal terms.
                        if (!wildcard) pattern = "*" + pattern + "*";
                        bool matched = WildcardMatchEscaped(text, pattern);
                        if (negate ? matched : !matched) return false;
                    }
                    start = i + 1;
                }
            }
            return hasTerm;
        }

        private static bool LogIsEscaped(string value, int index)
        {
            int count = 0;
            for (int i = index - 1; i >= 0 && value[i] == '\\'; i--) count++;
            return (count & 1) != 0;
        }

        private static bool WildcardMatchEscaped(string text, string pattern)
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
        /// Wildcard match: * = zero or more chars, ? = exactly one char, # = zero or more digits.
        /// </summary>
        private bool WildcardMatch(string text, string pattern)
        {
            if (text == null || pattern == null) return false;

            if (pattern.IndexOf('*') < 0 && pattern.IndexOf('?') < 0 && pattern.IndexOf('#') < 0)
            {
                pattern = "*" + pattern + "*";
            }

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

        private string Join(string[] p, string separator = ",")
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

        private string IpAddress
        {
            get
            {
                {
                    NetworkInterface[] nis = NetworkInterface.GetAllNetworkInterfaces();
                    if (nis.Length > 0)
                    {
                        // get the first interface
                        NetworkInterface ni = nis[0];
                        return ni.IPv4Address;
                    }
                    else
                        return null;
                }
            }
        }

        /// <summary>
        /// Quote-aware splitter for the AT+PUB parameter list (v2.4.8.5).
        /// Transport = SYNTAX ONLY for SPLITTING: splits on commas outside quotes.
        /// The producer's quote chars STAY in the arg - the wrapper travels the
        /// chain exactly as the producer gave it, and ONLY the subscriber unwraps
        /// it (LoraModule SEND v3.0.0.10 UnwrapTextArg). JSON-escaped quotes from
        /// HTTP clients (\") toggle inQuotes, skip the backslash, keep the quote.
        ///   - ',' outside quotes  -> arg separator
        ///   - ',' inside quotes   -> kept as data   ("Hello, ABBA" -> ONE arg)
        ///   - '"' or '\"'         -> toggle inQuotes, quote char KEPT in the arg
        /// LoraModule SEND v3.0.0.10 UnwrapTextArg() is the ONLY consumer of the
        /// wrapper: strips the outer pair iff the arg is wrapped BOTH ends AND
        /// contains a comma. Naked args pass unchanged.
        /// nanoFramework-safe: no LINQ/Regex, manual scan via StringBuilder.
        /// </summary>
        private static string[] SplitAtArgs(string raw)
        {
            var result = new ArrayList();
            var sb = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < raw.Length; i++)
            {
                char c = raw[i];

                // JSON-escaped quote from HTTP client (\") -> behaves like a plain
                // quote: toggle inQuotes, KEEP the quote char, skip the backslash.
                // Prevents the 5C leak seen in trace 22:20:04 (args were \Hello
                // ABBA\) without dropping the producer's wrapper.
                if (c == '\\' && i + 1 < raw.Length && raw[i + 1] == '"')
                {
                    inQuotes = !inQuotes;
                    sb.Append('"');          // wrapper putuje chainom -> subscriber unwraps
                    i++;
                }
                else if (c == '"')
                {
                    inQuotes = !inQuotes;    // toggle; quote KEPT (syntax pre split, nie dáta)
                    sb.Append(c);
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(sb.ToString());
                    sb.Clear();
                }
                else
                {
                    sb.Append(c);
                }
            }
            result.Add(sb.ToString());
            return (string[])result.ToArray(typeof(string));
        }

        private static bool TryParseInt(string s, out int value)
        {
            value = 0;
            if (s == null) return false;

            s = s.Trim();

            // HEX
            if (s.Length > 2 && s[0] == '0' && (s[1] == 'x' || s[1] == 'X'))
            {
                uint tmp = 0;

                for (int i = 2; i < s.Length; i++)
                {
                    int nib = s[i].HexNibble();
                    if (nib < 0) return false;

                    tmp = (tmp << 4) | (uint)nib;
                }

                value = (int)tmp;
                return true;
            }

            // DECIMAL
            int dec = 0;
            bool neg = false;
            int idx = 0;

            if (s.Length > 0 && s[0] == '-')
            {
                neg = true;
                idx = 1;
            }

            for (; idx < s.Length; idx++)
            {
                char c = s[idx];
                if (c < '0' || c > '9') return false;

                dec = dec * 10 + (c - '0');
            }

            value = neg ? -dec : dec;
            return true;
        }


        #endregion
    }
}
