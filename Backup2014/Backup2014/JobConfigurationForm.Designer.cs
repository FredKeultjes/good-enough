namespace Backup2014
{
    partial class JobConfigurationForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.butNew = new System.Windows.Forms.Button();
            this.butEdit = new System.Windows.Forms.Button();
            this.butRemove = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.listSources = new System.Windows.Forms.ListView();
            this.colDirectory = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colExclusions = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label2 = new System.Windows.Forms.Label();
            this.txtTargetDir = new System.Windows.Forms.TextBox();
            this.butBrowse = new System.Windows.Forms.Button();
            this.chkConfirmAllActions = new System.Windows.Forms.CheckBox();
            this.butOK = new System.Windows.Forms.Button();
            this.butCancel = new System.Windows.Forms.Button();
            this.chkZipUpdatedAndDeleteFiles = new System.Windows.Forms.CheckBox();
            this.butWizard = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // butNew
            // 
            this.butNew.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butNew.Location = new System.Drawing.Point(419, 28);
            this.butNew.Name = "butNew";
            this.butNew.Size = new System.Drawing.Size(75, 23);
            this.butNew.TabIndex = 2;
            this.butNew.Text = "New...";
            this.butNew.UseVisualStyleBackColor = true;
            this.butNew.Click += new System.EventHandler(this.butNew_Click);
            // 
            // butEdit
            // 
            this.butEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butEdit.Location = new System.Drawing.Point(419, 58);
            this.butEdit.Name = "butEdit";
            this.butEdit.Size = new System.Drawing.Size(75, 23);
            this.butEdit.TabIndex = 3;
            this.butEdit.Text = "Edit...";
            this.butEdit.UseVisualStyleBackColor = true;
            this.butEdit.Click += new System.EventHandler(this.butEdit_Click);
            // 
            // butRemove
            // 
            this.butRemove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butRemove.Location = new System.Drawing.Point(419, 87);
            this.butRemove.Name = "butRemove";
            this.butRemove.Size = new System.Drawing.Size(75, 23);
            this.butRemove.TabIndex = 4;
            this.butRemove.Text = "Remove";
            this.butRemove.UseVisualStyleBackColor = true;
            this.butRemove.Click += new System.EventHandler(this.butRemove_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(78, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Source Folders";
            // 
            // listSources
            // 
            this.listSources.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listSources.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colDirectory,
            this.colExclusions});
            this.listSources.FullRowSelect = true;
            this.listSources.Location = new System.Drawing.Point(16, 29);
            this.listSources.MultiSelect = false;
            this.listSources.Name = "listSources";
            this.listSources.Size = new System.Drawing.Size(386, 205);
            this.listSources.TabIndex = 1;
            this.listSources.UseCompatibleStateImageBehavior = false;
            this.listSources.View = System.Windows.Forms.View.Details;
            this.listSources.DoubleClick += new System.EventHandler(this.listSources_DoubleClick);
            this.listSources.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listSources_KeyDown);
            // 
            // colDirectory
            // 
            this.colDirectory.Text = "Source Directory";
            this.colDirectory.Width = 300;
            // 
            // colExclusions
            // 
            this.colExclusions.Text = "Exclusions";
            this.colExclusions.Width = 200;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 251);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(110, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Backup Target Folder";
            // 
            // txtTargetDir
            // 
            this.txtTargetDir.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtTargetDir.Location = new System.Drawing.Point(16, 277);
            this.txtTargetDir.Name = "txtTargetDir";
            this.txtTargetDir.Size = new System.Drawing.Size(386, 20);
            this.txtTargetDir.TabIndex = 7;
            // 
            // butBrowse
            // 
            this.butBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butBrowse.Location = new System.Drawing.Point(419, 277);
            this.butBrowse.Name = "butBrowse";
            this.butBrowse.Size = new System.Drawing.Size(75, 23);
            this.butBrowse.TabIndex = 8;
            this.butBrowse.Text = "Browse...";
            this.butBrowse.UseVisualStyleBackColor = true;
            this.butBrowse.Click += new System.EventHandler(this.butBrowse_Click);
            // 
            // chkConfirmAllActions
            // 
            this.chkConfirmAllActions.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkConfirmAllActions.AutoSize = true;
            this.chkConfirmAllActions.Location = new System.Drawing.Point(16, 309);
            this.chkConfirmAllActions.Name = "chkConfirmAllActions";
            this.chkConfirmAllActions.Size = new System.Drawing.Size(111, 17);
            this.chkConfirmAllActions.TabIndex = 9;
            this.chkConfirmAllActions.Text = "Confirm all actions";
            this.chkConfirmAllActions.UseVisualStyleBackColor = true;
            // 
            // butOK
            // 
            this.butOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butOK.Location = new System.Drawing.Point(419, 309);
            this.butOK.Name = "butOK";
            this.butOK.Size = new System.Drawing.Size(75, 23);
            this.butOK.TabIndex = 11;
            this.butOK.Text = "OK";
            this.butOK.UseVisualStyleBackColor = true;
            this.butOK.Click += new System.EventHandler(this.butOK_Click);
            // 
            // butCancel
            // 
            this.butCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.butCancel.Location = new System.Drawing.Point(419, 339);
            this.butCancel.Name = "butCancel";
            this.butCancel.Size = new System.Drawing.Size(75, 23);
            this.butCancel.TabIndex = 12;
            this.butCancel.Text = "Cancel";
            this.butCancel.UseVisualStyleBackColor = true;
            // 
            // chkZipUpdatedAndDeleteFiles
            // 
            this.chkZipUpdatedAndDeleteFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkZipUpdatedAndDeleteFiles.AutoSize = true;
            this.chkZipUpdatedAndDeleteFiles.Location = new System.Drawing.Point(16, 333);
            this.chkZipUpdatedAndDeleteFiles.Name = "chkZipUpdatedAndDeleteFiles";
            this.chkZipUpdatedAndDeleteFiles.Size = new System.Drawing.Size(163, 17);
            this.chkZipUpdatedAndDeleteFiles.TabIndex = 10;
            this.chkZipUpdatedAndDeleteFiles.Text = "Zip updated and deleted files";
            this.chkZipUpdatedAndDeleteFiles.UseVisualStyleBackColor = true;
            // 
            // butWizard
            // 
            this.butWizard.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butWizard.Location = new System.Drawing.Point(419, 117);
            this.butWizard.Name = "butWizard";
            this.butWizard.Size = new System.Drawing.Size(75, 23);
            this.butWizard.TabIndex = 5;
            this.butWizard.Text = "Wizard...";
            this.butWizard.UseVisualStyleBackColor = true;
            this.butWizard.Click += new System.EventHandler(this.butWizard_Click);
            // 
            // JobConfigurationForm
            // 
            this.AcceptButton = this.butOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.butCancel;
            this.ClientSize = new System.Drawing.Size(506, 376);
            this.Controls.Add(this.butWizard);
            this.Controls.Add(this.chkZipUpdatedAndDeleteFiles);
            this.Controls.Add(this.butCancel);
            this.Controls.Add(this.butOK);
            this.Controls.Add(this.chkConfirmAllActions);
            this.Controls.Add(this.butBrowse);
            this.Controls.Add(this.txtTargetDir);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.listSources);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.butRemove);
            this.Controls.Add(this.butEdit);
            this.Controls.Add(this.butNew);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "JobConfigurationForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "JobConfigurationForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button butNew;
        private System.Windows.Forms.Button butEdit;
        private System.Windows.Forms.Button butRemove;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ListView listSources;
        private System.Windows.Forms.ColumnHeader colDirectory;
        private System.Windows.Forms.ColumnHeader colExclusions;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtTargetDir;
        private System.Windows.Forms.Button butBrowse;
        private System.Windows.Forms.CheckBox chkConfirmAllActions;
        private System.Windows.Forms.Button butOK;
        private System.Windows.Forms.Button butCancel;
        private System.Windows.Forms.CheckBox chkZipUpdatedAndDeleteFiles;
        private System.Windows.Forms.Button butWizard;
    }
}