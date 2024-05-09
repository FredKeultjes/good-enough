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
using System.IO;
using System.Reflection;
using System.Text;


namespace Backup2014
{
    public class Utilities
    {
        public static string AssemblyDirectory
        {
            get
            {
                string path = Assembly.GetExecutingAssembly().Location;
                return Path.GetDirectoryName(path);
            }
        }

        
        public static bool ValueMatchesWildcard( string strValue, string wildCard )
        {
            if (wildCard == "*")
                return true;

            int posWildcard = 0;
            int iValueLength = strValue.Length;

            for( ; ; )
            {
	            // ignore leading spaces
                while (posWildcard < wildCard.Length && wildCard[posWildcard] == ' ') 
                    posWildcard++;

                int posNextComma = wildCard.IndexOf(',', posWildcard);

                int posElemEnd = posNextComma < 0 ? wildCard.Length : posNextComma;

                // ignore trailing spaces
                while (posElemEnd > posWildcard && wildCard [posElemEnd-1] == ' ')
                    posElemEnd--;

                int iElemLength = (int)(posElemEnd - posWildcard);
                string partText = wildCard.Substring(posWildcard, iElemLength);

                if( partText.StartsWith("*") )
                {
                    posWildcard++;
                    if( partText.EndsWith("*"))
                    {
                        if( strValue.IndexOf(partText.Substring(1, partText.Length-2), StringComparison.InvariantCultureIgnoreCase) >=0 )
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if( string.Compare(strValue, iValueLength-partText.Length+1, partText, 1, partText.Length-1, true )==0 )
	                    {
                            return true;
	                    }
                    }
                }
                else if (partText.EndsWith("*"))
                {
                    if  (string.Compare(strValue, 0, partText, 0, partText.Length - 1, true) == 0)
	                {
                        return true;
	                }
                }
                else
                {
                    if (iValueLength == partText.Length && iValueLength > 0 && string.Compare(strValue, 0, partText, 0, partText.Length, true) == 0)
	                {
                        return true;
	                }
                }

                if (posNextComma<0)
                    break;
                else
                    posWildcard = posNextComma + 1;
            }
            return false;
        }


        public static string PrePendTextLines(string text, string prefix)
        {
            var result = new StringBuilder();
            int prevPos = 0;
            for (int pos = 0; pos < text.Length; pos++)
            {
                char c = text[pos];
                if (c == '\n')
                {
                    result.Append(prefix);
                    result.AppendLine(text.Substring(prevPos, pos > 0 && text[pos - 1] == '\r' ? pos - prevPos - 1 : pos - prevPos));
                    prevPos = pos + 1;
                }
            }
            if (prevPos < text.Length)
            {
                result.Append(prefix);
                result.AppendLine(text.Substring(prevPos, text.Length-prevPos));
            }
            return result.ToString();
        }


        public static void EnsureDirectoriesCreated(string strTargetFilePath, int iStartPos)
        {
            int iPos = iStartPos;

            while (iPos < strTargetFilePath.Length)
            {
                int nextBackSlash = strTargetFilePath.IndexOf('\\', iPos);
                if (nextBackSlash < 0)
                {
                    break;
                }

                string sDirPath = strTargetFilePath.Substring(0, nextBackSlash);
                if (!Directory.Exists(sDirPath))
                {
                    Directory.CreateDirectory(sDirPath);
                }

                iPos = nextBackSlash + 1;
            }
        }

        public static string RepresentFileSize(long fileSize)
        {
            return fileSize >= 1073741824
            ? ((double)fileSize / 1073741824).ToString("0.0") + " GB"
            : fileSize >= 1048576
            ? ((double)fileSize / 1048576).ToString("0.0") + " MB"
            : (fileSize / 1024).ToString("d") + " kB";
        }

        public static string CodeDirectoryPathIntoFileName(string directoryPath)
        {
            if (directoryPath.Length >= 3 && char.IsLetter(directoryPath[0]) && directoryPath[1] == ':' && directoryPath[2] == '\\')
                return directoryPath[0] + "on" + System.Environment.MachineName + "_" + directoryPath.Substring(3).Replace("_", "__").Replace("\\", "_b");
            else
                return directoryPath.Replace(":", "").Replace("_", "__").Replace("\\", "_b");
        }

        public static bool IsFileNameCodedDirectoryPath(string fileName)
        {
            if (fileName.Length >= 5 && (fileName[0] >= 'C' && fileName[0] <= 'Z'))
            {
                string testFor = "on" + System.Environment.MachineName + "_";
                if (fileName.Length> testFor.Length && fileName.Substring(1, testFor.Length) == testFor)
                    return true;
            }
            else
            {
                if (fileName.StartsWith("_b_b") && fileName.Length >= 6 && (char.IsLetter(fileName[4])))
                    return true;
            }
            return false;
        }
    }
}
