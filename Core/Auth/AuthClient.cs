using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using OpenSSL;

namespace MUd {
    public delegate void AuthAgeReply(uint transID, ENetError result, Guid instance, uint mcpid, uint vaultID, IPAddress gameSrv);
    public delegate void AuthChunkDownloaded(uint transID, ENetError result, uint fileSize, uint chunkPos, byte[] data);
    public delegate void AuthClientRegistered(uint challenge);
    public delegate void AuthFileDownloaded(uint transID, string name, byte[] data);
    public delegate void AuthFileList(uint transID, ENetError result, byte[] data);
    public delegate void AuthGotPublicAges(uint transID, ENetError result, NetAgeInfo[] ages);
    public delegate void AuthKickedOff(ENetError reason);
    public delegate void AuthLoggedIn(uint transID, ENetError result, uint flags, uint[] droidKey, uint billing, Guid acctUuid);
    public delegate void AuthPlayerInfo(uint transID, string name, uint idx, string shape, uint explorer);
    public delegate void AuthPong(uint transID, uint pingTime, byte[] payload);
    public delegate void AuthResult(uint transID, ENetError result);
    public delegate void AuthServerAddr(int addr, Guid token);
    public delegate void AuthVaultNodeAdded(uint parentID, uint childID, uint saverID);
    public delegate void AuthVaultNodeChanged(uint nodeID, Guid revUuid);
    public delegate void AuthVaultNodeCreated(uint transID, ENetError result, uint nodeID);
    public delegate void AuthVaultNodeFetched(uint transID, ENetError result, byte[] data);
    public delegate void AuthVaultNodeFound(uint transID, ENetError result, uint[] nodeIDs);
    public delegate void AuthVaultNodeRemoved(uint parentID, uint childID);
    public delegate void AuthVaultTreeFetched(uint transID, ENetError result, VaultNodeRef[] refs);

    public class AuthClient : Cli2SrvBase {

        public event AuthChunkDownloaded ChunkDownloaded;
        public event AuthClientRegistered ClientRegistered;
        public event AuthFileDownloaded FileDownloaded;
        public event AuthFileList FileList;
        public event AuthAgeReply GotAge;
        public event AuthGotPublicAges GotPublicAges;
        public event AuthKickedOff KickedOff;
        public event AuthLoggedIn LoggedIn;
        public event AuthPlayerInfo PlayerInfo;
        public event AuthResult PlayerSet;
        public event AuthPong Pong;
        public event AuthServerAddr ServerAddr;
        public event AuthVaultNodeAdded VaultNodeAdded;
        public event AuthResult VaultNodeAddReply;
        public event AuthVaultNodeChanged VaultNodeChanged;
        public event AuthVaultNodeCreated VaultNodeCreated;
        public event AuthVaultNodeFetched VaultNodeFetched;
        public event AuthVaultNodeFound VaultNodeFound;
        public event AuthVaultNodeRemoved VaultNodeRemoved;
        public event AuthResult VaultNodeRemoveReply;
        public event AuthResult VaultNodeSaveReply;
        public event AuthVaultTreeFetched VaultTreeFetched;

        Dictionary<uint, string> fTransToName = new Dictionary<uint, string>();
        Dictionary<uint, byte[]> fDownloads = new Dictionary<uint, byte[]>();

        ManualResetEvent fChgHack = new ManualResetEvent(false);
        uint fSrvChallenge;
        public uint Challenge {
            get { return fSrvChallenge; }
        }

        public AuthClient() : base() { 
            fHeader.fType = EConnType.kConnTypeCliToAuth; 
        }

