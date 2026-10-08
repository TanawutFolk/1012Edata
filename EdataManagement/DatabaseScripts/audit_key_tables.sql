-- ============================================================
-- audit_key_tables : audit columns + audit trail for the per-spec key tables
--   key_send_methods_<p>, email_attach_<p>, key_group_email_<p>, email_subject_<p>, email_body_<p>
--   for <p> = 1480, 1480ve, 980, 980dual, 1480dual (uitla is left out: 1111 uses it, source not checked yet)
--
-- Every table gets CREATE_BY / CREATE_DATE / UPDATE_BY / UPDATE_DATE, filled by triggers only,
-- so no program has to know these columns exist:
--   BEFORE INSERT  -> CREATE_BY, CREATE_DATE
--   BEFORE UPDATE  -> UPDATE_BY, UPDATE_DATE   (only when the value really changes)
--   AFTER UPDATE   -> one row in log_setting_edata with old / new value   (SOURCE = 'trigger')
--   AFTER DELETE   -> one row in log_setting_edata                         (SOURCE = 'trigger')
-- Who: session variable @app_user (1012 runs SET @app_user = '<EMPCODE>' / 'auto-sync' before writing),
--      otherwise 'ip:<client address>' - an edit made in Navicat or plain SQL still shows which PC did it.
-- Rows that existed before this script keep NULL in the CREATE_* columns (nobody knows them).
--
-- Safe to run again: columns are only added when missing, triggers are dropped and re-created.
-- Run against ONE config database, e.g.  mysql ... qa_oc_test < audit_key_tables.sql
-- ============================================================

DROP PROCEDURE IF EXISTS tmp_add_column;
DELIMITER $$
CREATE PROCEDURE tmp_add_column(IN p_table VARCHAR(64), IN p_column VARCHAR(64), IN p_definition VARCHAR(255))
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.COLUMNS
                   WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = p_table AND COLUMN_NAME = p_column) THEN
        SET @ddl = CONCAT('ALTER TABLE `', p_table, '` ADD COLUMN `', p_column, '` ', p_definition);
        PREPARE st FROM @ddl; EXECUTE st; DEALLOCATE PREPARE st;
    END IF;
END$$
DELIMITER ;

-- log table: which PC / database account, and whether the row came from a trigger or from a program
CALL tmp_add_column('log_setting_edata', 'CHANGE_HOST', 'VARCHAR(80) COLLATE utf8_unicode_ci NULL AFTER CHANGE_USER');
CALL tmp_add_column('log_setting_edata', 'SOURCE', 'VARCHAR(10) COLLATE utf8_unicode_ci NOT NULL DEFAULT ''app'' AFTER CHANGE_HOST');

-- ---------------- key_send_methods_1480 (SEND_METHODS_ID) ----------------
CALL tmp_add_column('key_send_methods_1480', 'CREATE_BY',   'VARCHAR(20) COLLATE utf8_unicode_ci NULL');
CALL tmp_add_column('key_send_methods_1480', 'CREATE_DATE', 'DATETIME NULL');
CALL tmp_add_column('key_send_methods_1480', 'UPDATE_BY',   'VARCHAR(20) COLLATE utf8_unicode_ci NULL');
CALL tmp_add_column('key_send_methods_1480', 'UPDATE_DATE', 'DATETIME NULL');
DROP TRIGGER IF EXISTS trg_key_send_methods_1480_bi;
DROP TRIGGER IF EXISTS trg_key_send_methods_1480_bu;
DROP TRIGGER IF EXISTS trg_key_send_methods_1480_au;
DROP TRIGGER IF EXISTS trg_key_send_methods_1480_ad;
DELIMITER $$
CREATE TRIGGER trg_key_send_methods_1480_bi BEFORE INSERT ON key_send_methods_1480 FOR EACH ROW
BEGIN
    SET NEW.CREATE_BY = IFNULL(NEW.CREATE_BY, LEFT(IFNULL(@app_user, CONCAT('ip:', SUBSTRING_INDEX(USER(), '@', -1))), 20));
    SET NEW.CREATE_DATE = IFNULL(NEW.CREATE_DATE, NOW());
END$$
CREATE TRIGGER trg_key_send_methods_1480_bu BEFORE UPDATE ON key_send_methods_1480 FOR EACH ROW
BEGIN
    IF NOT (OLD.SEND_METHODS_ID <=> NEW.SEND_METHODS_ID) THEN
        SET NEW.UPDATE_BY = LEFT(IFNULL(@app_user, CONCAT('ip:', SUBSTRING_INDEX(USER(), '@', -1))), 20);
        SET NEW.UPDATE_DATE = NOW();
    END IF;
END$$
CREATE TRIGGER trg_key_send_methods_1480_au AFTER UPDATE ON key_send_methods_1480 FOR EACH ROW
BEGIN
    IF NOT (OLD.SEND_METHODS_ID <=> NEW.SEND_METHODS_ID) THEN
        INSERT INTO log_setting_edata (PRODUCT, TABLE_NAME, KEY_ID, CUSTOMER, SPEC, OLD_VALUE, NEW_VALUE, CHANGE_USER, CHANGE_HOST, SOURCE, CHANGE_DATE)
        SELECT '1480', 'key_send_methods_1480', NEW.KEY_ID, ic.Customer, ic.Specification, OLD.SEND_METHODS_ID, NEW.SEND_METHODS_ID, NEW.UPDATE_BY, USER(), 'trigger', NOW()
        FROM (SELECT 1) one LEFT JOIN `ddb`.info_cus_condition ic ON ic.CusCondition_ID = NEW.KEY_ID;
    END IF;
END$$
CREATE TRIGGER trg_key_send_methods_1480_ad AFTER DELETE ON key_send_methods_1480 FOR EACH ROW
BEGIN
    INSERT INTO log_setting_edata (PRODUCT, TABLE_NAME, KEY_ID, CUSTOMER, SPEC, OLD_VALUE, NEW_VALUE, CHANGE_USER, CHANGE_HOST, SOURCE, CHANGE_DATE)
    SELECT '1480', 'key_send_methods_1480', OLD.KEY_ID, ic.Customer, ic.Specification, OLD.SEND_METHODS_ID, NULL, LEFT(IFNULL(@app_user, CONCAT('ip:', SUBSTRING_INDEX(USER(), '@', -1))), 20), USER(), 'trigger', NOW()
    FROM (SELECT 1) one LEFT JOIN `ddb`.info_cus_condition ic ON ic.CusCondition_ID = OLD.KEY_ID;
END$$
DELIMITER ;

-- ---------------- email_attach_1480 (FILE_FORMATS_ID) ----------------
CALL tmp_add_column('email_attach_1480', 'CREATE_BY',   'VARCHAR(20) COLLATE utf8_unicode_ci NULL');
CALL tmp_add_column('email_attach_1480', 'CREATE_DATE', 'DATETIME NULL');
CALL tmp_add_column('email_attach_1480', 'UPDATE_BY',   'VARCHAR(20) COLLATE utf8_unicode_ci NULL');
CALL tmp_add_column('email_attach_1480', 'UPDATE_DATE', 'DATETIME NULL');
DROP TRIGGER IF EXISTS trg_email_attach_1480_bi;
DROP TRIGGER IF EXISTS trg_email_attach_1480_bu;
DROP TRIGGER IF EXISTS trg_email_attach_1480_au;
DROP TRIGGER IF EXISTS trg_email_attach_1480_ad;
DELIMITER $$
CREATE TRIGGER trg_email_attach_1480_bi BEFORE INSERT ON email_attach_1480 FOR EACH ROW
BEGIN
    SET NEW.CREATE_BY = IFNULL(NEW.CREATE_BY, LEFT(IFNULL(@app_user, CONCAT('ip:', SUBSTRING_INDEX(USER(), '@', -1))), 20));
    SET NEW.CREATE_DATE = IFNULL(NEW.CREATE_DATE, NOW());
END$$
CREATE TRIGGER trg_email_attach_1480_bu BEFORE UPDATE ON email_attach_1480 FOR EACH ROW
BEGIN
    IF NOT (OLD.FILE_FORMATS_ID <=> NEW.FILE_FORMATS_ID) THEN
        SET NEW.UPDATE_BY = LEFT(IFNULL(@app_user, CONCAT('ip:', SUBSTRING_INDEX(USER(), '@', -1))), 20);
        SET NEW.UPDATE_DATE = NOW();
    END IF;
END$$
CREATE TRIGGER trg_email_attach_1480_au AFTER UPDATE ON email_attach_1480 FOR EACH ROW
BEGIN
    IF NOT (OLD.FILE_FORMATS_ID <=> NEW.FILE_FORMATS_ID) THEN
        INSERT INTO log_setting_edata (PRODUCT, TABLE_NAME, KEY_ID, CUSTOMER, SPEC, OLD_VALUE, NEW_VALUE, CHANGE_USER, CHANGE_HOST, SOURCE, CHANGE_DATE)
        SELECT '1480', 'email_attach_1480', NEW.KEY_ID, ic.Customer, ic.Specification, OLD.FILE_FORMATS_ID, NEW.FILE_FORMATS_ID, NEW.UPDATE_BY, USER(), 'trigger', NOW()
        FROM (SELECT 1) one LEFT JOIN `ddb`.info_cus_condition ic ON ic.CusCondition_ID = NEW.KEY_ID;
    END IF;
END$$
CREATE TRIGGER trg_email_attach_1480_ad AFTER DELETE ON email_attach_1480 FOR EACH ROW
BEGIN
    INSERT INTO log_setting_edata (PRODUCT, TABLE_NAME, KEY_ID, CUSTOMER, SPEC, OLD_VALUE, NEW_VALUE, CHANGE_USER, CHANGE_HOST, SOURCE, CHANGE_DATE)
    SELECT '1480', 'email_attach_1480', OLD.KEY_ID, ic.Customer, ic.Specification, OLD.FILE_FORMATS_ID, NULL, LEFT(IFNULL(@app_user, CONCAT('ip:', SUBSTRING_INDEX(USER(), '@', -1))), 20), USER(), 'trigger', NOW()
    FROM (SELECT 1) one LEFT JOIN `ddb`.info_cus_condition ic ON ic.CusCondition_ID = OLD.KEY_ID;
END$$
DELIMITER ;

-- ---------------- key_group_email_1480 (GROUP_ID) ----------------
CALL tmp_add_column('key_group_email_1480', 'CREATE_BY',   'VARCHAR(20) COLLATE utf8_unicode_ci NULL');
CALL tmp_add_column('key_group_email_1480', 'CREATE_DATE', 'DATETIME NULL');
CALL tmp_add_column('key_group_email_1480', 'UPDATE_BY',   'VARCHAR(20) COLLATE utf8_unicode_ci NULL');
CALL tmp_add_column('key_group_email_1480', 'UPDATE_DATE', 'DATETIME NULL');
DROP TRIGGER IF EXISTS trg_key_group_email_1480_bi;
DROP TRIGGER IF EXISTS trg_key_group_email_1480_bu;
DROP TRIGGER IF EXISTS trg_key_group_email_1480_au;
DROP TRIGGER IF EXISTS trg_key_group_email_1480_ad;
DELIMITER $$
CREATE TRIGGER trg_key_group_email_1480_bi BEFORE INSERT ON key_group_email_1480 FOR EACH ROW
BEGIN
    SET NEW.CREATE_BY = IFNULL(NEW.CREATE_BY, LEFT(IFNULL(@app_user, CONCAT('ip:', SUBSTRING_INDEX(USER(), '@', -1))), 20));
    SET NEW.CREATE_DATE = IFNULL(NEW.CREATE_DATE, NOW());
END$$
CREATE TRIGGER trg_key_group_email_1480_bu BEFORE UPDATE ON key_group_email_1480 FOR EACH ROW
BEGIN
    IF NOT (OLD.GROUP_ID <=> NEW.GROUP_ID) THEN
        SET NEW.UPDATE_BY = LEFT(IFNULL(@app_user, CONCAT('ip:', SUBSTRING_INDEX(USER(), '@', -1))), 20);
        SET NEW.UPDATE_DATE = NOW();
    END IF;
