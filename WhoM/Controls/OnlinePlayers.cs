using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace MUd {
    public partial class OnlinePlayers : UserControl {

        private MainForm fParent;
        public new MainForm Parent {
            set { fParent = value; }
        }

        public bool CanRemove {
            set { fRemoveMenuItem.Enabled = value; }
        }

        private static readonly string[] kAgesToName = new string[] { "Bevin", "Neighborhood" };

        private Dictionary<uint, uint> fPlayerToInfo = new Dictionary<uint, uint>();
        private bool fInitialized = false;
        private uint fBaseNode;

        public OnlinePlayers() {
            InitializeComponent();
            fDataGridView.SortCompare += new DataGridViewSortCompareEventHandler(IOnGridSort);
        }

        public void AddPlayerInfo(VaultNode node) {
            VaultPlayerInfoNode info = new VaultPlayerInfoNode(node);
            if (fPlayerToInfo.ContainsKey(info.PlayerID)) return;

            Bitmap img = IGetImage(info);
            string age = (info.Online ? IGetAgeName(info) : String.Empty);
            fDataGridView.Rows.Add(new object[] { img, info.PlayerID, info.PlayerName, age });
            fPlayerToInfo.Add(info.PlayerID, info.BaseNode.ID);

            //Sort... If no default, use online status
            if (fDataGridView.SortedColumn != null)
                fDataGridView.Sort(fDataGridView.SortedColumn, (fDataGridView.SortOrder == SortOrder.Ascending ? ListSortDirection.Ascending : ListSortDirection.Descending));
            else
                fDataGridView.Sort(fOnline, ListSortDirection.Descending);
        }

        public void Clear() {
            fPlayerToInfo.Clear();
            fDataGridView.Rows.Clear();
            fInitialized = false;
        }

        private void IDelayedAgeName(uint[] matches, VaultPlayerInfoNode info) {
            if (matches.Length == 1) {
                uint trans = fParent.AuthCli.FetchVaultNode(matches[0]);
                fParent.RegisterAuthCB(trans, new Action<VaultNode, VaultPlayerInfoNode>(IDelayedAgeName), new object[] { info });
            } else
                fParent.LogWarn(String.Format("Got {0} AgeInfos!", matches.Length)); 
        }

        private void IDelayedAgeName(VaultNode node, VaultPlayerInfoNode info) {
            VaultAgeInfoNode age = new VaultAgeInfoNode(node);

            string name = String.Empty;
            if (age.Description == String.Empty)
                if (age.SequenceNumber == 0)
                    name = String.Format("{0} {1}", age.UserDefinedName, age.InstanceName);
                else
                    name = String.Format("{0} ({1}) {2}", age.UserDefinedName, age.SequenceNumber, age.InstanceName);
            else
                name = age.Description;
                

            //Find the player's row
            int idx = -1;
            foreach (DataGridViewRow r in fDataGridView.Rows) {
                if (Convert.ToUInt32(r.Cells[1].Value) == info.PlayerID) {
                    idx = fDataGridView.Rows.IndexOf(r);
                    break;
                }
            }

            //Actually update the entry
            fDataGridView.Rows[idx].Cells[3].Value = name;
        }

        private string IGetAgeName(VaultPlayerInfoNode info) {
            if (kAgesToName.Contains(info.AgeInstanceName)) {
                VaultAgeInfoNode search = new VaultAgeInfoNode();
                search.InstanceName = info.AgeInstanceName;
                search.InstanceUUID = info.AgeInstanceUUID;
                
                //Do the search
                uint trans = fParent.AuthCli.FindVaultNode(search.BaseNode.ToArray());
                fParent.RegisterAuthCB(trans, new Action<uint[], VaultPlayerInfoNode>(IDelayedAgeName), new object[] { info });
            }

            if (info.AgeInstanceName == "AhnonayCathedral")
                return "Ahnonay Cathedral";
            else if (info.AgeInstanceName == "AvatarCustomization")
                return "Closet";
            else if (info.AgeInstanceName == "Bevin")
                return "Neighborhood";
            else if (info.AgeInstanceName == "city")
                return "Ae'gura";
            else if (info.AgeInstanceName == "Descent")
                return "Great Shaft";
            else if (info.AgeInstanceName == "EderDelin")
                return "Eder Delin";
            else if (info.AgeInstanceName == "EderTsogal")
                return "Eder Tsogal";
            else if (info.AgeInstanceName == "Ercana")
                return "Er'cana";
            else if (info.AgeInstanceName == "ErcanaCitySilo")
                return "Uran Silo";
            else if (info.AgeInstanceName == "GreatZero")
                return "Great Zero";
            else if (info.AgeInstanceName == "GuildPub-Cartographers")
                return "Cartographers' Pub";
            else if (info.AgeInstanceName == "GuildPub-Greeters")
                return "Greeters' Pub";
            else if (info.AgeInstanceName == "GuildPub-Maintainers")
                return "Maintainers' Pub";
            else if (info.AgeInstanceName == "GuildPub-Messengers")
                return "Messengers' Pub";
            else if (info.AgeInstanceName == "GuildPub-Writers")
                return "Writers' Pub";
            else if (info.AgeInstanceName == "Hood")
                return "Neighborhood";
            else if (info.AgeInstanceName == "Kveer")
                return "K'veer";
            else if (info.AgeInstanceName == "PelletBahroCave")
                return "Pellet Cave";
            else if (info.AgeInstanceName == "Personal")
                return "Relto";
            else if (info.AgeInstanceName == "philRelto")
                return "Phil's Relto";
            else if (info.AgeInstanceName == "spyroom")
                return "Sharper's Spy Room";
            else if (info.AgeInstanceName == String.Empty)
                return "*** Linking ***";

            return info.AgeInstanceName;
        }

        public bool HasPlayer(uint id) {
            return fPlayerToInfo.ContainsKey(id);
        }

        public void SetFolder(uint folder) {
            if (fInitialized) return;

            fInitialized = true;
            fBaseNode = folder;

            fParent.FetchChildren(folder, new Action<VaultNode>(AddPlayerInfo));
        }
        
        private void IOnGridSort(object sender, DataGridViewSortCompareEventArgs e) {
            if (e.Column.Index == 0) {
                fOnline.HeaderCell.SortGlyphDirection = fDataGridView.SortOrder;

                Bitmap c1 = (Bitmap)e.CellValue1;
                Bitmap c2 = (Bitmap)e.CellValue2;

                e.SortResult = String.Compare(c1.Tag.ToString(), c2.Tag.ToString());
                e.Handled = true;
            } else if (e.Column.Index == 3) {
                string c1 = e.CellValue1.ToString();
                string c2 = e.CellValue2.ToString();

                //Player names (secondary sorting)
                string p1 = fDataGridView.Rows[e.RowIndex1].Cells[2].Value.ToString();
                string p2 = fDataGridView.Rows[e.RowIndex2].Cells[2].Value.ToString();

                //Keep the logged out players at the end of the list
                bool need_reverse = (fDataGridView.SortOrder == SortOrder.Descending);
                if (c1.Equals(c2))
                    e.SortResult = String.Compare(p1, p2);
                else if (c1 == String.Empty && c2 != String.Empty)
                    e.SortResult = (need_reverse) ? -1 : 1;
                else if (c1 != String.Empty && c2 == String.Empty)
                    e.SortResult = (need_reverse) ? 1 : -1;
                else
                    e.SortResult = String.Compare(c1, c2);

                e.Handled = true;
            }
        }

        public bool UpdateNode(VaultPlayerInfoNode info) {
            int idx = -1;
            foreach (DataGridViewRow r in fDataGridView.Rows) {
                if (Convert.ToUInt32(r.Cells[1].Value) == info.PlayerID) {
                    idx = fDataGridView.Rows.IndexOf(r);
                    break;
                }
            }

            if (idx == -1) return false;

            //Has the player logged in?
            bool retval = ((fDataGridView.Rows[idx].Cells[0].Value as Bitmap).Tag.ToString() == "offline" && info.Online);

            //Change values
            fDataGridView.Rows[idx].Cells[0].Value = IGetImage(info);
            fDataGridView.Rows[idx].Cells[3].Value = (info.Online ? IGetAgeName(info) : String.Empty);

            //Update and stuff
            fDataGridView.UpdateCellValue(0, idx);
            fDataGridView.UpdateCellValue(3, idx);
            
            //Sort... If no default, use online status
            if (fDataGridView.SortedColumn != null)
                fDataGridView.Sort(fDataGridView.SortedColumn, (fDataGridView.SortOrder == SortOrder.Ascending ? ListSortDirection.Ascending : ListSortDirection.Descending));
            else
                fDataGridView.Sort(fOnline, ListSortDirection.Descending);

            return retval;
        }

        public void RemoveNode(VaultNode node) {
            VaultPlayerInfoNode info = new VaultPlayerInfoNode(node);
            if (fPlayerToInfo.ContainsKey(info.PlayerID))
                fPlayerToInfo.Remove(info.PlayerID);
            else
                return;

            for (int i = 0; i < fDataGridView.Rows.Count; i++) {
                if (Convert.ToUInt32(fDataGridView.Rows[i].Cells[1].Value) == info.PlayerID) {
                    fDataGridView.Rows.RemoveAt(i);
                    break;
                }
            }
        }

        private void ICellMouseClick(object sender, DataGridViewCellMouseEventArgs e) {
            if (e.RowIndex == -1) return;
            if (fDataGridView.Rows[e.RowIndex].Selected) {
                if (e.Button == MouseButtons.Left) fDataGridView.Rows[e.RowIndex].Selected = false;
            } else {
                if (e.Button == MouseButtons.Right) {
                    if (fDataGridView.SelectedRows.Count == 0)
                        fDataGridView.Rows[e.RowIndex].Selected = true;
                    else if ((Control.ModifierKeys & Keys.Shift) != 0 || (Control.ModifierKeys & Keys.Control) != 0)
                        fDataGridView.Rows[e.RowIndex].Selected = !fDataGridView.Rows[e.RowIndex].Selected;
                    else {
                        fDataGridView.ClearSelection();
                        fDataGridView.Rows[e.RowIndex].Selected = true;
                    }
                }
            }
        }

        private void ICopy(object sender, EventArgs e) {
            string copy = String.Empty;
            foreach (DataGridViewRow row in fDataGridView.SelectedRows) {
                if (copy != String.Empty) copy += "\r\n";
                copy += String.Format("{0} [KI: {1}]", row.Cells[2].Value, row.Cells[1].Value);
            }

            if (copy != String.Empty)
                Clipboard.SetText(copy, TextDataFormat.UnicodeText);
        }

        private Bitmap IGetImage(VaultPlayerInfoNode info) {
            if (info.Online) {
                Bitmap b = MUd.Properties.Resources.bullet_green;
                b.Tag = "online";
                return b;
            } else {
                Bitmap b = MUd.Properties.Resources.bullet_red;
                b.Tag = "offline";
                return b;
            }
        }

        private void IRemovePlayer(object sender, EventArgs e) {
            foreach (DataGridViewRow row in fDataGridView.SelectedRows) {
                uint playerID = Convert.ToUInt32(row.Cells[1].Value);
                uint info = fPlayerToInfo[playerID];

                fParent.LogInfo(String.Format("Removed node [ID: {0}]", info));
                fParent.AuthCli.RemoveVaultNode(fBaseNode, info);
            }
        }
    }
}
