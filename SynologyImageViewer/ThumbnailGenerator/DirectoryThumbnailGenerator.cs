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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;

namespace ThumbnailGenerator
{
    public class GeneratorSettings
    {
        public bool GenerateAlways;
        public int ThumbnailSize;
        public int ThumbnailsPerSheet;
        public int PreviewWidth;
        public int NumberOfPreviewsPerSheet;
        public bool GenerateHtmlPages;
    }
    public class ProgressUpdater
    {
        public int FileCount;
        private Control ctlUpdateFileCount;
        private DateTime NextUpdate;

        public ProgressUpdater(Control ctlUpdateFileCount)
        {
            this.ctlUpdateFileCount = ctlUpdateFileCount;
            FileCount = 0;
            NextUpdate = DateTime.Now;
        }

        public void IncrementFileCount()
        {
            FileCount++;
            Update(false);
        }

        public void Update(bool force)
        {
            if( force || NextUpdate<=DateTime.Now)
            {
                ctlUpdateFileCount.Text = ((FileCount+1)/2).ToString();
                ctlUpdateFileCount.Invalidate();
                ctlUpdateFileCount.Update();
                ctlUpdateFileCount.Refresh();
                Application.DoEvents();

                NextUpdate = DateTime.Now.AddSeconds(1);
            }
        }
    }


    public static class DirectoryThumbnailGenerator
    {
        public static void Generate(string rootPath, GeneratorSettings settings, ProgressUpdater progress)
        {
            GenerateDir(rootPath, rootPath, settings, progress);
            progress.Update(true);
        }

        private static void GenerateDir(string rootPath, string dirPath, GeneratorSettings settings, ProgressUpdater progress)
        {
            List<string> imgOrMediaFilePaths = new List<string>();
            List<string> otherFilePaths = new List<string>();
            List<string> thmFiles = new List<string>();
            DateTime? lastModificationDate = null;

            foreach (var curPath in Directory.GetFiles(dirPath))
            {
                var curFileName = Path.GetFileName(curPath);
                if (curFileName.StartsWith("thmh200_") || curFileName.StartsWith("thmv200_") || curFileName.StartsWith("thmv800_") || curFileName.EndsWith(".prv.html") || curFileName== "index.html" || curFileName == "viewer.css" || curFileName == "thm_index.js")
                {
                    thmFiles.Add(curPath);
                }
                else if (curFileName.EndsWith(".jpg", StringComparison.InvariantCultureIgnoreCase)
                       || curFileName.EndsWith(".jpeg", StringComparison.InvariantCultureIgnoreCase)
                       || curFileName.EndsWith(".png", StringComparison.InvariantCultureIgnoreCase)
                       || curFileName.EndsWith(".gif", StringComparison.InvariantCultureIgnoreCase)
                       || curFileName.EndsWith(".tif", StringComparison.InvariantCultureIgnoreCase)
                       || curFileName.EndsWith(".tiff", StringComparison.InvariantCultureIgnoreCase)
                       || curFileName.EndsWith(".mp3", StringComparison.InvariantCultureIgnoreCase)
                       || curFileName.EndsWith(".mp4", StringComparison.InvariantCultureIgnoreCase))
                {
                    imgOrMediaFilePaths.Add(curPath);
                    var curDate = File.GetLastWriteTime(curPath);
                    if (!lastModificationDate.HasValue || curDate > lastModificationDate.Value)
                    {
                        lastModificationDate = curDate;
                    }
                }
                else
                {
                    otherFilePaths.Add(curPath);
                }
            }

            string scriptFilePath = Path.Combine(dirPath, "thm_index.js");
            string indexFilePath = Path.Combine(dirPath, "index.html");
            string cssFilePath = Path.Combine(dirPath, "viewer.css");

            if (imgOrMediaFilePaths.Count > 0)
            {
                var doGenerate = settings.GenerateAlways
                    ? true
                    : settings.GenerateHtmlPages
                    ? (!File.Exists(indexFilePath) || File.GetLastWriteTime(indexFilePath) < lastModificationDate)
                    : (!File.Exists(scriptFilePath) || File.GetLastWriteTime(scriptFilePath) < lastModificationDate);

                if ( doGenerate )
                {
                    foreach (var elem in thmFiles)
                    {
                        File.Delete(elem);
                    }

                    imgOrMediaFilePaths.Sort();
                    List<string> imgFilePaths = imgOrMediaFilePaths.Where(x => !x.EndsWith(".mp3", StringComparison.InvariantCultureIgnoreCase) && !x.EndsWith(".mp4", StringComparison.InvariantCultureIgnoreCase)).ToList();

                    DirectoryContactSheets sheetsSmall = new DirectoryContactSheets(settings.ThumbnailSize, false, settings.ThumbnailsPerSheet, imgFilePaths, dirPath);
                    DirectoryContactSheets sheetsBig = new DirectoryContactSheets(settings.PreviewWidth, true, settings.NumberOfPreviewsPerSheet, imgFilePaths, dirPath);
                    {
                        sheetsSmall.Generate(progress);
                        sheetsBig.Generate(progress);

                        if (!settings.GenerateHtmlPages)
                        {
                            using (var stream = File.CreateText(scriptFilePath))
                            {
                                stream.Write(sheetsSmall.GetContactSheetJavaScript());
                                stream.Write(sheetsBig.GetContactSheetJavaScript());
                                stream.Close();
                            }
                        }

                        // generate file preview page

                        if (settings.GenerateHtmlPages)
                        {
                            for (var index = 0; index < imgOrMediaFilePaths.Count; index++)
                            {
                                var curFilePath = imgOrMediaFilePaths[index];

                                GenerateFilePreviewPage(curFilePath + ".prv.html", curFilePath,
                                    index > 0 ? imgOrMediaFilePaths[index - 1] : null,
                                    index < imgOrMediaFilePaths.Count - 1 ? imgOrMediaFilePaths[index + 1] : null,
                                    sheetsSmall,
                                    sheetsBig);
                            }

                            GenerateFileIndexPage(indexFilePath, dirPath, imgOrMediaFilePaths, otherFilePaths,  sheetsSmall, sheetsBig, dirPath.Length>rootPath.Length);
                        }
                    }
                }
            }
            else
            {
                if( File.Exists(scriptFilePath))
                    File.Delete(scriptFilePath);

                foreach (var elem in thmFiles)
                {
                    File.Delete(elem);
                }

                if (settings.GenerateHtmlPages)
                {
                    GenerateFileIndexPage(indexFilePath, dirPath, imgOrMediaFilePaths, otherFilePaths, null, null, dirPath.Length > rootPath.Length);
                }
            }
            if(settings.GenerateHtmlPages && (!File.Exists(cssFilePath) || settings.GenerateAlways) )
            {
                GenerateCssFile(cssFilePath);
            }

            foreach (var subDir in Directory.GetDirectories(dirPath))
            {
                GenerateDir(rootPath, subDir, settings, progress);
            }
        }

