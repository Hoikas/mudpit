namespace MUd {
    partial class ConnectForm {
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConnectForm));
            this.fShardDropDown = new System.Windows.Forms.ComboBox();
            this.fShardHostBox = new System.Windows.Forms.TextBox();
            this.fShardLabel = new System.Windows.Forms.Label();
            this.fShardHostLabel = new System.Windows.Forms.Label();
            this.fUserBox = new System.Windows.Forms.TextBox();
            this.fPasswordBox = new System.Windows.Forms.TextBox();
            this.fUserLabel = new System.Windows.Forms.Label();
            this.fPasswordLabel = new System.Windows.Forms.Label();
            this.fLogin = new System.Windows.Forms.Button();
            this.fClose = new System.Windows.Forms.Button();
            this.fRememberMe = new System.Windows.Forms.CheckBox();
            this.fAutoConnect = new System.Windows.Forms.CheckBox();
            this.fHelloLabel = new System.Windows.Forms.Label();
            this.fRefreshLink = new System.Windows.Forms.LinkLabel();
            this.fRefreshImage = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.fRefreshImage)).BeginInit();
            this.SuspendLayout();
            // 
            // fShardDropDown
            // 
            this.fShardDropDown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.fShardDropDown.Enabled = false;
            this.fShardDropDown.FormattingEnabled = true;
            this.fShardDropDown.Location = new System.Drawing.Point(104, 40);
            this.fShardDropDown.Name = "fShardDropDown";
            this.fShardDropDown.Size = new System.Drawing.Size(147, 21);
            this.fShardDropDown.TabIndex = 0;
            this.fShardDropDown.SelectedIndexChanged += new System.EventHandler(this.IShardChanged);
            // 
            // fShardHostBox
            // 
            this.fShardHostBox.Enabled = false;
            this.fShardHostBox.Location = new System.Drawing.Point(104, 67);
            this.fShardHostBox.Name = "fShardHostBox";
            this.fShardHostBox.Size = new System.Drawing.Size(147, 20);
            this.fShardHostBox.TabIndex = 1;
            // 
            // fShardLabel
            // 
            this.fShardLabel.AutoSize = true;
            this.fShardLabel.Location = new System.Drawing.Point(32, 43);
            this.fShardLabel.Name = "fShardLabel";
            this.fShardLabel.Size = new System.Drawing.Size(66, 13);
            this.fShardLabel.TabIndex = 2;
            this.fShardLabel.Text = "Shard Name";
            // 
            // fShardHostLabel
            // 
            this.fShardHostLabel.AutoSize = true;
            this.fShardHostLabel.Location = new System.Drawing.Point(12, 70);
            this.fShardHostLabel.Name = "fShardHostLabel";
            this.fShardHostLabel.Size = new System.Drawing.Size(86, 13);
            this.fShardHostLabel.TabIndex = 3;
            this.fShardHostLabel.Text = "Shard Hostname";
            // 
            // fUserBox
            // 
            this.fUserBox.Location = new System.Drawing.Point(104, 92);
            this.fUserBox.Name = "fUserBox";
            this.fUserBox.Size = new System.Drawing.Size(147, 20);
            this.fUserBox.TabIndex = 4;
            // 
            // fPasswordBox
            // 
            this.fPasswordBox.Location = new System.Drawing.Point(104, 118);
            this.fPasswordBox.Name = "fPasswordBox";
            this.fPasswordBox.Size = new System.Drawing.Size(147, 20);
            this.fPasswordBox.TabIndex = 5;
            this.fPasswordBox.UseSystemPasswordChar = true;
            // 
            // fUserLabel
            // 
            this.fUserLabel.AutoSize = true;
            this.fUserLabel.Location = new System.Drawing.Point(20, 95);
            this.fUserLabel.Name = "fUserLabel";
            this.fUserLabel.Size = new System.Drawing.Size(78, 13);
            this.fUserLabel.TabIndex = 6;
            this.fUserLabel.Text = "Account Name";
            // 
            // fPasswordLabel
            // 
            this.fPasswordLabel.AutoSize = true;
            this.fPasswordLabel.Location = new System.Drawing.Point(45, 121);
            this.fPasswordLabel.Name = "fPasswordLabel";
            this.fPasswordLabel.Size = new System.Drawing.Size(53, 13);
            this.fPasswordLabel.TabIndex = 7;
            this.fPasswordLabel.Text = "Password";
            // 
            // fLogin
            // 
            this.fLogin.Enabled = false;
            this.fLogin.Location = new System.Drawing.Point(66, 213);
            this.fLogin.Name = "fLogin";
            this.fLogin.Size = new System.Drawing.Size(75, 23);
            this.fLogin.TabIndex = 9;
            this.fLogin.Text = "Login";
            this.fLogin.UseVisualStyleBackColor = true;
            this.fLogin.Click += new System.EventHandler(this.IBeginLoginProcess);
            // 
            // fClose
            // 
            this.fClose.Location = new System.Drawing.Point(148, 213);
            this.fClose.Name = "fClose";
            this.fClose.Size = new System.Drawing.Size(75, 23);
            this.fClose.TabIndex = 10;
            this.fClose.Text = "Close";
            this.fClose.UseVisualStyleBackColor = true;
            this.fClose.Click += new System.EventHandler(this.ICloseForm);
            // 
            // fRememberMe
            // 
            this.fRememberMe.AutoSize = true;
            this.fRememberMe.Location = new System.Drawing.Point(106, 167);
            this.fRememberMe.Name = "fRememberMe";
            this.fRememberMe.Size = new System.Drawing.Size(95, 17);
            this.fRememberMe.TabIndex = 7;
            this.fRememberMe.Text = "Remember Me";
            this.fRememberMe.UseVisualStyleBackColor = true;
            this.fRememberMe.CheckedChanged += new System.EventHandler(this.IRememberMeChecked);
            // 
            // fAutoConnect
            // 
            this.fAutoConnect.AutoSize = true;
            this.fAutoConnect.Enabled = false;
            this.fAutoConnect.Location = new System.Drawing.Point(106, 190);
            this.fAutoConnect.Name = "fAutoConnect";
            this.fAutoConnect.Size = new System.Drawing.Size(106, 17);
            this.fAutoConnect.TabIndex = 8;
            this.fAutoConnect.Text = "Connect on Start";
            this.fAutoConnect.UseVisualStyleBackColor = true;
            // 
            // fHelloLabel
            // 
            this.fHelloLabel.AutoSize = true;
            this.fHelloLabel.Location = new System.Drawing.Point(35, 13);
            this.fHelloLabel.Name = "fHelloLabel";
            this.fHelloLabel.Size = new System.Drawing.Size(167, 13);
            this.fHelloLabel.TabIndex = 12;
            this.fHelloLabel.Text = "Please choose a shard to login to.";
            // 
            // fRefreshLink
            // 
            this.fRefreshLink.AutoSize = true;
            this.fRefreshLink.Location = new System.Drawing.Point(101, 146);
            this.fRefreshLink.Name = "fRefreshLink";
            this.fRefreshLink.Size = new System.Drawing.Size(94, 13);
            this.fRefreshLink.TabIndex = 6;
            this.fRefreshLink.TabStop = true;
            this.fRefreshLink.Text = "Refresh Shard List";
            this.fRefreshLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.IRefreshShards);
            // 
            // fRefreshImage
            // 
            this.fRefreshImage.Image = ((System.Drawing.Image)(resources.GetObject("fRefreshImage.Image")));
            this.fRefreshImage.Location = new System.Drawing.Point(79, 145);
            this.fRefreshImage.Name = "fRefreshImage";
            this.fRefreshImage.Size = new System.Drawing.Size(16, 16);
            this.fRefreshImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.fRefreshImage.TabIndex = 14;
            this.fRefreshImage.TabStop = false;
            // 
            // ConnectForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(291, 248);
            this.Controls.Add(this.fRefreshImage);
            this.Controls.Add(this.fRefreshLink);
            this.Controls.Add(this.fHelloLabel);
            this.Controls.Add(this.fAutoConnect);
            this.Controls.Add(this.fRememberMe);
            this.Controls.Add(this.fClose);
            this.Controls.Add(this.fLogin);
            this.Controls.Add(this.fPasswordLabel);
            this.Controls.Add(this.fUserLabel);
            this.Controls.Add(this.fPasswordBox);
            this.Controls.Add(this.fUserBox);
            this.Controls.Add(this.fShardHostLabel);
            this.Controls.Add(this.fShardLabel);
            this.Controls.Add(this.fShardHostBox);
            this.Controls.Add(this.fShardDropDown);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ConnectForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Connect to a Shard";
            ((System.ComponentModel.ISupportInitialize)(this.fRefreshImage)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox fShardDropDown;
        private System.Windows.Forms.TextBox fShardHostBox;
        private System.Windows.Forms.Label fShardLabel;
        private System.Windows.Forms.Label fShardHostLabel;
        private System.Windows.Forms.TextBox fUserBox;
        private System.Windows.Forms.TextBox fPasswordBox;
        private System.Windows.Forms.Label fUserLabel;
        private System.Windows.Forms.Label fPasswordLabel;
        private System.Windows.Forms.Button fLogin;
        private System.Windows.Forms.Button fClose;
        private System.Windows.Forms.CheckBox fRememberMe;
        private System.Windows.Forms.CheckBox fAutoConnect;
        private System.Windows.Forms.Label fHelloLabel;
        private System.Windows.Forms.LinkLabel fRefreshLink;
        private System.Windows.Forms.PictureBox fRefreshImage;
    }
}