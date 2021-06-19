//     Copyright (c) Microsoft Corporation.  All rights reserved.
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.Experimental.IO.Interop;
using Microsoft.Win32.SafeHandles;

namespace Microsoft.Experimental.IO {

    /// <summary>
    ///     Provides static methods for creating, copying, deleting, moving, and opening of files
    ///     with long paths, that is, paths that exceed 259 characters.
    /// </summary>
    public static class LongPathFile {

        /// <summary>
        ///     Returns a value indicating whether the specified path refers to an existing file.
        /// </summary>
        /// <param name="path">
        ///     A <see cref="String"/> containing the path to check.
        /// </param>
        /// <returns>
        ///     <see langword="true"/> if <paramref name="path"/> refers to an existing file; 
        ///     otherwise, <see langword="false"/>.
        /// </returns>
        /// <remarks>
        ///     Note that this method will return false if any error occurs while trying to determine 
        ///     if the specified file exists. This includes situations that would normally result in 
        ///     thrown exceptions including (but not limited to); passing in a file name with invalid 
        ///     or too many characters, an I/O error such as a failing or missing disk, or if the caller
        ///     does not have Windows or Code Access Security (CAS) permissions to to read the file.
        /// </remarks>
        public static bool Exists(string path) {

            bool isDirectory;
            if (LongPathCommon.Exists(path, out isDirectory)) {

                return !isDirectory;
            }

            return false;
        }

        /// <summary>
        ///     Deletes the specified file.
        /// </summary>
        /// <param name="path">
        ///      A <see cref="String"/> containing the path of the file to delete.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="path"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="path"/> is an empty string (""), contains only white 
        ///     space, or contains one or more invalid characters as defined in 
        ///     <see cref="Path.GetInvalidPathChars()"/>.
        ///     <para>
        ///         -or-
        ///     </para>
        ///     <paramref name="path"/> contains one or more components that exceed
        ///     the drive-defined maximum length. For example, on Windows-based 
        ///     platforms, components must not exceed 255 characters.
        /// </exception>
        /// <exception cref="PathTooLongException">
        ///     <paramref name="path"/> exceeds the system-defined maximum length. 
        ///     For example, on Windows-based platforms, paths must not exceed 
        ///     32,000 characters.
        /// </exception>
        /// <exception cref="FileNotFoundException">
        ///     <paramref name="path"/> could not be found.
        /// </exception>
        /// <exception cref="DirectoryNotFoundException">
        ///     One or more directories in <paramref name="path"/> could not be found.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        ///     The caller does not have the required access permissions.
        ///     <para>
        ///         -or-
        ///     </para>
        ///     <paramref name="path"/> refers to a file that is read-only.
        ///     <para>
        ///         -or-
        ///     </para>
        ///     <paramref name="path"/> is a directory.
        /// </exception>
        /// <exception cref="IOException">
        ///     <paramref name="path"/> refers to a file that is in use.
        ///     <para>
        ///         -or-
        ///     </para>
        ///     <paramref name="path"/> specifies a device that is not ready.
        /// </exception>
        public static void Delete(string path) {

            string normalizedPath = LongPathCommon.NormalizeLongPath(path);
            if (!NativeMethods.DeleteFile(normalizedPath)) {
                throw LongPathCommon.GetExceptionFromLastWin32Error();
            }
        }

