using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Timers;
using MySql.Data.MySqlClient;
using OpenSSL;

namespace MUd {
    public partial class AuthThread : Srv2CliBase {
        const int kAuthHeaderSize = 20;

        Dictionary<uint, Queue<Chunk>> fQueuedChunks = new Dictionary<uint, Queue<Chunk>>();
        Dictionary<uint, uint> fFileSizes = new Dictionary<uint, uint>();

        List<uint> fPlayers = new List<uint>();
        Guid fAcctUUID = Guid.Empty;
        uint[] fDroidKey = new uint[4];
        uint fChallenge;
        uint fActivePlayer = 0;
        bool fLoggedIn = false;

        AuthServer fParent;
        LookupClient fLookupCli;
        VaultClient fVaultCli;
        DbConnection fDB;

        public AuthThread(AuthServer parent, Socket s, ConnectHeader hdr, LogProcessor log)
            : base(s, hdr, log) {
            fParent = parent;

            fLookupCli = new LookupClient();
            fLookupCli.SetIdleBehavior(IdleBehavior.Ping, 180000);
            fLookupCli.Host = Configuration.GetString("lookup_addr", "127.0.0.1");
            fLookupCli.Port = Configuration.GetInteger("lookup_port", 14617);
            fLookupCli.ProductID = Configuration.GetGuid("auth_guid");
            fLookupCli.Token = Configuration.GetGuid("lookup_token");

            fLookupCli.AgeFound += new LookupAgeFound(ILookupAgeFound);

            fVaultCli = new VaultClient();
            fVaultCli.SetIdleBehavior(IdleBehavior.Ping, 180000);
            fVaultCli.Host = Configuration.GetString("vault_addr", "127.0.0.1");
            fVaultCli.Port = Configuration.GetInteger("vault_port", 14617);
            fVaultCli.ProductID = Configuration.GetGuid("auth_guid");
            fVaultCli.Token = Configuration.GetGuid("vault_token");

            fVaultCli.AgeCreated += new VaultAgeCreated(IVaultOnAgeCreated);
            fVaultCli.NodeAdded += new VaultNodeAdded(IVaultOnNodeAdded);
            fVaultCli.NodeAddReply += new VaultResult(IVaultOnNodeAddReply);
            fVaultCli.NodeChanged += new VaultNodeChanged(IVaultOnNodeChanged);
            fVaultCli.NodeCreated += new VaultNodeCreated(IVaultOnNodeCreated);
            fVaultCli.NodeFetched += new VaultNodeFetched(IVaultOnNodeFetched);
            fVaultCli.NodeFound += new VaultNodeFound(IVaultOnNodeFound);
            fVaultCli.NodeRefsFetched += new VaultNodeRefsFetched(IVaultOnNodeRefsFetched);
            fVaultCli.NodeSaved += new VaultResult(IVaultOnNodeSaved);
            fVaultCli.PlayerCreated += new VaultPlayerCreated(IVaultOnPlayerCreate);
            fVaultCli.Pong += new VaultPong(IVaultOnPong);

            try {
                fDB = Database.Connect();
            } catch (DbException) {
                Error("Failed to connect to DATABASE...");
                Stop();
            }
        }

        public override void Start() {
            UruStream s = new UruStream(new NetworkStream(fSocket, false));

            //Eat AuthConnectHeader
            int size = s.ReadInt();
            if (size != kAuthHeaderSize) Warn("Invalid auth header size!");
            s.ReadBytes(size - 4);

            //NetCliConnect
            byte[] y_data = null;
            if (s.ReadByte() != (byte)NetCliConnectMsg.kNetCliConnect) {
                Error("FATAL: Invalid NetCliConnect");
                Stop();
            } else {
                size = (int)s.ReadByte();
                y_data = s.ReadBytes(size - 2);

                if (y_data.Length > 64) {
                    Warn("YData too big. Truncating.");
                    byte[] old = y_data;
                    y_data = new byte[64];
                    Buffer.BlockCopy(old, 0, y_data, 0, 64);
                }
            }

            //Handoff
            if (!ISetupEncryption("Auth", y_data)) {
                Error("Cannot setup encryption keys!");
                Stop();
                return;
            }

            //Send the NetCliEncrypt response
            s.BufferWriter();
            s.WriteByte((byte)NetCliConnectMsg.kNetCliEncrypt);
            s.WriteByte(9);
            s.WriteBytes(fServerSeed);
            s.FlushWriter();

            //Verify the DB is connected.
            //If there is no DB connection, kick the client.
            if (fDB == null) {
                Warn("Database link is NULL. Kicking client...");
                IKickClient(ENetError.kNetErrInternalError);
                return;
            }

            //Begin receiving data
            fSocket.BeginReceive(new byte[2], 0, 2, SocketFlags.Peek, new AsyncCallback(IReceiveFromUru), null);
            s.Close();
        }

