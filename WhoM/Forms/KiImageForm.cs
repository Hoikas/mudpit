using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace MUd {
    public partial class KiImageForm : KiMailBaseForm {

        VaultImageNode fImage;

        public KiImageForm(VaultNode node) {
            InitializeComponent();

            //Maybe I can be convinced to add support for this...
            //Later...
            fReplaceButton.Enabled = false;
            fRevertButton.Enabled = false;
            fSaveButton.Enabled = false;

            fImage = new VaultImageNode(node);
            if (fImage.ImageType != VaultImageNode.ImgType.kNone) {
                MemoryStream ms = new MemoryStream(fImage.ImageData, 4, fImage.ImageData.Length - 4);
                fPictureBox.Image = Bitmap.FromStream(ms);
            }

            Text = fImage.ImageName;
        }

        protected override void IExport(object sender, EventArgs e) {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.FileName = fImage.ImageName;
            sfd.Filter = "JPEG Image (*.jpg)|*.jpg";
            if (sfd.ShowDialog(this) == DialogResult.OK)
                fPictureBox.Image.Save(sfd.FileName, ImageFormat.Jpeg);
        }
    }
}