END$$
CREATE TRIGGER trg_key_group_email_1480_au AFTER UPDATE ON key_group_email_1480 FOR EACH ROW
BEGIN
    IF NOT (OLD.GROUP_ID <=> NEW.GROUP_ID) THEN
        INSERT INTO log_setting_edata (PRODUCT, TABLE_NAME, KEY_ID, CUSTOMER, SPEC, OLD_VALUE, NEW_VALUE, CHANGE_USER, CHANGE_HOST, SOURCE, CHANGE_DATE)
        SELECT '1480', 'key_group_email_1480', NEW.KEY_ID, ic.Customer, ic.Specification, OLD.GROUP_ID, NEW.GROUP_ID, NEW.UPDATE_BY, USER(), 'trigger', NOW()
        FROM (SELECT 1) one LEFT JOIN `ddb`.info_cus_condition ic ON ic.CusCondition_ID = NEW.KEY_ID;
    END IF;
END$$
CREATE TRIGGER trg_key_group_email_1480_ad AFTER DELETE ON key_group_email_1480 FOR EACH ROW
BEGIN
    INSERT INTO log_setting_edata (PRODUCT, TABLE_NAME, KEY_ID, CUSTOMER, SPEC, OLD_VALUE, NEW_VALUE, CHANGE_USER, CHANGE_HOST, SOURCE, CHANGE_DATE)
    SELECT '1480', 'key_group_email_1480', OLD.KEY_ID, ic.Customer, ic.Specification, OLD.GROUP_ID, NULL, LEFT(IFNULL(@app_user, CONCAT('ip:', SUBSTRING_INDEX(USER(), '@', -1))), 20), USER(), 'trigger', NOW()
    FROM (SELECT 1) one LEFT JOIN `ddb`.info_cus_condition ic ON ic.CusCondition_ID = OLD.KEY_ID;
END$$
DELIMITER ;

-- ---------------- email_subject_1480 (SUBJECT_ID) ----------------
CALL tmp_add_column('email_subject_1480', 'CREATE_BY',   'VARCHAR(20) COLLATE utf8_unicode_ci NULL');
CALL tmp_add_column('email_subject_1480', 'CREATE_DATE', 'DATETIME NULL');
CALL tmp_add_column('email_subject_1480', 'UPDATE_BY',   'VARCHAR(20) COLLATE utf8_unicode_ci NULL');
CALL tmp_add_column('email_subject_1480', 'UPDATE_DATE', 'DATETIME NULL');
DROP TRIGGER IF EXISTS trg_email_subject_1480_bi;
DROP TRIGGER IF EXISTS trg_email_subject_1480_bu;
DROP TRIGGER IF EXISTS trg_email_subject_1480_au;
DROP TRIGGER IF EXISTS trg_email_subject_1480_ad;
DELIMITER $$
CREATE TRIGGER trg_email_subject_1480_bi BEFORE INSERT ON email_subject_1480 FOR EACH ROW
BEGIN
    SET NEW.CREATE_BY = IFNULL(NEW.CREATE_BY, LEFT(IFNULL(@app_user, CONCAT('ip:', SUBSTRING_INDEX(USER(), '@', -1))), 20));
    SET NEW.CREATE_DATE = IFNULL(NEW.CREATE_DATE, NOW());
END$$
CREATE TRIGGER trg_email_subject_1480_bu BEFORE UPDATE ON email_subject_1480 FOR EACH ROW
BEGIN
    IF NOT (OLD.SUBJECT_ID <=> NEW.SUBJECT_ID) THEN
        SET NEW.UPDATE_BY = LEFT(IFNULL(@app_user, CONCAT('ip:', SUBSTRING_INDEX(USER(), '@', -1))), 20);
        SET NEW.UPDATE_DATE = NOW();
    END IF;
END$$
CREATE TRIGGER trg_email_subject_1480_au AFTER UPDATE ON email_subject_1480 FOR EACH ROW
BEGIN
    IF NOT (OLD.SUBJECT_ID <=> NEW.SUBJECT_ID) THEN
        INSERT INTO log_setting_edata (PRODUCT, TABLE_NAME, KEY_ID, CUSTOMER, SPEC, OLD_VALUE, NEW_VALUE, CHANGE_USER, CHANGE_HOST, SOURCE, CHANGE_DATE)
        SELECT '1480', 'email_subject_1480', NEW.KEY_ID, ic.Customer, ic.Specification, OLD.SUBJECT_ID, NEW.SUBJECT_ID, NEW.UPDATE_BY, USER(), 'trigger', NOW()
        FROM (SELECT 1) one LEFT JOIN `ddb`.info_cus_condition ic ON ic.CusCondition_ID = NEW.KEY_ID;
    END IF;
END$$
CREATE TRIGGER trg_email_subject_1480_ad AFTER DELETE ON email_subject_1480 FOR EACH ROW
BEGIN
    INSERT INTO log_setting_edata (PRODUCT, TABLE_NAME, KEY_ID, CUSTOMER, SPEC, OLD_VALUE, NEW_VALUE, CHANGE_USER, CHANGE_HOST, SOURCE, CHANGE_DATE)
    SELECT '1480', 'email_subject_1480', OLD.KEY_ID, ic.Customer, ic.Specification, OLD.SUBJECT_ID, NULL, LEFT(IFNULL(@app_user, CONCAT('ip:', SUBSTRING_INDEX(USER(), '@', -1))), 20), USER(), 'trigger', NOW()
    FROM (SELECT 1) one LEFT JOIN `ddb`.info_cus_condition ic ON ic.CusCondition_ID = OLD.KEY_ID;
END$$
DELIMITER ;

-- ---------------- email_body_1480 (BODY_ID) ----------------
CALL tmp_add_column('email_body_1480', 'CREATE_BY',   'VARCHAR(20) COLLATE utf8_unicode_ci NULL');
CALL tmp_add_column('email_body_1480', 'CREATE_DATE', 'DATETIME NULL');
CALL tmp_add_column('email_body_1480', 'UPDATE_BY',   'VARCHAR(20) COLLATE utf8_unicode_ci NULL');
CALL tmp_add_column('email_body_1480', 'UPDATE_DATE', 'DATETIME NULL');
DROP TRIGGER IF EXISTS trg_email_body_1480_bi;
DROP TRIGGER IF EXISTS trg_email_body_1480_bu;
DROP TRIGGER IF EXISTS trg_email_body_1480_au;
DROP TRIGGER IF EXISTS trg_email_body_1480_ad;
DELIMITER $$
CREATE TRIGGER trg_email_body_1480_bi BEFORE INSERT ON email_body_1480 FOR EACH ROW
BEGIN
    SET NEW.CREATE_BY = IFNULL(NEW.CREATE_BY, LEFT(IFNULL(@app_user, CONCAT('ip:', SUBSTRING_INDEX(USER(), '@', -1))), 20));
    SET NEW.CREATE_DATE = IFNULL(NEW.CREATE_DATE, NOW());
END$$
CREATE TRIGGER trg_email_body_1480_bu BEFORE UPDATE ON email_body_1480 FOR EACH ROW
BEGIN
    IF NOT (OLD.BODY_ID <=> NEW.BODY_ID) THEN
        SET NEW.UPDATE_BY = LEFT(IFNULL(@app_user, CONCAT('ip:', SUBSTRING_INDEX(USER(), '@', -1))), 20);
        SET NEW.UPDATE_DATE = NOW();
    END IF;
END$$
CREATE TRIGGER trg_email_body_1480_au AFTER UPDATE ON email_body_1480 FOR EACH ROW
BEGIN
    IF NOT (OLD.BODY_ID <=> NEW.BODY_ID) THEN
        INSERT INTO log_setting_edata (PRODUCT, TABLE_NAME, KEY_ID, CUSTOMER, SPEC, OLD_VALUE, NEW_VALUE, CHANGE_USER, CHANGE_HOST, SOURCE, CHANGE_DATE)
        SELECT '1480', 'email_body_1480', NEW.KEY_ID, ic.Customer, ic.Specification, OLD.BODY_ID, NEW.BODY_ID, NEW.UPDATE_BY, USER(), 'trigger', NOW()
        FROM (SELECT 1) one LEFT JOIN `ddb`.info_cus_condition ic ON ic.CusCondition_ID = NEW.KEY_ID;
    END IF;
END$$
CREATE TRIGGER trg_email_body_1480_ad AFTER DELETE ON email_body_1480 FOR EACH ROW
BEGIN
    INSERT INTO log_setting_edata (PRODUCT, TABLE_NAME, KEY_ID, CUSTOMER, SPEC, OLD_VALUE, NEW_VALUE, CHANGE_USER, CHANGE_HOST, SOURCE, CHANGE_DATE)
    SELECT '1480', 'email_body_1480', OLD.KEY_ID, ic.Customer, ic.Specification, OLD.BODY_ID, NULL, LEFT(IFNULL(@app_user, CONCAT('ip:', SUBSTRING_INDEX(USER(), '@', -1))), 20), USER(), 'trigger', NOW()
    FROM (SELECT 1) one LEFT JOIN `ddb`.info_cus_condition ic ON ic.CusCondition_ID = OLD.KEY_ID;
END$$
DELIMITER ;

-- ---------------- key_send_methods_1480ve (SEND_METHODS_ID) ----------------
CALL tmp_add_column('key_send_methods_1480ve', 'CREATE_BY',   'VARCHAR(20) COLLATE utf8_unicode_ci NULL');
CALL tmp_add_column('key_send_methods_1480ve', 'CREATE_DATE', 'DATETIME NULL');
CALL tmp_add_column('key_send_methods_1480ve', 'UPDATE_BY',   'VARCHAR(20) COLLATE utf8_unicode_ci NULL');
CALL tmp_add_column('key_send_methods_1480ve', 'UPDATE_DATE', 'DATETIME NULL');
DROP TRIGGER IF EXISTS trg_key_send_methods_1480ve_bi;
DROP TRIGGER IF EXISTS trg_key_send_methods_1480ve_bu;
DROP TRIGGER IF EXISTS trg_key_send_methods_1480ve_au;
DROP TRIGGER IF EXISTS trg_key_send_methods_1480ve_ad;
DELIMITER $$
CREATE TRIGGER trg_key_send_methods_1480ve_bi BEFORE INSERT ON key_send_methods_1480ve FOR EACH ROW
BEGIN
    SET NEW.CREATE_BY = IFNULL(NEW.CREATE_BY, LEFT(IFNULL(@app_user, CONCAT('ip:', SUBSTRING_INDEX(USER(), '@', -1))), 20));
    SET NEW.CREATE_DATE = IFNULL(NEW.CREATE_DATE, NOW());
END$$
CREATE TRIGGER trg_key_send_methods_1480ve_bu BEFORE UPDATE ON key_send_methods_1480ve FOR EACH ROW
BEGIN
    IF NOT (OLD.SEND_METHODS_ID <=> NEW.SEND_METHODS_ID) THEN
        SET NEW.UPDATE_BY = LEFT(IFNULL(@app_user, CONCAT('ip:', SUBSTRING_INDEX(USER(), '@', -1))), 20);
        SET NEW.UPDATE_DATE = NOW();
    END IF;
END$$
CREATE TRIGGER trg_key_send_methods_1480ve_au AFTER UPDATE ON key_send_methods_1480ve FOR EACH ROW
BEGIN
    IF NOT (OLD.SEND_METHODS_ID <=> NEW.SEND_METHODS_ID) THEN
        INSERT INTO log_setting_edata (PRODUCT, TABLE_NAME, KEY_ID, CUSTOMER, SPEC, OLD_VALUE, NEW_VALUE, CHANGE_USER, CHANGE_HOST, SOURCE, CHANGE_DATE)
        SELECT '1480VE', 'key_send_methods_1480ve', NEW.KEY_ID, ic.Customer, ic.Specification, OLD.SEND_METHODS_ID, NEW.SEND_METHODS_ID, NEW.UPDATE_BY, USER(), 'trigger', NOW()
        FROM (SELECT 1) one LEFT JOIN `1480ve`.info_cus_condition ic ON ic.CusCondition_ID = NEW.KEY_ID;
    END IF;
