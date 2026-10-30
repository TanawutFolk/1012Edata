# 1012 Send E-data (EdataManagement)

โปรแกรมเตรียมและส่ง E-data (ไฟล์ผลเทสของ shipment) ให้ลูกค้า ทางอีเมล (Outlook) หรือ SFTP
เลือก Product + วันที่ ship + ลูกค้า แล้วโปรแกรมจะหาไฟล์ใน `yyMMdd-Shipping Data` ตรวจความครบ แบ่งโฟลเดอร์ / zip และสร้างเมลตามค่าที่ตั้งไว้ของแต่ละ spec

- Branch ปัจจุบัน: `ver4.0.0` (เริ่มจาก commit `Start 4.0.0`)
- Output: `EdataManagement\bin\Debug\1012 Send E-data 48-98 Direct V4.0.0.exe`

---

## 1. Tech stack

| เรื่อง | ใช้ |
|---|---|
| UI | WinForms, .NET Framework 4.8, Bunifu UI v1.52 |
| Database | MySQL (MySql.Data 8.0.13; `BusinessData.dll` compile กับ 6.3.6 จึงมี bindingRedirect ใน App.config) |
| Mail | Outlook (Interop) |
| SFTP | `ftpUserControl` |
| Excel / ini | ExcelCls, IniParser |

## 2. โครงสร้าง solution

```
EdataManagement.sln
├─ BusinessData\          ต่อ DB: DatabaseAction (connect / commit / rollback / close), ผลลัพธ์เป็น OutputOnDbProperty
└─ EdataManagement\
   ├─ Program.cs          เช็ค IP → เปิด frmMain
   ├─ Views\              ฟอร์มและ UserControl
   │  ├─ frmMain          หน้าหลัก เลือก product / วันที่ / ลูกค้า / order
   │  ├─ emailUserControl ตรวจไฟล์แนบ + ส่งเมล
   │  ├─ ftpUserControl   อัปโหลด SFTP + เตรียม zip สำหรับ DUC
   │  ├─ frmLogin         ประตูเข้า Setting
   │  ├─ frmConfirm / frmError
   │  └─ Setting\         frmSetting, ucSpecSetting (+ .Recipients), ucLoginSetting, GroupOverviewDialog, SettingShared
   ├─ Controllers\        เรียกจาก View
   ├─ Models\             Controller → Services
   ├─ Services\           เลือก connection + รัน SQL ผ่าน BusinessData
   ├─ SQLFactory\         สร้างข้อความ SQL
   ├─ Property\           DTO ที่ส่งไปมาระหว่างชั้น
   ├─ Utilities\          EmailCls, FileCls, MailSafety, DatabaseSelection, DucZipCls, IPChecker, ExcelCls
   └─ DatabaseScripts\    SQL สร้างตาราง / เตรียม DB, สคริปต์ข้อมูลเทส
```

ลำดับการเรียก: **View → Controller → Model → Service → SQLFactory → BusinessData**

## 3. การทำงานหลัก

1. **เปิดโปรแกรม** – `IPChecker` เทียบ IP เครื่องกับ `allowIP` ใน App.config ไม่ตรง = ปิดโปรแกรม
2. **frmMain โหลด** – ดึงรายการ product (`info_db_name`, path shipping) และ sync ข้อมูล spec ล่าสุดของแต่ละ product (`updateLastData`)
3. **เลือก Product + วันที่ → Confirm** – ดึงลูกค้าที่มี shipment วันนั้นจาก product DB
4. **เลือกลูกค้า** – แสดง order (test report) จากตาราง `shipping`
5. **ส่ง**
   - Email: `EmailCls.prepareFileInfoEmail` (All order) หรือ `prepareFileInfoEmailSelection` (เฉพาะที่เลือก) แยกตาม `FILE_FORMATS_ID` → `emailUserControl` แสดงไฟล์ / เมลแล้วส่ง
   - SFTP: `ftpUserControl`

path ที่ใช้ (frmMain):
```
emailProp.pathShipping   = {PATH_SHIPPING}\yyMMdd-Shipping Data\
emailProp.folderCustomer = {pathShipping}yyMMdd-{Customer}({productNameInEmail})\
```

