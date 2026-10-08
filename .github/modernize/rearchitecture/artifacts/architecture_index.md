# Architecture Index

This index is not the full contract. Do not implement from this file alone; follow the artifact paths below.

## Implementation Guide

### Global artifacts

- `unit_graph.yaml`: entry points, source anchors, dependencies, dynamic entry points, and shared references. Filter by the assigned `unit.name`.
- `migration_boundary.yaml`: analysis-only scope; `must_rewrite` is empty. Do not treat source anchors as rewrite targets.
- `wire_contracts.yaml`: filter contracts by `consumers` before changing database, Outlook, or HTTP behavior.
- `shared_modules.yaml`: filter rows where `used_by_units` contains the assigned unit; preserve the listed field types and strategy.
- `cross_unit_state.yaml`: filter flows where the unit is writer or reader; preserve matched state hand-offs.
- `seams.yaml`: filter `cut_between` for the unit; the frozen side rule applies to BusinessData.
- `project-structure.md`, `tech-stack.md`, `data-model.md`: global context only; they document existing facts and do not authorize redesign.

### Unit: application_startup

- external trigger: process launch
- must read: `units/application_startup/behavior.yaml`, `units/application_startup/bindings.yaml`, `units/application_startup/unit_decomposition.yaml`
- relevant global rows: `unit_graph.yaml` startup row; `shared_modules.yaml` IP/mail safety references
- before DONE report: artifacts read, IP-denial and frmMain-launch behavior preserved, and verification evidence.

### Unit: main_shipping_workflow

- external trigger: frmMain product/date/customer actions
- must read: `units/main_shipping_workflow/behavior.yaml`, `units/main_shipping_workflow/bindings.yaml`, `units/main_shipping_workflow/unit_decomposition.yaml`
- relevant global rows: wire database contract; shared DB/mail safety rows; cross-unit grid state.
- before DONE report: selected product/date/customer and empty-result behavior preserved, plus verification evidence.

### Unit: email_workflow

- external trigger: emailUserControl send action
- must read: `units/email_workflow/behavior.yaml`, `units/email_workflow/bindings.yaml`, `units/email_workflow/unit_decomposition.yaml`
- relevant global rows: Outlook contract, MailSafety shared module, main-to-email state flow.
- before DONE report: attachment validation, recipient mapping, test-mode draft safety, and Outlook error behavior preserved.

### Unit: settings_specification

- external trigger: Settings specification tab
- must read: `units/settings_specification/behavior.yaml`, `units/settings_specification/bindings.yaml`, `units/settings_specification/unit_decomposition.yaml`
- relevant global rows: MySQL contract, settings UI shared module, login-to-settings state.
- before DONE report: unsaved-change prompts, save/cancel, and specification/recipient updates preserved.

### Unit: settings_login

- external trigger: frmLogin or Login Permission tab
- must read: `units/settings_login/behavior.yaml`, `units/settings_login/bindings.yaml`, `units/settings_login/unit_decomposition.yaml`
- relevant global rows: database contract and CurrentUser shared state; Security Center HTTP contract is currently disabled.
- before DONE report: employee/resigned/whitelist/role branches and ADMIN visibility preserved.

### Unit: business_data_access

- external trigger: service calls from application units
- must read: `units/business_data_access/behavior.yaml`, `units/business_data_access/bindings.yaml`, `units/business_data_access/unit_decomposition.yaml`
- relevant global rows: database wire contract, shared DB gateway, BusinessData seam.
- before DONE report: connection, commit, rollback, close, and result-envelope semantics preserved.
