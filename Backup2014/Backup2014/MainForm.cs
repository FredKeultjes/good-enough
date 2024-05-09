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
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml;

namespace Backup2014
{
    public partial class MainForm : Form
    {
        private JobConfiguration settings = new JobConfiguration();

        private ContextMenuStrip notifyContextMenu;
        private NotifyIcon notifyIcon;
        public const string AppName = "Fred's Backup";

        public MainForm()
        {
            
            notifyIcon = new NotifyIcon();
            notifyIcon.Text = AppName;

            notifyIcon.Icon = new Icon(Properties.Resources.BackupMinimized, 16, 16);
            notifyIcon.DoubleClick += new EventHandler(notifyIcon_DoubleClick);
            

            notifyContextMenu = new System.Windows.Forms.ContextMenuStrip();
            this.ContextMenuStrip = notifyContextMenu;
            notifyContextMenu.Opening += NotifyContextMenu_Opening;
            notifyContextMenu.ItemClicked += NotifyContextMenu_ItemClicked;
            var ms = new MenuStrip();
            ms.Dock = DockStyle.Top;
            ToolStripMenuItem itemRestore = new ToolStripMenuItem("Commands");
            ms.Items.Add(itemRestore);
            notifyIcon.ContextMenuStrip = notifyContextMenu;
            InitializeComponent();

            LoadSettings();
        }

        private void NotifyContextMenu_Opening(object sender, CancelEventArgs e)
        {
            // Clear the ContextMenuStrip control's Items collection.
            notifyContextMenu.Items.Clear();

            // Check the source control first.

            // Populate the ContextMenuStrip control with its default items.
            notifyContextMenu.Items.Add("Restore");
            notifyContextMenu.Items.Add("Exit");

            // Set Cancel to false. 
            // It is optimized to true based on empty entry.
            e.Cancel = false;
        }

        private void NotifyContextMenu_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem.Text == "Exit")
            {
                notifyIcon.Visible = false;
                Application.Exit();
            }
            else if (e.ClickedItem.Text == "Restore")
            {
                OnNotifyRestore(sender, e);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private BackupThread backupThread = null;

        private void MainForm_Load(object sender, EventArgs e)
        {
            backupThread = new BackupThread();
            backupThread.Start();
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            backupThread.Stop();
        }

        private string _settingsFilePath = null;

        private string SettingsFilePath
        {
            get
            {
                if (_settingsFilePath==null )
                    _settingsFilePath = Path.Combine(Utilities.AssemblyDirectory, "backupjob.xml");
                return _settingsFilePath;
            }
        }
        private void LoadSettings()
        {
            if (File.Exists(SettingsFilePath))
            {
                try
                {
                    using (XmlReader reader = XmlReader.Create(SettingsFilePath))
                    {
                        System.Xml.Serialization.XmlSerializer ser = new System.Xml.Serialization.XmlSerializer(settings.GetType());
                        settings = (JobConfiguration)ser.Deserialize(reader);
                    }
                }
                catch
                {
                    // ignoring wrong file
                    MessageBox.Show("Error reading settings file", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
        }
        private void SaveSettings()
        {
            XmlWriterSettings xmlSettings = new XmlWriterSettings();
            xmlSettings.Indent = true;
            using (XmlWriter writer = XmlWriter.Create(SettingsFilePath, xmlSettings))
            {
                System.Xml.Serialization.XmlSerializer ser = new System.Xml.Serialization.XmlSerializer(settings.GetType());
                ser.Serialize(writer, settings);
            }
        }

        private void butConfigure_Click(object sender, EventArgs e)
        {
            JobConfigurationForm dlg = new JobConfigurationForm(settings);
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                SaveSettings();
            }
        }

        private void butBackup_Click(object sender, EventArgs e)
        {
            if( !backupThread.StartJob(settings) )
            {
                return;
            }
            butCancelBackup.Enabled = true;
            butBackup.Enabled = false;
            butCancel.Enabled = false;
            butConfigure.Enabled = false;
            timer1.Enabled = true;
        }

        private void UpdateNotifyIconText(bool isBackuping)
        {
            if (notifyIcon.Visible )
            {
                string textForBalloon = !isBackuping ? "" : txtProgressOverall.Text.Length != 0 ? txtStatus.Text + "\r\n" + txtProgressOverall.Text : txtProgress.Text.Length > 0 ? txtStatus.Text + "\r\n" + txtProgress.Text : txtStatus.Text;
                 if (textForBalloon != notifyIcon.BalloonTipText)
                 {
                     notifyIcon.BalloonTipText = textForBalloon;
                     notifyIcon.BalloonTipIcon = ToolTipIcon.Info;
                     notifyIcon.ShowBalloonTip(3000);
                 }
            }
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            bool isBackuping = backupThread.PollUIWorkload(this, txtStatus, progressBar1, txtProgress, progressBarOverall, txtProgressOverall, txtErrors);
            if (!isBackuping)
            {
                if (butCancelBackup.Enabled)
                {
                    butCancelBackup.Enabled = false;
                    butBackup.Enabled = true;
                    butCancel.Enabled = true;
                    butConfigure.Enabled = true;
                    timer1.Enabled = false;
                }
            }

            UpdateNotifyIconText(isBackuping);
        }

        private void butCancelBackup_Click(object sender, EventArgs e)
        {
            backupThread.StopJob();
        }
        private void butCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        void MainForm_Resize(object sender, EventArgs e)
        {
            switch (this.WindowState)
            {
                case FormWindowState.Minimized:
                    notifyIcon.Visible = true;
                    notifyIcon.BalloonTipText = "(Minimized)";
                    notifyIcon.BalloonTipTitle = AppName;
                    notifyIcon.BalloonTipIcon = ToolTipIcon.Info;
                    notifyIcon.Tag = null;
                    notifyIcon.BalloonTipClicked += new EventHandler(notifyIcon_BalloonTipClicked);
                    notifyIcon.ShowBalloonTip(3000);
                    this.Hide();
                    break;
                case FormWindowState.Normal:
                    notifyIcon.Visible = false;
                    break;
            }
        }
        void notifyIcon_DoubleClick(object sender, EventArgs e)
        {
            OnNotifyRestore(sender, e);
        }
        private void OnNotifyRestore(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }
        void notifyIcon_BalloonTipClicked(object sender, EventArgs e)
        {
            RestoreWindowForMessage();
        }

        internal void RestoreWindowForMessage()
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Show();
                this.WindowState = FormWindowState.Normal;
            }
        }
    }
}
