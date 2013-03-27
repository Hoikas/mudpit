using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;

namespace MUd {
    public enum ENodeType {
            kNodeInvalid, kNodeVNodeMgrLow, kNodePlayer, kNodeAge, kNodeGameServer,
            kNodeAdmin, kNodeVaultServer, kNodeCCR, kNodeVNodeMgrHigh = 21,
            kNodeFolder, kNodePlayerInfo, kNodeSystem, kNodeImage, kNodeTextNote,
            kNodeSDL, kNodeAgeLink, kNodeChronicle, kNodePlayerInfoList,
            kNodeUNUSED, kNodeMarker, kNodeAgeInfo, kNodeAgeInfoList,
            kNodeMarkerList
    }

    public enum EStandardNode {
            kInvalid = -1, kUserDefinedNode, kInboxFolder, kBuddyListFolder, kIgnoreListFolder,
            kPeopleIKnowAboutFolder, kVaultMgrGlobalDataFolder, kChronicleFolder,
            kAvatarOutfitFolder, kAgeTypeJournalFolder, kSubAgesFolder,
            kDeviceInboxFolder, kHoodMembersFolder, kAllPlayersFolder,
            kAgeMembersFolder, kAgeJournalsFolder, kAgeDevicesFolder,
            kAgeInstanceSDLNode, kAgeGlobalSDLNode, kCanVisitFolder, kAgeOwnersFolder,
            kAllAgeGlobalSDLNodesFolder, kPlayerInfoNode, kPublicAgesFolder,
            kAgesIOwnFolder, kAgesICanVisitFolder, kAvatarClosetFolder, kAgeInfoNode,
            kSystemNode, kPlayerInviteFolder, kCCRPlayersFolder, kGlobalInboxFolder,
            kChildAgesFolder, kGameScoresFolder, kLastStandardNode
    }
    
    public enum ENoteType {
            kNoteGeneric, kNoteCCRPetition, kNoteDevice, kNoteInvite, kNoteVisit,
            kNoteUnVisit
    }

    public class VaultNode {
        [Flags]
        public enum Fields : long {
            kNodeIdx = (1 << 0),
            kCreateTime = (1 << 1),
            kModifyTime = (1 << 2),
            kCreateAgeName = (1 << 3),
            kCreateAgeUuid = (1 << 4),
            kCreatorUuid = (1 << 5),
            kCreatorIdx = (1 << 6),
            kNodeType = (1 << 7),
            kInt32_1 = (1 << 8),
            kInt32_2 = (1 << 9),
            kInt32_3 = (1 << 10),
            kInt32_4 = (1 << 11),
            kUInt32_1 = (1 << 12),
            kUInt32_2 = (1 << 13),
            kUInt32_3 = (1 << 14),
            kUInt32_4 = (1 << 15),
            kUuid_1 = (1 << 16),
            kUuid_2 = (1 << 17),
            kUuid_3 = (1 << 18),
            kUuid_4 = (1 << 19),
            kString64_1 = (1 << 20),
            kString64_2 = (1 << 21),
            kString64_3 = (1 << 22),
            kString64_4 = (1 << 23),
            kString64_5 = (1 << 24),
            kString64_6 = (1 << 25),
            kIString64_1 = (1 << 26),
            kIString64_2 = (1 << 27),
            kText_1 = (1 << 28),
            kText_2 = (1 << 29),
            kBlob_1 = (1 << 30),
            kBlob_2 = (1 << 31)
        }

