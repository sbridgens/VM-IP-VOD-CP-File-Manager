using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace VM_IP_VOD_CP_File_Manager.Application
{
    public class CpJob
    {
        public string FileOperation { get; set; }
        public string FileDestination { get; set; }
    }
    public class CpJobs
    {
        public string CPName { get; set; }
        public string CpFileSource { get; set; }
        public List<CpJob> CpJob { get; set; }
    }

    public class AppConfig
    {
        public string PollIntervalInSeconds { get; set; }
        public string PackageFileExtension { get; set; }
        public string CPFailedDirectory { get; set; }
        public string IgnoreFilesContainingStrings { get; set; }
        public List<CpJobs> CpJobs { get; set; }
    }
}
