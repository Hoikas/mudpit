using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MUd {
    public abstract class VaultNodeAccess {
        protected VaultNode fBase;

        public uint CreatorID {
            get { return fBase.fCreatorIdx; }
            set { fBase.fCreatorIdx = value; }
        }

        public Guid CreatorUUID {
            get { return fBase.fCreatorUuid; }
            set { fBase.fCreatorUuid = value; }
        }

        public VaultNode BaseNode {
            get { return fBase; }
        }

        public uint ID {
            get { return fBase.ID; }
            set { fBase.ID = value; }
        }

        public VaultNodeAccess() { }
        public VaultNodeAccess(VaultNode n) { fBase = n; }
        public VaultNodeAccess(ENodeType type) { fBase = new VaultNode(type); }
    }

    public sealed class VaultAgeNode : VaultNodeAccess {

        public Guid Instance {
            get { return fBase.fUuid[0]; }
            set { fBase.fUuid[0] = value; }
        }

        public Guid ParentInstance {
            get { return fBase.fUuid[1]; }
            set { fBase.fUuid[1] = value; }
        }

        public string AgeName {
            get { return fBase.fString64[0]; }
            set { fBase.fString64[0] = value; }
        }

        public VaultAgeNode() : base(ENodeType.kNodeAge) { }
        public VaultAgeNode(VaultNode node) : base(node) { }
    }

    public sealed class VaultAgeInfoNode : VaultNodeAccess {

        public int SequenceNumber {
            get { return fBase.fInt32[0]; }
            set { fBase.fInt32[0] = value; }
        }

        public bool Public {
            get { return Convert.ToBoolean(fBase.fInt32[1]); }
            set { fBase.fInt32[1] = Convert.ToInt32(value); }
        }

        public int Language {
            get { return fBase.fInt32[2]; }
            set { fBase.fInt32[2] = value; }
        }

        public uint AgeNodeID {
            get { return fBase.fUInt32[0]; }
            set { fBase.fUInt32[0] = value; }
        }

        public uint TsarID {
            get { return fBase.fUInt32[1]; }
            set { fBase.fUInt32[1] = value; }
        }

        public uint Flags {
            get { return fBase.fUInt32[2]; }
            set { fBase.fUInt32[2] = value; }
        }

        public Guid InstanceUUID {
            get { return fBase.fUuid[0]; }
            set { fBase.fUuid[0] = value; }
        }

        public Guid ParentInstanceUUID {
            get { return fBase.fUuid[1]; }
            set { fBase.fUuid[1] = value; }
        }

        public string Filename {
            get { return fBase.fString64[1]; }
            set { fBase.fString64[1] = value; }
        }

        public string InstanceName {
            get { return fBase.fString64[2]; }
            set { fBase.fString64[2] = value; }
        }

        public string UserDefinedName {
            get { return fBase.fString64[3]; }
            set { fBase.fString64[3] = value; }
        }

        public string Description {
            get { return fBase.fText[0]; }
            set { fBase.fText[0] = value; }
        }

        public VaultAgeInfoNode() : base(ENodeType.kNodeAgeInfo) { }
        public VaultAgeInfoNode(VaultNode node) : base(node) { }
    }

    public sealed class VaultAgeLinkNode : VaultNodeAccess {

        public bool Unlocked {
            get { return Convert.ToBoolean(fBase.fInt32[0]); }
            set { fBase.fInt32[0] = Convert.ToInt32(value); }
        }

        public bool Volatile {
            get { return Convert.ToBoolean(fBase.fInt32[1]); }
            set { fBase.fInt32[1] = Convert.ToInt32(value); }
        }

        public string SpawnPoints { 
            get { return Encoding.UTF8.GetString(fBase.fBlob[0]); }
            set { fBase.fBlob[0] = Encoding.UTF8.GetBytes(value); }
        }

        public VaultAgeLinkNode() : base(ENodeType.kNodeAgeLink) { }
        public VaultAgeLinkNode(VaultNode node) : base(node) { }
    }

    public sealed class VaultChronicleNode : VaultNodeAccess {

        public int EntryType {
            get { return fBase.fInt32[0]; }
            set { fBase.fInt32[0] = value; }
        }

        public string EntryName {
            get { return fBase.fString64[0]; }
            set { fBase.fString64[0] = value; }
        }

        public string EntryValue {
            get { return fBase.fText[0]; }
            set { fBase.fText[0] = value; }
        }

        public VaultChronicleNode() : base(ENodeType.kNodeChronicle) { }
        public VaultChronicleNode(VaultNode node) : base(node) { }
    }

    public class VaultFolderNode : VaultNodeAccess {

        public EStandardNode FolderType {
            get { return (EStandardNode)fBase.fInt32[0]; }
            set { fBase.fInt32[0] = (int)value; }
        }

        public string FolderName {
            get { return fBase.fString64[0]; }
            set { fBase.fString64[0] = value; }
        }

        public VaultFolderNode() : base(ENodeType.kNodeFolder) { }
        protected VaultFolderNode(ENodeType type) : base(type) { }
        public VaultFolderNode(VaultNode node) : base(node) { }
    }

    public sealed class VaultAgeInfoListNode : VaultFolderNode {
        public VaultAgeInfoListNode() : base(ENodeType.kNodeAgeInfoList) { }
        public VaultAgeInfoListNode(VaultNode node) : base(node) { }
    }

    public sealed class VaultPlayerInfoListNode : VaultFolderNode {
        public VaultPlayerInfoListNode() : base(ENodeType.kNodePlayerInfoList) { }
        public VaultPlayerInfoListNode(VaultNode node) : base(node) { }
    }

    public sealed class VaultImageNode : VaultNodeAccess {
        public enum ImgType { kNone, kJPEG }

        public ImgType ImageType {
            get { return (ImgType)fBase.fInt32[0]; }
            set { fBase.fInt32[0] = (int)value; }
        }

        public string ImageName {
            get { return fBase.fString64[0]; }
            set { fBase.fString64[0] = value; }
        }

        public byte[] ImageData {
            get { return fBase.fBlob[0]; }
            set { fBase.fBlob[0] = value; }
        }

        public VaultImageNode() : base(ENodeType.kNodeImage) { }
        public VaultImageNode(VaultNode node) : base(node) { }
    }

    public sealed class VaultMarkerListNode : VaultNodeAccess {

        public int GameType {
            get { return fBase.fInt32[0]; }
            set { fBase.fInt32[0] = value; }
        }

        public int RoundLength {
            get { return fBase.fInt32[1]; }
            set { fBase.fInt32[1] = value; }
        }

        public string OwnerName {
            get { return fBase.fString64[0]; }
            set { fBase.fString64[0] = value; }
        }

        public VaultMarkerListNode() : base(ENodeType.kNodeMarkerList) { }
        public VaultMarkerListNode(VaultNode node) : base(node) { }
    }

    public sealed class VaultMarkerNode : VaultNodeAccess {

        public string AgeName {
            get { return fBase.fCreateAgeName; }
            set { fBase.fCreateAgeName = value; }
        }

        public int Torans {
            get { return fBase.fInt32[0]; }
            set { fBase.fInt32[0] = value; }
        }

        public int HSpans {
            get { return fBase.fInt32[1]; }
            set { fBase.fInt32[1] = value; }
        }

        public int VSpans {
            get { return fBase.fInt32[2]; }
            set { fBase.fInt32[2] = value; }
        }

        public uint PosX {
            get { return fBase.fUInt32[0]; }
            set { fBase.fUInt32[0] = value; }
        }

        public uint PosY {
            get { return fBase.fUInt32[1]; }
            set { fBase.fUInt32[1] = value; }
        }

        public uint PosZ {
            get { return fBase.fUInt32[2]; }
            set { fBase.fUInt32[2] = value; }
        }

        public string MarkerText {
            get { return fBase.fText[0]; }
            set { fBase.fText[0] = value; }
        }

        public VaultMarkerNode() : base(ENodeType.kNodeMarker) { }
        public VaultMarkerNode(VaultNode node) : base(node) { }
    }

    public sealed class VaultPlayerInfoNode : VaultNodeAccess {

        public bool Online {
            get { return Convert.ToBoolean(fBase.fInt32[0]); }
            set { fBase.fInt32[0] = Convert.ToInt32(value); }
        }

        public uint PlayerID {
            get { return fBase.fUInt32[0]; }
            set { fBase.fUInt32[0] = value; }
        }

        public Guid AgeInstanceUUID {
            get { return fBase.fUuid[0]; }
            set { fBase.fUuid[0] = value; }
        }

        public string AgeInstanceName {
            get { return fBase.fString64[0]; }
            set { fBase.fString64[0] = value; }
        }

        public string PlayerName {
            get { return fBase.fIString64[0]; }
            set { fBase.fIString64[0] = value; }
        }

        public VaultPlayerInfoNode() : base(ENodeType.kNodePlayerInfo) { }
        public VaultPlayerInfoNode(VaultNode node) : base(node) { }
    }

    public sealed class VaultPlayerNode : VaultNodeAccess {
        
        public bool Banned {
            get { return Convert.ToBoolean(fBase.fInt32[0]); }
            set { fBase.fInt32[0] = Convert.ToInt32(value); }
        }

        public bool Explorer {
            get { return Convert.ToBoolean(fBase.fInt32[1]); }
            set { fBase.fInt32[1] = Convert.ToInt32(value); }
        }

        public TimeSpan OnlineTime {
            get { return TimeSpan.FromSeconds((double)fBase.fUInt32[0]); }
            set { fBase.fUInt32[0] = (uint)value.TotalSeconds; }
        }

        public Guid AccountUUID {
            get { return fBase.fUuid[0]; }
            set { fBase.fUuid[0] = value; }
        }

        public string AvatarShape {
            get { return fBase.fString64[0]; }
            set { fBase.fString64[0] = value; }
        }

        public string PlayerName {
            get { return fBase.fIString64[0]; }
            set { fBase.fIString64[0] = value; }
        }

        public VaultPlayerNode() : base(ENodeType.kNodePlayer) { }
        public VaultPlayerNode(VaultNode node) : base(node) { }
    }

    public sealed class VaultSDLNode : VaultNodeAccess {

        public int StateIdent {
            get { return fBase.fInt32[0]; }
            set { fBase.fInt32[0] = value; }
        }

        public string StateName {
            get { return fBase.fString64[0]; }
            set { fBase.fString64[0] = value; }
        }

        /*
         * public StateDataRecord StateData {
         * 
         * }
         */

        public VaultSDLNode() : base(ENodeType.kNodeSDL) { }
        public VaultSDLNode(VaultNode node) : base(node) { }
    }

    public sealed class VaultSystemNode : VaultNodeAccess {

        public int CCRStatus {
            get { return fBase.fInt32[0]; }
            set { fBase.fInt32[0] = value; }
        }

        public VaultSystemNode() : base(ENodeType.kNodeSystem) { }
        public VaultSystemNode(VaultNode node) : base(node) { }
    }

    public sealed class VaultTextNode : VaultNodeAccess {

        public ENoteType NodeType {
            get { return (ENoteType)fBase.fInt32[0]; }
            set { fBase.fInt32[0] = (int)value; }
        }

        public ENoteType NodeSubType {
            get { return (ENoteType)fBase.fInt32[1]; }
            set { fBase.fInt32[1] = (int)value; }
        }

        public string NoteName {
            get { return fBase.fString64[0]; }
            set { fBase.fString64[0] = value; }
        }

        public string Text {
            get { return fBase.fText[0]; }
            set { fBase.fText[0] = value; }
        }

        public VaultTextNode() : base(ENodeType.kNodeTextNote) { }
        public VaultTextNode(VaultNode node) : base(node) { }
    }
}