END$$
CREATE TRIGGER trg_key_send_methods_1480ve_ad AFTER DELETE ON key_send_methods_1480ve FOR EACH ROW
BEGIN
    INSERT INTO log_setting_edata (PRODUCT, TABLE_NAME, KEY_ID, CUSTOMER, SPEC, OLD_VALUE, NEW_VALUE, CHANGE_USER, CHANGE_HOST, SOURCE, CHANGE_DATE)
    SELECT '1480VE', 'key_send_methods_1480ve', OLD.KEY_ID, ic.Customer, ic.Specification, OLD.SEND_METHODS_ID, NULL, LEFT(IFNULL(@app_user, CONCAT('ip:', SUBSTRING_INDEX(USER(), '@', -1))), 20), USER(), 'trigger', NOW()
    FROM (SELECT 1) one LEFT JOIN `1480ve`.info_cus_condition ic ON ic.CusCondition_ID = OLD.KEY_ID;
END$$
DELIMITER ;

-- ---------------- email_attach_1480ve (FILE_FORMATS_ID) ----------------
CALL tmp_add_column('email_attach_1480ve', 'CREATE_BY',   'VARCHAR(20) COLLATE utf8_unicode_ci NULL');
CALL tmp_add_column('email_attach_1480ve', 'CREATE_DATE', 'DATETIME NULL');
CALL tmp_add_column('email_attach_1480ve', 'UPDATE_BY',   'VARCHAR(20) COLLATE utf8_unicode_ci NULL');
CALL tmp_add_column('email_attach_1480ve', 'UPDATE_DATE', 'DATETIME NULL');
DROP TRIGGER IF EXISTS trg_email_attach_1480ve_bi;
DROP TRIGGER IF EXISTS trg_email_attach_1480ve_bu;
DROP TRIGGER IF EXISTS trg_email_attach_1480ve_au;
DROP TRIGGER IF EXISTS trg_email_attach_1480ve_ad;
DELIMITER $$
CREATE TRIGGER trg_email_attach_1480ve_bi BEFORE INSERT ON email_attach_1480ve FOR EACH ROW
BEGIN
    SET NEW.CREATE_BY = IFNULL(NEW.CREATE_BY, LEFT(IFNULL(@app_user, CONCAT('ip:', SUBSTRING_INDEX(USER(), '@', -1))), 20));
    SET NEW.CREATE_DATE = IFNULL(NEW.CREATE_DATE, NOW());
END$$
CREATE TRIGGER trg_email_attach_1480ve_bu BEFORE UPDATE ON email_attach_1480ve FOR EACH ROW
BEGIN
    IF NOT (OLD.FILE_FORMATS_ID <=> NEW.FILE_FORMATS_ID) THEN
        SET NEW.UPDATE_BY = LEFT(IFNULL(@app_user, CONCAT('ip:', SUBSTRING_INDEX(USER(), '@', -1))), 20);
        SET NEW.UPDATE_DATE = NOW();
    END IF;
END$$
CREATE TRIGGER trg_email_attach_1480ve_au AFTER UPDATE ON email_attach_1480ve FOR EACH ROW
BEGIN
    IF NOT (OLD.FILE_FORMATS_ID <=> NEW.FILE_FORMATS_ID) THEN
        INSERT INTO log_setting_edata (PRODUCT, TABLE_NAME, KEY_ID, CUSTOMER, SPEC, OLD_VALUE, NEW_VALUE, CHANGE_USER, CHANGE_HOST, SOURCE, CHANGE_DATE)
        SELECT '1480VE', 'email_attach_1480ve', NEW.KEY_ID, ic.Customer, ic.Specification, OLD.FILE_FORMATS_ID, NEW.FILE_FORMATS_ID, NEW.UPDATE_BY, USER(), 'trigger', NOW()
        FROM (SELECT 1) one LEFT JOIN `1480ve`.info_cus_condition ic ON ic.CusCondition_ID = NEW.KEY_ID;
    END IF;
END$$
CREATE TRIGGER trg_email_attach_1480ve_ad AFTER DELETE ON email_attach_1480ve FOR EACH ROW
BEGIN
    INSERT INTO log_setting_edata (PRODUCT, TABLE_NAME, KEY_ID, CUSTOMER, SPEC, OLD_VALUE, NEW_VALUE, CHANGE_USER, CHANGE_HOST, SOURCE, CHANGE_DATE)
    SELECT '1480VE', 'email_attach_1480ve', OLD.KEY_ID, ic.Customer, ic.Specification, OLD.FILE_FORMATS_ID, NULL, LEFT(IFNULL(@app_user, CONCAT('ip:', SUBSTRING_INDEX(USER(), '@', -1))), 20), USER(), 'trigger', NOW()
    FROM (SELECT 1) one LEFT JOIN `1480ve`.info_cus_condition ic ON ic.CusCondition_ID = OLD.KEY_ID;
END$$
DELIMITER ;

-- ---------------- key_group_email_1480ve (GROUP_ID) ----------------
CALL tmp_add_column('key_group_email_1480ve', 'CREATE_BY',   'VARCHAR(20) COLLATE utf8_unicode_ci NULL');
CALL tmp_add_column('key_group_email_1480ve', 'CREATE_DATE', 'DATETIME NULL');
CALL tmp_add_column('key_group_email_1480ve', 'UPDATE_BY',   'VARCHAR(20) COLLATE utf8_unicode_ci NULL');
CALL tmp_add_column('key_group_email_1480ve', 'UPDATE_DATE', 'DATETIME NULL');
DROP TRIGGER IF EXISTS trg_key_group_email_1480ve_bi;
DROP TRIGGER IF EXISTS trg_key_group_email_1480ve_bu;
DROP TRIGGER IF EXISTS trg_key_group_email_1480ve_au;
DROP TRIGGER IF EXISTS trg_key_group_email_1480ve_ad;
DELIMITER $$
CREATE TRIGGER trg_key_group_email_1480ve_bi BEFORE INSERT ON key_group_email_1480ve FOR EACH ROW
BEGIN
    SET NEW.CREATE_BY = IFNULL(NEW.CREATE_BY, LEFT(IFNULL(@app_user, CONCAT('ip:', SUBSTRING_INDEX(USER(), '@', -1))), 20));
    SET NEW.CREATE_DATE = IFNULL(NEW.CREATE_DATE, NOW());
END$$
CREATE TRIGGER trg_key_group_email_1480ve_bu BEFORE UPDATE ON key_group_email_1480ve FOR EACH ROW
BEGIN
    IF NOT (OLD.GROUP_ID <=> NEW.GROUP_ID) THEN
        SET NEW.UPDATE_BY = LEFT(IFNULL(@app_user, CONCAT('ip:', SUBSTRING_INDEX(USER(), '@', -1))), 20);
        SET NEW.UPDATE_DATE = NOW();
    END IF;
END$$
CREATE TRIGGER trg_key_group_email_1480ve_au AFTER UPDATE ON key_group_email_1480ve FOR EACH ROW
BEGIN
    IF NOT (OLD.GROUP_ID <=> NEW.GROUP_ID) THEN
        INSERT INTO log_setting_edata (PRODUCT, TABLE_NAME, KEY_ID, CUSTOMER, SPEC, OLD_VALUE, NEW_VALUE, CHANGE_USER, CHANGE_HOST, SOURCE, CHANGE_DATE)
        SELECT '1480VE', 'key_group_email_1480ve', NEW.KEY_ID, ic.Customer, ic.Specification, OLD.GROUP_ID, NEW.GROUP_ID, NEW.UPDATE_BY, USER(), 'trigger', NOW()
        FROM (SELECT 1) one LEFT JOIN `1480ve`.info_cus_condition ic ON ic.CusCondition_ID = NEW.KEY_ID;
    END IF;
END$$
CREATE TRIGGER trg_key_group_email_1480ve_ad AFTER DELETE ON key_group_email_1480ve FOR EACH ROW
BEGIN
    INSERT INTO log_setting_edata (PRODUCT, TABLE_NAME, KEY_ID, CUSTOMER, SPEC, OLD_VALUE, NEW_VALUE, CHANGE_USER, CHANGE_HOST, SOURCE, CHANGE_DATE)
    SELECT '1480VE', 'key_group_email_1480ve', OLD.KEY_ID, ic.Customer, ic.Specification, OLD.GROUP_ID, NULL, LEFT(IFNULL(@app_user, CONCAT('ip:', SUBSTRING_INDEX(USER(), '@', -1))), 20), USER(), 'trigger', NOW()
    FROM (SELECT 1) one LEFT JOIN `1480ve`.info_cus_condition ic ON ic.CusCondition_ID = OLD.KEY_ID;
END$$
DELIMITER ;

-- ---------------- email_subject_1480ve (SUBJECT_ID) ----------------
CALL tmp_add_column('email_subject_1480ve', 'CREATE_BY',   'VARCHAR(20) COLLATE utf8_unicode_ci NULL');
CALL tmp_add_column('email_subject_1480ve', 'CREATE_DATE', 'DATETIME NULL');
CALL tmp_add_column('email_subject_1480ve', 'UPDATE_BY',   'VARCHAR(20) COLLATE utf8_unicode_ci NULL');
CALL tmp_add_column('email_subject_1480ve', 'UPDATE_DATE', 'DATETIME NULL');
DROP TRIGGER IF EXISTS trg_email_subject_1480ve_bi;
DROP TRIGGER IF EXISTS trg_email_subject_1480ve_bu;
DROP TRIGGER IF EXISTS trg_email_subject_1480ve_au;
DROP TRIGGER IF EXISTS trg_email_subject_1480ve_ad;
DELIMITER $$
CREATE TRIGGER trg_email_subject_1480ve_bi BEFORE INSERT ON email_subject_1480ve FOR EACH ROW
BEGIN
    SET NEW.CREATE_BY = IFNULL(NEW.CREATE_BY, LEFT(IFNULL(@app_user, CONCAT('ip:', SUBSTRING_INDEX(USER(), '@', -1))), 20));
    SET NEW.CREATE_DATE = IFNULL(NEW.CREATE_DATE, NOW());
END$$
CREATE TRIGGER trg_email_subject_1480ve_bu BEFORE UPDATE ON email_subject_1480ve FOR EACH ROW
BEGIN
    IF NOT (OLD.SUBJECT_ID <=> NEW.SUBJECT_ID) THEN
        SET NEW.UPDATE_BY = LEFT(IFNULL(@app_user, CONCAT('ip:', SUBSTRING_INDEX(USER(), '@', -1))), 20);
        SET NEW.UPDATE_DATE = NOW();
    END IF;
END$$
CREATE TRIGGER trg_email_subject_1480ve_au AFTER UPDATE ON email_subject_1480ve FOR EACH ROW
BEGIN
    IF NOT (OLD.SUBJECT_ID <=> NEW.SUBJECT_ID) THEN
        INSERT INTO log_setting_edata (PRODUCT, TABLE_NAME, KEY_ID, CUSTOMER, SPEC, OLD_VALUE, NEW_VALUE, CHANGE_USER, CHANGE_HOST, SOURCE, CHANGE_DATE)
        SELECT '1480VE', 'email_subject_1480ve', NEW.KEY_ID, ic.Customer, ic.Specification, OLD.SUBJECT_ID, NEW.SUBJECT_ID, NEW.UPDATE_BY, USER(), 'trigger', NOW()
        FROM (SELECT 1) one LEFT JOIN `1480ve`.info_cus_condition ic ON ic.CusCondition_ID = NEW.KEY_ID;
    END IF;
END$$
CREATE TRIGGER trg_email_subject_1480ve_ad AFTER DELETE ON email_subject_1480ve FOR EACH ROW
BEGIN
    INSERT INTO log_setting_edata (PRODUCT, TABLE_NAME, KEY_ID, CUSTOMER, SPEC, OLD_VALUE, NEW_VALUE, CHANGE_USER, CHANGE_HOST, SOURCE, CHANGE_DATE)
    SELECT '1480VE', 'email_subject_1480ve', OLD.KEY_ID, ic.Customer, ic.Specification, OLD.SUBJECT_ID, NULL, LEFT(IFNULL(@app_user, CONCAT('ip:', SUBSTRING_INDEX(USER(), '@', -1))), 20), USER(), 'trigger', NOW()
    FROM (SELECT 1) one LEFT JOIN `1480ve`.info_cus_condition ic ON ic.CusCondition_ID = OLD.KEY_ID;
