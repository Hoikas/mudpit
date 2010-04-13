namespace MUd {
    partial class OnlinePlayers {
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
            this.fDataGridView = new System.Windows.Forms.DataGridView();
            this.fContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.fCopyMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fRemoveMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fOnline = new System.Windows.Forms.DataGridViewImageColumn();
            this.fKI = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.fPlayerName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.fCurrentAge = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.fDataGridView)).BeginInit();
            this.fContextMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // fDataGridView
            // 
            this.fDataGridView.AllowUserToAddRows = false;
            this.fDataGridView.AllowUserToDeleteRows = false;
            this.fDataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.fDataGridView.BackgroundColor = System.Drawing.SystemColors.Control;
            this.fDataGridView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.fDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.fDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.fOnline,
            this.fKI,
            this.fPlayerName,
            this.fCurrentAge});
            this.fDataGridView.ContextMenuStrip = this.fContextMenu;
            this.fDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fDataGridView.GridColor = System.Drawing.SystemColors.ControlLight;
            this.fDataGridView.Location = new System.Drawing.Point(0, 0);
            this.fDataGridView.Name = "fDataGridView";
            this.fDataGridView.ReadOnly = true;
            this.fDataGridView.RowHeadersVisible = false;
            this.fDataGridView.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
            this.fDataGridView.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.fDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.fDataGridView.ShowEditingIcon = false;
            this.fDataGridView.Size = new System.Drawing.Size(471, 332);
            this.fDataGridView.TabIndex = 0;
            this.fDataGridView.CellMouseDown += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.ICellMouseClick);
            // 
            // fContextMenu
            // 
            this.fContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fCopyMenuItem,
            this.fRemoveMenuItem});
            this.fContextMenu.Name = "fContextMenu";
            this.fContextMenu.Size = new System.Drawing.Size(111, 48);
            // 
            // fCopyMenuItem
            // 
            this.fCopyMenuItem.Name = "fCopyMenuItem";
            this.fCopyMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            this.fCopyMenuItem.ShowShortcutKeys = false;
            this.fCopyMenuItem.Size = new System.Drawing.Size(110, 22);
            this.fCopyMenuItem.Text = "Copy";
            this.fCopyMenuItem.Click += new System.EventHandler(this.ICopy);
            // 
            // fRemoveMenuItem
            // 
            this.fRemoveMenuItem.Name = "fRemoveMenuItem";
            this.fRemoveMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.R)));
            this.fRemoveMenuItem.ShowShortcutKeys = false;
            this.fRemoveMenuItem.Size = new System.Drawing.Size(110, 22);
            this.fRemoveMenuItem.Text = "&Remove";
            this.fRemoveMenuItem.Click += new System.EventHandler(this.IRemovePlayer);
            // 
            // fOnline
            // 
            this.fOnline.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.fOnline.HeaderText = "Online";
            this.fOnline.Name = "fOnline";
            this.fOnline.ReadOnly = true;
            this.fOnline.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.fOnline.Width = 62;
            // 
            // fKI
            // 
            this.fKI.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.fKI.HeaderText = "KI";
            this.fKI.Name = "fKI";
            this.fKI.ReadOnly = true;
            this.fKI.Width = 42;
            // 
            // fPlayerName
            // 
            this.fPlayerName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.fPlayerName.HeaderText = "Player Name";
            this.fPlayerName.Name = "fPlayerName";
            this.fPlayerName.ReadOnly = true;
            // 
            // fCurrentAge
            // 
            this.fCurrentAge.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.fCurrentAge.HeaderText = "Current Age";
            this.fCurrentAge.Name = "fCurrentAge";
            this.fCurrentAge.ReadOnly = true;
            // 
            // OnlinePlayers
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.fDataGridView);
            this.Name = "OnlinePlayers";
            this.Size = new System.Drawing.Size(471, 332);
            ((System.ComponentModel.ISupportInitialize)(this.fDataGridView)).EndInit();
            this.fContextMenu.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView fDataGridView;
        private System.Windows.Forms.ContextMenuStrip fContextMenu;
        private System.Windows.Forms.ToolStripMenuItem fRemoveMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fCopyMenuItem;
        private System.Windows.Forms.DataGridViewImageColumn fOnline;
        private System.Windows.Forms.DataGridViewTextBoxColumn fKI;
        private System.Windows.Forms.DataGridViewTextBoxColumn fPlayerName;
        private System.Windows.Forms.DataGridViewTextBoxColumn fCurrentAge;

    }
}
