namespace MUd {
    partial class KiMailBaseForm {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.fExportButton = new System.Windows.Forms.Button();
            this.fReplaceButton = new System.Windows.Forms.Button();
            this.fSaveButton = new System.Windows.Forms.Button();
            this.fRevertButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // fExportButton
            // 
            this.fExportButton.Location = new System.Drawing.Point(37, 8);
            this.fExportButton.Name = "fExportButton";
            this.fExportButton.Size = new System.Drawing.Size(75, 23);
            this.fExportButton.TabIndex = 1;
            this.fExportButton.Text = "Export";
            this.fExportButton.UseVisualStyleBackColor = true;
            this.fExportButton.Click += new System.EventHandler(this.IExport);
            // 
            // fReplaceButton
            // 
            this.fReplaceButton.Location = new System.Drawing.Point(118, 8);
            this.fReplaceButton.Name = "fReplaceButton";
            this.fReplaceButton.Size = new System.Drawing.Size(75, 23);
            this.fReplaceButton.TabIndex = 2;
            this.fReplaceButton.Text = "Replace";
            this.fReplaceButton.UseVisualStyleBackColor = true;
            this.fReplaceButton.Click += new System.EventHandler(this.IReplace);
            // 
            // fSaveButton
            // 
            this.fSaveButton.Location = new System.Drawing.Point(281, 8);
            this.fSaveButton.Name = "fSaveButton";
            this.fSaveButton.Size = new System.Drawing.Size(75, 23);
            this.fSaveButton.TabIndex = 3;
            this.fSaveButton.Text = "Save";
            this.fSaveButton.UseVisualStyleBackColor = true;
            this.fSaveButton.Click += new System.EventHandler(this.ISave);
            // 
            // fRevertButton
            // 
            this.fRevertButton.Location = new System.Drawing.Point(200, 8);
            this.fRevertButton.Name = "fRevertButton";
            this.fRevertButton.Size = new System.Drawing.Size(75, 23);
            this.fRevertButton.TabIndex = 4;
            this.fRevertButton.Text = "Revert";
            this.fRevertButton.UseVisualStyleBackColor = true;
            this.fRevertButton.Click += new System.EventHandler(this.IRevert);
            // 
            // KiMailBaseForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(400, 337);
            this.Controls.Add(this.fRevertButton);
            this.Controls.Add(this.fSaveButton);
            this.Controls.Add(this.fReplaceButton);
            this.Controls.Add(this.fExportButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "KiMailBaseForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "KI Mail";
            this.ResumeLayout(false);

        }

        #endregion

        protected System.Windows.Forms.Button fExportButton;
        protected System.Windows.Forms.Button fReplaceButton;
        protected System.Windows.Forms.Button fSaveButton;
        protected System.Windows.Forms.Button fRevertButton;
    }
}