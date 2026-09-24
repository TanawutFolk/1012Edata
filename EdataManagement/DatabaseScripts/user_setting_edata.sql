-- ============================================================
-- user_setting_edata : who may open the Setting screen of
--                      "1012 Send E-data 48-98 Direct"
--
-- This is a WHITELIST (not an override table): an employee can open
-- Setting only when EMPCODE is here and INUSE = 1.
--
-- Login runs in two steps (frmLogin):
--   1) EMPCODE + password against the Security Center API (portal password)
--      -> switched OFF for now, the code is in SecurityCenterControllers
--         and the call is commented out in frmLogin. No password is stored here.
--   2) EMPCODE must exist in mfg.info_employees (and not be resigned)
--      and be listed in this table with INUSE = 1 -> gives ROLE.
--
-- ROLE
--   ADMIN : everything, including the Login Permission tab (manage this table)
--   USER  : email recipients / groups only, no Permission tab
--
-- Database: qa_oc_test  (App.config -> ConnectionQAOC)
-- Server  : 192.168.2.99
--
-- COLLATION NOTE: EMPCODE must stay utf8_unicode_ci to match
-- mfg.info_employees.EmpCode, otherwise the JOIN fails with
-- ERROR 1267 Illegal mix of collations.
-- ============================================================
CREATE TABLE IF NOT EXISTS user_setting_edata (
    ID          INT          NOT NULL AUTO_INCREMENT,
    EMPCODE     VARCHAR(20)  COLLATE utf8_unicode_ci NOT NULL,              -- employee code = login user
    ROLE        VARCHAR(20)  COLLATE utf8_unicode_ci NOT NULL DEFAULT 'USER', -- ADMIN / USER
    INUSE       TINYINT(1)   NOT NULL DEFAULT 1,                            -- 0 = revoked, keeps the history row
    REMARK      VARCHAR(255) COLLATE utf8_unicode_ci NULL,
    CREATE_USER VARCHAR(20)  COLLATE utf8_unicode_ci NULL,
    CREATE_DATE DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UPDATE_DATE DATETIME     NULL,
    PRIMARY KEY (ID),
    UNIQUE KEY uq_user_setting_edata_empcode (EMPCODE),
    KEY idx_user_setting_edata_role (ROLE)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;

-- first ADMIN, otherwise nobody can open Setting to grant anyone else
INSERT INTO user_setting_edata (EMPCODE, ROLE, REMARK, CREATE_USER)
VALUES ('S00823', 'ADMIN', 'initial admin', 'setup')
ON DUPLICATE KEY UPDATE ROLE = 'ADMIN', INUSE = 1, UPDATE_DATE = NOW();

-- day to day:
--   grant  : INSERT INTO user_setting_edata (EMPCODE, ROLE, CREATE_USER) VALUES ('S00616', 'USER', 'S00823');
--   revoke : UPDATE user_setting_edata SET INUSE = 0, UPDATE_DATE = NOW() WHERE EMPCODE = 'S00616';
--   promote: UPDATE user_setting_edata SET ROLE = 'ADMIN', UPDATE_DATE = NOW() WHERE EMPCODE = 'S00616';
