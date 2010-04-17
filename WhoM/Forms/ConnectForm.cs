using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace MUd {
    public partial class ConnectForm : Form {

        readonly Guid kUruExplorer = new Guid("ea489821-6c35-4bd0-9dae-bb17c585e680");

        WebRequest fShardReq;
        Shard OurShard {
            get { return (Shard)fShardDropDown.Items[fShardDropDown.SelectedIndex]; }
        }

        AuthClient fAuthCli;
        AuthLoggedIn fLoginCB;
        AuthClientRegistered fRegisterCB;

        GateClient fGateCli = new GateClient();
        FileClient fFileCli = new FileClient();

        LogProcessor fLog;
        ManualResetEvent fBuildReset = new ManualResetEvent(false);
        ManualResetEvent fRegisterReset = new ManualResetEvent(false);

        uint fBuildID = 0;
        bool fCanAutoConnect = false;

        public ConnectForm(AuthClient cli, LogProcessor log, bool canAutoConnect) {
            fLog = log;
            InitializeComponent();
            if (Prefrences.RememberLogin) {
                fAutoConnect.Checked = Prefrences.AutoConnect;
                fRememberMe.Checked = true;
                fUserBox.Text = Prefrences.Username;
                fPasswordBox.Text = Prefrences.Password;
            }

            fRegisterCB = new AuthClientRegistered(IOnAuthCliRegister);
            fLoginCB = new AuthLoggedIn(IOnAuthLoggedIn);
            fCanAutoConnect = canAutoConnect;

            fAuthCli = cli;
            fAuthCli.ClientRegistered += fRegisterCB;
            fAuthCli.LoggedIn += fLoginCB;

            fShardReq = WebRequest.Create("http://mud.hoikas.com/shards.xml");
            fShardReq.BeginGetResponse(new AsyncCallback(IGotShardList), null);

            fAuthCli.ProductID = kUruExplorer;
            fFileCli.ProductID = kUruExplorer;
            fGateCli.ProductID = kUruExplorer;

            fGateCli.GotFileSrvIP += new GateIP(IGotFileSrvIP);
            fFileCli.GotBuildID += new FileBuildIdReply(IGotBuildID);
        }

        private void IGotBuildID(uint transID, ENetError result, uint buildID) {
            fFileCli.Disconnect();
            fBuildID = buildID;
            fBuildReset.Set();

            fLog.Debug(String.Format("BuildID: [{0}]", buildID));
        }

        private void IGotFileSrvIP(uint transID, string ip) {
            fGateCli.Disconnect();

            fFileCli.Host = ip;
            fFileCli.Connect();
            fFileCli.RequestBuildID();

            fLog.Debug(String.Format("FileSrv IP: [{0}]", ip));
        }

        private void IGotShardList(IAsyncResult ar) {
            WebResponse resp = fShardReq.EndGetResponse(ar);
            ShardList s = ShardList.Create(resp.GetResponseStream());
            Invoke(new Action<Shard[]>(IInvokedUpdate), new object[] { s.fShards });
        }

        private void IInvokedUpdate(Shard[] shards) {
            int sel = 0;
            foreach (Shard s in shards) {
                string test = Prefrences.Shard;
                fShardDropDown.Items.Add(s);
                if (s.fName == test)
                    sel = fShardDropDown.Items.IndexOf(s);
            }

            fLogin.Enabled = true;
            fShardDropDown.Enabled = true;
            fShardHostBox.Enabled = true;
            fShardDropDown.SelectedIndex = sel;

            if (fAutoConnect.Checked && fCanAutoConnect)
                IBeginLoginProcess(null, null);
        }

        private void IOnAuthCliRegister(uint challenge) {
            fRegisterReset.Set();
        }

        private void IOnAuthLoggedIn(uint transID, ENetError result, uint flags, uint[] droidKey, uint billing, Guid acctUuid) {
            Invoke(new Action<ENetError>(IEndLogin), new object[] { result });
        }

        private void IEndLogin(ENetError result) {
            fLog.Info(String.Format("Login Complete [RESULT: {0}]", result.ToString().Substring(4)));

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
                    if (fRememberMe.Checked) {
                        Prefrences.AutoConnect = fAutoConnect.Checked;
                        Prefrences.RememberLogin = true;
                        Prefrences.Shard = OurShard.fName;
                        Prefrences.Username = fUserBox.Text;
                        Prefrences.Password = fPasswordBox.Text;
                    }

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
            fAuthCli.N = OurShard.fAuth.N;
            fAuthCli.X = OurShard.fAuth.X;
            fGateCli.N = OurShard.fGate.N;
            fGateCli.X = OurShard.fGate.X;

            //IP Addresses...
            fAuthCli.Host = OurShard.fAuth.fHost;
            fGateCli.Host = OurShard.fGate.fHost;

            if (fBuildID == 0) {
                fGateCli.Connect();
                fGateCli.GetFileHost(true);
                fBuildReset.WaitOne(); //Wait on the BuildID
            }

            if (!fAuthCli.Connected) {
                fAuthCli.BranchID = 1;
                fAuthCli.BuildID = fBuildID;
                fAuthCli.Connect();
                fRegisterReset.WaitOne(); //Wait for the ClientRegister
            }

            fAuthCli.Login(fUserBox.Text, fPasswordBox.Text, fAuthCli.Challenge);
        }

        private void IFormClosing(object sender, FormClosingEventArgs e) {
            fAuthCli.ClientRegistered -= fRegisterCB;
            fAuthCli.LoggedIn -= fLoginCB;
        }

        private void IRememberMeChecked(object sender, EventArgs e) {
            fAutoConnect.Enabled = fRememberMe.Checked;
        }

        private void IShardChanged(object sender, EventArgs e) {
            fShardHostBox.Text = OurShard.fGate.fHost;
        }
        #endregion
    }
}