END$$
DELIMITER ;

-- ---------------- email_body_1480ve (BODY_ID) ----------------
CALL tmp_add_column('email_body_1480ve', 'CREATE_BY',   'VARCHAR(20) COLLATE utf8_unicode_ci NULL');
CALL tmp_add_column('email_body_1480ve', 'CREATE_DATE', 'DATETIME NULL');
CALL tmp_add_column('email_body_1480ve', 'UPDATE_BY',   'VARCHAR(20) COLLATE utf8_unicode_ci NULL');
CALL tmp_add_column('email_body_1480ve', 'UPDATE_DATE', 'DATETIME NULL');
DROP TRIGGER IF EXISTS trg_email_body_1480ve_bi;
DROP TRIGGER IF EXISTS trg_email_body_1480ve_bu;
DROP TRIGGER IF EXISTS trg_email_body_1480ve_au;
DROP TRIGGER IF EXISTS trg_email_body_1480ve_ad;
DELIMITER $$
CREATE TRIGGER trg_email_body_1480ve_bi BEFORE INSERT ON email_body_1480ve FOR EACH ROW
BEGIN
    SET NEW.CREATE_BY = IFNULL(NEW.CREATE_BY, LEFT(IFNULL(@app_user, CONCAT('ip:', SUBSTRING_INDEX(USER(), '@', -1))), 20));
    SET NEW.CREATE_DATE = IFNULL(NEW.CREATE_DATE, NOW());
END$$
CREATE TRIGGER trg_email_body_1480ve_bu BEFORE UPDATE ON email_body_1480ve FOR EACH ROW
BEGIN
    IF NOT (OLD.BODY_ID <=> NEW.BODY_ID) THEN
        SET NEW.UPDATE_BY = LEFT(IFNULL(@app_user, CONCAT('ip:', SUBSTRING_INDEX(USER(), '@', -1))), 20);
        SET NEW.UPDATE_DATE = NOW();
    END IF;
END$$
CREATE TRIGGER trg_email_body_1480ve_au AFTER UPDATE ON email_body_1480ve FOR EACH ROW
BEGIN
    IF NOT (OLD.BODY_ID <=> NEW.BODY_ID) THEN
        INSERT INTO log_setting_edata (PRODUCT, TABLE_NAME, KEY_ID, CUSTOMER, SPEC, OLD_VALUE, NEW_VALUE, CHANGE_USER, CHANGE_HOST, SOURCE, CHANGE_DATE)
        SELECT '1480VE', 'email_body_1480ve', NEW.KEY_ID, ic.Customer, ic.Specification, OLD.BODY_ID, NEW.BODY_ID, NEW.UPDATE_BY, USER(), 'trigger', NOW()
        FROM (SELECT 1) one LEFT JOIN `1480ve`.info_cus_condition ic ON ic.CusCondition_ID = NEW.KEY_ID;
    END IF;
END$$
CREATE TRIGGER trg_email_body_1480ve_ad AFTER DELETE ON email_body_1480ve FOR EACH ROW
BEGIN
    INSERT INTO log_setting_edata (PRODUCT, TABLE_NAME, KEY_ID, CUSTOMER, SPEC, OLD_VALUE, NEW_VALUE, CHANGE_USER, CHANGE_HOST, SOURCE, CHANGE_DATE)
    SELECT '1480VE', 'email_body_1480ve', OLD.KEY_ID, ic.Customer, ic.Specification, OLD.BODY_ID, NULL, LEFT(IFNULL(@app_user, CONCAT('ip:', SUBSTRING_INDEX(USER(), '@', -1))), 20), USER(), 'trigger', NOW()
    FROM (SELECT 1) one LEFT JOIN `1480ve`.info_cus_condition ic ON ic.CusCondition_ID = OLD.KEY_ID;
END$$
DELIMITER ;

-- ---------------- key_send_methods_980 (SEND_METHODS_ID) ----------------
CALL tmp_add_column('key_send_methods_980', 'CREATE_BY',   'VARCHAR(20) COLLATE utf8_unicode_ci NULL');
CALL tmp_add_column('key_send_methods_980', 'CREATE_DATE', 'DATETIME NULL');
CALL tmp_add_column('key_send_methods_980', 'UPDATE_BY',   'VARCHAR(20) COLLATE utf8_unicode_ci NULL');
CALL tmp_add_column('key_send_methods_980', 'UPDATE_DATE', 'DATETIME NULL');
DROP TRIGGER IF EXISTS trg_key_send_methods_980_bi;
DROP TRIGGER IF EXISTS trg_key_send_methods_980_bu;
DROP TRIGGER IF EXISTS trg_key_send_methods_980_au;
DROP TRIGGER IF EXISTS trg_key_send_methods_980_ad;
DELIMITER $$
CREATE TRIGGER trg_key_send_methods_980_bi BEFORE INSERT ON key_send_methods_980 FOR EACH ROW
BEGIN
    SET NEW.CREATE_BY = IFNULL(NEW.CREATE_BY, LEFT(IFNULL(@app_user, CONCAT('ip:', SUBSTRING_INDEX(USER(), '@', -1))), 20));
    SET NEW.CREATE_DATE = IFNULL(NEW.CREATE_DATE, NOW());
END$$
CREATE TRIGGER trg_key_send_methods_980_bu BEFORE UPDATE ON key_send_methods_980 FOR EACH ROW
BEGIN
    IF NOT (OLD.SEND_METHODS_ID <=> NEW.SEND_METHODS_ID) THEN
        SET NEW.UPDATE_BY = LEFT(IFNULL(@app_user, CONCAT('ip:', SUBSTRING_INDEX(USER(), '@', -1))), 20);
        SET NEW.UPDATE_DATE = NOW();
    END IF;
END$$
CREATE TRIGGER trg_key_send_methods_980_au AFTER UPDATE ON key_send_methods_980 FOR EACH ROW
BEGIN
    IF NOT (OLD.SEND_METHODS_ID <=> NEW.SEND_METHODS_ID) THEN
        INSERT INTO log_setting_edata (PRODUCT, TABLE_NAME, KEY_ID, CUSTOMER, SPEC, OLD_VALUE, NEW_VALUE, CHANGE_USER, CHANGE_HOST, SOURCE, CHANGE_DATE)
        SELECT '980', 'key_send_methods_980', NEW.KEY_ID, ic.Customer, ic.Specification, OLD.SEND_METHODS_ID, NEW.SEND_METHODS_ID, NEW.UPDATE_BY, USER(), 'trigger', NOW()
        FROM (SELECT 1) one LEFT JOIN `980ldm`.info_cus_condition ic ON ic.CusCondition_ID = NEW.KEY_ID;
    END IF;
END$$
CREATE TRIGGER trg_key_send_methods_980_ad AFTER DELETE ON key_send_methods_980 FOR EACH ROW
BEGIN
    INSERT INTO log_setting_edata (PRODUCT, TABLE_NAME, KEY_ID, CUSTOMER, SPEC, OLD_VALUE, NEW_VALUE, CHANGE_USER, CHANGE_HOST, SOURCE, CHANGE_DATE)
    SELECT '980', 'key_send_methods_980', OLD.KEY_ID, ic.Customer, ic.Specification, OLD.SEND_METHODS_ID, NULL, LEFT(IFNULL(@app_user, CONCAT('ip:', SUBSTRING_INDEX(USER(), '@', -1))), 20), USER(), 'trigger', NOW()
    FROM (SELECT 1) one LEFT JOIN `980ldm`.info_cus_condition ic ON ic.CusCondition_ID = OLD.KEY_ID;
END$$
DELIMITER ;

-- ---------------- email_attach_980 (FILE_FORMATS_ID) ----------------
CALL tmp_add_column('email_attach_980', 'CREATE_BY',   'VARCHAR(20) COLLATE utf8_unicode_ci NULL');
CALL tmp_add_column('email_attach_980', 'CREATE_DATE', 'DATETIME NULL');
CALL tmp_add_column('email_attach_980', 'UPDATE_BY',   'VARCHAR(20) COLLATE utf8_unicode_ci NULL');
CALL tmp_add_column('email_attach_980', 'UPDATE_DATE', 'DATETIME NULL');
DROP TRIGGER IF EXISTS trg_email_attach_980_bi;
DROP TRIGGER IF EXISTS trg_email_attach_980_bu;
DROP TRIGGER IF EXISTS trg_email_attach_980_au;
DROP TRIGGER IF EXISTS trg_email_attach_980_ad;
DELIMITER $$
CREATE TRIGGER trg_email_attach_980_bi BEFORE INSERT ON email_attach_980 FOR EACH ROW
BEGIN
    SET NEW.CREATE_BY = IFNULL(NEW.CREATE_BY, LEFT(IFNULL(@app_user, CONCAT('ip:', SUBSTRING_INDEX(USER(), '@', -1))), 20));
    SET NEW.CREATE_DATE = IFNULL(NEW.CREATE_DATE, NOW());
END$$
CREATE TRIGGER trg_email_attach_980_bu BEFORE UPDATE ON email_attach_980 FOR EACH ROW
BEGIN
    IF NOT (OLD.FILE_FORMATS_ID <=> NEW.FILE_FORMATS_ID) THEN
        SET NEW.UPDATE_BY = LEFT(IFNULL(@app_user, CONCAT('ip:', SUBSTRING_INDEX(USER(), '@', -1))), 20);
        SET NEW.UPDATE_DATE = NOW();
    END IF;
END$$
CREATE TRIGGER trg_email_attach_980_au AFTER UPDATE ON email_attach_980 FOR EACH ROW
BEGIN
    IF NOT (OLD.FILE_FORMATS_ID <=> NEW.FILE_FORMATS_ID) THEN
        INSERT INTO log_setting_edata (PRODUCT, TABLE_NAME, KEY_ID, CUSTOMER, SPEC, OLD_VALUE, NEW_VALUE, CHANGE_USER, CHANGE_HOST, SOURCE, CHANGE_DATE)
        SELECT '980', 'email_attach_980', NEW.KEY_ID, ic.Customer, ic.Specification, OLD.FILE_FORMATS_ID, NEW.FILE_FORMATS_ID, NEW.UPDATE_BY, USER(), 'trigger', NOW()
        FROM (SELECT 1) one LEFT JOIN `980ldm`.info_cus_condition ic ON ic.CusCondition_ID = NEW.KEY_ID;
    END IF;
END$$
CREATE TRIGGER trg_email_attach_980_ad AFTER DELETE ON email_attach_980 FOR EACH ROW
BEGIN
    INSERT INTO log_setting_edata (PRODUCT, TABLE_NAME, KEY_ID, CUSTOMER, SPEC, OLD_VALUE, NEW_VALUE, CHANGE_USER, CHANGE_HOST, SOURCE, CHANGE_DATE)
    SELECT '980', 'email_attach_980', OLD.KEY_ID, ic.Customer, ic.Specification, OLD.FILE_FORMATS_ID, NULL, LEFT(IFNULL(@app_user, CONCAT('ip:', SUBSTRING_INDEX(USER(), '@', -1))), 20), USER(), 'trigger', NOW()
    FROM (SELECT 1) one LEFT JOIN `980ldm`.info_cus_condition ic ON ic.CusCondition_ID = OLD.KEY_ID;
END$$
DELIMITER ;

