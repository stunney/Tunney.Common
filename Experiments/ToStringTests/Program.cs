using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Text;

namespace ToStringTests
{
    class Program
    {
        static void Main(string[] args)
        {
            FileCreateAndWriteTest();

            Console.ReadLine();
        }

        static void FileCreateAndWriteTest()
        {
            DataSet ds = new DataSet();

            //NOTE:  Filename should be something like "<TargetTableName>_<GUID>_DataSet.dat"
            FileInfo fi = new FileInfo(string.Format(@"{0}{3}{1}_{2}_DataSet.dat", @"\\core-spsdata11\TableDump", @"APoopyPants", Guid.NewGuid().ToString().Replace("-", string.Empty), Path.DirectorySeparatorChar));

            //NOTE:  Intentional file read&write lock!
            using (FileStream fs = new FileStream(fi.FullName, FileMode.CreateNew, FileAccess.Write, FileShare.Write))
            {
                using (DeflateStream zipper = new DeflateStream(fs, CompressionMode.Compress))
                {
                    Tunney.Serializer.Serialize(ds, zipper);
                    fs.Flush();
                }
            }
        }

        static void DateTimeTest()
        {
            DateTime dt = DateTime.Parse(@"2011-09-28 04:00:00", CultureInfo.InvariantCulture, DateTimeStyles.None);

            if (dt.Kind == DateTimeKind.Utc) Console.WriteLine(@"UTC Kind");
            if (dt.Kind == DateTimeKind.Local) Console.WriteLine(@"Local Kind");

            Console.WriteLine(@"DateTime = " + dt);

            DateTimeOffset dto = new DateTimeOffset(dt, TimeSpan.Zero);

            Console.WriteLine(@"DateTimeOffset = " + dto);
        }

        static void HashTableTest()
        {
            Hashtable t = new Hashtable();

            t.Add("Monkey", 9);
            t.Add("List", new List<string>(new[] { "one", "two" }));
            t.Add("Who", "fit to be king!");

            StringBuilder sb = new StringBuilder();
            foreach (object key in t.Keys)
            {
                Type ty = ((object)t[key]).GetType();

                MethodInfo[] mi = ty.GetMethods();
                foreach (MethodInfo m in mi)
                {
                    if(m.Name.Equals("ToString") && m.IsVirtual)
                    {
                        Console.WriteLine("Virtual!");
                    }
                }

                sb.AppendFormat("{0}={1}\n", key, t[key]);
            }

            Console.WriteLine(sb.ToString());
        }
    }
}
