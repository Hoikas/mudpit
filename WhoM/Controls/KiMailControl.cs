using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MUd.Properties;

namespace MUd {
    public partial class KiMailControl : UserControl {

        enum Icon {
            Text,
            Image,
        }

        private MainForm fParent;
        public new MainForm Parent {
            set { fParent = value; }
        }

        private bool fInitialized = false;
        private uint fBaseNode;

        public KiMailControl() {
            InitializeComponent();

            fMailList.SmallImageList = new ImageList();
            fMailList.SmallImageList.Images.Add(Resources.text_x_generic);
            fMailList.SmallImageList.Images.Add(Resources.image_x_generic);
        }

        private void IAddJournalFolder(VaultNode node) {
            VaultFolderNode age = new VaultFolderNode(node);
            if (fParent.GetChildCount(node.ID) == 0) {
                fParent.LogDebug(String.Format("Ignoring empty journal... [AGE: {0}] [NodeID: {1}]", age.FolderName, age.ID));
                return;
            }

            fParent.LogDebug(String.Format("Got journal [AGE: {0}] [NodeID: {1}]", age.FolderName, age.ID));

            ListViewItem lvi = new ListViewItem(age.FolderName);
            lvi.Tag = node;
            fAgeList.Items.Add(lvi);
        }

        private void IAddKiItem(VaultNode node) {
            //Callbacks can be evil sometimes...
            //Make sure that the incoming node is for the CURRENT age
            VaultNode tag = (VaultNode)fAgeList.FocusedItem.Tag;
            if (!fParent.HasChild(tag.ID, node.ID))
                return;

            ListViewItem lvi = new ListViewItem(node.fString64[0]); //String64_1 is pretty much always the title...
            lvi.Tag = node;

            switch (node.NodeType) {
                case ENodeType.kNodeImage:
                    lvi.ImageIndex = (int)Icon.Image;
                    break;
                case ENodeType.kNodeTextNote:
                    lvi.ImageIndex = (int)Icon.Text;
                    break;
                default:
                    //Unhandled Type... Bail out!
                    return;
            }

            fMailList.Items.Add(lvi);
        }

        public void AddNode(VaultNode node, uint parentID) {
            if (fAgeList.FocusedItem == null) return;

            VaultNode tag = (VaultNode)fAgeList.FocusedItem.Tag;
            if (tag.ID == parentID)
                IAddKiItem(node);
        }

        private void IDeleteKiItem(object sender, EventArgs e) {
            if (fMailList.FocusedItem == null) return;

            VaultNode cTag = (VaultNode)fMailList.FocusedItem.Tag;
            VaultNode pTag = (VaultNode)fAgeList.FocusedItem.Tag;

            fParent.AuthCli.RemoveVaultNode(pTag.ID, cTag.ID);
        }

        private void IJournalSelected(object sender, ListViewItemSelectionChangedEventArgs e) {
            if (!e.IsSelected) return;
            fMailList.Items.Clear();

            VaultNode folder = (VaultNode)e.Item.Tag;
            fParent.FetchChildren(folder.ID, new Action<VaultNode>(IAddKiItem));
        }

        private void IKiItemActivated(object sender, EventArgs e) {
            VaultNode node = (VaultNode)fMailList.FocusedItem.Tag;

            KiMailBaseForm kiform = null;
            if (node.NodeType == ENodeType.kNodeImage)
                kiform = new KiImageForm(node);
            else if (node.NodeType == ENodeType.kNodeTextNote)
                kiform = new KiNoteForm(node);
            kiform.Parent = fParent;
            kiform.Show(fParent);
        }

        private void IKiItemRenamed(object sender, LabelEditEventArgs e) {
            VaultNode node = (VaultNode)fMailList.Items[e.Item].Tag;
            node.fString64[0] = e.Label;

            //Cancel the edit...
            //Why? Let the rename be done by the server
            //Slight delay, but it insures correctness (Cyan's Server is Weird...)
            e.CancelEdit = true;

            //Send the updated node
            fParent.AuthCli.SaveVaultNode(Guid.NewGuid(), node.ID, node.ToArray());
        }

        public void RemoveNode(VaultNode node) {
            for (int i = 0; i < fMailList.Items.Count; i++) {
                VaultNode tag = (VaultNode)fMailList.Items[i].Tag;
                if (tag.ID == node.ID) {
                    fMailList.Items.RemoveAt(i);
                    break;
                }
            }
        }

        private void IRenameKiItem(object sender, EventArgs e) {
            if (fMailList.FocusedItem == null) return;
            fMailList.FocusedItem.BeginEdit();
        }

        public void SetFolder(uint journals, uint inbox) {
            if (fInitialized) return;
            fParent.LogInfo(String.Format("Initializing KI Mail [NodeID: {0}]", journals));

            fInitialized = true;
            fBaseNode = journals;
            fParent.FetchChildren(journals, new Action<VaultNode>(IAddJournalFolder));
            
            //Create a "fake" folder for the inbox
            VaultFolderNode folder = new VaultFolderNode();
            folder.FolderName = "Inbox";
            folder.FolderType = EStandardNode.kInboxFolder;
            folder.ID = inbox;

            //Pass it off
            IAddJournalFolder(folder.BaseNode);
        }

        public void UpdateNode(VaultNode node) {
            for (int i = 0; i < fMailList.Items.Count; i++) {
                VaultNode tag = (VaultNode)fMailList.Items[i].Tag;
                if (tag.ID == node.ID) {
                    fMailList.Items[i].Tag = node;
                    fMailList.Items[i].Text = node.fString64[0];

                    //We must resort manually...
                    fMailList.Sort();
                    break;
                }
            }
        }
    }
}