        /// <summary>
        ///     Moves the specified file to a new location.
        /// </summary>
        /// <param name="sourcePath">
        ///     A <see cref="String"/> containing the path of the file to move.
        /// </param>
        /// <param name="destinationPath">
        ///     A <see cref="String"/> containing the new path of the file.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="sourcePath"/> and/or <paramref name="destinationPath"/> is 
        ///     <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="sourcePath"/> and/or <paramref name="destinationPath"/> is 
        ///     an empty string (""), contains only white space, or contains one or more 
        ///     invalid characters as defined in <see cref="Path.GetInvalidPathChars()"/>.
        ///     <para>
        ///         -or-
        ///     </para>
        ///     <paramref name="sourcePath"/> and/or <paramref name="destinationPath"/> 
        ///     contains one or more components that exceed the drive-defined maximum length. 
        ///     For example, on Windows-based platforms, components must not exceed 255 characters.
        /// </exception>
        /// <exception cref="PathTooLongException">
        ///     <paramref name="sourcePath"/> and/or <paramref name="destinationPath"/> 
        ///     exceeds the system-defined maximum length. For example, on Windows-based platforms, 
        ///     paths must not exceed 32,000 characters.
        /// </exception>
        /// <exception cref="FileNotFoundException">
        ///     <paramref name="sourcePath"/> could not be found.
        /// </exception>
        /// <exception cref="DirectoryNotFoundException">
        ///     One or more directories in <paramref name="sourcePath"/> and/or 
        ///     <paramref name="destinationPath"/> could not be found.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        ///     The caller does not have the required access permissions.
        /// </exception>
        /// <exception cref="IOException">
        ///     <paramref name="destinationPath"/> refers to a file that already exists.
        ///     <para>
        ///         -or-
        ///     </para>
        ///     <paramref name="sourcePath"/> and/or <paramref name="destinationPath"/> is a 
        ///     directory.
        ///     <para>
        ///         -or-
        ///     </para>
        ///     <paramref name="sourcePath"/> refers to a file that is in use.
        ///     <para>
        ///         -or-
        ///     </para>
        ///     <paramref name="sourcePath"/> and/or <paramref name="destinationPath"/> specifies 
        ///     a device that is not ready.
        /// </exception>
        public static void Move(string sourcePath, string destinationPath) {

            string normalizedSourcePath = LongPathCommon.NormalizeLongPath(sourcePath, "sourcePath");
            string normalizedDestinationPath = LongPathCommon.NormalizeLongPath(destinationPath, "destinationPath");

            if (!NativeMethods.MoveFile(normalizedSourcePath, normalizedDestinationPath))
                throw LongPathCommon.GetExceptionFromLastWin32Error();
        }

        /// <summary>
        ///     Copies the specified file to a specified new file, indicating whether to overwrite an existing file.
        /// </summary>
        /// <param name="sourcePath">
        ///     A <see cref="String"/> containing the path of the file to copy.
        /// </param>
        /// <param name="destinationPath">
        ///     A <see cref="String"/> containing the new path of the file.
        /// </param>
        /// <param name="overwrite">
        ///     <see langword="true"/> if <paramref name="destinationPath"/> should be overwritten 
        ///     if it refers to an existing file, otherwise, <see langword="false"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="sourcePath"/> and/or <paramref name="destinationPath"/> is 
        ///     <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="sourcePath"/> and/or <paramref name="destinationPath"/> is 
        ///     an empty string (""), contains only white space, or contains one or more 
        ///     invalid characters as defined in <see cref="Path.GetInvalidPathChars()"/>.
        ///     <para>
        ///         -or-
        ///     </para>
        ///     <paramref name="sourcePath"/> and/or <paramref name="destinationPath"/> 
        ///     contains one or more components that exceed the drive-defined maximum length. 
        ///     For example, on Windows-based platforms, components must not exceed 255 characters.
        /// </exception>
        /// <exception cref="PathTooLongException">
        ///     <paramref name="sourcePath"/> and/or <paramref name="destinationPath"/> 
        ///     exceeds the system-defined maximum length. For example, on Windows-based platforms, 
        ///     paths must not exceed 32,000 characters.
        /// </exception>
        /// <exception cref="FileNotFoundException">
        ///     <paramref name="sourcePath"/> could not be found.
        /// </exception>
        /// <exception cref="DirectoryNotFoundException">
        ///     One or more directories in <paramref name="sourcePath"/> and/or 
        ///     <paramref name="destinationPath"/> could not be found.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        ///     The caller does not have the required access permissions.
        ///     <para>
        ///         -or-
        ///     </para>
        ///     <paramref name="overwrite"/> is true and <paramref name="destinationPath"/> refers to a 
        ///     file that is read-only.
        /// </exception>
        /// <exception cref="IOException">
        ///     <paramref name="overwrite"/> is false and <paramref name="destinationPath"/> refers to 
        ///     a file that already exists.
        ///     <para>
        ///         -or-
        ///     </para>
        ///     <paramref name="sourcePath"/> and/or <paramref name="destinationPath"/> is a 
        ///     directory.
        ///     <para>
        ///         -or-
        ///     </para>
        ///     <paramref name="overwrite"/> is true and <paramref name="destinationPath"/> refers to 
        ///     a file that already exists and is in use.
        ///     <para>
        ///         -or-
        ///     </para>
        ///     <paramref name="sourcePath"/> refers to a file that is in use.
        ///     <para>
        ///         -or-
        ///     </para>
        ///     <paramref name="sourcePath"/> and/or <paramref name="destinationPath"/> specifies 
        ///     a device that is not ready.
        /// </exception>
        public static void Copy(string sourcePath, string destinationPath, bool overwrite) {

            string normalizedSourcePath = LongPathCommon.NormalizeLongPath(sourcePath, "sourcePath");
            string normalizedDestinationPath = LongPathCommon.NormalizeLongPath(destinationPath, "destinationPath");

            if (!NativeMethods.CopyFile(normalizedSourcePath, normalizedDestinationPath, !overwrite))
                throw LongPathCommon.GetExceptionFromLastWin32Error();
        }