-- ---------------- key_group_email_980 (GROUP_ID) ----------------
CALL tmp_add_column('key_group_email_980', 'CREATE_BY',   'VARCHAR(20) COLLATE utf8_unicode_ci NULL');
CALL tmp_add_column('key_group_email_980', 'CREATE_DATE', 'DATETIME NULL');
CALL tmp_add_column('key_group_email_980', 'UPDATE_BY',   'VARCHAR(20) COLLATE utf8_unicode_ci NULL');
CALL tmp_add_column('key_group_email_980', 'UPDATE_DATE', 'DATETIME NULL');
DROP TRIGGER IF EXISTS trg_key_group_email_980_bi;
DROP TRIGGER IF EXISTS trg_key_group_email_980_bu;
DROP TRIGGER IF EXISTS trg_key_group_email_980_au;
DROP TRIGGER IF EXISTS trg_key_group_email_980_ad;
DELIMITER $$
CREATE TRIGGER trg_key_group_email_980_bi BEFORE INSERT ON key_group_email_980 FOR EACH ROW
BEGIN
    SET NEW.CREATE_BY = IFNULL(NEW.CREATE_BY, LEFT(IFNULL(@app_user, CONCAT('ip:', SUBSTRING_INDEX(USER(), '@', -1))), 20));
    SET NEW.CREATE_DATE = IFNULL(NEW.CREATE_DATE, NOW());
END$$
CREATE TRIGGER trg_key_group_email_980_bu BEFORE UPDATE ON key_group_email_980 FOR EACH ROW
BEGIN
    IF NOT (OLD.GROUP_ID <=> NEW.GROUP_ID) THEN
        SET NEW.UPDATE_BY = LEFT(IFNULL(@app_user, CONCAT('ip:', SUBSTRING_INDEX(USER(), '@', -1))), 20);
        SET NEW.UPDATE_DATE = NOW();
    END IF;
END$$
CREATE TRIGGER trg_key_group_email_980_au AFTER UPDATE ON key_group_email_980 FOR EACH ROW
BEGIN
    IF NOT (OLD.GROUP_ID <=> NEW.GROUP_ID) THEN
        INSERT INTO log_setting_edata (PRODUCT, TABLE_NAME, KEY_ID, CUSTOMER, SPEC, OLD_VALUE, NEW_VALUE, CHANGE_USER, CHANGE_HOST, SOURCE, CHANGE_DATE)
        SELECT '980', 'key_group_email_980', NEW.KEY_ID, ic.Customer, ic.Specification, OLD.GROUP_ID, NEW.GROUP_ID, NEW.UPDATE_BY, USER(), 'trigger', NOW()
        FROM (SELECT 1) one LEFT JOIN `980ldm`.info_cus_condition ic ON ic.CusCondition_ID = NEW.KEY_ID;
    END IF;
END$$
CREATE TRIGGER trg_key_group_email_980_ad AFTER DELETE ON key_group_email_980 FOR EACH ROW
BEGIN
    INSERT INTO log_setting_edata (PRODUCT, TABLE_NAME, KEY_ID, CUSTOMER, SPEC, OLD_VALUE, NEW_VALUE, CHANGE_USER, CHANGE_HOST, SOURCE, CHANGE_DATE)
    SELECT '980', 'key_group_email_980', OLD.KEY_ID, ic.Customer, ic.Specification, OLD.GROUP_ID, NULL, LEFT(IFNULL(@app_user, CONCAT('ip:', SUBSTRING_INDEX(USER(), '@', -1))), 20), USER(), 'trigger', NOW()
    FROM (SELECT 1) one LEFT JOIN `980ldm`.info_cus_condition ic ON ic.CusCondition_ID = OLD.KEY_ID;
END$$
DELIMITER ;

-- ---------------- email_subject_980 (SUBJECT_ID) ----------------
CALL tmp_add_column('email_subject_980', 'CREATE_BY',   'VARCHAR(20) COLLATE utf8_unicode_ci NULL');
CALL tmp_add_column('email_subject_980', 'CREATE_DATE', 'DATETIME NULL');
CALL tmp_add_column('email_subject_980', 'UPDATE_BY',   'VARCHAR(20) COLLATE utf8_unicode_ci NULL');
CALL tmp_add_column('email_subject_980', 'UPDATE_DATE', 'DATETIME NULL');
DROP TRIGGER IF EXISTS trg_email_subject_980_bi;
DROP TRIGGER IF EXISTS trg_email_subject_980_bu;
DROP TRIGGER IF EXISTS trg_email_subject_980_au;
DROP TRIGGER IF EXISTS trg_email_subject_980_ad;
DELIMITER $$
CREATE TRIGGER trg_email_subject_980_bi BEFORE INSERT ON email_subject_980 FOR EACH ROW
BEGIN
    SET NEW.CREATE_BY = IFNULL(NEW.CREATE_BY, LEFT(IFNULL(@app_user, CONCAT('ip:', SUBSTRING_INDEX(USER(), '@', -1))), 20));
    SET NEW.CREATE_DATE = IFNULL(NEW.CREATE_DATE, NOW());
END$$
CREATE TRIGGER trg_email_subject_980_bu BEFORE UPDATE ON email_subject_980 FOR EACH ROW
BEGIN
    IF NOT (OLD.SUBJECT_ID <=> NEW.SUBJECT_ID) THEN
        SET NEW.UPDATE_BY = LEFT(IFNULL(@app_user, CONCAT('ip:', SUBSTRING_INDEX(USER(), '@', -1))), 20);
        SET NEW.UPDATE_DATE = NOW();
    END IF;
END$$
CREATE TRIGGER trg_email_subject_980_au AFTER UPDATE ON email_subject_980 FOR EACH ROW
BEGIN
    IF NOT (OLD.SUBJECT_ID <=> NEW.SUBJECT_ID) THEN
        INSERT INTO log_setting_edata (PRODUCT, TABLE_NAME, KEY_ID, CUSTOMER, SPEC, OLD_VALUE, NEW_VALUE, CHANGE_USER, CHANGE_HOST, SOURCE, CHANGE_DATE)
        SELECT '980', 'email_subject_980', NEW.KEY_ID, ic.Customer, ic.Specification, OLD.SUBJECT_ID, NEW.SUBJECT_ID, NEW.UPDATE_BY, USER(), 'trigger', NOW()
        FROM (SELECT 1) one LEFT JOIN `980ldm`.info_cus_condition ic ON ic.CusCondition_ID = NEW.KEY_ID;
    END IF;
END$$
CREATE TRIGGER trg_email_subject_980_ad AFTER DELETE ON email_subject_980 FOR EACH ROW
BEGIN
    INSERT INTO log_setting_edata (PRODUCT, TABLE_NAME, KEY_ID, CUSTOMER, SPEC, OLD_VALUE, NEW_VALUE, CHANGE_USER, CHANGE_HOST, SOURCE, CHANGE_DATE)
    SELECT '980', 'email_subject_980', OLD.KEY_ID, ic.Customer, ic.Specification, OLD.SUBJECT_ID, NULL, LEFT(IFNULL(@app_user, CONCAT('ip:', SUBSTRING_INDEX(USER(), '@', -1))), 20), USER(), 'trigger', NOW()
    FROM (SELECT 1) one LEFT JOIN `980ldm`.info_cus_condition ic ON ic.CusCondition_ID = OLD.KEY_ID;
END$$
DELIMITER ;

-- ---------------- email_body_980 (BODY_ID) ----------------
CALL tmp_add_column('email_body_980', 'CREATE_BY',   'VARCHAR(20) COLLATE utf8_unicode_ci NULL');
CALL tmp_add_column('email_body_980', 'CREATE_DATE', 'DATETIME NULL');
CALL tmp_add_column('email_body_980', 'UPDATE_BY',   'VARCHAR(20) COLLATE utf8_unicode_ci NULL');
CALL tmp_add_column('email_body_980', 'UPDATE_DATE', 'DATETIME NULL');
DROP TRIGGER IF EXISTS trg_email_body_980_bi;
DROP TRIGGER IF EXISTS trg_email_body_980_bu;
DROP TRIGGER IF EXISTS trg_email_body_980_au;
DROP TRIGGER IF EXISTS trg_email_body_980_ad;
DELIMITER $$
CREATE TRIGGER trg_email_body_980_bi BEFORE INSERT ON email_body_980 FOR EACH ROW
BEGIN
    SET NEW.CREATE_BY = IFNULL(NEW.CREATE_BY, LEFT(IFNULL(@app_user, CONCAT('ip:', SUBSTRING_INDEX(USER(), '@', -1))), 20));
    SET NEW.CREATE_DATE = IFNULL(NEW.CREATE_DATE, NOW());
END$$
CREATE TRIGGER trg_email_body_980_bu BEFORE UPDATE ON email_body_980 FOR EACH ROW
BEGIN
    IF NOT (OLD.BODY_ID <=> NEW.BODY_ID) THEN
        SET NEW.UPDATE_BY = LEFT(IFNULL(@app_user, CONCAT('ip:', SUBSTRING_INDEX(USER(), '@', -1))), 20);
        SET NEW.UPDATE_DATE = NOW();
    END IF;
END$$
CREATE TRIGGER trg_email_body_980_au AFTER UPDATE ON email_body_980 FOR EACH ROW
BEGIN
    IF NOT (OLD.BODY_ID <=> NEW.BODY_ID) THEN
        INSERT INTO log_setting_edata (PRODUCT, TABLE_NAME, KEY_ID, CUSTOMER, SPEC, OLD_VALUE, NEW_VALUE, CHANGE_USER, CHANGE_HOST, SOURCE, CHANGE_DATE)
        SELECT '980', 'email_body_980', NEW.KEY_ID, ic.Customer, ic.Specification, OLD.BODY_ID, NEW.BODY_ID, NEW.UPDATE_BY, USER(), 'trigger', NOW()
        FROM (SELECT 1) one LEFT JOIN `980ldm`.info_cus_condition ic ON ic.CusCondition_ID = NEW.KEY_ID;
    END IF;
END$$
CREATE TRIGGER trg_email_body_980_ad AFTER DELETE ON email_body_980 FOR EACH ROW
BEGIN
    INSERT INTO log_setting_edata (PRODUCT, TABLE_NAME, KEY_ID, CUSTOMER, SPEC, OLD_VALUE, NEW_VALUE, CHANGE_USER, CHANGE_HOST, SOURCE, CHANGE_DATE)
    SELECT '980', 'email_body_980', OLD.KEY_ID, ic.Customer, ic.Specification, OLD.BODY_ID, NULL, LEFT(IFNULL(@app_user, CONCAT('ip:', SUBSTRING_INDEX(USER(), '@', -1))), 20), USER(), 'trigger', NOW()
    FROM (SELECT 1) one LEFT JOIN `980ldm`.info_cus_condition ic ON ic.CusCondition_ID = OLD.KEY_ID;
END$$
DELIMITER ;

-- ---------------- key_send_methods_980dual (SEND_METHODS_ID) ----------------
CALL tmp_add_column('key_send_methods_980dual', 'CREATE_BY',   'VARCHAR(20) COLLATE utf8_unicode_ci NULL');
CALL tmp_add_column('key_send_methods_980dual', 'CREATE_DATE', 'DATETIME NULL');
CALL tmp_add_column('key_send_methods_980dual', 'UPDATE_BY',   'VARCHAR(20) COLLATE utf8_unicode_ci NULL');
CALL tmp_add_column('key_send_methods_980dual', 'UPDATE_DATE', 'DATETIME NULL');
DROP TRIGGER IF EXISTS trg_key_send_methods_980dual_bi;
DROP TRIGGER IF EXISTS trg_key_send_methods_980dual_bu;
DROP TRIGGER IF EXISTS trg_key_send_methods_980dual_au;
DROP TRIGGER IF EXISTS trg_key_send_methods_980dual_ad;
DELIMITER $$
CREATE TRIGGER trg_key_send_methods_980dual_bi BEFORE INSERT ON key_send_methods_980dual FOR EACH ROW
BEGIN
    SET NEW.CREATE_BY = IFNULL(NEW.CREATE_BY, LEFT(IFNULL(@app_user, CONCAT('ip:', SUBSTRING_INDEX(USER(), '@', -1))), 20));
    SET NEW.CREATE_DATE = IFNULL(NEW.CREATE_DATE, NOW());
END$$
CREATE TRIGGER trg_key_send_methods_980dual_bu BEFORE UPDATE ON key_send_methods_980dual FOR EACH ROW
BEGIN
    IF NOT (OLD.SEND_METHODS_ID <=> NEW.SEND_METHODS_ID) THEN
        SET NEW.UPDATE_BY = LEFT(IFNULL(@app_user, CONCAT('ip:', SUBSTRING_INDEX(USER(), '@', -1))), 20);
        SET NEW.UPDATE_DATE = NOW();
    END IF;
