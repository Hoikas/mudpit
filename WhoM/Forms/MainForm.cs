using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;

using Timer = System.Windows.Forms.Timer;

namespace MUd {
    public partial class MainForm : CallbackCliForm {

        Dictionary<uint, List<uint>> fVaultTree = new Dictionary<uint, List<uint>>();
        Dictionary<uint, VaultNode> fNodes = new Dictionary<uint, VaultNode>();

        Guid fAcctUuid;
        internal uint fActivePlayer;

        Dictionary<EStandardNode, uint> fBaseNodes = new Dictionary<EStandardNode, uint>();
        Timer fAgeRefresh = new Timer();

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

        public MainForm() : base() {
            InitializeComponent();
            EnableLogging("WhoM");

            fBuddyCtrl.Parent = this;
            fKiMailCtrl.Parent = this;
            fNeighborsCtrl.CanRemove = false; //NO!!!!!!!!!!!!!!!!!
            fNeighborsCtrl.Parent = this;
            fPublicAgesCtrl.Parent = this;
            fRecentsCtrl.Parent = this;

#if !DEBUG
            AuthCli.ExceptionHandler +=new ExceptionArgs(IOnAuthException);
#endif
        }

        #region Auth Client Helpers
        public void FetchNode(uint nodeID, Delegate func) { FetchNode(nodeID, func, new object[0]); }
        public void FetchNode(uint nodeID, Delegate func, object[] args) {
            if (fNodes.ContainsKey(nodeID)) {
                BeginInvoke(func, CreateArgArray(args, new object[] { fNodes[nodeID] }));
            } else {
                //Do it the way we're supposed to.
                uint trans = AuthCli.FetchVaultNode(nodeID);
                RegisterAuthCB(trans, func, args);
            }
        }

        public void FetchChildren(uint nodeID, Delegate func) { FetchChildren(nodeID, func, new object[0]); }
        public void FetchChildren(uint nodeID, Delegate func, object[] args) {
            if (!fVaultTree.ContainsKey(nodeID)) return; //Can't do it.

            foreach (uint childID in fVaultTree[nodeID]) {
                if (fNodes.ContainsKey(childID)) {
                    //We already have the node
                    //Fire the CB immediately!
                    BeginInvoke(func, CreateArgArray(args, new object[] { fNodes[childID] }));
                } else {
                    //Do it the way we're supposed to.
                    uint trans = AuthCli.FetchVaultNode(childID);
                    RegisterAuthCB(trans, func, args);
                }
            }
        }

        public int GetChildCount(uint nodeID) {
            //Be careful!
            //If the node's tree has not been fetched, this may errouneously return 0!
            if (fVaultTree.ContainsKey(nodeID))
                return fVaultTree[nodeID].Count;
            else
                return 0;
        }

        public bool HasChild(uint parentID, uint childID) {
            if (fVaultTree.ContainsKey(parentID))
                return fVaultTree[parentID].Contains(childID);
            else
                return false;
        }
        #endregion

        #region Auth Client Event Handlers
        private void IOnAuthException(Exception e) {
            Invoke(new Action<object, ThreadExceptionEventArgs>(Application_ThreadException), new object[] { null, new ThreadExceptionEventArgs(e) });
        }

        protected override void OnAuthKickedOff(ENetError reason) {
            base.OnAuthKickedOff(reason);
            Invoke(new Action<ENetError>(IKickedByAuth), new object[] { reason });
        }

        protected override void OnAuthPlayerInfo(uint transID, string name, uint idx, string shape, uint explorer) {
            base.OnAuthPlayerInfo(transID, name, idx, shape, explorer);
            Invoke(new Action<string, uint>(IAddAvatar), new object[] { name, idx });
        }

