# CodeCraft EmptyModule

Document: CodeCraft Model-First README  
Document type: `business_module`  
Component: `empty`  
Component version: `3.1.5.12`  
Scope: `device/modules/empty/`  
Applies to: `CodeCraft_empty_module.json`, `CodeCraft_empty_at.json`  
Authority: Informational; module DDR artifacts are authoritative  
Portability: Reusable agile component  
Projection: Consume and exclude  
Platform dependency: Neutral; no HAL resources required  
Status: Baseline  
Copyright: © 2026 Roman Kiss, rkiss@pathcom.com  
Development: With assistance from ChatGPT and CodeCraft

> This README is a human- and agent-readable navigation document. It must not supply missing generation semantics. When this document conflicts with a module DDR artifact, the JSON DDR is authoritative.

## Purpose

EmptyModule is the minimal CodeCraft business module. It validates module discovery, class generation, multiple instances, container routing, responses, eventing, subscriptions and context propagation without hardware dependencies.

## Component Boundaries

EMPTY owns only `ECHO`, `INFO` and `LOOPBACK` plus its emitted business events. Routing, inherited commands, subscriptions, filters, lifecycle and instrumentation belong to VirtualModule.

## DDR File Structure

```text
device/modules/empty/
├── CodeCraft_empty_module.json
├── CodeCraft_empty_at.json
└── README.md
```

## DDR Map

```text
Empty module context
|
+-- CodeCraft_empty_module.json
|   +-- Design
|   |   +-- identity, base and agility
|   |   +-- factory, handlers and topics
|   |   +-- emitted events and invariants
|   |   `-- deterministic source generation
|   +-- Deployment
|   |   +-- no HAL requirement
|   |   +-- required namespaces
|   |   `-- required types
|   `-- Runtime
|       +-- inherited state
|       +-- message flow
|       `-- generated source artifact
|
`-- CodeCraft_empty_at.json
    +-- Design
    |   +-- message contract
    |   +-- business and inherited commands
    |   `-- events
    +-- Deployment
    |   `-- optional producer adapter examples
    `-- Runtime
        `-- registry validation
```

## Public Contract

- `ECHO` returns the joined payload and emits `/ECHO/DONE`.
- `INFO` returns name, AtGroup, enabled state and inherited receive count and emits `/INFO`.
- `LOOPBACK` routes `ECHO` through the Level 0 container using the same context and returns `OK` or `FAIL`.
- The factory emits `/ATTACH` after initialization and start.

## Agility and Portability

The generated class has no fixed chain position and no dependency on another business module. Runtime composition may create zero, one or many instances from the same generated class by supplying different suffixes. Concrete instances are never retained in static application fields.

## Generation or Projection

Generate exactly one `device/modules/empty/EmptyModule.cs` per discovered Empty module type, regardless of runtime instance count. Consume and exclude both DDR files and this README from Package 2.

## Examples

```text
Publish("EMPTY0", "INFO", ctx)
Publish("EMPTY0", "ECHO", ctx, "hello")
Publish("EMPTY0", "LOOPBACK", ctx)
```

The DDR also contains optional WiFiAgent `AT+PUB` examples. WiFiAgent is not part of EMPTY business semantics.

## Validation

Generated `RegisterHandlers` must contain exactly `ECHO`, `INFO` and `LOOPBACK` in the declared order. Inherited Level 0 commands must not be regenerated. Responses and emitted events preserve the incoming context.

## Known Limitations / TODO

EmptyModule intentionally owns no HAL, driver or persistent business state.

## Changelog

### 3.1.5.1

- Replaced ambiguous module-local `$DESIGN/...` aliases with explicit artifact-local `#/Design/...` references; runtime behavior is unchanged.

### 3.1.4.10

- Package relabel only; the EMPTY semantic contract and canonical fingerprint are unchanged.

### 3.1.4.9

- Package relabel only; the EMPTY semantic contract and canonical fingerprint are unchanged.

### 3.1.4.8

- Normalized EMPTY as the canonical business-module template with runtime connectivity, logical artifact identity, canonical semantic fingerprint and a metadata connector whose physical resource map is explicitly empty.
- Added connector comparison inputs for incremental Code-First generation; runtime behavior is unchanged.

### 3.1.4.7

- Synchronized the reusable Empty module DDR label with package patch `3.1.4.7`; behavior is unchanged.

### 3.1.4.4

- Synchronized the reusable Empty module DDR label with package patch `3.1.4.4`; behavior is unchanged.

### 3.1.4.3

- Synchronized the mutable DDR package label; Empty behavior and projection contract are unchanged.

### 3.1.4.2

- Synchronized mutable DDR component state with the closed read-only package label; Empty behavior is unchanged.

### 3.1.4.1

- Corrected SYS NEO HAL order to RGB after observed Red/Green swap. New physical color verification pending.

### 3.1.4.0

- Replaced the component metadata table with a compact standard text header; Empty behavior is unchanged.

### 3.1.2.1

- Adopted the package artifact-scope boundary contract; Empty behavior and generated class contract are unchanged.

### 3.1.2.0

- Normalized the earlier v3.0.4 declarations to the current `Design`, `Deployment`, `Runtime` structure.
- Added explicit agility, container-access and deterministic source-generation contracts.
- Synchronized inherited commands, X8 context notation and message-registry validation with Level 0 v3.0.1.
