# CodeCraft SysModule

Document: CodeCraft Model-First README  
Document type: `business_module`  
Component: `sys`  
Component version: `3.1.5.12`  
Scope: `device/modules/sys/`  
Applies to: `CodeCraft_sys_module.json`, `CodeCraft_sys_at.json`  
Authority: Informational; module DDR artifacts are authoritative  
Portability: Reusable agile component  
Projection: Consume and exclude  
Platform dependency: Neutral contracts; device Deployment supplies optional resource bindings  
Status: Repository-ready metadata-connector pilot  
Copyright: © 2026 Roman Kiss, rkiss@pathcom.com  
Development: With assistance from ChatGPT and CodeCraft

> This README is a navigation document. It must not supply missing generation semantics. The JSON DDR artifacts are authoritative.

## Purpose

Version 3.1.5.5 adds idempotent deployment provisioning of missing FLASH `RD` roots while retaining explicit logical per-scope quota semantics. SYS retains one private GPIO controller for its GPIO resources; FLASH is an independent logical storage resource and uses no GPIO.

SYS remains an ordinary agile `VirtualModule`. Runtime may select it as preferred head, but the generated class assumes no fixed chain position.

## File Structure

```text
device/modules/sys/
├── CodeCraft_sys_module.json
├── CodeCraft_sys_at.json
└── README.md
```

## DDR Map

```text
SYS module context
|
+-- CodeCraft_sys_module.json
|   +-- Design
|   |   +-- logical artifact identity, locator and fingerprint contract
|   |   +-- identity, base, agility and factory
|   |   +-- platform-neutral physical connector resources
|   |   +-- normalized resource lifecycle
|   |   +-- separate NEO and LED push semantics
|   |   +-- BUTTON Press and Holding event semantics
|   |   +-- FLASH logical scopes, policy enforcement and eventing
|   |   +-- handlers and source generation
|   |   `-- subsystem extension policy
|   +-- Deployment
|   |   +-- conditional HAL requirements
|   |   `-- platform capability resolution
|   `-- Runtime
|       +-- resource state and message flow
|       `-- generated source artifact
|
`-- CodeCraft_sys_at.json
    +-- Design
    |   +-- message and resource contracts
    |   +-- independent NEO, LED and BUTTON operations
    |   +-- color normalization
    |   `-- inherited commands and events
    +-- Deployment
    |   `-- optional producer examples
    `-- Runtime
        `-- generation validation
```

## Normalized SYS Resource Contract

```text
AT+PUB=SYS,<RESOURCE>,<ctx>,<OPERATION>[,<args>]

RESOURCE
├── OPEN
├── CLOSE
├── INFO
└── resource-specific operations
```

- Every resource implements `OPEN`, `CLOSE` and `INFO`.
- Connector capabilities and configuration schemas are declared here; physical values are supplied by `entry.json#/Deployment/HAL/module_bindings/sys/resources`.
- `OPEN` receives the resolved device HAL configuration at runtime through the metadata connector.
- The agile module contains no compiler-hardcoded target pins, counts, order, power control or other physical values.
- Repeated `OPEN` returns `already`.
- `CLOSE` is safe and idempotent.
- `CLOSE` removes the resource's physical callbacks, pins and drivers only; it retains the SYS controller and subscriptions.
- `INFO` works while open or closed.
- Other operations return `ENOTOPEN` while closed.

Later SYS resources such as timers and watchdogs reuse this lifecycle and result pattern.

## NEO Contract

```text
NEO
├── OPEN,<data>[,<count>[,<order>[,<power>[,<powerActive>]]]]
├── CLOSE
├── INFO
├── SET,<color>
├── OFF
└── BLINK,<color>,<count>[,<phaseMs>]
```

NEO is an onboard status indicator, not a general NeoPixel module. It has no target selector, wildcard, GET, shadow color array, runtime platform override, animation queue or sequence ownership. Every write is immediate and a concurrent or later status write may overwrite an active indication.

The neutral defaults are `count=1`, `order=RGB`, `power=-1` and `powerActive=HIGH`. Therefore a board whose only non-default value is data pin 21 uses `OPEN,21`. A non-negative power pin enables explicit supply control; `-1` means that no EN pin is used.

## LED Contract

```text
LED
├── OPEN,<pin>,<active>
├── CLOSE
├── INFO
├── ON
├── OFF
├── TOGGLE
└── BLINK,<count>[,<phaseMs>]
```

This package binds the single active-LOW onboard LED at GPIO 6. The target is implicit and no index is accepted. `TOGGLE` uses the physical pin operation and therefore needs no retained logical state.

## BUTTON Contract

```text
BUTTON
├── OPEN,<pin>[,<pinMode>[,<debounceMs>]]
├── CLOSE
└── INFO
```

