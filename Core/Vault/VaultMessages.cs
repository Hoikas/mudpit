using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MUd {
    public enum VaultCli2Srv {
        PingRequest,
        CreatePlayerRequest,
        FetchNodeRefs,
        FindNode,
        FetchNode,
        CreateAgeRequest,
        CreateNodeRequest,
        AddNodeRequest,
        SaveNodeRequest,
    }

    public enum VaultSrv2Cli {
        PingReply,
        CreatePlayerReply,
        NodeRefsFetched,
        FindNodeReply,
        FetchNodeReply,
        CreateAgeReply,
        CreateNodeReply,
        AddNodeReply,
        AddNodeNotify,
        SaveNodeReply,
        NodeChanged,
    }

    public struct Vault_AddNodeNotify {
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

    public struct Vault_AddNodeReply {
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

    public struct Vault_AddNodeRequest {
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

    public struct Vault_CreateAgeReply {
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

    public struct Vault_CreateAgeRequest {
        public uint fTransID;
        public Guid fAgeUUID, fParentUUID;
        public string fFilename;     //Len  260
        public string fInstanceName; //Len  260
        public string fUserName;     //Len  260
        public string fDescription;  //Len 1024
        public int fSequenceNumber;
        public int fLanguage;
        public uint fCreatorID;

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
            fCreatorID = s.ReadUInt();
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
            s.WriteUInt(fCreatorID);
        }
    }

    public struct Vault_CreateNodeReply {
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

    public struct Vault_CreateNodeRequest {
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

    public struct Vault_CreatePlayerReply {
        public uint fTransID;
        public uint fPlayerID;
        public string fName;  //Len 40
        public string fModel; //Len 64

        public void Read(UruStream s) {
            fTransID = s.ReadUInt();
            fPlayerID = s.ReadUInt();
            fName = s.ReadUnicodeStringV16(40);
            fModel = s.ReadUnicodeStringV16(64);
        }

        public void Write(UruStream s) {
            s.WriteUInt(fTransID);
            s.WriteUInt(fPlayerID);
            s.WriteUnicodeStringV16(fName, 40);
            s.WriteUnicodeStringV16(fModel, 64);
        }
    }

    public struct Vault_CreatePlayerRequest {
        public uint fTransID;
        public Guid fAcctUUID;
        public string fName;   //Len 40
        public string fModel;  //Len 64
        public string fInvite; //Len 260

        public void Read(UruStream s) {
            fTransID = s.ReadUInt();
            fAcctUUID = new Guid(s.ReadBytes(16));
            fName = s.ReadUnicodeStringV16(40);
            fModel = s.ReadUnicodeStringV16(64);
            fInvite = s.ReadUnicodeStringV16(260);
        }

        public void Write(UruStream s) {
            s.WriteUInt(fTransID);
            s.WriteBytes(fAcctUUID.ToByteArray());
            s.WriteUnicodeStringV16(fName, 40);
            s.WriteUnicodeStringV16(fModel, 64);
            s.WriteUnicodeStringV16(fInvite, 260);
        }
    }

    public struct Vault_FetchNode {
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

    public struct Vault_FetchNodeReply {
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

    public struct Vault_FetchNodeRefs {
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

    public struct Vault_FindNode {
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

    public struct Vault_FindNodeReply {
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

    public struct Vault_NodeChanged {
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

    public struct Vault_NodeRefsFetched {
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

    public struct Vault_PingPong {
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

    public struct Vault_SaveNodeRequest {
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

    public struct Vault_SaveNodeReply {
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
