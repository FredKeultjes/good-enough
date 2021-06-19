using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Experimental.IO.Interop;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;

namespace Microsoft.Experimental.IO
{
    public class LongPathFileInfo
    {
        private string path;

        public LongPathFileInfo(string path)
        {
            this.path = path;
        }

        public bool Exists
        {
            get
            {
                string normalizedPath;
                if (!LongPathCommon.TryNormalizeLongPath(path, out normalizedPath))
                    return false;
                FileAttributes attributes;
                int errorCode = LongPathCommon.TryGetFileAttributes(normalizedPath, out attributes);
                return errorCode == 0;
            }
        }

        public FileAttributes Attributes
        {
            get
            {
                return LongPathFile.GetAttributes(path);
            }
        }

        public DateTime LastWriteTime
        {
            get
            {
                return LongPathFile.GetLastWriteTime(path);
            }
        }
        public DateTime LastAccessTime
        {
            get
            {
                return LongPathFile.GetLastAccessTime(path);
            }
        }
        public DateTime CreationTime
        {
            get
            {
                return LongPathFile.GetCreationTime(path);
            }
        }

        public long Length
        {
            get
            {
#if true
                NativeMethods.WIN32_FIND_DATA findData;

                using (SafeFindHandle handle = NativeMethods.FindFirstFile(LongPathCommon.NormalizeLongPath(path), out findData))
                {
                    if (handle.IsInvalid) 
                    {
                        throw LongPathCommon.GetExceptionFromLastWin32Error(path);
                    }
                    return (((Int64)findData.nFileSizeHigh) << 32 | (UInt32)findData.nFileSizeLow);
                }
#else
                using (SafeFileHandle handle = LongPathCommon.GetFileHandle(LongPathCommon.NormalizeLongPath(path), FileMode.Open, FileAccess.Read, FileShare.ReadWrite, FileOptions.None))
                {
                    if (handle.IsInvalid)
                    {
                        throw LongPathCommon.GetExceptionFromLastWin32Error(path);
                    }
                    long fileLength;
                    if (!NativeMethods.GetFileSizeEx(handle, out fileLength))
                    {
                        throw LongPathCommon.GetExceptionFromLastWin32Error(path);
                    }
                    return fileLength;
                }
#endif
            }
        }
    }
}
