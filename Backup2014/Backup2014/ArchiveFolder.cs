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
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Experimental.IO;

namespace Backup2014
{
    public class ArchiveFolderBuffer
    {
        public const int TransferBlockSize = 81920;
        public byte[] buffer;

        public ArchiveFolderBuffer()
        {
            buffer = new byte[TransferBlockSize];
        }
    }

    public class ArchiveFolder : IDisposable
    {
        private BackupTaskStep taskStep;
        private string archiveDir;
        private string stepSourceDirPrefix;


        private int currentRemoveFilesArchiveIndex = 0;
        private FileStream currentRemoveFilesArchiveStream = null;
        private long currentRemoveFilesArchiveStreamLength = 0;
        private ZipArchive currentRemoveFilesArchive = null;
        private long currentRemoveFilesArchiveSourceLength = 0;

        public ArchiveFolder(BackupTaskStep taskStep, string archiveDir, string stepSourceDirPrefix)
        {
            this.taskStep = taskStep;
            this.archiveDir = archiveDir;
            this.stepSourceDirPrefix = stepSourceDirPrefix;
        }

        public void Dispose()
        {
            if (currentRemoveFilesArchive != null)
            {
                currentRemoveFilesArchive.Dispose();
                currentRemoveFilesArchive = null;
            }
            if (currentRemoveFilesArchiveStream != null)
            {
                currentRemoveFilesArchiveStream.Dispose();
                currentRemoveFilesArchiveStream = null;
                currentRemoveFilesArchiveStreamLength = 0;
                currentRemoveFilesArchiveSourceLength = 0;
            }
        }

