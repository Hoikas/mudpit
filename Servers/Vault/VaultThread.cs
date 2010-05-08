using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Timers;
using MySql.Data.MySqlClient;
using OpenSSL;

namespace MUd {
    public class VaultThread : Srv2CliBase {

        [Flags]
        enum SubcriptionFlags {
            kNodeReferences = 0x01,
            kNodeData = 0x02,
        }

        const int kVaultHeaderSize = 20;
        byte[] fServerSeed = new byte[] { 0x4F, 0x17, 0xC8, 0x19, 0x3D, 0x08, 0xF3 };

        Dictionary<uint, SubcriptionFlags> fNodeSubscriptions = new Dictionary<uint, SubcriptionFlags>();

        DbConnection fDB;
        VaultServer fParent;
        UruStream fStream;

        uint SystemNode {
            get {
                SelectStatement sel = new SelectStatement();
                sel.Limit = 1;
                sel.Select.Add("Idx");
                sel.Table = "Nodes";
                sel.Where.Add("NodeType", ENodeType.kNodeSystem.ToString("D"));
                sel.Where.Add("Int32_1", EStandardNode.kSystemNode.ToString("D"));

                MySqlDataReader r = sel.ExecuteQuery(fDB);
                uint id = 0;
                if (r.Read()) id = r.GetUInt32("Idx");
                r.Close();
                return id;
            }
        }
        
        public VaultThread(VaultServer parent, Socket s, ConnectHeader hdr, LogProcessor log) : base(s, hdr, log) {
            fParent = parent;

            try {
                fDB = Database.Connect();
            } catch (DbException) {
                Error("Failed to connect to DATABASE...");
                Stop();
            }
        }

        public override void Start() {
            UruStream s = new UruStream(new NetworkStream(fSocket, false));

            //VaultConnectHeader
            int size = s.ReadInt();
            if (size != kVaultHeaderSize) Warn("Invalid auth header size!");
            Guid token = new Guid(s.ReadBytes(size - 4));
            Guid expect = Configuration.GetGuid("vault_token");
            if (token != expect) {
                Error("Vault Token invalid. S-T-R-I-K-E!");
                Stop();
                return;
            }

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
            if (!ISetupEncryption(y_data)) {
                Error("Cannot setup encryption keys!");
                Stop();
                return;
            }

            //Send the NetCliEncrypt response
            byte[] enc = new byte[9];
            enc[0] = (byte)NetCliConnectMsg.kNetCliEncrypt;
            enc[1] = 9;
            Buffer.BlockCopy(fServerSeed, 0, enc, 2, 7);
            s.WriteBytes(enc);

            //Begin receiving data
            fSocket.BeginReceive(new byte[2], 0, 2, SocketFlags.Peek, new AsyncCallback(IFactorizeMessage), null);
            s.Close();
        }

        private bool ISetupEncryption(byte[] y_data) {
            string dir = Configuration.GetString("enc_keys", "G:\\Plasma\\Servers\\Encryption Keys");
            string priv = Path.Combine(dir, "Vault_Private.key");
            string pub = Path.Combine(dir, "Vault_Public.key");

            //Test for keys
            if (!File.Exists(pub) || !File.Exists(priv))
                return false;

            BigNum Y = new BigNum(y_data);
            byte[] data = new byte[64];

            FileStream fs = new FileStream(priv, FileMode.Open, FileAccess.Read);
            fs.Read(data, 0, 64);
            fs.Close();
            BigNum K = new BigNum(data);

            fs = new FileStream(pub, FileMode.Open, FileAccess.Read);
            fs.Read(data, 0, 64);
            fs.Close();
            BigNum N = new BigNum(data);

            BigNum client_seed = Y.PowMod(K, N);
            byte[] seed_data = client_seed.ToArray();
            byte[] key = new byte[7];

            fServerSeed = RNG.Random(7);
            for (int i = 0; i < key.Length; i++) {
                if (i >= seed_data.Length) key[i] = fServerSeed[i];
                else key[i] = (byte)(seed_data[i] ^ fServerSeed[i]);
            }

            fStream = new UruStream(new CryptoNetStream(key, fSocket));

            K.Dispose();
            N.Dispose();
            client_seed.Dispose();

            return true;
        }