        /// <summary>
        ///     Opens the specified file.
        /// </summary>
        /// <param name="path">
        ///     A <see cref="String"/> containing the path of the file to open.
        /// </param>
        /// <param name="access">
        ///     One of the <see cref="FileAccess"/> value that specifies the operations that can be 
        ///     performed on the file. 
        /// </param>
        /// <param name="mode">
        ///     One of the <see cref="FileMode"/> values that specifies whether a file is created
        ///     if one does not exist, and determines whether the contents of existing files are 
        ///     retained or overwritten.
        /// </param>
        /// <returns>
        ///     A <see cref="FileStream"/> that provides access to the file specified in 
        ///     <paramref name="path"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="path"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="path"/> is an empty string (""), contains only white 
        ///     space, or contains one or more invalid characters as defined in 
        ///     <see cref="Path.GetInvalidPathChars()"/>.
        ///     <para>
        ///         -or-
        ///     </para>
        ///     <paramref name="path"/> contains one or more components that exceed
        ///     the drive-defined maximum length. For example, on Windows-based 
        ///     platforms, components must not exceed 255 characters.
        /// </exception>
        /// <exception cref="PathTooLongException">
        ///     <paramref name="path"/> exceeds the system-defined maximum length. 
        ///     For example, on Windows-based platforms, paths must not exceed 
        ///     32,000 characters.
        /// </exception>
        /// <exception cref="DirectoryNotFoundException">
        ///     One or more directories in <paramref name="path"/> could not be found.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        ///     The caller does not have the required access permissions.
        ///     <para>
        ///         -or-
        ///     </para>
        ///     <paramref name="path"/> refers to a file that is read-only and <paramref name="access"/>
        ///     is not <see cref="FileAccess.Read"/>.
        ///     <para>
        ///         -or-
        ///     </para>
        ///     <paramref name="path"/> is a directory.
        /// </exception>
        /// <exception cref="IOException">
        ///     <paramref name="path"/> refers to a file that is in use.
        ///     <para>
        ///         -or-
        ///     </para>
        ///     <paramref name="path"/> specifies a device that is not ready.
        /// </exception>
        public static FileStream Open(string path, FileMode mode, FileAccess access) {

            return Open(path, mode, access, FileShare.None, 0, FileOptions.None);
        }

