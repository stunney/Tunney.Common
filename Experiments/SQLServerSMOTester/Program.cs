using System;
using System.Collections.Generic;
using System.Collections.Specialized;

using Microsoft.SqlServer.Management.Smo;
using System.Data.SqlClient;

namespace SQLServerSMOTester
{
    class Program
    {
        static void Main(string[] args)
        {
            ScriptingOptions options = new ScriptingOptions();
            //options.ContinueScriptingOnError = true;
            //options.IncludeDatabaseContext = false;
            options.ScriptSchema = true;
            //options.IncludeHeaders = true;
            options.SchemaQualify = true;
            //options.SchemaQualifyForeignKeysReferences = true;
            options.NoCollation = true;
            options.DriAllConstraints = true;
            options.DriAll = true;
            options.DriAllKeys = true;
            options.DriIndexes = true;
            options.ClusteredIndexes = true;
            options.NonClusteredIndexes = true;
            options.ToFileOnly = false;

            const string targetTableName = "SPS_Sessions";

            SqlConnection conn = new SqlConnection(@"Data Source=;Initial Catalog=SPS_Data;Integrated Security=True;");

            Server dumpDatabase = new Server(new Microsoft.SqlServer.Management.Common.ServerConnection(conn));
            Database db = dumpDatabase.Databases[conn.Database];
            Table table = db.Tables[targetTableName];

            if (null == table) throw new InvalidOperationException(string.Format(@"Unable to location table {0} in {1}", targetTableName, conn.ConnectionString));

            Scripter scripter = new Scripter(dumpDatabase);
            scripter.Options = options;

            StringCollection createTableDDL = scripter.Script(new[] { table });

            foreach (string s in createTableDDL)
            {
                Console.WriteLine(s);
            }
        }
    }
}