        public override bool Connect() {
            if (!base.Connect()) return false;

            //Send the AuthConnectHeader
            UruStream s = new UruStream(new NetworkStream(fSocket, false));
            s.BufferWriter();
            fHeader.Write(s);
            s.WriteInt(20);
            s.WriteBytes(Guid.Empty.ToByteArray());
            s.FlushWriter();
            s.Close();

            //Init encryption
            if(!base.NetCliConnect(41))
                return false;

            //Register the client...
            //Don't require the user to do this.
            Auth_RegisterRequest req = new Auth_RegisterRequest();
            req.fBuildID = fHeader.fBuildID;

            fStream.BufferWriter();
            fStream.WriteUShort((ushort)AuthCli2Srv.ClientRegisterRequest);
            req.Write(fStream);
            fStream.FlushWriter();

            fSocket.BeginReceive(new byte[2], 0, 2, SocketFlags.Peek, new AsyncCallback(IReceive), null);
            fChgHack.Reset(); fChgHack.WaitOne();

            return true;
        }

        protected override void RunIdleBehavior() {
            switch (fIdleBeh) {
                case IdleBehavior.Disconnect:
                    Disconnect();
                    break;
                case IdleBehavior.Ping:
                    Ping((uint)DateTime.Now.Ticks, Encoding.UTF8.GetBytes("IDLE"));
                    break;
            }
        }

        public uint AddVaultNode(uint parent, uint child, uint saver) {
            Auth_VaultNodeAdd req = new Auth_VaultNodeAdd();
            req.fChildID = child;
            req.fParentID = parent;
            req.fSaverID = saver;
            req.fTransID = IGetTransID();

            ResetIdleTimer();
            lock (fStream) {
                fStream.BufferWriter();
                fStream.WriteUShort((ushort)AuthCli2Srv.VaultNodeAdd);
                req.Write(fStream);
                fStream.FlushWriter();
            }

            return req.fTransID;
        }

        public uint CreateVaultNode(byte[] data) {
            Auth_VaultNodeCreate req = new Auth_VaultNodeCreate();
            req.fTransID = IGetTransID();
            req.fNodeData = data;

            ResetIdleTimer();
            lock (fStream) {
                fStream.BufferWriter();
                fStream.WriteUShort((ushort)AuthCli2Srv.VaultNodeCreate);
                req.Write(fStream);
                fStream.FlushWriter();
            }

            return req.fTransID;
        }

        public uint DownloadFile(string name) {
            Auth_FileDownloadRequest req = new Auth_FileDownloadRequest();
            req.fDownload = name;
            req.fTransID = IGetTransID();

            lock (fTransToName)
                fTransToName.Add(req.fTransID, name);

            ResetIdleTimer();
            lock (fStream) {
                fStream.BufferWriter();
                fStream.WriteUShort((ushort)AuthCli2Srv.FileDownloadRequest);
                req.Write(fStream);
                fStream.FlushWriter();
            }

            return req.fTransID;
        }

        public uint FetchVaultNode(uint nodeID) {
            Auth_VaultNodeFetch req = new Auth_VaultNodeFetch();
            req.fNodeID = nodeID;
            req.fTransID = IGetTransID();

            ResetIdleTimer();
            lock (fStream) {
                fStream.BufferWriter();
                fStream.WriteUShort((ushort)AuthCli2Srv.VaultNodeFetch);
                req.Write(fStream);
                fStream.FlushWriter();
            }

            return req.fTransID;
        }

        public uint FetchVaultTree(uint nodeID) {
            Auth_VaultFetchNodeRefs req = new Auth_VaultFetchNodeRefs();
            req.fNodeID = nodeID;
            req.fTransID = IGetTransID();

            ResetIdleTimer();
            lock (fStream) {
                fStream.BufferWriter();
                fStream.WriteUShort((ushort)AuthCli2Srv.VaultFetchNodeRefs);
                req.Write(fStream);
                fStream.FlushWriter();
            }

            return req.fTransID;
        }

        public uint FindVaultNode(byte[] pattern) {
            Auth_VaultNodeFind req = new Auth_VaultNodeFind();
            req.fNodeData = pattern;
            req.fTransID = IGetTransID();

            ResetIdleTimer();
            lock (fStream) {
                fStream.BufferWriter();
                fStream.WriteUShort((ushort)AuthCli2Srv.VaultNodeFind);
                req.Write(fStream);
                fStream.FlushWriter();
            }

            return req.fTransID;
        }

