-- Names of the recipient groups, shown only by the Setting page of 1012 (ucSpecSetting).
-- One row per named group; a group without a row has no name and shows as "#12" like before.
-- info_group_email (one row per address, also used by uITLA / EdataWeb / 1012 v3.6.0) is not touched,
-- and the sending side never reads this table.
-- Run on the config database: qa_oc_test first, qa_oc only when the new 1012 goes to production.
-- Re-runnable.

CREATE TABLE IF NOT EXISTS info_group_name (
    GROUP_ID    INT(11)      NOT NULL,
    GROUP_NAME  VARCHAR(100) NOT NULL,
    CREATE_BY   VARCHAR(20)  NULL,
    CREATE_DATE DATETIME     NULL DEFAULT CURRENT_TIMESTAMP,
    UPDATE_BY   VARCHAR(20)  NULL,
    UPDATE_DATE DATETIME     NULL,
    PRIMARY KEY (GROUP_ID)
) ENGINE = InnoDB DEFAULT CHARSET = utf8 COLLATE = utf8_unicode_ci;