END$$
CREATE TRIGGER trg_key_send_methods_980dual_au AFTER UPDATE ON key_send_methods_980dual FOR EACH ROW
BEGIN
    IF NOT (OLD.SEND_METHODS_ID <=> NEW.SEND_METHODS_ID) THEN
        INSERT INTO log_setting_edata (PRODUCT, TABLE_NAME, KEY_ID, CUSTOMER, SPEC, OLD_VALUE, NEW_VALUE, CHANGE_USER, CHANGE_HOST, SOURCE, CHANGE_DATE)
        SELECT '980Dual', 'key_send_methods_980dual', NEW.KEY_ID, ic.Customer, ic.Specification, OLD.SEND_METHODS_ID, NEW.SEND_METHODS_ID, NEW.UPDATE_BY, USER(), 'trigger', NOW()
        FROM (SELECT 1) one LEFT JOIN `980Dual`.info_cus_condition ic ON ic.CusCondition_ID = NEW.KEY_ID;
    END IF;
END$$
CREATE TRIGGER trg_key_send_methods_980dual_ad AFTER DELETE ON key_send_methods_980dual FOR EACH ROW
BEGIN
    INSERT INTO log_setting_edata (PRODUCT, TABLE_NAME, KEY_ID, CUSTOMER, SPEC, OLD_VALUE, NEW_VALUE, CHANGE_USER, CHANGE_HOST, SOURCE, CHANGE_DATE)
    SELECT '980Dual', 'key_send_methods_980dual', OLD.KEY_ID, ic.Customer, ic.Specification, OLD.SEND_METHODS_ID, NULL, LEFT(IFNULL(@app_user, CONCAT('ip:', SUBSTRING_INDEX(USER(), '@', -1))), 20), USER(), 'trigger', NOW()
    FROM (SELECT 1) one LEFT JOIN `980Dual`.info_cus_condition ic ON ic.CusCondition_ID = OLD.KEY_ID;
END$$
DELIMITER ;

-- ---------------- email_attach_980dual (FILE_FORMATS_ID) ----------------
CALL tmp_add_column('email_attach_980dual', 'CREATE_BY',   'VARCHAR(20) COLLATE utf8_unicode_ci NULL');
CALL tmp_add_column('email_attach_980dual', 'CREATE_DATE', 'DATETIME NULL');
CALL tmp_add_column('email_attach_980dual', 'UPDATE_BY',   'VARCHAR(20) COLLATE utf8_unicode_ci NULL');
CALL tmp_add_column('email_attach_980dual', 'UPDATE_DATE', 'DATETIME NULL');
DROP TRIGGER IF EXISTS trg_email_attach_980dual_bi;
DROP TRIGGER IF EXISTS trg_email_attach_980dual_bu;
DROP TRIGGER IF EXISTS trg_email_attach_980dual_au;
DROP TRIGGER IF EXISTS trg_email_attach_980dual_ad;
DELIMITER $$
CREATE TRIGGER trg_email_attach_980dual_bi BEFORE INSERT ON email_attach_980dual FOR EACH ROW
BEGIN
    SET NEW.CREATE_BY = IFNULL(NEW.CREATE_BY, LEFT(IFNULL(@app_user, CONCAT('ip:', SUBSTRING_INDEX(USER(), '@', -1))), 20));
    SET NEW.CREATE_DATE = IFNULL(NEW.CREATE_DATE, NOW());
END$$
CREATE TRIGGER trg_email_attach_980dual_bu BEFORE UPDATE ON email_attach_980dual FOR EACH ROW
BEGIN
    IF NOT (OLD.FILE_FORMATS_ID <=> NEW.FILE_FORMATS_ID) THEN
        SET NEW.UPDATE_BY = LEFT(IFNULL(@app_user, CONCAT('ip:', SUBSTRING_INDEX(USER(), '@', -1))), 20);
        SET NEW.UPDATE_DATE = NOW();
    END IF;
END$$
CREATE TRIGGER trg_email_attach_980dual_au AFTER UPDATE ON email_attach_980dual FOR EACH ROW
BEGIN
    IF NOT (OLD.FILE_FORMATS_ID <=> NEW.FILE_FORMATS_ID) THEN
        INSERT INTO log_setting_edata (PRODUCT, TABLE_NAME, KEY_ID, CUSTOMER, SPEC, OLD_VALUE, NEW_VALUE, CHANGE_USER, CHANGE_HOST, SOURCE, CHANGE_DATE)
        SELECT '980Dual', 'email_attach_980dual', NEW.KEY_ID, ic.Customer, ic.Specification, OLD.FILE_FORMATS_ID, NEW.FILE_FORMATS_ID, NEW.UPDATE_BY, USER(), 'trigger', NOW()
        FROM (SELECT 1) one LEFT JOIN `980Dual`.info_cus_condition ic ON ic.CusCondition_ID = NEW.KEY_ID;
    END IF;
END$$
CREATE TRIGGER trg_email_attach_980dual_ad AFTER DELETE ON email_attach_980dual FOR EACH ROW
BEGIN
    INSERT INTO log_setting_edata (PRODUCT, TABLE_NAME, KEY_ID, CUSTOMER, SPEC, OLD_VALUE, NEW_VALUE, CHANGE_USER, CHANGE_HOST, SOURCE, CHANGE_DATE)
    SELECT '980Dual', 'email_attach_980dual', OLD.KEY_ID, ic.Customer, ic.Specification, OLD.FILE_FORMATS_ID, NULL, LEFT(IFNULL(@app_user, CONCAT('ip:', SUBSTRING_INDEX(USER(), '@', -1))), 20), USER(), 'trigger', NOW()
    FROM (SELECT 1) one LEFT JOIN `980Dual`.info_cus_condition ic ON ic.CusCondition_ID = OLD.KEY_ID;
END$$
DELIMITER ;

-- ---------------- key_group_email_980dual (GROUP_ID) ----------------
CALL tmp_add_column('key_group_email_980dual', 'CREATE_BY',   'VARCHAR(20) COLLATE utf8_unicode_ci NULL');
CALL tmp_add_column('key_group_email_980dual', 'CREATE_DATE', 'DATETIME NULL');
CALL tmp_add_column('key_group_email_980dual', 'UPDATE_BY',   'VARCHAR(20) COLLATE utf8_unicode_ci NULL');
CALL tmp_add_column('key_group_email_980dual', 'UPDATE_DATE', 'DATETIME NULL');
DROP TRIGGER IF EXISTS trg_key_group_email_980dual_bi;
DROP TRIGGER IF EXISTS trg_key_group_email_980dual_bu;
DROP TRIGGER IF EXISTS trg_key_group_email_980dual_au;
DROP TRIGGER IF EXISTS trg_key_group_email_980dual_ad;
DELIMITER $$
CREATE TRIGGER trg_key_group_email_980dual_bi BEFORE INSERT ON key_group_email_980dual FOR EACH ROW
BEGIN
    SET NEW.CREATE_BY = IFNULL(NEW.CREATE_BY, LEFT(IFNULL(@app_user, CONCAT('ip:', SUBSTRING_INDEX(USER(), '@', -1))), 20));
    SET NEW.CREATE_DATE = IFNULL(NEW.CREATE_DATE, NOW());
END$$
CREATE TRIGGER trg_key_group_email_980dual_bu BEFORE UPDATE ON key_group_email_980dual FOR EACH ROW
BEGIN
    IF NOT (OLD.GROUP_ID <=> NEW.GROUP_ID) THEN
        SET NEW.UPDATE_BY = LEFT(IFNULL(@app_user, CONCAT('ip:', SUBSTRING_INDEX(USER(), '@', -1))), 20);
        SET NEW.UPDATE_DATE = NOW();
    END IF;
END$$
CREATE TRIGGER trg_key_group_email_980dual_au AFTER UPDATE ON key_group_email_980dual FOR EACH ROW
BEGIN
    IF NOT (OLD.GROUP_ID <=> NEW.GROUP_ID) THEN
        INSERT INTO log_setting_edata (PRODUCT, TABLE_NAME, KEY_ID, CUSTOMER, SPEC, OLD_VALUE, NEW_VALUE, CHANGE_USER, CHANGE_HOST, SOURCE, CHANGE_DATE)
        SELECT '980Dual', 'key_group_email_980dual', NEW.KEY_ID, ic.Customer, ic.Specification, OLD.GROUP_ID, NEW.GROUP_ID, NEW.UPDATE_BY, USER(), 'trigger', NOW()
        FROM (SELECT 1) one LEFT JOIN `980Dual`.info_cus_condition ic ON ic.CusCondition_ID = NEW.KEY_ID;
    END IF;
END$$
CREATE TRIGGER trg_key_group_email_980dual_ad AFTER DELETE ON key_group_email_980dual FOR EACH ROW
BEGIN
    INSERT INTO log_setting_edata (PRODUCT, TABLE_NAME, KEY_ID, CUSTOMER, SPEC, OLD_VALUE, NEW_VALUE, CHANGE_USER, CHANGE_HOST, SOURCE, CHANGE_DATE)
    SELECT '980Dual', 'key_group_email_980dual', OLD.KEY_ID, ic.Customer, ic.Specification, OLD.GROUP_ID, NULL, LEFT(IFNULL(@app_user, CONCAT('ip:', SUBSTRING_INDEX(USER(), '@', -1))), 20), USER(), 'trigger', NOW()
    FROM (SELECT 1) one LEFT JOIN `980Dual`.info_cus_condition ic ON ic.CusCondition_ID = OLD.KEY_ID;
END$$
DELIMITER ;

-- ---------------- email_subject_980dual (SUBJECT_ID) ----------------
CALL tmp_add_column('email_subject_980dual', 'CREATE_BY',   'VARCHAR(20) COLLATE utf8_unicode_ci NULL');
CALL tmp_add_column('email_subject_980dual', 'CREATE_DATE', 'DATETIME NULL');
CALL tmp_add_column('email_subject_980dual', 'UPDATE_BY',   'VARCHAR(20) COLLATE utf8_unicode_ci NULL');
CALL tmp_add_column('email_subject_980dual', 'UPDATE_DATE', 'DATETIME NULL');
DROP TRIGGER IF EXISTS trg_email_subject_980dual_bi;
DROP TRIGGER IF EXISTS trg_email_subject_980dual_bu;
DROP TRIGGER IF EXISTS trg_email_subject_980dual_au;
DROP TRIGGER IF EXISTS trg_email_subject_980dual_ad;
DELIMITER $$
CREATE TRIGGER trg_email_subject_980dual_bi BEFORE INSERT ON email_subject_980dual FOR EACH ROW
BEGIN
    SET NEW.CREATE_BY = IFNULL(NEW.CREATE_BY, LEFT(IFNULL(@app_user, CONCAT('ip:', SUBSTRING_INDEX(USER(), '@', -1))), 20));
    SET NEW.CREATE_DATE = IFNULL(NEW.CREATE_DATE, NOW());
END$$
CREATE TRIGGER trg_email_subject_980dual_bu BEFORE UPDATE ON email_subject_980dual FOR EACH ROW
BEGIN
    IF NOT (OLD.SUBJECT_ID <=> NEW.SUBJECT_ID) THEN
        SET NEW.UPDATE_BY = LEFT(IFNULL(@app_user, CONCAT('ip:', SUBSTRING_INDEX(USER(), '@', -1))), 20);
        SET NEW.UPDATE_DATE = NOW();
    END IF;
END$$
CREATE TRIGGER trg_email_subject_980dual_au AFTER UPDATE ON email_subject_980dual FOR EACH ROW
BEGIN
    IF NOT (OLD.SUBJECT_ID <=> NEW.SUBJECT_ID) THEN
        INSERT INTO log_setting_edata (PRODUCT, TABLE_NAME, KEY_ID, CUSTOMER, SPEC, OLD_VALUE, NEW_VALUE, CHANGE_USER, CHANGE_HOST, SOURCE, CHANGE_DATE)
        SELECT '980Dual', 'email_subject_980dual', NEW.KEY_ID, ic.Customer, ic.Specification, OLD.SUBJECT_ID, NEW.SUBJECT_ID, NEW.UPDATE_BY, USER(), 'trigger', NOW()
        FROM (SELECT 1) one LEFT JOIN `980Dual`.info_cus_condition ic ON ic.CusCondition_ID = NEW.KEY_ID;
    END IF;