The current BOOT button binding is GPIO 0 with neutral defaults `InputPullUp`, `333 ms` debounce, holding enabled and double-press disabled. The selected nanoFramework `GpioButton` driver provides callbacks; SYS does not generate a polling loop or duplicate debounce/holding timer.

```text
short press -> /BUTTON/PRESS
long press  -> /BUTTON/HOLDING/STARTED
            -> /BUTTON/PRESS
            -> /BUTTON/HOLDING/COMPLETED
```

All events in one long interaction preserve one non-zero context. The three states are topic identities and carry no payload. `GpioButton` receives the retained SYS controller with `shouldDispose=false`; BUTTON CLOSE detaches its handlers and disposes only the button driver/pin.

## GPIO Controller Lifetime

For nanoFramework, separate managed `GpioController` objects do not provide isolated hardware-disposal scopes. Code-First therefore generates one private controller when each GPIO-consuming module instance is created and retains it across every resource `OPEN` and `CLOSE`.

```text
SYS module lifetime
└── retained ioctrl
    ├── optional NEO power pin
    ├── LED pin
    └── BUTTON driver/pin
```

Resource cleanup never calls `GpioController.Dispose()`. LED and optional NEO power use provider pin-close operations. BUTTON driver disposal releases its pin and must not be followed by a second pin close. LORA continues to use its own retained module controller.

## FLASH Contract

The nanoFramework projection binds the internal drive `I:/`. Deployment owns the physical directory map; business consumers use stable logical scope names and relative paths only.

```text
FLASH
├── CREATE,<generated missing RD roots>
├── OPEN
├── CLOSE
├── INFO
├── LDIR,<scope>[,<relativePath>[,<offset>]]
├── LIST,<scope>[,<relativePath>[,<offset>]]
├── WRITE,<scope>,<relativePath>,<text>
├── READ,<scope>,<relativePath>
├── DELETE,<scope>,<relativePath>
└── EXISTS,<scope>,<relativePath>
```

This package binds:

```text
metadata -> I:/device/metadata/  RD, recursive
config   -> I:/abc/config/      RW, flat, max 65536 bytes
data     -> I:/abc/data/        RW, recursive, max 65536 bytes
```

`RD` permits `READ` and `EXISTS`. `RW` additionally permits `WRITE` and `DELETE`. Every RW quota is the aggregate encoded size of files inside that scope. Caller paths cannot contain a drive, an absolute root or `..`; `recursive=false` permits files directly in the scope root only. The current contract is text-only and the AT adapter supports a single-line payload, including a properly quoted payload containing commas.

The first generated activation `OPEN` validates and locks the complete Deployment policy. `CLOSE` changes only opened state and preserves the policy and inherited subscriptions. A later public `OPEN` reuses the locked policy and cannot replace physical paths, modes or quotas.

Before `OPEN`, `PlatformPostInit` checks the Deployment-declared `RD` scope roots. If one or more roots are missing, it sends one synchronous `FLASH,CREATE` command through the Level 0 container with only those missing logical-name/path pairs. `CREATE` makes the roots, leaves FLASH closed, creates no files or nested content, never provisions `RW` scopes and preserves every existing directory and file. It is omitted when all `RD` roots already exist. Provisioned roots and runtime-created content survive reboot and ordinary firmware flashing; a mass erase may remove them.

`LDIR` travels the logical directory hierarchy from the scope root or one existing selected level. It returns descendant directories only, never files, and hides physical paths. `LIST` returns files directly inside exactly one selected pathscope; it never returns directories or recurses. Both use ordinal ordering, a fixed page size of 16 and an optional zero-based offset. Their response includes `count`, `total` and `next`; `next=-1` identifies the last page.

### FLASH eventing

```text
/FLASH/CREATE  createdScope...
/FLASH/OPEN    <scope descriptors...>
/FLASH/CLOSE
/FLASH/WRITE   scope,scopeAvailableBytes,relativeDirectory,fileName,fileBytes
/FLASH/DELETE  scope,scopeAvailableBytes,relativeDirectory,fileName,releasedBytes
```

`/FLASH/CREATE` is emitted once only when at least one missing `RD` root was created and contains only the created logical scope names. The OPEN event uses Polish/KISS projection: `metadata` means default `RD`, while `config:RW` and `data:RW` mark only writable exceptions. Physical drive used/free space is deliberately outside the contract. `FLASH,INFO,<scope>` calculates logical RW `usedBytes` from stored UTF-8 payload lengths and returns `availableBytes=quotaBytes-usedBytes`; RD scopes have no quota accounting. Events preserve the command context and are emitted only after successful state change. `INFO`, `LDIR`, `LIST`, `READ` and `EXISTS` emit no FLASH event, and file content is never published in an event.

### Color Formats

All these represent blue:

```text
Blue
0x000000FF
0:0:255
```