        public Fields NodeFields {
            get {
                Fields f = (Fields)0;
                if (fIdx != 0) f |= Fields.kNodeIdx;
                if (fCreateAgeName != String.Empty) f |= Fields.kCreateAgeName;
                if (fCreateAgeUuid != Guid.Empty) f |= Fields.kCreateAgeUuid;
                if (fCreatorIdx.HasValue) f |= Fields.kCreatorIdx;
                if (fCreatorUuid != Guid.Empty) f |= Fields.kCreatorUuid;
                if (fNodeType != ENodeType.kNodeInvalid) f |= Fields.kNodeType;
                if (fInt32[0].HasValue) f |= Fields.kInt32_1;
                if (fInt32[1].HasValue) f |= Fields.kInt32_2;
                if (fInt32[2].HasValue) f |= Fields.kInt32_3;
                if (fInt32[3].HasValue) f |= Fields.kInt32_4;
                if (fUInt32[0].HasValue) f |= Fields.kUInt32_1;
                if (fUInt32[1].HasValue) f |= Fields.kUInt32_2;
                if (fUInt32[2].HasValue) f |= Fields.kUInt32_3;
                if (fUInt32[3].HasValue) f |= Fields.kUInt32_4;
                if (fUuid[0] != Guid.Empty) f |= Fields.kUuid_1;
                if (fUuid[1] != Guid.Empty) f |= Fields.kUuid_2;
                if (fUuid[2] != Guid.Empty) f |= Fields.kUuid_3;
                if (fUuid[3] != Guid.Empty) f |= Fields.kUuid_4;
                if (fString64[0] != String.Empty) f |= Fields.kString64_1;
                if (fString64[1] != String.Empty) f |= Fields.kString64_2;
                if (fString64[2] != String.Empty) f |= Fields.kString64_3;
                if (fString64[3] != String.Empty) f |= Fields.kString64_4;
                if (fString64[4] != String.Empty) f |= Fields.kString64_5;
                if (fString64[5] != String.Empty) f |= Fields.kString64_6;
                if (fIString64[0] != String.Empty) f |= Fields.kIString64_1;
                if (fIString64[1] != String.Empty) f |= Fields.kIString64_2;
                if (fText[0] != String.Empty) f |= Fields.kText_1;
                if (fText[1] != String.Empty) f |= Fields.kText_2;
                if (fBlob[0] != null) 
                    if (fBlob[0].Length > 0) f |= Fields.kBlob_1;
                if (fBlob[1] != null) 
                    if (fBlob[1].Length > 0) f |= Fields.kBlob_2;

                return f;
            }
        }

        public uint ID {
            get { return fIdx; }
            set { if (fIdx == 0) fIdx = value; }
        }

        public DateTime CreateTime {
            get { return fCreateTime; }
        }

        public DateTime ModifyTime {
            get { return fModifyTime; }
            set { fModifyTime = value; }
        }

        public ENodeType NodeType {
            get { return fNodeType; }
        }

        public Dictionary<string, string> NodeData {
            get {
                ulong bit = 1;
                VaultNode.Fields f = NodeFields;
                Dictionary<string, string> dict = new Dictionary<string, string>();
                while (bit != 0 && bit <= (ulong)f) {
                    switch ((f & (VaultNode.Fields)bit)) {
                        case VaultNode.Fields.kBlob_1:
                            dict.Add("Blob_1", Convert.ToBase64String(fBlob[0]));
                            break;
                        case VaultNode.Fields.kBlob_2:
                            dict.Add("Blob_2", Convert.ToBase64String(fBlob[1]));
                            break;
                        case VaultNode.Fields.kCreateAgeName:
                            dict.Add("CreateAgeName", fCreateAgeName);
                            break;
                        case VaultNode.Fields.kCreateAgeUuid:
                            dict.Add("CreateAgeUuid", fCreateAgeUuid.ToString().ToLower());
                            break;
                        case VaultNode.Fields.kCreateTime:
                            dict.Add("CreateTime", VaultNode.ToUnixTime(fModifyTime).ToString());
                            break;
                        case VaultNode.Fields.kCreatorIdx:
                            dict.Add("CreatorIdx", fCreatorIdx.Value.ToString());
                            break;
                        case VaultNode.Fields.kCreatorUuid:
                            dict.Add("CreatorUuid", fCreatorUuid.ToString().ToLower());
                            break;
                        case VaultNode.Fields.kInt32_1:
                            dict.Add("Int32_1", fInt32[0].Value.ToString());
                            break;
                        case VaultNode.Fields.kInt32_2:
                            dict.Add("Int32_2", fInt32[1].Value.ToString());
                            break;
                        case VaultNode.Fields.kInt32_3:
                            dict.Add("Int32_3", fInt32[2].Value.ToString());
                            break;
                        case VaultNode.Fields.kInt32_4:
                            dict.Add("Int32_4", fInt32[3].Value.ToString());
                            break;
                        case VaultNode.Fields.kIString64_1:
                            dict.Add("IString64_1", fIString64[0]);
                            break;
                        case VaultNode.Fields.kIString64_2:
                            dict.Add("IString64_2", fIString64[1]);
                            break;
                        case VaultNode.Fields.kModifyTime:
                            dict.Add("ModifyTime", VaultNode.ToUnixTime(fModifyTime).ToString());
                            break;
                        case VaultNode.Fields.kNodeIdx:
                            dict.Add("Idx", ID.ToString());
                            break;
                        case VaultNode.Fields.kNodeType:
                            dict.Add("NodeType", NodeType.ToString("D"));
                            break;
                        case VaultNode.Fields.kString64_1:
                            dict.Add("String64_1", fString64[0]);
                            break;
                        case VaultNode.Fields.kString64_2:
                            dict.Add("String64_2", fString64[1]);
                            break;
                        case VaultNode.Fields.kString64_3:
                            dict.Add("String64_3", fString64[2]);
                            break;
                        case VaultNode.Fields.kString64_4:
                            dict.Add("String64_4", fString64[3]);
                            break;
                        case VaultNode.Fields.kString64_5:
                            dict.Add("String64_5", fString64[4]);
                            break;
                        case VaultNode.Fields.kString64_6:
                            dict.Add("String64_6", fString64[5]);
                            break;
                        case VaultNode.Fields.kText_1:
                            dict.Add("Text_1", fText[0]);
                            break;
                        case VaultNode.Fields.kText_2:
                            dict.Add("Text_2", fText[0]);
                            break;
                        case VaultNode.Fields.kUInt32_1:
                            dict.Add("UInt32_1", fUInt32[0].Value.ToString());
                            break;
                        case VaultNode.Fields.kUInt32_2:
                            dict.Add("UInt32_2", fUInt32[1].Value.ToString());
                            break;
                        case VaultNode.Fields.kUInt32_3:
                            dict.Add("UInt32_3", fUInt32[2].Value.ToString());
                            break;
                        case VaultNode.Fields.kUInt32_4:
                            dict.Add("UInt32_4", fUInt32[3].Value.ToString());
                            break;
                        case VaultNode.Fields.kUuid_1:
                            dict.Add("Uuid_1", fUuid[0].ToString().ToLower());
                            break;
                        case VaultNode.Fields.kUuid_2:
                            dict.Add("Uuid_2", fUuid[1].ToString().ToLower());
                            break;
                        case VaultNode.Fields.kUuid_3:
                            dict.Add("Uuid_3", fUuid[2].ToString().ToLower());
                            break;
                        case VaultNode.Fields.kUuid_4:
                            dict.Add("Uuid_4", fUuid[3].ToString().ToLower());
                            break;
                    }
                    bit <<= 1;
                }

                return dict;
            }
        }

