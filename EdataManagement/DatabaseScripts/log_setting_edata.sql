-- ============================================================
-- log_setting_edata : who changed which key setting, and when
--
-- Written by the Send Method tab of frmSetting in the same
-- transaction as the change itself, one row per changed value:
--   key_send_methods_<product>  SEND_METHODS_ID
--   email_attach_<product>      FILE_FORMATS_ID
--   key_group_email_<product>   GROUP_ID
--   email_subject_<product>     SUBJECT_ID
--   email_body_<product>        BODY_ID
--
-- Background: 2026-09 a Fabrinet spec (KEY_ID 2755) was given MPB's
-- email setting and nobody could tell who or when - the key tables
-- only hold KEY_ID + value.
--
-- Database: qa_oc_test  (App.config -> ConnectionQAOC)
-- Server  : 192.168.2.99
-- ============================================================
CREATE TABLE IF NOT EXISTS log_setting_edata (
    ID          INT          NOT NULL AUTO_INCREMENT,
    PRODUCT     VARCHAR(20)  COLLATE utf8_unicode_ci NOT NULL,   -- 1480 / 980 / 980Dual / 1480VE
    TABLE_NAME  VARCHAR(64)  COLLATE utf8_unicode_ci NOT NULL,   -- key_send_methods_1480 ...
    KEY_ID      INT          NOT NULL,                           -- = info_cus_condition.CusCondition_ID
    CUSTOMER    VARCHAR(100) COLLATE utf8_unicode_ci NULL,
    SPEC        VARCHAR(50)  COLLATE utf8_unicode_ci NULL,
    OLD_VALUE   INT          NULL,
    NEW_VALUE   INT          NULL,
    CHANGE_USER VARCHAR(20)  COLLATE utf8_unicode_ci NOT NULL,   -- EMPCODE of the signed-in user
    CHANGE_DATE DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (ID),
    KEY idx_log_setting_edata_key (PRODUCT, KEY_ID),
    KEY idx_log_setting_edata_date (CHANGE_DATE)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;

-- who touched one spec:
--   SELECT * FROM log_setting_edata WHERE PRODUCT = '1480' AND KEY_ID = 2755 ORDER BY CHANGE_DATE DESC;