        private VaultAgeLinkNode ICreateAgeLink(uint player, string age, string instance, string userName, string description) {
            VaultAgeInfoNode info = ICreateAgeNodes(player, age, instance, userName, description);

            VaultAgeLinkNode link = new VaultAgeLinkNode();
            link.BaseNode.fCreatorIdx = player;
            link.SpawnPoints = "Default:LinkInPointDefault;";
            ICreateNode(link.BaseNode);
            ICreateNodeRef(link.BaseNode.ID, info.BaseNode.ID, player);

            return link;
        }

        private VaultAgeInfoNode ICreateAgeNodes(uint creator, string filename, string instance, string userName, string description) {
            return ICreateAgeNodes(creator, Guid.NewGuid(), Guid.Empty, filename, instance, userName, description, 0, 0);
        }

        private VaultAgeInfoNode ICreateAgeNodes(uint creator, Guid ageUuid, Guid parentUuid, string filename, string instance, string userName, string description, int seqNum, int lang) {
            Info(String.Format("Creating an age... [AGE: {0}] [PLAYER: {1}]", filename, creator));
            
            VaultAgeNode age = new VaultAgeNode();
            age.AgeName = filename;
            age.BaseNode.fCreatorIdx = creator;
            age.Instance = ageUuid;
            age.ParentInstance = parentUuid;
            ICreateNode(age.BaseNode);

            VaultFolderNode devices = new VaultFolderNode();
            devices.FolderType = EStandardNode.kAgeDevicesFolder;
            ICreateNode(devices.BaseNode);
            ICreateNodeRef(age.BaseNode.ID, devices.BaseNode.ID, creator);

            VaultAgeInfoNode age_info = new VaultAgeInfoNode();
            age_info.AgeNodeID = age.BaseNode.ID;
            age_info.BaseNode.fCreatorIdx = creator;
            age_info.Description = description;
            age_info.Filename = filename;
            age_info.InstanceName = instance;
            age_info.InstanceUUID = ageUuid;
            age_info.Language = lang;
            age_info.ParentInstanceUUID = parentUuid;
            age_info.Public = false;
            age_info.SequenceNumber = seqNum;
            age_info.UserDefinedName = userName;
            ICreateNode(age_info.BaseNode);

            VaultPlayerInfoListNode owners = new VaultPlayerInfoListNode();
            owners.BaseNode.fCreatorIdx = creator;
            owners.FolderType = EStandardNode.kAgeOwnersFolder;
            ICreateNode(owners.BaseNode);
            ICreateNodeRef(age_info.BaseNode.ID, owners.BaseNode.ID, creator);

            VaultPlayerInfoListNode vistors = new VaultPlayerInfoListNode();
            vistors.BaseNode.fCreatorIdx = creator;
            vistors.FolderType = EStandardNode.kCanVisitFolder;
            ICreateNode(vistors.BaseNode);
            ICreateNodeRef(age_info.BaseNode.ID, vistors.BaseNode.ID, creator);

            VaultAgeInfoListNode child_ages = new VaultAgeInfoListNode();
            child_ages.BaseNode.fCreatorIdx = creator;
            child_ages.FolderType = EStandardNode.kChildAgesFolder;
            ICreateNode(child_ages.BaseNode);
            ICreateNodeRef(age_info.BaseNode.ID, child_ages.BaseNode.ID, creator);

            return age_info;
        }

