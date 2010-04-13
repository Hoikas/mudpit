﻿namespace MUd {
    partial class MainForm {
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.fMainMenu = new System.Windows.Forms.MenuStrip();
            this.fFileMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.fConnectMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fDisconnectMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fFileMenuSeparator01 = new System.Windows.Forms.ToolStripSeparator();
            this.fOptionsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fFileMenuSeparator02 = new System.Windows.Forms.ToolStripSeparator();
            this.fExitMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fHelpMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.fLinksMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fDrcForumMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fMudSiteMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fMoulForumMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fAboutMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fTabControl = new System.Windows.Forms.TabControl();
            this.fBuddiesTab = new System.Windows.Forms.TabPage();
            this.fAddBuddy = new MUd.AddPlayer();
            this.fBuddyCtrl = new MUd.OnlinePlayers();
            this.fNeighborsPage = new System.Windows.Forms.TabPage();
            this.fNeighborsCtrl = new MUd.OnlinePlayers();
            this.fRecentsPage = new System.Windows.Forms.TabPage();
            this.fRecentsCtrl = new MUd.OnlinePlayers();
            this.fAvatarSelector = new System.Windows.Forms.ComboBox();
            this.fAvatarLabel = new System.Windows.Forms.Label();
            this.fStatusStrip = new System.Windows.Forms.StatusStrip();
            this.fProgressBar = new System.Windows.Forms.ToolStripProgressBar();
            this.fProgressLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.fNotifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.fMainMenu.SuspendLayout();
            this.fTabControl.SuspendLayout();
            this.fBuddiesTab.SuspendLayout();
            this.fNeighborsPage.SuspendLayout();
            this.fRecentsPage.SuspendLayout();
            this.fStatusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // fMainMenu
            // 
            this.fMainMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fFileMenu,
            this.fHelpMenu});
            this.fMainMenu.Location = new System.Drawing.Point(0, 0);
            this.fMainMenu.Name = "fMainMenu";
            this.fMainMenu.Size = new System.Drawing.Size(454, 24);
            this.fMainMenu.TabIndex = 0;
            this.fMainMenu.Text = "menuStrip1";
            // 
            // fFileMenu
            // 
            this.fFileMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fConnectMenuItem,
            this.fDisconnectMenuItem,
            this.fFileMenuSeparator01,
            this.fOptionsMenuItem,
            this.fFileMenuSeparator02,
            this.fExitMenuItem});
            this.fFileMenu.Name = "fFileMenu";
            this.fFileMenu.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F)));
            this.fFileMenu.ShowShortcutKeys = false;
            this.fFileMenu.Size = new System.Drawing.Size(37, 20);
            this.fFileMenu.Text = "&File";
            // 
            // fConnectMenuItem
            // 
            this.fConnectMenuItem.Name = "fConnectMenuItem";
            this.fConnectMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.C)));
            this.fConnectMenuItem.ShowShortcutKeys = false;
            this.fConnectMenuItem.Size = new System.Drawing.Size(126, 22);
            this.fConnectMenuItem.Tag = "";
            this.fConnectMenuItem.Text = "&Connect";
            this.fConnectMenuItem.Click += new System.EventHandler(this.IConnect);
            // 
            // fDisconnectMenuItem
            // 
            this.fDisconnectMenuItem.Enabled = false;
            this.fDisconnectMenuItem.Name = "fDisconnectMenuItem";
            this.fDisconnectMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.D)));
            this.fDisconnectMenuItem.ShowShortcutKeys = false;
            this.fDisconnectMenuItem.Size = new System.Drawing.Size(126, 22);
            this.fDisconnectMenuItem.Text = "&Disconnect";
            this.fDisconnectMenuItem.Click += new System.EventHandler(this.IDisconnect);
            // 
            // fFileMenuSeparator01
            // 
            this.fFileMenuSeparator01.Name = "fFileMenuSeparator01";
            this.fFileMenuSeparator01.Size = new System.Drawing.Size(123, 6);
            // 
            // fOptionsMenuItem
            // 
            this.fOptionsMenuItem.Name = "fOptionsMenuItem";
            this.fOptionsMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.O)));
            this.fOptionsMenuItem.ShowShortcutKeys = false;
            this.fOptionsMenuItem.Size = new System.Drawing.Size(126, 22);
            this.fOptionsMenuItem.Text = "&Options";
            this.fOptionsMenuItem.Click += new System.EventHandler(this.IOptions);
            // 
            // fFileMenuSeparator02
            // 
            this.fFileMenuSeparator02.Name = "fFileMenuSeparator02";
            this.fFileMenuSeparator02.Size = new System.Drawing.Size(123, 6);
            // 
            // fExitMenuItem
            // 
            this.fExitMenuItem.Name = "fExitMenuItem";
            this.fExitMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.X)));
            this.fExitMenuItem.ShowShortcutKeys = false;
            this.fExitMenuItem.Size = new System.Drawing.Size(126, 22);
            this.fExitMenuItem.Text = "E&xit";
            this.fExitMenuItem.Click += new System.EventHandler(this.IExit);
            // 
            // fHelpMenu
            // 
            this.fHelpMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fLinksMenuItem,
            this.fAboutMenuItem});
            this.fHelpMenu.Name = "fHelpMenu";
            this.fHelpMenu.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.H)));
            this.fHelpMenu.Size = new System.Drawing.Size(44, 20);
            this.fHelpMenu.Text = "&Help";
            // 
            // fLinksMenuItem
            // 
            this.fLinksMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fDrcForumMenuItem,
            this.fMudSiteMenuItem,
            this.fMoulForumMenuItem});
            this.fLinksMenuItem.Name = "fLinksMenuItem";
            this.fLinksMenuItem.Size = new System.Drawing.Size(152, 22);
            this.fLinksMenuItem.Text = "Links";
            // 
            // fDrcForumMenuItem
            // 
            this.fDrcForumMenuItem.Name = "fDrcForumMenuItem";
            this.fDrcForumMenuItem.Size = new System.Drawing.Size(176, 22);
            this.fDrcForumMenuItem.Text = "DRC Forum";
            this.fDrcForumMenuItem.Click += new System.EventHandler(this.IOpenDrcForum);
            // 
            // fMudSiteMenuItem
            // 
            this.fMudSiteMenuItem.Name = "fMudSiteMenuItem";
            this.fMudSiteMenuItem.Size = new System.Drawing.Size(176, 22);
            this.fMudSiteMenuItem.Text = "MUd Project Site";
            this.fMudSiteMenuItem.Click += new System.EventHandler(this.IOpenMUdSite);
            // 
            // fMoulForumMenuItem
            // 
            this.fMoulForumMenuItem.Name = "fMoulForumMenuItem";
            this.fMoulForumMenuItem.Size = new System.Drawing.Size(176, 22);
            this.fMoulForumMenuItem.Text = "Myst Online Forum";
            this.fMoulForumMenuItem.Click += new System.EventHandler(this.IOpenMOULForum);
            // 
            // fAboutMenuItem
            // 
            this.fAboutMenuItem.Name = "fAboutMenuItem";
            this.fAboutMenuItem.ShowShortcutKeys = false;
            this.fAboutMenuItem.Size = new System.Drawing.Size(152, 22);
            this.fAboutMenuItem.Text = "&About";
            this.fAboutMenuItem.Click += new System.EventHandler(this.IAboutMe);
            // 
            // fTabControl
            // 
            this.fTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.fTabControl.Controls.Add(this.fBuddiesTab);
            this.fTabControl.Controls.Add(this.fNeighborsPage);
            this.fTabControl.Controls.Add(this.fRecentsPage);
            this.fTabControl.Location = new System.Drawing.Point(0, 55);
            this.fTabControl.Name = "fTabControl";
            this.fTabControl.SelectedIndex = 0;
            this.fTabControl.Size = new System.Drawing.Size(450, 494);
            this.fTabControl.TabIndex = 1;
            this.fTabControl.Tag = "";
            this.fTabControl.Selected += new System.Windows.Forms.TabControlEventHandler(this.ITabSelected);
            // 
            // fBuddiesTab
            // 
            this.fBuddiesTab.Controls.Add(this.fAddBuddy);
            this.fBuddiesTab.Controls.Add(this.fBuddyCtrl);
            this.fBuddiesTab.Location = new System.Drawing.Point(4, 22);
            this.fBuddiesTab.Name = "fBuddiesTab";
            this.fBuddiesTab.Padding = new System.Windows.Forms.Padding(3);
            this.fBuddiesTab.Size = new System.Drawing.Size(442, 468);
            this.fBuddiesTab.TabIndex = 0;
            this.fBuddiesTab.Tag = "buddies";
            this.fBuddiesTab.Text = "Buddies List";
            this.fBuddiesTab.UseVisualStyleBackColor = true;
            // 
            // fAddBuddy
            // 
            this.fAddBuddy.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.fAddBuddy.Label = "Add New Buddy";
            this.fAddBuddy.Location = new System.Drawing.Point(196, 440);
            this.fAddBuddy.Name = "fAddBuddy";
            this.fAddBuddy.Size = new System.Drawing.Size(240, 25);
            this.fAddBuddy.TabIndex = 1;
            this.fAddBuddy.Add += new System.Action<uint>(this.IAddBuddy);
            // 
            // fBuddyCtrl
            // 
            this.fBuddyCtrl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.fBuddyCtrl.Location = new System.Drawing.Point(3, 3);
            this.fBuddyCtrl.Name = "fBuddyCtrl";
            this.fBuddyCtrl.Size = new System.Drawing.Size(436, 431);
            this.fBuddyCtrl.TabIndex = 0;
            // 
            // fNeighborsPage
            // 
            this.fNeighborsPage.Controls.Add(this.fNeighborsCtrl);
            this.fNeighborsPage.Location = new System.Drawing.Point(4, 22);
            this.fNeighborsPage.Name = "fNeighborsPage";
            this.fNeighborsPage.Padding = new System.Windows.Forms.Padding(3);
            this.fNeighborsPage.Size = new System.Drawing.Size(442, 468);
            this.fNeighborsPage.TabIndex = 1;
            this.fNeighborsPage.Tag = "neighbors";
            this.fNeighborsPage.Text = "Neighbors";
            this.fNeighborsPage.UseVisualStyleBackColor = true;
            // 
            // fNeighborsCtrl
            // 
            this.fNeighborsCtrl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fNeighborsCtrl.Location = new System.Drawing.Point(3, 3);
            this.fNeighborsCtrl.Name = "fNeighborsCtrl";
            this.fNeighborsCtrl.Size = new System.Drawing.Size(436, 462);
            this.fNeighborsCtrl.TabIndex = 0;
            // 
            // fRecentsPage
            // 
            this.fRecentsPage.Controls.Add(this.fRecentsCtrl);
            this.fRecentsPage.Location = new System.Drawing.Point(4, 22);
            this.fRecentsPage.Name = "fRecentsPage";
            this.fRecentsPage.Padding = new System.Windows.Forms.Padding(3);
            this.fRecentsPage.Size = new System.Drawing.Size(442, 468);
            this.fRecentsPage.TabIndex = 2;
            this.fRecentsPage.Tag = "recents";
            this.fRecentsPage.Text = "Recent List";
            this.fRecentsPage.UseVisualStyleBackColor = true;
            // 
            // fRecentsCtrl
            // 
            this.fRecentsCtrl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fRecentsCtrl.Location = new System.Drawing.Point(3, 3);
            this.fRecentsCtrl.Name = "fRecentsCtrl";
            this.fRecentsCtrl.Size = new System.Drawing.Size(436, 462);
            this.fRecentsCtrl.TabIndex = 0;
            // 
            // fAvatarSelector
            // 
            this.fAvatarSelector.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.fAvatarSelector.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.fAvatarSelector.FormattingEnabled = true;
            this.fAvatarSelector.Location = new System.Drawing.Point(259, 27);
            this.fAvatarSelector.Name = "fAvatarSelector";
            this.fAvatarSelector.Size = new System.Drawing.Size(191, 21);
            this.fAvatarSelector.TabIndex = 2;
            this.fAvatarSelector.SelectedIndexChanged += new System.EventHandler(this.IAvatarSelected);
            // 
            // fAvatarLabel
            // 
            this.fAvatarLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.fAvatarLabel.AutoSize = true;
            this.fAvatarLabel.Location = new System.Drawing.Point(164, 30);
            this.fAvatarLabel.Name = "fAvatarLabel";
            this.fAvatarLabel.Size = new System.Drawing.Size(89, 13);
            this.fAvatarLabel.TabIndex = 3;
            this.fAvatarLabel.Text = "Avatars Available";
            // 
            // fStatusStrip
            // 
            this.fStatusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fProgressBar,
            this.fProgressLabel});
            this.fStatusStrip.Location = new System.Drawing.Point(0, 552);
            this.fStatusStrip.Name = "fStatusStrip";
            this.fStatusStrip.Size = new System.Drawing.Size(454, 22);
            this.fStatusStrip.TabIndex = 4;
            this.fStatusStrip.Text = "statusStrip1";
            // 
            // fProgressBar
            // 
            this.fProgressBar.Name = "fProgressBar";
            this.fProgressBar.Size = new System.Drawing.Size(100, 16);
            this.fProgressBar.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.fProgressBar.Visible = false;
            // 
            // fProgressLabel
            // 
            this.fProgressLabel.Name = "fProgressLabel";
            this.fProgressLabel.Size = new System.Drawing.Size(139, 17);
            this.fProgressLabel.Text = "Downloading Player Tree";
            this.fProgressLabel.Visible = false;
            // 
            // fNotifyIcon
            // 
            this.fNotifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("fNotifyIcon.Icon")));
            this.fNotifyIcon.Text = "WhoM";
            this.fNotifyIcon.Visible = true;
            this.fNotifyIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.INotifyIconDoubleClicked);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(454, 574);
            this.Controls.Add(this.fStatusStrip);
            this.Controls.Add(this.fAvatarLabel);
            this.Controls.Add(this.fAvatarSelector);
            this.Controls.Add(this.fTabControl);
            this.Controls.Add(this.fMainMenu);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.fMainMenu;
            this.Name = "MainForm";
            this.Text = "WhoM";
            this.Shown += new System.EventHandler(this.IFormShown);
            this.Resize += new System.EventHandler(this.IFormResized);
            this.fMainMenu.ResumeLayout(false);
            this.fMainMenu.PerformLayout();
            this.fTabControl.ResumeLayout(false);
            this.fBuddiesTab.ResumeLayout(false);
            this.fNeighborsPage.ResumeLayout(false);
            this.fRecentsPage.ResumeLayout(false);
            this.fStatusStrip.ResumeLayout(false);
            this.fStatusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip fMainMenu;
        private System.Windows.Forms.ToolStripMenuItem fFileMenu;
        private System.Windows.Forms.ToolStripMenuItem fConnectMenuItem;
        private System.Windows.Forms.TabControl fTabControl;
        private System.Windows.Forms.TabPage fBuddiesTab;
        private System.Windows.Forms.TabPage fNeighborsPage;
        private System.Windows.Forms.ComboBox fAvatarSelector;
        private System.Windows.Forms.Label fAvatarLabel;
        private System.Windows.Forms.ToolStripMenuItem fHelpMenu;
        private System.Windows.Forms.ToolStripMenuItem fDisconnectMenuItem;
        private System.Windows.Forms.TabPage fRecentsPage;
        private System.Windows.Forms.StatusStrip fStatusStrip;
        private System.Windows.Forms.ToolStripProgressBar fProgressBar;
        private System.Windows.Forms.ToolStripStatusLabel fProgressLabel;
        private OnlinePlayers fBuddyCtrl;
        private System.Windows.Forms.ToolStripMenuItem fAboutMenuItem;
        private OnlinePlayers fNeighborsCtrl;
        private OnlinePlayers fRecentsCtrl;
        private AddPlayer fAddBuddy;
        private System.Windows.Forms.ToolStripSeparator fFileMenuSeparator01;
        private System.Windows.Forms.ToolStripMenuItem fOptionsMenuItem;
        private System.Windows.Forms.ToolStripSeparator fFileMenuSeparator02;
        private System.Windows.Forms.ToolStripMenuItem fExitMenuItem;
        private System.Windows.Forms.NotifyIcon fNotifyIcon;
        private System.Windows.Forms.ToolStripMenuItem fLinksMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fMoulForumMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fMudSiteMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fDrcForumMenuItem;
    }
}

