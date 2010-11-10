﻿using System;
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

        public KiMailControl() {
            InitializeComponent();

            fMailList.SmallImageList = new ImageList();
            fMailList.SmallImageList.Images.Add(Resources.text_x_generic);
            fMailList.SmallImageList.Images.Add(Resources.image_x_generic);
            fMailList.SmallImageList.Images.Add(Resources.start_here);
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

        public void Clear() {
            fAgeList.Items.Clear();
            fMailList.Items.Clear();
            fInitialized = false;
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
