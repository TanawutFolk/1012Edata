# 1480Dual E-data (1012) — งานค้าง / สถานะ ณ 2026-10-05

> **2026-10-16 เปลี่ยนแล้ว:** Molex format 14 **มีชั้นนอก** `MOLEX-Data for 1012 program` (user เลือกชื่อเอง เพราะ `MOLEK:Data for 1012` ในรูปใส่ `:` ไม่ได้)
> layout: `yyMMdd-Shipping Data\MOLEX-Data for 1012 program\yyMMdd-MOLEX(48Dual)-n\{order folder}\{txt}` + `...\MOLEX-Data for 1012 program\yyMMdd-MOLEX(48Dual)-n.zip` (เปิด zip เจอโฟลเดอร์ order ทันที)
> โค้ด: `prepareCase14TxtZip` ชื่อชั้นนอก = `{customerName}-Data for 1012 program` — build ผ่าน ยังไม่ได้รันเทส
> (2026-10-05 เคยตัดสินว่าไม่มีชั้นนอก — ข้อ 3.4 ด้านล่างเป็นประวัติ)

---

## 1. ภาพรวม

| ลูกค้า | Work order (Specification) | FILE_FORMATS_ID | สถานะ |
|---|---|---|---|
| COHERENT | Y-3ER4003A | 13 (TXT_Edata) | เทสใน qa_oc_test ผ่าน ผู้ใช้ยืนยันเนื้อเมลแล้ว — **spec จริงยังไม่มีใน `1480dual.info_cus_condition`** |
| INFINERA | Y-3ER2002A | 13 (TXT_Edata) | โค้ดเดียวกับ Coherent — **spec จริงยังไม่มี, ยังไม่มีลูกค้า INFINERA ใน 1480dual เลย** |
| MOLEX (Oplink) | Y-3ER4002C | 14 (TXT_Zip) | โค้ดตรงกับ flow แล้ว (ไม่มีโฟลเดอร์ชั้นนอก) — เหลือเรื่องในข้อ 3.5 + ขึ้น production (ข้อ 6.4) |

- "Work order" = คอลัมน์ `Specification` ใน `<product DB>.info_cus_condition`
- 1012 อ่าน product DB แค่ 3 ตาราง: `info_cus_condition`, `shipping`, `db_check_sheet`
- ค่าตั้งการส่งเมลต่อ spec (KEY_ID = `CusCondition_ID`) อยู่ใน config DB: `key_send_methods_1480dual`, `email_attach_1480dual`, `key_group_email_1480dual`, `email_subject_1480dual`, `email_body_1480dual`

---

## 2. Format 13 — COHERENT / INFINERA (เสร็จฝั่งเทส)

โค้ด: `EmailCls.prepareCase13TxtEdata` (case "13" ทั้ง All order และ Selection) + `emailUserControl` (1 เมลต่อ 1 sub-folder)

1. `{customer folder}\yyMMdd-{FEC order}(full Part No)-{sub}` (หรือไม่มี `-{sub}`) ไม่เจอ → "ไม่พบเทสรีพอตหมายเลข ..."
2. `...\yyMMdd-{FEC order}(full Part No)_Edata` ไม่เจอ → "ไม่พบเทสรีพอตหมายเลข ..."
3. จำนวน `*.txt` ต้องเท่ากับ Qty (จำนวนแถว serial ใน `shipping` ของเทสรีพอตนั้น)
4. รวม txt ทุก order — เกิน 10 MiB หรือ 256 ไฟล์ → copy ไป `{customer folder}\yyMMdd-{Customer}(48Dual)_Edata-n`, 1 โฟลเดอร์ = 1 เมล, subject ต่อท้าย `(i/N)`
5. เนื้อเมล: subject 7 + body 8 + signature (ผู้ใช้ยืนยัน "ถูกต้อง ที่ผมต้องการคือแบบนี้แหละ")

