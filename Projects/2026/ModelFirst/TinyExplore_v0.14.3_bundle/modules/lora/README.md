# CodeCraft LoraModule

Document: CodeCraft Model-First README  
Document type: `business_module`  
Component: `lora`  
Component version: `3.1.5.12`  
Scope: `device/modules/lora/`  
Applies to: `CodeCraft_lora_module.json`, `CodeCraft_lora_at.json`  
Authority: Informational; module DDR artifacts are authoritative  
Portability: Reusable agile component  
Projection: Consume and exclude  
Platform dependency: Business contract neutral; HAL and driver bindings supplied by Deployment  
Status: Baseline  
Copyright: © 2026 Roman Kiss, rkiss@pathcom.com  
Development: With assistance from ChatGPT and CodeCraft

> This README is a human- and agent-readable navigation document. It must not supply missing generation semantics. When this document conflicts with a module DDR artifact, the JSON DDR is authoritative.

## Purpose

LoraModule exposes an agile LoRa business endpoint over the Level 0 container. It owns driver lifecycle commands, half-duplex RX/TX queues, TextPacket serialization and `/QRX/PACK` events while assuming no fixed position in a composition.

## Component Boundaries

The module owns `OPEN`, `CLOSE`, `SETUP`, `RESET` and `SEND` together with its common business handlers. Physical pins, SPI bus, address, radio configuration and concrete driver selection belong to Deployment HAL. Runtime instance count and topology belong to package Runtime composition.

## DDR File Structure

```text
device/modules/lora/
├── CodeCraft_lora_module.json
├── CodeCraft_lora_at.json
└── README.md
```

## DDR Map

```text
LORA module context
|
+-- CodeCraft_lora_module.json
|   +-- Design
|   |   +-- identity, base and agility
|   |   +-- factory, handlers and topics
|   |   +-- emitted events
|   |   +-- queue and driver invariants
|   |   `-- deterministic source generation
|   +-- Deployment
|   |   +-- platform and HAL references
|   |   +-- required namespaces
|   |   `-- required types
|   `-- Runtime
|       +-- driver and queue state
|       +-- wire packet
|       +-- receive and transmit flows
|       `-- generated source artifact
|
`-- CodeCraft_lora_at.json
    +-- Design
    |   +-- message contract
    |   +-- business and inherited commands
    |   +-- events
    |   `-- producer examples
    +-- Deployment
    |   `-- optional producer adapter
    `-- Runtime
        `-- registry validation
```

## Public Contract

The factory creates and starts the endpoint and emits `/ATTACH`. Driver commands emit their matching completion events. Received packets emit `/QRX/PACK` with context, source, destination, RSSI, SNR and data. Common `ECHO`, `INFO` and `LOOPBACK` handlers follow the Level 0 context contract.

## Agility and Portability

LoraModule assumes no predecessor, successor, producer, consumer or fixed chain position. It may be inserted, removed, reordered or replaced through container composition. No concrete LoraModule instance may be retained outside the Level 0 topology.

## Generation or Projection

Generate exactly one `device/modules/lora/LoraModule.cs` per discovered LORA module type. Preserve the declared handler behavior, trace text, queue decisions, exception boundaries, packet context and cleanup order literally. Consume and exclude both DDR files and this README from Package 2.

## Examples

Programmatic and optional WiFiAgent examples are provided in `CodeCraft_lora_at.json`. They demonstrate protocol use only; HAL values remain authoritative in the containing package Deployment section.

## Validation

Validate exact command registration, OPEN/SETUP argument ordering, driver and queue lifecycle, TX serialization, RX parsing, original-packet diagnostic traces, RSSI/SNR event arguments and context preservation across the LoRa packet boundary.

## Known Limitations / TODO

The current tested source-generation profile resolves the SX1262 driver. Generalizing construction through the declared driver interface is a future independently tested change.

## Changelog

### 3.1.5.1

- Replaced ambiguous module-local `$DESIGN/...` aliases with explicit artifact-local `#/Design/...` references; IRQ lifetime and runtime behavior are unchanged.

### 3.1.4.10

