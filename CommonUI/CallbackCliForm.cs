using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MUd {
    public delegate void Action<T1, T2, T3, T4, T5>(T1 o1, T2 o2, T3 o3, T4 o4, T5 o5);
    public delegate void Action<T1, T2, T3, T4, T5, T6>(T1 o1, T2 o2, T3 o3, T4 o4, T5 o5, T6 o6);

    public partial class CallbackCliForm : Form {
        struct Callback {
            public Delegate fFunc;
            public object[] fMyArgs;

            public Callback(Delegate func) {
                fFunc = func;
                fMyArgs = new object[0];
            }

            public Callback(Delegate func, object[] args) {
                fFunc = func;
                fMyArgs = args;
            }
        }

        private AuthClient fAuthCli = new AuthClient();
        private FileClient fFileCli = new FileClient();
        private GameClient fGameCli = new GameClient();
        private GateClient fGateCli = new GateClient();

        private LogProcessor fLogger;

        private Dictionary<uint, Callback> fAuthCBs = new Dictionary<uint, Callback>();
        private Dictionary<uint, Callback> fFileCBs = new Dictionary<uint, Callback>();
        private Dictionary<uint, Callback> fGameCBs = new Dictionary<uint, Callback>();
        private Dictionary<uint, Callback> fGateCBs = new Dictionary<uint, Callback>();

        public AuthClient AuthCli {
            get { return fAuthCli; }
            set { fAuthCli = value; }
        }

        public FileClient FileCli {
            get { return fFileCli; }
            set { fFileCli = value; }
        }

        public GameClient GameCli {
            get { return fGameCli; }
            set { fGameCli = value; }
        }

        public GateClient GateCli {
            get { return fGateCli; }
            set { fGateCli = value; }
        }

        public CallbackCliForm() {
            InitializeComponent();

            //Auth Client
            fAuthCli.GotAge += new AuthAgeReply(OnAuthGotAge);
            fAuthCli.GotPublicAges += new AuthGotPublicAges(OnAuthGotPublicAges);
            fAuthCli.KickedOff += new AuthKickedOff(OnAuthKickedOff);
            fAuthCli.LoggedIn += new AuthLoggedIn(OnAuthLoggedIn);
            fAuthCli.PlayerInfo += new AuthPlayerInfo(OnAuthPlayerInfo);
            fAuthCli.PlayerSet += new AuthResult(OnAuthPlayerSet);
            fAuthCli.VaultNodeAdded += new AuthVaultNodeAdded(OnAuthVaultNodeAdded);
            fAuthCli.VaultNodeChanged += new AuthVaultNodeChanged(OnAuthVaultNodeChanged);
            fAuthCli.VaultNodeFetched += new AuthVaultNodeFetched(OnAuthVaultNodeFetched);
            fAuthCli.VaultNodeFound += new AuthVaultNodeFound(OnAuthVaultNodeFound);
            fAuthCli.VaultNodeRemoved += new AuthVaultNodeRemoved(OnAuthVaultNodeRemoved);
            fAuthCli.VaultNodeRemoveReply += new AuthResult(OnAuthVaultNodeRemoveReply);
            fAuthCli.VaultNodeSaveReply += new AuthResult(OnAuthVaultNodeSaveReply);
            fAuthCli.VaultTreeFetched += new AuthVaultTreeFetched(OnAuthVaultTreeFetched);

            //File Client
            fFileCli.GotBuildID += new FileBuildIdReply(OnFileGotBuildID);

            //Game Client
            fGameCli.AgeJoined += new GameAgeJoined(OnGameAgeJoined);
            fGameCli.BufferPropagated += new GameRawBuffer(OnGameBufferPropagated);

            //Gate Client
            fGateCli.GotFileSrvIP += new GateIP(OnGateGotFileSrvIP);
        }

        #region AuthCli Event Handlers
        protected virtual void OnAuthGotAge(uint transID, ENetError result, Guid instance, uint mcpid, uint vaultID, System.Net.IPAddress gameSrv) {
            //Fire callback
            // - Method: ISomething(ENetError result, Guid instance, uint mcpid, uint vaultID, IPAddress gameSrv ...)
            IFireAuthCallback(transID, new object[] { result, instance, mcpid, vaultID, gameSrv });
        }

        protected virtual void OnAuthGotPublicAges(uint transID, ENetError result, NetAgeInfo[] ages) {
            //Fire callback
            // - Method: ISomething(NetAgeInfo[] ages, ...)
            IFireAuthCallback(transID, new object[] { ages });
        }

        protected virtual void OnAuthKickedOff(ENetError reason) { }

        protected virtual void OnAuthLoggedIn(uint transID, ENetError result, uint flags, uint[] droidKey, uint billing, Guid acctUuid) {
            //Fire callback
            // - Method: ISomething(ENetError result, uint flags, uint[] droidKey, Guid acctUuid, ...)
            IFireAuthCallback(transID, new object[] { result, flags, droidKey, acctUuid });
        }

        protected virtual void OnAuthPlayerInfo(uint transID, string name, uint idx, string shape, uint explorer) { }

        protected virtual void OnAuthPlayerSet(uint transID, ENetError result) {
            //Fire callback
            // - Method: ISomething(ENetError result, ...)
            IFireAuthCallback(transID, new object[] { result });
        }

        protected virtual void OnAuthVaultNodeAdded(uint parentID, uint childID, uint saverID) { }
        protected virtual void OnAuthVaultNodeChanged(uint nodeID, Guid revUuid) { }

        protected virtual void OnAuthVaultNodeFetched(uint transID, ENetError result, byte[] data) {
            //Fire callback
            // - Method: ISomething(VaultNode fetched);
            IFireAuthCallback(transID, new object[] { VaultNode.Parse(data) });
        }

        protected virtual void OnAuthVaultNodeFound(uint transID, ENetError result, uint[] nodeIDs) {
            //Simply fire the callback...
            //We don't know what the caller wants these nodes for anyway...
            // - Method: ISomething(uint[] nodeIDs, ...)
            IFireAuthCallback(transID, new object[] { nodeIDs });
        }

        protected virtual void OnAuthVaultNodeRemoved(uint parentID, uint childID) { }

        protected void OnAuthVaultNodeRemoveReply(uint transID, ENetError result) {
            //Fire callback
            // - Method: ISomething(ENetError result, ...)
            IFireAuthCallback(transID, new object[] { result });
        }

        protected void OnAuthVaultNodeSaveReply(uint transID, ENetError result) {
            //Fire callback
            // - Method: ISomething(ENetError result, ...)
            IFireAuthCallback(transID, new object[] { result });
        }

        protected virtual void OnAuthVaultTreeFetched(uint transID, ENetError result, VaultNodeRef[] refs) {
            //Fire callback
            // - Method: ISomething(VaultNodeRef[] refs, ...)
            IFireAuthCallback(transID, new object[] { refs });
        }
        #endregion

        #region File Client Event Handlers
        protected virtual void OnFileGotBuildID(uint transID, ENetError result, uint buildID) {
            //Fire callback
            // - Method ISomething(uint buildID, ...)
            IFireFileCallback(transID, new object[] { buildID });
        }
        #endregion

        #region Game Client Event Handlers
        protected virtual void OnGameAgeJoined(uint transID, ENetError result) {
            //Fire callback
            // - Method: ISomething(ENetError result, ..)
            IFireGameCallback(transID, new object[] { result });
        }

        protected virtual void OnGameBufferPropagated(NetMessage msg, bool handled) {
        }
        #endregion

        #region Gate Client Event Handlers
        protected virtual void OnGateGotFileSrvIP(uint transID, string ip) {
            //Fire callback
            // - Method ISomething(string ip, ...)
            IFireGateCallback(transID, new object[] { ip });
        }
        #endregion

        #region Callback Helpers
        protected object[] CreateArgArray(object[] myArgs, object[] args) {
            //Create the list of arguemnts
            // - Method: ISomething([cb args], [custom args])
            List<object> lArgs = new List<object>(args);
            foreach (object arg in myArgs)
                lArgs.Add(arg);
            return lArgs.ToArray();
        }

        private void IFireAuthCallback(uint transID, params object[] args) {
            if (fAuthCBs.ContainsKey(transID)) {
                Callback c = fAuthCBs[transID];
                fAuthCBs.Remove(transID); //Delete it

                //Run CB on the same thread as the form
                BeginInvoke(c.fFunc, CreateArgArray(c.fMyArgs, args));
            }
        }

        private void IFireFileCallback(uint transID, params object[] args) {
            if (fFileCBs.ContainsKey(transID)) {
                Callback c = fFileCBs[transID];
                fFileCBs.Remove(transID); //Delete it

                //Run CB on the same thread as the form
                BeginInvoke(c.fFunc, CreateArgArray(c.fMyArgs, args));
            }
        }

        private void IFireGameCallback(uint transID, params object[] args) {
            if (fGameCBs.ContainsKey(transID)) {
                Callback c = fGameCBs[transID];
                fGameCBs.Remove(transID); //Delete it

                //Run CB on the same thread as the form
                BeginInvoke(c.fFunc, CreateArgArray(c.fMyArgs, args));
            }
        }

        private void IFireGateCallback(uint transID, params object[] args) {
            if (fGateCBs.ContainsKey(transID)) {
                Callback c = fGateCBs[transID];
                fGateCBs.Remove(transID); //Delete it

                //Run CB on the same thread as the form
                BeginInvoke(c.fFunc, CreateArgArray(c.fMyArgs, args));
            }
        }
        #endregion

        #region Callback Public Stuff
        public void RegisterAuthCB(uint transID, Delegate func, object[] args) {
            fAuthCBs.Add(transID, new Callback(func, args));
        }

        public void RegisterAuthCB(uint transID, Delegate func) {
            fAuthCBs.Add(transID, new Callback(func, new object[0]));
        }

        public void RegisterFileCB(uint transID, Delegate func, object[] args) {
            fFileCBs.Add(transID, new Callback(func, args));
        }

        public void RegisterFileCB(uint transID, Delegate func) {
            fFileCBs.Add(transID, new Callback(func, new object[0]));
        }

        public void RegisterGameCB(uint transID, Delegate func, object[] args) {
            fGameCBs.Add(transID, new Callback(func, args));
        }

        public void RegisterGameCB(uint transID, Delegate func) {
            fGameCBs.Add(transID, new Callback(func, new object[0]));
        }

        public void RegisterGateCB(uint transID, Delegate func) {
            fGateCBs.Add(transID, new Callback(func, new object[0]));
        }

        public void RegisterGateCB(uint transID, Delegate func, object[] args) {
            fGateCBs.Add(transID, new Callback(func, args));
        }
        #endregion

        #region Log Methods
        protected void EnableLogging(string logname) {
            if (fLogger == null)
                fLogger = new LogProcessor(logname);
        }

        public void LogDebug(string line) {
            if (fLogger != null)
                fLogger.Debug(line);
        }

        public void LogError(string line) {
            if (fLogger != null)
                fLogger.Error(line);
        }

        public void LogInfo(string line) {
            if (fLogger != null)
                fLogger.Info(line);
        }

        public void LogVerbose(string line) {
            if (fLogger != null)
                fLogger.Verbose(line);
        }

        public void LogWarn(string line) {
            if (fLogger != null)
                fLogger.Warn(line);
        }
        #endregion
    }
}