Named colors are `Black`, `White`, `Red`, `Green`, `Blue`, `Yellow`, `Cyan`, `Magenta`, `Orange` and `Purple`. Names are case-insensitive. Hex input is `0x00RRGGBB`. Decimal input is three colon-separated bytes. The value is converted for the immediate platform write and is not retained for GET.

### Synchronous BLINK

```text
AT+PUB=SYS,NEO,0x1430,BLINK,Blue,5,25
```

`count` is `1..5`; `phaseMs` is `10..100` and defaults to `25`. Every repetition performs indication, sleep, OFF, sleep. BLINK remains OFF when complete and emits no event. It does not reserve the resource: another status write may appear between phases and the last physical write wins.

## Deployment Binding

This reusable module declares no board or fixed GPIO. The device package independently enables and binds NEO, LED and BUTTON:

```text
NEO: data, count=1, optional power, RGB order, platform driver
LED: one GPIO reference, active HIGH or LOW, platform GPIO driver
BUTTON: GPIO reference, pin mode, debounce and platform GpioButton driver
```

The NEO open configuration is constructed from HAL references in the platform entry and delivered through `NEO,OPEN`. Neutral defaults reduce the common configuration to `OPEN,21`; no target-specific value is compiled into `SysModule.cs`.

## Smoke-Test Script

Prerequisite: device `entry.json` supplies valid SYS NEO, LED and BUTTON Deployment bindings. There are no empty physical lines because an empty Batch Run line terminates execution. After the subscription commands, manually perform one short press and one long hold before requesting the log.

```text
/log?clear&size=500
/log?purge
.clear
---SYS_GPIO_LIFETIME_EVENT_SMOKE_V3_1_4_7
AT+PUB=SYS,NEO,0x1401,INFO
AT+PUB=SYS,NEO,0x1402,OPEN,21
AT+PUB=SYS,NEO,0x1403,OPEN,21
AT+PUB=SYS,NEO,0x1410,SET,Blue
AT+PUB=SYS,NEO,0x1412,SET,0x00FF0000
AT+PUB=SYS,NEO,0x1413,SET,0:255:0
AT+PUB=SYS,NEO,0x1430,BLINK,White,5,25
AT+PUB=SYS,NEO,0x1440,OFF
AT+PUB=SYS,NEO,0x1441,CLOSE
AT+PUB=SYS,NEO,0x1442,CLOSE
AT+PUB=SYS,NEO,0x1443,SET,Blue
AT+PUB=SYS,NEO,0x1444,INFO
AT+PUB=SYS,LED,0x1450,INFO
AT+PUB=SYS,LED,0x1451,ON
AT+PUB=SYS,LED,0x1452,OFF
AT+PUB=SYS,LED,0x1453,TOGGLE
AT+PUB=SYS,LED,0x1454,TOGGLE
AT+PUB=SYS,LED,0x1455,BLINK,3,25
AT+PUB=SYS,BUTTON,0x1460,INFO
AT+PUB=SYS,SUB,0x1461,EMPTY0,/BUTTON/PRESS,*,PUSH
AT+PUB=SYS,SUB,0x1462,EMPTY0,/BUTTON/HOLDING/*,*,PUSH
AT+PUB=SYS,SUB,0x1463,EMPTY1,/BUTTON/PRESS,*,PUSH
AT+PUB=SYS,SUB,0x1464,EMPTY1,/BUTTON/HOLDING/*,*,PUSH
AT+PUB=SYS,SUBLIST,0x1465,*,/BUTTON/**,*
AT+PUB=SYS,BUTTON,0x1470,CLOSE
AT+PUB=SYS,BUTTON,0x1471,INFO
AT+PUB=SYS,BUTTON,0x1472,CLOSE
AT+PUB=SYS,LED,0x1473,BLINK,3,25
AT+PUB=SYS,NEO,0x1474,BLINK,Blue,1,25
AT+PUB=LORA,INFO,0x1475
AT+PUB=SYS,BUTTON,0x1476,OPEN,0
AT+PUB=SYS,BUTTON,0x1477,INFO
/log?
```

Expected checkpoints:

- first `INFO` reports the current composition-owned open state;
- first `OPEN` succeeds when closed or returns `already` when Runtime activation opened NEO; the repeated `OPEN` returns `already`;
- named, hexadecimal and decimal colors are accepted;
- `BLINK` completes synchronously and leaves the physical indicator OFF unless another concurrent status write overwrites it;
- first `CLOSE` succeeds and second returns `already`;
- `SET` after `CLOSE` returns `ENOTOPEN`.
- active-LOW LED commands drive ON=LOW and OFF=HIGH; two `TOGGLE` calls restore the initial physical level;
- a short button press delivers one `/BUTTON/PRESS` event;
- a long hold delivers STARTED, PRESS and COMPLETED in order with one preserved context.
- recursive `SUBLIST` with `/BUTTON/**` reports all four subscriptions;
- BUTTON CLOSE leaves LED, NEO and LORA functional, repeated CLOSE returns `already`, and BUTTON OPEN restores event callbacks.