        protected uint fIdx = 0;
        protected DateTime fCreateTime = DateTime.UtcNow.Subtract(TimeSpan.FromHours(7));
        internal DateTime fModifyTime = DateTime.UtcNow.Subtract(TimeSpan.FromHours(7));
        public string fCreateAgeName = String.Empty;
        public Guid fCreateAgeUuid = Guid.Empty;
        public Nullable<uint> fCreatorIdx = new Nullable<uint>();
        public Guid fCreatorUuid = Guid.Empty;
        protected ENodeType fNodeType = ENodeType.kNodeInvalid;
        public Nullable<int>[] fInt32 = new Nullable<int>[4];
        public Nullable<uint>[] fUInt32 = new Nullable<uint>[4];
        public Guid[] fUuid = new Guid[] { Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty };
        public string[] fString64 = new string[] { String.Empty, String.Empty, String.Empty, String.Empty, String.Empty, String.Empty };
        public string[] fIString64 = new string[] { String.Empty, String.Empty };
        public string[] fText = new string[] { String.Empty, String.Empty };
        public byte[][] fBlob = new byte[][] { null, null };

        public VaultNode() { }
        public VaultNode(ENodeType type) { fNodeType = type; }
        public VaultNode(ENodeType type, DateTime createTime, DateTime modifyTime) {
            fNodeType = type;
            fCreateTime = createTime;
            fModifyTime = modifyTime;
        }

        public static Guid TryGetGuid(string uuid) {
            try {
                return new Guid(uuid);
            } catch (FormatException) {
                return Guid.Empty;
            }
        }

        public static VaultNode Parse(byte[] data) {
            MemoryStream ms = new MemoryStream(data);
            UruStream s = new UruStream(ms);

            VaultNode n = new VaultNode();
            n.Read(s);

            s.Close();
            ms.Close();

            return n;
        }

        public static VaultNode SafeParse(byte[] data, bool replaceTimes) {
            VaultNode node = Parse(data);
            node.fIdx = 0;

            if (replaceTimes) {
                node.fCreateTime = DateTime.UtcNow.Subtract(TimeSpan.FromHours(7));
                node.fModifyTime = DateTime.UtcNow.Subtract(TimeSpan.FromHours(7));
            }

            return node;
        }

