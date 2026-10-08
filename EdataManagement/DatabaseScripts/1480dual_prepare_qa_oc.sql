-- ============================================================
-- 1480dual_prepare_qa_oc : get qa_oc ready for 1480Dual (PRODUCT_ID 11, file format 13 = txt per serial)
--   mysql ... qa_oc < 1480dual_prepare_qa_oc.sql
--
-- Can run before the new 1012 is deployed - nothing that runs today reads what this adds:
--   * 1012 v3.6.0 and EdataWeb load products 1,2,4,9 only (and INNER JOIN info_path_po_all, which 11 has no row in)
--   * neither of them reads info_file_formats
--   * the *_1480dual tables are new, nobody references them
-- No spec is configured here. When the new 1012 starts, its sync fills the *_1480dual tables with 0 for every
-- 1480Dual spec (0 = Don't Send), so nothing is mailed until a spec is set on purpose
-- (Y-3ER4003A / Y-3ER2002A: after the engineer creates them in 1480dual.info_cus_condition).
--
-- Safe to run again: tables only created when missing, rows only inserted when the key is free.
-- ============================================================

-- 1. per-spec key tables, same structure as 980Dual in qa_oc
CREATE TABLE IF NOT EXISTS key_send_methods_1480dual LIKE key_send_methods_980dual;
CREATE TABLE IF NOT EXISTS email_attach_1480dual     LIKE email_attach_980dual;
CREATE TABLE IF NOT EXISTS key_group_email_1480dual  LIKE key_group_email_980dual;
CREATE TABLE IF NOT EXISTS email_subject_1480dual    LIKE email_subject_980dual;
CREATE TABLE IF NOT EXISTS email_body_1480dual       LIKE email_body_980dual;

-- 2. file format 13 (EmailCls.prepareCase13TxtEdata)
INSERT IGNORE INTO info_file_formats (ID, FILE_FORMATS, DETAILS, SHOW_NAME) VALUES
(13, 'TXT per serial (_Edata)',
 'yymmdd-[FEC Order No](full Part No)[-sub]\\...(full Part No)_Edata\\*.txt, txt count = serials in shipping, all orders in one mail; over 10 MB or 256 files -> copied to yymmdd-Customer(48Dual)_Edata-n folders, one mail each',
 'TXT_Edata');

-- 3. product DB + shipping share (no info_path_po_all: format 13 does not read Shipping-PO-all)
INSERT IGNORE INTO info_db_name (PRODUCT_ID, DATABASE_NAME) VALUES (11, '1480dual');
INSERT IGNORE INTO info_path_shipping (PRODUCT_ID, PATH_SHIPPING) VALUES (11, '\\\\office_SRV\\C06_Shipping-Data\\1480Dual');

-- check: every line must show the 1480Dual value (an old value here means the key was already taken - look before going on)
SELECT 'info_db_name' AS item, DATABASE_NAME AS value FROM info_db_name WHERE PRODUCT_ID = 11
UNION ALL SELECT 'info_path_shipping', PATH_SHIPPING FROM info_path_shipping WHERE PRODUCT_ID = 11
UNION ALL SELECT 'info_file_formats 13', SHOW_NAME FROM info_file_formats WHERE ID = 13
UNION ALL SELECT 'key tables', CAST(COUNT(*) AS CHAR) FROM information_schema.TABLES
          WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME IN
          ('key_send_methods_1480dual','email_attach_1480dual','key_group_email_1480dual','email_subject_1480dual','email_body_1480dual');
