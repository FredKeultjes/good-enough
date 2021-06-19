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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThumbnailGenerator
{
    public static class Utilities
    {
        public static string RepresentFileSize(long fileSize)
        {
            return fileSize >= 1073741824
            ? ((double)fileSize / 1073741824).ToString("0.0") + " GB"
            : fileSize >= 1048576
            ? ((double)fileSize / 1048576).ToString("0.0") + " MB"
            : (fileSize / 1024).ToString("d") + " kB";
        }
    }
}