        private static void GenerateFilePreviewPage(string targatePagePath, string filePath, string prevFilePath, string nextFilePath,
                                    DirectoryContactSheets sheetsSmall, DirectoryContactSheets sheetsBig)
        {
            var fileName = Path.GetFileName(filePath);
            var dirPath = Path.GetDirectoryName(filePath);
            var dirName = Path.GetFileName(dirPath);
            StringBuilder result = new StringBuilder();
            result.Append("<!DOCTYPE html><html><head><title>");
            result.Append(HttpUtility.HtmlEncode(fileName));
            result.Append("</title><link rel=\"stylesheet\" type=\"text/css\" href=\"viewer.css\" /></head><body class=\"imgviewer\"><div class=\"wrap\">");

            result.Append("<a href=\"index.html\">To Index of: <b>");
            result.Append(HttpUtility.HtmlEncode(dirName));
            result.Append("</b></a>");
            result.Append("<div class=\"container\"><h1>");
            result.Append(HttpUtility.HtmlEncode(dirName + ": " + fileName));
            result.Append("<div class=\"subs\">");
            result.Append(Utilities.RepresentFileSize(new FileInfo(filePath).Length));
            result.Append("</div></h1>");

            result.Append("<div class=\"section\"><table><tbody><tr><td class=\"prev\">");
            if (!string.IsNullOrEmpty(prevFilePath))
            {
                var prevFileName = Path.GetFileName(prevFilePath);
                result.Append("<a href=\"");
                result.Append(HttpUtility.HtmlEncode(prevFileName));
                result.Append(".prv.html\"><div class=\"supr\">Previous</div>");
                if (prevFileName.EndsWith(".mp3", StringComparison.InvariantCultureIgnoreCase))
                {
                    result.Append("<span class=\"prev200txt\">AUDIO</span>");
                }
                else if (prevFileName.EndsWith(".mp4", StringComparison.InvariantCultureIgnoreCase))
                {
                    result.Append("<span class=\"prev200txt\">VIDEO</span>");
                }
                else
                {
                    var thmbPrev = sheetsSmall.FileIndex[prevFileName];
                    result.Append("<span class=\"prev200\" style=\"margin:");
                    result.Append((200 - thmbPrev.ContactSheetRect.Height) / 2);
                    result.Append("px ");
                    result.Append((200 - thmbPrev.ContactSheetRect.Width) / 2);
                    result.Append("px;background-image:url('");
                    result.Append(thmbPrev.GetContactSheetName(200));
                    result.Append("');background-position:");
                    result.Append(-thmbPrev.ContactSheetRect.X);
                    result.Append("px ");
                    result.Append(-thmbPrev.ContactSheetRect.Y);
                    result.Append("px;width:");
                    result.Append(thmbPrev.ContactSheetRect.Width);
                    result.Append("px;height:");
                    result.Append(thmbPrev.ContactSheetRect.Height);
                    result.Append("px;\"></span>");
                }
                result.Append("<div class=\"subs\">");
                result.Append(HttpUtility.HtmlEncode(prevFileName));
                result.Append("</div></a>");
            }
            result.Append("</td><td class=\"main\">");
            if (fileName.EndsWith(".mp3", StringComparison.InvariantCultureIgnoreCase))
            {
                result.Append("<div class=\"mediaitem\"><audio controls><source src=\"");
                result.Append(HttpUtility.HtmlEncode(fileName));
                result.Append("\" type=\"audio/mpeg\"/></audio><div class=\"subs\"><a href=\"");
                result.Append(HttpUtility.HtmlEncode(fileName));
                result.Append("\">");
                result.Append(HttpUtility.HtmlEncode(fileName));
                result.Append("</a> ");
                result.Append(Utilities.RepresentFileSize(new FileInfo(filePath).Length));
                result.Append("</div></div>");
            }
            else if (fileName.EndsWith(".mp4", StringComparison.InvariantCultureIgnoreCase))
            {
                result.Append("<div class=\"mediaitem\"><video width=\"800\" controls><source src=\"");
                result.Append(HttpUtility.HtmlEncode(fileName));
                result.Append("\" type=\"video/mp4\"/></video><div class=\"subs\"> <a href=\"");
                result.Append(HttpUtility.HtmlEncode(fileName));
                result.Append("\">");
                result.Append(HttpUtility.HtmlEncode(fileName));
                result.Append("</a> ");
                result.Append(Utilities.RepresentFileSize(new FileInfo(filePath).Length));
                result.Append("</div></div>");
            }
            else
            {
                result.Append("<div class=\"img\"><a href=\"");
                result.Append(HttpUtility.HtmlEncode(fileName));
                result.Append("\" title=\"");
                result.Append(HttpUtility.HtmlEncode(fileName));
                result.Append(" (");
                result.Append(Utilities.RepresentFileSize(new FileInfo(filePath).Length));
                var previewMain = sheetsBig.FileIndex[fileName];
                result.Append(")\"><span class=\"prev800\" style=\"background-image:url('");
                result.Append(previewMain.GetContactSheetName(800));
                result.Append("');background-position:");
                result.Append(-previewMain.ContactSheetRect.X);
                result.Append("px ");
                result.Append(-previewMain.ContactSheetRect.Y);
                result.Append("px;height:");
                result.Append(previewMain.ContactSheetRect.Height);
                result.Append("px\"></span></a></div>");
            }
            result.Append("</td><td class=\"next\">");
            if (!string.IsNullOrEmpty(nextFilePath))
            {
                var nextFileName = Path.GetFileName(nextFilePath);
                result.Append("<a href=\"");
                result.Append(HttpUtility.HtmlEncode(nextFileName));
                result.Append(".prv.html\"><div class=\"supr\">Next</div>");
                if (nextFileName.EndsWith(".mp3", StringComparison.InvariantCultureIgnoreCase))
                {
                    result.Append("<span class=\"prev200txt\">AUDIO</span>");
                }
                else if (nextFileName.EndsWith(".mp4", StringComparison.InvariantCultureIgnoreCase))
                {
                    result.Append("<span class=\"prev200txt\">VIDEO</span>");
                }
                else
                {
                    var thmbNext = sheetsSmall.FileIndex[nextFileName];
                    result.Append("<span class=\"prev200\" style=\"margin:");
                    result.Append((200-thmbNext.ContactSheetRect.Height)/2);
                    result.Append("px ");
                    result.Append((200-thmbNext.ContactSheetRect.Width)/2);
                    result.Append("px;background-image:url('");
                    result.Append(thmbNext.GetContactSheetName(200));
                    result.Append("');background-position:");
                    result.Append(-thmbNext.ContactSheetRect.X);
                    result.Append("px ");
                    result.Append(-thmbNext.ContactSheetRect.Y);
                    result.Append("px;width:");
                    result.Append(thmbNext.ContactSheetRect.Width);
                    result.Append("px;height:");
                    result.Append(thmbNext.ContactSheetRect.Height);
                    result.Append("px;\"></span>");
                }
                result.Append("<div class=\"subs\">");
                result.Append(HttpUtility.HtmlEncode(nextFileName));
                result.Append("</div></a>");
            }
            result.Append("</td></tr></table>");
            result.Append("</div></div></div></body></html>");

            using (var stream = File.CreateText(filePath + ".prv.html"))
            {
                stream.Write(result.ToString());
                stream.Close();
            }
        }