        public uint GetPublicAges(string filename) {
            Auth_GetPublicAges req = new Auth_GetPublicAges();
            req.fFilename = filename;
            req.fTransID = IGetTransID();

            ResetIdleTimer();
            lock (fStream) {
                fStream.BufferWriter();
                fStream.WriteUShort((ushort)AuthCli2Srv.GetPublicAgeList);
                req.Write(fStream);
                fStream.FlushWriter();
            }

            return req.fTransID;
        }

        public uint ListFiles(string dir, string ext) {
            Auth_FileListRequest req = new Auth_FileListRequest();
            req.fDirectory = dir;
            req.fExtension = ext;
            req.fTransID = IGetTransID();

            ResetIdleTimer();
            lock (fStream) {
                fStream.BufferWriter();
                fStream.WriteUShort((ushort)AuthCli2Srv.FileListRequest);
                req.Write(fStream);
                fStream.FlushWriter();
            }

            return req.fTransID;
        }

        public uint Login(string acctName, string acctPW, uint srvChal) {
            int clientChal = BitConverter.ToInt32(Helpers.StrongRandom(4), 0);

            //Note: Usernames without "@" are SHA-1
            //Otherwise, they are SHA-0 with the strcopy bug
            if (acctName.IndexOf('@') > -1) {
                return Login(acctName, ShaHash.HashLoginInfo(acctName, acctPW, clientChal, srvChal), clientChal);
            } else {
                ShaHash temp = ShaHash.HashPW(acctPW);
                return Login(acctName, temp, clientChal);
            }
        }

        public uint Login(string acctName, ShaHash acctPW, int cliChal) {
            Auth_LoginRequest req = new Auth_LoginRequest();
            req.fAccount = acctName;
            req.fChallenge = cliChal;
            req.fHash = acctPW;
            req.fOS = "MUd 2";
            req.fTransID = IGetTransID();

            ResetIdleTimer();
            lock (fStream) {
                fStream.BufferWriter();
                fStream.WriteUShort((ushort)AuthCli2Srv.AcctLoginRequest);
                req.Write(fStream);
                fStream.FlushWriter();
            }

            return req.fTransID;
        }

        public uint Ping(uint pingTime, byte[] payload) {
            Auth_PingPong req = new Auth_PingPong();
            req.fPayload = payload;
            req.fPingTime = pingTime;
            req.fTransID = IGetTransID();

            ResetIdleTimer();
            lock (fStream) {
                fStream.BufferWriter();
                fStream.WriteUShort((ushort)AuthCli2Srv.PingRequest);
                req.Write(fStream);
                fStream.FlushWriter();
            }

            return req.fTransID;
        }

        public uint RemoveVaultNode(uint parent, uint child) {
            Auth_VaultNodeRemove req = new Auth_VaultNodeRemove();
            req.fChildID = child;
            req.fParentID = parent;
            req.fTransID = IGetTransID();

            ResetIdleTimer();
            lock (fStream) {
                fStream.BufferWriter();
                fStream.WriteUShort((ushort)AuthCli2Srv.VaultNodeRemove);
                req.Write(fStream);
                fStream.FlushWriter();
            }

            return req.fTransID;
        }

        public uint RequestAge(string filename, Guid instance) {
            Auth_AgeRequest req = new Auth_AgeRequest();
            req.fAgeName = filename;
            req.fAgeInstanceUuid = instance;
            req.fTransID = IGetTransID();

            ResetIdleTimer();
            lock (fStream) {
                fStream.BufferWriter();
                fStream.WriteUShort((ushort)AuthCli2Srv.AgeRequest);
                req.Write(fStream);
                fStream.FlushWriter();
            }

            return req.fTransID;
        }