        private void ICreateNode(VaultNode node) {
            Dictionary<string, string> data = node.NodeData;
            if (data.ContainsKey("Idx")) data.Remove("Idx");
            if (!data.ContainsKey("CreateTime")) data.Add("CreateTime", VaultNode.ToUnixTime(node.CreateTime).ToString());
            if (!data.ContainsKey("ModifyTime")) data.Add("ModifyTime", VaultNode.ToUnixTime(node.ModifyTime).ToString());

            //Stupid bug somewhere in MySQL...
            //Must init all Guid fields to Guid.Empty
            string emptyUuid = Guid.Empty.ToString();
            if (!data.ContainsKey("CreateAgeUuid")) data.Add("CreateAgeUuid", emptyUuid);
            if (!data.ContainsKey("CreatorUuid")) data.Add("CreatorUuid", emptyUuid);
            if (!data.ContainsKey("Uuid_1")) data.Add("Uuid_1", emptyUuid);
            if (!data.ContainsKey("Uuid_2")) data.Add("Uuid_2", emptyUuid);
            if (!data.ContainsKey("Uuid_3")) data.Add("Uuid_3", emptyUuid);
            if (!data.ContainsKey("Uuid_4")) data.Add("Uuid_4", emptyUuid);

            InsertStatement insert = new InsertStatement();
            insert.Insert = data;
            insert.Table = "Nodes";
            insert.ExcecuteNonQuery(fDB);

            node.ID = Database.LastInsertID(fDB);
            Debug(String.Format("Created VaultNode [ID: {0}] [Type: {1}]", node.ID, node.NodeType));

            lock (fNodeSubscriptions) {
                fNodeSubscriptions.Add(node.ID, SubcriptionFlags.kNodeData);
            }
        }

        private void ICreateNodeRef(uint parent, uint child, uint saver) {
            InsertStatement i = new InsertStatement();
            i.Insert.Add("ParentIdx", parent.ToString());
            i.Insert.Add("ChildIdx", child.ToString());
            i.Insert.Add("SaverIdx", saver.ToString());
            i.Table = "NodeRefs";

            i.ExcecuteNonQuery(fDB);
            Debug(String.Format("New NodeRef [PARENT: {0}] [CHILD: {1}] [SAVER: {2}]", parent, child, saver));

            lock (fNodeSubscriptions) {
                if (fNodeSubscriptions.ContainsKey(parent)) {
                    if ((fNodeSubscriptions[parent] & SubcriptionFlags.kNodeReferences) == 0)
                        fNodeSubscriptions[parent] |= SubcriptionFlags.kNodeReferences;
                } else {
                    fNodeSubscriptions.Add(parent, SubcriptionFlags.kNodeReferences);
                }

                if (fNodeSubscriptions.ContainsKey(child)) {
                    if ((fNodeSubscriptions[child] & SubcriptionFlags.kNodeReferences) == 0)
                        fNodeSubscriptions[child] |= SubcriptionFlags.kNodeReferences;
                } else {
                    fNodeSubscriptions.Add(child, SubcriptionFlags.kNodeReferences);
                }
            }
        }

        private bool INodeExists(uint nodeID) {
            SelectStatement sel = new SelectStatement();
            sel.Select.Add("COUNT(*)");
            sel.Table = "Nodes";
            sel.Where.Add("Idx", nodeID.ToString());

            MySqlDataReader r = sel.ExecuteQuery(fDB);
            bool retval = false;
            if (r.Read())
                if (r.GetInt32(0) > 0) retval = true;
            r.Close();

            return retval;
        }

        public void NodeAdded(Vault_AddNodeNotify notify) {
            lock (fNodeSubscriptions) {
                if (fNodeSubscriptions.ContainsKey(notify.fParentID))
                    if ((fNodeSubscriptions[notify.fParentID] & SubcriptionFlags.kNodeReferences) != 0)
                        lock (fStream) {
                            fStream.WriteUShort((ushort)VaultSrv2Cli.AddNodeNotify);
                            notify.Write(fStream);
                        }
            }
        }

        public void NodeChanged(Vault_NodeChanged notify) {
            lock (fNodeSubscriptions) {
                if (fNodeSubscriptions.ContainsKey(notify.fNodeID))
                    if ((fNodeSubscriptions[notify.fNodeID] & SubcriptionFlags.kNodeData) != 0)
                        lock (fStream) {
                            fStream.WriteUShort((ushort)VaultSrv2Cli.NodeChanged);
                            notify.Write(fStream);
                        }
            }
        }