### FLASH incremental smoke-test

There are no empty physical lines because an empty Batch Run line terminates execution. FLASH is opened by Runtime activation before `Modules are READY`; this test closes and reopens it after subscriptions are installed so OPEN and CLOSE delivery is observable.

```text
/log?clear&size=500
/log?purge
.clear
---FLASH_INCREMENTAL_SMOKE_V3_1_5_3
AT+PUB=SYS,SUB,0x1500,EMPTY0,/FLASH/OPEN,*,PUSH
AT+PUB=SYS,SUB,0x1501,EMPTY0,/FLASH/CLOSE,*,PUSH
AT+PUB=SYS,SUB,0x1502,EMPTY0,/FLASH/WRITE,*,PUSH
AT+PUB=SYS,SUB,0x1503,EMPTY0,/FLASH/DELETE,*,PUSH
AT+PUB=SYS,SUBLIST,0x1504,EMPTY0,/FLASH/**,*
AT+PUB=SYS,FLASH,0x1510,INFO
AT+PUB=SYS,FLASH,0x151D,INFO,metadata
AT+PUB=SYS,FLASH,0x151E,INFO,config
AT+PUB=SYS,FLASH,0x151F,INFO,data
AT+PUB=SYS,FLASH,0x1520,LDIR,metadata
AT+PUB=SYS,FLASH,0x1521,LIST,metadata,level11/level12
AT+PUB=SYS,FLASH,0x1522,LDIR,data
AT+PUB=SYS,FLASH,0x1523,LIST,data
AT+PUB=SYS,FLASH,0x1511,WRITE,config,settings.txt,"Hello, ABBA"
AT+PUB=SYS,FLASH,0x1512,READ,config,settings.txt
AT+PUB=SYS,FLASH,0x1513,EXISTS,config,settings.txt
AT+PUB=SYS,FLASH,0x1514,WRITE,metadata,forbidden.txt,NO
AT+PUB=SYS,FLASH,0x1515,WRITE,config,sub/forbidden.txt,NO
AT+PUB=SYS,FLASH,0x1516,READ,config,../escape.txt
AT+PUB=SYS,FLASH,0x1517,DELETE,config,settings.txt
AT+PUB=SYS,FLASH,0x1518,EXISTS,config,settings.txt
AT+PUB=SYS,FLASH,0x1519,CLOSE
AT+PUB=SYS,FLASH,0x151A,READ,config,settings.txt
AT+PUB=SYS,FLASH,0x151B,OPEN
AT+PUB=SYS,FLASH,0x151C,INFO
/log?
```

Expected event delivery is exactly one successful WRITE, DELETE, CLOSE and reopen OPEN event. INFO, LDIR, LIST, READ and EXISTS emit none. LDIR returns only logical directories; LIST returns only files in one exact pathscope. Negative checks return `EREADONLY`, `EPATH` and `ENOTOPEN` without a success event. A generated supplemental quota test must use a bounded payload that crosses the declared 65536-byte aggregate limit and expect `EQUOTA`.

## Agility and Generation

Generate exactly one `device/modules/sys/SysModule.cs` per SYS module type, regardless of runtime instance count. Consume and exclude both DDR files and this README from Package 2.

The generator must not invent a board, GPIO, driver or fixed platform API. It resolves enabled HAL bindings into platform-owned OPEN arguments. `SysModule.cs` keeps only neutral defaults and mutable fields populated by successful OPEN; the nullable driver reference is the sole lifecycle flag. Parsing, validation and sleeps remain outside any hardware lock; only a non-thread-safe driver write such as `Image.SetPixel + Update` may use a minimal lock.

## Planned Increments

```text
3.1.4.7  retained module GPIO controller lifetime correction
3.1.4.4  mandatory loosely coupled runtime OPEN binding
3.1.4.3  repository-ready SYS metadata connector pilot
3.1.4.2  separate lightweight NEO and LED push resources
later    reboot and reset reason
later    multiple named timers and eventing
3.1.5.2  event-driven policy-bound internal I:/ text storage
```

## Changelog

### 3.1.5.5

- Closed the FLASH metadata-connector boundary: `SysModule.Create(...)` receives `FlashOpenConfig` and `FlashProvisionConfig` before `Initialize()` and `Start()`.
- Prohibited a public post-creation `BindFlashDeployment` API and any running partially bound SYS instance.
- Kept capsule identity and semantic fingerprint in generation/provenance metadata rather than runtime module arguments or comparisons.
- Preserved the complete verified FLASH command, policy, provisioning, discovery, quota and event behavior from v3.1.5.4.

### 3.1.5.3

- Removed unsupported physical FLASH used/free accounting after the v3.1.5.2 BLOCKED provider finding.
- Added general and scope-specific INFO with explicit logical RW quota accounting.
- Simplified OPEN/CLOSE/WRITE/DELETE event arguments to application-relevant scope values only.
- Declared UTF-8 payload-byte accounting through `FileInfo.Length`; filesystem allocation overhead is outside the business contract.