        private static void GenerateFileIndexPage(string targetPagePath, string dirPath, List<string> imgOrMediaFilePaths, List<string> otherFilePaths,
                                    DirectoryContactSheets sheetsSmall, DirectoryContactSheets sheetsBig, bool canGoToParent)
        {
            var pageName = Path.GetFileName(dirPath);

            StringBuilder result = new StringBuilder();
            result.Append("<!DOCTYPE html><html><head><title>");
            result.Append(HttpUtility.HtmlEncode(pageName));
            result.Append("</title><meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\"><link rel=\"stylesheet\" type=\"text/css\" href=\"viewer.css\" /></head><body class=\"index\"><div class=\"wrap\">");

            if (canGoToParent )
            {
                var parentDirName = Path.GetFileName(Path.GetDirectoryName(dirPath));
                result.Append("<a href=\"../index.html\">To Parent Directory: <b>");
                result.Append(HttpUtility.HtmlEncode(parentDirName));
                result.Append("</b></a>");
            }
            result.Append("<h1>");
            result.Append(HttpUtility.HtmlEncode(pageName));
            result.Append("</h1>");

            var subDirs = Directory.GetDirectories(dirPath);
            if( subDirs.Length>0)
            {
                result.Append("<div class=\"container\"><h2>Subdirectories</h2><div class=\"section\"><ul>");
                foreach (var curPath in subDirs)
                {
                    var curDirName = Path.GetFileName(curPath);
                    result.Append("<li><a href=\"");
                    result.Append(HttpUtility.HtmlEncode(curDirName));
                    result.Append("/index.html\">");
                    result.Append(HttpUtility.HtmlEncode(curDirName));
                    result.Append("</a></li>");
                }
                result.Append("</ul></div></div>");
            }

            if( otherFilePaths.Count>0 )
            {
                result.Append("<div class=\"container\"><h2>Files</h2><div class=\"section\"><ul>");
                foreach( var curPath in otherFilePaths )
                {
                    var curFileName = Path.GetFileName(curPath);
                    
                    result.Append("<li><a href=\"");
                    result.Append(HttpUtility.HtmlEncode(curFileName));
                    result.Append("\">");
                    result.Append(HttpUtility.HtmlEncode(curFileName));
                    result.Append("</a><span>");
                    result.Append(Utilities.RepresentFileSize(new FileInfo(curPath).Length));
                    result.Append("</span></li>");                    
                }
                result.Append("</ul></div></div>");
            }

            if (imgOrMediaFilePaths.Count > 0)
            {
                bool anyImages = false;
                bool anyAudio = false;
                bool anyVideo = false;

                foreach(var curPath in imgOrMediaFilePaths)
                {
                    if (curPath.EndsWith(".mp3", StringComparison.InvariantCultureIgnoreCase))
                    {
                        anyAudio = true;
                    }
                    else if (curPath.EndsWith(".mp4", StringComparison.InvariantCultureIgnoreCase))
                    {
                        anyVideo = true;
                    }
                    else
                    {
                        anyImages = true;
                    }

                    if (anyAudio && anyVideo && anyImages)
                        break;
                }

                string sectionName = anyImages ? "Images" : "";
                if( anyAudio )
                {
                    if (sectionName.Length > 0)
                        sectionName += ", ";
                    sectionName += "Audio";
                }
                if (anyVideo)
                {
                    if (sectionName.Length > 0)
                        sectionName += ", ";
                    sectionName += "Video";
                }

                result.Append("<div class=\"container\"><h2>");
                result.Append(sectionName);
                result.Append("</h2><div class=\"section\">");
                foreach (var curPath in imgOrMediaFilePaths)
                {
                    var curFileName = Path.GetFileName(curPath);
                    result.Append("<div class=\"img\"><a href=\"");
                    result.Append(HttpUtility.HtmlEncode(curFileName));
                    result.Append(".prv.html\">");
                    if (curFileName.EndsWith(".mp3", StringComparison.InvariantCultureIgnoreCase))
                    {
                        result.Append("<span class=\"prev200txt\">AUDIO</span>");
                    }
                    else if (curFileName.EndsWith(".mp3", StringComparison.InvariantCultureIgnoreCase))
                    {
                        result.Append("<span class=\"prev200txt\">VIDEO</span>");
                    }
                    else
                    {
                        var thmbCur = sheetsSmall.FileIndex[curFileName];
                        result.Append("<span class=\"prev200\" style=\"margin:");
                        result.Append((200 - thmbCur.ContactSheetRect.Height) / 2);
                        result.Append("px ");
                        result.Append((200 - thmbCur.ContactSheetRect.Width) / 2);
                        result.Append("px;background-image:url('");
                        result.Append(thmbCur.GetContactSheetName(200));
                        result.Append("');background-position:");
                        result.Append(-thmbCur.ContactSheetRect.X);
                        result.Append("px ");
                        result.Append(-thmbCur.ContactSheetRect.Y);
                        result.Append("px;width:");
                        result.Append(thmbCur.ContactSheetRect.Width);
                        result.Append("px;height:");
                        result.Append(thmbCur.ContactSheetRect.Height);
                        result.Append("px;\"></span>");
                    }
                    result.Append("</a><div class=\"subs\">");
                    result.Append(HttpUtility.HtmlEncode(curFileName));
                    result.Append(", ");
                    result.Append(Utilities.RepresentFileSize(new FileInfo(curPath).Length));
                    result.Append("</div></div>"); 
                }
                result.Append("<div class=\"clear\"></div></div></div>");
            }
            result.Append("</div></body></html>");
            using (var stream = File.CreateText(targetPagePath))
            {
                stream.Write(result.ToString());
                stream.Close();
            }
        }