        #region Message Handlers
        private void IFactorizeMessage(IAsyncResult ar) {
            try {
                fSocket.EndReceive(ar);

                VaultCli2Srv msg = VaultCli2Srv.PingRequest;
                try {
                    msg = (VaultCli2Srv)fStream.ReadUShort();
                } catch (IOException) {
                    Verbose("Disconnected");
                }

                switch (msg) {
                    case VaultCli2Srv.AddNodeRequest:
                        IAddNode();
                        break;
                    case VaultCli2Srv.CreateAgeRequest:
                        ICreateAge();
                        break;
                    case VaultCli2Srv.CreateNodeRequest:
                        ICreateNode();
                        break;
                    case VaultCli2Srv.CreatePlayerRequest:
                        ICreatePlayer();
                        break;
                    case VaultCli2Srv.FindNode:
                        IFindNode();
                        break;
                    case VaultCli2Srv.FetchNode:
                        IFetchNode();
                        break;
                    case VaultCli2Srv.FetchNodeRefs:
                        IFetchNodeRefs();
                        break;
                    case VaultCli2Srv.PingRequest:
                        IPingPong();
                        break;
                    case VaultCli2Srv.SaveNodeRequest:
                        ISaveNode();
                        break;
                    default:
                        break;
                }

                fSocket.BeginReceive(new byte[2], 0, 2, SocketFlags.Peek, new AsyncCallback(IFactorizeMessage), null);
            } catch (SocketException e) {
                IHandleSocketException(e);
            } catch (IOException) {
                Debug("Disconnected");
                Stop();
            } catch (ObjectDisposedException) { }
        }

        private void IAddNode() {
            Vault_AddNodeRequest req = new Vault_AddNodeRequest();
            req.Read(fStream);

            bool child = !INodeExists(req.fChildID);
            bool parent = !INodeExists(req.fParentID);
            bool saver = !INodeExists(req.fSaverID);

            Vault_AddNodeReply reply = new Vault_AddNodeReply();
            reply.fResult = ENetError.kNetPending;
            reply.fTransID = req.fTransID;

            if (child || parent || saver) {
                string err = "Adding node failed--nodes do not exist: ";
                if (child) err += String.Format("Child [ID: {0}] ", req.fChildID);
                if (parent) err += String.Format("Parent [ID: {0}] ", req.fParentID);
                if (saver) err += String.Format("Saver [ID: {0}]", req.fSaverID);
                reply.fResult = ENetError.kNetErrVaultNodeNotFound;
            } else {
                ICreateNodeRef(req.fParentID, req.fChildID, req.fSaverID);

                Vault_AddNodeNotify notify = new Vault_AddNodeNotify();
                notify.fChildID = req.fChildID;
                notify.fParentID = req.fParentID;
                notify.fSaverID = req.fSaverID;

                fParent.NodeAdded(notify);
            }
        }

        private void ICreateAge() {
            Vault_CreateAgeRequest req = new Vault_CreateAgeRequest();
            req.Read(fStream);

            Vault_CreateAgeReply reply = new Vault_CreateAgeReply();
            reply.fResult = ENetError.kNetPending;
            reply.fTransID = req.fTransID;

            VaultAgeInfoNode info = ICreateAgeNodes(req.fCreatorID, req.fAgeUUID, req.fParentUUID, req.fFilename, req.fInstanceName, req.fUserName, req.fDescription, req.fSequenceNumber, req.fLanguage);
            if (info == null) {
                reply.fResult = ENetError.kNetErrInternalError;
            } else {
                reply.fResult = ENetError.kNetSuccess;
                reply.fInfoNodeID = info.BaseNode.ID;
                reply.fAgeNodeID = info.AgeNodeID;
            }

            fStream.WriteUShort((ushort)VaultSrv2Cli.CreateAgeReply);
            reply.Write(fStream);
        }

        private void ICreateNode() {
            Vault_CreateNodeRequest req = new Vault_CreateNodeRequest();
            req.Read(fStream);

            VaultNode node = VaultNode.SafeParse(req.fNodeData, true);
            ICreateNode(node);

            Vault_CreateNodeReply reply = new Vault_CreateNodeReply();
            reply.fNodeID = node.ID;
            reply.fResult = ENetError.kNetSuccess;
            reply.fTransID = req.fTransID;

            fStream.WriteUShort((ushort)VaultSrv2Cli.CreateNodeReply);
            reply.Write(fStream);
        }

