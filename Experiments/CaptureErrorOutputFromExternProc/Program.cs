using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO.Compression;
using System.IO;

namespace CaptureErrorOutputFromExternProc
{
    class Program
    {
        static void Main(string[] args)
        {
            //List<double> ds = new List<double>(1000000);

            //for(int idx = 0; idx < ds.Capacity; idx++)
            //{
            //    ds.Add(3.0d);
            //}

            ////NOTE:  Filename should be something like "<TargetTableName>_<GUID>_DataSet.dat"
            //FileInfo fi = new FileInfo(string.Format(@"{0}{3}{1}_{2}_DataSet.dat", @"C:", "Poop-Poop_Yahooo", Guid.NewGuid().ToString().Replace("-", string.Empty), Path.DirectorySeparatorChar));

            ////NOTE:  Intentional file read&write lock!
            //using (FileStream fs = new FileStream(fi.FullName, FileMode.Create, FileAccess.Write, FileShare.None))
            //{                
            //    using (DeflateStream zipper = new DeflateStream(fs, CompressionMode.Compress))
            //    {
            //        Tunney.Serializer.Serialize(ds, zipper);
            //        fs.Flush();
            //    }
            //}
            
            using (Process p = new Process())
            {
                p.StartInfo = new ProcessStartInfo("Test.exe");
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = false;
                p.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                p.StartInfo.RedirectStandardError = true;
                //p.StartInfo.RedirectStandardInput = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.Start();

                long peakWorkingSet = 0;

                do
                {
                    if (!p.HasExited)
                    {
                        peakWorkingSet = p.PeakWorkingSet64;
                    }
                } while (!p.WaitForExit(1000));

                string error = p.StandardError.ReadToEnd();
                string std = p.StandardOutput.ReadToEnd();

                Console.WriteLine(@"Error: " + error);
                Console.WriteLine(@"Standard: " + std);

                Console.WriteLine(@"MaxMemoryUsed: " + peakWorkingSet);
            }

            Console.ReadLine();
        }
    }
}
