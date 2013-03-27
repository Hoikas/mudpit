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
            MarkerList,
        }

        private MainForm fParent;
        public new MainForm Parent {
            set { fParent = value; }
        }

        private bool fInitialized = false;
        private uint fBaseNode;
        private List<ListViewItem> fHiddenFolders = new List<ListViewItem>();

        public KiMailControl() {
            InitializeComponent();

            fMailList.SmallImageList = new ImageList();
            fMailList.SmallImageList.Images.Add(Resources.text_x_generic);
            fMailList.SmallImageList.Images.Add(Resources.image_x_generic);
            fMailList.SmallImageList.Images.Add(Resources.start_here);
        }

        private void IAddJournalFolder(VaultNode node) {
            VaultFolderNode age = new VaultFolderNode(node);
            fParent.LogDebug(String.Format("Got journal [AGE: {0}] [NodeID: {1}]", age.FolderName, age.ID));

            ListViewItem folder = new ListViewItem(age.FolderName);
            folder.Tag = node;
            if (fParent.GetChildCount(node.ID) == 0 && !fShowEmptyFolders.Checked)
                fHiddenFolders.Add(folder);
            else
                fAgeList.Items.Add(folder);
        }

        private void IAddKiItem(VaultNode node) {
            if (fAgeList.FocusedItem == null) return;

            //Callbacks can be evil sometimes...
            //Make sure that the incoming node is for the CURRENT age
            VaultNode tag = (VaultNode)fAgeList.FocusedItem.Tag;
            if (!fParent.HasChild(tag.ID, node.ID))
                return;

            ListViewItem lvi = new ListViewItem(IGetTitle(node)); //String64_1 is pretty much always the title...
            lvi.Tag = node;

            switch (node.NodeType) {
                case ENodeType.kNodeImage:
                    lvi.ImageIndex = (int)Icon.Image;
                    break;
                case ENodeType.kNodeMarkerList:
                    lvi.ImageIndex = (int)Icon.MarkerList;
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

        private void IAddUserNode(ENetError result, uint nodeID, uint parentID) {
            if (result != ENetError.kNetSuccess) {
                fParent.LogError("Failed to create a user node, so not adding it.");
                MessageBox.Show("Failed to create KI Mail\r\n" + result.ToString().Substring(4), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Ref the node. We'll add it to the list in a callback
            fParent.AuthCli.AddVaultNode(parentID, nodeID, fParent.fActivePlayer);
        }

        public void Clear() {
            fAgeList.Items.Clear();
            fMailList.Items.Clear();
            fInitialized = false;
        }

        private void ICreateTextNote(object sender, EventArgs e) {
            if (fAgeList.FocusedItem == null) return;

            VaultTextNode text = new VaultTextNode();
            text.NoteName = "WhoM Text Note";
            text.NodeType = ENoteType.kNoteGeneric;
            text.Text = "Hello, World!";

            // Save the node, then add it to the age journals on a callback
            uint transID = fParent.AuthCli.CreateVaultNode(text.BaseNode.ToArray());
            fParent.RegisterAuthCB(transID, new Action<ENetError, uint, uint>(IAddUserNode), new object[] { ((VaultNode)fAgeList.FocusedItem.Tag).ID });
        }

        private void IDeleteFolder(object sender, EventArgs e) {
            if (fAgeList.FocusedItem == null) return;
            VaultFolderNode folder = new VaultFolderNode((VaultNode)fAgeList.FocusedItem.Tag);

            // If this folder has children, we should prompt the user about the deletion...
            if (fParent.GetChildCount(folder.ID) > 0) {
                DialogResult dr = MessageBox.Show(String.Format("Are you sure you want to delete \"{0}\"?\r\nAny items left in this folder will be orphaned.",
                    folder.FolderName), "Delete Age Folder", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dr == DialogResult.No)
                    return;
            }

            // Callback from Auth will remove it from the list
            fParent.AuthCli.RemoveVaultNode(fBaseNode, folder.ID);
        }

        private void IDeleteKiItem(object sender, EventArgs e) {
            if (fMailList.FocusedItem == null) return;
            VaultNode cTag = (VaultNode)fMailList.FocusedItem.Tag;
            VaultNode pTag = (VaultNode)fAgeList.FocusedItem.Tag;

            DialogResult dr = MessageBox.Show(String.Format("Are you sure you want to delete \"{0}\"?", IGetTitle(cTag)), "Delete KI Mail", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dr == DialogResult.Yes)
                fParent.AuthCli.RemoveVaultNode(pTag.ID, cTag.ID);
        }

        private string IGetTitle(VaultNode node) {
            switch (node.NodeType) {
                case ENodeType.kNodeImage:
                case ENodeType.kNodeTextNote:
                    return node.fString64[0];
                case ENodeType.kNodeMarkerList:
                    return node.fText[0];
            }

            return String.Empty;
        }

        private void IItemSelected(object sender, ListViewItemSelectionChangedEventArgs e) {
            fDeleteKiItemMenuItem.Visible = e.IsSelected;
            fRenameKiItemMenuItem.Visible = e.IsSelected;
        }

        private void IJournalSelected(object sender, ListViewItemSelectionChangedEventArgs e) {
            if (!e.IsSelected) {
                fDeleteFolderMenuItem.Visible = false;
                return;
            }
            fMailList.Items.Clear();

            VaultNode node = (VaultNode)e.Item.Tag;
            fParent.FetchChildren(node.ID, new Action<VaultNode>(IAddKiItem));

            // No context menus for the KI Inbox magic folder
            VaultFolderNode folder = new VaultFolderNode(node);
            fDeleteFolderMenuItem.Visible = (folder.FolderType != EStandardNode.kInboxFolder);
        }

        private void IKiItemActivated(object sender, EventArgs e) {
            VaultNode node = (VaultNode)fMailList.FocusedItem.Tag;

            KiMailBaseForm kiform = null;
            if (node.NodeType == ENodeType.kNodeImage)
                kiform = new KiImageForm(node);
            else if (node.NodeType == ENodeType.kNodeTextNote)
                kiform = new KiNoteForm(node);

            //In case we get something weird...
            if (kiform != null) {
                kiform.Parent = fParent;
                kiform.Show(fParent);
            }
        }

        private void IKiItemRenamed(object sender, LabelEditEventArgs e) {
            VaultNode node = (VaultNode)fMailList.Items[e.Item].Tag;
            ISetTitle(node, e.Label);

            //Cancel the edit...
            //Why? Let the rename be done by the server
            //Slight delay, but it ensures correctness (Cyan's Server is Weird...)
            e.CancelEdit = true;

            //Send the updated node
            fParent.AuthCli.SaveVaultNode(Guid.NewGuid(), node.ID, node.ToArray());
        }

        public void RemoveFolder(VaultNode node) {
            foreach (ListViewItem lvi in fAgeList.Items) {
                VaultNode tag = (VaultNode)lvi.Tag;
                if (tag.ID == node.ID) {
                    lvi.Remove();
                    break;
                }
            }
        }

        public void RemoveKiItem(VaultNode node) {
            foreach (ListViewItem lvi in fMailList.Items) {
                VaultNode tag = (VaultNode)lvi.Tag;
                if (tag.ID == node.ID) {
                    lvi.Remove();
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

        private void IShowHiddenToggled(object sender, EventArgs e) {
            fAgeList.BeginUpdate();
            if (fShowEmptyFolders.Checked) {
                foreach (ListViewItem lvi in fHiddenFolders)
                    fAgeList.Items.Add(lvi);
                fHiddenFolders.Clear();
            } else {
                foreach (ListViewItem lvi in fAgeList.Items) {
                    VaultNode node = (VaultNode)lvi.Tag;
                    if (fParent.GetChildCount(node.ID) == 0) {
                        fHiddenFolders.Add(lvi);
                        lvi.Remove();
                    }
                }
            }
            fAgeList.EndUpdate();
        }

        private void ISetTitle(VaultNode node, string text) {
            switch (node.NodeType) {
                case ENodeType.kNodeImage:
                case ENodeType.kNodeTextNote:
                    node.fString64[0] = text;
                    break;
                case ENodeType.kNodeMarkerList:
                    node.fText[0] = text;
                    break;
            }
        }

        public void UpdateNode(VaultNode node) {
            for (int i = 0; i < fMailList.Items.Count; i++) {
                VaultNode tag = (VaultNode)fMailList.Items[i].Tag;
                if (tag.ID == node.ID) {
                    fMailList.Items[i].Tag = node;
                    fMailList.Items[i].Text = IGetTitle(node);

                    //We must resort manually...
                    fMailList.Sort();
                    break;
                }
            }
        }
    }
}