        private ENetError ICanDoStuff() {
            ENetError err = ENetError.kNetSuccess;
            if (!fLoggedIn) err = ENetError.kNetErrAuthenticationFailed;
            else if (fActivePlayer == 0) err = ENetError.kNetErrPlayerNotFound;
            return err;
        }

        private void IReceiveFromUru(IAsyncResult ar) {
            try {
                fSocket.EndReceive(ar);

                lock (fStream) {
                    AuthCli2Srv msg = AuthCli2Srv.PingRequest;
                    try {
                        msg = (AuthCli2Srv)fStream.ReadUShort();
                    } catch (IOException) {
                        Debug("[UruCli] Disconnected");
                    }

                    switch (msg) {
                        case AuthCli2Srv.AcctLoginRequest:
                            ILogin();
                            break;
                        case AuthCli2Srv.AcctSetPlayerRequest:
                            ISetActivePlayer();
                            break;
                        case AuthCli2Srv.AgeRequest:
                            IFindAge();
                            break;
                        case AuthCli2Srv.ClientRegisterRequest:
                            IRegisterClient();
                            break;
                        case AuthCli2Srv.FileDownloadChunkAck:
                            IAckChunk();
                            break;
                        case AuthCli2Srv.FileDownloadRequest:
                            ISendFile();
                            break;
                        case AuthCli2Srv.FileListRequest:
                            IListFiles();
                            break;
                        case AuthCli2Srv.LogClientDebuggerConnect:
                            IDebuggerConnect();
                            break;
                        case AuthCli2Srv.LogPythonTraceback:
                            ILogPythonTraceback();
                            break;
                        case AuthCli2Srv.LogStackDump:
                            ILogStackDump();
                            break;
                        case AuthCli2Srv.PingRequest:
                            IPingPong();
                            break;
                        case AuthCli2Srv.PlayerCreateRequest:
                            ICreatePlayer();
                            break;
                        case AuthCli2Srv.VaultFetchNodeRefs:
                            IFetchNodeRefs();
                            break;
                        case AuthCli2Srv.VaultInitAgeRequest:
                            ICreateAge();
                            break;
                        case AuthCli2Srv.VaultNodeAdd:
                            IAddNode();
                            break;
                        case AuthCli2Srv.VaultNodeCreate:
                            ICreateNode();
                            break;
                        case AuthCli2Srv.VaultNodeFetch:
                            IFetchNode();
                            break;
                        case AuthCli2Srv.VaultNodeFind:
                            IFindNode();
                            break;
                        case AuthCli2Srv.VaultNodeSave:
                            ISaveNode();
                            break;
                        default:
                            string test = Enum.GetName(typeof(AuthCli2Srv), msg);
                            if (test != null && test != String.Empty) {
                                string type = "Cli2Auth_" + test;
                                if (type == null) type = "0x" + msg.ToString("X");
                                Error("Unimplemented message type " + type);
                                IKickClient(ENetError.kNetErrInternalError);
                                Stop();
                            } else {
                                Warn(String.Format("Received garbage... [0x{0}] Expected: MSG_TYPE", msg.ToString("X")));
                                Stop();
                            }

                            break;
                    }
                }

                fSocket.BeginReceive(new byte[2], 0, 2, SocketFlags.Peek, new AsyncCallback(IReceiveFromUru), null);
            } catch (SocketException e) {
                IHandleSocketException(e);
            } catch (IOException) {
                Debug("[UruCli] Disconnected");
                Stop();
            } catch (ObjectDisposedException) { }
        }

        #region Auth Transactions
        private void IAckChunk() {
            Auth_FileDownloadChunkAck ack = new Auth_FileDownloadChunkAck();
            ack.Read(fStream);
            ISendChunk(ack.fTransID);
        }

