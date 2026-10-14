using System;
using System.Configuration;
using System.Data.Common;
using System.Diagnostics;

namespace EdataManagement.Utilities
{
    internal static class DatabaseSelection
    {
        public const string TestDatabase = "qa_oc_test";
        public const string ProductionDatabase = "qa_oc";

        public static string CurrentDatabase
        {
            get
            {
                string connection = ConfigurationManager.ConnectionStrings["ConnectionQAOC"].ConnectionString;
                DbConnectionStringBuilder builder = new DbConnectionStringBuilder { ConnectionString = connection };
                return builder["Database"].ToString().Trim();
            }
        }

        public static void SaveAndRestart(string databaseName)
        {
            if (databaseName != TestDatabase && databaseName != ProductionDatabase)
            {
                throw new ArgumentException("Unsupported database.", "databaseName");
            }

            Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            SetDatabase(configuration.ConnectionStrings.ConnectionStrings["ConnectionQAOC"], databaseName);
            SetDatabase(configuration.ConnectionStrings.ConnectionStrings["ConnectionStandAlone"], databaseName);
            configuration.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("connectionStrings");

            string executable = Process.GetCurrentProcess().MainModule.FileName;
            Process.Start(executable);
            Environment.Exit(0);
        }

        private static void SetDatabase(ConnectionStringSettings settings, string databaseName)
        {
            if (settings == null)
            {
                throw new ConfigurationErrorsException("Required connection string is missing.");
            }

            DbConnectionStringBuilder builder = new DbConnectionStringBuilder
            {
                ConnectionString = settings.ConnectionString
            };
            builder["Database"] = databaseName;
            settings.ConnectionString = builder.ConnectionString;
        }
    }
}