        public void Read(UruStream s) {
            ulong bit = 1;
            Fields f = (Fields)s.ReadULong();
            while (bit != 0 && bit <= (ulong)f) {
                switch (f & (Fields)bit) {
                    case Fields.kBlob_1:
                        fBlob[0] = s.ReadBytes(s.ReadInt());
                        break;
                    case Fields.kBlob_2:
                        fBlob[1] = s.ReadBytes(s.ReadInt());
                        break;
                    case Fields.kCreateAgeName:
                        fCreateAgeName = s.ReadUnicodeStringV32();
                        break;
                    case Fields.kCreateAgeUuid:
                        fCreateAgeUuid = new Guid(s.ReadBytes(16));
                        break;
                    case Fields.kCreateTime:
                        fCreateTime = ToDateTime(s.ReadUInt());
                        break;
                    case Fields.kCreatorIdx:
                        fCreatorIdx = s.ReadUInt();
                        break;
                    case Fields.kCreatorUuid:
                        fCreatorUuid = new Guid(s.ReadBytes(16));
                        break;
                    case Fields.kInt32_1:
                        fInt32[0] = s.ReadInt();
                        break;
                    case Fields.kInt32_2:
                        fInt32[1] = s.ReadInt();
                        break;
                    case Fields.kInt32_3:
                        fInt32[2] = s.ReadInt();
                        break;
                    case Fields.kInt32_4:
                        fInt32[3] = s.ReadInt();
                        break;
                    case Fields.kIString64_1:
                        fIString64[0] = s.ReadUnicodeStringV32();
                        break;
                    case Fields.kIString64_2:
                        fIString64[1] = s.ReadUnicodeStringV32();
                        break;
                    case Fields.kModifyTime:
                        fModifyTime = ToDateTime(s.ReadUInt());
                        break;
                    case Fields.kNodeIdx:
                        fIdx = s.ReadUInt();
                        break;
                    case Fields.kNodeType:
                        fNodeType = (ENodeType)s.ReadUInt();
                        break;
                    case Fields.kString64_1:
                        fString64[0] = s.ReadUnicodeStringV32();
                        break;
                    case Fields.kString64_2:
                        fString64[1] = s.ReadUnicodeStringV32();
                        break;
                    case Fields.kString64_3:
                        fString64[2] = s.ReadUnicodeStringV32();
                        break;
                    case Fields.kString64_4:
                        fString64[3] = s.ReadUnicodeStringV32();
                        break;
                    case Fields.kString64_5:
                        fString64[4] = s.ReadUnicodeStringV32();
                        break;
                    case Fields.kString64_6:
                        fString64[5] = s.ReadUnicodeStringV32();
                        break;
                    case Fields.kText_1:
                        fText[0] = s.ReadUnicodeStringV32();
                        break;
                    case Fields.kText_2:
                        fText[0] = s.ReadUnicodeStringV32();
                        break;
                    case Fields.kUInt32_1:
                        fUInt32[0] = s.ReadUInt();
                        break;
                    case Fields.kUInt32_2:
                        fUInt32[1] = s.ReadUInt();
                        break;
                    case Fields.kUInt32_3:
                        fUInt32[2] = s.ReadUInt();
                        break;
                    case Fields.kUInt32_4:
                        fUInt32[3] = s.ReadUInt();
                        break;
                    case Fields.kUuid_1:
                        fUuid[0] = new Guid(s.ReadBytes(16));
                        break;
                    case Fields.kUuid_2:
                        fUuid[1] = new Guid(s.ReadBytes(16));
                        break;
                    case Fields.kUuid_3:
                        fUuid[2] = new Guid(s.ReadBytes(16));
                        break;
                    case Fields.kUuid_4:
                        fUuid[3] = new Guid(s.ReadBytes(16));
                        break;
                }

                bit <<= 1;
            }
        }