        /// <summary>
        ///     Opens the specified file.
        /// </summary>
        /// <param name="path">
        ///     A <see cref="String"/> containing the path of the file to open.
        /// </param>
        /// <param name="access">
        ///     One of the <see cref="FileAccess"/> value that specifies the operations that can be 
        ///     performed on the file. 
        /// </param>
        /// <param name="mode">
        ///     One of the <see cref="FileMode"/> values that specifies whether a file is created
        ///     if one does not exist, and determines whether the contents of existing files are 
        ///     retained or overwritten.
        /// </param>
        /// <param name="share">
        ///     One of the <see cref="FileShare"/> values specifying the type of access other threads 
        ///     have to the file. 
        /// </param>
        /// <returns>
        ///     A <see cref="FileStream"/> that provides access to the file specified in 
        ///     <paramref name="path"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="path"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="path"/> is an empty string (""), contains only white 
        ///     space, or contains one or more invalid characters as defined in 
        ///     <see cref="Path.GetInvalidPathChars()"/>.
        ///     <para>
        ///         -or-
        ///     </para>
        ///     <paramref name="path"/> contains one or more components that exceed
        ///     the drive-defined maximum length. For example, on Windows-based 
        ///     platforms, components must not exceed 255 characters.
        /// </exception>
        /// <exception cref="PathTooLongException">
        ///     <paramref name="path"/> exceeds the system-defined maximum length. 
        ///     For example, on Windows-based platforms, paths must not exceed 
        ///     32,000 characters.
        /// </exception>
        /// <exception cref="DirectoryNotFoundException">
        ///     One or more directories in <paramref name="path"/> could not be found.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        ///     The caller does not have the required access permissions.
        ///     <para>
        ///         -or-
        ///     </para>
        ///     <paramref name="path"/> refers to a file that is read-only and <paramref name="access"/>
        ///     is not <see cref="FileAccess.Read"/>.
        ///     <para>
        ///         -or-
        ///     </para>
        ///     <paramref name="path"/> is a directory.
        /// </exception>
        /// <exception cref="IOException">
        ///     <paramref name="path"/> refers to a file that is in use.
        ///     <para>
        ///         -or-
        ///     </para>
        ///     <paramref name="path"/> specifies a device that is not ready.
        /// </exception>
        public static FileStream Open(string path, FileMode mode, FileAccess access, FileShare share) {

            return Open(path, mode, access, share, 0, FileOptions.None);
        }