        public bool MoveFileToArchiveDir(string sRelaFilename, ref long amountProcessed, long workloadAmount, ref DateTime latestCancelTest)
        {
            const bool useCompressedSize = false;
            string sFullFilename = Path.Combine(taskStep.StepTargetBaseDir, sRelaFilename);

            ZipArchiveEntry zipArchiveEntry = null;
            long bytesRemaining = 0;
            try
            {
                // remove 'readonly', 'hidden' and 'system' flag
                long newFileLength;
                DateTime lastWriteTime;
                if( BackupTask.UseLongPaths )
                {
                    LongPathFileInfo newFileInfo = new LongPathFileInfo(sFullFilename);
                    newFileLength = newFileInfo.Length;
                    lastWriteTime = newFileInfo.LastWriteTime;
                    if ((newFileInfo.Attributes & (FileAttributes.ReadOnly | FileAttributes.Hidden | FileAttributes.System)) != 0)
                    {
                        LongPathFile.SetAttributes(sFullFilename, newFileInfo.Attributes & ~((FileAttributes.ReadOnly | FileAttributes.Hidden | FileAttributes.System)));
                    }
                }
                else
                {
                    FileInfo newFileInfo = new FileInfo(sFullFilename);
                    newFileLength = newFileInfo.Length;
                    lastWriteTime = newFileInfo.LastWriteTime;
                    if ((newFileInfo.Attributes & (FileAttributes.ReadOnly | FileAttributes.Hidden | FileAttributes.System)) != 0)
                    {
                        File.SetAttributes(sFullFilename, newFileInfo.Attributes & ~((FileAttributes.ReadOnly | FileAttributes.Hidden | FileAttributes.System)));
                    }
                }

                bytesRemaining = newFileLength;

                if (taskStep.task.Settings.ZipUpdatedAndDeletedFiles && (newFileLength < BackupTaskStep.MaxZipSize))
                {
                    if (currentRemoveFilesArchiveStream != null)
                    {

                        if ( useCompressedSize 
                             ? newFileLength + currentRemoveFilesArchiveSourceLength > BackupTaskStep.MaxZipSize 
                               && newFileLength + currentRemoveFilesArchiveStreamLength > BackupTaskStep.MaxZipSize
                             : newFileLength + currentRemoveFilesArchiveSourceLength > BackupTaskStep.MaxZipSourceSize )
                        {
                            currentRemoveFilesArchiveIndex++;
                            currentRemoveFilesArchive.Dispose();
                            currentRemoveFilesArchive = null;
                            currentRemoveFilesArchiveStream.Close();
                            currentRemoveFilesArchiveStream = null;
                            currentRemoveFilesArchiveStreamLength = 0;
                            currentRemoveFilesArchiveSourceLength = 0;
                        }
                    }

                    if (currentRemoveFilesArchiveStream == null)
                    {
                        currentRemoveFilesArchiveStream =  BackupTask.UseLongPaths
                                ? LongPathFile.Open(Path.Combine(archiveDir, string.Format("{0}_{1}.zip", currentRemoveFilesArchiveIndex, stepSourceDirPrefix)), FileMode.CreateNew, FileAccess.ReadWrite, FileShare.Read)
                                : new FileStream(Path.Combine(archiveDir, string.Format("{0}_{1}.zip", currentRemoveFilesArchiveIndex, stepSourceDirPrefix)), FileMode.CreateNew);
                        currentRemoveFilesArchive = new ZipArchive(currentRemoveFilesArchiveStream, ZipArchiveMode.Update, true);
                    }

                    string ext = Path.GetExtension(sFullFilename);
                    CompressionLevel compLevel;
                    switch (ext.ToLower())
                    {
                        case ".zip":
                        case ".wmv":
                        case ".mpg":
                            compLevel = CompressionLevel.NoCompression;
                            break;
                        default:
                            compLevel = CompressionLevel.Fastest;
                            break;
                    }

                    zipArchiveEntry = currentRemoveFilesArchive.CreateEntry(sRelaFilename, compLevel);
                    using (Stream zipStream = zipArchiveEntry.Open())
                    {
                        zipStream.SetLength(newFileLength);
                        using (FileStream reader = BackupTask.UseLongPaths
                                ? LongPathFile.Open(sFullFilename, FileMode.Open, FileAccess.Read, FileShare.Read)
                                : File.Open(sFullFilename, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            byte[] buffer = taskStep.task.ArchBuffer.buffer;
                            int bytesRead;

                            while ((bytesRead = reader.Read(buffer, 0, ArchiveFolderBuffer.TransferBlockSize)) > 0)
                            {
                                zipStream.Write(buffer, 0, bytesRead);
                                zipStream.Flush();
                                amountProcessed += BackupWorkload.WeightWriteFileByBytes(bytesRead);
                                bytesRemaining -= bytesRead;

                                taskStep.task.ShowProgress(workloadAmount, amountProcessed);

                                if (taskStep.task.IsCancelRequested(ref latestCancelTest))
                                {
                                    zipStream.Close();
                                    reader.Close();
                                    zipArchiveEntry.Delete();
                                    amountProcessed += BackupWorkload.WeightWriteFileByBytes(bytesRemaining);
                                    return false;
                                }
                            }
                        }
                    }

                    currentRemoveFilesArchiveSourceLength += newFileLength;
                    try
                    {
                        zipArchiveEntry.LastWriteTime = lastWriteTime;
                    }
                    catch(Exception ex)
                    {
                        string msg = string.Format("Ignoring LastWriteTime of {0} in archive: {1}", sFullFilename, BackupTaskStep.RepresentException(ex));
                        taskStep.task.Log(msg);
                    }

                    // close and re-open
                    if ( useCompressedSize && currentRemoveFilesArchiveSourceLength > BackupTaskStep.MaxZipSize)
                    {
                        
                        zipArchiveEntry = null;
                        if (currentRemoveFilesArchive != null)
                        {
                            currentRemoveFilesArchive.Dispose();
                            currentRemoveFilesArchive = null;
                        }
                        if (currentRemoveFilesArchiveStream != null)
                        {
                            currentRemoveFilesArchiveStream.Dispose();
                            currentRemoveFilesArchiveStream = null;
                        }
                        currentRemoveFilesArchiveStream = BackupTask.UseLongPaths
                                    ? LongPathFile.Open(Path.Combine(archiveDir, string.Format("{0}_{1}.zip", currentRemoveFilesArchiveIndex, stepSourceDirPrefix)), FileMode.Open, FileAccess.ReadWrite, FileShare.Read)
                                    : new FileStream(Path.Combine(archiveDir, string.Format("{0}_{1}.zip", currentRemoveFilesArchiveIndex, stepSourceDirPrefix)), FileMode.Open);
                        currentRemoveFilesArchive = new ZipArchive(currentRemoveFilesArchiveStream, ZipArchiveMode.Update, true);

                        if( currentRemoveFilesArchiveStreamLength==0)
                        {
                            currentRemoveFilesArchiveStreamLength = 0;
                            foreach( var curEntry in currentRemoveFilesArchive.Entries )
                            {
                                currentRemoveFilesArchiveStreamLength += curEntry.CompressedLength;
                            }
                        }
                        else
                        {
                            zipArchiveEntry = currentRemoveFilesArchive.GetEntry(sRelaFilename);
                            currentRemoveFilesArchiveStreamLength += zipArchiveEntry.CompressedLength;
                        }
                    }

                    if (BackupTask.UseLongPaths)
                    {
                        LongPathFile.Delete(sFullFilename);
                    }
                    else
                    {
                        File.Delete(sFullFilename);
                    }
                }
                else
                {
                    string strArchTargetFilePath = Path.Combine(Path.Combine(archiveDir, stepSourceDirPrefix), sRelaFilename);

                    Utilities.EnsureDirectoriesCreated(strArchTargetFilePath, archiveDir.Length + 1);
                    var sourceSize = BackupTask.UseLongPaths ? new LongPathFileInfo(sFullFilename).Length : new FileInfo(sFullFilename).Length;
                    if (BackupTask.UseLongPaths)
                    {
                        LongPathFile.Move(sFullFilename, strArchTargetFilePath);
                    }
                    else
                    {
                        File.Move(sFullFilename, strArchTargetFilePath);
                    }
                    amountProcessed += BackupWorkload.WeightWriteFileByBytes(sourceSize);
                    taskStep.task.ShowProgress(workloadAmount, amountProcessed);
                }
            }
            catch (Exception ex)
            {
                string msg = string.Format("Error removing file {0} to archive: {1}", sFullFilename, BackupTaskStep.RepresentException(ex));
                taskStep.task.Log(msg);
                taskStep.task.PostMessage(true, msg);
                amountProcessed += BackupWorkload.WeightWriteFileByBytes(bytesRemaining);
                if (zipArchiveEntry != null)
                {
                    zipArchiveEntry.Delete();
                }
                return false;
            }

            return true;
        }

    }
}
