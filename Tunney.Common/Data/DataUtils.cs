using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace Tunney.Common.Data
{
    public class DataUtils
    {
        public static string ExportDataTableToTempCSVFile(DataTable _table)
        {
            string filename = string.Format("{0}.csv", Path.GetTempFileName());
            FileInfo file = new FileInfo(filename);
            if (file.Exists) file.Delete();

            file.Refresh();

            // Create the CSV file to which grid data will be exported.
            using (StreamWriter writer = file.CreateText())
            {
                // First we will write the headers.

                int iColCount = _table.Columns.Count;
                for (int i = 0; i < iColCount; i++)
                {
                    writer.Write(_table.Columns[i]);
                    if (i < iColCount - 1)
                    {
                        writer.Write(",");
                    }
                }
                writer.WriteLine();
                // Now write all the rows.
                foreach (DataRow dr in _table.Rows)
                {
                    for (int i = 0; i < iColCount; i++)
                    {
                        if (!Convert.IsDBNull(dr[i]))
                        {
                            writer.Write(dr[i].ToString());
                        }

                        if (i < iColCount - 1)
                        {
                            writer.Write(",");
                        }
                    }
                    writer.WriteLine();
                }
                writer.Flush();
                writer.Close();
            }

            return file.FullName;
        }
    }
}