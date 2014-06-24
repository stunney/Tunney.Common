using System;
using System.Runtime.InteropServices;
using System.Text;

using System.Data.SQLite;

namespace Tunney.Common.Data.SQLite
{
    public class SQLiteBackupProvider
    {
        private const string SQLITE_DLL_FILENAME = "System.Data.SQLite.DLL";

        [DllImport(SQLITE_DLL_FILENAME, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_open(byte[] filename, out IntPtr db);

        [DllImport(SQLITE_DLL_FILENAME, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_close(IntPtr db);

        [DllImport(SQLITE_DLL_FILENAME, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr sqlite3_backup_init(IntPtr destDb, byte[] destname, IntPtr srcDB, byte[] srcname);

        [DllImport(SQLITE_DLL_FILENAME, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_backup_step(IntPtr backup, int pages);

        [DllImport(SQLITE_DLL_FILENAME, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_backup_finish(IntPtr backup);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_filename"></param>
        /// <param name="_errorCode"></param>
        /// <returns></returns>
        public virtual SQLiteConnection RestoreDatabaseFromFile(string _filename, out int _errorCode)
        {
            _errorCode = (int)SQLiteErrorCode.Ok;

            SQLiteConnection inMemoryConnection = new SQLiteConnection(string.Format(SQLiteDataStore.SQLITE_CONNECTION_STRING_FORMAT, SQLiteDataStore.SQLITE_IN_MEMORY_CONNECTION_NAME));
            inMemoryConnection.Open();

            using (SQLiteCommand cmd = inMemoryConnection.CreateCommand())
            {
                cmd.CommandText = string.Format(@"ATTACH DATABASE '{0}' as disk", _filename);
                cmd.ExecuteNonQuery();

                cmd.CommandText = string.Format(@"SELECT name FROM disk.sqlite_master WHERE type='table' ORDER BY name;");
                using (SQLiteDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        string name = dr.GetString(0);

                        if (string.IsNullOrEmpty(name)) continue;

                        using (SQLiteCommand cmd2 = inMemoryConnection.CreateCommand())
                        {
                            cmd2.CommandText = string.Format(@"CREATE TABLE {0} AS SELECT * FROM disk.{0}", name);
                            cmd2.ExecuteNonQuery();
                        }
                    }
                }

                cmd.CommandText = string.Format(@"DETACH DATABASE disk");
            }

            //NOTE:  Could not get the below code to work, commented out for now.  Based on C++ code found here: http://www.sqlite.org/backup.html

            //IntPtr memDB = inMemoryConnection.GetConnectionHandle();            

            //byte[] encodedFilename = encoding.GetBytes(_filename);

            //_errorCode = (int)(SQLiteErrorCode)sqlite3_open(encodedFilename, out fileDB);
            //if (_errorCode == (int)SQLiteErrorCode.Ok)
            //{
            //    //begin the backup
            //    destDB = sqlite3_backup_init(destDB, encoding.GetBytes("main"), fileDB, encodedFilename);
            //    if (destDB != IntPtr.Zero)
            //    {
            //        sqlite3_backup_step(destDB, -1);
            //        sqlite3_backup_finish(destDB);
            //    }
            //    _errorCode = (int)(System.Data.SQLite.SQLiteErrorCode)destDB;
            //}
            
            //sqlite3_close(fileDB);
            return inMemoryConnection;
        }

        /// <summary>
        /// Backs up the in-memory database to the specified file.
        /// </summary>
        /// <param>name="filename" The filename.</param>
        /// <returns>0 if successful; otherwise, an SQLite error code.</returns>
        public virtual int BackupDatabaseToFile(string _filename, SQLiteConnection _sourceConnection)
        {
            IntPtr backup = new IntPtr();
            IntPtr destDB = new IntPtr();

            SQLiteErrorCode returnCode;

            Encoding encoding = new System.Text.UTF8Encoding();

            byte[] destName = encoding.GetBytes(_filename);

            //get the handle to the memory database
            IntPtr memDB = _sourceConnection.GetConnectionHandle();

            //the rest is similar to the SQLite C examples
            //open the file for writing
            returnCode = (System.Data.SQLite.SQLiteErrorCode)sqlite3_open(destName, out destDB);
            if (returnCode == SQLiteErrorCode.Ok)
            {
                //begin the backup
                backup = sqlite3_backup_init(destDB, encoding.GetBytes("main"), memDB,
                 encoding.GetBytes("main"));
                if (backup != IntPtr.Zero)
                {
                    sqlite3_backup_step(backup, -1);
                    sqlite3_backup_finish(backup);
                }
                returnCode = (System.Data.SQLite.SQLiteErrorCode)destDB;
            }
            //close the file, but not the memory database
            sqlite3_close(destDB);
            return (int)returnCode;
        }
    }
}