END$$
CREATE TRIGGER trg_email_subject_980dual_ad AFTER DELETE ON email_subject_980dual FOR EACH ROW
BEGIN
    INSERT INTO log_setting_edata (PRODUCT, TABLE_NAME, KEY_ID, CUSTOMER, SPEC, OLD_VALUE, NEW_VALUE, CHANGE_USER, CHANGE_HOST, SOURCE, CHANGE_DATE)
    SELECT '980Dual', 'email_subject_980dual', OLD.KEY_ID, ic.Customer, ic.Specification, OLD.SUBJECT_ID, NULL, LEFT(IFNULL(@app_user, CONCAT('ip:', SUBSTRING_INDEX(USER(), '@', -1))), 20), USER(), 'trigger', NOW()
    FROM (SELECT 1) one LEFT JOIN `980Dual`.info_cus_condition ic ON ic.CusCondition_ID = OLD.KEY_ID;
END$$
DELIMITER ;

-- ---------------- email_body_980dual (BODY_ID) ----------------
CALL tmp_add_column('email_body_980dual', 'CREATE_BY',   'VARCHAR(20) COLLATE utf8_unicode_ci NULL');
CALL tmp_add_column('email_body_980dual', 'CREATE_DATE', 'DATETIME NULL');
CALL tmp_add_column('email_body_980dual', 'UPDATE_BY',   'VARCHAR(20) COLLATE utf8_unicode_ci NULL');
CALL tmp_add_column('email_body_980dual', 'UPDATE_DATE', 'DATETIME NULL');
DROP TRIGGER IF EXISTS trg_email_body_980dual_bi;
DROP TRIGGER IF EXISTS trg_email_body_980dual_bu;
DROP TRIGGER IF EXISTS trg_email_body_980dual_au;
DROP TRIGGER IF EXISTS trg_email_body_980dual_ad;
DELIMITER $$
CREATE TRIGGER trg_email_body_980dual_bi BEFORE INSERT ON email_body_980dual FOR EACH ROW
BEGIN
    SET NEW.CREATE_BY = IFNULL(NEW.CREATE_BY, LEFT(IFNULL(@app_user, CONCAT('ip:', SUBSTRING_INDEX(USER(), '@', -1))), 20));
    SET NEW.CREATE_DATE = IFNULL(NEW.CREATE_DATE, NOW());
END$$
CREATE TRIGGER trg_email_body_980dual_bu BEFORE UPDATE ON email_body_980dual FOR EACH ROW
BEGIN
    IF NOT (OLD.BODY_ID <=> NEW.BODY_ID) THEN
        SET NEW.UPDATE_BY = LEFT(IFNULL(@app_user, CONCAT('ip:', SUBSTRING_INDEX(USER(), '@', -1))), 20);
        SET NEW.UPDATE_DATE = NOW();
    END IF;
END$$
CREATE TRIGGER trg_email_body_980dual_au AFTER UPDATE ON email_body_980dual FOR EACH ROW
BEGIN
    IF NOT (OLD.BODY_ID <=> NEW.BODY_ID) THEN
        INSERT INTO log_setting_edata (PRODUCT, TABLE_NAME, KEY_ID, CUSTOMER, SPEC, OLD_VALUE, NEW_VALUE, CHANGE_USER, CHANGE_HOST, SOURCE, CHANGE_DATE)
        SELECT '980Dual', 'email_body_980dual', NEW.KEY_ID, ic.Customer, ic.Specification, OLD.BODY_ID, NEW.BODY_ID, NEW.UPDATE_BY, USER(), 'trigger', NOW()
        FROM (SELECT 1) one LEFT JOIN `980Dual`.info_cus_condition ic ON ic.CusCondition_ID = NEW.KEY_ID;
    END IF;
END$$
CREATE TRIGGER trg_email_body_980dual_ad AFTER DELETE ON email_body_980dual FOR EACH ROW
BEGIN
    INSERT INTO log_setting_edata (PRODUCT, TABLE_NAME, KEY_ID, CUSTOMER, SPEC, OLD_VALUE, NEW_VALUE, CHANGE_USER, CHANGE_HOST, SOURCE, CHANGE_DATE)
    SELECT '980Dual', 'email_body_980dual', OLD.KEY_ID, ic.Customer, ic.Specification, OLD.BODY_ID, NULL, LEFT(IFNULL(@app_user, CONCAT('ip:', SUBSTRING_INDEX(USER(), '@', -1))), 20), USER(), 'trigger', NOW()
    FROM (SELECT 1) one LEFT JOIN `980Dual`.info_cus_condition ic ON ic.CusCondition_ID = OLD.KEY_ID;
END$$
DELIMITER ;

-- ---------------- key_send_methods_1480dual (SEND_METHODS_ID) ----------------
CALL tmp_add_column('key_send_methods_1480dual', 'CREATE_BY',   'VARCHAR(20) COLLATE utf8_unicode_ci NULL');
CALL tmp_add_column('key_send_methods_1480dual', 'CREATE_DATE', 'DATETIME NULL');
CALL tmp_add_column('key_send_methods_1480dual', 'UPDATE_BY',   'VARCHAR(20) COLLATE utf8_unicode_ci NULL');
CALL tmp_add_column('key_send_methods_1480dual', 'UPDATE_DATE', 'DATETIME NULL');
DROP TRIGGER IF EXISTS trg_key_send_methods_1480dual_bi;
DROP TRIGGER IF EXISTS trg_key_send_methods_1480dual_bu;
DROP TRIGGER IF EXISTS trg_key_send_methods_1480dual_au;
DROP TRIGGER IF EXISTS trg_key_send_methods_1480dual_ad;
DELIMITER $$
CREATE TRIGGER trg_key_send_methods_1480dual_bi BEFORE INSERT ON key_send_methods_1480dual FOR EACH ROW
BEGIN
    SET NEW.CREATE_BY = IFNULL(NEW.CREATE_BY, LEFT(IFNULL(@app_user, CONCAT('ip:', SUBSTRING_INDEX(USER(), '@', -1))), 20));
    SET NEW.CREATE_DATE = IFNULL(NEW.CREATE_DATE, NOW());
END$$
CREATE TRIGGER trg_key_send_methods_1480dual_bu BEFORE UPDATE ON key_send_methods_1480dual FOR EACH ROW
BEGIN
    IF NOT (OLD.SEND_METHODS_ID <=> NEW.SEND_METHODS_ID) THEN
        SET NEW.UPDATE_BY = LEFT(IFNULL(@app_user, CONCAT('ip:', SUBSTRING_INDEX(USER(), '@', -1))), 20);
        SET NEW.UPDATE_DATE = NOW();
    END IF;
END$$
CREATE TRIGGER trg_key_send_methods_1480dual_au AFTER UPDATE ON key_send_methods_1480dual FOR EACH ROW
BEGIN
    IF NOT (OLD.SEND_METHODS_ID <=> NEW.SEND_METHODS_ID) THEN
        INSERT INTO log_setting_edata (PRODUCT, TABLE_NAME, KEY_ID, CUSTOMER, SPEC, OLD_VALUE, NEW_VALUE, CHANGE_USER, CHANGE_HOST, SOURCE, CHANGE_DATE)
        SELECT '1480Dual', 'key_send_methods_1480dual', NEW.KEY_ID, ic.Customer, ic.Specification, OLD.SEND_METHODS_ID, NEW.SEND_METHODS_ID, NEW.UPDATE_BY, USER(), 'trigger', NOW()
        FROM (SELECT 1) one LEFT JOIN `1480dual`.info_cus_condition ic ON ic.CusCondition_ID = NEW.KEY_ID;
    END IF;
END$$
CREATE TRIGGER trg_key_send_methods_1480dual_ad AFTER DELETE ON key_send_methods_1480dual FOR EACH ROW
BEGIN
    INSERT INTO log_setting_edata (PRODUCT, TABLE_NAME, KEY_ID, CUSTOMER, SPEC, OLD_VALUE, NEW_VALUE, CHANGE_USER, CHANGE_HOST, SOURCE, CHANGE_DATE)
    SELECT '1480Dual', 'key_send_methods_1480dual', OLD.KEY_ID, ic.Customer, ic.Specification, OLD.SEND_METHODS_ID, NULL, LEFT(IFNULL(@app_user, CONCAT('ip:', SUBSTRING_INDEX(USER(), '@', -1))), 20), USER(), 'trigger', NOW()
    FROM (SELECT 1) one LEFT JOIN `1480dual`.info_cus_condition ic ON ic.CusCondition_ID = OLD.KEY_ID;
END$$
DELIMITER ;

-- ---------------- email_attach_1480dual (FILE_FORMATS_ID) ----------------
CALL tmp_add_column('email_attach_1480dual', 'CREATE_BY',   'VARCHAR(20) COLLATE utf8_unicode_ci NULL');
CALL tmp_add_column('email_attach_1480dual', 'CREATE_DATE', 'DATETIME NULL');
CALL tmp_add_column('email_attach_1480dual', 'UPDATE_BY',   'VARCHAR(20) COLLATE utf8_unicode_ci NULL');
CALL tmp_add_column('email_attach_1480dual', 'UPDATE_DATE', 'DATETIME NULL');
DROP TRIGGER IF EXISTS trg_email_attach_1480dual_bi;
DROP TRIGGER IF EXISTS trg_email_attach_1480dual_bu;
DROP TRIGGER IF EXISTS trg_email_attach_1480dual_au;
DROP TRIGGER IF EXISTS trg_email_attach_1480dual_ad;
DELIMITER $$
CREATE TRIGGER trg_email_attach_1480dual_bi BEFORE INSERT ON email_attach_1480dual FOR EACH ROW
BEGIN
    SET NEW.CREATE_BY = IFNULL(NEW.CREATE_BY, LEFT(IFNULL(@app_user, CONCAT('ip:', SUBSTRING_INDEX(USER(), '@', -1))), 20));
    SET NEW.CREATE_DATE = IFNULL(NEW.CREATE_DATE, NOW());
END$$
CREATE TRIGGER trg_email_attach_1480dual_bu BEFORE UPDATE ON email_attach_1480dual FOR EACH ROW
BEGIN
    IF NOT (OLD.FILE_FORMATS_ID <=> NEW.FILE_FORMATS_ID) THEN
        SET NEW.UPDATE_BY = LEFT(IFNULL(@app_user, CONCAT('ip:', SUBSTRING_INDEX(USER(), '@', -1))), 20);
        SET NEW.UPDATE_DATE = NOW();
    END IF;
END$$
CREATE TRIGGER trg_email_attach_1480dual_au AFTER UPDATE ON email_attach_1480dual FOR EACH ROW
BEGIN
    IF NOT (OLD.FILE_FORMATS_ID <=> NEW.FILE_FORMATS_ID) THEN
        INSERT INTO log_setting_edata (PRODUCT, TABLE_NAME, KEY_ID, CUSTOMER, SPEC, OLD_VALUE, NEW_VALUE, CHANGE_USER, CHANGE_HOST, SOURCE, CHANGE_DATE)
        SELECT '1480Dual', 'email_attach_1480dual', NEW.KEY_ID, ic.Customer, ic.Specification, OLD.FILE_FORMATS_ID, NEW.FILE_FORMATS_ID, NEW.UPDATE_BY, USER(), 'trigger', NOW()
        FROM (SELECT 1) one LEFT JOIN `1480dual`.info_cus_condition ic ON ic.CusCondition_ID = NEW.KEY_ID;
    END IF;
