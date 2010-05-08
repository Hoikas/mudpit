using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace MUd {
    public partial class ConnectForm : Form {

        readonly Guid kUruExplorer = new Guid("ea489821-6c35-4bd0-9dae-bb17c585e680");
        uint fBuildID = 0;

        DateTime fLastUpdate = DateTime.Now.Subtract(new TimeSpan(0, 0, 5));
        WebRequest fShardReq;
        Shard OurShard {
            get { return (Shard)fShardDropDown.Items[fShardDropDown.SelectedIndex]; }
        }

        #region Internal Stuff
        CallbackCliForm fParent;
        public new CallbackCliForm Parent {
            set { fParent = value; }
        }

        bool fCanAutoConnect = false;
        public bool CanAutoConnect {
            set { fCanAutoConnect = value; }
        }
        #endregion

        public bool AutoConnect {
            get { return fAutoConnect.Checked; }
            set { fAutoConnect.Checked = value; }
        }

        public string Password {
            get { return fPasswordBox.Text; }
            set { fPasswordBox.Text = value; }
        }

        public bool RememberMe {
            get { return fRememberMe.Checked; }
            set { fRememberMe.Checked = value; }
        }

        public string Username {
            get { return fUserBox.Text; }
            set { fUserBox.Text = value; }
        }

        string fPrefShard;
        public string WantShard {
            get { return fPrefShard; }
            set { fPrefShard = value; }
        }

        public ConnectForm() {
            InitializeComponent();

            if (!File.Exists("shards.xml")) {
                IGrabShardList();
            } else {
                FileInfo info = new FileInfo("shards.xml");

                //Update if over a week old
                if ((info.LastWriteTime - DateTime.Now) > new TimeSpan(7, 0, 0, 0))
                    IGrabShardList();
                else {
                    FileStream fs = new FileStream("shards.xml", FileMode.Open, FileAccess.Read);
                    ShardList list = ShardList.Create(fs);
                    IInvokedUpdate(list.fShards);
                    fs.Close();
                }
            }
        }

        private void IGotBuildID(uint buildID) {
            fParent.FileCli.Disconnect();
            fBuildID = buildID;

            fParent.AuthCli.BuildID = buildID;
            fParent.AuthCli.BranchID = 1;
            fParent.AuthCli.ProductID = kUruExplorer;
            fParent.AuthCli.Connect();

            uint transID = fParent.AuthCli.Login(fUserBox.Text, fPasswordBox.Text, fParent.AuthCli.Challenge);
            fParent.RegisterAuthCB(transID, new Action<ENetError, uint, uint[], Guid>(IEndLogin));
        }

        private void IGotFileIP(string ip) {
            fParent.GateCli.Disconnect();

            fParent.FileCli.Host = ip;
            fParent.FileCli.ProductID = kUruExplorer;
            fParent.FileCli.Connect();

            uint transID = fParent.FileCli.RequestBuildID();
            fParent.RegisterFileCB(transID, new Action<uint>(IGotBuildID));
        }

        private void IGotShardList(IAsyncResult ar) {
            WebResponse resp = fShardReq.EndGetResponse(ar);
            Stream s = resp.GetResponseStream();
            ShardList list = ShardList.Create(s);
            Invoke(new Action<Shard[]>(IInvokedUpdate), new object[] { list.fShards });
            list.Serialize("shards.xml");
            fLastUpdate = DateTime.Now;
        }

        private void IGrabShardList() {
            fShardReq = WebRequest.Create("http://mud.hoikas.com/shards.xml");
            fShardReq.BeginGetResponse(new AsyncCallback(IGotShardList), null);
        }

        private void IInvokedUpdate(Shard[] shards) {
            int sel = 0;
            foreach (Shard s in shards) {
                fShardDropDown.Items.Add(s);
                if (s.fName.Equals(fPrefShard))
                    sel = fShardDropDown.Items.IndexOf(s);
            }

            fLogin.Enabled = true;
            fShardDropDown.Enabled = true;
            fShardHostBox.Enabled = true;
            fShardDropDown.SelectedIndex = sel;

            if (fAutoConnect.Checked && fCanAutoConnect)
                IBeginLoginProcess(null, null);
        }

        private void IEndLogin(ENetError result, uint flags, uint[] droid, Guid uuid) {
            fParent.LogInfo(String.Format("Login Complete [RESULT: {0}]", result.ToString().Substring(4)));

            switch (result) {
                case ENetError.kNetErrAccountBanned:
                    MessageBox.Show(this, "Your account has been banned", "Banned", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                case ENetError.kNetErrAccountNotActivated:
                    MessageBox.Show(this, "Your account is not activated", "Not Activated", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                case ENetError.kNetErrAccountNotFound:
                    MessageBox.Show(this, "Your account was not found on the server.", "Account Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                case ENetError.kNetErrAuthenticationFailed:
                    MessageBox.Show(this, "Your password is invalid. Please ensure CAPS LOCK is off.", "Invalid Password", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                case ENetError.kNetErrLoginDenied:
                    MessageBox.Show(this, "The server is denying login attempts. Please try again later.", "Login Denied", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
                case ENetError.kNetErrTooManyFailedLogins:
                    MessageBox.Show(this, "You have failed to authenticate too many times. Please try again later.", "Login Denied", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                case ENetError.kNetSuccess:
                    DialogResult = DialogResult.OK;
                    Close();
                    break;
                default:
                    MessageBox.Show(this, "Unhandled Error Code: " + result.ToString().Substring(4), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
            }

            //Reenable button
            fLogin.Enabled = true;
        }

        #region WinForms Events
        private void ICloseForm(object sender, EventArgs e) {
            this.DialogResult = DialogResult.Cancel;
            Close();
        }

        private void IBeginLoginProcess(object sender, EventArgs e) {
            if (fUserBox.Text == String.Empty || fPasswordBox.Text == String.Empty) {
                MessageBox.Show(this, "Username and Password cannot be blank", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            fLogin.Enabled = false; //Don't allow spam.

            //Set encryption keys...
            fParent.AuthCli.N = OurShard.fAuth.N;
            fParent.AuthCli.X = OurShard.fAuth.X;
            fParent.GateCli.N = OurShard.fGate.N;
            fParent.GateCli.X = OurShard.fGate.X;

            //IP Addresses...
            fParent.AuthCli.Host = OurShard.fAuth.fHost;
            fParent.GateCli.Host = OurShard.fGate.fHost;

            if (fBuildID == 0) {
                fParent.GateCli.ProductID = kUruExplorer;
                fParent.GateCli.Connect();

                uint transID = fParent.GateCli.GetFileHost(true);
                fParent.RegisterGateCB(transID, new Action<string>(IGotFileIP));
            } else {
                fParent.AuthCli.BuildID = fBuildID;
                fParent.AuthCli.BranchID = 1;
                fParent.AuthCli.ProductID = kUruExplorer;
                fParent.AuthCli.Connect();

                uint transID = fParent.AuthCli.Login(fUserBox.Text, fPasswordBox.Text, fParent.AuthCli.Challenge);
                fParent.RegisterAuthCB(transID, new Action<ENetError, uint, uint[], Guid>(IEndLogin));
            }
        }

        private void IRememberMeChecked(object sender, EventArgs e) {
            fAutoConnect.Enabled = fRememberMe.Checked;
        }

        private void IShardChanged(object sender, EventArgs e) {
            fShardHostBox.Text = OurShard.fGate.fHost;
        }
        #endregion

        private void IRefreshShards(object sender, LinkLabelLinkClickedEventArgs e) {
            if ((DateTime.Now - fLastUpdate).Seconds < 5)
                return;

            fLogin.Enabled = false;
            fShardDropDown.Enabled = false;
            fShardHostBox.Enabled = false;
            fShardDropDown.Items.Clear();

            IGrabShardList();
        }
    }
}
