using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using DbUp;
using DbUp.Helpers;

namespace DbUpConsole
{
    class Program
    {
        const string master = "master";
        const string edFiApplicationDatabase = "EdFi_ApplicationTest";
        const string edFiDashboardDatabase = "EdFi_DashboardTest";
        const string edFiDashboardDwDatabase = "EdFi_DashboardDWTest";

        static int Main(string[] args)
        {
            // Drop Databases
            var connectionString = string.Format("Server=(local); Database={0}; Trusted_connection=true", master);
            var fileSystemPath = @"C:\Users\david.doyle\Documents\DLP SQL\Drop Databases";
            if (!RunScripts(connectionString, fileSystemPath))
            {
                return -1;
            }

            // Remove Database Files
            RemoveDatabaseFiles();

            // Create Databases
            fileSystemPath = @"C:\Users\david.doyle\Documents\DLP SQL\Create Databases";
            if (!RunScripts(connectionString, fileSystemPath))
            {
                return -1;
            }

            // Create Application Database Tables
            connectionString = string.Format("Server=(local); Database={0}; Trusted_connection=true", edFiApplicationDatabase);
            fileSystemPath = @"D:\Projects\FloridaDOE\Ed-Fi-Core\Database\Structure\Application";
            if (!RunScripts(connectionString, fileSystemPath))
            {
                return -1;
            }

            // Create Application Database Data
            fileSystemPath = @"C:\Users\david.doyle\Documents\DLP SQL\Create Data\EdFi Application Data Update.sql";
            if (!RunScripts(connectionString, fileSystemPath))
            {
                return -1;
            }

            // Create Dashboard Database Tables
            connectionString = string.Format("Server=(local); Database={0}; Trusted_connection=true", edFiDashboardDatabase);
            fileSystemPath = @"D:\Projects\FloridaDOE\Ed-Fi-Core\Database\Structure\Dashboard";
            if (!RunScripts(connectionString, fileSystemPath))
            {
                return -1;
            }

            // Create Dashboard Database Data
            fileSystemPath = @"C:\Users\david.doyle\Documents\DLP SQL\Create Data\EdFi Dashboard Data Update.sql";
            if (!RunScripts(connectionString, fileSystemPath))
            {
                return -1;
            }

            // Create Dashboard Data Warehouse Database Tables
            connectionString = string.Format("Server=(local); Database={0}; Trusted_connection=true", edFiDashboardDwDatabase);
            fileSystemPath = @"D:\Projects\FloridaDOE\Ed-Fi-Core\Database\Structure\DashboardDW";
            if (!RunScripts(connectionString, fileSystemPath))
            {
                return -1;
            }

            // Create Dashboard Data Warehouse Database Data
            fileSystemPath = @"C:\Users\david.doyle\Documents\DLP SQL\Create Data\EdFi DashboardDW Data Update.sql";
            if (!RunScripts(connectionString, fileSystemPath))
            {
                return -1;
            }

            // TODO: Run PostLoad files if needed
            // TODO: Run any plugin files you may have

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Success!");
            Console.ResetColor();

            Console.ReadLine();
            return 0;
        }

        private static bool RunScripts(string connectionString, string fileSystemPath)
        {
            Console.WriteLine("Script Batch Start Time: {0}", DateTime.Now);
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var upgrader =
                DeployChanges.To
                    .SqlDatabase(connectionString)
                    .JournalTo(new NullJournal())
                    .WithScriptsFromFileSystem(fileSystemPath)
                    .WithExecutionTimeout(new TimeSpan(0, 60, 0))
                    .LogToConsole()
                    .Build();

            var result = upgrader.PerformUpgrade();

            if (result.Successful)
            {
                stopwatch.Stop();
                Console.WriteLine("Script End Time: {0}", DateTime.Now);
                Console.WriteLine("Time Elapsed: {0}", stopwatch.Elapsed);
                return true;
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(DateTime.Now);
            Console.WriteLine(result.Error);
            Console.ResetColor();
            Console.ReadLine();
            return false;
        }

        private static void RemoveDatabaseFiles()
        {
            var databaseFiles = new List<string>
            {
                Path.Combine(@"C:\Program Files\Microsoft SQL Server\MSSQL11.MSSQLSERVER\MSSQL\DATA\",
                    string.Format("{0}.mdf", edFiApplicationDatabase)),
                Path.Combine(@"C:\Program Files\Microsoft SQL Server\MSSQL11.MSSQLSERVER\MSSQL\DATA\",
                    string.Format("{0}_log.ldf", edFiApplicationDatabase)),
                Path.Combine(@"C:\Program Files\Microsoft SQL Server\MSSQL11.MSSQLSERVER\MSSQL\DATA\",
                    string.Format("{0}.mdf", edFiDashboardDatabase)),
                Path.Combine(@"C:\Program Files\Microsoft SQL Server\MSSQL11.MSSQLSERVER\MSSQL\DATA\",
                    string.Format("{0}_log.ldf", edFiDashboardDatabase)),
                Path.Combine(@"C:\Program Files\Microsoft SQL Server\MSSQL11.MSSQLSERVER\MSSQL\DATA\",
                    string.Format("{0}.mdf", edFiDashboardDwDatabase)),
                Path.Combine(@"C:\Program Files\Microsoft SQL Server\MSSQL11.MSSQLSERVER\MSSQL\DATA\",
                    string.Format("{0}_log.ldf", edFiDashboardDwDatabase)),
            };

            foreach (var databaseFile in databaseFiles)
            {
                if (File.Exists(databaseFile))
                {
                    File.Delete(databaseFile);
                }
            }
        }
    }
}
