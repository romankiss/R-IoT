# v0.14.3

- Promote the reconciled XIAOSenseTH v1.1.2 pair to the current golden baseline. Include the async CAM result trace fix and the aligned startup banner. Keep Example 3 as the other built-in baseline.

# v0.14.2

- Promote the reconciled XIAOSenseTH v1.1.1 pair to the current golden baseline. Keep Example 3 as the other built-in baseline.

# v0.14.1

- Show only the two declared baseline pairs in Create Package 0 for the bundled Micro LAR.
- Keep tutorial and module-provenance archives available for inspection.

# v0.14.0

- Close the v0.13 preview cycle and add the verified XIAOSenseTH golden pair alongside the existing Example 3 pair.
- Add Skip to Yours Extension on both entry pages and support remembering the Yours Extension selection.

# v0.13.0-preview.42

- Link the From a Golden Pair architecture slide directly to Yours Extension.

# v0.13.0-preview.41

- Add the From a Golden Pair architecture slide: increment, decrement, board change and deployment zero patch.
- Correct the downstream workflow version in the exported Package 0 README.

# v0.13.0-preview.40

- Explain optional trained-pair shortcuts and add module and sensor examples in Notepad F1.

# v0.13.0-preview.39

- Explain how to adapt Integrate nuget and add copyable CAM board and rename examples.

# v0.13.0-preview.38

- Move patch inputs before Notepad, adapt F1 to patch inclusion and use compact copy icons.

# v0.13.0-preview.37

- Flag unrecognized Notepad section starts in Package 0 Preview and align workflow rules.

# v0.13.0-preview.36

- Add request-specific Notepad help with copyable examples.

# v0.13.0-preview.35

- Place the F1 help button immediately after the Package 0 heading.

# v0.13.0-preview.34

- Add contextual Package 0 help and hide Preview while its panel is open.

# v0.13.0-preview.33

- Show only the Package 0 inputs relevant to the selected generation request.

# v0.13.0-preview.32

- Support copy-only deployment for any existing module.

# v0.13.0-preview.31

- Support copy-only EMPTY and explicit Program.cs / packages.config changes in a deployment zero patch.

# v0.13.0-preview.30

- Resolve one or more zero-patch target files from Notepad and the verified baseline pair.

# v0.13.0-preview.29

- Add a Package 0 request for implementation-only deployment zero patches.

# v0.13.0-preview.28

- Accept VERIFIED Package 12 baselines that explicitly declare no active deployment patch.

# v0.13.0-preview.27

- Simplify patch choices and let a compatible HAL patch be selected without a board-change request.

# v0.13.0-preview.26

- Clarify the opt-in checkbox for additional deployment zero patches.

# v0.13.0-preview.25

- Include-only patches are VS2022 placement inputs and do not gate Model-First or Code-First generation.

# v0.13.0-preview.23

- Package 0 preview aligns baseline labels and values and shows ZIP folders and files in a tree.

# v0.13.0-preview.22

- Package 0 preview with task and baseline sections, clipboard copy and the exact ZIP used by Download.
- Patch selection now names the revision, explains compatibility and distinguishes carrying a patch from integrating it.

# v0.13.0-preview.21

- Lower Resume package0 to overlap the status bar.

# v0.13.0-preview.20

- Resize the package0 panel from its left edge. Download stays visible; collapse is in the header.

# v0.13.0-preview.19

- Docked Create package0 editor with session drafts; browse HAL without closing the task.
- Keep the chosen pair and repository inputs bound to the task.
- Align Example 3 acceptance path and documented workflow versions.

# v0.13.0-preview.18

- HAL browser draft and view restoration.
- Shared board Notes in PinMap, Resources and package0.
- PinMap DELETE with the same marking control as Resources.

## Previous preview changes

- Merge HAL Paste into the selected custom board; remove draft rows and preview Human Request text.
- Require explicit selection for additional repository zero patches; preserve patches carried by the selected pair.

- Match DDR board identity to its uniquely corresponding preprocessor symbol case-insensitively; never highlight #define as a missing-section fallback.

- Code-First Deployment Target: DDR Overview and Source highlighting of the matching Program.cs board preprocessor section.

- JSON scalar grammar (quoted strings, bare numbers, lowercase booleans, distinct DELETE), shared validation and Type column.
- Model-First Deployment double-click shortcuts to HAL/Resources and HAL/PinMap; Code-First Placer routing preserved.
- Resize all HAL table columns; group-scoped DDR suffix labels and HAL-only Other source highlight.
- Add row appears only for a selected custom board; copy/clone before extending pins or resources.
- Context-sensitive PinMap / Resources menu and Add row; resource suffix in a concrete group, full path in Other / All resources.
- New resource rows preserve board values and reject duplicate/unsafe/path-shape conflicts.
- Packages / HAL / Placer share a subtle visual group; HAL is a separate context-gated Tab.
- Package breadcrumb navigation replaces global Exploring / HAL-source selectors; no nested HAL guessing.
- Active Micro LAR stays left on toolbar; repository actions move into the dots menu.
- PinMap and Resources have bounded vertical and horizontal table scrolling.
- New-board import with collision suffixes and missing PinMap rows; derived Resource groups and group-scoped export.
- Compact Query buttons, hidden when empty. Scoped acceptance and integrated-patch history policies.
- Separate PinMap / HAL Resources inheritance; scalar syntax/type validation; domain/hardware checks remain downstream; explicit DELETE operation and hover ×.
- Multiple same-board tables, resizable columns and mandatory pin_map semantic contract in AI output.
- Current versus superseded review hash references and exclusion of generated caches declared in generic workflows.
- Bounded two-column DDR / END_DDR format with matching board names; legacy three-column input supported.
- Partial updates and empty rename-only table; atomic validation and protected Default.
- Existing scalar Resources editable in custom profiles; Copy board copies complete PinMap and Resources.
- Copy/Paste/Import/Export use one text grammar with explicit base/profile provenance in package0.
- Generic behavioral SYS startup rules for primary HAL pin=-1; normal platform entry generation, no implementation variables in DDR.
- Known Foundation board requests carry a hash-guarded four-resource compatibility overlay with evidence; no active Program.cs replacement.
- Foundation pair, library, drivers and agile module archive bytes unchanged. Resource fixes remain separate active patches until selected for integration.

Closed stable baseline: v0.12.0. Only this latest preview is distributed; previous previews remain separately archived.

## HAL board drafts and notes
Browser drafts restore the board, rows and current HAL view after reopening this Explorer in the same browser. Export text for portability. Board Notes are shared by PinMap and Resources, and travel with Human Request and text transfer. Omitted Notes leave the current value untouched on Paste; Notes: "" clears it. DELETE marks a target-board key for removal and is distinct from an empty cell, -1 and the draft-row ×.
