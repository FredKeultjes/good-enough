/*
Programming by Fred Keultjes

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

*/
using Microsoft.Experimental.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace Backup2014
{
    public class BackupTask
    {
        public const bool UseLongPaths = true;
        public JobConfiguration Settings;
        private BackupThread backupThread;
        public ArchiveFolderBuffer ArchBuffer = new ArchiveFolderBuffer();

        public BackupTask(JobConfiguration settings, BackupThread backupThread)
        {
            this.Settings = settings;
            this.backupThread = backupThread;
        }
        
        private string _archiveDirectory = null;
        public string ArchiveDirectory
        {
            get
            {
                if (_archiveDirectory == null)
                {
                    _archiveDirectory = Path.Combine(Settings.TargetDirectory, "_arch");
                    if (UseLongPaths)
                    {
                        if (!LongPathDirectory.Exists(_archiveDirectory))
                        {
                            LongPathDirectory.Create(_archiveDirectory);
                        }
                    }
                    else
                    {
                        if (!Directory.Exists(_archiveDirectory))
                        {
                            Directory.CreateDirectory(_archiveDirectory);
                        }
                    }
                }
                return _archiveDirectory;
            }
        }

        private string _archiveSessionDirectory = null;
        public string ArchiveSessionDirectory
        {
            get
            {
                if (_archiveSessionDirectory == null)
                {
                    _archiveSessionDirectory = Path.Combine(ArchiveDirectory, DateTime.Now.ToString("yyyyMMdd_HHmmss"));
                    if (UseLongPaths)
                    {
                        LongPathDirectory.Create(_archiveSessionDirectory);
                    }
                    else
                    {
                        Directory.CreateDirectory(_archiveSessionDirectory);
                    }
                }
                return _archiveSessionDirectory;
            }
        }
        private string _logFilePath = null;
        public string LogFilePath
        {
            get
            {
                if( _logFilePath==null )
                {
                    _logFilePath = Path.Combine(ArchiveSessionDirectory, "log.txt");
                }
                return _logFilePath;
            }
        }

        private string _archiveSessionDeletedFilesDirectory = null;
        private string _archiveSessionUpdatedFilesDirectory = null;

        public string ArchiveSessionDeletedFilesDirectory
        {
            get
            {
                if( _archiveSessionDeletedFilesDirectory == null )
                {
                    _archiveSessionDeletedFilesDirectory = Path.Combine(ArchiveSessionDirectory, "Deleted");
                    if (UseLongPaths)
                    {
                        LongPathDirectory.Create(_archiveSessionDeletedFilesDirectory);
                    }
                    else
                    {
                        Directory.CreateDirectory(_archiveSessionDeletedFilesDirectory);
                    }
                }
                return _archiveSessionDeletedFilesDirectory;
            }
        }
        public string ArchiveSessionUpdatedFilesDirectory
        {
            get
            {
                if (_archiveSessionUpdatedFilesDirectory == null)
                {
                    _archiveSessionUpdatedFilesDirectory = Path.Combine(ArchiveSessionDirectory, "Updated");
                    if (UseLongPaths)
                    {
                        LongPathDirectory.Create(_archiveSessionUpdatedFilesDirectory);
                    }
                    else
                    {
                        Directory.CreateDirectory(_archiveSessionUpdatedFilesDirectory);
                    }
                }
                return _archiveSessionUpdatedFilesDirectory;
            }
        }

        public void SetStatus(string message)
        {
            backupThread.PostStatus(message);
        }

        public void Log(string message)
        {
            if (UseLongPaths)
            {
                LongPathFile.AppendAllText( LogFilePath, Utilities.PrePendTextLines(message, DateTime.Now.ToString("yyyyMMdd HHmmss\\:\\ ")));
            }
            else
            {
                File.AppendAllText( LogFilePath, Utilities.PrePendTextLines(message, DateTime.Now.ToString("yyyyMMdd HHmmss\\:\\ ")));
            }
        }

        public void PostMessage(bool error, string msg)
        {
            backupThread.PostMessage(error, msg);
        }
        public DialogResult Confirm(string msg)
        {
            return backupThread.Confirm(msg);
        }
        public void ShowProgress(long max, long current)
        {
            backupThread.ShowProgress( max, current);
        }
        public void SetProgressStart(long progressStartCurrent)
        {
            backupThread.SetProgressStart(progressStartCurrent);
        }

        private bool isCancelReported = false;
        public bool IsCancelRequested(ref DateTime latestCancelTest)
        {
            if (isCancelReported)
                return true;

            DateTime now = DateTime.Now;
            if (now > latestCancelTest.AddMilliseconds(300))
            {
                latestCancelTest = now;
                if (backupThread.IsCancelJobRequested())
                {
                    isCancelReported = true;
                    Log("Cancelled by user");
                    SetStatus("Cancelled by user");
                    PostMessage(false, "Cancelled by user");
                    return true;
                }
            }
            return false;
        }


        public bool CopyFileCancellable(string sourceFileName, string destinationFileName, bool overwrite, ref long amountProcessed, long workloadAmount, ref DateTime latestCancelTest)
        {
            long bytesRemaining = 0;
            try
            {
                FileAttributes sourceAttributes;
                DateTime sourceLastWriteTime;
                if (UseLongPaths)
                {
                    LongPathFileInfo sourceInfo = new LongPathFileInfo(sourceFileName);
                    sourceAttributes = sourceInfo.Attributes;
                    sourceLastWriteTime = sourceInfo.LastWriteTime;
                    bytesRemaining = sourceInfo.Length;
                }
                else
                {
                    FileInfo sourceInfo = new FileInfo(sourceFileName);
                    sourceAttributes = sourceInfo.Attributes;
                    sourceLastWriteTime = sourceInfo.LastWriteTime;
                    bytesRemaining = sourceInfo.Length;
                }

                using (FileStream inputStream = UseLongPaths 
                                              ? LongPathFile.Open(sourceFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
                                              : File.Open(sourceFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite) )
                {
                    using (FileStream outputStream = UseLongPaths 
                                              ? LongPathFile.Open(destinationFileName, overwrite ? FileMode.Create : FileMode.CreateNew, FileAccess.Write, FileShare.Read)
                                              : File.Open(destinationFileName, overwrite ? FileMode.Create : FileMode.CreateNew, FileAccess.Write, FileShare.Read))
                    {
                        int bytesRead;
                        while ((bytesRead = inputStream.Read(ArchBuffer.buffer, 0, ArchiveFolderBuffer.TransferBlockSize)) > 0)
                        {
                            outputStream.Write(ArchBuffer.buffer, 0, bytesRead);

                            amountProcessed += BackupWorkload.WeightWriteFileByBytes(bytesRead);
                            bytesRemaining -= bytesRead;
                            ShowProgress(workloadAmount, amountProcessed);

                            if (IsCancelRequested(ref latestCancelTest))
                            {
                                outputStream.Close();
                                inputStream.Close();
                                if (UseLongPaths)
                                {
                                    LongPathFile.Delete(destinationFileName);
                                }
                                else
                                {
                                    File.Delete(destinationFileName);
                                }
                                amountProcessed += BackupWorkload.WeightWriteFileByBytes(bytesRemaining);
                                return false;
                            }
                        }
                    }
                }
                if (UseLongPaths)
                {
                    LongPathFile.SetLastWriteTime(destinationFileName, sourceLastWriteTime);
                    if ((sourceAttributes & FileAttributes.ReadOnly) != 0)
                    {
                        LongPathFile.SetAttributes(destinationFileName, LongPathFile.GetAttributes(destinationFileName) | FileAttributes.ReadOnly);
                    }
                }
                else
                {
                    File.SetLastWriteTime(destinationFileName, sourceLastWriteTime);
                    if ((sourceAttributes & FileAttributes.ReadOnly) != 0)
                    {
                        File.SetAttributes(destinationFileName, File.GetAttributes(destinationFileName) | FileAttributes.ReadOnly);
                    }
                }
            }
            catch (Exception ex)
            {
                if (UseLongPaths)
                {
                    if (LongPathFile.Exists(destinationFileName))
                        LongPathFile.Delete(destinationFileName);
                }
                else
                {
                    if (File.Exists(destinationFileName))
                        File.Delete(destinationFileName);
                }
                amountProcessed += BackupWorkload.WeightWriteFileByBytes(bytesRemaining);
                throw;
            }
            return true;
        }

        public bool BackupAllSources()
        {
            List<BackupTaskStep> steps = new List<BackupTaskStep>();
            try
            {
                long totalWorkload = 0;
                backupThread.ShowOverallProgress(totalWorkload, 0);

                // check target existence
                bool targetExists = UseLongPaths ? LongPathDirectory.Exists(Settings.TargetDirectory) : Directory.Exists(Settings.TargetDirectory);
                if (!targetExists)
                {
                    string msg = string.Format("Cannot find target directory {0}. Scanning aborted", Settings.TargetDirectory);
                    PostMessage(true, msg);
                    Log(msg);
                    return false;
                }

                // check source existence
                Dictionary<string,int> assignedTargetSubDirsLCase = new Dictionary<string,int>();
                foreach (SourceDefinition sourceDef in Settings.Sources)
                {
                    BackupTaskStep step = new BackupTaskStep(this, sourceDef);
                    steps.Add(step);
                    if (!step.CheckSourceExistence())
                        return false;
                    assignedTargetSubDirsLCase.Add(step.StepSourceDirPrefix.ToLower(), 0);
                }

                // find target directories not matching source
                string[] targetSubFolders = BackupTask.UseLongPaths ? LongPathDirectory.GetDirectories(Settings.TargetDirectory) : Directory.GetDirectories(Settings.TargetDirectory);
                foreach (string targetSubFolderPath in targetSubFolders)
                {
                    string targetSubFolderName = Path.GetFileName(targetSubFolderPath);

                    if (Utilities.IsFileNameCodedDirectoryPath(targetSubFolderName) && !assignedTargetSubDirsLCase.ContainsKey(targetSubFolderName.ToLower()))
                    {
                        BackupTaskStep step = new BackupTaskStep(this, targetSubFolderName);
                        steps.Add(step);
                    }
                }

                // calculate backup task workload
                foreach (BackupTaskStep step in steps)
                {
                    if (!step.EstablishWorkload())
                        return false;

                    totalWorkload += step.workloadAmount;
                }

                long totalCurrent = 0;
                backupThread.ShowOverallProgress(totalWorkload, totalCurrent);
                backupThread.SetOverallProgressStart(totalCurrent);

                // do backup tasks
                foreach (BackupTaskStep step in steps)
                {
                    if (!step.Execute())
                        return false;

                    totalCurrent += step.workloadAmount;
                    backupThread.ShowOverallProgress(totalWorkload, totalCurrent);
                    step.CloseArchives();
                }
                PostMessage(false, "Backup completed");
            }
            catch(Exception ex)
            {
                string msg = string.Format("Uncaught exception: {0}", ex.ToString());
                Log(msg);
                PostMessage(true, msg);
                return false;
            }
            finally
            {
                foreach (BackupTaskStep step in steps)
                {
                    step.Dispose();
                }
                backupThread.ResetCancelJobRequest();
            }
            return true;
        }
       



    }


}