        protected override void OnAuthPlayerSet(uint transID, ENetError result) {
            base.OnAuthPlayerSet(transID, result);

            LogInfo(String.Format("Player Set [RESULT: {0}]", result.ToString().Substring(4)));
            switch (result) {
                case ENetError.kNetErrPlayerNotFound:
                    IShowError("Selected player not found!");
                    break;
                case ENetError.kNetSuccess:
                    fActivePlayer = IGetPlayer().fID;
                    AuthCli.FetchVaultTree(fActivePlayer);
                    break;
                default:
                    IShowError("Unhandled Error Code: " + result.ToString().Substring(4));
                    break;
            }
        }

        protected override void OnAuthVaultNodeAdded(uint parentID, uint childID, uint saverID) {
            base.OnAuthVaultNodeAdded(parentID, childID, saverID);

            if (fVaultTree.ContainsKey(parentID))
                fVaultTree[parentID].Add(childID);

            //No TransID, so don't use IFireTransCallbacks
            if (fBaseNodes.ContainsValue(parentID))
                FetchNode(childID, new Action<VaultNode, uint, uint>(IAddToPanes), new object[] { parentID, childID });
        }

        protected override void OnAuthVaultNodeChanged(uint nodeID, Guid revUuid) {
            base.OnAuthVaultNodeChanged(nodeID, revUuid);

            if (!fNodes.ContainsKey(nodeID)) return;

            //No TransID, so we will figure out what we need to update ourselves
            BeginInvoke(new Action<uint>(IUpdateNode), new object[] { nodeID });
        }

        protected override void OnAuthVaultNodeFetched(uint transID, ENetError result, byte[] data) {
            base.OnAuthVaultNodeFetched(transID, result, data);

            VaultNode node = VaultNode.Parse(data);
            if (fNodes.ContainsKey(node.ID)) fNodes[node.ID] = node;
            else fNodes.Add(node.ID, node);
        }

        protected override void OnAuthVaultNodeRemoved(uint parentID, uint childID) {
            base.OnAuthVaultNodeRemoved(parentID, childID);

            if (!fVaultTree.ContainsKey(parentID)) return;
            fVaultTree[parentID].Remove(childID);

            //No TransID, so we'll figure out what to do ourselves
            FetchNode(childID, new Action<VaultNode, uint, uint>(IRemoveFromPanes), new object[] { parentID, childID });
        }

        protected override void OnAuthVaultTreeFetched(uint transID, ENetError result, VaultNodeRef[] refs) {
            base.OnAuthVaultTreeFetched(transID, result, refs);

            foreach (VaultNodeRef nRef in refs) {
                if (!fVaultTree.ContainsKey(nRef.fParentIdx))
                    fVaultTree.Add(nRef.fParentIdx, new List<uint>());
                if (!fVaultTree[nRef.fParentIdx].Contains(nRef.fChildIdx)) {
                    LogDebug(String.Format("NodeRef [PARENT: {0}] [CHILD: {1}] [SAVER: {2}]", nRef.fParentIdx, nRef.fChildIdx, nRef.fSaverIdx));
                    fVaultTree[nRef.fParentIdx].Add(nRef.fChildIdx);
                }

                //Is this a "core node" ?
                if (nRef.fParentIdx == fActivePlayer)
                    FetchNode(nRef.fChildIdx, new Action<VaultNode>(IAddFolderToPanes), new object[0]);
            }
        }
        #endregion

        #region Menu Events
        private void IAboutMe(object sender, EventArgs e) {
            AboutBox abox = new AboutBox();
            abox.Show(this);
        }

