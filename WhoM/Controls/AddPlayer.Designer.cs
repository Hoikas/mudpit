namespace MUd {
    partial class AddPlayer {
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.fLabel = new System.Windows.Forms.Label();
            this.fButton = new System.Windows.Forms.Button();
            this.fTextBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // fLabel
            // 
            this.fLabel.AutoSize = true;
            this.fLabel.Location = new System.Drawing.Point(3, 8);
            this.fLabel.Name = "fLabel";
            this.fLabel.Size = new System.Drawing.Size(79, 13);
            this.fLabel.TabIndex = 0;
            this.fLabel.Text = "Add Something";
            // 
            // fButton
            // 
            this.fButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.fButton.Enabled = false;
            this.fButton.Location = new System.Drawing.Point(197, 3);
            this.fButton.Name = "fButton";
            this.fButton.Size = new System.Drawing.Size(37, 23);
            this.fButton.TabIndex = 3;
            this.fButton.Text = "Add";
            this.fButton.UseVisualStyleBackColor = true;
            this.fButton.Click += new System.EventHandler(this.IButtonClicked);
            // 
            // fTextBox
            // 
            this.fTextBox.Location = new System.Drawing.Point(91, 5);
            this.fTextBox.Name = "fTextBox";
            this.fTextBox.Size = new System.Drawing.Size(100, 20);
            this.fTextBox.TabIndex = 2;
            this.fTextBox.TextChanged += new System.EventHandler(this.ITextChanged);
            this.fTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.IKeyPress);
            // 
            // AddPlayer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.fTextBox);
            this.Controls.Add(this.fButton);
            this.Controls.Add(this.fLabel);
            this.Name = "AddPlayer";
            this.Size = new System.Drawing.Size(240, 25);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label fLabel;
        private System.Windows.Forms.Button fButton;
        private System.Windows.Forms.TextBox fTextBox;
    }
}
