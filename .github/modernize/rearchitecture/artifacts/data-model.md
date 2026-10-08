# Existing Data Model

## Entity and table families

- Product catalog: `info_db_name`, `info_path_shipping`, and related product/path metadata used to populate the main product selector.
- Shipping source data: product-specific shipping/customer/serial/wafer tables queried by `ShippingSQLFactory`.
- Email specification: product-specific `key_send_methods_*`, `email_subject_*`, `email_body_*`, `email_attach_*`, and `key_group_email_*` tables.
- Reference data: `info_cus_condition`, `info_send_methods`, `info_file_formats`, `info_email_subject`, `info_group_email`, and `info_email`.
- Permission data: `mfg.info_employees` plus `user_setting_edata` with employee code, role, INUSE, and audit timestamps.
- Audit data: `log_setting_edata`; MySQL triggers in `audit_key_tables.sql` record old/new values and update metadata.
- Database result envelope: `OutputOnDbProperty` carries status, message, `DataTable`, and count values across BusinessData and the application.

## Relationships

- Product -> customer/shipping data is selected by product and shipping date.
- Product/customer/specification rows -> email subject/body/file-format and recipient group.
- Recipient group -> many email addresses through `info_group_email` and `info_email`.
- Setting update -> audit log row and/or trigger-generated audit record.
- Employee -> optional application role through `user_setting_edata`.

## Transaction boundaries

`BusinessData.Models.DatabaseExecuteModels` opens a MySQL connection for each operation. Insert/update/delete methods commit after all statements succeed and roll back on the first failure. List variants execute multiple SQL statements in one transaction.

## Key entities

1. Product/path configuration: determines product selection and source directories.
2. Shipping/customer/serial record: drives the rows shown in the main workflow.
3. Email specification: determines send method, format, subject, body, and attachment rules.
4. Recipient group: resolves TO/CC addresses for a customer/specification.
5. User permission: gates access to Settings and separates ADMIN from USER.
6. Audit log: records configuration changes and trigger metadata.
