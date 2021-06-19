namespace ThumbnailGenerator
{
    partial class Form1
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
            this.label1 = new System.Windows.Forms.Label();
            this.butClose = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.txtStartDirectory = new System.Windows.Forms.TextBox();
            this.txtNumberOfFiles = new System.Windows.Forms.TextBox();
            this.butStart = new System.Windows.Forms.Button();
            this.chkGenerateAlways = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.numThumbnailSize = new System.Windows.Forms.NumericUpDown();
            this.numPreviewWidth = new System.Windows.Forms.NumericUpDown();
            this.numThumbnailsPerSheet = new System.Windows.Forms.NumericUpDown();
            this.numNumberOfPreviewsPerSheet = new System.Windows.Forms.NumericUpDown();
            this.chkGenerateHtml = new System.Windows.Forms.CheckBox();
            this.lblProgress = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numThumbnailSize)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numPreviewWidth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numThumbnailsPerSheet)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numNumberOfPreviewsPerSheet)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(74, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Start Directory";
            // 
            // butClose
            // 
            this.butClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.butClose.Location = new System.Drawing.Point(465, 200);
            this.butClose.Name = "butClose";
            this.butClose.Size = new System.Drawing.Size(75, 23);
            this.butClose.TabIndex = 2;
            this.butClose.Text = "Close";
            this.butClose.UseVisualStyleBackColor = true;
            this.butClose.Click += new System.EventHandler(this.butClose_Click);
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 203);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(83, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Number of Files:";
            // 
            // txtStartDirectory
            // 
            this.txtStartDirectory.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtStartDirectory.Location = new System.Drawing.Point(110, 19);
            this.txtStartDirectory.Name = "txtStartDirectory";
            this.txtStartDirectory.Size = new System.Drawing.Size(430, 20);
            this.txtStartDirectory.TabIndex = 4;
            // 
            // txtNumberOfFiles
            // 
            this.txtNumberOfFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.txtNumberOfFiles.Location = new System.Drawing.Point(110, 200);
            this.txtNumberOfFiles.Name = "txtNumberOfFiles";
            this.txtNumberOfFiles.ReadOnly = true;
            this.txtNumberOfFiles.Size = new System.Drawing.Size(56, 20);
            this.txtNumberOfFiles.TabIndex = 4;
            // 
            // butStart
            // 
            this.butStart.Location = new System.Drawing.Point(110, 163);
            this.butStart.Name = "butStart";
            this.butStart.Size = new System.Drawing.Size(75, 23);
            this.butStart.TabIndex = 5;
            this.butStart.Text = "Start";
            this.butStart.UseVisualStyleBackColor = true;
            this.butStart.Click += new System.EventHandler(this.butStart_Click);
            // 
            // chkGenerateAlways
            // 
            this.chkGenerateAlways.AutoSize = true;
            this.chkGenerateAlways.Location = new System.Drawing.Point(110, 46);
            this.chkGenerateAlways.Name = "chkGenerateAlways";
            this.chkGenerateAlways.Size = new System.Drawing.Size(228, 17);
            this.chkGenerateAlways.TabIndex = 6;
            this.chkGenerateAlways.Text = "Generated Always (also when no new files)";
            this.chkGenerateAlways.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 72);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(80, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Thumbnail size:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(190, 72);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(159, 13);
            this.label4.TabIndex = 9;
            this.label4.Text = "Number of thumbnails per sheet:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(13, 100);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(76, 13);
            this.label5.TabIndex = 11;
            this.label5.Text = "Preview width:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(190, 100);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(182, 13);
            this.label6.TabIndex = 12;
            this.label6.Text = "Number of preview images per sheet:";
            // 
            // numThumbnailSize
            // 
            this.numThumbnailSize.Location = new System.Drawing.Point(110, 70);
            this.numThumbnailSize.Name = "numThumbnailSize";
            this.numThumbnailSize.Size = new System.Drawing.Size(62, 20);
            this.numThumbnailSize.TabIndex = 13;
            // 
            // numPreviewWidth
            // 
            this.numPreviewWidth.Location = new System.Drawing.Point(110, 98);
            this.numPreviewWidth.Name = "numPreviewWidth";
            this.numPreviewWidth.Size = new System.Drawing.Size(62, 20);
            this.numPreviewWidth.TabIndex = 13;
            // 
            // numThumbnailsPerSheet
            // 
            this.numThumbnailsPerSheet.Location = new System.Drawing.Point(387, 70);
            this.numThumbnailsPerSheet.Name = "numThumbnailsPerSheet";
            this.numThumbnailsPerSheet.Size = new System.Drawing.Size(62, 20);
            this.numThumbnailsPerSheet.TabIndex = 13;
            // 
            // numNumberOfPreviewsPerSheet
            // 
            this.numNumberOfPreviewsPerSheet.Location = new System.Drawing.Point(387, 98);
            this.numNumberOfPreviewsPerSheet.Name = "numNumberOfPreviewsPerSheet";
            this.numNumberOfPreviewsPerSheet.Size = new System.Drawing.Size(62, 20);
            this.numNumberOfPreviewsPerSheet.TabIndex = 13;
            // 
            // chkGenerateHtml
            // 
            this.chkGenerateHtml.AutoSize = true;
            this.chkGenerateHtml.Location = new System.Drawing.Point(110, 125);
            this.chkGenerateHtml.Name = "chkGenerateHtml";
            this.chkGenerateHtml.Size = new System.Drawing.Size(124, 17);
            this.chkGenerateHtml.TabIndex = 14;
            this.chkGenerateHtml.Text = "Generate html pages";
            this.chkGenerateHtml.UseVisualStyleBackColor = true;
            // 
            // lblProgress
            // 
            this.lblProgress.AutoSize = true;
            this.lblProgress.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblProgress.Location = new System.Drawing.Point(199, 162);
            this.lblProgress.Name = "lblProgress";
            this.lblProgress.Size = new System.Drawing.Size(0, 24);
            this.lblProgress.TabIndex = 15;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.butClose;
            this.ClientSize = new System.Drawing.Size(552, 243);
            this.Controls.Add(this.lblProgress);
            this.Controls.Add(this.chkGenerateHtml);
            this.Controls.Add(this.numNumberOfPreviewsPerSheet);
            this.Controls.Add(this.numThumbnailsPerSheet);
            this.Controls.Add(this.numPreviewWidth);
            this.Controls.Add(this.numThumbnailSize);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.chkGenerateAlways);
            this.Controls.Add(this.butStart);
            this.Controls.Add(this.txtNumberOfFiles);
            this.Controls.Add(this.txtStartDirectory);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.butClose);
            this.Controls.Add(this.label1);
            this.Name = "Form1";
            this.Text = "Thumbnail generator";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numThumbnailSize)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numPreviewWidth)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numThumbnailsPerSheet)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numNumberOfPreviewsPerSheet)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button butClose;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtStartDirectory;
        private System.Windows.Forms.TextBox txtNumberOfFiles;
        private System.Windows.Forms.Button butStart;
        private System.Windows.Forms.CheckBox chkGenerateAlways;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.NumericUpDown numThumbnailSize;
        private System.Windows.Forms.NumericUpDown numPreviewWidth;
        private System.Windows.Forms.NumericUpDown numThumbnailsPerSheet;
        private System.Windows.Forms.NumericUpDown numNumberOfPreviewsPerSheet;
        private System.Windows.Forms.CheckBox chkGenerateHtml;
        private System.Windows.Forms.Label lblProgress;
    }
}

