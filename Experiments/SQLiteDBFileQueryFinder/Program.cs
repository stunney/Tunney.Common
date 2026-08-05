using System;
using System.Collections.Generic;
using System.IO;
using Tunney.Common.Data;

namespace SQLiteDBFileQueryFinder
{
    public class Program
    {
        public static void Main(string[] args)
        {
            DirectoryInfo di = new DirectoryInfo(args[0]);
            string sql = args[1];

            foreach (FileInfo fi in di.GetFiles(@"*.db3"))
            {
                SQLiteDataStore ds = new SQLiteDataStore(new System.Data.SQLite.SQLiteConnection(string.Format(SQLiteDataStore.SQLITE_CONNECTION_STRING_FORMAT, fi.FullName)));

                long ret = 0;

                try
                {
                    ret = (long)ds.ExecuteScalar(sql, false);
                }
                catch (Exception _ex)
                {
                    if (_ex.Message.ToLower().Contains(@"no such table"))
                    {
                        ret = 0;
                    }
                    else throw _ex;
                }

                if (0 < ret)
                {
                    fi.CopyTo(string.Format(@"C:\Users\stunney\Desktop\Test\Valid\{0}", fi.Name), true);
                    Console.WriteLine(@"File [{0}] has found a match.", fi.FullName);
                }                    
            }
        }
    }
}