        private void IConnect(object sender, EventArgs e) {
            ConnectForm cf = new ConnectForm();
            cf.Parent = this;
            if (Prefrences.RememberLogin) {
                cf.AutoConnect = Prefrences.AutoConnect;
                cf.CanAutoConnect = (sender == null ? true : false);
                cf.Password = Prefrences.Password;
                cf.RememberMe = true;
                cf.Username = Prefrences.Username;
                cf.WantShard = Prefrences.Shard;
            }

            if (cf.ShowDialog(this) == DialogResult.OK) {
                fConnectMenuItem.Enabled = false;
                fDisconnectMenuItem.Enabled = true;

                //Set the registry prefs from the ConnectForm
                Prefrences.AutoConnect = cf.AutoConnect;
                Prefrences.Password = cf.Password;
                Prefrences.RememberLogin = cf.RememberMe;
                Prefrences.Shard = cf.WantShard;
                Prefrences.Username = cf.Username;

                //Let's remember the account uuid
                fAcctUuid = cf.AcctUUID;

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
            LogInfo("Disconnected");
            AuthCli.Disconnect();

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
                LogWarn(String.Format("Attempted to add #{0} to buddies list, but he is already there!", obj));
                return;
            }

            VaultPlayerInfoNode search = new VaultPlayerInfoNode();
            search.PlayerID = obj;

            //Do the search
            uint trans = AuthCli.FindVaultNode(search.BaseNode.ToArray());
            RegisterAuthCB(trans, new Action<uint[], uint>(IAddBuddy), new object[] { obj });
        }

        private void IAddBuddy(uint[] nodeIDs, uint buddy) {
            if (nodeIDs.Length == 0) {
                MessageBox.Show(this, String.Format("KI #{0} is not on the lattice", buddy), "Not Found", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                LogError(String.Format("Attempted to add buddy {0}, but he does not exist!", buddy));
                return;
            }

            AuthCli.AddVaultNode(fBaseNodes[EStandardNode.kBuddyListFolder], nodeIDs[0], fActivePlayer);
        }

        private void IAvatarSelected(object sender, EventArgs e) {
            Player p = IGetPlayer();
            if (fActivePlayer == p.fID)
                //Don't redownload the current player...
                return;

            IThrowAway();

            if (p.ToString() == "NULL") return;
            LogInfo(String.Format("Set Active Player [ID: {0}] [NAME: {1}]", p.fID.ToString(), p.fName));
            AuthCli.SetActivePlayer(p.fID);
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
                LogInfo(String.Format("Selecting [TAB: {0}]", tag));
                return;
            } else if (e.Action != TabControlAction.Selected) return;

            if (tag == "buddies")
                if (fBaseNodes.ContainsKey(EStandardNode.kBuddyListFolder))
                    fBuddyCtrl.SetFolder(fBaseNodes[EStandardNode.kBuddyListFolder]);

            if (tag == "kimail")
                if (fBaseNodes.ContainsKey(EStandardNode.kAgeJournalsFolder) && 
                    fBaseNodes.ContainsKey(EStandardNode.kInboxFolder))
                    fKiMailCtrl.SetFolder(fBaseNodes[EStandardNode.kAgeJournalsFolder], fBaseNodes[EStandardNode.kInboxFolder]);

            if (tag == "neighbors")
                if (fBaseNodes.ContainsKey(EStandardNode.kHoodMembersFolder))
                    fNeighborsCtrl.SetFolder(fBaseNodes[EStandardNode.kHoodMembersFolder]);

            if (tag == "publicages")
                if (AuthCli.Connected) { //Don't fail ;)
                    fPublicAgesCtrl.RefreshAgeList();
                    fAgeRefresh.Tick += new EventHandler(IRefreshAges);
                    fAgeRefresh.Interval = 300000; //Five minutes
                    fAgeRefresh.Start();
                }

            if (tag == "recents")
                if (fBaseNodes.ContainsKey(EStandardNode.kPeopleIKnowAboutFolder))
                    fRecentsCtrl.SetFolder(fBaseNodes[EStandardNode.kPeopleIKnowAboutFolder]);
        }
        #endregion

        #region UNSORTED Methods
        private void IAddAvatar(string name, uint idx) {
            Player p = new Player(name, idx);
            fAvatarSelector.Items.Add(p);
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

            if (node.NodeType == ENodeType.kNodeImage || node.NodeType == ENodeType.kNodeTextNote)
                fKiMailCtrl.AddNode(node, parentID);
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
                    FetchChildren(node.ID, new Action<VaultNode>(IAddFolderToPanes));
                }
            }