        private void ICreatePlayer() {
            Vault_CreatePlayerRequest req = new Vault_CreatePlayerRequest();
            req.Read(fStream);

            VaultPlayerNode player = new VaultPlayerNode();
            player.AccountUUID = req.fAcctUUID;
            player.AvatarShape = req.fModel;
            player.Explorer = true;
            player.PlayerName = req.fName;
            ICreateNode(player.BaseNode);

            VaultPlayerInfoNode info = new VaultPlayerInfoNode();
            info.BaseNode.fCreatorIdx = player.BaseNode.ID;
            info.PlayerID = player.BaseNode.ID;
            info.PlayerName = req.fName;
            ICreateNode(info.BaseNode);

            VaultAgeInfoListNode agesiown = new VaultAgeInfoListNode();
            agesiown.BaseNode.fCreatorIdx = player.BaseNode.ID;
            agesiown.FolderType = EStandardNode.kAgesIOwnFolder;
            ICreateNode(agesiown.BaseNode);

            VaultAgeInfoListNode agesicanvisit = new VaultAgeInfoListNode();
            agesicanvisit.BaseNode.fCreatorIdx = player.BaseNode.ID;
            agesicanvisit.FolderType = EStandardNode.kAgesICanVisitFolder;
            ICreateNode(agesicanvisit.BaseNode);

            VaultFolderNode agejournals = new VaultFolderNode();
            agejournals.BaseNode.fCreatorIdx = player.BaseNode.ID;
            agejournals.FolderType = EStandardNode.kAgeJournalsFolder;
            ICreateNode(agejournals.BaseNode);

            VaultPlayerInfoListNode buddies = new VaultPlayerInfoListNode();
            buddies.BaseNode.fCreatorIdx = player.BaseNode.ID;
            buddies.FolderType = EStandardNode.kBuddyListFolder;
            ICreateNode(buddies.BaseNode);

            VaultPlayerInfoListNode recents = new VaultPlayerInfoListNode();
            recents.BaseNode.fCreatorIdx = player.BaseNode.ID;
            recents.FolderType = EStandardNode.kPeopleIKnowAboutFolder;
            ICreateNode(recents.BaseNode);

            VaultPlayerInfoListNode ignore = new VaultPlayerInfoListNode();
            ignore.BaseNode.fCreatorIdx = player.BaseNode.ID;
            ignore.FolderType = EStandardNode.kIgnoreListFolder;
            ICreateNode(ignore.BaseNode);

            VaultFolderNode inbox = new VaultFolderNode();
            inbox.BaseNode.fCreatorIdx = player.BaseNode.ID;
            inbox.FolderType = EStandardNode.kInboxFolder;
            ICreateNode(inbox.BaseNode);

            VaultFolderNode invites = new VaultFolderNode();
            invites.BaseNode.fCreatorIdx = player.BaseNode.ID;
            invites.FolderType = EStandardNode.kPlayerInviteFolder;
            ICreateNode(invites.BaseNode);

            VaultFolderNode outfit = new VaultFolderNode();
            outfit.BaseNode.fCreatorIdx = player.BaseNode.ID;
            outfit.FolderType = EStandardNode.kAvatarOutfitFolder;
            ICreateNode(outfit.BaseNode);

            VaultFolderNode closet = new VaultFolderNode();
            closet.BaseNode.fCreatorIdx = player.BaseNode.ID;
            closet.FolderType = EStandardNode.kAvatarClosetFolder;
            ICreateNode(closet.BaseNode);

            VaultFolderNode chron = new VaultFolderNode();
            chron.BaseNode.fCreatorIdx = player.BaseNode.ID;
            chron.FolderType = EStandardNode.kChronicleFolder;
            ICreateNode(chron.BaseNode);

            //Spin the web
            ICreateNodeRef(player.BaseNode.ID, info.BaseNode.ID, player.BaseNode.ID);
            ICreateNodeRef(player.BaseNode.ID, agesiown.BaseNode.ID, player.BaseNode.ID);
            ICreateNodeRef(player.BaseNode.ID, agesicanvisit.BaseNode.ID, player.BaseNode.ID);
            ICreateNodeRef(player.BaseNode.ID, agejournals.BaseNode.ID, player.BaseNode.ID);
            ICreateNodeRef(player.BaseNode.ID, buddies.BaseNode.ID, player.BaseNode.ID);
            ICreateNodeRef(player.BaseNode.ID, recents.BaseNode.ID, player.BaseNode.ID);
            ICreateNodeRef(player.BaseNode.ID, ignore.BaseNode.ID, player.BaseNode.ID);
            ICreateNodeRef(player.BaseNode.ID, inbox.BaseNode.ID, player.BaseNode.ID);
            ICreateNodeRef(player.BaseNode.ID, invites.BaseNode.ID, player.BaseNode.ID);
            ICreateNodeRef(player.BaseNode.ID, outfit.BaseNode.ID, player.BaseNode.ID);
            ICreateNodeRef(player.BaseNode.ID, closet.BaseNode.ID, player.BaseNode.ID);
            ICreateNodeRef(player.BaseNode.ID, chron.BaseNode.ID, player.BaseNode.ID);
            ICreateNodeRef(player.BaseNode.ID, SystemNode, player.BaseNode.ID);

            //Respond
            Vault_CreatePlayerReply reply = new Vault_CreatePlayerReply();
            reply.fModel = req.fModel;
            reply.fName = req.fName;
            reply.fPlayerID = player.BaseNode.ID;
            reply.fTransID = req.fTransID;

            fStream.WriteUShort((ushort)VaultSrv2Cli.CreatePlayerReply);
            reply.Write(fStream);
        }

