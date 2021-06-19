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
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Backup2014
{
    public partial class SourceDefinitionForm : Form
    {
        private SourceDefinition configData;

        public SourceDefinitionForm(SourceDefinition configData)        
        {
            this.configData = configData;

            InitializeComponent();

            txtSourceDirectory.Text = configData.SourceDirectory;
            txtExclusions.Text = configData.Exclusions;
        }

        private void butBrowse_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            dlg.SelectedPath = txtSourceDirectory.Text;
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txtSourceDirectory.Text = dlg.SelectedPath;
            }
        }

        private void butOK_Click(object sender, EventArgs e)
        {
            if (txtSourceDirectory.Text.Length==0 || !Directory.Exists(txtSourceDirectory.Text) )
            {
                MessageBox.Show("Specify an existing directory", "Source", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            configData.SourceDirectory = txtSourceDirectory.Text;
            configData.Exclusions = txtExclusions.Text;
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            Close();
        }

    }
}
