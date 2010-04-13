namespace MUd {
    partial class ExceptionForm {
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExceptionForm));
            this.fErrorPicture = new System.Windows.Forms.PictureBox();
            this.fErrorLabel = new System.Windows.Forms.Label();
            this.fOkay = new System.Windows.Forms.Button();
            this.fCopyDump = new System.Windows.Forms.Button();
            this.fTextBox = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.fErrorPicture)).BeginInit();
            this.SuspendLayout();
            // 
            // fErrorPicture
            // 
            this.fErrorPicture.Image = ((System.Drawing.Image)(resources.GetObject("fErrorPicture.Image")));
            this.fErrorPicture.Location = new System.Drawing.Point(13, 13);
            this.fErrorPicture.Name = "fErrorPicture";
            this.fErrorPicture.Size = new System.Drawing.Size(64, 64);
            this.fErrorPicture.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.fErrorPicture.TabIndex = 0;
            this.fErrorPicture.TabStop = false;
            // 
            // fErrorLabel
            // 
            this.fErrorLabel.AutoSize = true;
            this.fErrorLabel.Location = new System.Drawing.Point(87, 13);
            this.fErrorLabel.Name = "fErrorLabel";
            this.fErrorLabel.Size = new System.Drawing.Size(283, 65);
            this.fErrorLabel.TabIndex = 1;
            this.fErrorLabel.Text = resources.GetString("fErrorLabel.Text");
            // 
            // fOkay
            // 
            this.fOkay.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.fOkay.Location = new System.Drawing.Point(441, 13);
            this.fOkay.Name = "fOkay";
            this.fOkay.Size = new System.Drawing.Size(75, 23);
            this.fOkay.TabIndex = 3;
            this.fOkay.Text = "OK";
            this.fOkay.UseVisualStyleBackColor = true;
            this.fOkay.Click += new System.EventHandler(this.IOkay);
            // 
            // fCopyDump
            // 
            this.fCopyDump.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.fCopyDump.Location = new System.Drawing.Point(440, 43);
            this.fCopyDump.Name = "fCopyDump";
            this.fCopyDump.Size = new System.Drawing.Size(75, 23);
            this.fCopyDump.TabIndex = 4;
            this.fCopyDump.Text = "Copy Dump";
            this.fCopyDump.UseVisualStyleBackColor = true;
            this.fCopyDump.Click += new System.EventHandler(this.ICopyDump);
            // 
            // fTextBox
            // 
            this.fTextBox.Location = new System.Drawing.Point(13, 102);
            this.fTextBox.Multiline = true;
            this.fTextBox.Name = "fTextBox";
            this.fTextBox.ReadOnly = true;
            this.fTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.fTextBox.Size = new System.Drawing.Size(502, 157);
            this.fTextBox.TabIndex = 5;
            this.fTextBox.Text = "Unhandled Exception\r\nProgram: MUd.WhoM\r\nVersion {0}\r\n\r\nException Type: {1}\r\nExcep" +
                "tion Message: {2}\r\nException Source: {3}\r\n\r\nInnerException Type: {4}\r\nInnerExcep" +
                "tion Message: {5}\r\n\r\nStack Trace";
            // 
            // ExceptionForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(528, 271);
            this.Controls.Add(this.fTextBox);
            this.Controls.Add(this.fCopyDump);
            this.Controls.Add(this.fOkay);
            this.Controls.Add(this.fErrorLabel);
            this.Controls.Add(this.fErrorPicture);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ExceptionForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Unhandled Exception";
            ((System.ComponentModel.ISupportInitialize)(this.fErrorPicture)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox fErrorPicture;
        private System.Windows.Forms.Label fErrorLabel;
        private System.Windows.Forms.Button fOkay;
        private System.Windows.Forms.Button fCopyDump;
        private System.Windows.Forms.TextBox fTextBox;
    }
}