using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MUd {
    public partial class OptionsForm : Form {
        public OptionsForm() {
            InitializeComponent();

            fBuddyLogin.Checked = Prefrences.BuddyAlert;
            fNeighborLogin.Checked = Prefrences.NeighborAlert;
        }

        private void ISaveChanges(object sender, EventArgs e) {
            Prefrences.BuddyAlert = fBuddyLogin.Checked;
            Prefrences.NeighborAlert = fNeighborLogin.Checked;
            Close();
        }

        private void ICancel(object sender, EventArgs e) {
            Close();
        }
    }
}
