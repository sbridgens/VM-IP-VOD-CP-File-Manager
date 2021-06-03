using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VM_IP_VOD_CP_File_Manager.Application;
using VM_IP_VOD_CP_File_Manager.BusinessLogic.Contracts;
using VM_IP_VOD_CP_File_Manager.Service;

namespace VM_IP_VOD_CP_File_Manager.BusinessLogic
{
    public class WorkflowProcessor : IWorkflowProcessor
    {
        private readonly ILogger<Worker> _logger;

        private readonly AppConfig _options;
        private List<FileInfo> SourceFileList { get; set; }

        public WorkflowProcessor(ILogger<Worker> logger,
            IOptions<AppConfig> options)
        {
            _logger = logger;
            _options = options.Value;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            foreach (var cpOperation in _options.CpJobs)
            {
                _logger.LogInformation($"==== Starting Operations for {cpOperation.CPName} ====");
                BuildFileList(cpOperation.CPName, cpOperation.CpFileSource, cpOperation.PackageFileExtension);
                
                if (SourceFileList.Count > 0)
                {
                    foreach (var cpJob in cpOperation.CpJob.OrderByDescending(c => c.FileOperation.ToLower() == "copy"))
                    {
                        _logger.LogInformation($"Processing {cpJob.FileOperation} Job for Content provider: {cpOperation.CPName}");
                        await StartCpFileOperations(cpJob);
                        _logger.LogInformation($"Operations Complete for CP: {cpOperation.CPName}");
                    }
                }
                else
                {
                    _logger.LogInformation($"No new files detected for {cpOperation.CPName}");
                }
                _logger.LogInformation($"==== Completed Operations for {cpOperation.CPName} ====");
            }
        }
        
        private void BuildFileList(string cpname, string sourceDirectory, string searchPattern)
        {
            try
            {
                SourceFileList = new List<FileInfo>();

                foreach (var cpFiles in Directory.EnumerateFiles(sourceDirectory, $"*{searchPattern}", SearchOption.TopDirectoryOnly))
                {
                    var file = new FileInfo(cpFiles);
                    if (!IsValidFile(file)) 
                        continue;

                    _logger.LogInformation($"File {file.Name} detected for CP: {cpname}");
                    SourceFileList.Add(file);
                }
            }
            catch (Exception bflException)
            {
                _logger.LogError($"Error building file list for source directory: {sourceDirectory}");
                _logger.LogError($"Error Message: {bflException.Message}");
            }
        }


        private Task StartCpFileOperations(CpJob cpJob)
        {
            try
            {
                foreach (var cpFile in SourceFileList)
                {
                    switch (cpJob.FileOperation.ToLower())
                    {
                        case "copy": 
                            CopyFileToDestination(cpFile, cpJob.FileDestination);
                            break;
                        case "move":
                            MoveFileToDestination(cpFile, cpJob.FileDestination);
                            break;
                    }
                }

            }
            catch (Exception scfoException)
            {
                _logger.LogError("Failed to completed CP File Opertations, check error log before continuing.");
                return Task.FromException(scfoException);
            }
            return Task.CompletedTask;
        }

        private bool IsValidFile(FileInfo filename)
        {
            if (string.IsNullOrEmpty(_options.IgnoreFilesContainingStrings))
                return true;
            var skipList = _options.IgnoreFilesContainingStrings.Split(',');
            if (!skipList.Any(w => filename.Name.Contains(w.Trim())))
                return true;

            _logger.LogInformation(
                $"Skipping File {filename.Name} as it matches a IgnoreFilesContainingStrings parameter");
            return false;

        }

        private void CopyFileToDestination(FileInfo sourceFile, string destinationDirectory)
        {
            try
            {
                if (File.Exists(sourceFile.FullName))
                {
                    var destinationFile = Path.Combine(destinationDirectory, sourceFile.Name);
                    var tempDestinationFile = $"{destinationFile}.tmp";

                    if(File.Exists(tempDestinationFile))
                    {
                        _logger.LogInformation($"Duplicate temp file detected: {tempDestinationFile}, removing temp file.");
                        File.Delete(tempDestinationFile);
                    }

                    if (File.Exists(destinationFile))
                    {
                        _logger.LogWarning($"Copy file: {destinationFile} already exists, bypassing operation.");
                    }
                    else
                    {
                        _logger.LogInformation($"Copying File: {sourceFile.Name} to {tempDestinationFile}");
                        File.Copy(sourceFile.FullName, tempDestinationFile);

                        _logger.LogInformation($"Renaming File: {tempDestinationFile} to {destinationFile}");
                        File.Move(tempDestinationFile, destinationFile);
                        _logger.LogInformation("CP File successfully copied.");
                    }
                }
            }
            catch (Exception cfdException)
            {
                _logger.LogError($"Error copying file {sourceFile.FullName} to destination: {destinationDirectory}");
                _logger.LogError(cfdException.Message);
                MoveErrorFileToFailed(sourceFile, _options.CPFailedDirectory);
            }
        }

        private void MoveFileToDestination(FileInfo sourceFile, string destinationDirectory)
        {
            try
            {
                if (!File.Exists(sourceFile.FullName)) 
                    return;

                var destinationFile = Path.Combine(destinationDirectory, sourceFile.Name);
                    
                if (File.Exists(destinationFile))
                    throw new Exception(
                        $"Cannot move file: {sourceFile.Name} to {destinationFile} as the file exists in the destination");
                _logger.LogInformation($"Moving File: {sourceFile.Name} to {destinationFile}");
                File.Move(sourceFile.FullName, destinationFile);
                _logger.LogInformation("CP File successfully moved.");
            }
            catch (Exception mfdException)
            {
                _logger.LogError($"Error moving file {sourceFile.FullName} to destination: {destinationDirectory}");
                _logger.LogError(mfdException.Message);
                MoveErrorFileToFailed(sourceFile, _options.CPFailedDirectory);
            }
        }

        private void MoveErrorFileToFailed(FileInfo sourceFile, string destinationDirectory)
        {
            try
            {
                if (!File.Exists(sourceFile.FullName))
                    return;
                var destinationFile = Path.Combine(destinationDirectory, sourceFile.Name);
                _logger.LogWarning($"Moving Failed Operation File: {sourceFile.Name} to {destinationFile}");
                File.Move(sourceFile.FullName, destinationFile);
                _logger.LogInformation("Failed File successfully moved.");
            }
            catch (Exception mfdException)
            {
                _logger.LogError($"Error moving Failed file {sourceFile.FullName} to destination: {destinationDirectory}");
                _logger.LogError(mfdException.Message);
            }
        }
    }
}
