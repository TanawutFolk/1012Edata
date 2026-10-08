# Project Structure

## Project type

Desktop backend-integrated WinForms application, not a web service. The solution contains the deployable `EdataManagement` executable and the `BusinessData` class library. `Login` is a separate legacy WinForms project in the repository but is not included in `EdataManagement.sln`.

## Modules and layers

- `EdataManagement/`
  - `Program.cs`: process entry point; checks allowed IP, then opens `frmMain`.
  - `Views/`: WinForms forms and user controls. Main user-facing areas are shipping lookup, email preparation/sending, and settings.
  - `Controllers/`: UI-facing orchestration; converts service results to `DataTable`/properties and displays errors.
  - `Services/`: database-facing operations derived from `BusinessData.Interface.DatabaseAction<T>`.
  - `SQLFactory/`: SQL string construction for product, shipping, email, Excel/PO, and settings operations.
  - `Models/`: controller-to-service delegation and result shaping.
  - `Property/`: request and UI data-transfer properties.
  - `Utilities/`: Excel/Outlook/SFTP/file handling, IP guard, mail safety.
  - `DatabaseScripts/`: MySQL schema, audit triggers, setup, and test data.
- `BusinessData/`
  - `DBManage/`: MySQL connection, transaction, select/execute/commit/rollback primitives.
  - `Interface/`: database action contracts.
  - `Property/`: shared database result and property objects.
- `Login/`: standalone older login executable; not a solution project reference.

## Runtime entry points

1. `EdataManagement/Program.cs:18`: checks the configured allowed IP and exits on denial.
2. `EdataManagement/Program.cs:42`: starts `EdataManagement.Views.frmMain`.
3. `frmMain`: loads products and coordinates shipping/email controls.
4. `frmSetting`: lazy-loads `ucSpecSetting` and `ucLoginSetting`.
5. `frmLogin`: validates employee whitelist before opening settings.

## Functional areas

- Product and shipping-date/customer lookup.
- Serial, wafer, PO/Excel and shipping document discovery.
- Email subject/body/recipient generation and Outlook attachment sending.
- SFTP/file/archive support used by shipping workflows.
- Specification settings: send method, file format, subject/body, attachment, recipient groups.
- Login permission settings and ADMIN/USER authorization.
- MySQL audit logging and trigger-based change history.

## Important exclusions

`bin`, `obj`, `.vs`, generated DLL/PDB files, and `msbuild-tmp-obj` are build/editor outputs, not source boundaries.
