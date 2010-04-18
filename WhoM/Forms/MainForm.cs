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

        Dictionary<uint, List<uint>> fVaultTree = new Dictionary<uint, List<uint>>();
        Dictionary<uint, VaultNode> fNodes = new Dictionary<uint, VaultNode>();
        internal Dictionary<uint, Callback> fCallbacks = new Dictionary<uint, Callback>();
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
                        MessageBox.Show("OpenSSL is not installed!\r\nPlease run \"apt-get install libssl-dev\" or compile libcrypto", "OpenSSL Missing", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
            }
        }

        #region Auth Client Helpers
        public void FetchNode(uint nodeID, Callback cb) {
            if (fNodes.ContainsKey(nodeID)) {
                //We already have the node
                //Fire the CB immediately!
                BeginInvoke(cb.fFunc, ICreateArgArray(cb, new object[] { fNodes[nodeID] }));
            } else {
                //Do it the way we're supposed to.
                uint trans = fAuthCli.FetchVaultNode(nodeID);
                fCallbacks.Add(trans, cb);
            }
        }

        public void FetchChildren(uint nodeID, Callback cb) {
            if (!fVaultTree.ContainsKey(nodeID)) return; //Can't do it.

            foreach (uint childID in fVaultTree[nodeID]) {
                if (fNodes.ContainsKey(childID)) {
                    //We already have the node
                    //Fire the CB immediately!
                    BeginInvoke(cb.fFunc, ICreateArgArray(cb, new object[] { fNodes[childID] }));
                } else {
                    //Do it the way we're supposed to.
                    uint trans = fAuthCli.FetchVaultNode(childID);
                    fCallbacks.Add(trans, cb);
                }
            }
        }

        private object[] ICreateArgArray(Callback cb, params object[] args) {
            //Create the list of arguemnts
            //End result
            // - Method: ISomething([auth cb args], [custom args])
            List<object> lArgs = new List<object>(args);
            foreach (object arg in cb.fMyArgs)
                lArgs.Add(arg);
            return lArgs.ToArray();
        }

        private void IFireTransCallback(uint transID, params object[] args) {
            if (fCallbacks.ContainsKey(transID)) {
                Callback c = fCallbacks[transID];
                fCallbacks.Remove(transID); //Delete it

                object[] debug = ICreateArgArray(c, args);
                BeginInvoke(c.fFunc, debug);
            }
        }
        #endregion

        #region Auth Client Message Callbacks
        private void IOnAuthVaultTreeFetched(uint transID, ENetError result, VaultNodeRef[] refs) {
            foreach (VaultNodeRef nRef in refs) {
                if (!fVaultTree.ContainsKey(nRef.fParentIdx))
                    fVaultTree.Add(nRef.fParentIdx, new List<uint>());
                if (!fVaultTree[nRef.fParentIdx].Contains(nRef.fChildIdx)) {
                    fLog.Debug(String.Format("NodeRef [PARENT: {0}] [CHILD: {1}] [SAVER: {2}]", nRef.fParentIdx, nRef.fChildIdx, nRef.fSaverIdx));
                    fVaultTree[nRef.fParentIdx].Add(nRef.fChildIdx);
                }

                //Is this a "core node" ?
                if (nRef.fParentIdx == fActivePlayer) {
                    uint trans = fAuthCli.FetchVaultNode(nRef.fChildIdx);
                    fCallbacks.Add(trans, new Callback(new Action<VaultNode>(IAddFolderToPanes)));
                }
            }
        }

        private void IOnAuthVaultNodeRemoved(uint parentID, uint childID) {
            if (!fVaultTree.ContainsKey(parentID)) return;
            fVaultTree[parentID].Remove(childID);

            //No TransID, so we'll figure out what to do ourselves
            BeginInvoke(new Action<uint, uint>(IRemoveFromPanes), new object[] { parentID, childID });
        }

        private void IOnAuthVaultNodeFound(uint transID, ENetError result, uint[] nodeIDs) {
            //Simply fire the callback...
            //We don't know what the caller wants these nodes for anyway...
            // - Method: ISomething(uint[] nodeIDs)
            IFireTransCallback(transID, new object[] { nodeIDs });
        }

        private void IOnAuthVaultNodeFetched(uint transID, ENetError result, byte[] data) {
            VaultNode node = VaultNode.Parse(data);
            if (fNodes.ContainsKey(node.ID)) fNodes[node.ID] = node;
            else fNodes.Add(node.ID, node);

            //Fire callback
            // - Method: ISomething(VaultNode fetched);
            IFireTransCallback(transID, new object[] { VaultNode.Parse(data) });
        }

        private void IOnAuthVaultNodeChanged(uint nodeID, Guid revUuid) {
            if (!fNodes.ContainsKey(nodeID)) return;

            //No TransID, so we will figure out what we need to update ourselves
            BeginInvoke(new Action<uint>(IUpdateNode), new object[] { nodeID });
        }

        private void IOnAuthVaultNodeAdded(uint parentID, uint childID, uint saverID) {
            if (fVaultTree.ContainsKey(parentID))
                fVaultTree[parentID].Add(childID);

            //No TransID, so don't use IFireTransCallbacks
            if (fBaseNodes.ContainsValue(parentID))
                BeginInvoke(new Action<uint, uint>(IAddToPanes), new object[] { parentID, childID });
        }

        private void IOnAuthPlayerSet(uint transID, ENetError result) {
            fLog.Info(String.Format("Player Set [RESULT: {0}]", result.ToString().Substring(4)));
            switch (result) {
                case ENetError.kNetErrPlayerNotFound:
                    IShowError("Selected player not found!");
                    break;
                case ENetError.kNetSuccess:
                    fActivePlayer = IGetPlayer().fID;
                    fAuthCli.FetchVaultTree(fActivePlayer);
                    break;
                default:
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

            //Do the search
            uint trans = fAuthCli.FindVaultNode(search.BaseNode.ToArray());
            fCallbacks.Add(trans, new Callback(new Action<uint[], uint>(IAddBuddy), new object[] { obj }));
        }

        private void IAddBuddy(uint[] nodeIDs, uint buddy) {
            if (nodeIDs.Length == 0) {
                MessageBox.Show(this, String.Format("KI #{0} is not on the lattice", buddy), "Not Found", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                fLog.Error(String.Format("Attempted to add buddy {0}, but he does not exist!", buddy));
                return;
            }

            fAuthCli.AddVaultNode(fBaseNodes[EStandardNode.kBuddyListFolder], nodeIDs[0], fActivePlayer);
            fLog.Info("Added a buddies");
        }

        private void IAvatarSelected(object sender, EventArgs e) {
            Player p = IGetPlayer();
            if (fActivePlayer == p.fID)
                //Don't redownload the current player...
                return;

            IThrowAway();

            if (p.ToString() == "NULL") return;
            fLog.Info(String.Format("Set Active Player [ID: {0}] [NAME: {1}]", p.fID.ToString(), p.fName));
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
            Callback cb = new Callback(new Action<VaultNode, uint, uint>(IAddToPanes), new object[] { parentID, childID });
            FetchNode(childID, cb); //See IAddToPanes
        }

        private void IAddToPanes(VaultNode node, uint parentID, uint childID) {
            if (node.NodeType == ENodeType.kNodePlayerInfo) {
                if (fBaseNodes[EStandardNode.kBuddyListFolder] == parentID)
                    fBuddyCtrl.AddPlayerInfo(node);
                if (fBaseNodes[EStandardNode.kHoodMembersFolder] == parentID)
                    fNeighborsCtrl.AddPlayerInfo(node);
                if (fBaseNodes[EStandardNode.kPeopleIKnowAboutFolder] == parentID)
                    fRecentsCtrl.AddPlayerInfo(node);
            }
        }

        private void IAddFolderToPanes(VaultNode node) {
            if (node.NodeType == ENodeType.kNodePlayerInfoList) {
                VaultPlayerInfoListNode list = new VaultPlayerInfoListNode(node);

                //Basic stuff
                if (list.FolderType == EStandardNode.kBuddyListFolder) fBaseNodes.Add(EStandardNode.kBuddyListFolder, node.ID);
                if (list.FolderType == EStandardNode.kIgnoreListFolder) fBaseNodes.Add(EStandardNode.kIgnoreListFolder, node.ID);
                if (list.FolderType == EStandardNode.kPeopleIKnowAboutFolder) fBaseNodes.Add(EStandardNode.kPeopleIKnowAboutFolder, node.ID);

                //If it's an AgeOwners folder, we have the Neighborhood :)
                if (list.FolderType == EStandardNode.kAgeOwnersFolder) fBaseNodes.Add(EStandardNode.kHoodMembersFolder, node.ID);
            }

            //Search for Neighborhood in the AgesIOwn
            if (node.NodeType == ENodeType.kNodeAgeInfoList) {
                VaultAgeInfoListNode ages = new VaultAgeInfoListNode(node);
                if (ages.FolderType == EStandardNode.kAgesIOwnFolder) {
                    Callback cb = new Callback(new Action<VaultNode>(IAddFolderToPanes));
                    FetchChildren(node.ID, cb);
                }
            }

            //Grab the AgeInfo...
            if (node.NodeType == ENodeType.kNodeAgeLink) {
                Callback cb = new Callback(new Action<VaultNode>(IAddFolderToPanes));
                FetchChildren(node.ID, cb);
            }

            //See if this is the neighborhood
            if (node.NodeType == ENodeType.kNodeAgeInfo) {
                VaultAgeInfoNode ageinfo = new VaultAgeInfoNode(node);
                if (ageinfo.Filename == "Neighborhood") {
                    //Yep! Grab children :)
                    Callback cb = new Callback(new Action<VaultNode>(IAddFolderToPanes));
                    FetchChildren(node.ID, cb);
                }
            }

            //FIXME: This is a strange hack...
            //Aparently, it causes the selected tab to update. I would say this is spammy though.
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

        private Player IGetPlayer() {
            if (InvokeRequired) {
                return Invoke(new Func<Player>(IGetPlayer)) as Player;
            }

            object obj = fAvatarSelector.Items[fAvatarSelector.SelectedIndex];
            if (obj is Player) return obj as Player;
            else return new Player("NULL", 0);
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
            Callback cb = new Callback(new Action<VaultNode, uint, uint>(IRemoveFromPanes), new object[] { parentID, childID });
            FetchNode(childID, cb); //See IRemoveFromPanes
        }

        private void IRemoveFromPanes(VaultNode node, uint parentID, uint childID) {
            if (fBaseNodes[EStandardNode.kBuddyListFolder] == parentID)
                fBuddyCtrl.RemoveNode(node);
            if (fBaseNodes[EStandardNode.kHoodMembersFolder] == parentID)
                fNeighborsCtrl.RemoveNode(node);
            if (fBaseNodes[EStandardNode.kPeopleIKnowAboutFolder] == parentID)
                fRecentsCtrl.RemoveNode(node);
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
            Callback cb = new Callback(new Action<VaultNode>(IUpdateNode));
            uint trans = fAuthCli.FetchVaultNode(idx);
            fCallbacks.Add(trans, cb);
        }

        private void IUpdateNode(VaultNode node) {
            if (node.NodeType == ENodeType.kNodePlayerInfo) {
                VaultPlayerInfoNode info = new VaultPlayerInfoNode(node);
                bool alerted = IDoBuddyUpdate(info, false);
                IDoNeighborUpdate(info, alerted);
                fRecentsCtrl.UpdateNode(info);
            }
        }
    }
}
