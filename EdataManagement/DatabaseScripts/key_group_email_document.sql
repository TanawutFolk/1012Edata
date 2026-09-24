-- ============================================================
-- key_group_email_document : recipient group per product + customer + E-data type
--
-- LD data (info_document 1 = Edata) keeps its group per spec in key_group_email_<product>.
-- The other E-data types go out as separate mails with their own recipients:
--   2 = Serial List   (Hisilicon 1480 / 1480VE / 980Dual)
--   3 = Wafer         (Hisilicon 980Dual)
-- Before this table those recipients lived in Setting\EmailSetting.ini next to the exe
-- ([1480HisiliconSERIAL_LIST] to= / cc=), so every PC had its own copy and the
-- Setting screen could not show them. EmailCls now reads this table first and only
-- falls back to the ini when no row exists.
--
-- Members of GROUP_ID are in info_group_email, like every other group.
-- Database: config database (qa_oc_test_tanawut while testing)
-- ============================================================
CREATE TABLE IF NOT EXISTS key_group_email_document (
    PRODUCT_NAME VARCHAR(20)  COLLATE utf8_unicode_ci NOT NULL,                     -- info_product.PRODUCT_NAME: 1480 / 1480VE / 980 / 980Dual
    -- utf8_general_ci like info_cus_condition.Customer, otherwise a JOIN fails with ERROR 1267
    CUSTOMER     VARCHAR(100) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,  -- = info_cus_condition.Customer
    DOCUMENT_ID  INT          NOT NULL,                                            -- info_document.DOCUMENT_ID
    GROUP_ID     INT          NOT NULL,                                            -- info_group_email.GROUP_ID
    UPDATE_USER  VARCHAR(20)  COLLATE utf8_unicode_ci NULL,
    UPDATE_DATE  DATETIME     NULL,
    PRIMARY KEY (PRODUCT_NAME, CUSTOMER, DOCUMENT_ID),
    KEY idx_key_group_email_document_group (GROUP_ID)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;

-- recipients of one type:
--   SELECT ge.RECIPIENT_ID, e.EMAIL_ADDRESS
--   FROM key_group_email_document d
--   JOIN info_group_email ge ON ge.GROUP_ID = d.GROUP_ID
--   JOIN info_email e        ON e.EMAIL_ID  = ge.EMAIL_ID
--   WHERE d.PRODUCT_NAME = '1480' AND d.CUSTOMER = 'HISILICON' AND d.DOCUMENT_ID = 2;
