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
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Backup2014
{
    public partial class JobConfigurationForm : Form
    {
        private JobConfiguration configData;

        public JobConfigurationForm(JobConfiguration configData)
        {
            this.configData = configData;

            InitializeComponent();

            txtTargetDir.Text = configData.TargetDirectory;
            chkConfirmAllActions.Checked = configData.ConfirmAllActions;
            chkZipUpdatedAndDeleteFiles.Checked = configData.ZipUpdatedAndDeletedFiles;
            foreach(SourceDefinition curDef in configData.Sources )
            {
                listSources.Items.Add(new ListViewItem(new string[]{curDef.SourceDirectory, curDef.Exclusions}));
            }
        }

        private void butNew_Click(object sender, EventArgs e)
        {
            SourceDefinition newSource = new SourceDefinition();
            SourceDefinitionForm dlg = new SourceDefinitionForm(newSource);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                listSources.Items.Add(new ListViewItem(new string[]{newSource.SourceDirectory, newSource.Exclusions})).Selected = true;
            }
        }

        private void butEdit_Click(object sender, EventArgs e)
        {
            if( listSources.SelectedItems.Count>0 )
            {
                ListViewItem curListItem = listSources.SelectedItems[0];
                SourceDefinition editSource = new SourceDefinition(curListItem.SubItems[0].Text, curListItem.SubItems[1].Text);
                SourceDefinitionForm dlg = new SourceDefinitionForm(editSource);
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    curListItem.SubItems[0].Text = editSource.SourceDirectory;
                    curListItem.SubItems[1].Text = editSource.Exclusions;
                }
            }
        }

        private void butRemove_Click(object sender, EventArgs e)
        {
            int lastSelectedIndex = -1;
            while (listSources.SelectedItems.Count > 0)
            {
                lastSelectedIndex = listSources.SelectedItems[0].Index;
                listSources.Items.Remove(listSources.SelectedItems[0]);                
            }

            if (lastSelectedIndex >= 0 && lastSelectedIndex < listSources.Items.Count)
            {
                listSources.Items[lastSelectedIndex].Selected = true;
            }
        }


        private void listSources_KeyDown(object sender, KeyEventArgs e)
        {
            if( e.KeyCode == Keys.Delete )
            {
                butRemove_Click(sender, e);
            }
        }

        private void listSources_DoubleClick(object sender, EventArgs e)
        {
            butEdit_Click(sender, e);
        }

        private void butBrowse_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            dlg.SelectedPath = txtTargetDir.Text;
            dlg.ShowNewFolderButton = true;
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txtTargetDir.Text = dlg.SelectedPath;
            }
        }

        private void butOK_Click(object sender, EventArgs e)
        {
            if (txtTargetDir.Text.Length == 0 || !Directory.Exists(txtTargetDir.Text))
            {
                MessageBox.Show("Specify an existing target directory", "Job Target", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);                
                return;
            }
            foreach (ListViewItem curListItem in listSources.Items)
            {
                if (curListItem.SubItems[0].Text.StartsWith(txtTargetDir.Text))
                {
                    MessageBox.Show(string.Format("Source directory {0} is located in the target directory. Please adjust the settings.", curListItem.SubItems[0].Text), "Job Target", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
                else if (txtTargetDir.Text.StartsWith(curListItem.SubItems[0].Text))
                {
                    MessageBox.Show(string.Format("The target directory is located in source directory {0}. Please adjust the settings.", curListItem.SubItems[0].Text), "Job Target", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
            }
            configData.TargetDirectory = txtTargetDir.Text;
            configData.ConfirmAllActions = chkConfirmAllActions.Checked;
            configData.ZipUpdatedAndDeletedFiles = chkZipUpdatedAndDeleteFiles.Checked;

            configData.Sources.Clear();
            foreach (ListViewItem curListItem in listSources.Items)
            {
                configData.Sources.Add(new SourceDefinition(curListItem.SubItems[0].Text, curListItem.SubItems[1].Text));
            }

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            Close();
        }

        private void butWizard_Click(object sender, EventArgs e)
        {
            AutoGeneratedLocalSourceDirectories wizardData = new AutoGeneratedLocalSourceDirectories();

            chkConfirmAllActions.Checked = true;
            chkZipUpdatedAndDeleteFiles.Checked = true;
            listSources.Items.Clear();
            foreach (SourceDefinition elem in wizardData.Directories)
            {
                listSources.Items.Add(new ListViewItem(new string[] { elem.SourceDirectory, elem.Exclusions }));
            }
        }


    }
}
