# Reference

Terminology is indexed in Legend.json and searchable with Query. package0 is the human/logical task, package1 the complete Model-First target, package2 the complete Code-First projection, and package12 the accepted pair within its explicit validation scope.

Human requirements are free-form text or Markdown; a sequence diagram is optional. Each task projection owns family/version and output names. The selected pair owns baseline contracts; the human task owns the requested delta. Generic workflow policies govern scope, review and packaging. Teaching snippets/examples supply no additional requirements.

Active pipeline: templates/CodeCraft_package0_workflow_v1.3.0_template.zip and templates/CodeCraft_increment_workflow_v1.7.0_template.zip. Support schemas, semantic review helper and fingerprint reference are under templates/pipeline_contracts/. Required references are copied into each exported task.

IN validation → work → OUT validation applies to each phase. Reviews bind exact input/output hashes and do not certify runtime behavior. Code-First independently reviews the Model-First interfaces; an unresolved authoritative contradiction blocks affected generation. Current inventory identity/hashes/status must describe current bytes; prior acceptance remains history, never silently inherited.

Active deployment_zero_patch is preserved by default, with contract, replacement, hashes, provenance and evidence. “Integrate patch: all.”, a revision or artifact path explicitly selects Model-First→Code-First integration. Ambiguity preserves affected valid patches with a finding; unsafe integrity/placement still blocks. Retire only reconciled integrated scope and keep history. Integration alone is not VERIFIED.

AEP = Artifact Exchange Protocol. ZIP IN/OUT is sufficient; AEP is optional transport. --- marks AppLog/RKB lines; ### is a worker expression only under an explicitly supplied compatible Role. Optional tests require real skip/branch semantics. Baseline tests/reports are historical evidence, not automatic current test instructions.

Explorer v0.12.0 is the closed community baseline. Next-minor previews replace one another; old development snapshots/ReleaseNotes are not included. Pair versions and their required lineage remain independent of Explorer version.