        private void IDebuggerConnect() {
            Auth_DebuggerAttached debugger = new Auth_DebuggerAttached();
            debugger.Read(fStream);

            if (!Configuration.GetBoolean("allow_debugger", false)) {
                Warn("Debugger attached: Auto-Kick.");
                IKickClient(ENetError.kNetErrKickedByCCR);
            } else {
                Debug("Debugger attached.");
            }
        }

        private void IFindAge() {
            Auth_AgeRequest req = new Auth_AgeRequest();
            req.Read(fStream);

            ENetError err = ICanDoStuff();
            if (err == ENetError.kNetSuccess) {
                //   ---Find Age Process---
                //Step #1: Find the AgeNode in the vault
                VaultAgeNode age = new VaultAgeNode();
                age.AgeName = req.fAgeName;
                age.Instance = req.fAgeInstanceUuid;

                uint trans = fVaultCli.FindNode(age.BaseNode.ToArray());
                lock (fVaultToAuthMap)
                    fVaultToAuthMap.Add(trans, req.fTransID);
                lock (fVaultTransTags)
                    fVaultTransTags.Add(trans, new AgeTag(req.fAgeName, req.fAgeInstanceUuid));
            } else {
                Error(String.Format("Tried to Find an Age, but we can't do stuff! [REASON: {0}]", err.ToString().Substring(4)));

                Auth_AgeReply reply = new Auth_AgeReply();
                reply.fResult = err;
                reply.fTransID = req.fTransID;

                fStream.BufferWriter();
                fStream.WriteUShort((ushort)AuthSrv2Cli.AgeReply);
                reply.Write(fStream);
                fStream.FlushWriter();
            }
        }

        private void IKickClient(ENetError reason) {
            Auth_KickedOff kick = new Auth_KickedOff();
            kick.fReason = reason;

            fStream.WriteUShort((ushort)AuthSrv2Cli.KickedOff);
            kick.Write(fStream);

            Stop();
        }

        private void ILogPythonTraceback() {
            Auth_LogDump dump = new Auth_LogDump();
            dump.Read(fStream);

            Warn("LOGGED: Python Traceback");
            fLog.DumpToLog(dump.fErrorMsg, ELogType.kLogWarning);
        }

        private void ILogStackDump() {
            Auth_LogDump dump = new Auth_LogDump();
            dump.Read(fStream);

            Warn("LOGGED: Stack Dump");
            fLog.DumpToLog(dump.fErrorMsg, ELogType.kLogWarning);
        }

        private void IListFiles() {
            Auth_FileListRequest req = new Auth_FileListRequest();
            req.Read(fStream);

            //Security... Do not let the user escape our jail.
            //Also, don't let anything weird happen with the extension
            req.fDirectory = req.fDirectory.Replace("../", null);
            req.fExtension = req.fExtension.Replace("/", null);

            Debug(String.Format("FileListReq: '{0}' (*.{1})", req.fDirectory, req.fExtension));

            //Begin setting up the reply
            Auth_FileListReply reply = new Auth_FileListReply();
            reply.fResult = ENetError.kNetPending;
            reply.fTransID = req.fTransID;

            string dir = Path.Combine(Configuration.GetString("preloader_source", "G:\\Plasma\\Servers\\Secure Preloader"), req.fDirectory);
            if (Directory.Exists(dir)) {
                string[] files = Directory.GetFiles(dir, "*." + req.fExtension);
                AuthManifest mfs = new AuthManifest(req.fDirectory, files);
                reply.fData = mfs.ToArray();
                reply.fResult = ENetError.kNetSuccess;
            } else {
                Warn("FileListReq: Directory (" + req.fDirectory + ") does not exist!");
                reply.fResult = ENetError.kNetErrFileNotFound;
            }

            fStream.WriteUShort((ushort)AuthSrv2Cli.FileListReply);
            reply.Write(fStream);
        }

