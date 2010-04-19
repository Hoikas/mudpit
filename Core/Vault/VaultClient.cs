using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Timers;
using OpenSSL;

using Timer = System.Timers.Timer;

namespace MUd {

    public delegate void VaultAgeCreated(uint transID, ENetError result, uint ageID, uint infoID);
    public delegate void VaultNodeAdded(uint parentID, uint childID, uint saverID);
    public delegate void VaultNodeChanged(uint nodeID, Guid revID);
    public delegate void VaultNodeCreated(uint transID, ENetError result, uint nodeID);
    public delegate void VaultNodeFetched(uint transID, ENetError result, byte[] nodeData);
    public delegate void VaultNodeFound(uint transID, ENetError result, uint[] nodes);
    public delegate void VaultNodeRefsFetched(uint transID, ENetError result, VaultNodeRef[] refs);
    public delegate void VaultPlayerCreated(uint transID, uint playerID, string playerName, string model);
    public delegate void VaultPong(uint transID, uint pingTime, byte[] payload);
    public delegate void VaultResult(uint transID, ENetError result);

    public class VaultClient : Srv2CliBase {

        public event VaultAgeCreated AgeCreated;
        public event VaultNodeAdded NodeAdded;
        public event VaultResult NodeAddReply;
        public event VaultNodeChanged NodeChanged;
        public event VaultNodeCreated NodeCreated;
        public event VaultNodeFetched NodeFetched;
        public event VaultNodeFound NodeFound;
        public event VaultNodeRefsFetched NodeRefsFetched;
        public event VaultResult NodeSaved;
        public event VaultPlayerCreated PlayerCreated;
        public event VaultPong Pong;

        public bool Connected {
            get { return fConnected; }
        }

        UruStream fStream;

        Dictionary<uint, ManualResetEvent> fMREs = new Dictionary<uint, ManualResetEvent>();
        Dictionary<uint, object> fWaitData = new Dictionary<uint, object>();

        bool fConnected = false;
        byte[] fDhData;
        byte[] fClientSeed;
        uint fTransID = 0;

        public VaultClient()
            : base(null, new ConnectHeader(), null) {
            throw new NotImplementedException();
        }

        public VaultClient(LogProcessor log)
            : base(new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp), new ConnectHeader(), log) {
        }

        #region Connect
        public bool Connect(string host, int port) {
            string empty = Guid.Empty.ToString();
            try {
                fSocket.Connect(host, port);
            } catch (SocketException) {
                Error("Cannot connect to vault!");
                return false;
            }

            fConn.fBranchID = 0;
            fConn.fBuildID = 0;
            fConn.fBuildType = 0;
            fConn.fProductID = new Guid(Configuration.GetString("auth_guid", empty));
            fConn.fSockHeaderSize = (ushort)ConnectHeader.kSize;
            fConn.fType = EConnType.kConnTypeSrvToVault;

            //Send ConnectHeader
            UruStream s = new UruStream(new NetworkStream(fSocket, false));
            fConn.Write(s);

            //Check status
            if (!fSocket.Connected) {
                Error("Auth UUID invalid");
                return false;
            }

            //Send the vault connection header
            Guid token = new Guid(Configuration.GetString("vault_token", empty));
            s.WriteInt(20);
            s.WriteBytes(token.ToByteArray());

            //Check status
            if (!fSocket.Connected) {
                Error("Vault Token invalid");
                return false;
            }

            //Calculate keys
            if (!ISetupKeys()) {
                fSocket.Close();
                Error("Cannot setup Vault keys!");
                return false;
            }

            //NetCliConnect
            s.WriteByte((byte)NetCliConnectMsg.kNetCliConnect);
            s.WriteByte((byte)(fDhData.Length + 2));
            s.WriteBytes(fDhData);

            //NetCliEncrypt
            if (s.ReadByte() != (byte)NetCliConnectMsg.kNetCliEncrypt) {
                fSocket.Close();
                Error("Vault sent an ERROR message after DhY data");
                return false;
            }

            byte[] seed = s.ReadBytes((int)(s.ReadByte() - 2));
            ISetupEncryption(seed);
            fConnected = true;
            return true;
        }

        private void ISetupEncryption(byte[] seed) {
            byte[] key = new byte[7];
            for (int i = 0; i < 7; i++) {
                if (i >= fClientSeed.Length) key[i] = seed[i];
                else key[i] = (byte)(fClientSeed[i] ^ seed[i]);
            }

            fStream = new UruStream(new CryptoNetStream(key, fSocket));
        }