        private static void GenerateCssFile(string targetPath)
        {
            StringBuilder result = new StringBuilder();
            result.Append("*,body,td {font-family:Georgia,Times New Roman} ");
            result.Append(".wrap {margin: 0 auto; width: 90%;} ");
            result.Append("h1 {font-size:26px} h2 {font-size:22px}");
            result.Append(".subs {font-size:14px} ");
            result.Append(".supr {font-size:14px} ");
            result.Append("h1 .subs {display:inline; padding-left:20px; font-size: 15px; font-weight:normal} ");
            result.Append(".section {border: 1px solid #f0e0e0; padding:10px} ");
            result.Append(".prev200 {width:200px;height:200px; display:inline-block; background-repeat:no-repeat;}");
            result.Append(".prev200txt {width:200px;height:110px; display:inline-block; background-repeat:no-repeat; padding-top:90px; text-align:center; border:2px solid #e0e0e0;}");            
            result.Append(".prev800 {width:800px;display:inline-block; background-repeat:no-repeat;}");
            result.Append(".clear {clear:both;}");


            result.Append("body.imgviewer table {margin:0 auto;}");
            result.Append("body.imgviewer td {vertical-align:middle;height:100%;}");
            result.Append("body.imgviewer td.prev a, body.imgviewer td.next a {vertical-align:middle; height:100%; display:table-cell; padding:10px;}");
            result.Append("body.imgviewer td.prev a img, body.imgviewer td.next a img {min-width:100px;}");
            result.Append("body.imgviewer td.prev a:hover, body.imgviewer td.next a:hover {background-color:#fff0f0;}");
            result.Append("body.imgviewer td.prev div.supr, body.imgviewer td.next div.supr {width:200px; text-align:center;margin:0 0 5px 0;}");
            result.Append("body.imgviewer td.prev div.subs, body.imgviewer td.next div.subs {width:200px; text-align:center;margin:5px 0 0 0;}");
            result.Append("body.imgviewer td.main a img{width:800px;}");

            result.Append("body.index div.img {float:left;margin:10px; width:220px; height:220px;;} ");
            result.Append("body.index div.img .subs{width:200px; text-align:center;} ");
            result.Append("body.index li span {padding-left:10px;}");
            
            using (var stream = File.CreateText(targetPath))
            {
                stream.Write(result.ToString());
                stream.Close();
            }
        }
    }

}
