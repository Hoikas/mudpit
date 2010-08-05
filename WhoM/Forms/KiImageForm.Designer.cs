namespace MUd {
    partial class KiImageForm {
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
            this.fPictureBox = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.fPictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // fPictureBox
            // 
            this.fPictureBox.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.fPictureBox.Location = new System.Drawing.Point(0, 37);
            this.fPictureBox.Name = "fPictureBox";
            this.fPictureBox.Size = new System.Drawing.Size(400, 300);
            this.fPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.fPictureBox.TabIndex = 5;
            this.fPictureBox.TabStop = false;
            // 
            // KiImageForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.ClientSize = new System.Drawing.Size(400, 337);
            this.Controls.Add(this.fPictureBox);
            this.Name = "KiImageForm";
            this.Controls.SetChildIndex(this.fPictureBox, 0);
            this.Controls.SetChildIndex(this.fExportButton, 0);
            this.Controls.SetChildIndex(this.fReplaceButton, 0);
            this.Controls.SetChildIndex(this.fSaveButton, 0);
            this.Controls.SetChildIndex(this.fRevertButton, 0);
            ((System.ComponentModel.ISupportInitialize)(this.fPictureBox)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox fPictureBox;
    }
}