        public uint SaveVaultNode(Guid revUuid, uint nodeID, byte[] data) {
            Auth_VaultNodeSave req = new Auth_VaultNodeSave();
            req.fNodeData = data;
            req.fNodeID = nodeID;
            req.fRevisionID = revUuid;
            req.fTransID = IGetTransID();

            ResetIdleTimer();
            lock (fStream) {
                fStream.BufferWriter();
                fStream.WriteUShort((ushort)AuthCli2Srv.VaultNodeSave);
                req.Write(fStream);
                fStream.FlushWriter();
            }

            return req.fTransID;
        }

        public uint SetActivePlayer(uint id) {
            Auth_SetPlayerRequest req = new Auth_SetPlayerRequest();
            req.fPlayerID = id;
            req.fTransID = IGetTransID();

            ResetIdleTimer();
            lock (fStream) {
                fStream.BufferWriter();
                fStream.WriteUShort((ushort)AuthCli2Srv.AcctSetPlayerRequest);
                req.Write(fStream);
                fStream.FlushWriter();
            }

            return req.fTransID;
        }

        public void SetAgePublic(uint ageInfoID, bool isPublic) {
            Auth_SetAgePublic req = new Auth_SetAgePublic();
            req.fAgeInfoID = ageInfoID;
            req.fPublic = isPublic;

            ResetIdleTimer();
            lock (fStream) {
                fStream.BufferWriter();
                fStream.WriteUShort((ushort)AuthCli2Srv.SetAgePublic);
                req.Write(fStream);
                fStream.FlushWriter();
            }
        }

        private void IReceive(IAsyncResult ar) {
            try {
                lock (fStream) {
                    fSocket.EndReceive(ar);
                    AuthSrv2Cli msg = (AuthSrv2Cli)fStream.ReadUShort();

                    ResetIdleTimer();
                    switch (msg) {
                        case AuthSrv2Cli.AcctLoginReply:
                            ILoggedIn();
                            break;
                        case AuthSrv2Cli.AcctPlayerInfo:
                            IPlayerInfo();
                            break;
                        case AuthSrv2Cli.AcctSetPlayerReply:
                            IPlayerSet();
                            break;
                        case AuthSrv2Cli.AgeReply:
                            IAgeReply();
                            break;
                        case AuthSrv2Cli.ClientRegisterReply:
                            IClientRegistered();
                            break;
                        case AuthSrv2Cli.FileDownloadChunk:
                            IDownloadChunk();
                            break;
                        case AuthSrv2Cli.FileListReply:
                            IFileList();
                            break;
                        case AuthSrv2Cli.KickedOff:
                            IKickedOff();
                            break;
                        case AuthSrv2Cli.PingReply:
                            IPong();
                            break;
                        case AuthSrv2Cli.PublicAgeList:
                            IPublicAgeList();
                            break;
                        case AuthSrv2Cli.ServerAddr:
                            IServerAddr();
                            break;
                        case AuthSrv2Cli.VaultAddNodeReply:
                            IVaultNodeAddReply();
                            break;
                        case AuthSrv2Cli.VaultNodeAdded:
                            IVaultNodeAdded();
                            break;
                        case AuthSrv2Cli.VaultNodeChanged:
                            IVaultNodeChanged();
                            break;
                        case AuthSrv2Cli.VaultNodeCreated:
                            IVaultNodeCreated();
                            break;
                        case AuthSrv2Cli.VaultNodeFetched:
                            IVaultNodeFetched();
                            break;
                        case AuthSrv2Cli.VaultNodeFindReply:
                            IVaultNodeFound();
                            break;
                        case AuthSrv2Cli.VaultNodeRefsFetched:
                            IVaultNodeRefsFetched();
                            break;
                        case AuthSrv2Cli.VaultNodeRemoved:
                            IVaultNodeRemoved();
                            break;
                        case AuthSrv2Cli.VaultRemoveNodeReply:
                            IVaultNodeRemoveReply();
                            break;
                        case AuthSrv2Cli.VaultSaveNodeReply:
                            IVaultNodeSaveReply();
                            break;
                        default:
                            string test = Enum.GetName(typeof(AuthSrv2Cli), msg);
                            throw new NotSupportedException(msg.ToString("X") + " - " + test);
                    }
                }

                fSocket.BeginReceive(new byte[2], 0, 2, SocketFlags.Peek, new AsyncCallback(IReceive), null);
            } catch (ObjectDisposedException) { } catch (SocketException) { fSocket.Close(); } 
#if !DEBUG
            catch (Exception e) {
                if (Connected)
                    FireException(e);
            }
#endif
        }

