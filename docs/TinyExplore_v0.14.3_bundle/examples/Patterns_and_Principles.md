# Patterns & Principles

**Package = State = Context.** Explicit ZIP inputs/outputs carry a stage's artifacts, contracts, provenance and evidence. New agents do not depend on unseen chat history.

| Term | Meaning |
| --- | --- |
| DDR | JSON metadata organized into Design, Deployment and Runtime. Describes contracts, behavior, state and binding. |
| LAR | Logical Artifacts Repository: contracts, modules, golden components, packages and runtime evidence. |
| uLAR / Micro LAR | Portable repository/workspace; an explicit snapshot carries its artifacts and provenance. |
| HAL | Hardware Abstraction Layer: board/platform resources and physical binding. |
| MAL | Metadata Access Layer: metadata-facing module capabilities and access contracts. Dedicated DDR MAL section remains a planned evolution. |
| MAL Connector | Realizes metadata binding for agile plug-in modules. Existing metadata_connector declarations are the starting point; avoid duplicate contracts. |
| AEP | Artifacts Exchange Pattern: shared state coordinates inline/resource exchange. |
| AEX | Versioned exchange protocol realizing AEP, with correlation, conditional transitions, lease coordination and acknowledgements. |
| Application role | Separate artifact defining application-level task semantics over generic AEX. |

Model-First → Code-First is the generation perspective. MAL → HAL is the bird/drone perspective from metadata capabilities through runtime connectors/drivers to physical realization. A neutral module such as EMPTY may require no hardware binding.

Agile modules declare contracts independently of board values, chain position and runtime instance count. Cross-module interaction uses commands/events. Same-module shortcuts use registered message handlers with equivalent contracts. Container/platform metadata owns composition and late binding.

Logical identity and semantic contract fingerprint support reuse decisions. Complete relevant bindings/platform/dependencies must also match; an unchanged fingerprint alone is insufficient. Golden bytes and frozen pairs stay immutable; replacements and new labeled states preserve provenance.

A trained pair is an empirically tested Model-First/Code-First baseline within its recorded acceptance profile, not model weights or a guarantee for arbitrary new hardware. PacketEnqueued is queue acceptance, not air transmission or reception. AEX ACK means receipt, not application success. Runtime KB keeps timestamps, module, ctx/correlation, markers and actual outcomes.

Proposals for community collaboration: NuGet assemblies for VirtualModule/WiFiAgent, plus metadata-friendly component/driver repositories with MAL contracts. These are roadmap items, not included packages.
