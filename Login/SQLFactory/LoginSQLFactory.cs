using System;
using Login.Property;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Login.SQLFactory
{
    public class LoginSQLFactory
    {
        private string sql;
        public string SearchEmpCompany(EmpProperty dataItem)
        {
            sql = @"SELECT EmpCode AS EMP_ID
                        ,IF(EmpCodeResign IS NULL,'1','0') AS EMP_STATUS
                        ,EmpName AS EMP_NAME
                        ,EmpSurname AS EMP_SURNAME 
                        ,EmpEmail AS EMP_EMAIL
                        ,EmpStartwork AS START_WORK
						,EmpPosition AS `POSITION`
						,EmpDept AS `DEPARTMENT`

                    FROM `info_employees`
                    WHERE empCode = '" + dataItem.code + @"';";

            return sql;
        }

        public string SearchEmpFitelGroup(EmpProperty dataItem)
        {
            sql = @"SELECT *
                    FROM `logon_fitel_group`
                    WHERE EMP_ID = '" + dataItem.code + @"';";

            return sql;
        }

        public string LoginFitelGroup(EmpProperty dataItem)
        {
            sql = @"SELECT*
                    FROM `logon_fitel_group`
                    WHERE EMP_ID = '"+ dataItem.code + @"'
                    AND `PASSWORD` = UNHEX(MD5('" + dataItem.pass + "'))";

            return sql;
        }

        public string InsertLoginFitelGroup(EmpProperty dataItem)
        {
            sql = @"INSERT INTO `logon_fitel_group` (
                        `EMP_ID`
                        , `PASSWORD`
                        , `CREATE_ACC_DATE`
                    )
                    VALUES(
                        '"+ dataItem.code + @"'
                        ,UNHEX(MD5('" + dataItem.pass + @"'))
                        , NOW()
                    );";

            return sql;
        }

        public string UpdatePassLoginFitelGroup(EmpProperty dataItem)
        {
            sql = @"UPDATE `logon_fitel_group` SET 
                    `PASSWORD` = UNHEX(MD5('" + dataItem.pass + @"'))
                    WHERE `EMP_ID` = '" + dataItem.code + @"'";
             
            return sql;
        }

        public string GetDatatime()
        {
            sql = @"SELECT NOW() AS DATETIME_NOW;";
            return sql;
        }
        public string InsertOTP(EmpProperty emp, string OTP)
        {
            sql = @"INSERT INTO `otp` (
                        EMP_ID
                        ,USED
                        ,OTP
                        ,EXP_OTP
                    )
                    VALUES(
                         '" + emp.code + @"'
                        ,0
                        ,'" + OTP + @"'
                        ,NOW() + INTERVAL 3 MINUTE
                    );";
            return sql;
        }

        public string DeleteOTP(EmpProperty emp)
        {
            sql = @"DELETE
                    FROM `otp`
                    WHERE EMP_ID = '" + emp.code + @"'
                    OR USED = 1
                    OR EXP_OTP < NOW();";
            return sql;
        }

        public string SearchOTP(EmpProperty emp, string OTP)
        {
            sql = @"SELECT EMP_ID 
                        ,USED
                        ,OTP
                        ,EXP_OTP

                    FROM `otp`
                    WHERE EMP_ID = '" + emp.code + @"'
                    AND OTP = '" + OTP + @"'";
            return sql;
        }

        public string UpdateOTPUsed(EmpProperty emp, string OTP)
        {
            sql = @"UPDATE `otp`  SET USED  = 1
                    WHERE EMP_ID = '" + emp.code + @"'
                    AND OTP = '" + OTP + @"'";

            return sql;
        }

    }
}
