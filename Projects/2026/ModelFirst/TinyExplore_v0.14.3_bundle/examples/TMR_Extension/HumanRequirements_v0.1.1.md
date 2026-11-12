# SYS Module Increment — Periodic TMR Support

Human requirements for package0

Revision: 0.1.1 — D01 approved  
Date: 2026-11-01  
Requirement prefix: SYS-PTMR

## Purpose and authority

This document specifies a reusable customization of the existing SYS business module. It is input to the package0 incremental workflow, together with the selected VERIFIED package12 baseline pair and the applicable workflow template.

The supplied pair contains Model-First and corresponding Code-First. The package entry must identify the exact selected baseline and this requirements revision. Do not infer a different baseline or a release version from conversation history.

These numbered human requirements define the requested change. The baseline supplies existing contracts and implementation context. This document does not replace the established package schema or workflow. Cite requirement IDs in questions, reviews, implementation traceability and acceptance evidence.

## Human requirements

### SYS-PTMR-01 — Module scope

Add periodic TMR capability to SYS. Implement it as resource behavior, without application handlers that restart a timer to provide periodicity. Preserve existing application behavior and unrelated modules. Do not modify immutable VirtualModule level0.

### SYS-PTMR-02 — Three behaviors over the existing timer driver

The library timer driver remains the common foundation for one-shot TMR, WDG and the new periodic TMR. Preserve existing one-shot and WDG contracts, lifecycle, event payloads and responses. Do not use this increment to redesign their delivery, restart, disposal or error behavior.

### SYS-PTMR-03 — Shared named timer infrastructure

Use the existing namedTimers Hashtable, allocation pool and configured capacity. Do not introduce a separate periodic-timer Hashtable or capacity. IDs are unique across all three behaviors. Allocation and pool reuse must initialize mode-specific configuration so periodic settings cannot affect a subsequent one-shot or WDG object.

### SYS-PTMR-04 — Select periodicity through OPEN

Preserve existing one-shot syntax:

```text
OPEN,<id>,<delayMs>[,<tag>]
```

Add periodic syntax:

```text
OPEN,<id>,<delayMs>,<tag>,<periodMs>
```

The additional argument is the only new input-command syntax required. Omitting it selects existing one-shot behavior. When present, periodMs must be a positive supported millisecond value. Retain existing validation for IDs, delay and tag; declare the new argument's exact supported range and errors consistently in the Model Contract. Preserve duplicate-OPEN behavior.

An empty tag retains its positional field:

```text
AT+PUB=SYS,TMR,0x09006001,OPEN,minute_tick,10000,,60000
AT+PUB=SYS,TMR,0x09006002,START,minute_tick
```

This example requests the first expiration after 10 seconds and subsequent expirations at 60-second intervals. Long periods, particularly minutes, are a primary use case; no automatic mode selection based on a time threshold is required.

### SYS-PTMR-05 — Existing control commands

Retain START, RESTART, CANCEL, CLOSE, INFO and LIST command shapes and existing selectors.

- OPEN configures the periodic timer without starting it.
- START begins the first-delay sequence. START on an active timer retains the existing already-active behavior.
- RESTART resets the sequence to the first delay.
- Existing optional delay overrides on START/RESTART affect delayMs, not periodMs; existing optional RESTART tag behavior remains available.
- CANCEL stops future expirations and retains configuration for a subsequent START.
- CLOSE stops and removes the object; existing CLOSE wildcard semantics remain applicable.
- A supplied period is retained until the object is removed or replaced according to baseline allocation rules. No new runtime command to edit periodMs is required.

### SYS-PTMR-06 — Event contract and interest

Use the existing /TMR/EXPIRED event with unchanged arguments id,tag and existing subscription/filter/delivery mechanisms. Preserve the context established by START or RESTART for the duration of that run.

Event interest is Fire & Forget: the producer does not require business acknowledgement or manage consumer continuation. Consumers decide independently whether further work or their own wakeup timer is needed. Periodic operation continues without subscribers until explicitly stopped; absent interest may suppress delivery and logging under existing event rules.