        private bool ISetupKeys() {
            string dir = Configuration.GetString("enc_keys", "G:\\Plasma\\Servers\\Encryption Keys");
            string pubkey = Path.Combine(dir, "Vault_Public.key");
            string shared = Path.Combine(dir, "Vault_Shared.key");

            if (!File.Exists(pubkey) || !File.Exists(shared))
                return false;

            byte[] data = new byte[64];
            BigNum b = BigNum.Random(512);

            FileStream fs = new FileStream(pubkey, FileMode.Open, FileAccess.Read);
            fs.Read(data, 0, 64);
            fs.Close();
            BigNum N = new BigNum(data);

            fs = new FileStream(shared, FileMode.Open, FileAccess.Read);
            fs.Read(data, 0, 64);
            fs.Close();
            BigNum X = new BigNum(data);

            //Calculate seeds
            BigNum client_seed = X.PowMod(b, N);
            BigNum server_seed = new BigNum(4).PowMod(b, N);

            //Dump data
            fDhData = server_seed.ToArray();
            fClientSeed = client_seed.ToArray();

            //Explicitly dispose unmanaged OpenSSL resources
            b.Dispose();
            N.Dispose();
            X.Dispose();
            client_seed.Dispose();
            server_seed.Dispose();

            return true;
        }
        #endregion

        #region Helpers
        private uint IGetTransID() {
            uint the_id = 0;
            lock (this) {
                the_id = fTransID;
                fTransID += 1;
            }

            return the_id;
        }
        #endregion

        #region ThreadClient
        public override void Start() {
            if (fConnected) {
                fSocket.BeginReceive(new byte[2], 0, 2, SocketFlags.Peek, new AsyncCallback(IFactorizeMessage), null);
            } else throw new Exception("Cannot start VaultCli before connection is made");
        }

        public override void Stop() {
            Monitor.Enter(fSocket);
            if (fStream != null) {
                Monitor.Enter(fStream);
                fStream.Close();
                Monitor.Exit(fStream);
            }

            fSocket.Close();
            Monitor.Exit(fSocket);
        }
        #endregion

        public uint AddNode(uint parentID, uint childID, uint saverID) {
            Vault_AddNodeRequest req = new Vault_AddNodeRequest();
            req.fChildID = childID;
            req.fParentID = parentID;
            req.fSaverID = saverID;
            req.fTransID = IGetTransID();

            lock (fStream) {
                fStream.WriteUShort((ushort)VaultCli2Srv.AddNodeRequest);
                req.Write(fStream);
            }

            return req.fTransID;
        }

        public uint CreateAge(Guid ageUuid, Guid parentUuid, string filename, string instanceName, string userName, string description, int seqNum, int lang, uint creator) {
            Vault_CreateAgeRequest req = new Vault_CreateAgeRequest();
            req.fAgeUUID = ageUuid;
            req.fCreatorID = creator;
            req.fDescription = description;
            req.fFilename = filename;
            req.fInstanceName = instanceName;
            req.fLanguage = lang;
            req.fParentUUID = parentUuid;
            req.fSequenceNumber = seqNum;
            req.fTransID = IGetTransID();
            req.fUserName = userName;

            lock (fStream) {
                fStream.WriteUShort((ushort)VaultCli2Srv.CreateAgeRequest);
                req.Write(fStream);
            }

            return req.fTransID;
        }

        public uint CreateNode(byte[] data) {
            Vault_CreateNodeRequest req = new Vault_CreateNodeRequest();
            req.fNodeData = data;
            req.fTransID = IGetTransID();

            lock (fStream) {
                fStream.WriteUShort((ushort)VaultCli2Srv.CreateNodeRequest);
                req.Write(fStream);
            }

            return req.fTransID;
        }

        public uint CreatePlayer(string name, Guid acctUuid, string shape, string invite) {
            Vault_CreatePlayerRequest req = new Vault_CreatePlayerRequest();
            req.fAcctUUID = acctUuid;
            req.fInvite = invite;
            req.fModel = shape;
            req.fName = name;
            req.fTransID = IGetTransID();

            lock (fStream) {
                fStream.WriteUShort((ushort)VaultCli2Srv.CreatePlayerRequest);
                req.Write(fStream);
            }

            return req.fTransID;
        }