### 3.1.5.2

- Added policy-bound FLASH text storage over the nanoFramework internal `I:/` drive.
- Added `metadata:RD:R`, `config:RW:N:65536` and `data:RW:R:65536` Deployment scopes.
- Added OPEN, CLOSE, INFO, LDIR, LIST, WRITE, READ, DELETE and EXISTS contracts.
- Added fixed-page logical directory traveling through LDIR and exact-pathscope file discovery through LIST; neither operation emits an event.
- Added `/FLASH/OPEN`, `/FLASH/CLOSE`, `/FLASH/WRITE` and `/FLASH/DELETE` with inherited subscription delivery and preserved contexts.
- Kept FLASH physical paths and policies outside reusable generated `SysModule.cs`; the first activation OPEN locks the resolved metadata connector policy.

### 3.1.5.1

- Replaced ambiguous module-local `$DESIGN/...` aliases with explicit artifact-local `#/Design/...` references; runtime behavior is unchanged.

### 3.1.4.10

- Package relabel only; the SYS semantic contract and canonical fingerprint are unchanged.

### 3.1.4.9

- Package relabel only; the SYS semantic contract and canonical fingerprint are unchanged.

### 3.1.4.8

- Normalized the existing SYS physical connector under the common metadata-connector contract and added runtime-connectivity and incremental projection keys.
- Changed canonical fingerprint semantics so package relabeling alone does not invalidate an unchanged module contract; runtime behavior is unchanged.

### 3.1.4.7

- Corrected nanoFramework GPIO ownership from one disposable controller per resource to one retained private controller per GPIO-consuming module instance.
- Required NEO power, LED and BUTTON CLOSE/rollback paths to release only their pins, callbacks and drivers and never call `GpioController.Dispose()`.
- Kept `GpioButton` on the retained SYS controller with `shouldDispose=false`; its disposal releases the button pin without a duplicate close.
- Added runtime isolation checks for BUTTON CLOSE/reopen, LED/NEO continuity, subscriptions and recursive `/BUTTON/**` inspection.
- Preserved the runtime-validated BUTTON PRESS and HOLDING event semantics and shared interaction context.

### 3.1.4.4

- Made the physical metadata connector the mandatory runtime initialization boundary for every module and container resource.
- Moved resolved NEO target configuration from generated `SysModule.cs` constants to platform-owned `NEO,OPEN` arguments.
- Added compact NEO defaults: `count=1`, `order=RGB`, `power=-1` and `powerActive=HIGH`; the current board therefore uses minimal `NeoOpenConfig = "21"`.
- Defined optional power/EN control, including active polarity and rollback on failed OPEN.
- Defined the nullable driver reference as the sole OPEN/CLOSED state and prohibited a duplicate boolean flag.
- Preserved explicit `OFF` and all existing synchronous status behavior.

### 3.1.4.3

- Added logical capsule identity `CodeCraft.module.sys` with repository-ready locator and canonical contract fingerprint rules.
- Declared neutral `pixel_output` and `digital_output` connector capabilities, cardinality, configuration schemas and lifecycle.
- Assigned physical providers, drivers, pins and derived open configuration exclusively to device HAL bindings.
- Kept NEO compile-time projected and performance-first; the connector adds no hot-path parsing or dynamic driver dispatch.

### 3.1.4.2

- Split NEO and LED into independent HAL-selected resource contracts.
- Made both resources best-effort push and last-write-wins.
- Removed NEO GET, target selectors, wildcard loops, shadow state, runtime platform overrides and sequence ownership.
- Required HAL-derived constants or readonly fields and a minimal driver-write lock outside parsing and sleeps.

### 3.1.4.1

- Corrected SYS NEO HAL order to RGB after observed Red/Green swap. New physical color verification pending.

### 3.1.4.0

- Added reusable `OPEN`, `CLOSE`, `INFO` SYS resource lifecycle.
- Added one NEO contract for NeoPixel-compatible RGB output and classic LED output.
- Added named, hexadecimal and decimal RGB input with normalized output.
- Added synchronous `SET`, `GET`, `OFF` and bounded `BLINK` without NEO eventing.
- Added a Batch Run-safe smoke-test script that is also an executable contract example.

## v3.1.5.12 — application event/messaging Hello World

Mandatory ApplicationStartUp runs after PlatformPostInit and readiness trace, always with [Application] ApplicationStartUp, even when empty. Application modules remain optional. This instance registers SYS as consumer of SYS /BUTTON/PRESS with $arg0>0|$tag=blink_then_lora.