### SYS-PTMR-07 — Periodic lifecycle and concurrency

A running periodic timer remains active across expirations until cancelled, closed or restarted. INFO must report its periodic configuration and remaining time to the next scheduled expiration. LIST must identify periodic configuration while preserving existing selectors and existing one-shot/WDG row formats.

Periodic callback completion must not overwrite a newer CANCEL, CLOSE or RESTART or resurrect an obsolete run. Define periodic delivery concurrency in the Model Contract before Code-First implementation, including the outcome of control operations invoked during delivery. Do not alter one-shot/WDG behavior to implement these rules.

For the existing one-shot/WDG paths, preserve expiration-state commitment before EmitEvent and do not introduce an unconditional State assignment after delivery. Retain their established post-delivery bookkeeping.

### SYS-PTMR-08 — Required Model-First artifacts

Update the existing SYS Model Contract's three DDR artifacts consistently: README, _at and _module, using their exact baseline filenames and structure.

- README describes purpose, usage, examples and behavior.
- _at declares argument syntax, validation, responses and errors.
- _module declares configuration, mode distinctions, lifecycle, event behavior and runtime semantics in the existing Design/Deployment/Runtime structure.

Keep resource declarations, command declarations, descriptions, examples and acceptance expectations consistent. Record the implementation trace to these requirements. Do not add example-specific application behavior.

### SYS-PTMR-09 — Step 1 and Step 2 deliverables

Follow the supplied incremental workflow and versioning rules.

Step 1 produces the revised Model-First package1 and review of these requirements. Step 2 produces corresponding Code-First package2, the updated package12 pair and acceptance material for build, flash and runtime testing. Preserve baseline provenance and identify changed artifacts. The resulting pair is a candidate reusable SYS business foundation, not a change to VirtualModule level0.

Do not claim VERIFIED until the required compiler, device and acceptance checks have actually passed. Record unavailable checks and unresolved findings truthfully.

### SYS-PTMR-10 — Acceptance evidence

Prepare and execute, where the environment permits, checks covering:

1. One-shot regression: OPEN/START, expiration, matching consumer delivery and retained expired object.
2. WDG regression: automatic route management, timeout delivery, valid confirmation, invalid/duplicate/late confirmation and cancellation.
3. Periodic first delay followed by at least three subsequent expirations; measure device RKB timestamps against the declared timing policy and tolerance.
4. Independent periodic timers with different delays/periods in the shared pool.
5. Correct context, timer ID, tag and filtered consumer delivery.
6. CANCEL, subsequent START, RESTART and CLOSE, including the agreed behavior during delivery.
7. No obsolete rearming or unexpected expiration after a successful stop.
8. Slow-consumer behavior according to the resolved timing decision below.
9. Pool reuse across periodic TMR, one-shot TMR and WDG, including shared capacity limits.
10. Invalid period input, omitted period, empty tag, duplicate OPEN and INFO/LIST behavior.

Use existing passive handlers, such as EMPTY consumers, when sufficient. Capture relevant RKB evidence before it can be overwritten. Distinguish command acceptance, event emission, delivery and application effects. Through AEP, preserve correlated results, perform guarded ACK and finish or explicitly report unresolved exchanges. Remove test-owned objects and routes without altering unrelated active resources.

## Approved requirement — SYS-PTMR-D01

The following policy is approved and binding:

- Keep cadence relative to scheduled expiration times rather than handler completion.
- Do not overlap deliveries for the same periodic timer.
- Skip missed periods rather than emit catch-up bursts when delivery takes longer than the period.

This policy is binding for Step 1 and Step 2. Define and disclose controlled acceptance timing tolerances; observed sample timings are not universal precision guarantees. Preserve existing one-shot and WDG behavior. Revision 0.1.1 records the user approval of D01; original 0.1.0 task bytes remain immutable history.

## Handoff boundary

This is the human-requirements attachment, not a complete package0, an entry.json schema, a generated DDR artifact or a VERIFIED release. The package0 assembler must include the selected baseline pair and applicable workflow and reference this document through the existing packaging contract. No prior chat history should be needed to interpret the requested increment.
