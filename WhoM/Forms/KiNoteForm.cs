using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace MUd {
    public partial class KiNoteForm : KiMailBaseForm {

        VaultTextNode fTextNode;

        public KiNoteForm(VaultNode node) {
            InitializeComponent();

            fTextNode = new VaultTextNode(node);
            IRevert(null, null);
            Text = fTextNode.NoteName;
        }

        protected override void IExport(object sender, EventArgs e) {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Text File (*.txt)|*.txt";
            if (sfd.ShowDialog(this) == DialogResult.OK) {
                byte[] buf = Encoding.UTF8.GetBytes(fTextBox.Text);

                FileStream fs = new FileStream(sfd.FileName, FileMode.Create, FileAccess.Write);
                fs.Write(buf, 0, buf.Length);
                fs.Close();
            }
        }

        protected override void IReplace(object sender, EventArgs e) {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Text File (*.txt)|*.txt";
            if (ofd.ShowDialog(this) == DialogResult.OK) {
                FileStream fs = new FileStream(ofd.FileName, FileMode.Open, FileAccess.Read);
                BinaryReader r = new BinaryReader(fs);
                byte[] buf = r.ReadBytes((int)fs.Length);
                r.Close();
                fs.Close();

                fTextBox.Text = Encoding.UTF8.GetString(buf);
            }
        }

        protected override void IRevert(object sender, EventArgs e) {
            fTextBox.Text = fTextNode.Text.Replace("\n", "\r\n");

            fRevertButton.Enabled = false;
            fSaveButton.Enabled = false;
        }

        protected override void ISave(object sender, EventArgs e) {
            fTextNode.Text = fTextBox.Text.Replace("\r\n", "\n");
            fParent.AuthCli.SaveVaultNode(Guid.NewGuid(), fTextNode.ID, fTextNode.BaseNode.ToArray());

            fRevertButton.Enabled = false;
            fSaveButton.Enabled = false;
        }

        private void ITextChanged(object sender, EventArgs e) {
            fRevertButton.Enabled = true;
            fSaveButton.Enabled = true;
        }
    }
}
