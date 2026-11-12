# AEP · Artifact Exchange Protocol

HTTP-capable participants exchange inline request lines or resource references through a distributed state machine represented by a shared semaphore blob. Azure Blob Storage is the demonstrated provider. Participants interpret versioned protocol metadata; no separate orchestration engine, traditional database, custom relay or inbound participant endpoint is required by this POC.

Normal exchange: IDLE → REQUEST → RUNNING → READY → IDLE. Commit referenced input before REQUEST and output before READY. Capture/verify the result before acknowledging receipt. READY does not imply business success; ACK does not imply a successful device command.

Writes use an exclusive finite blob lease, fresh state/correlation/identity checks and conditional ETag commit, with release in finally. Reads remain possible while leased; a logical lockedBy field is not the storage lease. Follow the deployed protocol for lease lifecycle and uncertain-write recovery; never force IDLE or duplicate device execution merely because of timeout. A lease cannot guarantee exactly-once external side effects.

Application Role is a separate artifact defining authorized work, payload meaning, defaults and completion. Bootstrap identifies protocol, Role, participants and semaphore; authorized credentials are supplied separately. app_ properties belong to application Role semantics, never override protocol fields/authority, and follow normal guarded updates. ---/### are Role/script semantics, not device or generic wire extensions.

The abort branch is REQUEST/RUNNING/READY → ABORT/PENDING → ABORT/ABORTED → IDLE. The peer quiesces before ABORTED; completed side effects are not undone. The exact deployed Protocol artifact governs behavior; the included reference JSON is sanitized explanatory metadata, not a live session schema.

Current illustrated exchange is two participants, one half-duplex slot and bounded polling. Full duplex/pool and fan-out accounting are future extensions. Application Role corresponds to application semantics (OSI L7 as an analogy); AEP uses existing networking, not new physical/network layer implementations.

AI Worker↔Semaphore↔AppLog↔device sessions and RKB capture have owner-supplied evidence. Explorer provides offline explanation only. Online toolbar, cloud configuration and session execution are external and optional; ZIP IN/OUT remains a complete pipeline route.