Tag convention: <filter expression>|$tag=<action_id>. One terminal unescaped marker; ID uses ASCII letters/digits/underscore. ParseEventTag is a generic main-module Helpers method. Invalid/missing tags yield empty string. Dispatch requires both delivered topic and tag. The golden filter still evaluates the suffix as an OR branch; payload must not match the tag pattern. This is a temporary application convention, not reserved VirtualModule syntax.

HelloWorldEventHandler.cs owns the action: validate counter, direct NeoHandler SET Blue, synchronous Publish LORA SEND to bound test peer with Hello World #<counter>, finally direct NeoHandler SET Black. Preserve incoming context. Continue only if Blue returns +OK. Black follows SEND return/exception, not physical TX completion. Unknown tags are trace-only.

BUTTON counter is protected by buttonSync, first PRESS=1, successful OPEN and CLOSE/reset zero it; OPEN-already does not reset. PRESS emits counter as args[0]. HOLDING events and context behavior remain unchanged.

loraAddressForTest is a string HAL constant inside the LoRa preprocessor group, sourced from Deployment.HAL.modules.lora.test_address=0x5557. SYS has explicit default 0x5557 and Create override applied before Initialize/Start. No address is carried inside the tag.

Verification is increment-only smoke plus direct impact closure, not full SYS/EMPTY/LORA regression. Larger or ambiguous future module changes may require module regression. PING/ACK and Explorer changes are deferred.

### Deferred golden VirtualModule updates

- Explicit PublishAsync / AT+PUBA to queue a precise destination without a wildcard workaround.
- Reserve $tag metadata and separate it from filter evaluation before matching; preserve consumer access to it. No golden source or immutable usage DDR is changed here.

## v3.1.5.12 — application subscription filter correction

Resolves finding APPLICATION_SUB_FILTER_HIERARCHY from v3.1.5.8. Golden filter validation rejects a universal OR branch combined with a tag branch. BUTTON PRESS has a positive decimal counter in args[0], so the correct startup expression is `$arg0>0|$tag=blink_then_lora`. Unknown-action tests, if used, must likewise use `$arg0>0|$tag=unknown_action`. The tag remains the temporary terminal metadata convention and is still evaluated as an OR filter pattern; its payload-collision limitation remains.

v3.1.5.8 is closed UNVERIFIED: owner reports build/flash and runtime smoke passed after a local filter correction. That does not certify its unchanged archive or this new candidate. The selected canonical VERIFIED pair remains v3.1.5.7. No v3.1.5.8 VERIFIED pair is invented.

Runtime delta from the intended v3.1.5.8 implementation is the startup filter correction plus identity/banner projection only. No new action, counter, resource, PING/ACK or golden changes. Recheck compiler/flash, fresh startup SUB, and one PRESS with peer5557 receipt. Do not repeat the full previous regression.

## v3.1.5.12 — cleaning and application PING/ACK

This section supersedes prior Hello World application descriptions for the current target; older changelog entries are historical.

- Replace HelloWorldEventHandler.cs with PingEventHandler.cs containing BlinkThenLora and BlinkOnAck exactly once.
- Keep generic ParseEventTag in SysModule Helpers and existing BUTTON counter/default/factory configuration.
- BUTTON event with `$arg0>0|$tag=blink_then_lora` invokes Blue -> unicast SEND `PING #<counter>` -> finally Black. Target remains loraAddressForTest from HAL (0x5557), applied through SYS Create.
- LORA `/QRX/PACK` subscription delivers to SYS using `ACK_*|$tag=blink_on_ack`. It is installed only after BUTTON subscription succeeds.
- ACK action accepts exactly five RX args (From,To,RSSI,SNR,payload), then directly calls NEO BLINK Green1x50ms and traces the full RX arguments. It does not parse From as a decimal counter or add pending-request correlation. Selection is subscription-owned.
- DefaultEventHandler retains trace and two explicit topic+tag dispatch rules. NEO calls are direct within SYS; inter-module SEND uses Publish.
- Startup always traces entry and, on normal completion, Done; Done alone does not prove subscription success. Failure and exception traces remain explicit.
- The temporary |$tag= suffix contract and collision limitation remain unchanged; no VirtualModule modification.

### Manual prototype reference (not generated candidate certification)

Owner tested on2026-10-15: BUTTON counter5 -> PING #5 -> peer5557 -> ACK_PING #5 -> Green. Context08000008 is preserved through both branches. QueueTX14:58:40.328 to QueueRX14:58:46.695 is approximately6.37s. ACK handler logs Green success at14:58:46.825. Peer random response delay is intentional. New generated output must pass its short smoke.

### Narrow validation

Compile/flash, successful two startup subscriptions, one physical PRESS round-trip with matching payload/context and Green response. No repeated full SYS/EMPTY/LORA regression. TMR one-shot/watchdog is the next independent patch; no Timer or retry implementation here. Explorer synchronization is deferred.

### Future radio Type-byte TODO

