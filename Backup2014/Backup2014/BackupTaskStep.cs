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
    public class BackupTaskStep : IDisposable
    {
        public BackupTask task;
        private SourceDefinition sourceDef;
        public readonly string StepSourceDirPrefix;
        public readonly string StepTargetBaseDir;
        private BackupWorkload workload = null;

        public long TotalBytesCopied = 0;
        public int TotalReplaced = 0;
        public int TotalNewDirectories = 0;
        public int TotalRemovedDirectories = 0;
        public int TotalRemovedFiles = 0;

        public long amountProcessed = 0;
        public long workloadAmount = 0;
        private DateTime latestCancelTest = DateTime.Now;

        public const long MaxZipSize = 400000000; //381 MB
        public const long MaxZipSourceSize = 800000000; // 762 MB

        /// <summary>
        /// Constructor for backup based on SourceDefinition
        /// </summary>
        /// <param name="task"></param>
        /// <param name="sourceDef"></param>
        public BackupTaskStep(BackupTask task, SourceDefinition sourceDef)
        {
            this.task = task;
            this.sourceDef = sourceDef;
            
            StepSourceDirPrefix = Utilities.CodeDirectoryPathIntoFileName(sourceDef.SourceDirectory);

            StepTargetBaseDir = Path.Combine(task.Settings.TargetDirectory, StepSourceDirPrefix);
        }

        /// <summary>
        /// Constructor for backup not unmatched target directory
        /// </summary>
        /// <param name="task"></param>
        /// <param name="stepSourceDirPrefix"></param>
        public BackupTaskStep(BackupTask task, string targetSubDirName)
        {
            this.task = task;
            this.sourceDef = null;
            this.StepSourceDirPrefix = targetSubDirName;

            StepTargetBaseDir = Path.Combine(task.Settings.TargetDirectory, StepSourceDirPrefix);
        }

        public void Dispose()
        {
            CloseArchives();
        }
        public void CloseArchives()
        {
            if (_deletedFilesArchiveFolder != null)
            {
                _deletedFilesArchiveFolder.Dispose();
                _deletedFilesArchiveFolder = null;
            }
            if (_updatedFilesArchiveFolder != null)
            {
                _updatedFilesArchiveFolder.Dispose();
                _updatedFilesArchiveFolder = null;
            }
        }

        public ArchiveFolder _deletedFilesArchiveFolder = null;
        public ArchiveFolder DeletedFilesArchiveFolder
        {
            get
            {
                if (_deletedFilesArchiveFolder == null)
                {
                    _deletedFilesArchiveFolder = new ArchiveFolder(this, task.ArchiveSessionDeletedFilesDirectory, StepSourceDirPrefix);
                }
                return _deletedFilesArchiveFolder;
            }
        }

        public ArchiveFolder _updatedFilesArchiveFolder = null;
        public ArchiveFolder UpdatedFilesArchiveFolder
        {
            get
            {
                if (_updatedFilesArchiveFolder == null)
                {
                    _updatedFilesArchiveFolder = new ArchiveFolder(this, task.ArchiveSessionUpdatedFilesDirectory, StepSourceDirPrefix);
                }
                return _updatedFilesArchiveFolder;
            }
        }

        public bool CheckSourceExistence()
        {
            return sourceDef.CheckSourceExistence(this);
        }

        public bool EstablishWorkload()
        {
            workload = new BackupWorkload(this, sourceDef);
            task.ShowProgress(0, 0);

            workloadAmount = 0;
            amountProcessed = -1;

            // read source tree 
            if ( sourceDef!=null && !sourceDef.ScanSource(this, workload))
                return false;

            try
            {
                if( BackupTask.UseLongPaths )
                {
                    if (!LongPathDirectory.Exists(StepTargetBaseDir))
                    {
                        LongPathDirectory.Create(StepTargetBaseDir);
                    }
                }
                else
                {
                    if (!Directory.Exists(StepTargetBaseDir))
                    {
                        Directory.CreateDirectory(StepTargetBaseDir);
                    }
                }
            }
            catch (Exception ex)
            {
                task.Log(string.Format("Error creating directory {0}: {1}", StepTargetBaseDir, RepresentException(ex)));
                return false;
            }

            // scan target directory tree determine new/updated/deleted files
            if (!ScanTargetDirectory())
                return false;

            workloadAmount = workload.WorkloadAmount();
            amountProcessed = 0;
            return true;
        }

        public bool Execute()
        {
            task.ShowProgress(workloadAmount, amountProcessed);

            // display actions and ask confirmation

            if (task.Settings.ConfirmAllActions )
            {
                DialogResult res = workload.AskConfirmation();
                switch(res)
                {
                    case DialogResult.Cancel:
                        task.SetStatus("Backup aborted");
                        return false;
                    case DialogResult.No:
                        task.SetStatus("Step aborted");
                        return true;
                }
            }
            task.SetProgressStart(amountProcessed);

            // replace modified files
            int iErrorCount = 0;
            {
                int iSize = workload.TargetFilesToReplace.Count;
                if (iSize > 0)
                {
                    task.SetStatus(string.Format("Replacing {0} Modified Files from {1}", iSize, sourceDef.SourceDirectory));

                    for (int iIndex = 0; iIndex < iSize; iIndex++)
                    {
                        if (task.IsCancelRequested(ref latestCancelTest))
                            return false;

                        if (!ReplaceFileToTarget(workload.TargetFilesToReplace[iIndex]))
                            iErrorCount++;
                    }
                }
            }

            // create new directories
            if (workload.mapRelativeSourceDirectoriesLCaseRemaining.Count>0)
            {
                task.SetStatus(string.Format("Creating {0} directories in {1}", workload.mapRelativeSourceDirectoriesLCaseRemaining.Count, StepTargetBaseDir));

                foreach(KeyValuePair<string,string> elem in workload.mapRelativeSourceDirectoriesLCaseRemaining)
                {
                    if (task.IsCancelRequested(ref latestCancelTest))
                        return false;

                    amountProcessed += BackupWorkload.WeightCreateTargetDirectory;
                    task.ShowProgress(workloadAmount, amountProcessed);

                    if (!CreateTargetDirectory(elem.Value))
                        iErrorCount++;
                }
            }

            // copy new files
            if (workload.mapRelaFileNameLCaseToSourceFile.Count>0 )
            {
                task.SetStatus(string.Format("Copying {0} new files from {1}", workload.mapRelaFileNameLCaseToSourceFile.Count, sourceDef.SourceDirectory));

                foreach(KeyValuePair<string,SourceFileInfo> elem in workload.mapRelaFileNameLCaseToSourceFile)
                {
                    if (task.IsCancelRequested(ref latestCancelTest))
                        return false;

                    if (!CopyFileToTarget(elem.Value))
                        iErrorCount++;
                }
            }

            // remove files
            {
                int iSize = workload.TargetRelaFilepathsToRemove.Count;

                task.SetStatus(string.Format("Removing {0} files in {1}", iSize, StepTargetBaseDir));

                for (int iIndex = 0; iIndex < iSize; iIndex++)
                {
                    if (task.IsCancelRequested(ref latestCancelTest))
                        return false;

                    if (!RemoveTargetFile(workload.TargetRelaFilepathsToRemove[iIndex]))
                        iErrorCount++;
                }
            }

            // remove directories
            {
                int iSize = workload.TargetDirectoriesToRemove.Count;
                if (iSize > 0)
                {
                    task.SetStatus(string.Format("Removing {0} directories in {1}", iSize, StepTargetBaseDir));

                    // note: from end down to index 0, to ensure that first the childs are removed and later the parents
                    for (int iIndex = iSize - 1; iIndex >= 0; iIndex--)
                    {
                        if (task.IsCancelRequested(ref latestCancelTest))
                            return false;

                        amountProcessed += BackupWorkload.WeightRemoveTargetDirectory;
                        task.ShowProgress(workloadAmount, amountProcessed);

                        if (!RemoveTargetDirectory(workload.TargetDirectoriesToRemove[iIndex]))
                            iErrorCount++;
                    }
                }
            }

            if (sourceDef == null)
            {
                if ((BackupTask.UseLongPaths ? LongPathDirectory.GetDirectories(StepTargetBaseDir).Length == 0 : Directory.GetDirectories(StepTargetBaseDir).Length == 0)
                  && (BackupTask.UseLongPaths ? LongPathDirectory.GetFiles(StepTargetBaseDir).Length == 0 : Directory.GetFiles(StepTargetBaseDir).Length == 0) )
                {
                    if (!RemoveTargetDirectory(string.Empty))
                        iErrorCount++;
                }
            }

            task.SetStatus("");
            return true;
        }

        public bool ScanTargetDirectory()
        {
            // scan target directory tree
            task.SetStatus(string.Format("Scanning Target Directory {0}", StepTargetBaseDir));
            try
            {
                List<string> arScanWorkload = new List<string>();  // to avoid recursive calls
                arScanWorkload.Add(StepTargetBaseDir);

                int iPrefixLength = StepTargetBaseDir.Length;
                if (iPrefixLength > 0 && !StepTargetBaseDir.EndsWith("\\"))
                    iPrefixLength++;

                for (int iDirIndex = 0; iDirIndex < arScanWorkload.Count; iDirIndex++)
                {
                    string sCurrentRoot = arScanWorkload[iDirIndex];

                    string[] subFolders = BackupTask.UseLongPaths ? LongPathDirectory.GetDirectories(sCurrentRoot) : Directory.GetDirectories(sCurrentRoot);
                    if (task.IsCancelRequested(ref latestCancelTest))
                        return false;

                    string[] files = BackupTask.UseLongPaths ? LongPathDirectory.GetFiles(sCurrentRoot) : Directory.GetFiles(sCurrentRoot);
                    if (task.IsCancelRequested(ref latestCancelTest))
                        return false;

                    foreach (string curDir in subFolders)
                    {
                        if (((BackupTask.UseLongPaths ? new LongPathFileInfo(curDir).Attributes : new FileInfo(curDir).Attributes) & FileAttributes.ReparsePoint) == 0)
                        {
                            string sRelaPath = curDir.Substring(iPrefixLength);
                            arScanWorkload.Add(curDir);

                            if (workload.mapRelativeSourceDirectoriesLCaseRemaining.Remove(sRelaPath.ToLower()))
                            {
                                workloadAmount -= BackupWorkload.WeightCreateTargetDirectory;
                            }
                            else
                            {
                                workload.TargetDirectoriesToRemove.Add(curDir);

                                workloadAmount += BackupWorkload.WeightRemoveTargetDirectory;
                                task.ShowProgress(workloadAmount, amountProcessed);
                            }
                        }
                    }

                    foreach (string filePath in files)
                    {
                        string sRelaPath = filePath.Substring(iPrefixLength);
                        string sRelaPathLCase = sRelaPath.ToLower();
                        SourceFileInfo srcFileInfo;

                        if (workload.mapRelaFileNameLCaseToSourceFile.TryGetValue(sRelaPathLCase, out srcFileInfo))
                        {
                            // check length and date
                            long targetLength;
                            DateTime targetLastWriteTime;
                            FileAttributes targetAttributes;
                            if( BackupTask.UseLongPaths )
                            {
                                LongPathFileInfo targetFileInfo = new LongPathFileInfo(filePath);
                                targetLength = targetFileInfo.Length;
                                targetLastWriteTime = targetFileInfo.LastWriteTime;
                                targetAttributes = targetFileInfo.Attributes;
                            }
                            else
                            {
                                FileInfo targetFileInfo = new FileInfo(filePath);
                                targetLength = targetFileInfo.Length;
                                targetLastWriteTime = targetFileInfo.LastWriteTime;
                                targetAttributes = targetFileInfo.Attributes;
                            }

                            if (srcFileInfo.FileSize != targetLength
                             || srcFileInfo.IsReadOnly != ((targetAttributes & FileAttributes.ReadOnly) != 0))
                            {
                                workload.TargetFilesToReplace.Add(srcFileInfo);

                                workloadAmount += workload.WeightTargetFileToReplace(srcFileInfo);
                                task.ShowProgress(workloadAmount, amountProcessed);
                            }
                            else
                            {
                                double diffInSeconds = Math.Abs((srcFileInfo.tmLastWrite - targetLastWriteTime).TotalSeconds);
                                if (diffInSeconds >= 60 && diffInSeconds!=3600)
                                {
                                    workload.TargetFilesToReplace.Add(srcFileInfo);

                                    workloadAmount += workload.WeightTargetFileToReplace(srcFileInfo);
                                    task.ShowProgress(workloadAmount, amountProcessed);
                                }
                            }

                            workload.mapRelaFileNameLCaseToSourceFile.Remove(sRelaPathLCase);

                            workloadAmount -= BackupWorkload.WeightWriteFileByBytes(srcFileInfo.FileSize);
                            task.ShowProgress(workloadAmount, amountProcessed);
                        }
                        else
                        {
                            workload.TargetRelaFilepathsToRemove.Add(sRelaPath);

                            workloadAmount += workload.WeightTargetRelaFilepathsToRemove(sRelaPath);
                            task.ShowProgress(workloadAmount, amountProcessed);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = string.Format("Error scanning target directory {0}: {1}", StepTargetBaseDir, RepresentException(ex));
                task.Log(msg);
                task.PostMessage(true, msg);
                return false;
            }
            return true;
        }


        private bool ReplaceFileToTarget(SourceFileInfo sourceFile)
        {
            string sFullSourceFilename = Path.Combine(workload.SourceDef.SourceDirectory, sourceFile.RelaFilename);
            string sFullTargetFilename = Path.Combine( StepTargetBaseDir, sourceFile.RelaFilename );

            System.Diagnostics.Debug.WriteLine("File {1} replaced by {0}", sFullSourceFilename, sFullTargetFilename);

            if (!(BackupTask.UseLongPaths ? LongPathFile.Exists(sFullSourceFilename) : File.Exists(sFullSourceFilename)))
            {
                string msg = string.Format("File {0} no longer exists", sFullSourceFilename);
                task.Log(msg);
                task.PostMessage(true, msg);
                return false;
            }

            if (!UpdatedFilesArchiveFolder.MoveFileToArchiveDir(sourceFile.RelaFilename, ref amountProcessed, workloadAmount,ref latestCancelTest))
                return false;            

            try
            {
                if (!task.CopyFileCancellable(sFullSourceFilename, sFullTargetFilename, true, ref amountProcessed, workloadAmount, ref latestCancelTest))
                    return false;
            }
            catch(Exception ex)
            {
                string msg = string.Format("Error copying {0} to {1}: {2}", sFullSourceFilename, sFullTargetFilename, RepresentException(ex));
                task.Log( msg );
                task.PostMessage(true, msg);
                return false;
            }

            task.Log(string.Format("File {1} replaced by {0}", sFullSourceFilename, sFullTargetFilename));

            TotalBytesCopied += sourceFile.FileSize;
            TotalReplaced++;
            return true;
        }

        private bool CopyFileToTarget(SourceFileInfo sourceFile)
        {
            string sFullSourceFilename = Path.Combine(workload.SourceDef.SourceDirectory, sourceFile.RelaFilename);
            string sFullTargetFilename = Path.Combine(StepTargetBaseDir, sourceFile.RelaFilename);

            System.Diagnostics.Debug.WriteLine("File {1} copied from {0}", sFullSourceFilename, sFullTargetFilename);

            if (!(BackupTask.UseLongPaths ? LongPathFile.Exists(sFullSourceFilename) : File.Exists(sFullSourceFilename)))
            {
                string msg = string.Format("File {0} no longer exists", sFullSourceFilename);
                task.Log(msg);
                task.PostMessage(true, msg);
                return false;
            }

            try
            {
                if (!task.CopyFileCancellable(sFullSourceFilename, sFullTargetFilename, true, ref amountProcessed, workloadAmount, ref latestCancelTest))
                    return false;
            }
            catch (Exception ex)
            {
                string msg = string.Format("Error copying {0} to {1}: {2}", sFullSourceFilename, sFullTargetFilename, RepresentException(ex));
                task.Log(msg);
                task.PostMessage(true, msg);
                return false;
            }

            task.Log(string.Format("File {1} copied from {0}", sFullSourceFilename, sFullTargetFilename));

            TotalBytesCopied += sourceFile.FileSize;
            TotalReplaced++;
            return true;
        }


        private bool CreateTargetDirectory(string sRelaDirname)
        {
            string sFullDirname = Path.Combine(StepTargetBaseDir, sRelaDirname);

            try
            {
                if( BackupTask.UseLongPaths)
                    LongPathDirectory.Create(sFullDirname);
                else
                    Directory.CreateDirectory(sFullDirname);
            }
            catch (Exception ex)
            {
                string msg = string.Format("Error creating directory {0}: {1}", sFullDirname, RepresentException(ex));
                task.Log(msg);
                task.PostMessage(true, msg);
                return false;
            }

            task.Log(string.Format("Directory {0} created", sFullDirname));

            TotalNewDirectories++;
            return true;
        }

        public bool RemoveTargetDirectory(string sRelaDirname)
        {
            string sFullDirname = Path.Combine(StepTargetBaseDir, sRelaDirname);

            try
            {
                if (BackupTask.UseLongPaths)
                    LongPathDirectory.Delete(sFullDirname);
                else
                    Directory.Delete(sFullDirname);
            }
            catch (Exception ex)
            {
                string msg = string.Format("Error deleting directory {0}: {1}", sFullDirname, RepresentException(ex));
                task.Log(msg);
                task.PostMessage(true, msg);
                return false;
            }

            task.Log(string.Format("Directory {0} deleted", sFullDirname));

            TotalRemovedDirectories++;
            return true;
        }

        public bool RemoveTargetFile(string sRelaFilename)
        {
            if (!DeletedFilesArchiveFolder.MoveFileToArchiveDir(sRelaFilename, ref amountProcessed, workloadAmount, ref latestCancelTest))
                return false;            

            task.Log( string.Format("File {0} removed",  Path.Combine(StepTargetBaseDir, sRelaFilename ) ));

            TotalRemovedFiles++;
            return true;
        }


        internal static string RepresentException(Exception ex)
        {
            if ( (ex is System.IO.IOException)
              || (ex is System.UnauthorizedAccessException) )
            {
                return ex.Message;
            }
            return ex.ToString();
        }
    }
}
