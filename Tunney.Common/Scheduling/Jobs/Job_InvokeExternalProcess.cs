using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Quartz;
using System.Threading;

namespace Tunney.Common.Scheduling.Jobs
{
    [Serializable]
    public class Job_InvokeExternalProcess : AJob
    {
        public const string CONFIG_STAGING_DIRECTORY = @"StagedFullyQualifiedDirectoryName";
        public const string CONFIG_EXE_FILENAME = @"JobExecutableFilename";
        public const string CONFIG_JOB_FQCN = @"JobClassName";        

        public override void ExecuteJob(JobDataMap dataMap)
        {            
            string fullyQualifiedJobClass = dataMap.GetString(CONFIG_JOB_FQCN);
            if(string.IsNullOrEmpty(fullyQualifiedJobClass)) throw new NullReferenceException(@"JobClassName is null.");

            string exeFilenameToFind = dataMap.GetString(CONFIG_EXE_FILENAME);
            if (string.IsNullOrEmpty(exeFilenameToFind)) throw new NullReferenceException(@"ExeFileName is null.");

            string stagingDirectoryPath = dataMap.GetString(CONFIG_STAGING_DIRECTORY);
            if (string.IsNullOrEmpty(stagingDirectoryPath)) throw new NullReferenceException(@"StagingDirectoryName is null.");

            DirectoryInfo stagingDirectory = new DirectoryInfo(stagingDirectoryPath);
            if (!stagingDirectory.Exists) throw new DirectoryNotFoundException(string.Format(@"Staging directory [{0}] does not exist.", stagingDirectory.FullName));

            string iocConfigFilename = dataMap.GetString(CONFIG_IOC_CONFIG_FILENAME);
            if (string.IsNullOrEmpty(iocConfigFilename)) throw new NullReferenceException(@"IoC Configuration Filename is null.");

            Hashtable configDetails = new Hashtable(dataMap.Count);
            //Dictionary<string, object> configDetails = new Dictionary<string, object>(context.JobDetail.JobDataMap.Count);
            foreach (string key in dataMap.Keys)
            {
                configDetails.Add(key, dataMap[key]);
            }

            DirectoryInfo tempWorkingFolder = Directory.CreateDirectory(Path.GetTempPath() + Path.DirectorySeparatorChar + Path.GetRandomFileName());
            FileInfo configFilename = null;
            try
            {                
                configFilename = SaveObjectToTempFile(configDetails, tempWorkingFolder);
            }
            catch (IOException _ioex)
            {
                if (_ioex.Message.Contains(@"There is not enough space on the disk."))
                {
                    TryToDeleteTempFiles(new DirectoryInfo(Path.GetTempPath()));
                    Logger.WARN("Not enough disk space.  Attempted to clean up User's %TEMP% folder.");                    
                }

                throw; //The job can try again later.
            }

            string subFolderName = exeFilenameToFind;
            if (subFolderName.Contains(@".exe")) subFolderName = subFolderName.Replace(@".exe", string.Empty);
            if (subFolderName.Contains(@".dll")) subFolderName = subFolderName.Replace(@".dll", string.Empty);

            DirectoryInfo jobAssemblyDirectory = new DirectoryInfo(string.Format(@"{0}{2}{1}", stagingDirectory.FullName, subFolderName, Path.DirectorySeparatorChar));

            FileInfo exeFilename = null;
            foreach (FileInfo fi in jobAssemblyDirectory.GetFiles())
            {
                FileInfo tmp = fi.CopyTo(tempWorkingFolder.FullName + Path.DirectorySeparatorChar + fi.Name);
                if (fi.Name.Equals(exeFilenameToFind))
                {
                    exeFilename = tmp;
                }
            }

            if (null == exeFilename) throw new ApplicationException(string.Format(@"Unable to find configured exe '{0}' for job '{1}.", exeFilenameToFind, JobName));

            try
            {
                ExecuteOutOfProcessJob(configFilename, exeFilename, fullyQualifiedJobClass, iocConfigFilename);
            }
            finally
            {
                Thread.Sleep(2000); //Here so that the process is given time to release locks on DLLs                

                TryToDeleteTempFiles(tempWorkingFolder);
            }
        }

        protected virtual void ExecuteOutOfProcessJob(FileInfo _configFilename, FileInfo _executableFilename, string _jobClassName, string _iocConfigFilename)
        {
            using (Process jobProcess = new Process())
            {
                jobProcess.StartInfo = new ProcessStartInfo(_executableFilename.FullName, string.Format(@"""{0}"" ""{1}"" ""{2}"" ""{3}"" ""{4}""", _configFilename.FullName, _jobClassName, JobName, JobGroup, _iocConfigFilename));
                jobProcess.StartInfo.WorkingDirectory = _executableFilename.Directory.FullName;

                //TODO:  Make this a passed-in setting
                //jobProcess.MaxWorkingSet = new IntPtr(2147483648); //2 * 1024 * 1024 * 1024 == 2 gigabytes

                jobProcess.StartInfo.UseShellExecute = false;
                jobProcess.StartInfo.CreateNoWindow = false;
                jobProcess.StartInfo.RedirectStandardError = true;
                jobProcess.StartInfo.RedirectStandardOutput = true;

                //if (JobName.Contains("GroupWatcher")) Thread.Sleep(5000);

                try
                {
                    long peakWorkingSet = 0;

                    jobProcess.Start();
                    do
                    {
                        if (!jobProcess.HasExited)
                        {
                            //peakWorkingSet = jobProcess.PeakWorkingSet64;
                            //TODO:  Track other items in here as well!
                        }
                    } while (!jobProcess.WaitForExit(1000));

                    jobProcess.WaitForExit();

                    int exitCode = jobProcess.ExitCode;

                    if (0 != exitCode)
                    {
                        throw new ApplicationException(string.Format(@"An error occurred executing an external process job of type {0}.  Exit Code=[{1}].", _jobClassName, exitCode));
                    }
                }

                catch (Exception _ex)
                {
                    string errMsg = jobProcess.StandardError.ReadToEnd();

                    throw new ApplicationException(@"External Process error:" + errMsg, _ex);
                }
                finally
                {
                    if (null != jobProcess)
                    {
                        try
                        {
                            jobProcess.Close();
                            jobProcess.Kill();
                        }
                        catch { }

                        jobProcess.Dispose();
                    }
                }
            }
        }

        protected virtual FileInfo SaveObjectToTempFile(object _serializableObject, DirectoryInfo _destinationFolder)
        {
            FileInfo file = new FileInfo(_destinationFolder.FullName + Path.DirectorySeparatorChar + Path.GetRandomFileName() + ".config.bin");
            
            using(FileStream fs = file.Create())
            {
                Serializer.Serialize(_serializableObject, fs);
                fs.Flush(true);
            }

            return file;
        }
    }
}