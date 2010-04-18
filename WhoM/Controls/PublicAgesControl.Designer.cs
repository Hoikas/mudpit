namespace MUd {
    partial class PublicAgesControl {
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PublicAgesControl));
            this.fDataGridView = new System.Windows.Forms.DataGridView();
            this.fAgeInstance = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.fAgeDescription = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.fCurrPopulation = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.fViewDetail = new System.Windows.Forms.DataGridViewLinkColumn();
            this.fRefreshImage = new System.Windows.Forms.PictureBox();
            this.fRefreshLink = new System.Windows.Forms.LinkLabel();
            ((System.ComponentModel.ISupportInitialize)(this.fDataGridView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.fRefreshImage)).BeginInit();
            this.SuspendLayout();
            // 
            // fDataGridView
            // 
            this.fDataGridView.AllowUserToAddRows = false;
            this.fDataGridView.AllowUserToDeleteRows = false;
            this.fDataGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.fDataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.fDataGridView.BackgroundColor = System.Drawing.SystemColors.Control;
            this.fDataGridView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.fDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.fDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.fAgeInstance,
            this.fAgeDescription,
            this.fCurrPopulation,
            this.fViewDetail});
            this.fDataGridView.Location = new System.Drawing.Point(0, 0);
            this.fDataGridView.Name = "fDataGridView";
            this.fDataGridView.ReadOnly = true;
            this.fDataGridView.RowHeadersVisible = false;
            this.fDataGridView.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
            this.fDataGridView.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.fDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.fDataGridView.ShowEditingIcon = false;
            this.fDataGridView.Size = new System.Drawing.Size(307, 209);
            this.fDataGridView.TabIndex = 0;
            // 
            // fAgeInstance
            // 
            this.fAgeInstance.HeaderText = "Age Name";
            this.fAgeInstance.Name = "fAgeInstance";
            this.fAgeInstance.ReadOnly = true;
            this.fAgeInstance.Width = 82;
            // 
            // fAgeDescription
            // 
            this.fAgeDescription.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.fAgeDescription.HeaderText = "Description";
            this.fAgeDescription.Name = "fAgeDescription";
            this.fAgeDescription.ReadOnly = true;
            // 
            // fCurrPopulation
            // 
            this.fCurrPopulation.HeaderText = "Population";
            this.fCurrPopulation.Name = "fCurrPopulation";
            this.fCurrPopulation.ReadOnly = true;
            this.fCurrPopulation.Width = 82;
            // 
            // fViewDetail
            // 
            this.fViewDetail.HeaderText = "Detail";
            this.fViewDetail.Name = "fViewDetail";
            this.fViewDetail.ReadOnly = true;
            this.fViewDetail.Text = "";
            this.fViewDetail.Width = 40;
            // 
            // fRefreshImage
            // 
            this.fRefreshImage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.fRefreshImage.Image = ((System.Drawing.Image)(resources.GetObject("fRefreshImage.Image")));
            this.fRefreshImage.Location = new System.Drawing.Point(3, 215);
            this.fRefreshImage.Name = "fRefreshImage";
            this.fRefreshImage.Size = new System.Drawing.Size(16, 16);
            this.fRefreshImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.fRefreshImage.TabIndex = 1;
            this.fRefreshImage.TabStop = false;
            // 
            // fRefreshLink
            // 
            this.fRefreshLink.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.fRefreshLink.AutoSize = true;
            this.fRefreshLink.Location = new System.Drawing.Point(26, 217);
            this.fRefreshLink.Name = "fRefreshLink";
            this.fRefreshLink.Size = new System.Drawing.Size(63, 13);
            this.fRefreshLink.TabIndex = 2;
            this.fRefreshLink.TabStop = true;
            this.fRefreshLink.Text = "Refresh List";
            // 
            // PublicAgesControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.fRefreshLink);
            this.Controls.Add(this.fRefreshImage);
            this.Controls.Add(this.fDataGridView);
            this.Name = "PublicAgesControl";
            this.Size = new System.Drawing.Size(307, 234);
            ((System.ComponentModel.ISupportInitialize)(this.fDataGridView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.fRefreshImage)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView fDataGridView;
        private System.Windows.Forms.DataGridViewTextBoxColumn fAgeInstance;
        private System.Windows.Forms.DataGridViewTextBoxColumn fAgeDescription;
        private System.Windows.Forms.DataGridViewTextBoxColumn fCurrPopulation;
        private System.Windows.Forms.DataGridViewLinkColumn fViewDetail;
        private System.Windows.Forms.PictureBox fRefreshImage;
        private System.Windows.Forms.LinkLabel fRefreshLink;
    }
}