        /// <summary>
        ///     Opens the specified file.
        /// </summary>
        /// <param name="path">
        ///     A <see cref="String"/> containing the path of the file to open.
        /// </param>
        /// <param name="access">
        ///     One of the <see cref="FileAccess"/> value that specifies the operations that can be 
        ///     performed on the file. 
        /// </param>
        /// <param name="mode">
        ///     One of the <see cref="FileMode"/> values that specifies whether a file is created
        ///     if one does not exist, and determines whether the contents of existing files are 
        ///     retained or overwritten.
        /// </param>
        /// <param name="share">
        ///     One of the <see cref="FileShare"/> values specifying the type of access other threads 
        ///     have to the file. 
        /// </param>
        /// <param name="bufferSize">
        ///     An <see cref="Int32"/> containing the number of bytes to buffer for reads and writes
        ///     to the file, or 0 to specified the default buffer size, 1024.
        /// </param>
        /// <param name="options">
        ///     One or more of the <see cref="FileOptions"/> values that describes how to create or 
        ///     overwrite the file.
        /// </param>
        /// <returns>
        ///     A <see cref="FileStream"/> that provides access to the file specified in 
        ///     <paramref name="path"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="path"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="path"/> is an empty string (""), contains only white 
        ///     space, or contains one or more invalid characters as defined in 
        ///     <see cref="Path.GetInvalidPathChars()"/>.
        ///     <para>
        ///         -or-
        ///     </para>
        ///     <paramref name="path"/> contains one or more components that exceed
        ///     the drive-defined maximum length. For example, on Windows-based 
        ///     platforms, components must not exceed 255 characters.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     <paramref name="bufferSize"/> is less than 0.
        /// </exception>
        /// <exception cref="PathTooLongException">
        ///     <paramref name="path"/> exceeds the system-defined maximum length. 
        ///     For example, on Windows-based platforms, paths must not exceed 
        ///     32,000 characters.
        /// </exception>
        /// <exception cref="DirectoryNotFoundException">
        ///     One or more directories in <paramref name="path"/> could not be found.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        ///     The caller does not have the required access permissions.
        ///     <para>
        ///         -or-
        ///     </para>
        ///     <paramref name="path"/> refers to a file that is read-only and <paramref name="access"/>
        ///     is not <see cref="FileAccess.Read"/>.
        ///     <para>
        ///         -or-
        ///     </para>
        ///     <paramref name="path"/> is a directory.
        /// </exception>
        /// <exception cref="IOException">
        ///     <paramref name="path"/> refers to a file that is in use.
        ///     <para>
        ///         -or-
        ///     </para>
        ///     <paramref name="path"/> specifies a device that is not ready.
        /// </exception>
        public static FileStream Open(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize, FileOptions options) {
    
            const int DefaultBufferSize = 1024;

            if (bufferSize == 0)
                bufferSize = DefaultBufferSize;

            string normalizedPath = LongPathCommon.NormalizeLongPath(path);

            SafeFileHandle handle = LongPathCommon.GetFileHandle(normalizedPath, mode, access, share, options);

            return new FileStream(handle, access, bufferSize, (options & FileOptions.Asynchronous) == FileOptions.Asynchronous);
        }

        public static void AppendAllText(string path, string contents)
        {
            using(FileStream outputStream = LongPathFile.Open(path, FileMode.Append, FileAccess.Write, FileShare.Read))
            {
                using (StreamWriter writer = new StreamWriter(outputStream))
                {
                    writer.Write(contents);
                }
            }
        }

        public static FileAttributes GetAttributes(string path)
        {
            FileAttributes attributes;
            int errorCode = LongPathCommon.TryGetFileAttributes(LongPathCommon.NormalizeLongPath(path), out attributes);
            if (errorCode != 0)
            {
                throw LongPathCommon.GetExceptionFromWin32Error(errorCode, path);
            }
            return attributes;
        }
        public static void SetAttributes(string path, FileAttributes attributes)
        {
            FileAttributes tempAttributes = attributes;
            if( !NativeMethods.SetFileAttributes(LongPathCommon.NormalizeLongPath(path), tempAttributes) )
            {
                throw LongPathCommon.GetExceptionFromLastWin32Error(path);
            }
        }        

