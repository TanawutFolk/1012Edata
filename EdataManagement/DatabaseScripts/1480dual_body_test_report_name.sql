-- ============================================================
-- 1480dual_body_test_report_name : body 12 for the 1480Dual formats 13 / 14
--
-- Customer request 2026-11-17 (COHERENT / INFINERA): the txt names are serials, so the mail must say which
-- test report it carries. Body 8 ("We attached file(s) of electric data in this e-mail.") has no FolderName
-- and is used by 154 specs of 1480, so it is left alone. Body 12 = body 8 + a blank line + FolderName:
--   format 13 (prepareCase13TxtEdata): FolderName = the test report folder  yyMMdd-{FEC order}(full Part No)-{sub}
--   format 14 (prepareCase14TxtZip)  : the same (one test report a mail since 2026-11-18)
-- The blank line (<BR><BR>, like bodies 4 and 6) was asked for on 2026-11-23; until then it was a single <BR>.
-- Every 1480Dual spec whose file format is 13 or 14 gets body 12. Re-runnable: run it again after a
-- spec is set to format 13 / 14 (the Email Body list of the Spec page is locked).
--
-- Database: qa_oc_test, and qa_oc when 1480Dual goes live (App.config -> ConnectionQAOC)
--   mysql ... qa_oc_test < 1480dual_body_test_report_name.sql
-- Id 12 is free on qa_oc (2026-11-17: qa_oc max 10); qa_oc_test already has it (updated to <BR><BR> 2026-11-23).
-- ============================================================

-- the audit triggers of email_body_1480dual fill UPDATE_BY / log_setting_edata from it
SET @app_user = 'tanawut.pat';

-- a database that ran the first version (single <BR>) gets the blank line
UPDATE info_email_body
SET BODY_NAME = 'We attached file(s) of electric data in this e-mail.<BR><BR>FolderName'
WHERE BODY_ID = 12
  AND BODY_NAME = 'We attached file(s) of electric data in this e-mail.<BR>FolderName';

INSERT INTO info_email_body (BODY_ID, BODY_NAME, CREATE_USER, CREATE_DATE, INUSE)
SELECT 12, 'We attached file(s) of electric data in this e-mail.<BR><BR>FolderName', 'tanawut.pat', NOW(), 1
FROM DUAL
WHERE NOT EXISTS (SELECT 1 FROM info_email_body WHERE BODY_ID = 12);

-- only when id 12 really is this body (another text there = the id was taken: nothing is changed)
INSERT INTO email_body_1480dual (KEY_ID, BODY_ID)
SELECT a.KEY_ID, 12
FROM email_attach_1480dual a
WHERE a.FILE_FORMATS_ID IN (13, 14)
  AND EXISTS (SELECT 1 FROM info_email_body
              WHERE BODY_ID = 12 AND BODY_NAME = 'We attached file(s) of electric data in this e-mail.<BR><BR>FolderName')
ON DUPLICATE KEY UPDATE BODY_ID = VALUES(BODY_ID);

SET @app_user = NULL;

-- check: body 12 is the text above, and every format 13 / 14 spec shows BODY_ID 12
SELECT BODY_ID, BODY_NAME, INUSE FROM info_email_body WHERE BODY_ID = 12;
SELECT a.KEY_ID, a.FILE_FORMATS_ID, b.BODY_ID
FROM email_attach_1480dual a
LEFT JOIN email_body_1480dual b ON (b.KEY_ID = a.KEY_ID)
WHERE a.FILE_FORMATS_ID IN (13, 14)
ORDER BY a.KEY_ID;
