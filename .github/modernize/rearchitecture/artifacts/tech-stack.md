# Existing Technology Stack

- Language: C#.
- Runtime: .NET Framework 4.8 for `EdataManagement` and `BusinessData`; the standalone `Login` project targets .NET Framework 4.5.
- UI: Windows Forms, with Bunifu UI controls.
- Data access: MySQL Connector/NET (`MySql.Data`), raw SQL strings, `DataTable`, and manual transactions.
- Office integration: Microsoft Office Excel Interop and Outlook Interop.
- File/integration libraries: SSH.NET for SFTP, SevenZipExtractor, INIFileParser.
- Build: Visual Studio/MSBuild legacy-style `.csproj`; solution format 12.
- Database configuration: `EdataManagement/App.config` connection strings and app settings.
- Binding: MySQL assembly binding redirect to 8.0.13 in the executable config; project references include MySql.Data 8.0.33.
- Authentication/authorization: employee lookup in MySQL plus `user_setting_edata` role/INUSE checks. A Security Center HTTP API client exists, but the password step is currently disabled in `frmLogin`.
- Configuration constraints: fixed database/server endpoints, SMTP/Outlook settings, allowed IP gate, and local filesystem paths are runtime-relevant.
- Legacy indicators: raw SQL concatenation, synchronous `.Result` HTTP calls, manual connection lifecycle, mixed old web references in `BusinessData`, and a separate .NET 4.5 Login project.