หมายเหตุ: ของที่ส่งจริงเมื่อ 260804 (`\\office_SRV\C06_Shipping-Data\1480Dual\260804-Shipping Data\send\260804-COHERENT(48Dual)-1.zip`) เป็น zip ทั้งโฟลเดอร์ order (Cover.xls, LDS, xls, _Edata\txt) — ผู้ใช้เลือก **คงแบบแนบ txt** (2026-09-29)

---

## 3. Format 14 — MOLEX (Oplink) Y-3ER4002C

### 3.1 Spec ที่ได้รับ (flowchart + รูป "Request 2.3 Add send Molex(Oplink) work order Y-3ER4002C")
1. Test report > Data base? → "จำนวนเทสรีพอตมากกว่าดาต้าเบส"
2. Have test report folder? (`yymmdd-[PO number](full part number)`) → "ไม่พบเทสรีพอตหมายเลข...."
3. File name of text correct? (`Molex PN_FEC order no_Qty_Furukawa MPN_Shipment date(YYMMDD)-x.txt`) → "ไม่พบไฟล์ดาต้า"
4. Issue folder LD_Data (`yymmdd-customer(48)-LD_Data : Molex`)
5. Copy text file paste on folder
6. Size over 10 MB or more than 256 files? → Separate to sub-folder (add suffix number)
7. Prepare E-data by Zip folder → Show path → Send Email

รูปประกอบ: "1. Folder create by program" ชี้กล่อง **"MOLEK:Data for 1012"** → ข้างในมี `260512-MOLEX(48Dual)-1\{order folder}\NA_...txt`, "2. Copy folder and text file inside ... (not show in folder)", "3. Zip file"

### 3.2 ที่ตัดสินใจแล้ว (ผู้ใช้ตอบ 2026-09-29)
- sub-folder ใช้ชื่อตามของจริง `yymmdd-MOLEX(48Dual)-n` + `-n.zip`
- copy **เฉพาะ txt** ใส่ไว้ใน **โฟลเดอร์ order ชื่อเดิม**: `-n\{order folder}\{txt}` (แก้แล้วหลังผู้ใช้ส่งรูป)
  - ผู้ใช้ยืนยันซ้ำ 2026-10-16 ว่า "txt อย่างเดียว" แม้ zip ที่ส่งจริง 260512 จะมีไฟล์ `260512-{order}(PN).xls` อยู่ด้วย (ไม่ต้อง copy xls)
- เกณฑ์ 10 MB / 256 ไฟล์ นับจากขนาด **ก่อน zip** (ใช้แบ่ง `-n` / zip เท่านั้น)
- ~~1 zip = 1 เมล~~ → **2026-10-19 เปลี่ยน:** ทุก zip ของวันนั้นแนบเมลเดียวกัน (แบบ case 10 ที่แนบทุกไฟล์ในเมลเดียว) ขึ้นเมลใหม่เมื่อ zip รวมกันจะเกิน 10 MiB (นับขนาด zip), zip เดียวที่เกินไปเมลของตัวเอง, subject `(i/N)` เฉพาะตอนมีหลายเมล
  - เหตุผล (user): "ส่งแค่ 2 zip shipping วันเดียวกัน แยก 2 เมลมันแปลก" — ของจริง 260512: txt 10PCS 6.96 MB → zip 0.93 MB
  - format 3 (MOLEX/OPLINK ของ 1480/980Dual) ยังเป็น 1 zip = 1 เมล (ยังไม่ได้แก้)
- subject 7 (`...for customerName` → "for MOLEX") + body 8 — ผู้ใช้เปลี่ยนใจจาก "Molex(Oplink)" กลับเป็น subject 7 เอง

### 3.3 ชื่อไฟล์ txt (ถอดจากของจริง `NA_301235900610001_10PCS_FRL1441D4045_260512-1.txt`)
`{Customer_Part_No}_{FECOrderNo ตัด '-'}_{Qty}PCS_{Part_No ก่อน '-' ตัวแรก}_{yyMMdd}-{SubFolder}.txt` — 1 ไฟล์ต่อ 1 order (รวมทุก serial)

