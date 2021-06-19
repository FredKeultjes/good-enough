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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ThumbnailGenerator
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();           
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string dirPath = Directory.GetCurrentDirectory();

            txtStartDirectory.Text = dirPath;
            chkGenerateAlways.Checked = false;
            //chkGenerateHtml.Checked = true;

            numThumbnailSize.Minimum = 30;
            numThumbnailSize.Maximum = 400;
            numThumbnailSize.Value = 200;
            numThumbnailsPerSheet.Minimum = 1;
            numThumbnailsPerSheet.Maximum = 500;
            numThumbnailsPerSheet.Value = 50;
            numPreviewWidth.Minimum = 400;
            numPreviewWidth.Maximum = 3000;
            numPreviewWidth.Value = 800;
            numNumberOfPreviewsPerSheet.Minimum = 1;
            numNumberOfPreviewsPerSheet.Maximum = 50;
            numNumberOfPreviewsPerSheet.Value = 10;
        }

        private void butClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void butStart_Click(object sender, EventArgs e)
        {
            butStart.Enabled = false;
            lblProgress.Text = "Generating...";

            DirectoryThumbnailGenerator.Generate(txtStartDirectory.Text, new GeneratorSettings()
            {
                GenerateAlways = chkGenerateAlways.Checked,
                ThumbnailSize = Convert.ToInt32(numThumbnailSize.Value),
                ThumbnailsPerSheet = Convert.ToInt32(numThumbnailsPerSheet.Value),
                PreviewWidth = Convert.ToInt32(numPreviewWidth.Value),
                NumberOfPreviewsPerSheet = Convert.ToInt32(numNumberOfPreviewsPerSheet.Value),
                GenerateHtmlPages = chkGenerateHtml.Checked
            }, new ProgressUpdater(txtNumberOfFiles));

            lblProgress.Text = "Ready";
            butStart.Enabled = true;
        }
    }
}
