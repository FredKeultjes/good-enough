namespace Backup2014
{
    partial class MainForm
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
            this.components = new System.ComponentModel.Container();
            this.butConfigure = new System.Windows.Forms.Button();
            this.butBackup = new System.Windows.Forms.Button();
            this.butCancel = new System.Windows.Forms.Button();
            this.txtStatus = new System.Windows.Forms.TextBox();
            this.butCancelBackup = new System.Windows.Forms.Button();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.txtProgress = new System.Windows.Forms.TextBox();
            this.progressBarOverall = new System.Windows.Forms.ProgressBar();
            this.txtProgressOverall = new System.Windows.Forms.TextBox();
            this.txtErrors = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // butConfigure
            // 
            this.butConfigure.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butConfigure.Location = new System.Drawing.Point(376, 12);
            this.butConfigure.Name = "butConfigure";
            this.butConfigure.Size = new System.Drawing.Size(75, 23);
            this.butConfigure.TabIndex = 1;
            this.butConfigure.Text = "&Configure...";
            this.butConfigure.UseVisualStyleBackColor = true;
            this.butConfigure.Click += new System.EventHandler(this.butConfigure_Click);
            // 
            // butBackup
            // 
            this.butBackup.Location = new System.Drawing.Point(12, 49);
            this.butBackup.Name = "butBackup";
            this.butBackup.Size = new System.Drawing.Size(258, 23);
            this.butBackup.TabIndex = 2;
            this.butBackup.Text = "Start Backup";
            this.butBackup.UseVisualStyleBackColor = true;
            this.butBackup.Click += new System.EventHandler(this.butBackup_Click);
            // 
            // butCancel
            // 
            this.butCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.butCancel.Location = new System.Drawing.Point(376, 79);
            this.butCancel.Name = "butCancel";
            this.butCancel.Size = new System.Drawing.Size(75, 23);
            this.butCancel.TabIndex = 3;
            this.butCancel.Text = "Close";
            this.butCancel.UseVisualStyleBackColor = true;
            this.butCancel.Click += new System.EventHandler(this.butCancel_Click);
            // 
            // txtStatus
            // 
            this.txtStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtStatus.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtStatus.Location = new System.Drawing.Point(12, 20);
            this.txtStatus.Name = "txtStatus";
            this.txtStatus.ReadOnly = true;
            this.txtStatus.Size = new System.Drawing.Size(357, 13);
            this.txtStatus.TabIndex = 0;
            // 
            // butCancelBackup
            // 
            this.butCancelBackup.Enabled = false;
            this.butCancelBackup.Location = new System.Drawing.Point(13, 79);
            this.butCancelBackup.Name = "butCancelBackup";
            this.butCancelBackup.Size = new System.Drawing.Size(257, 23);
            this.butCancelBackup.TabIndex = 4;
            this.butCancelBackup.Text = "Cancel Backup";
            this.butCancelBackup.UseVisualStyleBackColor = true;
            this.butCancelBackup.Click += new System.EventHandler(this.butCancelBackup_Click);
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // progressBar1
            // 
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar1.Location = new System.Drawing.Point(13, 203);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(438, 23);
            this.progressBar1.TabIndex = 5;
            this.progressBar1.Visible = false;
            // 
            // txtProgress
            // 
            this.txtProgress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtProgress.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtProgress.Location = new System.Drawing.Point(13, 177);
            this.txtProgress.Name = "txtProgress";
            this.txtProgress.ReadOnly = true;
            this.txtProgress.Size = new System.Drawing.Size(438, 13);
            this.txtProgress.TabIndex = 6;
            this.txtProgress.Visible = false;
            // 
            // progressBarOverall
            // 
            this.progressBarOverall.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBarOverall.Location = new System.Drawing.Point(13, 259);
            this.progressBarOverall.Name = "progressBarOverall";
            this.progressBarOverall.Size = new System.Drawing.Size(438, 23);
            this.progressBarOverall.TabIndex = 7;
            this.progressBarOverall.Visible = false;
            // 
            // txtProgressOverall
            // 
            this.txtProgressOverall.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtProgressOverall.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtProgressOverall.Location = new System.Drawing.Point(13, 238);
            this.txtProgressOverall.Name = "txtProgressOverall";
            this.txtProgressOverall.ReadOnly = true;
            this.txtProgressOverall.Size = new System.Drawing.Size(440, 13);
            this.txtProgressOverall.TabIndex = 8;
            this.txtProgressOverall.Visible = false;
            // 
            // txtErrors
            // 
            this.txtErrors.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtErrors.ForeColor = System.Drawing.SystemColors.WindowText;
            this.txtErrors.Location = new System.Drawing.Point(13, 109);
            this.txtErrors.Multiline = true;
            this.txtErrors.Name = "txtErrors";
            this.txtErrors.ReadOnly = true;
            this.txtErrors.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtErrors.Size = new System.Drawing.Size(438, 62);
            this.txtErrors.TabIndex = 9;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(465, 297);
            this.Controls.Add(this.txtErrors);
            this.Controls.Add(this.txtProgressOverall);
            this.Controls.Add(this.progressBarOverall);
            this.Controls.Add(this.txtProgress);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.butCancelBackup);
            this.Controls.Add(this.txtStatus);
            this.Controls.Add(this.butCancel);
            this.Controls.Add(this.butBackup);
            this.Controls.Add(this.butConfigure);
            this.MinimumSize = new System.Drawing.Size(481, 335);
            this.Name = "MainForm";
            this.Text = "Fred\'s Backup";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.Resize += new System.EventHandler(this.MainForm_Resize);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button butConfigure;
        private System.Windows.Forms.Button butBackup;
        private System.Windows.Forms.Button butCancel;
        private System.Windows.Forms.TextBox txtStatus;
        private System.Windows.Forms.Button butCancelBackup;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.TextBox txtProgress;
        private System.Windows.Forms.ProgressBar progressBarOverall;
        private System.Windows.Forms.TextBox txtProgressOverall;
        private System.Windows.Forms.TextBox txtErrors;
    }
}