### 3.4 (ประวัติ — ถูกแทนด้วยชั้นนอก `MOLEX-Data for 1012 program` เมื่อ 2026-10-16 ดูหัวไฟล์) ตัดสินเมื่อ 2026-10-05 — ไม่มีโฟลเดอร์ชั้นนอก

user แจ้งว่า Molex ของ 1480Dual ใช้ flow เดียวกับ Molex ของ 1480 (spec flow 1-6 ของ 1480 format 3 ที่ user ส่งมา 2026-10-05)
**ต่างกันแค่ไฟล์ที่เตรียม:** 1480 / 980Dual copy ทุกไฟล์ยกเว้น `For QA check`, 1480Dual copy **เฉพาะ txt**

ผลลัพธ์ = `yymmdd-MOLEX(48Dual)-n\` + `yymmdd-MOLEX(48Dual)-n.zip` วางที่ `{yymmdd}-Shipping Data\` ตรงๆ ระดับเดียวกับโฟลเดอร์ลูกค้า:
```
260512-Shipping Data\
   260512-MOLEX(48Dual)\                     (โฟลเดอร์ลูกค้า ต้นทาง)
   260512-MOLEX(48Dual)-1\{order folder}\{txt}
   260512-MOLEX(48Dual)-1.zip                (เปิดแล้วเจอ {order folder}\ ทันที)