        private void ILogin() {
            Auth_LoginRequest req = new Auth_LoginRequest();
            req.Read(fStream);

            ENetError err = ENetError.kNetPending;
            if (!fVaultCli.Connected) {
                if (fVaultCli.Connect()) {
                    Debug("Connection to vault established!");
                } else {
                    Error("Couldn't connect to VAULT...");
                    err = ENetError.kNetErrInternalError;
                }
            }

            if (err == ENetError.kNetPending) {
                StringBuilder sb = new StringBuilder(req.fHash.UruHash.Length * 4);
                foreach (uint b in req.fHash.UruHash)
                    sb.AppendFormat("{0:x8}", b);
                string hash = sb.ToString();
                Info(String.Format("Login Attempt! [OS: {0}] [USER: {1}] [HASH: {2}]", req.fOS, req.fAccount, hash));

                SelectStatement sel = new SelectStatement();
                sel.Limit = 1;
                sel.Table = "Accounts";
                sel.Where.Add("Name", req.fAccount);
                sel.Wildcard = true;

                MySqlDataReader r = sel.ExecuteQuery(fDB);
                if (r.Read()) {
                    if ((AcctPrivLevel)r.GetByte("PrivLevel") == AcctPrivLevel.Banned) {
                        err = ENetError.kNetErrAccountBanned;
                        Warn("Login Failed: Account Banned.");
                    } else if ((int)r.GetByte("PrivLevel") < Configuration.GetEnumInteger("restrict_login", AcctPrivLevel.Normal, typeof(AcctPrivLevel))) {
                        err = ENetError.kNetErrLoginDenied;
                        Warn("Login Failed: PrivLevel too low.");
                    } else if (hash.ToLower() == r.GetString("HashedPassword").ToLower()) {
                        err = ENetError.kNetSuccess;
                        fAcctUUID = new Guid(r.GetString("AcctUUID"));
                        Info("Login success!");
                    } else {
                        err = ENetError.kNetErrAuthenticationFailed;
                        Warn("Login Failed: Invalid password.");
                    }
                } else {
                    err = ENetError.kNetErrAccountNotFound;
                    Warn("Login Failed: Account not found.");
                }

                if (err == ENetError.kNetSuccess) {
                    fLoggedIn = true;
                    fDroidKey[0] = Configuration.GetUInteger("data_key01", 0x47612f39);
                    fDroidKey[1] = Configuration.GetUInteger("data_key02", 0x417a2cbf);
                    fDroidKey[2] = Configuration.GetUInteger("data_key03", 0x882941c2);
                    fDroidKey[3] = Configuration.GetUInteger("data_key04", 0x301d4c9a);
                }

                r.Close();
            }

            Auth_LoginReply reply = new Auth_LoginReply();
            if (err == ENetError.kNetSuccess) reply.fAcctGuid = fAcctUUID;
            reply.fBillingType = 1;
            reply.fDroidKey = fDroidKey;
            reply.fFlags = 0;
            reply.fResult = err;
            reply.fTransID = req.fTransID;

            if (err == ENetError.kNetSuccess) {
                SelectStatement sel = new SelectStatement();
                sel.Limit = 5;
                sel.Table = "Players";
                sel.Where.Add("AcctUUID", fAcctUUID.ToString().ToLower());
                sel.Wildcard = true;

                MySqlDataReader r = sel.ExecuteQuery(fDB);
                while (r.Read()) {
                    Auth_PlayerInfo player = new Auth_PlayerInfo();
                    player.fExplorer = 1;
                    player.fModel = r.GetString("Model");
                    player.fPlayerID = r.GetUInt32("NodeIdx");
                    player.fPlayerName = r.GetString("Name");
                    player.fTransID = req.fTransID;

                    fPlayers.Add(player.fPlayerID);
                    Debug(String.Format("Sending PlayerInfo [ID: {0}] [SHAPE: {1}] [NAME: {2}]", player.fPlayerID, player.fModel, player.fPlayerName));

                    fStream.WriteUShort((ushort)AuthSrv2Cli.AcctPlayerInfo);
                    player.Write(fStream);
                }

                r.Close();
            }

            fStream.WriteUShort((ushort)AuthSrv2Cli.AcctLoginReply);
            reply.Write(fStream);
        }

        private void IPingPong() {
            //Just read the ping data and bounce it back to the client...
            Auth_PingPong ping = new Auth_PingPong();
            ping.Read(fStream);

            fStream.WriteUShort((ushort)AuthSrv2Cli.PingReply);
            ping.Write(fStream);
            Verbose("[UruCli] PING?");
        }

        private void IRegisterClient() {
            Auth_RegisterRequest reg = new Auth_RegisterRequest();
            reg.Read(fStream);

            //Verify BuildID for obvious, stupid trickery
            if (reg.fBuildID != fConn.fBuildID) {
                Error(String.Format("BuildID mismatch! Header: [{0}] vs RegisterReq: [{1}]", fConn.fBuildID, reg.fBuildID));
                IKickClient(ENetError.kNetErrOldBuildId);
                return;
            }

            Auth_RegisterReply reply = new Auth_RegisterReply();
            reply.fChallenge = fChallenge;
            fChallenge = BitConverter.ToUInt32(RNG.Random(4), 0);

            //Actually send reply
            fStream.WriteUShort((ushort)AuthSrv2Cli.ClientRegisterReply);
            reply.Write(fStream);
        }