Product DB ที่ 1012 อ่าน: `info_cus_condition`, `shipping`, `db_check_sheet`
ค่าตั้งการส่งต่อ spec (KEY_ID = `CusCondition_ID`) อยู่ใน config DB:
`key_send_methods_{product}`, `email_attach_{product}`, `key_group_email_{product}`, `email_subject_{product}`, `email_body_{product}`

## 4. File formats (EmailCls)

| FILE_FORMATS_ID | ใช้กับ | หมายเหตุ |
|---|---|---|
| 1–8 | format เดิมของ 1012 | case 6 / 7 / 8 มีเมล Serial List / Wafer แยก (ผู้รับจาก `key_group_email_document`) |
| 3 (MOLEX, MOLEX-SG, OPLINK, OPLINK-SUBMARINE) | 1480, 980Dual | `prepareCase3PrepareZip` – ดูข้อ 4.1 |
| 10 | XTERA-INC, SMTC-CORP | ชื่อไฟล์ `yyMMdd-{Shipping-PO-all คอลัมน์ V}(short Part No).xls` อ่าน PO จาก `Case10SpecialPOAllPath` |
| 13 | 1480Dual COHERENT / INFINERA | `prepareCase13TxtEdata` – แนบ txt ใน `_Edata`, เกิน 10 MiB / 256 ไฟล์ แบ่งเป็นหลายเมล `(i/N)` |
| 14 | 1480Dual MOLEX | `prepareCase14TxtZip` – ดูข้อ 4.2 |

### 4.1 Format 3 – MOLEX / OPLINK ของ 1480, 980Dual
1. (All order) จำนวนโฟลเดอร์เทสรีพอต ต้องไม่มากกว่า order ใน DB
2. ทุก order ต้องมีโฟลเดอร์ `yyMMdd-{FEC order}(short Part No)[-sub]`
3. copy ทุกไฟล์ ยกเว้น `For QA check` แบ่ง `-n` เมื่อเกิน 10 MB / 256 ไฟล์ (ไม่ผ่าโฟลเดอร์เทสรีพอต)
4. zip แต่ละ `-n` แล้วอ่านกลับมาเช็คว่าครบ
5. 1 zip = 1 เมล, subject ต่อท้าย `(i/N)`

```
yyMMdd-Shipping Data\
   Send by 1012 program\
      yyMMdd-{Customer}({product})-n\{test report folder}\...
      yyMMdd-{Customer}({product})-n.zip
```
ถ้าเจอ `-n` / `-n.zip` อยู่แล้ว (ทั้งใน `Send by 1012 program` และที่ root ของ Shipping Data แบบเวอร์ชันก่อน) จะเตือนและหยุด ไม่เขียนทับ
(`Send` เป็นของโปรแกรม 1084 – 1012 ไม่แตะ)

### 4.2 Format 14 – 1480Dual MOLEX
```
yyMMdd-Shipping Data\
   MOLEX-Data for 1012 program\
      yyMMdd-MOLEX(48Dual)-n\{order folder}\{txt}
      yyMMdd-MOLEX(48Dual)-n.zip
```
- copy เฉพาะ txt (`{Customer PN}_{FEC order ตัด '-'}_{Qty}PCS_{Part No ก่อน '-'}_{yyMMdd}-{sub}.txt`)
- แบ่ง `-n` ตาม 10 MB / 256 ไฟล์ (ขนาดก่อน zip)
- หลาย zip รวมในเมลเดียวจนกว่าขนาด zip รวมจะเกิน 10 MiB

รายละเอียดและงานค้างของ 1480Dual: [`PENDING_1480Dual.md`](PENDING_1480Dual.md)

## 5. Setting

เปิดจากรูปเฟือง "Setting" ใน frmMain ต้อง login ก่อน (`frmLogin`)

- **Login**: รหัสพนักงานต้องอยู่ใน `mfg.info_employees`, ไม่ลาออก, และอยู่ใน `user_setting_edata` (INUSE = 1) → ได้ ROLE (`ADMIN` / `USER`)
  - การเช็ครหัสผ่าน portal (Security Center API) **ปิดอยู่** – ช่อง Password ไม่ถูกอ่าน (`frmLogin.btnLoginn_Click`)