        public uint FetchNode(uint nodeID) {
            Vault_FetchNode req = new Vault_FetchNode();
            req.fNodeID = nodeID;
            req.fTransID = IGetTransID();

            lock (fStream) {
                fStream.WriteUShort((ushort)VaultCli2Srv.FetchNode);
                req.Write(fStream);
            }

            return req.fTransID;
        }

        public byte[] FetchNodeWait(uint nodeID) {
            Vault_FetchNode req = new Vault_FetchNode();
            req.fNodeID = nodeID;
            req.fTransID = IGetTransID();

            lock (fStream) {
                fStream.WriteUShort((ushort)VaultCli2Srv.FetchNode);
                req.Write(fStream);
            }

            ManualResetEvent mre = new ManualResetEvent(false);
            lock (fMREs)
                fMREs.Add(req.fTransID, mre);
            mre.WaitOne();

            byte[] result = fWaitData[req.fTransID] as byte[];
            fWaitData.Remove(req.fTransID);
            fMREs.Remove(req.fTransID);
            return result;
        }

        public uint FetchNodeRefs(uint nodeID) {
            Vault_FetchNodeRefs req = new Vault_FetchNodeRefs();
            req.fNodeID = nodeID;
            req.fTransID = IGetTransID();

            lock (fStream) {
                fStream.WriteUShort((ushort)VaultCli2Srv.FetchNodeRefs);
                req.Write(fStream);
            }

            return req.fTransID;
        }

        public uint FindNode(byte[] node) {
            Vault_FindNode req = new Vault_FindNode();
            req.fNodeData = node;
            req.fTransID = IGetTransID();

            lock (fStream) {
                fStream.WriteUShort((ushort)VaultCli2Srv.FindNode);
                req.Write(fStream);
            }

            return req.fTransID;
        }

        public uint[] FindNodeWait(byte[] node) {
            Vault_FindNode req = new Vault_FindNode();
            req.fNodeData = node;
            req.fTransID = IGetTransID();

            lock (fStream) {
                fStream.WriteUShort((ushort)VaultCli2Srv.FindNode);
                req.Write(fStream);
            }

            ManualResetEvent mre = new ManualResetEvent(false);
            lock (fMREs)
                fMREs.Add(req.fTransID, mre);
            mre.WaitOne();

            uint[] result = fWaitData[req.fTransID] as uint[];
            fWaitData.Remove(req.fTransID);
            fMREs.Remove(req.fTransID);
            return result;
        }

        public uint Ping(uint pingTime, byte[] payload) {
            Vault_PingPong ping = new Vault_PingPong();
            ping.fPayload = payload;
            ping.fPingTime = pingTime;
            ping.fTransID = IGetTransID();

            lock (fStream) {
                fStream.WriteUShort((ushort)VaultCli2Srv.PingRequest);
                ping.Write(fStream);
            }

            return ping.fTransID;
        }

        public uint SaveNode(uint nodeID, Guid revID, byte[] nodeData) {
            Vault_SaveNodeRequest req = new Vault_SaveNodeRequest();
            req.fNodeData = nodeData;
            req.fNodeID = nodeID;
            req.fRevisionID = revID;
            req.fTransID = IGetTransID();

            lock (fStream) {
                fStream.WriteUShort((ushort)VaultCli2Srv.SaveNodeRequest);
                req.Write(fStream);
            }

            return req.fTransID;
        }