        private void ISendChunk(uint transID) {
            if (!fQueuedChunks.ContainsKey(transID)) return;
            if (fQueuedChunks[transID].Count == 0) {
                fQueuedChunks.Remove(transID);
                fFileSizes.Remove(transID);
                return;
            }

            Chunk c = fQueuedChunks[transID].Dequeue();

            Auth_FileDownloadChunk fdc = new Auth_FileDownloadChunk();
            fdc.fChunkData = c.fChunk;
            fdc.fChunkPos = c.fPos;
            fdc.fFileSize = fFileSizes[transID];
            fdc.fResult = ENetError.kNetSuccess;
            fdc.fTransID = transID;

            fStream.BufferWriter();
            fStream.WriteUShort((ushort)AuthSrv2Cli.FileDownloadChunk);
            fdc.Write(fStream);
            fStream.FlushWriter();
        }

        private void ISendFile() {
            Auth_FileDownloadRequest req = new Auth_FileDownloadRequest();
            req.Read(fStream);

            //Security
            req.fDownload = req.fDownload.Replace("../", null);

            //Begin the reply
            Auth_FileDownloadChunk reply = new Auth_FileDownloadChunk();
            reply.fResult = ENetError.kNetPending;
            reply.fTransID = req.fTransID;

            string file = Path.Combine(Configuration.GetString("preloader_source", "G:\\Plasma\\Servers\\Secure Preloader"), req.fDownload);
            if (File.Exists(file)) {
                int bufsize = Configuration.GetInteger("chunk_size", 16384);
                Verbose(String.Format("Sending {0} in {1}KB chunks", req.fDownload, bufsize / 1024));
                FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read);

                fQueuedChunks.Add(req.fTransID, new Queue<Chunk>());
                fFileSizes.Add(req.fTransID, (uint)fs.Length);
                while (fs.Length > fs.Position) {
                    Chunk c = new Chunk();
                    if ((fs.Length - fs.Position) > bufsize) c.fChunk = new byte[bufsize];
                    else c.fChunk = new byte[(int)fs.Length - fs.Position];

                    c.fPos = (uint)fs.Position;
                    fs.Read(c.fChunk, 0, c.fChunk.Length);
                    fQueuedChunks[req.fTransID].Enqueue(c);
                }

                fs.Close();

                ISendChunk(req.fTransID);
            } else {
                Warn("Uru404: Requested file [" + req.fDownload + "] does not exist!");

                reply.fChunkPos = 0;
                reply.fFileSize = 0;
                reply.fResult = ENetError.kNetErrFileNotFound;

                fStream.WriteUShort((ushort)AuthSrv2Cli.FileDownloadChunk);
                reply.Write(fStream);
            }
        }

        private void ISetActivePlayer() {
            Auth_SetPlayerRequest req = new Auth_SetPlayerRequest();
            req.Read(fStream);

            Auth_SetPlayerReply reply = new Auth_SetPlayerReply();
            reply.fTransID = req.fTransID;

            if (!fLoggedIn) {
                reply.fResult = ENetError.kNetErrPlayerNotFound;
                Warn(String.Format("SetActivePlayer [ID: {0}] FAILED! Not logged in.", req.fPlayerID));
            } else if (fPlayers.Contains(req.fPlayerID)) {
                fActivePlayer = req.fPlayerID;
                reply.fResult = ENetError.kNetSuccess;
                Info(String.Format("SetActivePlayer [ID: {0}] success!", req.fPlayerID));
            } else if (req.fPlayerID == 0) {
                fActivePlayer = 0;
                reply.fResult = ENetError.kNetSuccess;
                Debug("UnSetActivePlayer [ID: 0]");
            } else {
                reply.fResult = ENetError.kNetErrPlayerNotFound;
                Warn(String.Format("SetActivePlayer [ID: {0}] FAILED! Not in player list.", req.fPlayerID));
            }

            fStream.WriteUShort((ushort)AuthSrv2Cli.AcctSetPlayerReply);
            reply.Write(fStream);

            if (!fLoggedIn)
                IKickClient(ENetError.kNetErrKickedByCCR);
        }
        #endregion

