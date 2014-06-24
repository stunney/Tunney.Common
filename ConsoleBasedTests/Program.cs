using System;

using System.Collections.Generic;

using Tunney.Common.Data;
using Tunney.Common.Data.SQLite;

namespace ConsoleBasedTests
{
    class Program
    {
        static void Main(string[] args)
        {
            SQLiteBackupProvider backupProvider = new SQLiteBackupProvider();

            int errorCode = -1;

            SQLiteDataStore ds = new SQLiteDataStore(backupProvider.RestoreDatabaseFromFile(@"D:\Precache.db3", out errorCode));
            ds.Vacuum();

            long val = (long)ds.ExecuteScalar(@"SELECT COUNT(*) FROM BlockedIPs", false);
            val = (long)ds.ExecuteScalar(@"SELECT COUNT(*) FROM RawSessions_Naked_Extended", false);

            Console.ReadLine();
        }
    }
}