        public static DateTime GetLastWriteTime(string path)
        {
#if true
            NativeMethods.WIN32_FIND_DATA findData;

            using (SafeFindHandle handle = NativeMethods.FindFirstFile(LongPathCommon.NormalizeLongPath(path), out findData))
            {
                if (handle.IsInvalid) 
                {
                    throw LongPathCommon.GetExceptionFromLastWin32Error(path);
                }
                return DateTime.FromFileTime((((Int64)findData.ftLastWriteTime.dwHighDateTime) << 32 | (UInt32)findData.ftLastWriteTime.dwLowDateTime));
            }
#else
            using (SafeFileHandle handle = LongPathCommon.GetFileHandle(LongPathCommon.NormalizeLongPath(path), FileMode.Open, FileAccess.Read, FileShare.ReadWrite, FileOptions.None))
            {
                if (handle.IsInvalid)
                {
                    throw LongPathCommon.GetExceptionFromLastWin32Error(path);
                }
                else
                {
                    long ftLastWriteTime;
                    if (!NativeMethods.GetFileTime_LastWriteTime(handle, IntPtr.Zero, IntPtr.Zero, out ftLastWriteTime))
                    {
                        throw LongPathCommon.GetExceptionFromLastWin32Error(path);
                    }
                    return DateTime.FromFileTime(ftLastWriteTime);
                }
            }
#endif
        }
        public static DateTime GetLastAccessTime(string path)
        {
#if true
            NativeMethods.WIN32_FIND_DATA findData;

            using (SafeFindHandle handle = NativeMethods.FindFirstFile(LongPathCommon.NormalizeLongPath(path), out findData))
            {
                if (handle.IsInvalid) 
                {
                    throw LongPathCommon.GetExceptionFromLastWin32Error(path);
                }
                return DateTime.FromFileTime((((Int64)findData.ftLastAccessTime.dwHighDateTime) << 32 | (UInt32)findData.ftLastAccessTime.dwLowDateTime));
            }
#else
            using (SafeFileHandle handle = LongPathCommon.GetFileHandle(LongPathCommon.NormalizeLongPath(path), FileMode.Open, FileAccess.Read, FileShare.ReadWrite, FileOptions.None))
            {
                if (handle.IsInvalid)
                {
                    throw LongPathCommon.GetExceptionFromLastWin32Error(path);
                }
                else
                {
                    long ftLastAccessTime;
                    if (!NativeMethods.GetFileTime_LastAccessTime(handle, IntPtr.Zero, out ftLastAccessTime, IntPtr.Zero))
                    {
                        throw LongPathCommon.GetExceptionFromLastWin32Error(path);
                    }
                    return DateTime.FromFileTime(ftLastAccessTime);
                }
            }
#endif
        }
        public static DateTime GetCreationTime(string path)
        {
#if true
            NativeMethods.WIN32_FIND_DATA findData;

            using (SafeFindHandle handle = NativeMethods.FindFirstFile(LongPathCommon.NormalizeLongPath(path), out findData))
            {
                if (handle.IsInvalid) 
                {
                    throw LongPathCommon.GetExceptionFromLastWin32Error(path);
                }
                return DateTime.FromFileTime((((Int64)findData.ftCreationTime.dwHighDateTime) << 32 | (UInt32)findData.ftCreationTime.dwLowDateTime));
            }
#else
            using (SafeFileHandle handle = LongPathCommon.GetFileHandle(LongPathCommon.NormalizeLongPath(path), FileMode.Open, FileAccess.Read, FileShare.ReadWrite, FileOptions.None))
            {
                if (handle.IsInvalid)
                {
                    throw LongPathCommon.GetExceptionFromLastWin32Error(path);
                }
                else
                {
                    long ftCreationTime;
                    if (!NativeMethods.GetFileTime_CreationTime(handle, out ftCreationTime, IntPtr.Zero, IntPtr.Zero))
                    {
                        throw LongPathCommon.GetExceptionFromLastWin32Error(path);
                    }
                    return DateTime.FromFileTime(ftCreationTime);
                }
            }
#endif
        }

        public static void SetLastWriteTime(string path, DateTime newLastWriteTime)
        {
            using (SafeFileHandle handle = LongPathCommon.GetFileHandle(LongPathCommon.NormalizeLongPath(path), FileMode.Open, FileAccess.Write, FileShare.ReadWrite, FileOptions.None))
            {
                if (handle.IsInvalid)
                {
                    throw LongPathCommon.GetExceptionFromLastWin32Error(path);
                }
                else
                {
                    long ftLastWriteTime = newLastWriteTime.ToFileTime();
                    if (!NativeMethods.SetFileTime_LastWriteTime(handle, IntPtr.Zero, IntPtr.Zero, ref ftLastWriteTime))
                    {
                        throw LongPathCommon.GetExceptionFromLastWin32Error(path);
                    }
                }
            }
        }
    }
}
