namespace MUd {
    partial class OptionsForm {
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
            this.fNotificationsGroup = new System.Windows.Forms.GroupBox();
            this.fNeighborLogin = new System.Windows.Forms.CheckBox();
            this.fBuddyLogin = new System.Windows.Forms.CheckBox();
            this.fOkayButton = new System.Windows.Forms.Button();
            this.fCancelButton = new System.Windows.Forms.Button();
            this.fNotificationsGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // fNotificationsGroup
            // 
            this.fNotificationsGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.fNotificationsGroup.Controls.Add(this.fNeighborLogin);
            this.fNotificationsGroup.Controls.Add(this.fBuddyLogin);
            this.fNotificationsGroup.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.fNotificationsGroup.Location = new System.Drawing.Point(13, 13);
            this.fNotificationsGroup.Name = "fNotificationsGroup";
            this.fNotificationsGroup.Size = new System.Drawing.Size(157, 67);
            this.fNotificationsGroup.TabIndex = 0;
            this.fNotificationsGroup.TabStop = false;
            this.fNotificationsGroup.Text = "Display Alerts On";
            // 
            // fNeighborLogin
            // 
            this.fNeighborLogin.AutoSize = true;
            this.fNeighborLogin.ForeColor = System.Drawing.SystemColors.ControlText;
            this.fNeighborLogin.Location = new System.Drawing.Point(6, 42);
            this.fNeighborLogin.Name = "fNeighborLogin";
            this.fNeighborLogin.Size = new System.Drawing.Size(98, 17);
            this.fNeighborLogin.TabIndex = 2;
            this.fNeighborLogin.Text = "Neighbor Login";
            this.fNeighborLogin.UseVisualStyleBackColor = true;
            // 
            // fBuddyLogin
            // 
            this.fBuddyLogin.AutoSize = true;
            this.fBuddyLogin.ForeColor = System.Drawing.SystemColors.ControlText;
            this.fBuddyLogin.Location = new System.Drawing.Point(6, 19);
            this.fBuddyLogin.Name = "fBuddyLogin";
            this.fBuddyLogin.Size = new System.Drawing.Size(85, 17);
            this.fBuddyLogin.TabIndex = 0;
            this.fBuddyLogin.Text = "Buddy Login";
            this.fBuddyLogin.UseVisualStyleBackColor = true;
            // 
            // fOkayButton
            // 
            this.fOkayButton.Location = new System.Drawing.Point(13, 98);
            this.fOkayButton.Name = "fOkayButton";
            this.fOkayButton.Size = new System.Drawing.Size(75, 23);
            this.fOkayButton.TabIndex = 1;
            this.fOkayButton.Text = "OK";
            this.fOkayButton.UseVisualStyleBackColor = true;
            this.fOkayButton.Click += new System.EventHandler(this.ISaveChanges);
            // 
            // fCancelButton
            // 
            this.fCancelButton.Location = new System.Drawing.Point(94, 98);
            this.fCancelButton.Name = "fCancelButton";
            this.fCancelButton.Size = new System.Drawing.Size(75, 23);
            this.fCancelButton.TabIndex = 2;
            this.fCancelButton.Text = "Cancel";
            this.fCancelButton.UseVisualStyleBackColor = true;
            this.fCancelButton.Click += new System.EventHandler(this.ICancel);
            // 
            // OptionsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(182, 145);
            this.Controls.Add(this.fCancelButton);
            this.Controls.Add(this.fOkayButton);
            this.Controls.Add(this.fNotificationsGroup);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "OptionsForm";
            this.ShowIcon = false;
            this.Text = "Options";
            this.fNotificationsGroup.ResumeLayout(false);
            this.fNotificationsGroup.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox fNotificationsGroup;
        private System.Windows.Forms.CheckBox fBuddyLogin;
        private System.Windows.Forms.CheckBox fNeighborLogin;
        private System.Windows.Forms.Button fOkayButton;
        private System.Windows.Forms.Button fCancelButton;
    }
}