- **Spec** (`ucSpecSetting`): send method / file format / group / subject / body ของแต่ละ spec และผู้รับของ group (`info_group_email`, ชื่อ group ใน `info_group_name`)
- **Login Permission** (`ucLoginSetting`): เห็นเฉพาะ ADMIN
- **Database selector**: สลับ `qa_oc_test` ↔ `qa_oc` – เขียนลง `.exe.config` แล้วเปิดโปรแกรมใหม่ (`DatabaseSelection`)
- ทุกการแก้บันทึกลง `log_setting_edata`

## 6. Test mode (MailSafety)

ถ้า `ConnectionQAOC` **ไม่ใช่** `qa_oc` = test mode (อ่านชื่อ DB ไม่ได้ก็ถือเป็น test)

- เมลถูกเก็บเป็น **Draft ไม่มีผู้รับ** subject ขึ้นต้น `[TEST - NOT SENT] ` และมีกล่องบอกผู้รับที่ควรได้ด้านบน body
- ยกเว้นผู้รับ **ทุกคน** อยู่ใน group ของ `TestMailGroupIds` (App.config):
  - group 0 – กลุ่ม QA ที่อนุมัติ มีผู้รับภายนอกได้
  - group อื่น (เช่น 1000) – ต้องเป็น `@furukawaelectric.com`
- SFTP ไปได้เฉพาะโฟลเดอร์ `fftTest` บน server เท่านั้น
- frmMain แสดง `TEST MODE (...)` สีแดง

## 7. Config (App.config)

| key | ใช้ทำอะไร |
|---|---|
| `ConnectionQAOC`, `ConnectionStandAlone` | config DB (`qa_oc_test` = เทส, `qa_oc` = จริง) |
| `ConnectionShipment`, `ConnectionButterfly*` | DB ข้อมูล shipment / product |
| `ConnectionLogin`, `ConnectionPersonnel`, `ConnectionEMPQA` | ข้อมูลพนักงานสำหรับ login |
| `TestMailGroupIds` (`TestMailGroupId` แบบเก่า) | group ที่ส่งจริงได้ใน test mode |
| `allowIP` | IP ที่เปิดโปรแกรมได้ |
| `ProgramName`, `ProgramVer` | ชื่อ / เวอร์ชันที่แสดง |

## 8. Database scripts (`EdataManagement\DatabaseScripts`)

| ไฟล์ | ใช้ |
|---|---|
| `user_setting_edata.sql` | whitelist ผู้เข้า Setting |
| `log_setting_edata.sql` | log การแก้ Setting |
| `key_group_email_document.sql` (+ `_import_20260924.sql`) | ผู้รับเมล Serial List / Wafer ต่อ product + ลูกค้า (แทน `EmailSetting.ini` ที่เลิกใช้แล้ว) |
| `info_group_name.sql` | ชื่อ group ผู้รับ |
| `1480dual_prepare_qa_oc.sql` | เตรียมตาราง `*_1480dual` ใน config DB |
| `audit_key_tables.sql` | audit trigger ของตาราง key |
| `test_data\make_1480dual_format1{3,4}_test.ps1` | สร้างข้อมูลเทส 1480Dual |

รันบน `qa_oc_test` ก่อน แล้วค่อย `qa_oc` ตอนขึ้น production

## 9. Build

เปิด `EdataManagement.sln` ด้วย Visual Studio แล้ว Build (Debug) หรือ

```powershell
MSBuild.exe EdataManagement\EdataManagement.csproj /t:Build /p:Configuration=Debug
```

## 10. ก่อนขึ้น production

- [ ] App.config: `ConnectionQAOC` / `ConnectionStandAlone` → `qa_oc` (เมลจะถึงลูกค้าจริง)
- [ ] `ProgramVer` (ตอนนี้ยังเป็น `Version 3.0.0`)
- [ ] `allowIP` ของเครื่องที่ใช้งาน
- [ ] รัน script ที่ต้องใช้บน `qa_oc` (ข้อ 8) และตั้งค่า spec / group ผู้รับจริง (ห้ามใช้ group 0)
- [ ] ตัดสินใจเรื่องเปิดเช็ครหัสผ่าน portal ใน `frmLogin`
- [ ] ตรวจ `Case10SpecialPOAllPath` ใน `EmailCls.cs` ชี้ไฟล์ Shipping-PO-all ที่ถูกต้อง