            //Grab the AgeInfo...
            if (node.NodeType == ENodeType.kNodeAgeLink)
                FetchChildren(node.ID, new Action<VaultNode>(IAddFolderToPanes));

            //See if this is the Neighborhood or Relto
            if (node.NodeType == ENodeType.kNodeAgeInfo) {
                VaultAgeInfoNode ageinfo = new VaultAgeInfoNode(node);
                if (ageinfo.Filename == "Neighborhood") {
                    //Yep! Grab children :)
                    FetchChildren(node.ID, new Action<VaultNode>(IAddFolderToPanes));
                } 
                
                //HACK!!! Change back to PERSONAL asap!
                if (ageinfo.Filename == "Neighborhood") {
                    //Stash this node! Oh, and YES this is a strange label, but whatev.
                    fBaseNodes.Add(EStandardNode.kAgeInfoNode, ageinfo.ID);
                }
            }

            //Are these KI Mail Folders?
            if (node.NodeType == ENodeType.kNodeFolder) {
                VaultFolderNode folder = new VaultFolderNode(node);
                if (folder.FolderType == EStandardNode.kAgeJournalsFolder) fBaseNodes.Add(EStandardNode.kAgeJournalsFolder, node.ID);
                if (folder.FolderType == EStandardNode.kInboxFolder) fBaseNodes.Add(EStandardNode.kInboxFolder, node.ID);

            }

            //Is this my PlayerInfo?
            if (node.NodeType == ENodeType.kNodePlayerInfo) {
                VaultPlayerInfoNode info = new VaultPlayerInfoNode(node);
                if (info.PlayerID == fActivePlayer) fBaseNodes.Add(EStandardNode.kPlayerInfoNode, info.ID);
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
            LogError(String.Format("KICKED OFF [REASON: {0}]", temp));

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

        private void IRefreshAges(object sender, EventArgs e) {
            if (fTabControl.SelectedTab.Tag.Equals("publicages"))
                fPublicAgesCtrl.RefreshAgeList();
        }

        private void IRemoveFromPanes(VaultNode node, uint parentID, uint childID) {
            if (fBaseNodes[EStandardNode.kBuddyListFolder] == parentID)
                fBuddyCtrl.RemoveNode(node);
            if (fBaseNodes[EStandardNode.kHoodMembersFolder] == parentID)
                fNeighborsCtrl.RemoveNode(node);
            if (fBaseNodes[EStandardNode.kPeopleIKnowAboutFolder] == parentID)
                fRecentsCtrl.RemoveNode(node);
            if (fBaseNodes[EStandardNode.kAgeJournalsFolder] == parentID)
                fKiMailCtrl.RemoveFolder(node);

            if (node.NodeType == ENodeType.kNodeImage || node.NodeType == ENodeType.kNodeTextNote)
                fKiMailCtrl.RemoveKiItem(node);
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
            fKiMailCtrl.Clear();
            fNeighborsCtrl.Clear();
            fPublicAgesCtrl.Clear();
            fRecentsCtrl.Clear();
        }

        private void IUpdateNode(uint idx) {
            uint trans = AuthCli.FetchVaultNode(idx);
            RegisterAuthCB(trans, new Action<VaultNode>(IUpdateNode));
        }

        private void IUpdateNode(VaultNode node) {
            switch (node.NodeType) {
                case ENodeType.kNodeImage:
                case ENodeType.kNodeMarkerList:
                case ENodeType.kNodeTextNote:
                    fKiMailCtrl.UpdateNode(node);
                    break;
                case ENodeType.kNodePlayerInfo:
                    VaultPlayerInfoNode info = new VaultPlayerInfoNode(node);
                    bool alerted = IDoBuddyUpdate(info, false);
                    IDoNeighborUpdate(info, alerted);
                    fRecentsCtrl.UpdateNode(info);
                    break;
            }
        }
        #endregion
    }
}