        #region Vault Transactions
        private void IAddNode() {
            Auth_VaultNodeAdd req = new Auth_VaultNodeAdd();
            req.Read(fStream);

            ENetError err = ICanDoStuff();
            if (err == ENetError.kNetSuccess) {
                lock (fVaultToAuthMap) {
                    uint trans = fVaultCli.AddNode(req.fChildID, req.fParentID, req.fSaverID);
                    fVaultToAuthMap.Add(trans, req.fTransID);
                }
            } else {
                Warn("Tried to create a VaultNodeRef before entering the game!");

                Auth_VaultNodeAddReply reply = new Auth_VaultNodeAddReply();
                reply.fResult = ENetError.kNetErrVaultNodeNotFound;
                reply.fTransID = req.fTransID;

                fStream.BufferWriter();
                fStream.WriteUShort((ushort)AuthSrv2Cli.VaultAddNodeReply);
                reply.Write(fStream);
                fStream.FlushWriter();

                IKickClient(ENetError.kNetErrKickedByCCR);
            }
        }

        private void ICreateAge() {
            Auth_InitAgeRequest req = new Auth_InitAgeRequest();
            req.Read(fStream);

            ENetError err = ICanDoStuff();
            if (err == ENetError.kNetSuccess) {
                Info(String.Format("Creating Age... [FILE: {0}] [DESC: {1}] [NAME: {2}] [UUID: {3}] [USER: {4}]",
                    new object[] { req.fFilename, req.fDescription, req.fInstanceName, req.fAgeUUID, req.fUserName }));

                lock (fVaultToAuthMap) {
                    uint transID = fVaultCli.CreateAge(req.fAgeUUID, req.fParentUUID, req.fFilename, req.fInstanceName, req.fUserName, req.fDescription, req.fSequenceNumber, req.fLanguage, fActivePlayer);
                    fVaultToAuthMap.Add(transID, req.fTransID);
                }
            } else {
                Warn(String.Format("Tried to create an {0} age without being properly in game", req.fFilename));

                Auth_InitAgeReply reply = new Auth_InitAgeReply();
                reply.fResult = err;
                reply.fTransID = req.fTransID;

                fStream.BufferWriter();
                fStream.WriteUShort((ushort)AuthSrv2Cli.VaultInitAgeReply);
                reply.Write(fStream);
                fStream.FlushWriter();

                IKickClient(ENetError.kNetErrKickedByCCR);
            }
        }

        private void ICreateNode() {
            Auth_VaultNodeCreate req = new Auth_VaultNodeCreate();
            req.Read(fStream);

            ENetError err = ICanDoStuff();
            if (err == ENetError.kNetSuccess) {
                lock (fVaultToAuthMap) {
                    uint transID = fVaultCli.CreateNode(req.fNodeData);
                    fVaultToAuthMap.Add(transID, req.fTransID);
                }
            } else {
                Warn("Tried to create a VaultNode before entering the game!");

                Auth_VaultNodeCreated reply = new Auth_VaultNodeCreated();
                reply.fResult = ENetError.kNetErrInternalError;
                reply.fTransID = req.fTransID;

                fStream.BufferWriter();
                fStream.WriteUShort((ushort)AuthSrv2Cli.VaultNodeCreated);
                reply.Write(fStream);
                fStream.FlushWriter();

                IKickClient(ENetError.kNetErrKickedByCCR);
            }
        }

        private void ICreatePlayer() {
            Auth_PlayerCreateRequest req = new Auth_PlayerCreateRequest();
            req.Read(fStream);

            ENetError err = ENetError.kNetPending;
            if (!fLoggedIn) {
                err = ENetError.kNetErrAccountNotFound;
                Warn("Attempted to CreatePlayer before login");
            } else if (fPlayers.Count >= 5) {
                err = ENetError.kNetErrMaxPlayersOnAcct;
                Warn("Maximum number of players on accounts reached!");
            } else {
                SelectStatement sel = new SelectStatement();
                sel.Limit = 1;
                sel.Select.Add("Idx");
                sel.Table = "Players";
                sel.Where.Add("Name", req.fName);

                MySqlDataReader r = sel.ExecuteQuery(fDB);
                if (r.Read()) {
                    err = ENetError.kNetErrPlayerAlreadyExists;
                    Info("Player name already taken [NAME: " + req.fName + "]");
                } else err = ENetError.kNetSuccess;
                r.Close();
            }

            if (err == ENetError.kNetSuccess) {
                Debug(String.Format("Sending PlayerCreateRequest to vault [NAME: {0}] [SHAPE: {1}] [INVITE: {2}]", req.fName, req.fModel, req.fInvite));
                uint vtrans = fVaultCli.CreatePlayer(req.fName, fAcctUUID, req.fModel, req.fInvite);
                lock (fVaultToAuthMap)
                    fVaultToAuthMap.Add(vtrans, req.fTransID);
            } else {
                Auth_PlayerCreateReply reply = new Auth_PlayerCreateReply();
                reply.fResult = err;
                reply.fTransID = req.fTransID;

                fStream.WriteUShort((ushort)AuthSrv2Cli.PlayerCreateReply);
                reply.Write(fStream);
            }
        }