```
- ชื่อลูกค้าใน `-n` = `info_cus_condition.Customer` (`MOLEX`) — ไม่ใช่ `Molex`, ไม่มี `LD_Data`
- suffix ใช้ `-1` (ไม่ใช่ `_1`)
- order เดียวเกิน 10 MB / 256 ไฟล์ → อยู่ `-n` ของตัวเอง (ไม่ผ่า order)
- ข้อความ error คงแบบเดิม (หัวกล่อง + ข้อความภาษาไทย) — user ยืนยัน 2026-10-05
- 1084 ไม่รองรับ 1480Dual เลย → ไม่มีปัญหาโฟลเดอร์ `-n` ที่ root ไปขวาง 1084 (ต่างจาก 1480 / 980Dual)

<details><summary>ประวัติ: ตัวเลือกที่เคยพิจารณา (2026-09-29)</summary>

จุดขัดกัน:
- "MOLEK:Data for 1012" มี `:` → เป็นชื่อโฟลเดอร์ Windows ไม่ได้ (น่าจะเป็นป้ายอธิบาย) และสะกด MOLEK
- flowchart เขียน `yymmdd-customer(48)-LD_Data` แต่ 1480Dual ใช้ `(48Dual)`
- ของที่ส่งจริงเมื่อ 260512 **ไม่มี**โฟลเดอร์ชั้นนอก (`260512-MOLEX(48Dual)-1` + `.zip` อยู่ใต้ `260512-Shipping Data` ตรงๆ)
- โปรแกรมเก่า **1128** (`C:\Users\tanawut.pat\Desktop\1128\1128Edata\SendEdata_OC.sln`, `clsCopyData.cs`) ใช้ชั้นนอกชื่อ **`Send`**:
  ```
  {yymmdd}-Shipping Data\
     Temp\                                (copy ทั้งโฟลเดอร์ลูกค้ามาพัก แล้วลบ)
     Send\
        {yymmdd}-{Customer}({product})-1\
            {order folder}\               (ทุกไฟล์ ยกเว้น "For QA check")
        {yymmdd}-{Customer}({product})-1.zip
  ```
  - แยก -n ตามขนาดโฟลเดอร์ order > 10 MB, ถ้ามี `Send` อยู่แล้ว **ข้าม copy/zip ทั้งหมด**
  - อัปโหลดขึ้น **Box** (ไม่ใช่เมล), ลูกค้าที่อนุญาต: `Molex-SG`, `Molex`, `Oplink`, ini ไม่มี 1480Dual
  - `260804\send\260804-COHERENT(48Dual)-1.zip` บน share จริงตรงกับ layout นี้

ตัวเลือก:

| # | โฟลเดอร์ชั้นนอก | ของใน order | ข้อดี / ข้อเสีย |
|---|---|---|---|
| 1 | `yymmdd-MOLEX(48Dual)-LD_Data` | txt | ตรง flowchart + แบบที่ 1012 ใช้กับลูกค้าอื่น (case 7 `-LD_Data`), ไม่ชนกับ 1128 |
| 2 | `Send` | txt | เหมือน 1128 ที่ผู้ใช้คุ้น — **เสี่ยง:** ถ้าวันหลังตั้ง 1128 ให้ทำ 1480Dual มันจะเห็น `Send` ที่ 1012 สร้างแล้วข้าม copy/zip → อัปโหลดของไม่ครบ |
| 3 | (1 หรือ 2) | xls + txt | เหมือน 1128 และ zip จริง 260512 — แต่ผู้ใช้เคยตอบ "เฉพาะ txt" |

ตอนนั้นผม (Claude) แนะนำข้อ 1 — ไม่ได้ใช้ เพราะ user ตัดสินให้ไม่มีชั้นนอก (2026-10-05)

</details>

### 3.5 โค้ดตอนนี้ — ตรงกับที่ตัดสินแล้ว ไม่ต้องแก้เรื่องชั้นนอก
- `EmailCls.prepareCase14TxtZip(...)` — case "14" ใน `prepareFileInfoEmail` (มีเช็คจำนวนโฟลเดอร์) และ `prepareFileInfoEmailSelection` (ข้ามเช็คจำนวนโฟลเดอร์)
- สร้างที่ `emailProp.pathShipping` (= `...\yyMMdd-Shipping Data\`) → `yyMMdd-MOLEX(48Dual)-n\{order folder}\{txt}` + `-n.zip`
- zip สร้างจาก list ไฟล์ (ไม่ zip ทั้งโฟลเดอร์), อ่าน zip กลับมาเช็คจำนวนโฟลเดอร์ order + txt ต้องครบ
- subject ตอนมีหลายเมลต่อท้าย `(1/2)` **ไม่มีเว้นวรรค** เหมือน format 3 — user สั่งแก้ 2026-10-05 (`EmailCls.cs` บรรทัด `string subject = groups.Count > 1 ? ...` ใน `prepareCase14TxtZip`)
  - format 13 (COHERENT/INFINERA, `prepareCase13TxtEdata`) ยังเป็น ` (1/2)` มีเว้นวรรค — user ไม่ได้สั่งแก้
- exe ใน `bin\Debug` build 2026-10-05 (หลังแก้ subject) = เวอร์ชันนี้แล้ว

**จุดที่ format 14 ยังต่างจาก format 3 ของ 1480 (`prepareCase3PrepareZip`) — ยังไม่ได้ถาม user ว่าต้องให้เหมือนไหม:**

| เรื่อง | format 14 (1480Dual) ตอนนี้ | format 3 (1480 / 980Dual) |
|---|---|---|
| มี `-n` / `-n.zip` อยู่แล้ว | เขียนทับ (`FileMode.Create`, `File.Copy(..., true)`) ไฟล์เก่าใน `-n` ไม่ถูกลบ | เตือน "พบข้อมูลที่เตรียมไว้แล้ว" แล้วหยุด ไม่เขียนทับ |
| body | subject 7 + body 8 ที่ user เลือก 2026-09-29 (`FolderName` = ชื่อ zip) | body เดิมของ format 3 = รายชื่อโฟลเดอร์ order ตัด `yymmdd-` |

---

## 4. สถานะ DB

### qa_oc_test (config เทส — ใช้อยู่ใน App.config)
- `info_db_name` 11 = `1480dual_test_tanawut`, `info_path_shipping` 11 = `\\office_SRV\C06_Shipping-Data\Test\1480DUAL`
- `info_file_formats` 13 (TXT_Edata), 14 (TXT_Zip)
- key `*_1480dual` (17 แถว):

| KEY | Spec | send | format | group | subject | body |
|---|---|---|---|---|---|---|
| 7 | MOLEX Y-3ER4002C | 3 | 14 | 1000 | 7 | 8 |
| 8 | COHERENT Y-3ER4003 | 3 | 13 | 0 | 7 | 8 |
| 901 | COHERENT Y-3ER4003A (ตัวแทน) | 3 | 13 | 1000 | 7 | 8 |
| 902 | INFINERA Y-3ER2002A (ตัวแทน) | 3 | 13 | 1000 | 7 | 8 |
| อื่นๆ (1-6, 9-15) | | 0 | 0 | 0 | 0 | 0 |

- group 1000 = TO tanawut.patrawan, CC prapa.himploy, CC savita.churasri (ข้อยกเว้นกฎ "qa_oc_test ต้อง group 0" — ผู้ใช้ขอ)

### 1480dual_test_tanawut (product DB เทสของเราเอง — ผู้ใช้สร้าง DB, ผม copy ตาราง)
- 3 ตาราง copy จาก `1480dual` (โครงสร้าง + ข้อมูล), **ไม่มี trigger** โดยตั้งใจ — `1480dual.shipping` มี `tg_ship_copy` → `CALL MES_Shipping(serial)` ส่งเข้า MES ห้าม copy มา
- spec ตัวแทน 901 (COHERENT Y-3ER4003A), 902 (INFINERA Y-3ER2002A) Part_No ชั่วคราว `FRL1441D5050-878-1427-1457`, Note = 'TEST 1012 placeholder...'
- ห้ามใช้ `1480dual_test` (เป็นของทีมอื่น, ลบข้อมูลเทสเราออกแล้ว 2026-09-29)

### qa_oc (production) — รัน `EdataManagement\DatabaseScripts\1480dual_prepare_qa_oc.sql` แล้ว 2026-09-29
- สร้าง `*_1480dual` 5 ตาราง (ว่าง), `info_file_formats` 13, `info_db_name` 11 = `1480dual`, `info_path_shipping` 11 = `\\office_SRV\C06_Shipping-Data\1480Dual`
- ไม่กระทบ 1012 v3.6.0 / EdataWeb (โหลดแค่ product 1,2,4,9 + INNER JOIN info_path_po_all)
- **ยังไม่มี** `info_file_formats` 14, ยังไม่ได้ตั้งค่า spec ใดๆ

---

## 5. ข้อมูลเทส (`\\office_SRV\C06_Shipping-Data\Test\1480DUAL` + `1480dual_test_tanawut`)

สคริปต์สร้างใหม่ได้ (รันซ้ำได้ ลบแถวของตัวเองก่อน): `EdataManagement\DatabaseScripts\test_data\make_1480dual_format13_test.ps1`, `make_1480dual_format14_test.ps1` (ต้องใส่ `-SqlOut <ไฟล์.sql>` แล้วเอา SQL ไปรันเอง)

| วันที่ | ลูกค้า | เคส | ผลที่ควรได้ |
|---|---|---|---|
| 260915 | MOLEX | 1 order 10PCS | 1 zip / 1 เมล |
| 260916 | MOLEX | 10+10+3 PCS | 2 zip (-1: 1 order, -2: 2 order) |
| 260917 | MOLEX | ไฟล์เขียน 9PCS, DB 10 | ไม่พบไฟล์ดาต้า |
| 260918 | MOLEX | 2 โฟลเดอร์, DB 1 order | จำนวนเทสรีพอตมากกว่าดาต้าเบส |
| 260919 | MOLEX | ไม่มีโฟลเดอร์ order | ไม่พบเทสรีพอตหมายเลข TST14-1901 |
| 260921 | COHERENT / INFINERA | ปกติ | 1 เมลต่อลูกค้า |
| 260922 | COHERENT / INFINERA | เกิน 10 MB | 3 เมล (14/14/2) / 2 เมล (14/2) |
| 260923 | COHERENT | ไม่มี _Edata | ไม่พบเทสรีพอตหมายเลข |
| 260924 | COHERENT | DB 3 / txt 2 | จำนวนไฟล์ txt ไม่ตรง |
| 260925 | COHERENT | ไม่มีโฟลเดอร์ order | ไม่พบเทสรีพอตหมายเลข |
| 260926 | INFINERA | 300 ไฟล์เล็ก | 2 เมล (256/44) |

ล้างข้อมูลเทสใน DB:
```sql
DELETE FROM `1480dual_test_tanawut`.shipping WHERE Note IN ('TEST 1012 format13', 'TEST 1012 format14');
DELETE FROM `1480dual_test_tanawut`.db_check_sheet WHERE TestReportNo LIKE 'TST13-%' OR TestReportNo LIKE 'TST14-%';
```

---

## 6. เรื่องอื่นที่ค้าง

1. **Spec จริง Y-3ER4003A / Y-3ER2002A** ยังไม่มีใน `1480dual.info_cus_condition` — รอวิศวกร (+ Part_No จริง, ชื่อ Customer ของ INFINERA)
   - พอมีแล้ว: ลบ 901/902 ออกจาก qa_oc_test + `1480dual_test_tanawut`, ตั้งค่า KEY จริง (INFINERA จะได้ 0 จาก sync ต้องตั้งเอง)
2. **MOLEX KEY 10** (Y-3ER4002C ซ้ำ KEY 7) Part_No มีขีดเกินท้าย `FRL1441D4045-878-1427-1457-` → ชื่อโฟลเดอร์จะหาไม่เจอ ให้วิศวกรตรวจ
3. **ชั้นปีใน path**: รูปเขียน `1480Dual\yyyy\yymmdd-Shipping Data` แต่ share จริงไม่มีชั้นปี (`2026` ว่าง) — ถ้าจะใช้ชั้นปีต้องแก้ format 13 + 14
4. **ขึ้น production (qa_oc)** — ยังเหลือ:
   - `info_file_formats` 14
   - ตั้งค่า spec จริง (หา KEY จาก `Specification` ไม่ใช่เลขตายตัว) + กลุ่มเมลจริงของลูกค้า (ห้าม group 0)
   - ลงโปรแกรมใหม่: โค้ดยังไม่ได้ commit, `App.config` ต้องกลับเป็น `qa_oc` (ตอนนี้ `qa_oc_test`) — พอเป็น qa_oc เมลจะถึงลูกค้าจริง
   - (ถ้าต้องการ) audit trigger ใน qa_oc
5. **MailSafety** (DB ไม่ใช่ qa_oc): เมลเป็น Draft ไม่มีผู้รับ ยกเว้นผู้รับ**ทุกคน**อยู่ใน group ของ `App.config` `TestMailGroupId` (=1000) และเป็น @furukawaelectric.com → ส่งจริง เนื้อเมลเหมือนของจริงทุกอย่าง, SFTP ข้ามเสมอ
6. **ชื่อลูกค้า MOLEX(OPLINK)** (2026-10-15, พักไว้ตามที่ผู้ใช้สั่ง "เอาไว้ก่อน แต่จดไว้") — User เรียก 1480Dual MOLEX (Y-3ER4002C) ว่า "MOLEX(OPLINK)" แต่ใน DB เป็น `MOLEX`
   - ห้ามเปลี่ยน `info_cus_condition.Customer` เพราะชื่อนี้ใช้ทำชื่อโฟลเดอร์ `yyMMdd-MOLEX(48Dual)` / `-n` / zip / เนื้อเมล / query
   - ข้อเสนอ: Hardcode เฉพาะชื่อที่แสดง (1480Dual, MOLEX) → "MOLEX(OPLINK)" ใน combo ลูกค้าของ frmMain (`frmMain.cs:206-208`) และ Spec Setting, ValueMember ยังเป็น `MOLEX`; ถ้ามีหลายเคสค่อยย้ายเป็นตารางใน config DB
   - เวอร์ชันก่อนไม่เคยมี alias (combo = ชื่อจาก DB ตรง ๆ); `qa_oc.info_customer` (CUSTOMER_NAME → IN_EMAIL_NAME) เป็นตารางเก่า 1012 ไม่ได้ใช้
