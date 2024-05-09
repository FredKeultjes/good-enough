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
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Experimental.IO;

namespace Backup2014
{
    public class BackupWorkload
    {
        /// <summary>
        /// New directories to create. Weight is <see cref="BackupWorkload.WeightCreateTargetDirectory"/>.
        /// </summary>
        public readonly Dictionary<string, string> mapRelativeSourceDirectoriesLCaseRemaining = new Dictionary<string, string>();
        /// <summary>
        /// New files to copy. Weight is <see cref="BackupWorkload.WeightWriteFileByBytes"/>(size of source file);
        /// </summary>
        public readonly Dictionary<string, SourceFileInfo> mapRelaFileNameLCaseToSourceFile = new Dictionary<string, SourceFileInfo>();
        /// <summary>
        /// Target directories to remove. Weight is <see cref="BackupWorkload.WeightRemoveTargetDirectory"/>.
        /// </summary>
        public readonly List<string> TargetDirectoriesToRemove = new List<string>();
        /// <summary>
        /// Target files to remove. Weight is <see cref="BackupWorkload.WeightWriteFileByBytes"/>(size of target file);
        /// </summary>
        public readonly List<string> TargetRelaFilepathsToRemove = new List<string>();
        /// <summary>
        /// Target files to replace. Weight is <see cref="BackupWorkload.WeightWriteFileByBytes"/>(size of target file) + <see cref="BackupWorkload.WeightWriteFileByBytes"/>(size of source file);
        /// </summary>
        public readonly List<SourceFileInfo> TargetFilesToReplace = new List<SourceFileInfo>();


        private readonly BackupTaskStep taskStep;
        private readonly BackupTask task;
        public readonly SourceDefinition SourceDef;


        public BackupWorkload(BackupTaskStep taskStep, SourceDefinition sourceDef)
        {
            this.taskStep = taskStep;
            this.task = taskStep.task;
            this.SourceDef = sourceDef;
        }

        public const long WeightCreateTargetDirectory = 1000;
        public const long WeightRemoveTargetDirectory = 1000;

        public long WeightTargetFileToReplace(SourceFileInfo sourceFile)
        {
            string sFullTargetFilename = Path.Combine(taskStep.StepTargetBaseDir, sourceFile.RelaFilename);

            return WeightWriteFileByBytes(sourceFile.FileSize + (BackupTask.UseLongPaths ? new LongPathFileInfo(sFullTargetFilename).Length : new FileInfo(sFullTargetFilename).Length));
        }
        public static long WeightWriteFileByBytes(long nBytes)
        {
            return nBytes;
        }
        public long WeightSourceFileRemaining(SourceFileInfo sourceFile)
        {
            return WeightWriteFileByBytes(sourceFile.FileSize);
        }
        public long WeightTargetRelaFilepathsToRemove(string sRelaFilename)
        {
            string sFullSourceFilename = Path.Combine(taskStep.StepTargetBaseDir, sRelaFilename );
            return WeightWriteFileByBytes(BackupTask.UseLongPaths ? new LongPathFileInfo(sFullSourceFilename).Length : new FileInfo(sFullSourceFilename).Length);
        }

        public long WorkloadAmount()
        {
            long result = 0;
            foreach( SourceFileInfo sourceInfo in TargetFilesToReplace )
            {
                result += WeightTargetFileToReplace(sourceInfo);
            }
            result += mapRelativeSourceDirectoriesLCaseRemaining.Count * WeightCreateTargetDirectory;
            result += TargetDirectoriesToRemove.Count * WeightRemoveTargetDirectory;
            foreach (KeyValuePair<string,SourceFileInfo> elem in mapRelaFileNameLCaseToSourceFile)
            {
                result += WeightSourceFileRemaining(elem.Value);
            }
            foreach( string sRelaFilename in TargetRelaFilepathsToRemove )
            {
                result += WeightTargetRelaFilepathsToRemove(sRelaFilename);
            }
            return result;
        }

        public DialogResult AskConfirmation()
        {
            int iSourceFilesCount = mapRelaFileNameLCaseToSourceFile.Count;

            task.SetStatus("");

            StringBuilder sConfirmationMsg = new StringBuilder();

            if( TargetFilesToReplace.Count>0 )
            {
                if( sConfirmationMsg.Length>0 )
                {
                    sConfirmationMsg.Append(",\r\n");
                }
                sConfirmationMsg.Append( string.Format("replace {0} modified files from {1}", TargetFilesToReplace.Count, SourceDef.SourceDirectory ) );
            }
        
            int  iDirsToCreateSize = mapRelativeSourceDirectoriesLCaseRemaining.Count;
            if( iDirsToCreateSize>0 )
            {
                if( sConfirmationMsg.Length>0 )
                {
                    sConfirmationMsg.Append(",\r\n");
                }
                sConfirmationMsg.Append(string.Format("create {0} directories in {1}", iDirsToCreateSize, taskStep.StepTargetBaseDir));
            }

            if( iSourceFilesCount>0 )
            {
                if( sConfirmationMsg.Length>0 )
                {
                    sConfirmationMsg.Append(",\r\n");
                }
                sConfirmationMsg.Append( string.Format("copy {0} new files from {1}", iSourceFilesCount, SourceDef.SourceDirectory ) );
            }

            if( TargetRelaFilepathsToRemove.Count>0 )
            {
                if( sConfirmationMsg.Length>0 )
                {
                    sConfirmationMsg.Append(",\r\n");
                }
                sConfirmationMsg.Append(string.Format("remove {0} files in {1}", TargetRelaFilepathsToRemove.Count, taskStep.StepTargetBaseDir));
            }

            if( TargetDirectoriesToRemove.Count>0 )
            {
                if( sConfirmationMsg.Length>0 )
                {
                    sConfirmationMsg.Append(",\r\n");
                }
                sConfirmationMsg.Append( string.Format("remove {0} directories in {1}", TargetDirectoriesToRemove.Count, taskStep.StepTargetBaseDir ) );
            }

            if( sConfirmationMsg.Length>0 )
            {
                long amount = WorkloadAmount();
                return task.Confirm(string.Format("Do you really want to\r\n{0}? ({1})", sConfirmationMsg, Utilities.RepresentFileSize(amount) ));
            }
            return DialogResult.OK;
        }
    }
}
