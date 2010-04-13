using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace MUd {
    public partial class ExceptionForm : Form {

        public string AssemblyVersion {
            get {
                return Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

        public ExceptionForm(Exception e) {
            InitializeComponent();

            string innerException = "(null)";
            string innerExceptionMsg = "(null)";
            if (e.InnerException != null) {
                innerException = e.InnerException.GetType().ToString();
                innerExceptionMsg = e.InnerException.Message;
            }

            fTextBox.Text = String.Format(fTextBox.Text, AssemblyVersion, e.GetType().ToString(), e.Message, e.Source, innerException, innerExceptionMsg);
            fTextBox.Text += "\r\n" + e.StackTrace;
        }

        private void ICopyDump(object sender, EventArgs e) {
            Clipboard.SetText(fTextBox.Text, TextDataFormat.UnicodeText);
        }

        private void IOkay(object sender, EventArgs e) {
            Close();
        }
    }
}
