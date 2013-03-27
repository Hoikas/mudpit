namespace MUd {
    partial class KiMailControl {
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
            this.components = new System.ComponentModel.Container();
            this.fJournalItemMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.createTextNoteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fDeleteKiItemMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fRenameKiItemMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fGridPanel = new System.Windows.Forms.Panel();
            this.fGridSplitter = new System.Windows.Forms.Splitter();
            this.fMailList = new System.Windows.Forms.ListView();
            this.fKiMailColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.fAgeList = new System.Windows.Forms.ListView();
            this.fAgesColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.fFolderItemMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.fDeleteFolderMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fShowEmptyFolders = new System.Windows.Forms.CheckBox();
            this.fJournalItemMenu.SuspendLayout();
            this.fGridPanel.SuspendLayout();
            this.fFolderItemMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // fJournalItemMenu
            // 
            this.fJournalItemMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.createTextNoteToolStripMenuItem,
            this.fDeleteKiItemMenuItem,
            this.fRenameKiItemMenuItem});
            this.fJournalItemMenu.Name = "fJournalItemMenu";
            this.fJournalItemMenu.Size = new System.Drawing.Size(163, 92);
            // 
            // createTextNoteToolStripMenuItem
            // 
            this.createTextNoteToolStripMenuItem.Name = "createTextNoteToolStripMenuItem";
            this.createTextNoteToolStripMenuItem.Size = new System.Drawing.Size(162, 22);
            this.createTextNoteToolStripMenuItem.Text = "&Create Text Note";
            this.createTextNoteToolStripMenuItem.Click += new System.EventHandler(this.ICreateTextNote);
            // 
            // fDeleteKiItemMenuItem
            // 
            this.fDeleteKiItemMenuItem.Name = "fDeleteKiItemMenuItem";
            this.fDeleteKiItemMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Delete;
            this.fDeleteKiItemMenuItem.ShowShortcutKeys = false;
            this.fDeleteKiItemMenuItem.Size = new System.Drawing.Size(162, 22);
            this.fDeleteKiItemMenuItem.Text = "&Delete";
            this.fDeleteKiItemMenuItem.Visible = false;
            this.fDeleteKiItemMenuItem.Click += new System.EventHandler(this.IDeleteKiItem);
            // 
            // fRenameKiItemMenuItem
            // 
            this.fRenameKiItemMenuItem.Name = "fRenameKiItemMenuItem";
            this.fRenameKiItemMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.N)));
            this.fRenameKiItemMenuItem.ShowShortcutKeys = false;
            this.fRenameKiItemMenuItem.Size = new System.Drawing.Size(162, 22);
            this.fRenameKiItemMenuItem.Text = "Re&name";
            this.fRenameKiItemMenuItem.Visible = false;
            this.fRenameKiItemMenuItem.Click += new System.EventHandler(this.IRenameKiItem);
            // 
            // fGridPanel
            // 
            this.fGridPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.fGridPanel.Controls.Add(this.fGridSplitter);
            this.fGridPanel.Controls.Add(this.fMailList);
            this.fGridPanel.Controls.Add(this.fAgeList);
            this.fGridPanel.Location = new System.Drawing.Point(3, 3);
            this.fGridPanel.Name = "fGridPanel";
            this.fGridPanel.Size = new System.Drawing.Size(437, 400);
            this.fGridPanel.TabIndex = 3;
            // 
            // fGridSplitter
            // 
            this.fGridSplitter.Location = new System.Drawing.Point(145, 0);
            this.fGridSplitter.Name = "fGridSplitter";
            this.fGridSplitter.Size = new System.Drawing.Size(3, 400);
            this.fGridSplitter.TabIndex = 4;
            this.fGridSplitter.TabStop = false;
            // 
            // fMailList
            // 
            this.fMailList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.fKiMailColumn});
            this.fMailList.ContextMenuStrip = this.fJournalItemMenu;
            this.fMailList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fMailList.FullRowSelect = true;
            this.fMailList.GridLines = true;
            this.fMailList.LabelEdit = true;
            this.fMailList.Location = new System.Drawing.Point(145, 0);
            this.fMailList.Name = "fMailList";
            this.fMailList.Size = new System.Drawing.Size(292, 400);
            this.fMailList.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.fMailList.TabIndex = 3;
            this.fMailList.UseCompatibleStateImageBehavior = false;
            this.fMailList.View = System.Windows.Forms.View.Details;
            this.fMailList.ItemActivate += new System.EventHandler(this.IKiItemActivated);
            this.fMailList.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.IItemSelected);
            // 
            // fKiMailColumn
            // 
            this.fKiMailColumn.Text = "Journal Contents";
            this.fKiMailColumn.Width = 287;
            // 
            // fAgeList
            // 
            this.fAgeList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.fAgesColumn});
            this.fAgeList.ContextMenuStrip = this.fFolderItemMenu;
            this.fAgeList.Dock = System.Windows.Forms.DockStyle.Left;
            this.fAgeList.FullRowSelect = true;
            this.fAgeList.GridLines = true;
            this.fAgeList.HideSelection = false;
            this.fAgeList.Location = new System.Drawing.Point(0, 0);
            this.fAgeList.Name = "fAgeList";
            this.fAgeList.Size = new System.Drawing.Size(145, 400);
            this.fAgeList.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.fAgeList.TabIndex = 2;
            this.fAgeList.UseCompatibleStateImageBehavior = false;
            this.fAgeList.View = System.Windows.Forms.View.Details;
            this.fAgeList.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.IJournalSelected);
            // 
            // fAgesColumn
            // 
            this.fAgesColumn.Text = "Journal Folders";
            this.fAgesColumn.Width = 138;
            // 
            // fFolderItemMenu
            // 
            this.fFolderItemMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fDeleteFolderMenuItem});
            this.fFolderItemMenu.Name = "fFolderItemMenu";
            this.fFolderItemMenu.Size = new System.Drawing.Size(108, 26);
            // 
            // fDeleteFolderMenuItem
            // 
            this.fDeleteFolderMenuItem.Name = "fDeleteFolderMenuItem";
            this.fDeleteFolderMenuItem.Size = new System.Drawing.Size(107, 22);
            this.fDeleteFolderMenuItem.Text = "&Delete";
            this.fDeleteFolderMenuItem.Visible = false;
            this.fDeleteFolderMenuItem.Click += new System.EventHandler(this.IDeleteFolder);
            // 
            // fShowEmptyFolders
            // 
            this.fShowEmptyFolders.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.fShowEmptyFolders.AutoSize = true;
            this.fShowEmptyFolders.Location = new System.Drawing.Point(4, 410);
            this.fShowEmptyFolders.Name = "fShowEmptyFolders";
            this.fShowEmptyFolders.Size = new System.Drawing.Size(122, 17);
            this.fShowEmptyFolders.TabIndex = 4;
            this.fShowEmptyFolders.Text = "Show Empty Folders";
            this.fShowEmptyFolders.UseVisualStyleBackColor = true;
            this.fShowEmptyFolders.CheckedChanged += new System.EventHandler(this.IShowHiddenToggled);
            // 
            // KiMailControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.fShowEmptyFolders);
            this.Controls.Add(this.fGridPanel);
            this.Name = "KiMailControl";
            this.Size = new System.Drawing.Size(440, 431);
            this.fJournalItemMenu.ResumeLayout(false);
            this.fGridPanel.ResumeLayout(false);
            this.fFolderItemMenu.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ContextMenuStrip fJournalItemMenu;
        private System.Windows.Forms.ToolStripMenuItem fDeleteKiItemMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fRenameKiItemMenuItem;
        private System.Windows.Forms.Panel fGridPanel;
        private System.Windows.Forms.Splitter fGridSplitter;
        private System.Windows.Forms.ListView fMailList;
        private System.Windows.Forms.ColumnHeader fKiMailColumn;
        private System.Windows.Forms.ListView fAgeList;
        private System.Windows.Forms.ColumnHeader fAgesColumn;
        private System.Windows.Forms.CheckBox fShowEmptyFolders;
        private System.Windows.Forms.ContextMenuStrip fFolderItemMenu;
        private System.Windows.Forms.ToolStripMenuItem fDeleteFolderMenuItem;
        private System.Windows.Forms.ToolStripMenuItem createTextNoteToolStripMenuItem;
    }
}