END$$
CREATE TRIGGER trg_email_attach_1480dual_ad AFTER DELETE ON email_attach_1480dual FOR EACH ROW
BEGIN
    INSERT INTO log_setting_edata (PRODUCT, TABLE_NAME, KEY_ID, CUSTOMER, SPEC, OLD_VALUE, NEW_VALUE, CHANGE_USER, CHANGE_HOST, SOURCE, CHANGE_DATE)
    SELECT '1480Dual', 'email_attach_1480dual', OLD.KEY_ID, ic.Customer, ic.Specification, OLD.FILE_FORMATS_ID, NULL, LEFT(IFNULL(@app_user, CONCAT('ip:', SUBSTRING_INDEX(USER(), '@', -1))), 20), USER(), 'trigger', NOW()
    FROM (SELECT 1) one LEFT JOIN `1480dual`.info_cus_condition ic ON ic.CusCondition_ID = OLD.KEY_ID;
END$$
DELIMITER ;

-- ---------------- key_group_email_1480dual (GROUP_ID) ----------------
CALL tmp_add_column('key_group_email_1480dual', 'CREATE_BY',   'VARCHAR(20) COLLATE utf8_unicode_ci NULL');
CALL tmp_add_column('key_group_email_1480dual', 'CREATE_DATE', 'DATETIME NULL');
CALL tmp_add_column('key_group_email_1480dual', 'UPDATE_BY',   'VARCHAR(20) COLLATE utf8_unicode_ci NULL');
CALL tmp_add_column('key_group_email_1480dual', 'UPDATE_DATE', 'DATETIME NULL');
DROP TRIGGER IF EXISTS trg_key_group_email_1480dual_bi;
DROP TRIGGER IF EXISTS trg_key_group_email_1480dual_bu;
DROP TRIGGER IF EXISTS trg_key_group_email_1480dual_au;
DROP TRIGGER IF EXISTS trg_key_group_email_1480dual_ad;
DELIMITER $$
CREATE TRIGGER trg_key_group_email_1480dual_bi BEFORE INSERT ON key_group_email_1480dual FOR EACH ROW
BEGIN
    SET NEW.CREATE_BY = IFNULL(NEW.CREATE_BY, LEFT(IFNULL(@app_user, CONCAT('ip:', SUBSTRING_INDEX(USER(), '@', -1))), 20));
    SET NEW.CREATE_DATE = IFNULL(NEW.CREATE_DATE, NOW());
END$$
CREATE TRIGGER trg_key_group_email_1480dual_bu BEFORE UPDATE ON key_group_email_1480dual FOR EACH ROW
BEGIN
    IF NOT (OLD.GROUP_ID <=> NEW.GROUP_ID) THEN
        SET NEW.UPDATE_BY = LEFT(IFNULL(@app_user, CONCAT('ip:', SUBSTRING_INDEX(USER(), '@', -1))), 20);
        SET NEW.UPDATE_DATE = NOW();
    END IF;
END$$
CREATE TRIGGER trg_key_group_email_1480dual_au AFTER UPDATE ON key_group_email_1480dual FOR EACH ROW
BEGIN
    IF NOT (OLD.GROUP_ID <=> NEW.GROUP_ID) THEN
        INSERT INTO log_setting_edata (PRODUCT, TABLE_NAME, KEY_ID, CUSTOMER, SPEC, OLD_VALUE, NEW_VALUE, CHANGE_USER, CHANGE_HOST, SOURCE, CHANGE_DATE)
        SELECT '1480Dual', 'key_group_email_1480dual', NEW.KEY_ID, ic.Customer, ic.Specification, OLD.GROUP_ID, NEW.GROUP_ID, NEW.UPDATE_BY, USER(), 'trigger', NOW()
        FROM (SELECT 1) one LEFT JOIN `1480dual`.info_cus_condition ic ON ic.CusCondition_ID = NEW.KEY_ID;
    END IF;
END$$
CREATE TRIGGER trg_key_group_email_1480dual_ad AFTER DELETE ON key_group_email_1480dual FOR EACH ROW
BEGIN
    INSERT INTO log_setting_edata (PRODUCT, TABLE_NAME, KEY_ID, CUSTOMER, SPEC, OLD_VALUE, NEW_VALUE, CHANGE_USER, CHANGE_HOST, SOURCE, CHANGE_DATE)
    SELECT '1480Dual', 'key_group_email_1480dual', OLD.KEY_ID, ic.Customer, ic.Specification, OLD.GROUP_ID, NULL, LEFT(IFNULL(@app_user, CONCAT('ip:', SUBSTRING_INDEX(USER(), '@', -1))), 20), USER(), 'trigger', NOW()
    FROM (SELECT 1) one LEFT JOIN `1480dual`.info_cus_condition ic ON ic.CusCondition_ID = OLD.KEY_ID;
END$$
DELIMITER ;

-- ---------------- email_subject_1480dual (SUBJECT_ID) ----------------
CALL tmp_add_column('email_subject_1480dual', 'CREATE_BY',   'VARCHAR(20) COLLATE utf8_unicode_ci NULL');
CALL tmp_add_column('email_subject_1480dual', 'CREATE_DATE', 'DATETIME NULL');
CALL tmp_add_column('email_subject_1480dual', 'UPDATE_BY',   'VARCHAR(20) COLLATE utf8_unicode_ci NULL');
CALL tmp_add_column('email_subject_1480dual', 'UPDATE_DATE', 'DATETIME NULL');
DROP TRIGGER IF EXISTS trg_email_subject_1480dual_bi;
DROP TRIGGER IF EXISTS trg_email_subject_1480dual_bu;
DROP TRIGGER IF EXISTS trg_email_subject_1480dual_au;
DROP TRIGGER IF EXISTS trg_email_subject_1480dual_ad;
DELIMITER $$
CREATE TRIGGER trg_email_subject_1480dual_bi BEFORE INSERT ON email_subject_1480dual FOR EACH ROW
BEGIN
    SET NEW.CREATE_BY = IFNULL(NEW.CREATE_BY, LEFT(IFNULL(@app_user, CONCAT('ip:', SUBSTRING_INDEX(USER(), '@', -1))), 20));
    SET NEW.CREATE_DATE = IFNULL(NEW.CREATE_DATE, NOW());
END$$
CREATE TRIGGER trg_email_subject_1480dual_bu BEFORE UPDATE ON email_subject_1480dual FOR EACH ROW
BEGIN
    IF NOT (OLD.SUBJECT_ID <=> NEW.SUBJECT_ID) THEN
        SET NEW.UPDATE_BY = LEFT(IFNULL(@app_user, CONCAT('ip:', SUBSTRING_INDEX(USER(), '@', -1))), 20);
        SET NEW.UPDATE_DATE = NOW();
    END IF;
END$$
CREATE TRIGGER trg_email_subject_1480dual_au AFTER UPDATE ON email_subject_1480dual FOR EACH ROW
BEGIN
    IF NOT (OLD.SUBJECT_ID <=> NEW.SUBJECT_ID) THEN
        INSERT INTO log_setting_edata (PRODUCT, TABLE_NAME, KEY_ID, CUSTOMER, SPEC, OLD_VALUE, NEW_VALUE, CHANGE_USER, CHANGE_HOST, SOURCE, CHANGE_DATE)
        SELECT '1480Dual', 'email_subject_1480dual', NEW.KEY_ID, ic.Customer, ic.Specification, OLD.SUBJECT_ID, NEW.SUBJECT_ID, NEW.UPDATE_BY, USER(), 'trigger', NOW()
        FROM (SELECT 1) one LEFT JOIN `1480dual`.info_cus_condition ic ON ic.CusCondition_ID = NEW.KEY_ID;
    END IF;
END$$
CREATE TRIGGER trg_email_subject_1480dual_ad AFTER DELETE ON email_subject_1480dual FOR EACH ROW
BEGIN
    INSERT INTO log_setting_edata (PRODUCT, TABLE_NAME, KEY_ID, CUSTOMER, SPEC, OLD_VALUE, NEW_VALUE, CHANGE_USER, CHANGE_HOST, SOURCE, CHANGE_DATE)
    SELECT '1480Dual', 'email_subject_1480dual', OLD.KEY_ID, ic.Customer, ic.Specification, OLD.SUBJECT_ID, NULL, LEFT(IFNULL(@app_user, CONCAT('ip:', SUBSTRING_INDEX(USER(), '@', -1))), 20), USER(), 'trigger', NOW()
    FROM (SELECT 1) one LEFT JOIN `1480dual`.info_cus_condition ic ON ic.CusCondition_ID = OLD.KEY_ID;
END$$
DELIMITER ;

-- ---------------- email_body_1480dual (BODY_ID) ----------------
CALL tmp_add_column('email_body_1480dual', 'CREATE_BY',   'VARCHAR(20) COLLATE utf8_unicode_ci NULL');
CALL tmp_add_column('email_body_1480dual', 'CREATE_DATE', 'DATETIME NULL');
CALL tmp_add_column('email_body_1480dual', 'UPDATE_BY',   'VARCHAR(20) COLLATE utf8_unicode_ci NULL');
CALL tmp_add_column('email_body_1480dual', 'UPDATE_DATE', 'DATETIME NULL');
DROP TRIGGER IF EXISTS trg_email_body_1480dual_bi;
DROP TRIGGER IF EXISTS trg_email_body_1480dual_bu;
DROP TRIGGER IF EXISTS trg_email_body_1480dual_au;
DROP TRIGGER IF EXISTS trg_email_body_1480dual_ad;
DELIMITER $$
CREATE TRIGGER trg_email_body_1480dual_bi BEFORE INSERT ON email_body_1480dual FOR EACH ROW
BEGIN
    SET NEW.CREATE_BY = IFNULL(NEW.CREATE_BY, LEFT(IFNULL(@app_user, CONCAT('ip:', SUBSTRING_INDEX(USER(), '@', -1))), 20));
    SET NEW.CREATE_DATE = IFNULL(NEW.CREATE_DATE, NOW());
END$$
CREATE TRIGGER trg_email_body_1480dual_bu BEFORE UPDATE ON email_body_1480dual FOR EACH ROW
BEGIN
    IF NOT (OLD.BODY_ID <=> NEW.BODY_ID) THEN
        SET NEW.UPDATE_BY = LEFT(IFNULL(@app_user, CONCAT('ip:', SUBSTRING_INDEX(USER(), '@', -1))), 20);
        SET NEW.UPDATE_DATE = NOW();
    END IF;
END$$
CREATE TRIGGER trg_email_body_1480dual_au AFTER UPDATE ON email_body_1480dual FOR EACH ROW
BEGIN
    IF NOT (OLD.BODY_ID <=> NEW.BODY_ID) THEN
        INSERT INTO log_setting_edata (PRODUCT, TABLE_NAME, KEY_ID, CUSTOMER, SPEC, OLD_VALUE, NEW_VALUE, CHANGE_USER, CHANGE_HOST, SOURCE, CHANGE_DATE)
        SELECT '1480Dual', 'email_body_1480dual', NEW.KEY_ID, ic.Customer, ic.Specification, OLD.BODY_ID, NEW.BODY_ID, NEW.UPDATE_BY, USER(), 'trigger', NOW()
        FROM (SELECT 1) one LEFT JOIN `1480dual`.info_cus_condition ic ON ic.CusCondition_ID = NEW.KEY_ID;
    END IF;
END$$
CREATE TRIGGER trg_email_body_1480dual_ad AFTER DELETE ON email_body_1480dual FOR EACH ROW
BEGIN
    INSERT INTO log_setting_edata (PRODUCT, TABLE_NAME, KEY_ID, CUSTOMER, SPEC, OLD_VALUE, NEW_VALUE, CHANGE_USER, CHANGE_HOST, SOURCE, CHANGE_DATE)
    SELECT '1480Dual', 'email_body_1480dual', OLD.KEY_ID, ic.Customer, ic.Specification, OLD.BODY_ID, NULL, LEFT(IFNULL(@app_user, CONCAT('ip:', SUBSTRING_INDEX(USER(), '@', -1))), 20), USER(), 'trigger', NOW()
    FROM (SELECT 1) one LEFT JOIN `1480dual`.info_cus_condition ic ON ic.CusCondition_ID = OLD.KEY_ID;
END$$
DELIMITER ;

DROP PROCEDURE IF EXISTS tmp_add_column;