        private void IFetchNode() {
            Auth_VaultNodeFetch req = new Auth_VaultNodeFetch();
            req.Read(fStream);

            ENetError err = ICanDoStuff();
            if (err == ENetError.kNetSuccess) {
                lock (fVaultToAuthMap) {
                    uint vtrans = fVaultCli.FetchNode(req.fNodeID);
                    fVaultToAuthMap.Add(vtrans, req.fTransID);
                }

                Debug(String.Format("Sending VaultNodeFetch to Vault [ID: {0}]", req.fNodeID));
            } else {
                Warn("Attempted to fetch a VaultNode before being in game");

                Auth_VaultNodeFetched reply = new Auth_VaultNodeFetched();
                reply.fResult = err;
                reply.fTransID = req.fTransID;

                fStream.BufferWriter();
                fStream.WriteUShort((ushort)AuthSrv2Cli.VaultNodeFetched);
                reply.Write(fStream);
                fStream.FlushWriter();

                IKickClient(ENetError.kNetErrKickedByCCR);
            }
        }

        private void IFetchNodeRefs() {
            Auth_VaultFetchNodeRefs req = new Auth_VaultFetchNodeRefs();
            req.Read(fStream);

            lock (fVaultToAuthMap) {
                uint vtrans = fVaultCli.FetchNodeRefs(req.fNodeID);
                fVaultToAuthMap.Add(vtrans, req.fTransID);
            }
        }

        private void IFindNode() {
            Auth_VaultNodeFind req = new Auth_VaultNodeFind();
            req.Read(fStream);

            ENetError err = ICanDoStuff();
            if (err == ENetError.kNetSuccess) {
                lock (fVaultToAuthMap) {
                    uint vtrans = fVaultCli.FindNode(req.fNodeData);
                    fVaultToAuthMap.Add(vtrans, req.fTransID);
                }
            } else {
                Warn("Attempted to find a VaultNode before being in game");

                Auth_VaultNodeFindReply reply = new Auth_VaultNodeFindReply();
                reply.fResult = ENetError.kNetErrVaultNodeNotFound;
                reply.fTransID = req.fTransID;

                fStream.BufferWriter();
                fStream.WriteUShort((ushort)AuthSrv2Cli.VaultNodeFindReply);
                reply.Write(fStream);
                fStream.FlushWriter();

                IKickClient(ENetError.kNetErrKickedByCCR);
            }
        }

        private void ISaveNode() {
            Auth_VaultNodeSave req = new Auth_VaultNodeSave();
            req.Read(fStream);

            ENetError err = ICanDoStuff();
            if (err == ENetError.kNetSuccess) {
                lock (fVaultToAuthMap) {
                    uint trans = fVaultCli.SaveNode(req.fNodeID, req.fRevisionID, req.fNodeData);
                    fVaultToAuthMap.Add(trans, req.fTransID);
                }
            } else {
                Warn("Attempted to change a VaultNode before being in game");

                Auth_VaultNodeSaveReply reply = new Auth_VaultNodeSaveReply();
                reply.fResult = ENetError.kNetErrVaultNodeNotFound;
                reply.fTransID = req.fTransID;

                fStream.BufferWriter();
                fStream.WriteUShort((ushort)AuthSrv2Cli.VaultSaveNodeReply);
                reply.Write(fStream);
                fStream.FlushWriter();

                IKickClient(ENetError.kNetErrKickedByCCR);
            }
        }
        #endregion

        public override void Stop() {
            if (fStream != null) fStream.Close();
            fVaultCli.Disconnect();
            fSocket.Close();
            fParent.Remove(this);
        }
    }
}
