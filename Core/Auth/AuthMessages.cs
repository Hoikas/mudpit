using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MUd {
    public enum AuthCli2Srv {
        PingRequest = 0, ClientRegisterRequest,
        ClientSetCCRLevel, AcctLoginRequest,
        AcctSetEulaVersion, AcctSetDataRequest,
        AcctSetPlayerRequest, AcctCreateRequest,
        AcctChangePasswordRequest, AcctSetRolesRequest,
        AcctSetBillingTypeRequest, AcctActivateRequest,
        AcctCreateFromKeyRequest, PlayerDeleteRequest,
        PlayerUndeleteRequest, PlayerSelectRequest,
        PlayerRenameRequest, PlayerCreateRequest,
        PlayerSetStatus, PlayerChat,
        UpgradeVisitorRequest, SetPlayerBanStatusRequest,
        KickPlayer, ChangePlayerNameRequest,
        SendFriendInviteRequest, VaultNodeCreate,
        VaultNodeFetch, VaultNodeSave,
        VaultNodeDelete, VaultNodeAdd,
        VaultNodeRemove, VaultFetchNodeRefs,
        VaultInitAgeRequest, VaultNodeFind,
        VaultSetSeen, VaultSendNode, AgeRequest,
        FileListRequest, FileDownloadRequest,
        FileDownloadChunkAck, PropagateBuffer,
        GetPublicAgeList, SetAgePublic,
        LogPythonTraceback, LogStackDump,
        LogClientDebuggerConnect, ScoreCreate,
        ScoreDelete, ScoreGetScores, ScoreAddPoints,
        ScoreTransferPoints, ScoreSetPoints,
        ScoreGetRanks, AcctExistsRequest,
    }

    public enum AuthSrv2Cli {
        PingReply = 0, ServerAddr, NotifyNewBuild,
        ClientRegisterReply, AcctLoginReply, AcctData,
        AcctPlayerInfo, AcctSetPlayerReply,
        AcctCreateReply, AcctChangePasswordReply,
        AcctSetRolesReply, AcctSetBillingTypeReply,
        AcctActivateReply, AcctCreateFromKeyReply,
        PlayerList, PlayerChat, PlayerCreateReply,
        PlayerDeleteReply, UpgradeVisitorReply,
        SetPlayerBanStatusReply, ChangePlayerNameReply,
        SendFriendInviteReply, Unknown_22,
        VaultNodeCreated, VaultNodeFetched,
        VaultNodeChanged, VaultNodeDeleted,
        VaultNodeAdded, VaultNodeRemoved,
        VaultNodeRefsFetched, VaultInitAgeReply,
        VaultNodeFindReply, VaultSaveNodeReply,
        VaultAddNodeReply, VaultRemoveNodeReply,
        AgeReply, FileListReply, FileDownloadChunk,
        PropagateBuffer, KickedOff, PublicAgeList,
        ScoreCreateReply, ScoreDeleteReply,
        ScoreGetScoresReply, ScoreAddPointsReply,
        ScoreTransferPointsReply, ScoreSetPointsReply,
        ScoreGetRanksReply, AcctExistsReply,
    }

    public struct Auth_AgeReply {
        public uint fTransID;
        public ENetError fResult;
        public uint fAgeMcpID;
        public Guid fAgeInstanceUuid;
        public uint fAgeVaultID;
        public IPAddress fGameServerIP;

        public void Read(UruStream s) {
            fTransID = s.ReadUInt();
            fResult = (ENetError)s.ReadInt();
            fAgeMcpID = s.ReadUInt();
            fAgeInstanceUuid = new Guid(s.ReadBytes(16));
            fAgeVaultID = s.ReadUInt();

            byte[] game = s.ReadBytes(4);
            Array.Reverse(game);
            fGameServerIP = new IPAddress(game);
        }

        public void Write(UruStream s) {
            s.WriteUInt(fTransID);
            s.WriteInt((int)fResult);
            s.WriteUInt(fAgeMcpID);
            s.WriteBytes(fAgeInstanceUuid.ToByteArray());
            s.WriteUInt(fAgeVaultID);

            if (fGameServerIP == null) {
                s.WriteInt(0);
            } else {
                //Little Endian IP
                byte[] ip = fGameServerIP.GetAddressBytes();
                Array.Reverse(ip);
                s.WriteBytes(ip);
            }
        }
    }

    public struct Auth_AgeRequest {
        public uint fTransID;
        public string fAgeName;        //Len 40
        public Guid fAgeInstanceUuid;

        public void Read(UruStream s) {
            fTransID = s.ReadUInt();
            fAgeName = s.ReadUnicodeStringV16(40);
            fAgeInstanceUuid = new Guid(s.ReadBytes(16));
        }

        public void Write(UruStream s) {
            s.WriteUInt(fTransID);
            s.WriteUnicodeStringV16(fAgeName, 40);
            s.WriteBytes(fAgeInstanceUuid.ToByteArray());
        }
    }

    public struct Auth_DebuggerAttached {
        public uint fUnused;

        public void Read(UruStream s) {
            fUnused = s.ReadUInt();
        }

        public void Write(UruStream s) {
            s.WriteUInt(fUnused);
        }
    }

    public struct Auth_FileDownloadChunk {
        public uint fTransID;
        public ENetError fResult;
        public uint fFileSize;
        public uint fChunkPos;
        public byte[] fChunkData;

        public void Read(UruStream s) {
            fTransID = s.ReadUInt();
            fResult = (ENetError)s.ReadInt();
            fFileSize = s.ReadUInt();
            fChunkPos = s.ReadUInt();
            fChunkData = s.ReadBytes(s.ReadInt());
        }

        public void Write(UruStream s) {
            s.WriteUInt(fTransID);
            s.WriteInt((int)fResult);
            s.WriteUInt(fFileSize);
            s.WriteUInt(fChunkPos);
            if (fChunkData == null) fChunkData = new byte[0];
            s.WriteInt(fChunkData.Length);
            s.WriteBytes(fChunkData);
        }
    }

    public struct Auth_FileDownloadChunkAck {
        public uint fTransID;

        public void Read(UruStream s) {
            fTransID = s.ReadUInt();
        }

        public void Write(UruStream s) {
            s.WriteUInt(fTransID);
        }
    }

    public struct Auth_FileDownloadRequest {
        public uint fTransID;
        public string fDownload; //Len 260

        public void Read(UruStream s) {
            fTransID = s.ReadUInt();
            fDownload = s.ReadUnicodeStringV16(260);
        }

        public void Write(UruStream s) {
            s.WriteUInt(fTransID);
            s.WriteUnicodeStringV16(fDownload, 260);
        }
    }

    public struct Auth_FileListRequest {
        public uint fTransID;
        public string fDirectory; //Len 260
        public string fExtension; //Len 256

        public void Read(UruStream s) {
            fTransID = s.ReadUInt();
            fDirectory = s.ReadUnicodeStringV16(260);
            fExtension = s.ReadUnicodeStringV16(256);
        }

        public void Write(UruStream s) {
            s.WriteUInt(fTransID);
            s.WriteUnicodeStringV16(fDirectory, 260);
            s.WriteUnicodeStringV16(fExtension, 256);
        }
    }

    public struct Auth_FileListReply {
        public uint fTransID;
        public ENetError fResult;
        public byte[] fData;

        public void Read(UruStream s) {
            fTransID = s.ReadUInt();
            fResult = (ENetError)s.ReadInt();
            fData = s.ReadBytes(s.ReadInt() * 2);
        }

        public void Write(UruStream s) {
            s.WriteUInt(fTransID);
            s.WriteInt((int)fResult);
            if (fData == null) fData = new byte[0];
            s.WriteInt(fData.Length / 2);
            s.WriteBytes(fData);
        }
    }

    public struct Auth_GetPublicAges {
        public uint fTransID;
        public string fFilename; //Len 64

        public void Read(UruStream s) {
            fTransID = s.ReadUInt();
            fFilename = s.ReadUnicodeStringV16(64);
        }

        public void Write(UruStream s) {
            s.WriteUInt(fTransID);
            s.WriteUnicodeStringV16(fFilename, 64);
        }
    }

    public struct Auth_GotPublicAges {
        public uint fTransID;
        public ENetError fResult;
        public NetAgeInfo[] fAges;

        public void Read(UruStream s) {
            fTransID = s.ReadUInt();
            fResult = (ENetError)s.ReadInt();
            fAges = new NetAgeInfo[s.ReadInt()];
            for (int i = 0; i < fAges.Length; i++) {
                fAges[i] = new NetAgeInfo();
                fAges[i].Read(s);
            }
        }

        public void Write(UruStream s) {
            s.WriteUInt(fTransID);
            s.WriteInt((int)fResult);
            s.WriteInt(fAges.Length);
            foreach (NetAgeInfo nai in fAges)
                nai.Write(s);
        }
    }

    public struct Auth_InitAgeReply {
        public uint fTransID;
        public ENetError fResult;
        public uint fAgeNodeID;
        public uint fInfoNodeID;

        public void Read(UruStream s) {
            fTransID = s.ReadUInt();
            fResult = (ENetError)s.ReadInt();
            fAgeNodeID = s.ReadUInt();
            fInfoNodeID = s.ReadUInt();
        }

        public void Write(UruStream s) {
            s.WriteUInt(fTransID);
            s.WriteInt((int)fResult);
            s.WriteUInt(fAgeNodeID);
            s.WriteUInt(fInfoNodeID);
        }
    }

    public struct Auth_InitAgeRequest {
        public uint fTransID;
        public Guid fAgeUUID, fParentUUID;
        public string fFilename;     //Len  260
        public string fInstanceName; //Len  260
        public string fUserName;     //Len  260
        public string fDescription;  //Len 1024
        public int fSequenceNumber;
        public int fLanguage;

        public void Read(UruStream s) {
            fTransID = s.ReadUInt();
            fAgeUUID = new Guid(s.ReadBytes(16));
            fParentUUID = new Guid(s.ReadBytes(16));
            fFilename = s.ReadUnicodeStringV16(260);
            fInstanceName = s.ReadUnicodeStringV16(260);
            fUserName = s.ReadUnicodeStringV16(260);
            fDescription = s.ReadUnicodeStringV16(1024);
            fSequenceNumber = s.ReadInt();
            fLanguage = s.ReadInt();
        }

        public void Write(UruStream s) {
            s.WriteUInt(fTransID);
            s.WriteBytes(fAgeUUID.ToByteArray());
            s.WriteBytes(fParentUUID.ToByteArray());
            s.WriteUnicodeStringV16(fFilename, 260);
            s.WriteUnicodeStringV16(fInstanceName, 260);
            s.WriteUnicodeStringV16(fUserName, 260);
            s.WriteUnicodeStringV16(fDescription, 1024);
            s.WriteInt(fSequenceNumber);
            s.WriteInt(fLanguage);
        }
    }

    public struct Auth_KickedOff {
        public ENetError fReason;

        public void Read(UruStream s) {
            fReason = (ENetError)s.ReadInt();
        }

        public void Write(UruStream s) {
            s.WriteInt((int)fReason);
        }
    }

    public struct Auth_LogDump {
        public string fErrorMsg;

        public void Read(UruStream s) {
            fErrorMsg = s.ReadUnicodeStringV16(1024);
        }

        public void Write(UruStream s) {
            s.WriteUnicodeStringV16(fErrorMsg, 1024);
        }
    }

    public struct Auth_LoginRequest {
        public uint fTransID;
        public int fChallenge;
        public string fAccount;   //Len 64
        public ShaHash fHash;     //Len 20
        public string fAuthToken; //Len 64
        public string fOS;        //Len 08

        public void Read(UruStream s) {
            fTransID = s.ReadUInt();
            fChallenge = s.ReadInt();
            fAccount = s.ReadUnicodeStringV16(64);
            uint[] hash = new uint[5];
            for (int i = 0; i < hash.Length; i++)
                hash[i] = s.ReadUInt();
            fHash = new ShaHash(hash);
            fAuthToken = s.ReadUnicodeStringV16(64);
            fOS = s.ReadUnicodeStringV16(8);
        }

        public void Write(UruStream s) {
            s.WriteUInt(fTransID);
            s.WriteInt(fChallenge);
            s.WriteUnicodeStringV16(fAccount, 64);
            for (int i = 0; i < fHash.UruHash.Length; i++)
                s.WriteUInt(fHash.UruHash[i]);
            s.WriteUnicodeStringV16(fAuthToken, 64);
            s.WriteUnicodeStringV16(fOS, 8);
        }
    }

    public struct Auth_LoginReply {
        public uint fTransID;
        public ENetError fResult;
        public Guid fAcctGuid;
        public uint fFlags;
        public uint fBillingType;
        public uint[] fDroidKey;

        public void Read(UruStream s) {
            fTransID = s.ReadUInt();
            fResult = (ENetError)s.ReadInt();
            fAcctGuid = new Guid(s.ReadBytes(16));
            fFlags = s.ReadUInt();
            fBillingType = s.ReadUInt();
            fDroidKey = new uint[4];
            for (int i = 0; i < fDroidKey.Length; i++)
                fDroidKey[i] = s.ReadUInt();
        }

        public void Write(UruStream s) {
            s.WriteUInt(fTransID);
            s.WriteInt((int)fResult);
            s.WriteBytes(fAcctGuid.ToByteArray());
            s.WriteUInt(fFlags);
            s.WriteUInt(fBillingType);
            if (fDroidKey == null) fDroidKey = new uint[4];
            for (int i = 0; i < fDroidKey.Length; i++)
                s.WriteUInt(fDroidKey[i]);
        }
    }

    public struct Auth_PlayerCreateReply {
        public uint fTransID;
        public ENetError fResult;
        public uint fPlayerID;
        public uint fExplorer;
        public string fName;  //Len 40
        public string fModel; //Len 64

        public void Read(UruStream s) {
            fTransID = s.ReadUInt();
            fResult = (ENetError)s.ReadInt();
            fPlayerID = s.ReadUInt();
            fExplorer = s.ReadUInt();
            fName = s.ReadUnicodeStringV16(40);
            fModel = s.ReadUnicodeStringV16(64);
        }

        public void Write(UruStream s) {
            s.WriteUInt(fTransID);
            s.WriteInt((int)fResult);
            s.WriteUInt(fPlayerID);
            s.WriteUInt(fExplorer);
            s.WriteUnicodeStringV16(fName, 40);
            s.WriteUnicodeStringV16(fModel, 64);
        }
    }

    public struct Auth_PlayerCreateRequest {
        public uint fTransID;
        public string fName;   //Len 40
        public string fModel;  //Len 260
        public string fInvite; //Len 260

        public void Read(UruStream s) {
            fTransID = s.ReadUInt();
            fName = s.ReadUnicodeStringV16(40);
            fModel = s.ReadUnicodeStringV16(260);
            fInvite = s.ReadUnicodeStringV16(260);
        }

        public void Write(UruStream s) {
            s.WriteUInt(fTransID);
            s.WriteUnicodeStringV16(fName, 40);
            s.WriteUnicodeStringV16(fModel, 260);
            s.WriteUnicodeStringV16(fInvite, 260);
        }
    }

    public struct Auth_PlayerInfo {
        public uint fTransID;
        public uint fPlayerID;
        public string fPlayerName; //Len 40
        public string fModel;      //Len 64
        public uint fExplorer;

        public void Read(UruStream s) {
            fTransID = s.ReadUInt();
            fPlayerID = s.ReadUInt();
            fPlayerName = s.ReadUnicodeStringV16(40);
            fModel = s.ReadUnicodeStringV16(64);
            fExplorer = s.ReadUInt();
        }

        public void Write(UruStream s) {
            s.WriteUInt(fTransID);
            s.WriteUInt(fPlayerID);
            s.WriteUnicodeStringV16(fPlayerName, 40);
            s.WriteUnicodeStringV16(fModel, 64);
            s.WriteUInt(fExplorer);
        }
    }

    public struct Auth_PingPong {
        public uint fPingTime;
        public uint fTransID;
        public byte[] fPayload;

        public void Read(UruStream s) {
            fPingTime = s.ReadUInt();
            fTransID = s.ReadUInt();
            fPayload = s.ReadBytes(s.ReadInt());
        }

        public void Write(UruStream s) {
            s.WriteUInt(fPingTime);
            s.WriteUInt(fTransID);
            if (fPayload == null) s.WriteInt(0);
            else {
                s.WriteInt(fPayload.Length);
                s.WriteBytes(fPayload);
            }
        }
    }

    public struct Auth_RegisterRequest {
        public uint fBuildID;

        public void Read(UruStream s) {
            fBuildID = s.ReadUInt();
        }

        public void Write(UruStream s) {
            s.WriteUInt(fBuildID);
        }
    }

    public struct Auth_RegisterReply {
        public uint fChallenge;

        public void Read(UruStream s) {
            fChallenge = s.ReadUInt();
        }

        public void Write(UruStream s) {
            s.WriteUInt(fChallenge);
        }
    }

    public struct Auth_ServerAddr {
        public int fAddr;
        public Guid fToken;

        public void Read(UruStream s) {
            fAddr = s.ReadInt();
            fToken = new Guid(s.ReadBytes(16));
        }

        public void Write(UruStream s) {
            s.WriteInt(fAddr);
            s.WriteBytes(fToken.ToByteArray());
        }
    }

    public struct Auth_SetAgePublic {
        public uint fAgeInfoID;
        public bool fPublic;

        public void Read(UruStream s) {
            fAgeInfoID = s.ReadUInt();
            fPublic = s.ReadBool();
        }

        public void Write(UruStream s) {
            s.WriteUInt(fAgeInfoID);
            s.WriteBool(fPublic);
        }
    }

    public struct Auth_SetPlayerReply {
        public uint fTransID;
        public ENetError fResult;

        public void Read(UruStream s) {
            fTransID = s.ReadUInt();
            fResult = (ENetError)s.ReadInt();
        }

        public void Write(UruStream s) {
            s.WriteUInt(fTransID);
            s.WriteInt((int)fResult);
        }
    }

    public struct Auth_SetPlayerRequest {
        public uint fTransID;
        public uint fPlayerID;

        public void Read(UruStream s) {
            fTransID = s.ReadUInt();
            fPlayerID = s.ReadUInt();
        }

        public void Write(UruStream s) {
            s.WriteUInt(fTransID);
            s.WriteUInt(fPlayerID);
        }
    }

    public struct Auth_VaultFetchNodeRefs {
        public uint fTransID;
        public uint fNodeID;

        public void Read(UruStream s) {
            fTransID = s.ReadUInt();
            fNodeID = s.ReadUInt();
        }

        public void Write(UruStream s) {
            s.WriteUInt(fTransID);
            s.WriteUInt(fNodeID);
        }
    }

    public struct Auth_VaultNodeAdd {
        public uint fTransID;
        public uint fParentID;
        public uint fChildID;
        public uint fSaverID;

        public void Read(UruStream s) {
            fTransID = s.ReadUInt();
            fParentID = s.ReadUInt();
            fChildID = s.ReadUInt();
            fSaverID = s.ReadUInt();
        }

        public void Write(UruStream s) {
            s.WriteUInt(fTransID);
            s.WriteUInt(fParentID);
            s.WriteUInt(fChildID);
            s.WriteUInt(fSaverID);
        }
    }

    public struct Auth_VaultNodeAdded {
        public uint fParentID;
        public uint fChildID;
        public uint fSaverID;

        public void Read(UruStream s) {
            fParentID = s.ReadUInt();
            fChildID = s.ReadUInt();
            fSaverID = s.ReadUInt();
        }

        public void Write(UruStream s) {
            s.WriteUInt(fParentID);
            s.WriteUInt(fChildID);
            s.WriteUInt(fSaverID);
        }
    }

    public struct Auth_VaultNodeAddReply {
        public uint fTransID;
        public ENetError fResult;

        public void Read(UruStream s) {
            fTransID = s.ReadUInt();
            fResult = (ENetError)s.ReadInt();
        }

        public void Write(UruStream s) {
            s.WriteUInt(fTransID);
            s.WriteInt((int)fResult);
        }
    }

    public struct Auth_VaultNodeChanged {
        public uint fNodeID;
        public Guid fRevisionUuid;

        public void Read(UruStream s) {
            fNodeID = s.ReadUInt();
            fRevisionUuid = new Guid(s.ReadBytes(16));
        }

        public void Write(UruStream s) {
            s.WriteUInt(fNodeID);
            s.WriteBytes(fRevisionUuid.ToByteArray());
        }
    }

    public struct Auth_VaultNodeCreate {
        public uint fTransID;
        public byte[] fNodeData;

        public void Read(UruStream s) {
            fTransID = s.ReadUInt();
            fNodeData = s.ReadBytes(s.ReadInt());
        }

        public void Write(UruStream s) {
            s.WriteUInt(fTransID);

            if (fNodeData == null) fNodeData = new byte[0];
            s.WriteInt(fNodeData.Length);
            s.WriteBytes(fNodeData);
        }
    }

    public struct Auth_VaultNodeCreated {
        public uint fTransID;
        public ENetError fResult;
        public uint fNodeID;

        public void Read(UruStream s) {
            fTransID = s.ReadUInt();
            fResult = (ENetError)s.ReadInt();
            fNodeID = s.ReadUInt();
        }

        public void Write(UruStream s) {
            s.WriteUInt(fTransID);
            s.WriteInt((int)fResult);
            s.WriteUInt(fNodeID);
        }
    }

    public struct Auth_VaultNodeFetch {
        public uint fTransID;
        public uint fNodeID;

        public void Read(UruStream s) {
            fTransID = s.ReadUInt();
            fNodeID = s.ReadUInt();
        }

        public void Write(UruStream s) {
            s.WriteUInt(fTransID);
            s.WriteUInt(fNodeID);
        }
    }

    public struct Auth_VaultNodeFetched {
        public uint fTransID;
        public ENetError fResult;
        public byte[] fNodeData;

        public void Read(UruStream s) {
            fTransID = s.ReadUInt();
            fResult = (ENetError)s.ReadInt();
            fNodeData = s.ReadBytes(s.ReadInt());
        }

        public void Write(UruStream s) {
            s.WriteUInt(fTransID);
            s.WriteInt((int)fResult);
            if (fNodeData == null) fNodeData = new byte[0];
            s.WriteInt(fNodeData.Length);
            s.WriteBytes(fNodeData);
        }
    }

    public struct Auth_VaultNodeFind {
        public uint fTransID;
        public byte[] fNodeData;

        public void Read(UruStream s) {
            fTransID = s.ReadUInt();
            fNodeData = s.ReadBytes(s.ReadInt());
        }

        public void Write(UruStream s) {
            s.WriteUInt(fTransID);
            s.WriteInt(fNodeData.Length);
            s.WriteBytes(fNodeData);
        }
    }

    public struct Auth_VaultNodeFindReply {
        public uint fTransID;
        public ENetError fResult;
        public uint[] fNodeIDs;

        public void Read(UruStream s) {
            fTransID = s.ReadUInt();
            fResult = (ENetError)s.ReadInt();
            fNodeIDs = new uint[s.ReadInt()];
            for (int i = 0; i < fNodeIDs.Length; i++)
                fNodeIDs[i] = s.ReadUInt();
        }

        public void Write(UruStream s) {
            s.WriteUInt(fTransID);
            s.WriteInt((int)fResult);
            if (fNodeIDs == null) fNodeIDs = new uint[0];
            s.WriteInt(fNodeIDs.Length);
            for (int i = 0; i < fNodeIDs.Length; i++)
                s.WriteUInt(fNodeIDs[i]);
        }
    }

    public struct Auth_VaultNodeRefsFetched {
        public uint fTransID;
        public ENetError fResult;
        public VaultNodeRef[] fRefs;

        public void Read(UruStream s) {
            fTransID = s.ReadUInt();
            fResult = (ENetError)s.ReadInt();
            fRefs = new VaultNodeRef[s.ReadInt()];
            
            for (int i = 0; i < fRefs.Length; i++) {
                fRefs[i] = new VaultNodeRef();
                fRefs[i].Read(s);
            }
        }

        public void Write(UruStream s) {
            s.WriteUInt(fTransID);
            s.WriteInt((int)fResult);
            s.WriteInt(fRefs.Length);
            foreach (VaultNodeRef r in fRefs)
                r.Write(s);
        }
    }

    public struct Auth_VaultNodeRemove {
        public uint fTransID;
        public uint fParentID;
        public uint fChildID;
        
        public void Read(UruStream s) {
            fTransID = s.ReadUInt();
            fParentID = s.ReadUInt();
            fChildID = s.ReadUInt();
        }

        public void Write(UruStream s) {
            s.WriteUInt(fTransID);
            s.WriteUInt(fParentID);
            s.WriteUInt(fChildID);
        }
    }

    public struct Auth_VaultNodeRemoved {
        public uint fParentID;
        public uint fChildID;

        public void Read(UruStream s) {
            fParentID = s.ReadUInt();
            fChildID = s.ReadUInt();
        }

        public void Write(UruStream s) {
            s.WriteUInt(fParentID);
            s.WriteUInt(fChildID);
        }
    }

    public struct Auth_VaultNodeRemoveReply {
        public uint fTransID;
        public ENetError fResult;

        public void Read(UruStream s) {
            fTransID = s.ReadUInt();
            fResult = (ENetError)s.ReadInt();
        }

        public void Write(UruStream s) {
            s.WriteUInt(fTransID);
            s.WriteInt((int)fResult);
        }
    }

    public struct Auth_VaultNodeSave {
        public uint fTransID;
        public uint fNodeID;
        public Guid fRevisionID;
        public byte[] fNodeData;

        public void Read(UruStream s) {
            fTransID = s.ReadUInt();
            fNodeID = s.ReadUInt();
            fRevisionID = new Guid(s.ReadBytes(16));
            fNodeData = s.ReadBytes(s.ReadInt());
        }

        public void Write(UruStream s) {
            s.WriteUInt(fTransID);
            s.WriteUInt(fNodeID);
            s.WriteBytes(fRevisionID.ToByteArray());

            if (fNodeData == null) fNodeData = new byte[0];
            s.WriteInt(fNodeData.Length);
            s.WriteBytes(fNodeData);
        }
    }

    public struct Auth_VaultNodeSaveReply {
        public uint fTransID;
        public ENetError fResult;

        public void Read(UruStream s) {
            fTransID = s.ReadUInt();
            fResult = (ENetError)s.ReadInt();
        }

        public void Write(UruStream s) {
            s.WriteUInt(fTransID);
            s.WriteInt((int)fResult);
        }
    }
}