        private void IFetchNode() {
            Vault_FetchNode req = new Vault_FetchNode();
            req.Read(fStream);

            Vault_FetchNodeReply reply = new Vault_FetchNodeReply();
            reply.fResult = ENetError.kNetPending;
            reply.fTransID = req.fTransID;

            SelectStatement sel = new SelectStatement();
            sel.Limit = 1;
            sel.Table = "Nodes";
            sel.Where.Add("Idx", req.fNodeID.ToString());
            sel.Wildcard = true;

            MySqlDataReader r = null;
            try {
                r = sel.ExecuteQuery(fDB);
            } catch (MySqlException e) {
                Error("MySQL Error: " + e.Message);
                fLog.DumpToLog(e.StackTrace, "Stack Trace", ELogType.kLogDebug);
                reply.fResult = ENetError.kNetErrInternalError;
            }

            if (reply.fResult == ENetError.kNetPending) {
                if (r.FieldCount == 0) reply.fResult = ENetError.kNetErrVaultNodeNotFound;
                else {
                    if (r.Read()) {
                        VaultNode node = Database.RowToVaultNode(r);
                        reply.fNodeData = node.ToArray();
                        reply.fResult = ENetError.kNetSuccess;

                        //Add to subscriptions--if nesecary
                        lock (fNodeSubscriptions) {
                            if (!fNodeSubscriptions.ContainsKey(node.ID))
                                fNodeSubscriptions.Add(node.ID, SubcriptionFlags.kNodeData);
                            else if ((fNodeSubscriptions[node.ID] & SubcriptionFlags.kNodeData) == 0)
                                fNodeSubscriptions[node.ID] |= SubcriptionFlags.kNodeData;
                        }
                    } else reply.fResult = ENetError.kNetErrVaultNodeNotFound;

                    r.Close();
                }
            }

            fStream.WriteUShort((ushort)VaultSrv2Cli.FetchNodeReply);
            reply.Write(fStream);
        }

