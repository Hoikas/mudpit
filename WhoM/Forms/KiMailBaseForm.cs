using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MUd {
    public partial class KiMailBaseForm : Form {

        protected MainForm fParent;
        public new MainForm Parent {
            set { fParent = value; }
        }

        public KiMailBaseForm() {
            InitializeComponent();
        }

        protected virtual void IExport(object sender, EventArgs e) { }
        protected virtual void IReplace(object sender, EventArgs e) { }
        protected virtual void IRevert(object sender, EventArgs e) { }
        protected virtual void ISave(object sender, EventArgs e) { }
    }
}
