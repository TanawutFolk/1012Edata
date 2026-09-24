-- ============================================================
-- Move the Serial List / Wafer recipients of Setting\EmailSetting.ini into the database.
-- Source: EdataManagement\bin\Debug\Setting\EmailSetting.ini (the file used for real sending,
--         last changed 2026-05-27, 51 recipients) - checked equal to qa_oc_test_tanawut on 2026-09-24.
--
--   [1480HisiliconSERIAL_LIST]     -> 1480    / HISILICON / 2 Serial List
--   [980DualHisiliconSERIAL_LIST]  -> 980Dual / HISILICON / 2 Serial List
--   [980DualHisiliconWAFER]        -> 980Dual / HISILICON / 3 Wafer
--   [1480VEHisiliconSERIAL_LIST]   -> 1480VE  / HISILICON / 2 Serial List
--   [980Hisilicon*]                -> skipped, empty in the ini
--
-- Each section gets a NEW group = MAX(info_group_email.GROUP_ID) + 1 at run time, so it cannot
-- collide with groups of 1012 / EdataWeb / 1111 (uITLA) that share info_group_email.
-- An address missing from info_email is added (CREATE_USER 'ini-import'); existing ones are reused.
--
-- Run ONCE against the config database, after key_group_email_document.sql:
--   mysql ... qa_oc < key_group_email_document.sql
--   mysql ... qa_oc < key_group_email_document_import_20260924.sql
-- A second run stops on the primary key of key_group_email_document and nothing is committed.
-- ============================================================
START TRANSACTION;
-- [1480HisiliconSERIAL_LIST]
SET @g = (SELECT IFNULL(MAX(GROUP_ID), 0) + 1 FROM info_group_email);
INSERT INTO info_email (EMAIL_ID, EMAIL_ADDRESS, CREATE_USER, CREATE_DATE, INUSE) SELECT (SELECT MAX(EMAIL_ID) + 1 FROM info_email), 'ftsz.fitel_e-data_fehk@furukawaelectric.com', 'ini-import', NOW(), 1 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM info_email WHERE EMAIL_ADDRESS = 'ftsz.fitel_e-data_fehk@furukawaelectric.com');
INSERT INTO info_group_email (GROUP_ID, RECIPIENT_ID, EMAIL_ID) SELECT @g, 1, MIN(EMAIL_ID) FROM info_email WHERE EMAIL_ADDRESS = 'ftsz.fitel_e-data_fehk@furukawaelectric.com';
INSERT INTO info_email (EMAIL_ID, EMAIL_ADDRESS, CREATE_USER, CREATE_DATE, INUSE) SELECT (SELECT MAX(EMAIL_ID) + 1 FROM info_email), 'fec.fitelodqa.edt@furukawaelectric.com', 'ini-import', NOW(), 1 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM info_email WHERE EMAIL_ADDRESS = 'fec.fitelodqa.edt@furukawaelectric.com');
INSERT INTO info_group_email (GROUP_ID, RECIPIENT_ID, EMAIL_ID) SELECT @g, 1, MIN(EMAIL_ID) FROM info_email WHERE EMAIL_ADDRESS = 'fec.fitelodqa.edt@furukawaelectric.com';
INSERT INTO info_email (EMAIL_ID, EMAIL_ADDRESS, CREATE_USER, CREATE_DATE, INUSE) SELECT (SELECT MAX(EMAIL_ID) + 1 FROM info_email), 'chris.pang@nipponexpress.com', 'ini-import', NOW(), 1 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM info_email WHERE EMAIL_ADDRESS = 'chris.pang@nipponexpress.com');
INSERT INTO info_group_email (GROUP_ID, RECIPIENT_ID, EMAIL_ID) SELECT @g, 1, MIN(EMAIL_ID) FROM info_email WHERE EMAIL_ADDRESS = 'chris.pang@nipponexpress.com';
INSERT INTO info_email (EMAIL_ID, EMAIL_ADDRESS, CREATE_USER, CREATE_DATE, INUSE) SELECT (SELECT MAX(EMAIL_ID) + 1 FROM info_email), 'jack.pang@nipponexpress.com', 'ini-import', NOW(), 1 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM info_email WHERE EMAIL_ADDRESS = 'jack.pang@nipponexpress.com');
INSERT INTO info_group_email (GROUP_ID, RECIPIENT_ID, EMAIL_ID) SELECT @g, 1, MIN(EMAIL_ID) FROM info_email WHERE EMAIL_ADDRESS = 'jack.pang@nipponexpress.com';
INSERT INTO info_email (EMAIL_ID, EMAIL_ADDRESS, CREATE_USER, CREATE_DATE, INUSE) SELECT (SELECT MAX(EMAIL_ID) + 1 FROM info_email), 'david.tsoi@nipponexpress.com', 'ini-import', NOW(), 1 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM info_email WHERE EMAIL_ADDRESS = 'david.tsoi@nipponexpress.com');
INSERT INTO info_group_email (GROUP_ID, RECIPIENT_ID, EMAIL_ID) SELECT @g, 1, MIN(EMAIL_ID) FROM info_email WHERE EMAIL_ADDRESS = 'david.tsoi@nipponexpress.com';
INSERT INTO info_email (EMAIL_ID, EMAIL_ADDRESS, CREATE_USER, CREATE_DATE, INUSE) SELECT (SELECT MAX(EMAIL_ID) + 1 FROM info_email), 'cherry.to@nipponexpress.com', 'ini-import', NOW(), 1 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM info_email WHERE EMAIL_ADDRESS = 'cherry.to@nipponexpress.com');
INSERT INTO info_group_email (GROUP_ID, RECIPIENT_ID, EMAIL_ID) SELECT @g, 1, MIN(EMAIL_ID) FROM info_email WHERE EMAIL_ADDRESS = 'cherry.to@nipponexpress.com';
INSERT INTO info_email (EMAIL_ID, EMAIL_ADDRESS, CREATE_USER, CREATE_DATE, INUSE) SELECT (SELECT MAX(EMAIL_ID) + 1 FROM info_email), 'tatsuya.muro@furukawaelectric.com', 'ini-import', NOW(), 1 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM info_email WHERE EMAIL_ADDRESS = 'tatsuya.muro@furukawaelectric.com');
INSERT INTO info_group_email (GROUP_ID, RECIPIENT_ID, EMAIL_ID) SELECT @g, 2, MIN(EMAIL_ID) FROM info_email WHERE EMAIL_ADDRESS = 'tatsuya.muro@furukawaelectric.com';
INSERT INTO info_email (EMAIL_ID, EMAIL_ADDRESS, CREATE_USER, CREATE_DATE, INUSE) SELECT (SELECT MAX(EMAIL_ID) + 1 FROM info_email), 'siriporn.sinlapanon@furukawaelectric.com', 'ini-import', NOW(), 1 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM info_email WHERE EMAIL_ADDRESS = 'siriporn.sinlapanon@furukawaelectric.com');
INSERT INTO info_group_email (GROUP_ID, RECIPIENT_ID, EMAIL_ID) SELECT @g, 2, MIN(EMAIL_ID) FROM info_email WHERE EMAIL_ADDRESS = 'siriporn.sinlapanon@furukawaelectric.com';
INSERT INTO info_email (EMAIL_ID, EMAIL_ADDRESS, CREATE_USER, CREATE_DATE, INUSE) SELECT (SELECT MAX(EMAIL_ID) + 1 FROM info_email), 'savita.churasri@furukawaelectric.com', 'ini-import', NOW(), 1 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM info_email WHERE EMAIL_ADDRESS = 'savita.churasri@furukawaelectric.com');
INSERT INTO info_group_email (GROUP_ID, RECIPIENT_ID, EMAIL_ID) SELECT @g, 2, MIN(EMAIL_ID) FROM info_email WHERE EMAIL_ADDRESS = 'savita.churasri@furukawaelectric.com';
INSERT INTO info_email (EMAIL_ID, EMAIL_ADDRESS, CREATE_USER, CREATE_DATE, INUSE) SELECT (SELECT MAX(EMAIL_ID) + 1 FROM info_email), 'doungdeun.thippawan@furukawaelectric.com', 'ini-import', NOW(), 1 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM info_email WHERE EMAIL_ADDRESS = 'doungdeun.thippawan@furukawaelectric.com');
INSERT INTO info_group_email (GROUP_ID, RECIPIENT_ID, EMAIL_ID) SELECT @g, 2, MIN(EMAIL_ID) FROM info_email WHERE EMAIL_ADDRESS = 'doungdeun.thippawan@furukawaelectric.com';
INSERT INTO info_email (EMAIL_ID, EMAIL_ADDRESS, CREATE_USER, CREATE_DATE, INUSE) SELECT (SELECT MAX(EMAIL_ID) + 1 FROM info_email), 'prapa.himploy@furukawaelectric.com', 'ini-import', NOW(), 1 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM info_email WHERE EMAIL_ADDRESS = 'prapa.himploy@furukawaelectric.com');
INSERT INTO info_group_email (GROUP_ID, RECIPIENT_ID, EMAIL_ID) SELECT @g, 2, MIN(EMAIL_ID) FROM info_email WHERE EMAIL_ADDRESS = 'prapa.himploy@furukawaelectric.com';
INSERT INTO info_email (EMAIL_ID, EMAIL_ADDRESS, CREATE_USER, CREATE_DATE, INUSE) SELECT (SELECT MAX(EMAIL_ID) + 1 FROM info_email), 'winudda.unphun@furukawaelectric.com', 'ini-import', NOW(), 1 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM info_email WHERE EMAIL_ADDRESS = 'winudda.unphun@furukawaelectric.com');
INSERT INTO info_group_email (GROUP_ID, RECIPIENT_ID, EMAIL_ID) SELECT @g, 2, MIN(EMAIL_ID) FROM info_email WHERE EMAIL_ADDRESS = 'winudda.unphun@furukawaelectric.com';
INSERT INTO info_email (EMAIL_ID, EMAIL_ADDRESS, CREATE_USER, CREATE_DATE, INUSE) SELECT (SELECT MAX(EMAIL_ID) + 1 FROM info_email), 'sasiwimon.paenpho@furukawaelectric.com', 'ini-import', NOW(), 1 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM info_email WHERE EMAIL_ADDRESS = 'sasiwimon.paenpho@furukawaelectric.com');
INSERT INTO info_group_email (GROUP_ID, RECIPIENT_ID, EMAIL_ID) SELECT @g, 2, MIN(EMAIL_ID) FROM info_email WHERE EMAIL_ADDRESS = 'sasiwimon.paenpho@furukawaelectric.com';
INSERT INTO key_group_email_document (PRODUCT_NAME, CUSTOMER, DOCUMENT_ID, GROUP_ID, UPDATE_USER, UPDATE_DATE) VALUES ('1480', 'HISILICON', 2, @g, 'ini-import', NOW());
INSERT INTO log_setting_edata (PRODUCT, TABLE_NAME, KEY_ID, CUSTOMER, SPEC, OLD_VALUE, NEW_VALUE, CHANGE_USER, CHANGE_DATE) VALUES ('1480', 'key_group_email_document', 2, 'HISILICON', 'from EmailSetting.ini', NULL, @g, 'ini-import', NOW());
-- [980DualHisiliconSERIAL_LIST]
SET @g = (SELECT IFNULL(MAX(GROUP_ID), 0) + 1 FROM info_group_email);
INSERT INTO info_email (EMAIL_ID, EMAIL_ADDRESS, CREATE_USER, CREATE_DATE, INUSE) SELECT (SELECT MAX(EMAIL_ID) + 1 FROM info_email), 'fec.fitelodqa.edt@furukawaelectric.com', 'ini-import', NOW(), 1 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM info_email WHERE EMAIL_ADDRESS = 'fec.fitelodqa.edt@furukawaelectric.com');
INSERT INTO info_group_email (GROUP_ID, RECIPIENT_ID, EMAIL_ID) SELECT @g, 1, MIN(EMAIL_ID) FROM info_email WHERE EMAIL_ADDRESS = 'fec.fitelodqa.edt@furukawaelectric.com';
INSERT INTO info_email (EMAIL_ID, EMAIL_ADDRESS, CREATE_USER, CREATE_DATE, INUSE) SELECT (SELECT MAX(EMAIL_ID) + 1 FROM info_email), 'ftsz.fitel_e-data_fehk@furukawaelectric.com', 'ini-import', NOW(), 1 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM info_email WHERE EMAIL_ADDRESS = 'ftsz.fitel_e-data_fehk@furukawaelectric.com');
INSERT INTO info_group_email (GROUP_ID, RECIPIENT_ID, EMAIL_ID) SELECT @g, 1, MIN(EMAIL_ID) FROM info_email WHERE EMAIL_ADDRESS = 'ftsz.fitel_e-data_fehk@furukawaelectric.com';
INSERT INTO info_email (EMAIL_ID, EMAIL_ADDRESS, CREATE_USER, CREATE_DATE, INUSE) SELECT (SELECT MAX(EMAIL_ID) + 1 FROM info_email), 'chris.pang@nipponexpress.com', 'ini-import', NOW(), 1 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM info_email WHERE EMAIL_ADDRESS = 'chris.pang@nipponexpress.com');
INSERT INTO info_group_email (GROUP_ID, RECIPIENT_ID, EMAIL_ID) SELECT @g, 1, MIN(EMAIL_ID) FROM info_email WHERE EMAIL_ADDRESS = 'chris.pang@nipponexpress.com';
INSERT INTO info_email (EMAIL_ID, EMAIL_ADDRESS, CREATE_USER, CREATE_DATE, INUSE) SELECT (SELECT MAX(EMAIL_ID) + 1 FROM info_email), 'jack.pang@nipponexpress.com', 'ini-import', NOW(), 1 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM info_email WHERE EMAIL_ADDRESS = 'jack.pang@nipponexpress.com');
INSERT INTO info_group_email (GROUP_ID, RECIPIENT_ID, EMAIL_ID) SELECT @g, 1, MIN(EMAIL_ID) FROM info_email WHERE EMAIL_ADDRESS = 'jack.pang@nipponexpress.com';
INSERT INTO info_email (EMAIL_ID, EMAIL_ADDRESS, CREATE_USER, CREATE_DATE, INUSE) SELECT (SELECT MAX(EMAIL_ID) + 1 FROM info_email), 'david.tsoi@nipponexpress.com', 'ini-import', NOW(), 1 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM info_email WHERE EMAIL_ADDRESS = 'david.tsoi@nipponexpress.com');
INSERT INTO info_group_email (GROUP_ID, RECIPIENT_ID, EMAIL_ID) SELECT @g, 1, MIN(EMAIL_ID) FROM info_email WHERE EMAIL_ADDRESS = 'david.tsoi@nipponexpress.com';
INSERT INTO info_email (EMAIL_ID, EMAIL_ADDRESS, CREATE_USER, CREATE_DATE, INUSE) SELECT (SELECT MAX(EMAIL_ID) + 1 FROM info_email), 'cherry.to@nipponexpress.com', 'ini-import', NOW(), 1 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM info_email WHERE EMAIL_ADDRESS = 'cherry.to@nipponexpress.com');
INSERT INTO info_group_email (GROUP_ID, RECIPIENT_ID, EMAIL_ID) SELECT @g, 1, MIN(EMAIL_ID) FROM info_email WHERE EMAIL_ADDRESS = 'cherry.to@nipponexpress.com';
INSERT INTO info_email (EMAIL_ID, EMAIL_ADDRESS, CREATE_USER, CREATE_DATE, INUSE) SELECT (SELECT MAX(EMAIL_ID) + 1 FROM info_email), 'tatsuya.muro@furukawaelectric.com', 'ini-import', NOW(), 1 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM info_email WHERE EMAIL_ADDRESS = 'tatsuya.muro@furukawaelectric.com');
INSERT INTO info_group_email (GROUP_ID, RECIPIENT_ID, EMAIL_ID) SELECT @g, 2, MIN(EMAIL_ID) FROM info_email WHERE EMAIL_ADDRESS = 'tatsuya.muro@furukawaelectric.com';
INSERT INTO info_email (EMAIL_ID, EMAIL_ADDRESS, CREATE_USER, CREATE_DATE, INUSE) SELECT (SELECT MAX(EMAIL_ID) + 1 FROM info_email), 'siriporn.sinlapanon@furukawaelectric.com', 'ini-import', NOW(), 1 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM info_email WHERE EMAIL_ADDRESS = 'siriporn.sinlapanon@furukawaelectric.com');
INSERT INTO info_group_email (GROUP_ID, RECIPIENT_ID, EMAIL_ID) SELECT @g, 2, MIN(EMAIL_ID) FROM info_email WHERE EMAIL_ADDRESS = 'siriporn.sinlapanon@furukawaelectric.com';
INSERT INTO info_email (EMAIL_ID, EMAIL_ADDRESS, CREATE_USER, CREATE_DATE, INUSE) SELECT (SELECT MAX(EMAIL_ID) + 1 FROM info_email), 'savita.churasri@furukawaelectric.com', 'ini-import', NOW(), 1 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM info_email WHERE EMAIL_ADDRESS = 'savita.churasri@furukawaelectric.com');
INSERT INTO info_group_email (GROUP_ID, RECIPIENT_ID, EMAIL_ID) SELECT @g, 2, MIN(EMAIL_ID) FROM info_email WHERE EMAIL_ADDRESS = 'savita.churasri@furukawaelectric.com';
INSERT INTO info_email (EMAIL_ID, EMAIL_ADDRESS, CREATE_USER, CREATE_DATE, INUSE) SELECT (SELECT MAX(EMAIL_ID) + 1 FROM info_email), 'doungdeun.thippawan@furukawaelectric.com', 'ini-import', NOW(), 1 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM info_email WHERE EMAIL_ADDRESS = 'doungdeun.thippawan@furukawaelectric.com');
INSERT INTO info_group_email (GROUP_ID, RECIPIENT_ID, EMAIL_ID) SELECT @g, 2, MIN(EMAIL_ID) FROM info_email WHERE EMAIL_ADDRESS = 'doungdeun.thippawan@furukawaelectric.com';
INSERT INTO info_email (EMAIL_ID, EMAIL_ADDRESS, CREATE_USER, CREATE_DATE, INUSE) SELECT (SELECT MAX(EMAIL_ID) + 1 FROM info_email), 'prapa.himploy@furukawaelectric.com', 'ini-import', NOW(), 1 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM info_email WHERE EMAIL_ADDRESS = 'prapa.himploy@furukawaelectric.com');
INSERT INTO info_group_email (GROUP_ID, RECIPIENT_ID, EMAIL_ID) SELECT @g, 2, MIN(EMAIL_ID) FROM info_email WHERE EMAIL_ADDRESS = 'prapa.himploy@furukawaelectric.com';
INSERT INTO info_email (EMAIL_ID, EMAIL_ADDRESS, CREATE_USER, CREATE_DATE, INUSE) SELECT (SELECT MAX(EMAIL_ID) + 1 FROM info_email), 'warisa.sansup@furukawaelectric.com', 'ini-import', NOW(), 1 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM info_email WHERE EMAIL_ADDRESS = 'warisa.sansup@furukawaelectric.com');
INSERT INTO info_group_email (GROUP_ID, RECIPIENT_ID, EMAIL_ID) SELECT @g, 2, MIN(EMAIL_ID) FROM info_email WHERE EMAIL_ADDRESS = 'warisa.sansup@furukawaelectric.com';
INSERT INTO info_email (EMAIL_ID, EMAIL_ADDRESS, CREATE_USER, CREATE_DATE, INUSE) SELECT (SELECT MAX(EMAIL_ID) + 1 FROM info_email), 'supunnee.run@furukawaelectric.com', 'ini-import', NOW(), 1 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM info_email WHERE EMAIL_ADDRESS = 'supunnee.run@furukawaelectric.com');
INSERT INTO info_group_email (GROUP_ID, RECIPIENT_ID, EMAIL_ID) SELECT @g, 2, MIN(EMAIL_ID) FROM info_email WHERE EMAIL_ADDRESS = 'supunnee.run@furukawaelectric.com';
INSERT INTO info_email (EMAIL_ID, EMAIL_ADDRESS, CREATE_USER, CREATE_DATE, INUSE) SELECT (SELECT MAX(EMAIL_ID) + 1 FROM info_email), 'sasiwimon.paenpho@furukawaelectric.com', 'ini-import', NOW(), 1 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM info_email WHERE EMAIL_ADDRESS = 'sasiwimon.paenpho@furukawaelectric.com');
INSERT INTO info_group_email (GROUP_ID, RECIPIENT_ID, EMAIL_ID) SELECT @g, 2, MIN(EMAIL_ID) FROM info_email WHERE EMAIL_ADDRESS = 'sasiwimon.paenpho@furukawaelectric.com';
INSERT INTO key_group_email_document (PRODUCT_NAME, CUSTOMER, DOCUMENT_ID, GROUP_ID, UPDATE_USER, UPDATE_DATE) VALUES ('980Dual', 'HISILICON', 2, @g, 'ini-import', NOW());
INSERT INTO log_setting_edata (PRODUCT, TABLE_NAME, KEY_ID, CUSTOMER, SPEC, OLD_VALUE, NEW_VALUE, CHANGE_USER, CHANGE_DATE) VALUES ('980Dual', 'key_group_email_document', 2, 'HISILICON', 'from EmailSetting.ini', NULL, @g, 'ini-import', NOW());
-- [980DualHisiliconWAFER]
SET @g = (SELECT IFNULL(MAX(GROUP_ID), 0) + 1 FROM info_group_email);
INSERT INTO info_email (EMAIL_ID, EMAIL_ADDRESS, CREATE_USER, CREATE_DATE, INUSE) SELECT (SELECT MAX(EMAIL_ID) + 1 FROM info_email), 'fec.fitelodqa.edt@furukawaelectric.com', 'ini-import', NOW(), 1 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM info_email WHERE EMAIL_ADDRESS = 'fec.fitelodqa.edt@furukawaelectric.com');
INSERT INTO info_group_email (GROUP_ID, RECIPIENT_ID, EMAIL_ID) SELECT @g, 1, MIN(EMAIL_ID) FROM info_email WHERE EMAIL_ADDRESS = 'fec.fitelodqa.edt@furukawaelectric.com';
INSERT INTO info_email (EMAIL_ID, EMAIL_ADDRESS, CREATE_USER, CREATE_DATE, INUSE) SELECT (SELECT MAX(EMAIL_ID) + 1 FROM info_email), 'ftsz.fitel_e-data_fehk@furukawaelectric.com', 'ini-import', NOW(), 1 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM info_email WHERE EMAIL_ADDRESS = 'ftsz.fitel_e-data_fehk@furukawaelectric.com');
INSERT INTO info_group_email (GROUP_ID, RECIPIENT_ID, EMAIL_ID) SELECT @g, 1, MIN(EMAIL_ID) FROM info_email WHERE EMAIL_ADDRESS = 'ftsz.fitel_e-data_fehk@furukawaelectric.com';
INSERT INTO info_email (EMAIL_ID, EMAIL_ADDRESS, CREATE_USER, CREATE_DATE, INUSE) SELECT (SELECT MAX(EMAIL_ID) + 1 FROM info_email), 'qinyuejing@huawei.com', 'ini-import', NOW(), 1 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM info_email WHERE EMAIL_ADDRESS = 'qinyuejing@huawei.com');
INSERT INTO info_group_email (GROUP_ID, RECIPIENT_ID, EMAIL_ID) SELECT @g, 1, MIN(EMAIL_ID) FROM info_email WHERE EMAIL_ADDRESS = 'qinyuejing@huawei.com';
INSERT INTO info_email (EMAIL_ID, EMAIL_ADDRESS, CREATE_USER, CREATE_DATE, INUSE) SELECT (SELECT MAX(EMAIL_ID) + 1 FROM info_email), 'tatsuya.muro@furukawaelectric.com', 'ini-import', NOW(), 1 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM info_email WHERE EMAIL_ADDRESS = 'tatsuya.muro@furukawaelectric.com');
INSERT INTO info_group_email (GROUP_ID, RECIPIENT_ID, EMAIL_ID) SELECT @g, 2, MIN(EMAIL_ID) FROM info_email WHERE EMAIL_ADDRESS = 'tatsuya.muro@furukawaelectric.com';
INSERT INTO info_email (EMAIL_ID, EMAIL_ADDRESS, CREATE_USER, CREATE_DATE, INUSE) SELECT (SELECT MAX(EMAIL_ID) + 1 FROM info_email), 'siriporn.sinlapanon@furukawaelectric.com', 'ini-import', NOW(), 1 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM info_email WHERE EMAIL_ADDRESS = 'siriporn.sinlapanon@furukawaelectric.com');
INSERT INTO info_group_email (GROUP_ID, RECIPIENT_ID, EMAIL_ID) SELECT @g, 2, MIN(EMAIL_ID) FROM info_email WHERE EMAIL_ADDRESS = 'siriporn.sinlapanon@furukawaelectric.com';
INSERT INTO info_email (EMAIL_ID, EMAIL_ADDRESS, CREATE_USER, CREATE_DATE, INUSE) SELECT (SELECT MAX(EMAIL_ID) + 1 FROM info_email), 'savita.churasri@furukawaelectric.com', 'ini-import', NOW(), 1 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM info_email WHERE EMAIL_ADDRESS = 'savita.churasri@furukawaelectric.com');
INSERT INTO info_group_email (GROUP_ID, RECIPIENT_ID, EMAIL_ID) SELECT @g, 2, MIN(EMAIL_ID) FROM info_email WHERE EMAIL_ADDRESS = 'savita.churasri@furukawaelectric.com';
INSERT INTO info_email (EMAIL_ID, EMAIL_ADDRESS, CREATE_USER, CREATE_DATE, INUSE) SELECT (SELECT MAX(EMAIL_ID) + 1 FROM info_email), 'doungdeun.thippawan@furukawaelectric.com', 'ini-import', NOW(), 1 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM info_email WHERE EMAIL_ADDRESS = 'doungdeun.thippawan@furukawaelectric.com');
INSERT INTO info_group_email (GROUP_ID, RECIPIENT_ID, EMAIL_ID) SELECT @g, 2, MIN(EMAIL_ID) FROM info_email WHERE EMAIL_ADDRESS = 'doungdeun.thippawan@furukawaelectric.com';
INSERT INTO info_email (EMAIL_ID, EMAIL_ADDRESS, CREATE_USER, CREATE_DATE, INUSE) SELECT (SELECT MAX(EMAIL_ID) + 1 FROM info_email), 'prapa.himploy@furukawaelectric.com', 'ini-import', NOW(), 1 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM info_email WHERE EMAIL_ADDRESS = 'prapa.himploy@furukawaelectric.com');
INSERT INTO info_group_email (GROUP_ID, RECIPIENT_ID, EMAIL_ID) SELECT @g, 2, MIN(EMAIL_ID) FROM info_email WHERE EMAIL_ADDRESS = 'prapa.himploy@furukawaelectric.com';
INSERT INTO info_email (EMAIL_ID, EMAIL_ADDRESS, CREATE_USER, CREATE_DATE, INUSE) SELECT (SELECT MAX(EMAIL_ID) + 1 FROM info_email), 'warisa.sansup@furukawaelectric.com', 'ini-import', NOW(), 1 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM info_email WHERE EMAIL_ADDRESS = 'warisa.sansup@furukawaelectric.com');
INSERT INTO info_group_email (GROUP_ID, RECIPIENT_ID, EMAIL_ID) SELECT @g, 2, MIN(EMAIL_ID) FROM info_email WHERE EMAIL_ADDRESS = 'warisa.sansup@furukawaelectric.com';
INSERT INTO info_email (EMAIL_ID, EMAIL_ADDRESS, CREATE_USER, CREATE_DATE, INUSE) SELECT (SELECT MAX(EMAIL_ID) + 1 FROM info_email), 'supunnee.run@furukawaelectric.com', 'ini-import', NOW(), 1 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM info_email WHERE EMAIL_ADDRESS = 'supunnee.run@furukawaelectric.com');
INSERT INTO info_group_email (GROUP_ID, RECIPIENT_ID, EMAIL_ID) SELECT @g, 2, MIN(EMAIL_ID) FROM info_email WHERE EMAIL_ADDRESS = 'supunnee.run@furukawaelectric.com';
INSERT INTO info_email (EMAIL_ID, EMAIL_ADDRESS, CREATE_USER, CREATE_DATE, INUSE) SELECT (SELECT MAX(EMAIL_ID) + 1 FROM info_email), 'sasiwimon.paenpho@furukawaelectric.com', 'ini-import', NOW(), 1 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM info_email WHERE EMAIL_ADDRESS = 'sasiwimon.paenpho@furukawaelectric.com');
INSERT INTO info_group_email (GROUP_ID, RECIPIENT_ID, EMAIL_ID) SELECT @g, 2, MIN(EMAIL_ID) FROM info_email WHERE EMAIL_ADDRESS = 'sasiwimon.paenpho@furukawaelectric.com';
INSERT INTO key_group_email_document (PRODUCT_NAME, CUSTOMER, DOCUMENT_ID, GROUP_ID, UPDATE_USER, UPDATE_DATE) VALUES ('980Dual', 'HISILICON', 3, @g, 'ini-import', NOW());
INSERT INTO log_setting_edata (PRODUCT, TABLE_NAME, KEY_ID, CUSTOMER, SPEC, OLD_VALUE, NEW_VALUE, CHANGE_USER, CHANGE_DATE) VALUES ('980Dual', 'key_group_email_document', 3, 'HISILICON', 'from EmailSetting.ini', NULL, @g, 'ini-import', NOW());
-- [1480VEHisiliconSERIAL_LIST]
SET @g = (SELECT IFNULL(MAX(GROUP_ID), 0) + 1 FROM info_group_email);
INSERT INTO info_email (EMAIL_ID, EMAIL_ADDRESS, CREATE_USER, CREATE_DATE, INUSE) SELECT (SELECT MAX(EMAIL_ID) + 1 FROM info_email), 'ftsz.fitel_e-data_fehk@furukawaelectric.com', 'ini-import', NOW(), 1 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM info_email WHERE EMAIL_ADDRESS = 'ftsz.fitel_e-data_fehk@furukawaelectric.com');
INSERT INTO info_group_email (GROUP_ID, RECIPIENT_ID, EMAIL_ID) SELECT @g, 1, MIN(EMAIL_ID) FROM info_email WHERE EMAIL_ADDRESS = 'ftsz.fitel_e-data_fehk@furukawaelectric.com';
INSERT INTO info_email (EMAIL_ID, EMAIL_ADDRESS, CREATE_USER, CREATE_DATE, INUSE) SELECT (SELECT MAX(EMAIL_ID) + 1 FROM info_email), 'fec.fitelodqa.edt@furukawaelectric.com', 'ini-import', NOW(), 1 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM info_email WHERE EMAIL_ADDRESS = 'fec.fitelodqa.edt@furukawaelectric.com');
INSERT INTO info_group_email (GROUP_ID, RECIPIENT_ID, EMAIL_ID) SELECT @g, 1, MIN(EMAIL_ID) FROM info_email WHERE EMAIL_ADDRESS = 'fec.fitelodqa.edt@furukawaelectric.com';
INSERT INTO info_email (EMAIL_ID, EMAIL_ADDRESS, CREATE_USER, CREATE_DATE, INUSE) SELECT (SELECT MAX(EMAIL_ID) + 1 FROM info_email), 'chris.pang@nipponexpress.com', 'ini-import', NOW(), 1 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM info_email WHERE EMAIL_ADDRESS = 'chris.pang@nipponexpress.com');
INSERT INTO info_group_email (GROUP_ID, RECIPIENT_ID, EMAIL_ID) SELECT @g, 1, MIN(EMAIL_ID) FROM info_email WHERE EMAIL_ADDRESS = 'chris.pang@nipponexpress.com';
INSERT INTO info_email (EMAIL_ID, EMAIL_ADDRESS, CREATE_USER, CREATE_DATE, INUSE) SELECT (SELECT MAX(EMAIL_ID) + 1 FROM info_email), 'jack.pang@nipponexpress.com', 'ini-import', NOW(), 1 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM info_email WHERE EMAIL_ADDRESS = 'jack.pang@nipponexpress.com');
INSERT INTO info_group_email (GROUP_ID, RECIPIENT_ID, EMAIL_ID) SELECT @g, 1, MIN(EMAIL_ID) FROM info_email WHERE EMAIL_ADDRESS = 'jack.pang@nipponexpress.com';
INSERT INTO info_email (EMAIL_ID, EMAIL_ADDRESS, CREATE_USER, CREATE_DATE, INUSE) SELECT (SELECT MAX(EMAIL_ID) + 1 FROM info_email), 'david.tsoi@nipponexpress.com', 'ini-import', NOW(), 1 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM info_email WHERE EMAIL_ADDRESS = 'david.tsoi@nipponexpress.com');
INSERT INTO info_group_email (GROUP_ID, RECIPIENT_ID, EMAIL_ID) SELECT @g, 1, MIN(EMAIL_ID) FROM info_email WHERE EMAIL_ADDRESS = 'david.tsoi@nipponexpress.com';
INSERT INTO info_email (EMAIL_ID, EMAIL_ADDRESS, CREATE_USER, CREATE_DATE, INUSE) SELECT (SELECT MAX(EMAIL_ID) + 1 FROM info_email), 'cherry.to@nipponexpress.com', 'ini-import', NOW(), 1 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM info_email WHERE EMAIL_ADDRESS = 'cherry.to@nipponexpress.com');
INSERT INTO info_group_email (GROUP_ID, RECIPIENT_ID, EMAIL_ID) SELECT @g, 1, MIN(EMAIL_ID) FROM info_email WHERE EMAIL_ADDRESS = 'cherry.to@nipponexpress.com';
INSERT INTO info_email (EMAIL_ID, EMAIL_ADDRESS, CREATE_USER, CREATE_DATE, INUSE) SELECT (SELECT MAX(EMAIL_ID) + 1 FROM info_email), 'tatsuya.muro@furukawaelectric.com', 'ini-import', NOW(), 1 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM info_email WHERE EMAIL_ADDRESS = 'tatsuya.muro@furukawaelectric.com');
INSERT INTO info_group_email (GROUP_ID, RECIPIENT_ID, EMAIL_ID) SELECT @g, 2, MIN(EMAIL_ID) FROM info_email WHERE EMAIL_ADDRESS = 'tatsuya.muro@furukawaelectric.com';
INSERT INTO info_email (EMAIL_ID, EMAIL_ADDRESS, CREATE_USER, CREATE_DATE, INUSE) SELECT (SELECT MAX(EMAIL_ID) + 1 FROM info_email), 'siriporn.sinlapanon@furukawaelectric.com', 'ini-import', NOW(), 1 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM info_email WHERE EMAIL_ADDRESS = 'siriporn.sinlapanon@furukawaelectric.com');
INSERT INTO info_group_email (GROUP_ID, RECIPIENT_ID, EMAIL_ID) SELECT @g, 2, MIN(EMAIL_ID) FROM info_email WHERE EMAIL_ADDRESS = 'siriporn.sinlapanon@furukawaelectric.com';
INSERT INTO info_email (EMAIL_ID, EMAIL_ADDRESS, CREATE_USER, CREATE_DATE, INUSE) SELECT (SELECT MAX(EMAIL_ID) + 1 FROM info_email), 'savita.churasri@furukawaelectric.com', 'ini-import', NOW(), 1 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM info_email WHERE EMAIL_ADDRESS = 'savita.churasri@furukawaelectric.com');
INSERT INTO info_group_email (GROUP_ID, RECIPIENT_ID, EMAIL_ID) SELECT @g, 2, MIN(EMAIL_ID) FROM info_email WHERE EMAIL_ADDRESS = 'savita.churasri@furukawaelectric.com';
INSERT INTO info_email (EMAIL_ID, EMAIL_ADDRESS, CREATE_USER, CREATE_DATE, INUSE) SELECT (SELECT MAX(EMAIL_ID) + 1 FROM info_email), 'doungdeun.thippawan@furukawaelectric.com', 'ini-import', NOW(), 1 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM info_email WHERE EMAIL_ADDRESS = 'doungdeun.thippawan@furukawaelectric.com');
INSERT INTO info_group_email (GROUP_ID, RECIPIENT_ID, EMAIL_ID) SELECT @g, 2, MIN(EMAIL_ID) FROM info_email WHERE EMAIL_ADDRESS = 'doungdeun.thippawan@furukawaelectric.com';
INSERT INTO info_email (EMAIL_ID, EMAIL_ADDRESS, CREATE_USER, CREATE_DATE, INUSE) SELECT (SELECT MAX(EMAIL_ID) + 1 FROM info_email), 'prapa.himploy@furukawaelectric.com', 'ini-import', NOW(), 1 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM info_email WHERE EMAIL_ADDRESS = 'prapa.himploy@furukawaelectric.com');
INSERT INTO info_group_email (GROUP_ID, RECIPIENT_ID, EMAIL_ID) SELECT @g, 2, MIN(EMAIL_ID) FROM info_email WHERE EMAIL_ADDRESS = 'prapa.himploy@furukawaelectric.com';
INSERT INTO info_email (EMAIL_ID, EMAIL_ADDRESS, CREATE_USER, CREATE_DATE, INUSE) SELECT (SELECT MAX(EMAIL_ID) + 1 FROM info_email), 'winudda.unphun@furukawaelectric.com', 'ini-import', NOW(), 1 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM info_email WHERE EMAIL_ADDRESS = 'winudda.unphun@furukawaelectric.com');
INSERT INTO info_group_email (GROUP_ID, RECIPIENT_ID, EMAIL_ID) SELECT @g, 2, MIN(EMAIL_ID) FROM info_email WHERE EMAIL_ADDRESS = 'winudda.unphun@furukawaelectric.com';
INSERT INTO info_email (EMAIL_ID, EMAIL_ADDRESS, CREATE_USER, CREATE_DATE, INUSE) SELECT (SELECT MAX(EMAIL_ID) + 1 FROM info_email), 'sasiwimon.paenpho@furukawaelectric.com', 'ini-import', NOW(), 1 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM info_email WHERE EMAIL_ADDRESS = 'sasiwimon.paenpho@furukawaelectric.com');
INSERT INTO info_group_email (GROUP_ID, RECIPIENT_ID, EMAIL_ID) SELECT @g, 2, MIN(EMAIL_ID) FROM info_email WHERE EMAIL_ADDRESS = 'sasiwimon.paenpho@furukawaelectric.com';
INSERT INTO key_group_email_document (PRODUCT_NAME, CUSTOMER, DOCUMENT_ID, GROUP_ID, UPDATE_USER, UPDATE_DATE) VALUES ('1480VE', 'HISILICON', 2, @g, 'ini-import', NOW());
INSERT INTO log_setting_edata (PRODUCT, TABLE_NAME, KEY_ID, CUSTOMER, SPEC, OLD_VALUE, NEW_VALUE, CHANGE_USER, CHANGE_DATE) VALUES ('1480VE', 'key_group_email_document', 2, 'HISILICON', 'from EmailSetting.ini', NULL, @g, 'ini-import', NOW());
COMMIT;
