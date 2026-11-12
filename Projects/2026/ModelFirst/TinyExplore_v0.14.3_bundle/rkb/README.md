# Runtime Knowledge Base

The RKB tab explains AI Worker ↔ AEP Semaphore ↔ AppLog ↔ device and the runtime evidence returned from that interaction. Included exchange records, command reference and Example3 screenshot are offline examples, not a live session.

AEP uses an exclusively leased blob coordination object; Azure Blob Storage is the demonstrated provider. Application Role defines task/test semantics. AppLog and WiFiAgent connect to the actual device. Online use requires separately configured tools, authorization and credentials; no cloud credentials are bundled.

Only actual evidence supports scoped acceptance. RKB is bounded/sampled, wall times do not prove monotonic deadlines, and a copied trace is not proof that a new candidate was deployed. Scripts and ### expressions require a compatible supplied Role.