        public byte[] ToArray() {
            MemoryStream ms = new MemoryStream();
            UruStream s = new UruStream(ms);

            ulong bit = 1;
            Fields f = NodeFields;
            s.WriteULong((ulong)f);
            while (bit != 0 && bit <= (ulong)f) {
                switch ((f & (Fields)bit)) {
                    case Fields.kBlob_1:
                        s.WriteInt(fBlob[0].Length);
                        s.WriteBytes(fBlob[0]);
                        break;
                    case Fields.kBlob_2:
                        s.WriteInt(fBlob[1].Length);
                        s.WriteBytes(fBlob[1]);
                        break;
                    case Fields.kCreateAgeName:
                        s.WriteUnicodeStringV32(fCreateAgeName);
                        break;
                    case Fields.kCreateAgeUuid:
                        s.WriteBytes(fCreateAgeUuid.ToByteArray());
                        break;
                    case Fields.kCreateTime:
                        s.WriteUInt(ToUnixTime(fCreateTime));
                        break;
                    case Fields.kCreatorIdx:
                        s.WriteUInt(fCreatorIdx.Value);
                        break;
                    case Fields.kCreatorUuid:
                        s.WriteBytes(fCreatorUuid.ToByteArray());
                        break;
                    case Fields.kInt32_1:
                        s.WriteInt(fInt32[0].Value);
                        break;
                    case Fields.kInt32_2:
                        s.WriteInt(fInt32[1].Value);
                        break;
                    case Fields.kInt32_3:
                        s.WriteInt(fInt32[2].Value);
                        break;
                    case Fields.kInt32_4:
                        s.WriteInt(fInt32[3].Value);
                        break;
                    case Fields.kIString64_1:
                        s.WriteUnicodeStringV32(fIString64[0]);
                        break;
                    case Fields.kIString64_2:
                        s.WriteUnicodeStringV32(fIString64[1]);
                        break;
                    case Fields.kModifyTime:
                        s.WriteUInt(ToUnixTime(fModifyTime));
                        break;
                    case Fields.kNodeIdx:
                        s.WriteUInt(fIdx);
                        break;
                    case Fields.kNodeType:
                        s.WriteUInt((uint)fNodeType);
                        break;
                    case Fields.kString64_1:
                        s.WriteUnicodeStringV32(fString64[0]);
                        break;
                    case Fields.kString64_2:
                        s.WriteUnicodeStringV32(fString64[1]);
                        break;
                    case Fields.kString64_3:
                        s.WriteUnicodeStringV32(fString64[2]);
                        break;
                    case Fields.kString64_4:
                        s.WriteUnicodeStringV32(fString64[3]);
                        break;
                    case Fields.kString64_5:
                        s.WriteUnicodeStringV32(fString64[4]);
                        break;
                    case Fields.kString64_6:
                        s.WriteUnicodeStringV32(fString64[5]);
                        break;
                    case Fields.kText_1:
                        s.WriteUnicodeStringV32(fText[0]);
                        break;
                    case Fields.kText_2:
                        s.WriteUnicodeStringV32(fText[1]);
                        break;
                    case Fields.kUInt32_1:
                        s.WriteUInt(fUInt32[0].Value);
                        break;
                    case Fields.kUInt32_2:
                        s.WriteUInt(fUInt32[1].Value);
                        break;
                    case Fields.kUInt32_3:
                        s.WriteUInt(fUInt32[2].Value);
                        break;
                    case Fields.kUInt32_4:
                        s.WriteUInt(fUInt32[3].Value);
                        break;
                    case Fields.kUuid_1:
                        s.WriteBytes(fUuid[0].ToByteArray());
                        break;
                    case Fields.kUuid_2:
                        s.WriteBytes(fUuid[1].ToByteArray());
                        break;
                    case Fields.kUuid_3:
                        s.WriteBytes(fUuid[2].ToByteArray());
                        break;
                    case Fields.kUuid_4:
                        s.WriteBytes(fUuid[3].ToByteArray());
                        break;
                }

                bit <<= 1;
            }

            byte[] buf = ms.ToArray();
            s.Close();
            ms.Close();
            return buf;
        }

        public static uint ToUnixTime(DateTime dt) {
            TimeSpan ts = (dt - new DateTime(1970, 1, 1, 0, 0, 0));
            return Convert.ToUInt32(ts.TotalSeconds);
        }

        public static DateTime ToDateTime(uint unix) {
            DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            dt = dt.AddSeconds(Convert.ToDouble(unix));
            return dt;
        }
    }

    public struct VaultNodeRef {

        public uint fParentIdx;
        public uint fChildIdx;
        public uint fSaverIdx;

        public void Read(UruStream s) {
            fParentIdx = s.ReadUInt();
            fChildIdx = s.ReadUInt();
            fSaverIdx = s.ReadUInt();
            s.ReadByte(); //Seen
        }

        public void Write(UruStream s) {
            s.WriteUInt(fParentIdx);
            s.WriteUInt(fChildIdx);
            s.WriteUInt(fSaverIdx);
            s.WriteByte(0);
        }
    }
}
