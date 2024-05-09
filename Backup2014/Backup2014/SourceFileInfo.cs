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
using System.IO;

namespace Backup2014
{
    public class SourceFileInfo
    {
        public readonly string FilePath;
	    public readonly DateTime tmLastWrite;
        public readonly long FileSize;
        public readonly bool IsReadOnly;
        private readonly SourceDefinition sourceDef;
        public readonly string RelaFilename;

        public SourceFileInfo(SourceDefinition sourceDef, string filePath)
        {
            this.sourceDef = sourceDef;
            this.FilePath = filePath;
            if (BackupTask.UseLongPaths)
            {
                LongPathFileInfo info = new LongPathFileInfo(filePath);
                FileSize = info.Length;
                IsReadOnly = (info.Attributes & FileAttributes.ReadOnly) != 0;
                tmLastWrite = info.LastWriteTime;
            }
            else
            {
                FileInfo info = new FileInfo(filePath);
                FileSize = info.Length;
                IsReadOnly = (info.Attributes & FileAttributes.ReadOnly) != 0;
                tmLastWrite = info.LastWriteTime;
            }
            int iPrefixLength = sourceDef.SourceDirectory.Length;
            if (iPrefixLength > 0 && !sourceDef.SourceDirectory.EndsWith('\\'))
                iPrefixLength++;

            RelaFilename = FilePath.Substring(iPrefixLength);
        }
    }
}
