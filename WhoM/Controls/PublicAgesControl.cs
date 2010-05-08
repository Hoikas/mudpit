using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MUd {
    public partial class PublicAgesControl : UserControl {

        readonly Guid fCartographersPub = new Guid("35624301-841e-4a07-8db6-b735cf8f1f53");
        readonly Guid fGreetersPub = new Guid("381fb1ba-20a0-45fd-9bcb-fd5922439d05");
        readonly Guid fKveer = new Guid("68e219e0-ee25-4df0-b855-0435584e29e2");
        readonly Guid fMaintainersPub = new Guid("e8306311-56d3-4954-a32d-3da01712e9b5");
        readonly Guid fMessengersPub = new Guid("9420324e-11f8-41f9-b30b-c896171a8712");
        readonly Guid fWritersPub = new Guid("5cf4f457-d546-47dc-80eb-a07cdfefa95d");

        DateTime fLastRefresh;

        MainForm fParent;
        public new MainForm Parent {
            get { return fParent; }
            set { fParent = value; }
        }

        public PublicAgesControl() {
            InitializeComponent();
        }

        public void Clear() {
            fDataGridView.Rows.Clear();
        }

        public bool RefreshAgeList() {
            //Only allow ONE refresh per minute!!!
            //Unless the DGV is empty
            if ((DateTime.Now - fLastRefresh) > new TimeSpan(0, 1, 0) || fDataGridView.Rows.Count == 0) {
                IRequestAge("city");
                IRequestAge("GreatTreePub");   //Watcher's Pub
                IRequestAge("GuildPub-Cartograpers");
                IRequestAge("GuildPub-Greeters");
                IRequestAge("GuildPub-Maintainers");
                IRequestAge("GuildPub-Messengers");
                IRequestAge("GuildPub-Writers");
                IRequestAge("Kveer");
                IRequestAge("Neighborhood");
                IRequestAge("Neighborhood02"); //Kirel

                fLastRefresh = DateTime.Now;
                return true;
            } else
                return false;
        }

        private void IGotAges(NetAgeInfo[] ages, string filename) {
            if (ages.Length == 0) {
                //Arrrrr! We're pirates.
                //But seriously, HACK HACK HACK MOULa if there are no GPs or K'veers
                if (filename.Contains("GuildPub") || filename.Equals("Kveer"))
                    ITryToHackGpKveer(filename);
                return;
            }

            foreach (NetAgeInfo nai in ages) {
                //Find the DataGridViewRow for this age
                int index = -1;
                for (int i = 0; i < fDataGridView.Rows.Count; i++) {
                    if (!(fDataGridView.Rows[i].Tag is NetAgeInfo)) continue;
                    if (fDataGridView.Rows[i].Tag.Equals(nai)) {
                        index = i;
                        break;
                    }
                }

                //If not found (index == -1), then create a row
                //Otherwise, update old row
                if (index == -1) {
                    //If this is a Neighborhood...
                    //AND fCurrPopulation is zero, then DON'T ADD!
                    //Let's not junkify the list ;)
                    if (nai.fFilename.Equals("Neighborhood") && nai.fCurrPopulation == 0)
                        continue;

                    DataGridViewRow r = new DataGridViewRow();
                    r.CreateCells(fDataGridView, new object[] { IMakeInstance(nai), IMakeDescription(nai), nai.fCurrPopulation });
                    r.Tag = nai;

                    fDataGridView.Rows.Add(r);
                } else {
                    //If this is a Neighborhood...
                    //AND fCurrPopulation is zero, then DELETE!
                    //Otherwise, update as usual...
                    if (nai.fFilename.Equals("Neighborhood") && nai.fCurrPopulation == 0)
                        fDataGridView.Rows.RemoveAt(index);
                    else {
                        fDataGridView.Rows[index].Cells[0].Value = IMakeInstance(nai);
                        fDataGridView.Rows[index].Cells[1].Value = IMakeDescription(nai);
                        fDataGridView.Rows[index].Cells[2].Value = nai.fCurrPopulation;
                    }
                }
            }

            //Resort based on user prefs
            //If no pref, sort by age instance name
            if (fDataGridView.SortedColumn != null)
                fDataGridView.Sort(fDataGridView.SortedColumn, (fDataGridView.SortOrder == SortOrder.Ascending ? ListSortDirection.Ascending : ListSortDirection.Descending));
            else
                fDataGridView.Sort(fAgeInstance, ListSortDirection.Ascending);
        }

        private string IMakeDescription(NetAgeInfo nai) {
            if (nai.fFilename == "city") {
                return "D'ni-Ae'gura";
            } else if (nai.fFilename == "GreatTreePub") {
                return "The Watcher's Sanctuary";
            } else if (nai.fFilename == "GuildPub-Cartographers") {
                return "The Cartographers' Guild Pub";
            } else if (nai.fFilename == "GuildPub-Greeters") {
                return "The Greeters' Guild Pub";
            } else if (nai.fFilename == "GuildPub-Maintainers") {
                return "The Maintainers' Guild Pub";
            } else if (nai.fFilename == "GuildPub-Messengers") {
                return "The Messengers' Guild Pub";
            } else if (nai.fFilename == "GuildPub-Writers") {
                return "The Writers' Guild Pub";
            } else if (nai.fFilename == "Kveer") {
                return "Atrus's Childhood Prison";
            } else if (nai.fFilename == "Neighborhood02") {
                return "The DRC's Guild Age";
            } else if (nai.fDescription != String.Empty)
                return nai.fDescription;
            else if (nai.fSequenceNumber != 0)
                return String.Format("{0} ({1}) {2}", nai.fUserName, nai.fSequenceNumber, nai.fInstanceName);
            else if (nai.fUserName != String.Empty)
                return String.Format("{0} {1}", nai.fUserName, nai.fInstanceName);

            return String.Format("You should never see this. [FN: {0}]", nai.fFilename);
        }

        private string IMakeInstance(NetAgeInfo nai) {
            if (nai.fFilename.Contains("GuildPub")) {
                return "Guild Pub";
            } else if (nai.fFilename == "Kveer") {
                return "K'veer";
            } else {
                return nai.fInstanceName;
            }
        }

        private void IRefreshLinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            if (!fParent.AuthCli.Connected) return; //Thou Shalt Not...
            if (!RefreshAgeList())
                MessageBox.Show("You must wait one minute between refreshes.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void IRequestAge(string filename) {
            uint trans = fParent.AuthCli.GetPublicAges(filename);
            fParent.RegisterAuthCB(trans, new Action<NetAgeInfo[], string>(IGotAges), new object[] { filename });
        }

        private void ITryToHackGpKveer(string filename) {
            Guid age = Guid.Empty;
            if (filename.Equals("GuildPub-Cartographers"))
                age = fCartographersPub;
            else if (filename.Equals("GuildPub-Greeters"))
                age = fGreetersPub;
            else if (filename.Equals("GuildPub-Maintainers"))
                age = fMaintainersPub;
            else if (filename.Equals("GuildPub-Messengers"))
                age = fMessengersPub;
            else if (filename.Equals("GuildPub-Writers"))
                age = fWritersPub;
            else if (filename.Equals("Kveer"))
                age = fKveer;

            if (age == Guid.Empty)
                return;

            VaultAgeInfoNode info = new VaultAgeInfoNode();
            info.Filename = filename;
            info.InstanceUUID = age;

            uint transID = fParent.AuthCli.FindVaultNode(info.BaseNode.ToArray());
            fParent.RegisterAuthCB(transID, new Action<uint[], string>(ITryToHackGpKveer), new object[] { filename });
        }

        private void ITryToHackGpKveer(uint[] nodeIDs, string filename) {
            if (nodeIDs.Length == 0) {
                fParent.LogError(String.Format("Tried to make {0} public, but we could not find it in the vault!", filename));
                return;
            } else if (nodeIDs.Length > 1) {
                fParent.LogError(String.Format("Tried to make {0} public, but we got too many ages!!!", filename));
                return;
            }

            fParent.LogInfo(String.Format("Making {0} public...", filename));
            fParent.AuthCli.SetAgePublic(nodeIDs[0], true);

            //Re-request...
            IRequestAge(filename);
        }
    }
}