- Package-level fingerprint summary correction only; the v3.1.4.9 IRQ-pin lifetime contract and canonical fingerprint are unchanged.

### 3.1.4.9

- Declared the intentional golden SX1262 IRQ-pin lifetime. Generated LoraModule code must not close or dispose pinIrq during CLOSE; a later OPEN performs the safe rollover.

### 3.1.4.8

- Normalized LORA with runtime connectivity, logical artifact identity, canonical semantic fingerprint and a metadata connector for its SPI/GPIO/radio OPEN and SETUP bindings.
- Added connector comparison inputs for incremental Code-First generation; radio behavior is unchanged.

### 3.1.4.7

- Synchronized the reusable Lora DDR label with package patch `3.1.4.7`; the verified v3.1.4.5 runtime metadata connector and behavior are unchanged.

### 3.1.4.4

- Synchronized the reusable Lora module DDR label with package patch `3.1.4.4`; behavior and runtime OPEN/SETUP contracts are unchanged.

### 3.1.4.3

- Synchronized the mutable DDR package label; Lora behavior, HAL contract and driver bindings are unchanged.

### 3.1.4.2

- Synchronized mutable DDR component state with the closed read-only package label; Lora behavior is unchanged.

### 3.1.4.1

- Corrected SYS NEO HAL order to RGB after observed Red/Green swap. New physical color verification pending.

### 3.1.4.0

- Replaced the component metadata table with a compact standard text header; Lora behavior is unchanged.

### 3.1.2.1

- Adopted the package artifact-scope boundary contract; Lora behavior and generated class contract are unchanged.

### 3.1.2.0

- Added portable component documentation without changing the tested v3.1.1.2 LORA business behavior.
- Retained the deterministic source-generation, queue, packet and cleanup contracts verified by the v3.1.1.2 round-trip.

## 3.1.5.14 — demand-created ACK route and WDG_CONFIRM

Baseline3.1.5.13 remains VERIFIED and immutable. Add LORA SEND_ACK(destination,payload,consumer,ackFilter). Before enqueue, ensure missing exact /QRX/PACK consumer route using supported APIs; default new mode PUSH. Any existing route across delivery modes is preserved without filter/state/mode changes regardless of creator. OFF remains OFF. Multiple consumers are independent. Supplied filter is application data. No _crossSubs or private subscription identity dependency. Serialize producer command mutations so concurrent ensure cannot overwrite a route created meanwhile. No packet is queued on failed lookup/creation; if enqueue later fails, retain the created route. Created ACK routes persist until explicit administration via existing commands; no WDG-style last-packet cleanup.

For this PING example ApplicationStartUp subscribes only SYS/BUTTON/PRESS. First SEND_ACK establishes LORA ACK candidate route with ACK_*|$tag=blink_on_ack. Other applications may still choose startup-managed subscriptions. Generic LORA has no hardcoded PING payload, consumer or action. WDG correlation continues to decide whether an admitted ACK belongs to a pending pair.

Rename TMR operation CONFIRM -> WDG_CONFIRM and response token tmr,confirm -> tmr,wdg_confirm everywhere in generation, application checks, scripts, probe support and docs. No old-name alias in this model; old CONFIRM returns EOPERATION. All args and atomic accepted/ignored/error semantics unchanged. No standalone patch for this rename was made; it is bundled here with SEND_ACK.

Future packet-Type ACK-request support may implement the SEND_ACK intent, but is not generated now: Type0x00 remains backward-compatible text; driver/header and golden unchanged. Same-module application shortcuts are only registered handlers; other modules use Publish/events.

Acceptance is focused SEND_ACK_DELTA_SMOKE, not a repeated full regression. Worker must provide updated baseline validation-only probes outside production, rename exact accepted checks, and demonstrate EMPTY Publish plus SYS local WDG_CONFIRM. Preserve checkpoint/pool/routing evidence; restore production configuration after probes. Runtime/dependency/compile success cannot be claimed before actual evidence.

Explorer Example tab, Placer override support, PublishAsync and WiFiAgent full envelope-context support remain separate work. Existing emit_event_snapshot_v1 full-file override is preserved and still applied only to VS2022 placed copy; generated golden device/lib stays byte-identical.