        private void IAgeReply() {
            Auth_AgeReply reply = new Auth_AgeReply();
            reply.Read(fStream);
            if (GotAge != null)
                GotAge(reply.fTransID, reply.fResult, reply.fAgeInstanceUuid, reply.fAgeMcpID, reply.fAgeVaultID, reply.fGameServerIP);
        }

        private void IClientRegistered() {
            Auth_RegisterReply reply = new Auth_RegisterReply();
            reply.Read(fStream);
            fSrvChallenge = reply.fChallenge;
            fChgHack.Set();
            if (ClientRegistered != null)
                ClientRegistered(reply.fChallenge);
        }

        private void IDownloadChunk() {
            Auth_FileDownloadChunk chunk = new Auth_FileDownloadChunk();
            chunk.Read(fStream);

            if (ChunkDownloaded != null)
                ChunkDownloaded(chunk.fTransID, chunk.fResult, chunk.fFileSize, chunk.fChunkPos, chunk.fChunkData);

            lock (fDownloads) {
                if (!fDownloads.ContainsKey(chunk.fTransID))
                    fDownloads.Add(chunk.fTransID, new byte[chunk.fFileSize]);
                Buffer.BlockCopy(chunk.fChunkData, 0, fDownloads[chunk.fTransID], (int)chunk.fChunkPos, chunk.fChunkData.Length);

                //Are we done yet?
                if (chunk.fChunkPos + chunk.fChunkData.Length == chunk.fFileSize) {
                    string name = String.Empty;
                    lock (fTransToName) {
                        name = fTransToName[chunk.fTransID];
                        fTransToName.Remove(chunk.fTransID);
                    }

                    if (FileDownloaded != null) FileDownloaded(chunk.fTransID, name, fDownloads[chunk.fTransID]);
                    fDownloads.Remove(chunk.fTransID);
                }
            }

            //Send an ack... Let's not make the programmer do this manually.
            Auth_FileDownloadChunkAck ack = new Auth_FileDownloadChunkAck();
            ack.fTransID = chunk.fTransID;
            fStream.BufferWriter();
            fStream.WriteUShort((ushort)AuthCli2Srv.FileDownloadChunkAck);
            ack.Write(fStream);
            fStream.FlushWriter();
        }

        private void IFileList() {
            Auth_FileListReply reply = new Auth_FileListReply();
            reply.Read(fStream);
            if (FileList != null)
                FileList(reply.fTransID, reply.fResult, reply.fData);
        }

        private void IKickedOff() {
            Auth_KickedOff notify = new Auth_KickedOff();
            notify.Read(fStream);
            if (KickedOff != null)
                KickedOff(notify.fReason);
        }

        private void ILoggedIn() {
            Auth_LoginReply reply = new Auth_LoginReply();
            reply.Read(fStream);
            if (LoggedIn != null)
                LoggedIn(reply.fTransID, reply.fResult, reply.fFlags, reply.fDroidKey, reply.fBillingType, reply.fAcctGuid);
        }

        private void IPlayerInfo() {
            Auth_PlayerInfo reply = new Auth_PlayerInfo();
            reply.Read(fStream);
            if (PlayerInfo != null)
                PlayerInfo(reply.fTransID, reply.fPlayerName, reply.fPlayerID, reply.fModel, reply.fExplorer);
        }

