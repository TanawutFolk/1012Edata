using EdataManagement.Property;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdataManagement.SQLFactory
{
    /// <summary>
    /// SQL for login and for the Login Permission tab.
    /// user_setting_edata lives in the connection database (qa_oc_test),
    /// employee data in mfg.info_employees on the same server, so both are joined directly.
    /// </summary>
    public class SettingUserSQLFactory
    {
        private string sql;

        private string Esc(string value)
        {
            if (value == null) return "";
            return value.Replace("\\", "\\\\").Replace("'", "''");
        }

        /// <summary>
        /// One employee + the permission row, used by frmLogin.
        /// No row back = employee code does not exist at all.
        /// ROLE empty = employee exists but is not allowed into Setting.
        /// </summary>
        public string searchUserForLogin(SettingUserProperty dataItem)
        {
            sql = "SELECT e.EmpCode AS `EMPCODE` " +
                  ", IFNULL(e.EmpName, '') AS `EMP_NAME` " +
                  ", IFNULL(e.EmpSurname, '') AS `EMP_SURNAME` " +
                  ", IFNULL(e.EmpDept, '') AS `EMP_DEPT` " +
                  ", IFNULL(e.EmpSection, '') AS `EMP_SECTION` " +
                  ", IFNULL(e.EmpEmail, '') AS `EMP_EMAIL` " +
                  ", IF(e.EmpCodeResign IS NULL, 'ACTIVE', 'RESIGNED') AS `EMP_STATUS` " +
                  ", IFNULL(u.ROLE, '') AS `ROLE` " +
                  ", IFNULL(u.INUSE, 0) AS `INUSE` " +
                  "FROM mfg.info_employees e " +
                  "LEFT JOIN user_setting_edata u ON (u.EMPCODE = e.EmpCode AND u.INUSE = 1) " +
                  "WHERE e.EmpCode = '" + Esc(dataItem.empCode) + "' " +
                  "LIMIT 1 ";

            return sql;
        }

        /// <summary>Everyone registered in the whitelist - the Login Permission grid.</summary>
        public string searchAllUsers()
        {
            sql = "SELECT u.EMPCODE " +
                  ", TRIM(CONCAT(IFNULL(e.EmpName, ''), ' ', IFNULL(e.EmpSurname, ''))) AS `EMP_NAME` " +
                  ", IFNULL(e.EmpDept, '-') AS `EMP_DEPT` " +
                  ", u.ROLE " +
                  ", IF(u.INUSE = 1, 'ACTIVE', 'REVOKED') AS `STATUS` " +
                  ", IF(e.EmpCode IS NULL, 'NOT FOUND', IF(e.EmpCodeResign IS NULL, 'ACTIVE', 'RESIGNED')) AS `EMP_STATUS` " +
                  ", IFNULL(u.REMARK, '') AS `REMARK` " +
                  ", u.INUSE " +
                  "FROM user_setting_edata u " +
                  "LEFT JOIN mfg.info_employees e ON (e.EmpCode = u.EMPCODE) " +
                  "ORDER BY u.INUSE DESC, u.ROLE ASC, u.EMPCODE ASC ";

            return sql;
        }

        /// <summary>Employee lookup before granting, so a typo is caught straight away.</summary>
        public string searchEmployee(SettingUserProperty dataItem)
        {
            sql = "SELECT e.EmpCode AS `EMPCODE` " +
                  ", IFNULL(e.EmpName, '') AS `EMP_NAME` " +
                  ", IFNULL(e.EmpSurname, '') AS `EMP_SURNAME` " +
                  ", IFNULL(e.EmpDept, '') AS `EMP_DEPT` " +
                  ", IFNULL(e.EmpSection, '') AS `EMP_SECTION` " +
                  ", IF(e.EmpCodeResign IS NULL, 'ACTIVE', 'RESIGNED') AS `EMP_STATUS` " +
                  "FROM mfg.info_employees e " +
                  "WHERE e.EmpCode = '" + Esc(dataItem.empCode) + "' " +
                  "LIMIT 1 ";

            return sql;
        }

        public string searchUserRow(SettingUserProperty dataItem)
        {
            sql = "SELECT EMPCODE, ROLE, INUSE FROM user_setting_edata " +
                  "WHERE EMPCODE = '" + Esc(dataItem.empCode) + "' ";

            return sql;
        }

        /// <summary>Grant - a revoked row is reactivated instead of duplicated.</summary>
        public string insertUser(SettingUserProperty dataItem)
        {
            sql = "INSERT INTO user_setting_edata (EMPCODE, ROLE, INUSE, REMARK, CREATE_USER) " +
                  "VALUES ('" + Esc(dataItem.empCode) + "', '" + Esc(dataItem.role) + "', 1, '" + Esc(dataItem.remark) + "', '" + Esc(dataItem.createUser) + "') " +
                  "ON DUPLICATE KEY UPDATE ROLE = '" + Esc(dataItem.role) + "', INUSE = 1, UPDATE_DATE = NOW() ";

            return sql;
        }

        public string updateUserRole(SettingUserProperty dataItem)
        {
            sql = "UPDATE user_setting_edata SET ROLE = '" + Esc(dataItem.role) + "', UPDATE_DATE = NOW() " +
                  "WHERE EMPCODE = '" + Esc(dataItem.empCode) + "' ";

            return sql;
        }

        /// <summary>Revoke / restore. The row is kept so the history stays.</summary>
        public string updateUserInuse(SettingUserProperty dataItem)
        {
            sql = "UPDATE user_setting_edata SET INUSE = " + dataItem.inuse + ", UPDATE_DATE = NOW() " +
                  "WHERE EMPCODE = '" + Esc(dataItem.empCode) + "' ";

            return sql;
        }

        /// <summary>Guard: never leave the screen without an active ADMIN.</summary>
        public string countActiveAdmins(SettingUserProperty dataItem)
        {
            sql = "SELECT COUNT(*) AS `ADMIN_COUNT` FROM user_setting_edata " +
                  "WHERE ROLE = 'ADMIN' AND INUSE = 1 AND EMPCODE <> '" + Esc(dataItem.empCode) + "' ";

            return sql;
        }
    }
}
