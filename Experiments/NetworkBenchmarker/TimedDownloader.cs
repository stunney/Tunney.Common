using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

using Tunney.Common.Scheduling;

namespace NetworkBenchmarker
{
    public class TimedDownloader : IScheduleStarter
    {
        protected readonly string m_filename;
        protected readonly FileInfo m_fileInfo;
        protected readonly int m_intervalInMilliseconds;
        protected readonly string m_destinationDirectory;
        protected readonly DirectoryInfo m_destinationDirectoryInfo;
        protected readonly object m_timerLockObject = new object();
        protected Timer m_timer = null;

        protected readonly string m_csvFilename;
        protected readonly FileInfo m_csvFileInfo;

        protected readonly long m_fileSizeInBytes;

        public TimedDownloader(string _filename, string _destinationDirectory, int _intervalInMilliseconds, string _csvFilename)
        {
            if (string.IsNullOrEmpty(_filename)) throw new ArgumentNullException(@"_filename");
            m_filename = _filename;

            m_fileInfo = new FileInfo(m_filename);
            if (!m_fileInfo.Exists) throw new FileNotFoundException(@"Could not locate download file", m_filename);

            if (string.IsNullOrEmpty(_destinationDirectory)) throw new ArgumentNullException(@"_destinationDirectory");
            m_destinationDirectory = _destinationDirectory;

            m_destinationDirectoryInfo = new DirectoryInfo(_destinationDirectory);
            if (!m_destinationDirectoryInfo.Exists) throw new DirectoryNotFoundException(string.Format(@"Could not locate destination directory [{0}]", _destinationDirectory));

            if (0 >= _intervalInMilliseconds) throw new ArgumentException(@"_intervalInMilliseconds must be greater than zero.", @"_intervalInMilliseconds");
            m_intervalInMilliseconds = _intervalInMilliseconds;

            m_fileSizeInBytes = m_fileInfo.Length;

            if (string.IsNullOrEmpty(_csvFilename)) throw new ArgumentNullException(@"_csvFilename");
            m_csvFilename = _csvFilename;
            m_csvFileInfo = new FileInfo(m_csvFilename);

            try
            {
                if (!m_csvFileInfo.Exists) m_csvFileInfo.Create().Close();

                WriteCSVRow(@"Status", @"Timestamp of entry (end of operation)", @"Time(ms)", @"Filesize(bytes)");
            }
            catch (Exception _ex)
            {
                throw new ArgumentException(string.Format(@"Error creating csv file {0}", m_csvFilename), _ex);
            }
        }

        protected virtual void WriteCSVRow(params string[] _values)
        {
            List<string> vals = new List<string>(_values);

            using (StreamWriter sw = m_csvFileInfo.AppendText())
            {
                string joined = string.Format("{0}", string.Join(",", vals.ToArray()));
                sw.WriteLine(joined);
            }
        }

        protected virtual void m_timer_Elapsed(object _sender)
        {
            //Wipe out the destination first
            foreach (FileInfo fi in m_destinationDirectoryInfo.GetFiles())
            {
                fi.Delete();
            }

            string status = @"SUCCESS";
            Stopwatch sw = new Stopwatch();
            sw.Start();
            try
            {
                m_fileInfo.CopyTo(m_destinationDirectory + Path.DirectorySeparatorChar + m_fileInfo.Name + Path.GetRandomFileName() + @".tmp");
            }
            catch(Exception _ex)
            {
                status = string.Format(@"FAILED[{0}]", _ex.Message);
            }
            finally
            {
                sw.Stop();
            }

            WriteCSVRow(status, DateTime.Now.ToString(@"yyyy-MM-dd HH:mm:ss"), sw.ElapsedMilliseconds.ToString(), m_fileSizeInBytes.ToString());
        }

        #region IScheduleStarter Members

        public virtual void Start()
        {
            lock (m_timerLockObject)
            {
                if (null == m_timer)
                {
                    m_timer = new Timer(new TimerCallback(m_timer_Elapsed), new object(), 1000, m_intervalInMilliseconds);
                }
            }
        }

        public virtual void Stop()
        {
            lock (m_timerLockObject)
            {
                if (null != m_timer)
                {
                    m_timer.Dispose();
                    m_timer = null;
                }
            }
        }

        public virtual void Continue()
        {
        }

        public virtual void Pause()
        {       
        }

        #endregion

        #region IContainerUser Members

        public virtual Tunney.Common.IoC.IIoCContainer Container { get; set; }

        #endregion

        #region ILogWriter Members

        public virtual Tunney.Common.IoC.ILogger Logger { get; set; }

        #endregion
    }
}