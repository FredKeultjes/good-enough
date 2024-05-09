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

namespace Backup2014
{
    public class SourceDefinition
    {
        public string SourceDirectory = string.Empty;
        public string Exclusions = string.Empty;

        public SourceDefinition()
        {
            this.SourceDirectory = string.Empty;
            this.Exclusions = string.Empty;
        }

        public SourceDefinition(string sourceDirectory, string exclusions)
        {
            this.SourceDirectory = sourceDirectory;
            this.Exclusions = exclusions;
        }

        public bool CheckSourceExistence(BackupTaskStep taskStep)
        {
            BackupTask task = taskStep.task;
            // scan source

            if (!(BackupTask.UseLongPaths ? LongPathDirectory.Exists(SourceDirectory) : Directory.Exists(SourceDirectory)))
            {
                string msg = string.Format("Cannot find source directory {0}. Scanning aborted", SourceDirectory);
                task.PostMessage(true, msg);
                task.Log(msg);
                return false; // note: unavailability of source is fatal
            }
            return true;
        }

        public bool ScanSource(BackupTaskStep taskStep, BackupWorkload result)
        {
            BackupTask task = taskStep.task;
             // scan source

            if (!(BackupTask.UseLongPaths ? LongPathDirectory.Exists(SourceDirectory) : Directory.Exists(SourceDirectory)))
            {
                string msg = string.Format("Cannot find source directory {0}. Scanning aborted", SourceDirectory);
                task.PostMessage(true, msg);
                task.Log(msg);
                return false; // note: unavailability of source is fatal
            }

            task.SetStatus( string.Format("Scanning Source Directory {0}", SourceDirectory));

            List<string> arScanWorkload = new List<string>();  // to avoid recursive calls
            arScanWorkload.Add(SourceDirectory);

            try
            {
                int iPrefixLength = SourceDirectory.Length;
                if (iPrefixLength > 0 && !SourceDirectory.EndsWith("\\"))
                    iPrefixLength++;

                DateTime latestCancelTest = DateTime.Now;

                for (int iDirIndex = 0; iDirIndex < arScanWorkload.Count; iDirIndex++)
                {
                    string sCurrentRoot = arScanWorkload[iDirIndex];
                    string[] subFolders = null;
                    string[] files = null;
                    try
                    {
                        subFolders = BackupTask.UseLongPaths ? LongPathDirectory.GetDirectories(sCurrentRoot)  : Directory.GetDirectories(sCurrentRoot);

                        if (task.IsCancelRequested(ref latestCancelTest))
                            return false;

                        files = BackupTask.UseLongPaths ? LongPathDirectory.GetFiles(sCurrentRoot) : Directory.GetFiles(sCurrentRoot);

                        if (task.IsCancelRequested(ref latestCancelTest))
                            return false;
                    }
                    catch(UnauthorizedAccessException)
                    {
                        task.Log(string.Format("Skipping inaccessable folder {0}", sCurrentRoot) );
                    }

                    if (subFolders!=null )
                    { 
                        foreach (string curDir in subFolders)
                        {
                            if (((BackupTask.UseLongPaths ? new LongPathFileInfo(curDir).Attributes : new FileInfo(curDir).Attributes) & FileAttributes.ReparsePoint) == 0)
                            {
                                if (!Utilities.ValueMatchesWildcard(curDir, Exclusions))
                                {
                                    string relaDirName = curDir.Substring(iPrefixLength);
                                    result.mapRelativeSourceDirectoriesLCaseRemaining.Add(relaDirName.ToLower(), relaDirName);
                                    arScanWorkload.Add(curDir);

                                    taskStep.workloadAmount += BackupWorkload.WeightCreateTargetDirectory;
                                    task.ShowProgress(taskStep.workloadAmount, taskStep.amountProcessed);
                                }
                            }
                        }
                    }

                    if (files != null)
                    {
                        foreach (string filePath in files)
                        {
                            if (!Utilities.ValueMatchesWildcard(filePath, Exclusions))
                            {
                                SourceFileInfo newElem = new SourceFileInfo(this, filePath);
                                result.mapRelaFileNameLCaseToSourceFile.Add(newElem.RelaFilename.ToLower(), newElem);

                                taskStep.workloadAmount += BackupWorkload.WeightWriteFileByBytes(newElem.FileSize);
                                task.ShowProgress(taskStep.workloadAmount, taskStep.amountProcessed);
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                string msg = string.Format("Error scanning source directory {0}: {1}", SourceDirectory, BackupTaskStep.RepresentException(ex));
                task.PostMessage(true, msg);
                task.Log(msg);
                return false;
            }
            return true;
        }
    }
}
