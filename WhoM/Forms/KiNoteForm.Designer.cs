namespace MUd {
    partial class KiNoteForm {
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
            this.fTextBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // fTextBox
            // 
            this.fTextBox.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.fTextBox.Location = new System.Drawing.Point(0, 37);
            this.fTextBox.Multiline = true;
            this.fTextBox.Name = "fTextBox";
            this.fTextBox.Size = new System.Drawing.Size(400, 300);
            this.fTextBox.TabIndex = 5;
            this.fTextBox.TextChanged += new System.EventHandler(this.ITextChanged);
            // 
            // KiNoteForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.ClientSize = new System.Drawing.Size(400, 337);
            this.Controls.Add(this.fTextBox);
            this.Name = "KiNoteForm";
            this.Controls.SetChildIndex(this.fTextBox, 0);
            this.Controls.SetChildIndex(this.fExportButton, 0);
            this.Controls.SetChildIndex(this.fReplaceButton, 0);
            this.Controls.SetChildIndex(this.fSaveButton, 0);
            this.Controls.SetChildIndex(this.fRevertButton, 0);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox fTextBox;
    }
}