        private void IFetchNodeRefs() {
            Vault_FetchNodeRefs req = new Vault_FetchNodeRefs();
            req.Read(fStream);

            Vault_NodeRefsFetched reply = new Vault_NodeRefsFetched();
            reply.fTransID = req.fTransID;

            SelectStatement s = new SelectStatement();
            s.Limit = 1;
            s.Select.Add("CreateTime");
            s.Table = "Nodes";
            s.Where.Add("Idx", req.fNodeID.ToString());

            MySqlDataReader r = s.ExecuteQuery(fDB);
            if (r.Read()) reply.fResult = ENetError.kNetSuccess;
            else {
                Warn("Requested NodeRefs for invalid node [ID: " + req.fNodeID.ToString() + "]");
                reply.fResult = ENetError.kNetErrVaultNodeNotFound;
            }
            r.Close();

            if (reply.fResult == ENetError.kNetSuccess) {
                List<VaultNodeRef> refs = new List<VaultNodeRef>();
                Queue<uint> nodes = new Queue<uint>();
                nodes.Enqueue(req.fNodeID);

                while (nodes.Count > 0) {
                    s = new SelectStatement();
                    s.Select.Add("ParentIdx");
                    s.Select.Add("ChildIdx");
                    s.Select.Add("SaverIdx");
                    s.Table = "NodeRefs";
                    s.Where.Add("ParentIdx", nodes.Dequeue().ToString());

                    r = s.ExecuteQuery(fDB);
                    while (r.Read()) {
                        VaultNodeRef noderef = new VaultNodeRef();
                        noderef.fChildIdx = r.GetUInt32("ChildIdx");
                        noderef.fParentIdx = r.GetUInt32("ParentIdx");
                        noderef.fSaverIdx = r.GetUInt32("SaverIdx");

                        refs.Add(noderef);
                        nodes.Enqueue(noderef.fChildIdx);

                        if (fNodeSubscriptions.ContainsKey(noderef.fParentIdx)) {
                            if ((fNodeSubscriptions[noderef.fParentIdx] & SubcriptionFlags.kNodeReferences) == 0)
                                fNodeSubscriptions[noderef.fParentIdx] |= SubcriptionFlags.kNodeReferences;
                        } else fNodeSubscriptions.Add(noderef.fParentIdx, SubcriptionFlags.kNodeReferences);
                    }

                    r.Close();
                }

                reply.fRefs = refs.ToArray();
                Debug(String.Format("Sending vault tree [PARENT: {0}] [COUNT: {1}]", req.fNodeID, refs.Count));
            }

            fStream.WriteUShort((ushort)VaultSrv2Cli.NodeRefsFetched);
            reply.Write(fStream);
        }

        private void IFindNode() {
            Vault_FindNode req = new Vault_FindNode();
            req.Read(fStream);

            Vault_FindNodeReply reply = new Vault_FindNodeReply();
            reply.fResult = ENetError.kNetSuccess;
            reply.fTransID = req.fTransID;
            
            VaultNode node = VaultNode.Parse(req.fNodeData);
            Dictionary<string, string> where = node.NodeData;

            SelectStatement sel = new SelectStatement();
            sel.Limit = 512; //This is what the real MOUL Servers do...
            sel.Select.Add("Idx");
            sel.Table = "Nodes";
            sel.Where = where;

            MySqlDataReader r = null;
            try {
                r = sel.ExecuteQuery(fDB);
            } catch (MySqlException e) {
                Error("MySQL Error: " + e.Message);
                fLog.DumpToLog(e.StackTrace, "Stack Trace", ELogType.kLogDebug);
                reply.fResult = ENetError.kNetErrInternalError;
            }

            if (reply.fResult == ENetError.kNetSuccess) {
                List<uint> nodes = new List<uint>();
                while (r.Read())
                    nodes.Add(r.GetUInt32("Idx"));
                reply.fNodeIDs = nodes.ToArray();
                r.Close();
            }

            fStream.WriteUShort((ushort)VaultSrv2Cli.FindNodeReply);
            reply.Write(fStream);
        }

        private void IPingPong() {
            Vault_PingPong ping = new Vault_PingPong();
            ping.Read(fStream);

            //Easter Egg!
            if (Encoding.UTF8.GetString(ping.fPayload) == "Hello, Mr. Vault!")
                ping.fPayload = Encoding.UTF8.GetBytes("Wassup, Dawg?");

            //Spit it back at the client
            fStream.WriteUShort((ushort)VaultSrv2Cli.PingReply);
            ping.Write(fStream);
            Verbose("PING? PONG!");
        }