        #region Message Handlers
        private void IFactorizeMessage(IAsyncResult ar) {
            try {
                lock (fStream) {
                    VaultSrv2Cli msg = VaultSrv2Cli.PingReply;
                    fSocket.EndReceive(ar);
                    try {
                        msg = (VaultSrv2Cli)fStream.ReadUShort();
                    } catch (IOException) {
                        Debug("[VaultCli] Disconnected");
                    }

                    switch (msg) {
                        case VaultSrv2Cli.AddNodeNotify:
                            INodeAdded();
                            break;
                        case VaultSrv2Cli.AddNodeReply:
                            INodeAddReply();
                            break;
                        case VaultSrv2Cli.CreateAgeReply:
                            IAgeCreated();
                            break;
                        case VaultSrv2Cli.CreateNodeReply:
                            INodeCreated();
                            break;
                        case VaultSrv2Cli.CreatePlayerReply:
                            IPlayerCreated();
                            break;
                        case VaultSrv2Cli.FetchNodeReply:
                            INodeFetched();
                            break;
                        case VaultSrv2Cli.FindNodeReply:
                            INodeFound();
                            break;
                        case VaultSrv2Cli.NodeChanged:
                            INodeChanged();
                            break;
                        case VaultSrv2Cli.NodeRefsFetched:
                            INodeRefsFetched();
                            break;
                        case VaultSrv2Cli.PingReply:
                            IPong();
                            break;
                        case VaultSrv2Cli.SaveNodeReply:
                            INodeSaved();
                            break;
                        default:
                            break;
                    }
                }

                fSocket.BeginReceive(new byte[2], 0, 2, SocketFlags.Peek, new AsyncCallback(IFactorizeMessage), null);
            } catch (SocketException e) {
                IHandleSocketException(e);
            } catch (IOException) {
                Debug("[VaultCli] Disconnected");
                Stop();
            } catch (ObjectDisposedException) { }
        }

        private void IAgeCreated() {
            Vault_CreateAgeReply reply = new Vault_CreateAgeReply();
            reply.Read(fStream);
            if (AgeCreated != null)
                AgeCreated(reply.fTransID, reply.fResult, reply.fAgeNodeID, reply.fInfoNodeID);
        }

        private void INodeAdded() {
            Vault_AddNodeNotify notify = new Vault_AddNodeNotify();
            notify.Read(fStream);
            if (NodeAdded != null)
                NodeAdded(notify.fParentID, notify.fChildID, notify.fSaverID);
        }

        private void INodeAddReply() {
            Vault_AddNodeReply reply = new Vault_AddNodeReply();
            reply.Read(fStream);
            if (NodeAddReply != null)
                NodeAddReply(reply.fTransID, reply.fResult);
        }

        private void INodeChanged() {
            Vault_NodeChanged notify = new Vault_NodeChanged();
            notify.Read(fStream);
            if (NodeChanged != null)
                NodeChanged(notify.fNodeID, notify.fRevisionUuid);
        }

        private void INodeCreated() {
            Vault_CreateNodeReply reply = new Vault_CreateNodeReply();
            reply.Read(fStream);
            if (NodeCreated != null)
                NodeCreated(reply.fTransID, reply.fResult, reply.fNodeID);
        }

        private void INodeFetched() {
            Vault_FetchNodeReply reply = new Vault_FetchNodeReply();
            reply.Read(fStream);

            lock (fMREs) {
                if (fMREs.ContainsKey(reply.fTransID)) {
                    fWaitData.Add(fTransID, reply.fNodeData);
                    fMREs[reply.fTransID].Set();
                } else {
                    if (NodeFetched != null)
                        NodeFetched(reply.fTransID, reply.fResult, reply.fNodeData);
                }
            }
        }

        private void INodeFound() {
            Vault_FindNodeReply reply = new Vault_FindNodeReply();
            reply.Read(fStream);
            
            lock (fMREs) {
                if (fMREs.ContainsKey(reply.fTransID)) {
                    fWaitData.Add(reply.fTransID, reply.fNodeIDs);
                    fMREs[reply.fTransID].Set();
                } else {
                    if (NodeFound != null)
                        NodeFound(reply.fTransID, reply.fResult, reply.fNodeIDs);
                }
            }
        }

        private void INodeRefsFetched() {
            Vault_NodeRefsFetched reply = new Vault_NodeRefsFetched();
            reply.Read(fStream);
            if (NodeRefsFetched != null)
                NodeRefsFetched(reply.fTransID, reply.fResult, reply.fRefs);
        }

        private void INodeSaved() {
            Vault_SaveNodeReply reply = new Vault_SaveNodeReply();
            reply.Read(fStream);
            if (NodeSaved != null)
                NodeSaved(reply.fTransID, reply.fResult);
        }

        private void IPlayerCreated() {
            Vault_CreatePlayerReply reply = new Vault_CreatePlayerReply();
            reply.Read(fStream);
            if (PlayerCreated != null)
                PlayerCreated(reply.fTransID, reply.fPlayerID, reply.fName, reply.fModel);
        }

        private void IPong() {
            Vault_PingPong pong = new Vault_PingPong();
            pong.Read(fStream);
            if (Pong != null)
                Pong(pong.fTransID, pong.fPingTime, pong.fPayload);
        }
        #endregion
    }
}