        private void IPlayerSet() {
            Auth_SetPlayerReply reply = new Auth_SetPlayerReply();
            reply.Read(fStream);
            if (PlayerSet != null)
                PlayerSet(reply.fTransID, reply.fResult);
        }

        private void IPong() {
            Auth_PingPong reply = new Auth_PingPong();
            reply.Read(fStream);
            if (Pong != null)
                Pong(reply.fTransID, reply.fPingTime, reply.fPayload);
        }

        private void IPublicAgeList() {
            Auth_GotPublicAges reply = new Auth_GotPublicAges();
            reply.Read(fStream);
            if (GotPublicAges != null)
                GotPublicAges(reply.fTransID, reply.fResult, reply.fAges);
        }

        private void IServerAddr() {
            Auth_ServerAddr addr = new Auth_ServerAddr();
            addr.Read(fStream);
            if (ServerAddr != null)
                ServerAddr(addr.fAddr, addr.fToken);
        }

        private void IVaultNodeAdded() {
            Auth_VaultNodeAdded notify = new Auth_VaultNodeAdded();
            notify.Read(fStream);
            if (VaultNodeAdded != null)
                VaultNodeAdded(notify.fParentID, notify.fChildID, notify.fSaverID);
        }

        private void IVaultNodeAddReply() {
            Auth_VaultNodeAddReply reply = new Auth_VaultNodeAddReply();
            reply.Read(fStream);
            if (VaultNodeAddReply != null)
                VaultNodeAddReply(reply.fTransID, reply.fResult);
        }

        private void IVaultNodeChanged() {
            Auth_VaultNodeChanged notify = new Auth_VaultNodeChanged();
            notify.Read(fStream);
            if (VaultNodeChanged != null)
                VaultNodeChanged(notify.fNodeID, notify.fRevisionUuid);
        }

        private void IVaultNodeCreated() {
            Auth_VaultNodeCreated reply = new Auth_VaultNodeCreated();
            reply.Read(fStream);
            if (VaultNodeCreated != null)
                VaultNodeCreated(reply.fTransID, reply.fResult, reply.fNodeID);
        }

        private void IVaultNodeFetched() {
            Auth_VaultNodeFetched reply = new Auth_VaultNodeFetched();
            reply.Read(fStream);
            if (VaultNodeFetched != null)
                VaultNodeFetched(reply.fTransID, reply.fResult, reply.fNodeData);
        }

        private void IVaultNodeFound() {
            Auth_VaultNodeFindReply reply = new Auth_VaultNodeFindReply();
            reply.Read(fStream);
            if (VaultNodeFound != null)
                VaultNodeFound(reply.fTransID, reply.fResult, reply.fNodeIDs);
        }

        private void IVaultNodeRefsFetched() {
            Auth_VaultNodeRefsFetched reply = new Auth_VaultNodeRefsFetched();
            reply.Read(fStream);
            if (VaultTreeFetched != null)
                VaultTreeFetched(reply.fTransID, reply.fResult, reply.fRefs);
        }

        private void IVaultNodeRemoved() {
            Auth_VaultNodeRemoved notify = new Auth_VaultNodeRemoved();
            notify.Read(fStream);
            if (VaultNodeRemoved != null)
                VaultNodeRemoved(notify.fParentID, notify.fChildID);
        }

        private void IVaultNodeRemoveReply() {
            Auth_VaultNodeRemoveReply reply = new Auth_VaultNodeRemoveReply();
            reply.Read(fStream);
            if (VaultNodeRemoveReply != null)
                VaultNodeRemoveReply(reply.fTransID, reply.fResult);
        }

        private void IVaultNodeSaveReply() {
            Auth_VaultNodeSaveReply reply = new Auth_VaultNodeSaveReply();
            reply.Read(fStream);
            if (VaultNodeSaveReply != null)
                VaultNodeSaveReply(reply.fTransID, reply.fResult);
        }
    }
}
