//     Copyright (c) Microsoft Corporation.  All rights reserved.
using System;
using System.Diagnostics.Contracts;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
//using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;
using Microsoft.Experimental.IO.Interop;

namespace Microsoft.Experimental.IO {

    internal static class LongPathCommon {

        internal static string NormalizeSearchPattern(string searchPattern) {
            if (String.IsNullOrEmpty(searchPattern) || searchPattern == ".")
                return "*";

            return searchPattern;
        }

        internal static string NormalizeLongPath(string path) {

            return NormalizeLongPath(path, "path");
        }

        // Normalizes path (can be longer than MAX_PATH) and adds \\?\ long path prefix
        internal static string NormalizeLongPath(string path, string parameterName) {

            if (path == null)
                throw new ArgumentNullException(parameterName);

            if (path.Length == 0)
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, "'{0}' cannot be an empty string.", parameterName), parameterName);

            if (Path.IsPathRooted(path))
            {
                if (path.Length > NativeMethods.MAX_LONG_PATH)
                {
                    throw LongPathCommon.GetExceptionFromWin32Error(NativeMethods.ERROR_FILENAME_EXCED_RANGE, parameterName);
                }

                return AddLongPathPrefix(path);
            }
            else
            {
                StringBuilder buffer = new StringBuilder(path.Length + 1); // Add 1 for NULL
                uint length = NativeMethods.GetFullPathName(path, (uint)buffer.Capacity, buffer, IntPtr.Zero);
                if (length > buffer.Capacity)
                {
                    // Resulting path longer than our buffer, so increase it

                    buffer.Capacity = (int)length;
                    length = NativeMethods.GetFullPathName(path, length, buffer, IntPtr.Zero);
                }

                if (length == 0)
                {
                    throw LongPathCommon.GetExceptionFromLastWin32Error(parameterName);
                }

                if (length > NativeMethods.MAX_LONG_PATH)
                {
                    throw LongPathCommon.GetExceptionFromWin32Error(NativeMethods.ERROR_FILENAME_EXCED_RANGE, parameterName);
                }

                return AddLongPathPrefix(buffer.ToString());
            }
        }

        internal static bool TryNormalizeLongPath(string path, out string result) {

            try {

                result = NormalizeLongPath(path);
                return true;
            }
            catch (ArgumentException) {
            }
            catch (PathTooLongException) {
            }

            result = null;
            return false;
        }

        private static string AddLongPathPrefix(string path)
        {
            if (path.StartsWith(@"\\"))
            { // if we have been passed a UNC style path (server) prepend \\?\UNC\ but we need to replace the \\ with a single \
                return NativeMethods.LongPathUNCPrefix + path.Substring(2);
            }
            else //assume a standard path (ie C:\windows) and prepend \\?\ to it
            {
                return NativeMethods.LongPathPrefix + path;
            }
        }

        internal static string RemoveLongPathPrefix(string normalizedPath)
        {
            if (normalizedPath.StartsWith(NativeMethods.LongPathUNCPrefix)) // if we have been supplied a path with the \\?\UNC\ prefix
            {
                return @"\\" + normalizedPath.Substring(NativeMethods.LongPathUNCPrefix.Length);
            }
            else if (normalizedPath.StartsWith(NativeMethods.LongPathPrefix))  // if we have been supplied with the \\?\ prefix
            {
                return normalizedPath.Substring(NativeMethods.LongPathPrefix.Length);
            }

            return normalizedPath;

        }
        internal static bool Exists(string path, out bool isDirectory) {

            string normalizedPath;
            if (TryNormalizeLongPath(path, out normalizedPath)) {

                FileAttributes attributes;
                int errorCode = TryGetFileAttributes(normalizedPath, out attributes);
                if (errorCode == 0) {
                    isDirectory = LongPathDirectory.IsDirectory(attributes);
                    return true;
                }
            }

            isDirectory = false;
            return false;
        }

        internal static int TryGetDirectoryAttributes(string normalizedPath, out FileAttributes attributes) {

            int errorCode = TryGetFileAttributes(normalizedPath, out attributes);
            if (!LongPathDirectory.IsDirectory(attributes))
                errorCode = NativeMethods.ERROR_DIRECTORY;

            return errorCode;
        }

        internal static int TryGetFileAttributes(string normalizedPath, out FileAttributes attributes) {
            // NOTE: Don't be tempted to use FindFirstFile here, it does not work with root directories

            attributes = NativeMethods.GetFileAttributes(normalizedPath);
            if ((int)attributes == NativeMethods.INVALID_FILE_ATTRIBUTES)
                return System.Runtime.InteropServices.Marshal.GetLastWin32Error();

            return 0;
        }


        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "False positive")]
        internal static SafeFileHandle GetFileHandle(string normalizedPath, FileMode mode, FileAccess access, FileShare share, FileOptions options)
        {
            NativeMethods.EFileAccess underlyingAccess = GetUnderlyingAccess(access);

            SafeFileHandle handle;
            if (mode == FileMode.Append)
            {
                handle = NativeMethods.CreateFile(normalizedPath, underlyingAccess, (uint)share, IntPtr.Zero, (uint)FileMode.OpenOrCreate, (uint)options, IntPtr.Zero);
                if (handle.IsInvalid)
                    throw LongPathCommon.GetExceptionFromLastWin32Error();
                if( !NativeMethods.SetFilePointerEx_NoNewFilePointer(handle, 0, IntPtr.Zero, 2) )
                    throw LongPathCommon.GetExceptionFromLastWin32Error();
            }
            else
            {
                handle = NativeMethods.CreateFile(normalizedPath, underlyingAccess, (uint)share, IntPtr.Zero, (uint)mode, (uint)options, IntPtr.Zero);
                if (handle.IsInvalid)
                    throw LongPathCommon.GetExceptionFromLastWin32Error();
            }
            return handle;
        }

        internal static NativeMethods.EFileAccess GetUnderlyingAccess(FileAccess access)
        {

            switch (access)
            {
                case FileAccess.Read:
                    return NativeMethods.EFileAccess.GenericRead;

                case FileAccess.Write:
                    return NativeMethods.EFileAccess.GenericWrite;

                case FileAccess.ReadWrite:
                    return NativeMethods.EFileAccess.GenericRead | NativeMethods.EFileAccess.GenericWrite;

                default:
                    throw new ArgumentOutOfRangeException("access");
            }
        }


        internal static Exception GetExceptionFromLastWin32Error() {
            return GetExceptionFromLastWin32Error("path");
        }

        internal static Exception GetExceptionFromLastWin32Error(string parameterName) {
            return GetExceptionFromWin32Error(System.Runtime.InteropServices.Marshal.GetLastWin32Error(), parameterName);
        }

        internal static Exception GetExceptionFromWin32Error(int errorCode) {
            return GetExceptionFromWin32Error(errorCode, "path");
        }

        internal static Exception GetExceptionFromWin32Error(int errorCode, string parameterName) {

            string message = GetMessageFromErrorCode(errorCode);

            switch (errorCode) {

                case NativeMethods.ERROR_FILE_NOT_FOUND:
                    return new FileNotFoundException(message);

                case NativeMethods.ERROR_PATH_NOT_FOUND:
                    return new DirectoryNotFoundException(message);

                case NativeMethods.ERROR_ACCESS_DENIED:
                    return new UnauthorizedAccessException(message);

                case NativeMethods.ERROR_FILENAME_EXCED_RANGE:
                    return new PathTooLongException(message);

                case NativeMethods.ERROR_INVALID_DRIVE:
                    return new DriveNotFoundException(message);

                case NativeMethods.ERROR_OPERATION_ABORTED:
                    return new OperationCanceledException(message);

                case NativeMethods.ERROR_INVALID_NAME:
                    return new ArgumentException(message, parameterName);

                default:
                    return new IOException(message, NativeMethods.MakeHRFromErrorCode(errorCode));

            }
        }

        private static string GetMessageFromErrorCode(int errorCode) {

            StringBuilder buffer = new StringBuilder(512);

            int bufferLength = NativeMethods.FormatMessage(NativeMethods.FORMAT_MESSAGE_IGNORE_INSERTS | NativeMethods.FORMAT_MESSAGE_FROM_SYSTEM | NativeMethods.FORMAT_MESSAGE_ARGUMENT_ARRAY, IntPtr.Zero, errorCode, 0, buffer, buffer.Capacity, IntPtr.Zero);

            Contract.Assert(bufferLength != 0);

            return buffer.ToString();
        }
    }
}