Define driver/radio confirmation flags in a separate contract while preserving Type=0x00 as today's complete text-packet behavior without ACK request. Distinguish ACK request, ACK response and optional negative response. Application ACK_* remains independent of that future protocol.

## v3.1.5.12 — generic SYS TMR one-shot

New logical resource TimerResource.cs, no physical HAL requirement. SYS handles all timer state internally; remote/local consumers control it by messages. Keys are caller IDs, case-sensitive; max10 includes idle/active/expired. OPEN creates disabled idle entry; START arms idle/expired; START active returns already; RESTART resets deadline with Change, reusing the entry/Timer. Both support optional persistent delay override. OPEN optional tag stays payload state. CLOSE ID or * removes and disposes; expiration retains reusable entry and object as expired.

Only /TMR/EXPIRED is emitted: args[id,tag], context last successful START/RESTART. Timer tag is independent of subscription $tag. LIST returns[id,state,delayMs|...], LIST,NAMES returns[id|...]; INFO gives full state/context/tag/remaining snapshot. Same lock protects transitions; expiration committed under lock is emitted outside it. CLOSE/RESTART cannot retract an already committed event. Stale early callbacks must check current monotonic deadline, not just mutable generation of a reused callback state.

No PING watchdog declaration/integration in this patch. PingEventHandler, startup subscriptions and radio behavior remain unchanged. Test only TMR through the provided combined AppLog script; no repeated full module regression. Thread-pool wording in platform API documentation is not a verified concurrency guarantee; timer callback design must not depend on parallel delivery.

### Explorer synchronization TODO after validation

- MAIN lifecycle navigation: ApplicationStartUp after PlatformPostInit/readiness.
- Business public/private methods include relevant resource/event partial artifacts with correct file/method navigation.
- Event dispatch mapping topic + $tag to action implementation.
- Main xxxModule.cs first, resource parts next, *EventHandler.cs last.
- TMR commands, states, expiration event and source according to final verified contract.
- Add step-through PING sequence like Architecture: BUTTON, direct Blue, SEND/queue, remote peer, ACK RX, subscription, direct Green. Explain enqueue versus TX versus application ACK and preserved context.
- Refresh README/examples/latest VERIFIED pair byte-identically; preserve graphic style and old package support.

## 3.1.5.12 — WDG delta over VERIFIED 3.1.5.11

Extend existing TMR contract, not a new metadata architecture. WDG is a TMR specialization; metadata declares behavior/state, not C# internals. New WDG/CANCEL, checkpoint RESTART optional delay/tag, standby pool reclamation, active-only LIST with ALL diagnostic view. Capacity default10 passed only via SYS Create.

PING application: BUTTON -> Blue -> WDG15000 with exact ACK expectation -> SEND -> finally Black. Accepted ctx+payload before deadline consumes pending operation atomically once -> Green. Wrong counter does not consume pending state; unknown/late/duplicate ACK trace-only. Expiry -> Red. Failure to arm WDG logs but does not abort business SEND; no WDG means no ACK action eligibility. No sender authentication, driver Type/ACK or retries added.

PlatformInit must derive MsgIdGroup from local address, before context allocation: (numericLocalAddress >>8)&0x7F. Baseline metadata already declares this derivation; do not copy baseline Program hardcoding. Source command constants can be strings: parse the same address binding or project a numeric companion, never shift a string. No temporary group0 in deliverable.

### Frozen golden / deployment_zero_patch

Model-First and generated Code-First device/lib remain byte-identical to baseline. deployment_zero_patch/entry.json identifies a separate whole-file VirtualModule.cs override with EmitEvent snapshot delivery. It is mirrored under outer package2/device/lib/runtime/virtual/VirtualModule.cs. After placing generated code, replace that VS2022 target. Do not compile the supplemental tree twice or promote it to golden. New package is UNVERIFIED pending targeted checks; historical override/v6 tests are not validation of new generation. PublishAsync is future work.

### Focused acceptance

Run device/tests/WatchdogDeltaSmoke.txt and PING pair cases from ApplicationEventSmoke.txt; reproduce both scripts and oracles in worker handoff. No repeated full SYS/EMPTY/LORA regression for this delta. Use LIST,ALL for pool diagnostics; old count=10 LIST expectations are superseded. Existing NEO intermittent physical indication remains an open observation independent of correlation.

### Explorer next patch

Placer: discover manifest target paths and hashes, place generated tree then apply deployment replacements, preserve origin/revision/state; never silently patch golden archive. Keep main module first, resource files next, event action last with correct method navigation. Add simplified stepped PING/WDG sequence and checkpoint watchdog example; refresh fresh pair only after validation. No Explorer implementation in this request.

## 3.1.5.13 — public WDG correlation and cross-module agility

Current authoritative delta: retain3.1.5.12 runtime-tested behavior, correct its incomplete application/resource boundary. WDG accepts optional expectedReply/action as command arguments. CONFIRM is the public atomic completion command returning explicit accepted/ignored/error. Same-module application calls only registered handlers used by Publish; all capabilities available cross-module. Private generic resource helpers remain implementation detail behind handlers.

