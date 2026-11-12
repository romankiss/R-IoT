# HAL — walking through the selected package

Workspace contains all opened repositories and packages. **Active Micro LAR** is the default repository scope, not a substitute for the current package. Its actions are in the second-toolbar dots menu; unavailable actions remain grey.

HAL is a workspace Tab immediately after Packages. Navigate down through Packages to the concrete package1 or package2, then select HAL. The Tab is enabled only when that package directly declares `Deployment.HAL.board.pin_map` or `Deployment.bindings.HAL.board.pin_map`. A repository, package0 collection or pair container without its own HAL leaves the Tab disabled. HAL never searches nested members or an unrelated default Foundation.

The breadcrumb preserves the whole package path. Back from HAL returns to that package; breadcrumb links return to ancestors and Workspace. Packages, HAL and Placer share a subtle visual group. Examples remains the package0/12 and Module assembly area, where choosing sources is intentional.

HAL PinMap and Resources are sidebar views. Resource subgroups derive from model paths, without hardcoded module names. Source view highlights original DDR, including bindings/HAL for Code-First. The model source label comes from the package path, not another selector. PinMap/Resources tables scroll vertically and horizontally within their client area.

## Board drafts and transfer

Default is immutable. Add makes an unspecified custom board. Copy copies the complete selected profile, including PinMap and Resources. Drafts belong to exact source model bytes. Original input ZIPs are never rewritten. MicroLAR drafts can be retained with Export active; standalone package drafts can be exported as profiles JSON or transfer text.

Paste/Import works directly with Default selected: the DDR header supplies a new custom board name. A collision gets _copy(1), _copy(2), and so on. The new board is selected automatically. Missing logical PinMap identifiers add rows; other boards get blank cells for those new rows. Imports never overwrite an existing board. Structural conflicts reject the complete import before committing.

```text
DDR                     "ATOMS3R_CAM"
sys_neo_data            -1
sys_boot_button         -1
spi_mosi                5
spi_miso                6
spi_clock               7
sys_user_led            18
lora_irq                8
lora_reset              -1
lora_busy               39
lora_chip_select        38
END_DDR                 "ATOMS3R_CAM"

DDR                     "ATOMS3R_CAM"
HAL.modules.lora.address "0x0899"
END_DDR                 "ATOMS3R_CAM"
```

Multiple tables for the same board are merged. Duplicate resolved properties or mismatched terminators are conflicts. Spaces or tabs separate two columns; the remainder is the value. Legacy three-column pipe input remains readable.

Copy/Export includes only nonempty values from the selected board and visible PinMap/Resources group. Numbers are unprefixed; strings are JSON-quoted and booleans use true/false. Explorer validates scalar syntax and row types; hardware/domain semantic validation belongs to Model-First. Resources groups derive from DDR paths (Board resources, Buses, Modules), with All resources available. Filtering never removes other values. Drag board header edges to resize. In custom PinMap and Resource cells, hover × writes DELETE as an explicit removal request.

## package0 and increment semantics

Select a mandatory VERIFIED package12 pair. When exploring a standalone package or handoff, Create package0 offers **Selected package / current path**; it discovers only pairs available there or in its enclosing pair. A standalone package1/package2 without an available pair requires an explicit other source or Browse. No pair is invented.

Use `Integrate board: inline` with bounded tables or `Integrate board: <exact saved profile name>`. The exporter captures rows, values, provenance and digest in package0/HAL/board.json.

- New-board pins use explicit mappings only. Missing pins never inherit old-board wiring; required missing bindings become Model-First findings.
- Unlisted/empty HAL Resources inherit from the actual selected pair without asking.
- DELETE requests removal, subject to required-property and dependency checks.
- A header-only inline task is rename-only and retains the explicitly selected profile mapping.
- SYS resources with primary GPIO -1 skip activation and dependent indication; other activation continues. Assigned-resource failures retain the declared failure policy. DDR describes behavior, not implementation variables.
- New package1 must explicitly declare numeric GPIO minimum -1, sentinel/collision rules, symbolic values and consumer-specific support. Model-First OUT and Code-First IN independently check it.

Old Foundation resource fixes remain explicit compatibility patches until selected for integration. Known exact pair/preimage hashes guard their inclusion; Program.cs is generated normally. `Integrate patch: all` integrates through Model-First → Code-First. Retired scopes become historical in current wrappers; original patch manifests and evidence remain unchanged. Integration never grants VERIFIED by itself.

## Add resource properties
The dots menu follows the view: PinMap or Resources, each with Add row. In a concrete Resource group, the dialog shows its exact HAL prefix and accepts a dot-separated suffix such as radio.tx_power. Other and All resources accept a complete HAL.* property path (or HAL JSON pointer). The full resulting path is previewed before Add row. New cells are blank in every board; Default values are never edited. Use a custom board to supply values. Duplicate/unsafe paths and scalar/object conflicts are rejected atomically; property semantics and hardware support remain Model-First validation.

## Board extension and scoped table view
All columns (DDR, Function and board columns) can be resized at header edges. In a specific Resources group, DDR shows only the relative property suffix; the full path remains in its tooltip and exported text. The header includes the group name. Other stays inside HAL and its whole-group Source highlights the complete HAL object.

Add row appears only when a custom board column is selected, in both PinMap and Resources. Copy/clone the existing board and optionally rename it before extending it (for example ATOMS3R_CAM with internal camera bindings). Default does not offer Add row. New rows create blank cells in other boards; original DDR/baseline values remain unchanged.

## JSON scalar values — editor/import/export

Cells and two-column transfer use the same literal values as the DDR:

```text
DDR                "ATOMS3R_CAM"
spi_mosi           12
power_5v           "5V"
my_flag            false
HAL.custom.text    "123"
HAL.custom.remove  DELETE
HAL.custom.literal "DELETE"
END_DDR            "ATOMS3R_CAM"
```

Text is always JSON-quoted; numbers have no prefix. Boolean input accepts true/false and uppercase variants; output canonicalizes to true/false. DELETE without quotes removes a PinMap key or HAL Resource property. Blank is unspecified; "" is explicit empty text. Type stays a presentation column, not an additional transfer column. Existing types are read from DDR, while a new row binds on its first scalar value. Wrong types and malformed literals reject atomically. Native types and separate deletion entries persist into package0; no #, number: or bool: prefixes are used.

Model-First Deployment: double-click HAL to open HAL/Resources, or Pins to open HAL/PinMap. Code-First retains its Placer route.

Paste merges a matching DDR table into the selected custom board, adding missing rows and retaining unlisted values. Import .txt still creates a board. The Human Request tab shows the selected board and current view in inline export form. Use × in the DDR column to remove a draft row and its values across profiles; this does not emit a DELETE directive.

## HAL board drafts and notes
Browser drafts restore the board, rows and current HAL view after reopening this Explorer in the same browser. Export text for portability. Board Notes are shared by PinMap and Resources, and travel with Human Request and text transfer. Omitted Notes leave the current value untouched on Paste; Notes: "" clears it. DELETE marks a target-board key for removal and is distinct from an empty cell, -1 and the draft-row ×.

Create package0 opens a docked editor. Navigate to HAL, copy Human Request (including Board Notes), then paste into the task. Collapse and Resume keep the draft in the current Explorer session. Browsing another package or switching the active repository does not change a chosen baseline or this draft’s repository inputs. Download the ZIP before leaving the session.
