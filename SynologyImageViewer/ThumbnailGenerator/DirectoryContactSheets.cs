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
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThumbnailGenerator
{
    public enum SheetOrientation
    {
        Vertical,
        Horizontal
    }
    public class FileContactSheetIndex
    {
        public string ImageFilePath;
        public string ImageFileName;
        public int ContactSheetNumber;
        public Rectangle ContactSheetRect;
        public bool Error;
        public SheetOrientation SheetType;

        public string GetContactSheetName(int elementSize)
        {            
                return SheetType == SheetOrientation.Horizontal
                    ? string.Format("thmh{0}_{1}.jpg", elementSize, ContactSheetNumber)
                    : string.Format("thmv{0}_{1}.jpg", elementSize, ContactSheetNumber);
        }
        
    };

    public class DirectoryContactSheets
    {
        private readonly int elementSize;
        private readonly bool useFixedWidth;
        private readonly int sheetCapacity;
        private readonly string contactSheetDirPath;
        public Dictionary<string,FileContactSheetIndex> FileIndex = new Dictionary<string, FileContactSheetIndex>();
        public List<int> HeightsOfVerticallyOrientedSheets = new List<int>();
        public List<int> WidthsOfHorizontallyOrientedSheets = new List<int>();

        public DirectoryContactSheets(int elementSize, bool useFixedWidth, int sheetCapacity, List<string> filePaths, string contactSheetDirPath)
        {
            this.elementSize = elementSize;
            this.useFixedWidth = useFixedWidth;
            this.sheetCapacity = sheetCapacity;
            this.contactSheetDirPath = contactSheetDirPath;

            // first measure 
            int measureVerticalSheetNumber = 0;
            int measureVerticalIndexOnSheet = 0;
            int measureVerticalSheetHeight = 0;
            int measureHorizontalSheetNumber = 0;
            int measureHorizontalIndexOnSheet = 0;
            int measureHorizontalSheetWidth = 0;
            for (int index = 0; index < filePaths.Count; index++)
            {
                var curPath = filePaths[index];
                var fileName = Path.GetFileName(curPath);
                try
                {
                    using (Image img = Image.FromFile(curPath))
                    {
                        if (useFixedWidth)
                        {
                            if (measureVerticalIndexOnSheet >= sheetCapacity)
                            {
                                HeightsOfVerticallyOrientedSheets.Add(measureVerticalSheetHeight);
                                measureVerticalSheetNumber++;
                                measureVerticalSheetHeight = 0;
                                measureVerticalIndexOnSheet = 0;
                            }
                            int resizedHeight = (int)(elementSize * (float)img.Size.Height / (float)img.Size.Width);
                            FileIndex.Add(fileName, new FileContactSheetIndex()
                            {
                                ImageFileName = fileName,
                                ImageFilePath = curPath,
                                SheetType = SheetOrientation.Vertical,
                                ContactSheetNumber = measureVerticalSheetNumber,
                                ContactSheetRect = new Rectangle(new Point(0, measureVerticalSheetHeight), new Size(elementSize, resizedHeight))
                            });
                            measureVerticalSheetHeight += resizedHeight;
                            measureVerticalIndexOnSheet++;
                        }
                        else
                        {
                            if (img.Size.Width > img.Size.Height)
                            {
                                if (measureVerticalIndexOnSheet >= sheetCapacity)
                                {
                                    HeightsOfVerticallyOrientedSheets.Add(measureVerticalSheetHeight);
                                    measureVerticalSheetNumber++;
                                    measureVerticalSheetHeight = 0;
                                    measureVerticalIndexOnSheet = 0;
                                }
                                int resizedHeight = (int)(elementSize * (float)img.Size.Height / (float)img.Size.Width);
                                FileIndex.Add(fileName, new FileContactSheetIndex()
                                {
                                    ImageFileName = fileName,
                                    ImageFilePath = curPath,
                                    SheetType = SheetOrientation.Vertical,
                                    ContactSheetNumber = measureVerticalSheetNumber,
                                    ContactSheetRect = new Rectangle(new Point(0, measureVerticalSheetHeight), new Size(elementSize, resizedHeight))
                                });
                                measureVerticalSheetHeight += resizedHeight;
                                measureVerticalIndexOnSheet++;
                            }
                            else
                            {
                                if (measureHorizontalIndexOnSheet >= sheetCapacity)
                                {
                                    WidthsOfHorizontallyOrientedSheets.Add(measureHorizontalSheetWidth);
                                    measureHorizontalSheetNumber++;
                                    measureHorizontalSheetWidth = 0;
                                    measureHorizontalIndexOnSheet = 0;
                                }
                                int resizedWidth = (int)(elementSize * (float)img.Size.Width / (float)img.Size.Height);
                                FileIndex.Add(fileName, new FileContactSheetIndex()
                                {
                                    ImageFileName = fileName,
                                    ImageFilePath = curPath,
                                    SheetType = SheetOrientation.Horizontal,
                                    ContactSheetNumber = measureHorizontalSheetNumber,
                                    ContactSheetRect = new Rectangle(new Point(measureHorizontalSheetWidth, 0), new Size(resizedWidth, elementSize))
                                });
                                measureHorizontalSheetWidth += resizedWidth;
                                measureHorizontalIndexOnSheet++;
                            }
                        }
                    }
                }
                catch
                {
                    FileIndex.Add(fileName, new FileContactSheetIndex() {
                        ImageFileName = fileName,
                        ImageFilePath = curPath,
                        SheetType = SheetOrientation.Vertical,
                        ContactSheetRect = new Rectangle(new Point(0, 0), new Size(elementSize, elementSize))
                    });
                }
            }

            if (measureVerticalIndexOnSheet > 0)
            {
                HeightsOfVerticallyOrientedSheets.Add(measureVerticalSheetHeight);
            }
            if (measureHorizontalIndexOnSheet > 0)
            {
                WidthsOfHorizontallyOrientedSheets.Add(measureHorizontalSheetWidth);
            }
        }
               


        public void Generate(ProgressUpdater progress)
        {
            Bitmap currentHorzSheet = null;
            Bitmap currentVertSheet = null;
            Graphics grpHorz = null;
            Graphics grpVert = null;
            int indexHorzSheet = -1;
            int indexVertSheet = -1;

            foreach ( var elem in FileIndex)
            {
                if(elem.Value.SheetType == SheetOrientation.Horizontal )
                {
                    if (elem.Value.ContactSheetNumber != indexHorzSheet)
                    {
                        if( currentHorzSheet!=null )
                        {
                            currentHorzSheet.Save(Path.Combine(contactSheetDirPath, string.Format("thmh{0}_{1}.jpg", elementSize, indexHorzSheet)), ImageFormat.Jpeg);
                            currentHorzSheet.Dispose();
                            grpHorz.Dispose();
                        }
                        indexHorzSheet = elem.Value.ContactSheetNumber;
                        var sheetWidth = WidthsOfHorizontallyOrientedSheets[indexHorzSheet];
                        Bitmap bmpSheet = new Bitmap(sheetWidth, elementSize);
                        currentHorzSheet = bmpSheet;
                        grpHorz = Graphics.FromImage(bmpSheet);
                        grpHorz.FillRectangle(new SolidBrush(Color.White), new RectangleF(0, 0, sheetWidth, elementSize));
                    }
                    using (Image img = Image.FromFile(elem.Value.ImageFilePath))
                    {
                        grpHorz.DrawImage(img, elem.Value.ContactSheetRect, new Rectangle(new Point(0, 0), img.Size), GraphicsUnit.Pixel);
                    }
                }
                else
                {
                    if (elem.Value.ContactSheetNumber != indexVertSheet)
                    {
                        if (currentVertSheet != null)
                        {
                            currentVertSheet.Save(Path.Combine(contactSheetDirPath, string.Format("thmv{0}_{1}.jpg", elementSize, indexVertSheet)), ImageFormat.Jpeg);
                            currentVertSheet.Dispose();
                            grpVert.Dispose();
                        }
                        indexVertSheet = elem.Value.ContactSheetNumber;
                        var sheetHeight = HeightsOfVerticallyOrientedSheets[indexVertSheet];
                        Bitmap bmpSheet = new Bitmap(elementSize, sheetHeight);
                        currentVertSheet = bmpSheet;
                        grpVert = Graphics.FromImage(bmpSheet);
                        grpVert.FillRectangle(new SolidBrush(Color.White), new RectangleF(0, 0, elementSize, sheetHeight));
                    }
                    using (Image img = Image.FromFile(elem.Value.ImageFilePath))
                    {
                        grpVert.DrawImage(img, elem.Value.ContactSheetRect, new Rectangle(new Point(0, 0), img.Size), GraphicsUnit.Pixel);
                    }
                }
                
                progress.IncrementFileCount();
            }

            if (currentHorzSheet != null)
            {
                currentHorzSheet.Save(Path.Combine(contactSheetDirPath, string.Format("thmh{0}_{1}.jpg", elementSize, indexHorzSheet)), ImageFormat.Jpeg);
                currentHorzSheet.Dispose();
                currentHorzSheet = null;
                grpHorz.Dispose();
            }
            if (currentVertSheet != null)
            {
                currentVertSheet.Save(Path.Combine(contactSheetDirPath, string.Format("thmv{0}_{1}.jpg", elementSize, indexVertSheet)), ImageFormat.Jpeg);
                currentVertSheet.Dispose();
                currentVertSheet = null;
                grpVert.Dispose();
            }
        }

        public string GetContactSheetJavaScript()
        {
            StringBuilder result = new StringBuilder();
            result.Append("var thm");
            result.Append(elementSize);
            result.AppendLine("index=new Object();");
            foreach (var elem in FileIndex)
            {
                result.Append("thm");
                result.Append(elementSize);
                result.Append("index[\"");
                result.Append(elem.Value.ImageFileName);
                result.Append("\"]={sheet:\"");
                result.Append(elem.Value.GetContactSheetName(elementSize));
                result.Append("\",x:");
                result.Append(elem.Value.ContactSheetRect.X);
                result.Append(",y:");
                result.Append(elem.Value.ContactSheetRect.Y);
                result.Append(",w:");
                result.Append(elem.Value.ContactSheetRect.Width);
                result.Append(",h:");
                result.Append(elem.Value.ContactSheetRect.Height);
                result.Append(",err:");
                result.Append(elem.Value.Error ? "true" : "false");
                result.AppendLine("};");
            }
            return result.ToString();
        }

    }
}
