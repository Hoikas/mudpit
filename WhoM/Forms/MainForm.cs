using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace MUd {
    public partial class MainForm : Form {

        internal AuthClient fAuthCli = new AuthClient();
        internal LogProcessor fLog = new LogProcessor("WhoM");

        Dictionary<uint, uint[]> fNodesFound = new Dictionary<uint, uint[]>();
        Dictionary<uint, List<uint>> fVaultTree = new Dictionary<uint, List<uint>>();
        Dictionary<uint, VaultNode> fNodes = new Dictionary<uint, VaultNode>();
        Dictionary<uint, ManualResetEvent> fMREs = new Dictionary<uint, ManualResetEvent>();
        internal uint fActivePlayer;

        Dictionary<EStandardNode, uint> fBaseNodes = new Dictionary<EStandardNode, uint>();

        [STAThread]
        static void Main() {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
#if !DEBUG
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
#endif
            Application.Run(new MainForm());
        }

        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e) {
            ExceptionForm eForm = new ExceptionForm(e.Exception);
            eForm.ShowDialog();
            Application.Exit();
        }

        public MainForm() {
#if !DEBUG
            if (!System.IO.File.Exists("MUd.conf"))
                fLog.LogLevel = ELogType.kLogWarning;
#endif

            InitializeComponent();

            fBuddyCtrl.Parent = this;
            fNeighborsCtrl.CanRemove = false; //NO!!!!!!!!!!!!!!!!!
            fNeighborsCtrl.Parent = this;
            fRecentsCtrl.Parent = this;

#if !DEBUG
            fAuthCli.ExceptionHandler += new ExceptionArgs(IOnAuthException);
#endif
            fAuthCli.KickedOff += new AuthKickedOff(IOnAuthKickedOff);
            fAuthCli.PlayerInfo += new AuthPlayerInfo(IOnAuthPlayerInfo);
            fAuthCli.PlayerSet += new AuthResult(IOnAuthPlayerSet);
            fAuthCli.VaultNodeAdded += new AuthVaultNodeAdded(IOnAuthVaultNodeAdded);
            fAuthCli.VaultNodeChanged += new AuthVaultNodeChanged(IOnAuthVaultNodeChanged);
            fAuthCli.VaultNodeFetched += new AuthVaultNodeFetched(IOnAuthVaultNodeFetched);
            fAuthCli.VaultNodeFound += new AuthVaultNodeFound(IOnAuthVaultNodeFound);
            fAuthCli.VaultNodeRemoved += new AuthVaultNodeRemoved(IOnAuthVaultNodeRemoved);
            fAuthCli.VaultTreeFetched += new AuthVaultTreeFetched(IOnAuthVaultTreeFetched);

            //Are we running Mono?
            switch (Environment.OSVersion.Platform) {
                case PlatformID.MacOSX:
                case PlatformID.Unix:
                    if (!OpenSSL.OpenSSL.IsDllPresent)
                        MessageBox.Show("OpenSSL is not installed!\r\nPlease run apt-get install libssl-dev or compile libcrypto", "OpenSSL Missing", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
            }
        }

        public VaultNode FetchNode(uint id) {
            if (fNodes.ContainsKey(id)) return fNodes[id];
            else {
                IDownloadNode(id);
                return fNodes[id];
            }
        }

        public VaultNode[] FetchChildren(uint id) {
            if (!fVaultTree.ContainsKey(id)) return new VaultNode[0];

            List<VaultNode> nodes = new List<VaultNode>();
            List<uint> children = fVaultTree[id];
            foreach (uint child in children)
                nodes.Add(FetchNode(child));
            return nodes.ToArray();
        }

        public VaultNode[] FindNode(VaultNode search) {
            ManualResetEvent mre = new ManualResetEvent(false);
            uint transID = fAuthCli.FindVaultNode(search.ToArray());
            fMREs.Add(transID, mre);
            mre.WaitOne();

            uint[] ids = fNodesFound[transID];
            fNodesFound.Remove(transID);

            VaultNode[] nodes = new VaultNode[ids.Length];
            for (int i = 0; i < ids.Length; i++)
                nodes[i] = FetchNode(ids[i]);
            return nodes;
        }

        #region Auth Client CBs
        private void IOnAuthVaultTreeFetched(uint transID, ENetError result, VaultNodeRef[] refs) {
            if (result != ENetError.kNetSuccess) {
                IHideStatus();
                IShowError("Unhandled Error Code: " + result.ToString().Substring(4));
                return;
            }

            lock (fVaultTree) {
                foreach (VaultNodeRef nRef in refs) {
                    if (!fVaultTree.ContainsKey(nRef.fParentIdx))
                        fVaultTree.Add(nRef.fParentIdx, new List<uint>());
                    if (!fVaultTree[nRef.fParentIdx].Contains(nRef.fChildIdx)) {
                        fLog.Debug(String.Format("NodeRef [PARENT: {0}] [CHILD: {1}] [SAVER: {2}]", nRef.fParentIdx, nRef.fChildIdx, nRef.fSaverIdx));
                        fVaultTree[nRef.fParentIdx].Add(nRef.fChildIdx);
                    }

                    //Is this a "core node" ?
                    if (nRef.fParentIdx == fActivePlayer) {
                        fAuthCli.FetchVaultNode(nRef.fChildIdx);
                    }
                }
            }
        }

        private void IOnAuthVaultNodeRemoved(uint parentID, uint childID) {
            if (!fVaultTree.ContainsKey(parentID)) return;
            fVaultTree[parentID].Remove(childID);
            BeginInvoke(new Action<uint, uint>(IRemoveFromPanes), new object[] { parentID, childID });
        }

        private void IOnAuthVaultNodeFound(uint transID, ENetError result, uint[] nodeIDs) {
            fNodesFound.Add(transID, nodeIDs);

            //Release any MREs associated with this TransID
            lock (fMREs) {
                if (fMREs.ContainsKey(transID)) {
                    fMREs[transID].Set();
                    fMREs.Remove(transID);
                }
            }
        }

        private void IOnAuthVaultNodeFetched(uint transID, ENetError result, byte[] data) {
            if (result != ENetError.kNetSuccess) {
                IHideStatus();
                IShowError("Unhandled Error Code: " + result.ToString().Substring(4));
                return;
            }

            VaultNode node = VaultNode.Parse(data);
            lock (fNodes)
                if (fNodes.ContainsKey(node.ID)) fNodes[node.ID] = node;
                else fNodes.Add(node.ID, node);

            //Release any MREs associated with this TransID
            lock (fMREs) {
                if (fMREs.ContainsKey(transID)) {
                    fMREs[transID].Set();
                    fMREs.Remove(transID);
                }
            }

            //Send the node to the pane populator
            //The invoked method determines which folder we have, fetches any related nodes, etc.
            BeginInvoke(new Action<uint>(IAddFolderToPanes), new object[] { node.ID });
        }

        private void IOnAuthVaultNodeChanged(uint nodeID, Guid revUuid) {
            lock (fNodes) {
                if (!fNodes.ContainsKey(nodeID)) return;
                BeginInvoke(new Action<uint>(IUpdateNode), new object[] { nodeID });
            }
        }

        private void IOnAuthVaultNodeAdded(uint parentID, uint childID, uint saverID) {
            if (fVaultTree.ContainsKey(parentID))
                fVaultTree[parentID].Add(childID);

            if (fBaseNodes.ContainsValue(parentID))
                BeginInvoke(new Action<uint, uint>(IAddToPanes), new object[] { parentID, childID });
        }

        private void IOnAuthPlayerSet(uint transID, ENetError result) {
            fLog.Info(String.Format("Player Set [RESULT: {0}]", result.ToString().Substring(4)));
            switch (result) {
                case ENetError.kNetErrPlayerNotFound:
                    IHideStatus();
                    IShowError("Selected player not found!");
                    break;
                case ENetError.kNetSuccess:
                    ISetStatus("Downloading Player Tree");
                    fActivePlayer = IGetPlayer().fID;
                    fAuthCli.FetchVaultTree(fActivePlayer);
                    break;
                default:
                    IHideStatus();
                    IShowError("Unhandled Error Code: " + result.ToString().Substring(4));
                    break;
            }
        }

        private void IOnAuthPlayerInfo(uint transID, string name, uint idx, string shape, uint explorer) {
            Invoke(new Action<string, uint>(IAddAvatar), new object[] { name, idx });
        }

        private void IOnAuthKickedOff(ENetError reason) {
            Invoke(new Action<ENetError>(IKickedByAuth), new object[] { reason });
        }

        private void IOnAuthException(Exception e) {
            Invoke(new Action<object, ThreadExceptionEventArgs>(Application_ThreadException), new object[] { null, new ThreadExceptionEventArgs(e) });
        }
        #endregion

        #region Menu Events
        private void IAboutMe(object sender, EventArgs e) {
            AboutBox abox = new AboutBox();
            abox.Show(this);
        }

        private void IConnect(object sender, EventArgs e) {
            ConnectForm cf = new ConnectForm(fAuthCli, fLog, (sender == null));
            if (cf.ShowDialog(this) == DialogResult.OK) {
                fConnectMenuItem.Enabled = false;
                fDisconnectMenuItem.Enabled = true;

                //Attempt to select the last used avatar [by index]
                //If there aren't that many avatars, use the first one.
                uint avatar = Prefrences.LastAvatar;
                if (avatar == 0)
                    fAvatarSelector.SelectedIndex = 0;
                else {
                    bool selected = false;
                    foreach (object obj in fAvatarSelector.Items) {
                        if (((Player)obj).fID == avatar) {
                            fAvatarSelector.SelectedIndex = fAvatarSelector.Items.IndexOf(obj);
                            selected = true;
                            break;
                        }
                    }

                    //Select the first avatar if the cached ID was not in the list
                    //This happens when an avatar is deleted or a new account is used
                    if (!selected)
                        fAvatarSelector.SelectedIndex = 0;
                }
            }
        }

        private void IDisconnect(object sender, EventArgs e) {
            fLog.Info("Disconnected");
            fAuthCli.Disconnect();

            IThrowAway();
            fAvatarSelector.Items.Clear();
            fActivePlayer = 0;

            fDisconnectMenuItem.Enabled = false;
            fConnectMenuItem.Enabled = true;
        }

        private void IExit(object sender, EventArgs e) {
            Application.Exit();
        }

        private void IOpenDrcForum(object sender, EventArgs e) {
            System.Diagnostics.Process.Start("http://www.drcsite.org/forums/index.php");
        }

        private void IOpenMUdSite(object sender, EventArgs e) {
            System.Diagnostics.Process.Start("http://mud.hoikas.com");
        }

        private void IOpenMOULForum(object sender, EventArgs e) {
            System.Diagnostics.Process.Start("http://www.mystonline.com/forums");
        }

        private void IOptions(object sender, EventArgs e) {
            OptionsForm of = new OptionsForm();
            of.Show(this);
        }
        #endregion

        #region Misc WinForms Events
        private void IAddBuddy(uint obj) {
            if (fBuddyCtrl.HasPlayer(obj)) {
                MessageBox.Show(this, String.Format("KI #{0} is already in the Buddies List", obj), "Already Added", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                fLog.Warn(String.Format("Attempted to add #{0} to buddies list, but he is already there!", obj));
                return;
            }

            VaultPlayerInfoNode search = new VaultPlayerInfoNode();
            search.PlayerID = obj;

            VaultNode[] nodes = FindNode(search.BaseNode);
            if (nodes.Length == 0) {
                MessageBox.Show(this, String.Format("KI #{0} is not on the lattice", obj), "Not Found", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                fLog.Error(String.Format("Attempted to add #{0} to buddies list, but he does not exist!", obj));
                return;
            }

            fAuthCli.AddVaultNode(fBaseNodes[EStandardNode.kBuddyListFolder], nodes[0].ID, fActivePlayer);
            fLog.Info(String.Format("Added #{0} to buddies", obj));
        }

        private void IAvatarSelected(object sender, EventArgs e) {
            Player p = IGetPlayer();
            if (fActivePlayer == p.fID)
                //Don't redownload the current player...
                return;

            IThrowAway();

            if (p.ToString() == "NULL") return;
            fLog.Info(String.Format("Set Active Player [ID: {0}] [NAME: {1}]", p.fID.ToString(), p.fName));
            ISetStatus("Setting Active Player");
            fAuthCli.SetActivePlayer(p.fID);
            Prefrences.LastAvatar = p.fID;
        }

        private void IFormResized(object sender, EventArgs e) {
            if (WindowState == FormWindowState.Minimized)
                Hide();
        }

        private void IFormShown(object sender, EventArgs e) {
            if (Prefrences.AutoConnect && Prefrences.RememberLogin)
                IConnect(null, null);
        }

        private void INotifyIconDoubleClicked(object sender, MouseEventArgs e) {
            switch (WindowState) {
                case FormWindowState.Minimized:
                    BringToFront();
                    Show();
                    WindowState = FormWindowState.Normal;
                    break;
                case FormWindowState.Normal:
                case FormWindowState.Maximized:
                    WindowState = FormWindowState.Minimized;
                    break;
            }
        }

        private void ITabSelected(object sender, TabControlEventArgs e) {
            string tag = e.TabPage.Tag.ToString();
            if (e.Action == TabControlAction.Selecting) {
                fLog.Info(String.Format("Selecting [TAB: {0}]", tag));
                return;
            } else if (e.Action != TabControlAction.Selected) return;

            if (tag == "buddies")
                if (fBaseNodes.ContainsKey(EStandardNode.kBuddyListFolder))
                    fBuddyCtrl.SetFolder(fBaseNodes[EStandardNode.kBuddyListFolder]);

            if (tag == "neighbors")
                if (fBaseNodes.ContainsKey(EStandardNode.kHoodMembersFolder))
                    fNeighborsCtrl.SetFolder(fBaseNodes[EStandardNode.kHoodMembersFolder]);

            if (tag == "recents")
                if (fBaseNodes.ContainsKey(EStandardNode.kPeopleIKnowAboutFolder))
                    fRecentsCtrl.SetFolder(fBaseNodes[EStandardNode.kPeopleIKnowAboutFolder]);
        }
        #endregion

        private void IAddAvatar(string name, uint idx) {
            Player p = new Player(name, idx);
            fAvatarSelector.Items.Add(p);
        }

        private void IAddToPanes(uint parentID, uint childID) {
            VaultNode node = FetchNode(childID);

            if (node.NodeType == ENodeType.kNodePlayerInfo) {
                VaultPlayerInfoNode info = new VaultPlayerInfoNode(node);
                if (fBaseNodes[EStandardNode.kBuddyListFolder] == parentID)
                    fBuddyCtrl.AddPlayerInfo(info);
                if (fBaseNodes[EStandardNode.kHoodMembersFolder] == parentID)
                    fNeighborsCtrl.AddPlayerInfo(info);
                if (fBaseNodes[EStandardNode.kPeopleIKnowAboutFolder] == parentID)
                    fRecentsCtrl.AddPlayerInfo(info);
            }
        }

        private void IAddFolderToPanes(uint id) {
            VaultNode node = fNodes[id];
            if (node.NodeType == ENodeType.kNodePlayerInfoList) {
                VaultPlayerInfoListNode list = new VaultPlayerInfoListNode(node);
                if (list.FolderType == EStandardNode.kBuddyListFolder) fBaseNodes.Add(EStandardNode.kBuddyListFolder, id);
                if (list.FolderType == EStandardNode.kIgnoreListFolder) fBaseNodes.Add(EStandardNode.kIgnoreListFolder, id);
                if (list.FolderType == EStandardNode.kPeopleIKnowAboutFolder) fBaseNodes.Add(EStandardNode.kPeopleIKnowAboutFolder, id);
            }

            //Search for Neighbors
            if (node.NodeType == ENodeType.kNodeAgeInfoList) {
                VaultAgeInfoListNode ages = new VaultAgeInfoListNode(node);
                if (ages.FolderType == EStandardNode.kAgesIOwnFolder) {
                    VaultNode[] children = FetchChildren(id);
                    VaultAgeInfoNode hood = null;
                    foreach (VaultNode link in children) {
                        if (fVaultTree[link.ID].Count != 1) continue;
                        VaultAgeInfoNode ageinfo = new VaultAgeInfoNode(FetchNode(fVaultTree[link.ID][0]));
                        if (ageinfo.Filename == "Neighborhood") {
                            hood = ageinfo;
                            break;
                        }
                    }

                    if (hood == null) return;
                    VaultNode[] info_children = FetchChildren(hood.BaseNode.ID);
                    foreach (VaultNode child in info_children) {
                        if (child.NodeType == ENodeType.kNodePlayerInfoList) {
                            VaultPlayerInfoListNode list = new VaultPlayerInfoListNode(child);
                            if (list.FolderType == EStandardNode.kAgeOwnersFolder) {
                                fBaseNodes.Add(EStandardNode.kHoodMembersFolder, list.BaseNode.ID);
                                break;
                            }
                        }
                    }
                }
            }

            ITabSelected(null, new TabControlEventArgs(fTabControl.SelectedTab, fTabControl.SelectedIndex, TabControlAction.Selected));
        }

        private bool IDoNeighborUpdate(VaultPlayerInfoNode node, bool alertShown) {
            if (fNeighborsCtrl.UpdateNode(node))
                if (Prefrences.NeighborAlert && !alertShown) {
                    fNotifyIcon.ShowBalloonTip(10, "Neighbor Login", String.Format("{0} is now exploring D'ni", node.PlayerName), ToolTipIcon.None);
                    return true;
                }

            return false;
        }

        private bool IDoBuddyUpdate(VaultPlayerInfoNode node, bool alertShown) {
            if (fBuddyCtrl.UpdateNode(node))
                if (Prefrences.BuddyAlert && !alertShown) {
                    fNotifyIcon.ShowBalloonTip(10, "Buddy Login", String.Format("{0} is now exploring D'ni", node.PlayerName), ToolTipIcon.None);
                    return true;
                }

            return false;
        }

        private void IDownloadNode(uint id) {
            string status = "Downloading Vault Node #" + id.ToString();
            ISetStatus(status);
            fLog.Info(status);

            uint transID = fAuthCli.FetchVaultNode(id);
            ManualResetEvent mre = new ManualResetEvent(false);
            lock (fMREs) fMREs.Add(transID, mre);
            mre.WaitOne();
            IHideStatus();
        }

        private Player IGetPlayer() {
            if (InvokeRequired) {
                return Invoke(new Func<Player>(IGetPlayer)) as Player;
            }

            object obj = fAvatarSelector.Items[fAvatarSelector.SelectedIndex];
            if (obj is Player) return obj as Player;
            else return new Player("NULL", 0);
        }

        private void IHideStatus() {
            if (InvokeRequired) {
                BeginInvoke(new MethodInvoker(IHideStatus));
                return;
            }

            fProgressLabel.Visible = false;
            fProgressBar.Visible = false;
        }

        private void IKickedByAuth(ENetError reason) {
            //Get the enum name of the reason
            string temp = reason.ToString().Substring(4);
            fLog.Error(String.Format("KICKED OFF [REASON: {0}]", temp));

            //Try to find a prettier reason, but fall back on the ugly one...
            string display = String.Format("UKNOWN REASON [{0}]", temp);
            switch (reason) {
                case ENetError.kNetErrDisconnected:
                    display = "Something bad happened on the internet";
                    break;
                case ENetError.kNetErrKickedByCCR:
                    display = "You were kicked by CCR";
                    break;
                case ENetError.kNetErrLoggedInElsewhere:
                    display = "You logged in somewhere else";
                    break;
                case ENetError.kNetErrRemoteShutdown:
                    display = "The servers went down";
                    break;
            }

            fNotifyIcon.ShowBalloonTip(30, "Disconnected", display, ToolTipIcon.Warning);
            IDisconnect(null, null);
        }

        private void IRemoveFromPanes(uint parentID, uint childID) {
            VaultNode node = FetchNode(childID);
            if (fBaseNodes[EStandardNode.kBuddyListFolder] == parentID)
                fBuddyCtrl.RemoveNode(node);
            if (fBaseNodes[EStandardNode.kHoodMembersFolder] == parentID)
                fNeighborsCtrl.RemoveNode(node);
            if (fBaseNodes[EStandardNode.kPeopleIKnowAboutFolder] == parentID)
                fRecentsCtrl.RemoveNode(node);
        }



        private void ISetStatus(string text) {
            if (InvokeRequired) {
                BeginInvoke(new Action<string>(ISetStatus), new object[] { text });
                return;
            }

            fProgressLabel.Text = text;
            fProgressLabel.Visible = true;
            fProgressBar.Visible = true;
        }

        private void IShowError(string text) {
            if (InvokeRequired) {
                BeginInvoke(new Action<string>(IShowError), new object[] { text });
                return;
            }

            MessageBox.Show(this, text, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        private void IThrowAway() {
            fBaseNodes.Clear();
            fVaultTree.Clear();
            fNodes.Clear();

            fBuddyCtrl.Clear();
            fNeighborsCtrl.Clear();
            fRecentsCtrl.Clear();
        }

        private void IUpdateNode(uint idx) {
            IDownloadNode(idx);
            VaultNode node = fNodes[idx];
            if (node.NodeType == ENodeType.kNodePlayerInfo) {
                VaultPlayerInfoNode info = new VaultPlayerInfoNode(node);
                bool alerted = IDoBuddyUpdate(info, false);
                IDoNeighborUpdate(info, alerted);
                fRecentsCtrl.UpdateNode(info);
            }
        }
    }
}