        private void ISaveNode() {
            Vault_SaveNodeRequest req = new Vault_SaveNodeRequest();
            req.Read(fStream);

            Vault_SaveNodeReply reply = new Vault_SaveNodeReply();
            reply.fResult = ENetError.kNetPending;
            reply.fTransID = req.fTransID;

            VaultNode original = null;
            VaultNode changed = null;
            if (INodeExists(req.fNodeID)) {
                //Original Node
                SelectStatement sel = new SelectStatement();
                sel.Limit = 1;
                sel.Table = "Nodes";
                sel.Where.Add("Idx", req.fNodeID.ToString());
                sel.Wildcard = true;

                MySqlDataReader r = null;
                try {
                    r = sel.ExecuteQuery(fDB);
                } catch (MySqlException e) {
                    Error("MySQL Error: " + e.Message);
                    fLog.DumpToLog(e.StackTrace, "Stack Trace", ELogType.kLogDebug);
                    reply.fResult = ENetError.kNetErrInternalError;
                }

                //Actually create the node... We hope.
                if (r.Read()) {
                    original = Database.RowToVaultNode(r);
                } else {
                    Warn(String.Format("Got no results from MySQL [NodeID: {0}] on save attempt", req.fNodeID));
                    reply.fResult = ENetError.kNetErrVaultNodeNotFound;
                }

                r.Close();

                //Changed Node
                changed = VaultNode.SafeParse(req.fNodeData, false);
                if (changed.CreateTime == changed.ModifyTime)
                    changed.ModifyTime = DateTime.UtcNow.Subtract(TimeSpan.FromHours(7));
            } else {
                Warn(String.Format("Attempted to change nonexistant node [ID: {0}]", req.fNodeID));
                reply.fResult = ENetError.kNetErrVaultNodeNotFound;
            }

            //Still here?
            if (changed != null && original != null) {
                byte[] oData = original.ToArray();
                if (oData == req.fNodeData) {
                    Info(String.Format("Changed node [ID: {0}] has no differences!", req.fNodeID));
                    reply.fResult = ENetError.kNetSuccess;
                } else {
                    bool has_illegal_fields = false; //Set to true if WE change the changed node!
                    Dictionary<string, string> cData = changed.NodeData;
                    string name_illegal_fields = "[{0}]";
                    
                    //Check the changed node for illegal fields
                    string[] illegal_fields = new string[] { "Idx", "CreateTime", "CreateAgeName", 
                                                             "CreateAgeUuid", "CreatorUuid", "CreatorIdx" };
                    foreach (string field in illegal_fields) {
                        if (cData.ContainsKey(field)) {
                            cData.Remove(field);
                            name_illegal_fields = String.Format(name_illegal_fields, field + ", {0}");
                            has_illegal_fields = true;
                        }
                    }

                    if (has_illegal_fields) {
                        //We changed the node, so make sure the saver downloads our node rather than keeping his copy!
                        //NOTE: Let's just change it in the req and pretend that's perfectly okay.
                        req.fRevisionID = Guid.NewGuid();

                        //Log that the client did something illegal, but don't call The Police/Sting.
                        name_illegal_fields = name_illegal_fields.Replace(", {0}", null);
                        Warn(String.Format("Changed node [ID: {0}] has illegal fields {1}!", req.fNodeID, name_illegal_fields));
                    }

                    //Prepare UPDATE
                    UpdateStatement update = new UpdateStatement();
                    update.Data = cData;
                    update.Table = "Nodes";
                    update.Where.Add("Idx", req.fNodeID.ToString());

                    //Go for it *Cross Fingers*
                    try {
                        update.ExcecuteNonQuery(fDB);

                        //Send out the notification
                        Vault_NodeChanged notify = new Vault_NodeChanged();
                        notify.fNodeID = req.fNodeID;
                        notify.fRevisionUuid = req.fRevisionID;
                        fParent.NodeChanged(notify);

                        //Blah
                        Verbose(String.Format("Changes to Node [ID: {0}] [REV: {1}] accepted", req.fNodeID, req.fRevisionID));
                        reply.fResult = ENetError.kNetSuccess;
                    } catch (MySqlException e) {
                        Error("MySQL Error: " + e.Message);
                        fLog.DumpToLog(e.StackTrace, "Stack Trace", ELogType.kLogDebug);
                        reply.fResult = ENetError.kNetErrInternalError;
                    }
                }
            }

            //Finally, send out the reply...
            //After all of that, I hope we actually did something!
            fStream.WriteUShort((ushort)VaultSrv2Cli.SaveNodeReply);
            reply.Write(fStream);
        }

        public override void Stop() {
            if (fStream != null) fStream.Close();
            fSocket.Close();
            fParent.Remove(this);
        }
    }
        #endregion
}
