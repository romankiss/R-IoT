# Review — derived Model-First templates

Scope: static review only; no Code-First generation, compiler, flash or target runtime test performed. All imported ZIP bytes are unchanged. These are complete package1 templates, not VERIFIED package12 pairs.

## Findings

No blocking issue found in the reviewed structural checks.

- sys: one SYS module, one sys runtime instance; HAL bindings and metadata connectors owned by SYS.
- empty: one EMPTY module, one empty0 runtime instance, no GPIO/HAL module binding required.
- sys_empty: two module types, runtime chain SYS → EMPTY0 → EMPTY1; one EMPTY capsule reused twice.
- No LORA dependency or Pinging/ACK application layer found in the active JSON model. ApplicationStartUp has no business operations.
- JSON parses and ZIP CRC checks pass. Reviewed nonempty *_ref JSON pointers/artifact references resolve.
- All canonical module/AT contract fingerprints match the stored values.
- Retained device/lib bytes match the original verified v3.1.5.14 pair. Separate emit_event_snapshot_v1 baseline/replacement hashes match; golden files are not overwritten.
- Derived runtime statuses correctly remain UNVERIFIED / NOT_PERFORMED.

## Use and limits

SYS is a useful board foundation retaining NEO, LED, BUTTON, FLASH and TMR/WDG. Physical values are in Deployment.HAL and are passed through runtime metadata connectors rather than embedded in the reusable module. Pin-only adaptation is suitable only when the target board's peripherals and platform contracts remain compatible.

All three derived models explicitly select container identity 8 independently of LORA. Assign a unique ID in 1..127 for participating containers. This is an explicit deployment choice, not a local-radio-address derivation.

The SYS semantic fingerprint differs between sys and sys_empty; do not assume capsule interchangeability by filename/version alone. Both stored fingerprints validate. New compositions still require generation and appropriate target acceptance.

Archive names in this catalog follow their intrinsic identity; the upload suffix (1) on sys_empty was only an upload filename. Archive bytes were preserved.

| Template | SHA-256 |
|---|---|
| `CodeCraft_package1_sys_v3.1.5.14.zip` | `25c58fdcf915db89b9ad6d6dd4b7da3b895458f4aa0efc1d6fea1f8f6a58441b` |
| `CodeCraft_package1_empty_v3.1.5.14.zip` | `ccb4da9e4ababcdfb8e7137b9f63de8407d8f31ba38f7b3d2c5a27d7bcea10b0` |
| `CodeCraft_package1_sys_empty_v3.1.5.14.zip` | `8ee03563fcb340d017bf4770ef52e0b5dd5bda847706207cef3d832f9f2f937e` |
