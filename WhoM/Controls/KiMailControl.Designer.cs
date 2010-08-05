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
            this.fAgeList = new System.Windows.Forms.ListView();
            this.fAgesColumn = new System.Windows.Forms.ColumnHeader();
            this.fMailList = new System.Windows.Forms.ListView();
            this.fKiMailColumn = new System.Windows.Forms.ColumnHeader();
            this.fJournalItemMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.fDeleteMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fRenameMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fSplitter = new System.Windows.Forms.Splitter();
            this.fJournalItemMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // fAgeList
            // 
            this.fAgeList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.fAgesColumn});
            this.fAgeList.Dock = System.Windows.Forms.DockStyle.Left;
            this.fAgeList.FullRowSelect = true;
            this.fAgeList.GridLines = true;
            this.fAgeList.HideSelection = false;
            this.fAgeList.Location = new System.Drawing.Point(0, 0);
            this.fAgeList.Name = "fAgeList";
            this.fAgeList.Size = new System.Drawing.Size(175, 431);
            this.fAgeList.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.fAgeList.TabIndex = 0;
            this.fAgeList.UseCompatibleStateImageBehavior = false;
            this.fAgeList.View = System.Windows.Forms.View.Details;
            this.fAgeList.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.IJournalSelected);
            // 
            // fAgesColumn
            // 
            this.fAgesColumn.Text = "Journal Folders";
            this.fAgesColumn.Width = 169;
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
            this.fMailList.Location = new System.Drawing.Point(175, 0);
            this.fMailList.Name = "fMailList";
            this.fMailList.Size = new System.Drawing.Size(265, 431);
            this.fMailList.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.fMailList.TabIndex = 1;
            this.fMailList.UseCompatibleStateImageBehavior = false;
            this.fMailList.View = System.Windows.Forms.View.Details;
            this.fMailList.ItemActivate += new System.EventHandler(this.IKiItemActivated);
            this.fMailList.AfterLabelEdit += new System.Windows.Forms.LabelEditEventHandler(this.IKiItemRenamed);
            // 
            // fKiMailColumn
            // 
            this.fKiMailColumn.Text = "Journal Contents";
            this.fKiMailColumn.Width = 248;
            // 
            // fJournalItemMenu
            // 
            this.fJournalItemMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fDeleteMenuItem,
            this.fRenameMenuItem});
            this.fJournalItemMenu.Name = "fJournalItemMenu";
            this.fJournalItemMenu.Size = new System.Drawing.Size(111, 48);
            // 
            // fDeleteMenuItem
            // 
            this.fDeleteMenuItem.Name = "fDeleteMenuItem";
            this.fDeleteMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Delete;
            this.fDeleteMenuItem.ShowShortcutKeys = false;
            this.fDeleteMenuItem.Size = new System.Drawing.Size(110, 22);
            this.fDeleteMenuItem.Text = "&Delete";
            this.fDeleteMenuItem.Click += new System.EventHandler(this.IDeleteKiItem);
            // 
            // fRenameMenuItem
            // 
            this.fRenameMenuItem.Name = "fRenameMenuItem";
            this.fRenameMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.N)));
            this.fRenameMenuItem.ShowShortcutKeys = false;
            this.fRenameMenuItem.Size = new System.Drawing.Size(110, 22);
            this.fRenameMenuItem.Text = "Re&name";
            this.fRenameMenuItem.Click += new System.EventHandler(this.IRenameKiItem);
            // 
            // fSplitter
            // 
            this.fSplitter.Location = new System.Drawing.Point(175, 0);
            this.fSplitter.Name = "fSplitter";
            this.fSplitter.Size = new System.Drawing.Size(3, 431);
            this.fSplitter.TabIndex = 2;
            this.fSplitter.TabStop = false;
            // 
            // KiMailControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.fSplitter);
            this.Controls.Add(this.fMailList);
            this.Controls.Add(this.fAgeList);
            this.Name = "KiMailControl";
            this.Size = new System.Drawing.Size(440, 431);
            this.fJournalItemMenu.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView fAgeList;
        private System.Windows.Forms.ListView fMailList;
        private System.Windows.Forms.ColumnHeader fAgesColumn;
        private System.Windows.Forms.ColumnHeader fKiMailColumn;
        private System.Windows.Forms.ContextMenuStrip fJournalItemMenu;
        private System.Windows.Forms.ToolStripMenuItem fDeleteMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fRenameMenuItem;
        private System.Windows.Forms.Splitter fSplitter;
    }
}
