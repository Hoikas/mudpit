using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MUd {
    public partial class AddPlayer : UserControl {

        public event Action<uint> Add;

        public string Label {
            get { return fLabel.Text; }
            set { fLabel.Text = value; }
        }

        public AddPlayer() {
            InitializeComponent();
        }

        private void IButtonClicked(object sender, EventArgs e) {
            if (Add != null)
                try {
                    Add(Convert.ToUInt32(fTextBox.Text));
                } catch {
                    MessageBox.Show("Input must be a number", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
        }

        private void ITextChanged(object sender, EventArgs e) {
            try {
                Convert.ToUInt32(fTextBox.Text);
                fButton.Enabled = true;
            } catch {
                fButton.Enabled = false;
            }
        }

        private void IKeyPress(object sender, KeyPressEventArgs e) {
            if (e.KeyChar == (char)Keys.Enter) {
                if (fButton.Enabled) IButtonClicked(null, null);
                e.Handled = true;
            }
        }
    }
}