WDG syntax: TMR,WDG,id,delayMs,consumer[,tag[,expectedReply,action]]. Operation-inclusive counts4/5/7;6 rejected. Both correlation strings empty means disabled; one nonempty alone invalid. Expected/received reply is one text argument without comma/control chars; action is a nonempty ASCII identifier when enabled. No implicit PING default in resource.

CONFIRM syntax: TMR,CONFIRM,id,supervisedCtx,consumer,action,receivedReply. The command envelope ctx identifies its response; supervisedCtx is explicit full supported context in args. Successful matching response: +DATA:<commandCtx>,SYS:tmr,confirm,<id>,accepted. Ignored response: same prefix with ignored,reason=<reason>. Application executes action only on accepted, never merely on command success. Wrong reply/ownership/context preserves pending operation. Completed/late/duplicate replies never cause application effects. No sender authentication.

Remove ArmPingWatchdog and PING literals from TimerResource. PING passes ID/payload/action/tag/15000 through TimerHandler command arguments; no collection/entry writes after WDG. Incoming candidate ACK uses the same public CONFIRM handler. Resource does not execute an action token; it matches opaque ownership data and returns status. Remove direct _crossSubs access, internal identity encoding and SubscriptionEntry reference ownership checks; use supported subscription APIs. Managed routes must not be externally edited during their resource-owned lifetime.

### Acceptance and test cost

Run WatchdogCommandSmoke.txt for deterministic negative/positive/duplicate/deadline/legacy cases without radio injection. A direct AppLog call to SYS is NOT cross-module proof. Worker must include a minimal validation-only EMPTY Publish probe and exact setup/restore steps, kept outside canonical production artifacts, to run the same command arguments from EMPTY. Local PING covers handler shortcut. Demonstrate one accepted action and zero actions for rejected replies. Preserve atomicity and targeted coverage; do not ask owner to repeat unrelated full regression.

Baseline 3.1.5.12 remains VERIFIED runtime checkpoint; no retroactive rewriting of its evidence. New candidate UNVERIFIED until command/cross-module acceptance. Same immutable emit_event_snapshot_v1 deployment override still required after Placer; golden baseline stays byte-identical. WiFiAgent24-bit envelope finding remains TODO for separate golden replacement. No Explorer, PublishAsync or driver modifications here.

## 3.1.5.14 — demand-created ACK route and WDG_CONFIRM

Baseline3.1.5.13 remains VERIFIED and immutable. Add LORA SEND_ACK(destination,payload,consumer,ackFilter). Before enqueue, ensure missing exact /QRX/PACK consumer route using supported APIs; default new mode PUSH. Any existing route across delivery modes is preserved without filter/state/mode changes regardless of creator. OFF remains OFF. Multiple consumers are independent. Supplied filter is application data. No _crossSubs or private subscription identity dependency. Serialize producer command mutations so concurrent ensure cannot overwrite a route created meanwhile. No packet is queued on failed lookup/creation; if enqueue later fails, retain the created route. Created ACK routes persist until explicit administration via existing commands; no WDG-style last-packet cleanup.

For this PING example ApplicationStartUp subscribes only SYS/BUTTON/PRESS. First SEND_ACK establishes LORA ACK candidate route with ACK_*|$tag=blink_on_ack. Other applications may still choose startup-managed subscriptions. Generic LORA has no hardcoded PING payload, consumer or action. WDG correlation continues to decide whether an admitted ACK belongs to a pending pair.

Rename TMR operation CONFIRM -> WDG_CONFIRM and response token tmr,confirm -> tmr,wdg_confirm everywhere in generation, application checks, scripts, probe support and docs. No old-name alias in this model; old CONFIRM returns EOPERATION. All args and atomic accepted/ignored/error semantics unchanged. No standalone patch for this rename was made; it is bundled here with SEND_ACK.

Future packet-Type ACK-request support may implement the SEND_ACK intent, but is not generated now: Type0x00 remains backward-compatible text; driver/header and golden unchanged. Same-module application shortcuts are only registered handlers; other modules use Publish/events.

Acceptance is focused SEND_ACK_DELTA_SMOKE, not a repeated full regression. Worker must provide updated baseline validation-only probes outside production, rename exact accepted checks, and demonstrate EMPTY Publish plus SYS local WDG_CONFIRM. Preserve checkpoint/pool/routing evidence; restore production configuration after probes. Runtime/dependency/compile success cannot be claimed before actual evidence.

Explorer Example tab, Placer override support, PublishAsync and WiFiAgent full envelope-context support remain separate work. Existing emit_event_snapshot_v1 full-file override is preserved and still applied only to VS2022 placed copy; generated golden device/lib stays byte-identical.
