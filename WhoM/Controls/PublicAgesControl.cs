﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MUd {
    public partial class PublicAgesControl : UserControl {

        DateTime fLastRefresh;

        MainForm fParent;
        public new MainForm Parent {
            get { return fParent; }
            set { fParent = value; }
        }

        public PublicAgesControl() {
            InitializeComponent();
        }

        public bool RefreshAgeList() {
            //Only allow ONE refresh per minute!!!
#if !DEBUG
            if ((DateTime.Now - fLastRefresh) > new TimeSpan(0, 1, 0)) {
#endif
                IRequestAges("city");
                IRequestAges("GreatTreePub");   //Watcher's Pub
                IRequestAges("Neighborhood");
                IRequestAges("Neighborhood02"); //Kirel

                //Hacky Cyan stuff...
                //Note: Use BOTH instance and uuid
                //      K'veer is hood instanced and public instanced
                //      We want the public one...
                ICountPlayers(fWritersPub, "GuildPub-Writers", "Guild Pub", "The Guild of Writer's Pub");

                fLastRefresh = DateTime.Now;
                return true;
#if !DEBUG
            } else
                return false;
#endif
        }

        private void IGotAges(NetAgeInfo[] ages) {
            if (ages.Length == 0) return;

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
                    DataGridViewRow r = new DataGridViewRow();
                    r.CreateCells(fDataGridView, new object[] { nai.fInstanceName, IMakeDescription(nai), nai.fCurrPopulation, "View Details" });
                    r.Tag = nai;

                    fDataGridView.Rows.Add(r);
                } else {
                    fDataGridView.Rows[index].Cells[0].Value = nai.fInstanceName;
                    fDataGridView.Rows[index].Cells[1].Value = IMakeDescription(nai);
                    fDataGridView.Rows[index].Cells[2].Value = nai.fCurrPopulation;
                }
            }
        }

        private string IMakeDescription(NetAgeInfo nai) {
            if (nai.fDescription != String.Empty)
                return nai.fDescription;
            else if (nai.fSequenceNumber != 0)
                return String.Format("{0} ({1}) {2}", nai.fUserName, nai.fSequenceNumber, nai.fInstanceName);
            else {
                if (nai.fFilename == "city") {
                    return "D'ni-Ae'gura";
                } else if (nai.fFilename == "GreatTreePub") {
                    return "The Watcher's Sanctuary";
                } else if (nai.fFilename == "Neighborhood02") {
                    return "The DRC's Guild Neighborhood";
                }
            }

            return String.Format("You should never see this. [FN: {0}]", nai.fFilename);
        }

        private void IRefreshLinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            if (!RefreshAgeList())
                MessageBox.Show("You must wait one minute between refreshes.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void IRequestAges(string filename) {
            Callback cb = new Callback(new Action<NetAgeInfo[]>(IGotAges));
            uint trans = fParent.fAuthCli.GetPublicAges(filename);
            fParent.fCallbacks.Add(trans, cb);
        }
    }
}
