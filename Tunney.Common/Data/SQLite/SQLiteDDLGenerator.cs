using System;
using System.Data;
using System.Text;
using System.IO;

namespace Tunney.Common.Data
{
    [Serializable]
    public class SQLiteDDLGenerator : IDDLGenerator
    {
        protected const string DDL_CREATE_TABLE = @"CREATE TABLE [{0}] ({1})";

        /// <remarks>
        ///     http://www.sqlite.org/lang_createtable.html  Look here for "ROWIDs and the INTEGER PRIMARY KEY" stuff for SQLite.
        /// </remarks>
        protected const string DDL_ROWID_COLUMN_NAME = @"_rowid_"; //Defined automatically in SQLite tables

        //protected static readonly string DDL_ROWID_COLUMN_DEFN = string.Format(@"{0} INTEGER PRIMARY KEY ASC,", DDL_ROWID_COLUMN_NAME);

        protected const string DDL_COLUMN_DEFN_FORMAT = @"{0} {1} {2},";
        protected const string DDL_COLUMN_NOTNULL = @" NOT NULL";

        /// <summary>
        /// Generates a simple string containing the CREATE TABLE DDL compatible with SQLite.
        /// </summary>
        /// <param name="_schemaSource">
        /// The <see cref="DataTable"/> containing the schema to replicate in the DDL returned by this method.
        /// </param>
        /// <returns>
        /// SQLite-compatible DDL that will create a table that will hold the data contained in the given <paramref name="_schemaSource"/>.
        /// </returns>
        public virtual string GenerateDDL(DataTable _schemaSource)
        {
            if (null == _schemaSource) throw new ArgumentNullException(@"_schemaSource");
            if (0 == _schemaSource.Columns.Count) throw new ArgumentException(@"Schema source must have at least one defined column.", @"_schemaSource");

            //The different types to use in SQLite: http://www.sqlite.org/datatype3.html - 2.2 Affinity Name Examples
            //-TEXT
            //-NUMERIC
            //-INTEGER
            //-REAL
            //-BLOB
            //-NULL
            //-GUID

            StringBuilder colDefs = new StringBuilder(1000);            
            foreach (DataColumn dc in _schemaSource.Columns)
            {
                if (dc.DataType == typeof(DateTimeOffset))
                {
                    throw new NotSupportedException(@"DateTimeOffset is unfortunately NOT supported by this service.  Try storing it in two columns for the transition using 'CAST(SWITCHOFFSET([stamp], 0) AS DATETIME) AS [stampUTC]' for the actual DATETIME, and 'DATEPART(TZoffset, [stamp]) AS [stampOffset]' for the timezone information.  Use 'SWITCHOFFSET(CAST([stampUTC] AS DATETIMEOFFSET), [stampOffset]) AS [Combined]' to get it all back together again.  Good Luck!");
                }

                if (dc.DataType == typeof(int) || dc.DataType == typeof(short) || dc.DataType == typeof(byte) || dc.DataType == typeof(bool) || dc.DataType == typeof(long))
                {
                    colDefs.AppendFormat(DDL_COLUMN_DEFN_FORMAT, dc.ColumnName, @"INTEGER", dc.AllowDBNull ? string.Empty : DDL_COLUMN_NOTNULL);
                }
                else if (dc.DataType == typeof(double) || dc.DataType == typeof(float) || dc.DataType == typeof(decimal))
                {
                    colDefs.AppendFormat(DDL_COLUMN_DEFN_FORMAT, dc.ColumnName, @"REAL", dc.AllowDBNull ? string.Empty : DDL_COLUMN_NOTNULL);
                }
                else if (dc.DataType == typeof(DateTime))
                {
                    colDefs.AppendFormat(DDL_COLUMN_DEFN_FORMAT, dc.ColumnName, @"TEXT", dc.AllowDBNull ? string.Empty : DDL_COLUMN_NOTNULL); //NUMERIC, and INTEGER don't work :(
                }
                else if (dc.DataType == typeof(byte[]) || dc.DataType == typeof(Stream) || dc.DataType.IsSubclassOf(typeof(Stream)))
                {
                    colDefs.AppendFormat(DDL_COLUMN_DEFN_FORMAT, dc.ColumnName, @"BLOB", dc.AllowDBNull ? string.Empty : DDL_COLUMN_NOTNULL);
                }
                else if (dc.DataType == typeof(Guid))
                {
                    colDefs.AppendFormat(DDL_COLUMN_DEFN_FORMAT, dc.ColumnName, @"GUID", dc.AllowDBNull ? string.Empty : DDL_COLUMN_NOTNULL);
                }
                else
                {
                    colDefs.AppendFormat(DDL_COLUMN_DEFN_FORMAT, dc.ColumnName, @"TEXT", dc.AllowDBNull ? string.Empty : DDL_COLUMN_NOTNULL);
                }
            }

            colDefs.Remove(colDefs.Length - 1, 1);

            string ret = string.Format(DDL_CREATE_TABLE, _schemaSource.TableName, colDefs);
            return ret;
        }        

        public virtual string RowIDColumnName { get { return DDL_ROWID_COLUMN_NAME; } }
    }
}