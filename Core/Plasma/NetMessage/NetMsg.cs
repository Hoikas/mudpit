using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MUd {
    public abstract class NetMessage : Creatable {

        [Flags]
        protected enum MsgFlags {
            kHasTimeSent = 0x1,
            kHasGameMsgRecvrs = 0x2,
            kEchoBackToSender = 0x4,
            kRequestP2P = 0x8,
            kAllowTimeOut = 0x10,
            kIndirectMember = 0x20,
            kPublicIPClient = 0x40,
            kHasContext = 0x80,
            kAskVaultForGameState = 0x100,
            kHasTransactionID = 0x200,
            kNewSDLState = 0x400,
            kInitialAgeStateRequest = 0x800,
            kHasPlayerID = 0x1000,
            kUseRelevanceRegions = 0x2000,
            kHasAcctUUID = 0x4000,
            kInterAgeRouting = 0x8000,
            kHasVersion = 0x10000,
            kIsSystemMessage = 0x20000,
            kNeedsReliableSend = 0x40000,
            kRouteToAllPlayers = 0x80000
        }

        protected MsgFlags fFlags;
        protected UnifiedTime fTimeSent = new UnifiedTime();
        protected uint fContext, fTransID, fPlayerID;
        protected Guid fAcctUuid;

        #region Properties
        public Guid AcctUUID {
            get { return fAcctUuid; }
            set {
                fAcctUuid = value;

                bool hasUuid = ((fFlags & MsgFlags.kHasAcctUUID) != 0);
                if (hasUuid && value == Guid.Empty)
                    fFlags &= ~MsgFlags.kHasTimeSent;
                else
                    fFlags |= MsgFlags.kHasTimeSent;
            }
        }

        public uint Context {
            get { return fContext; }
            set {
                fContext = value;
                fFlags |= MsgFlags.kHasContext;
            }
        }

        public uint PlayerID {
            get { return fPlayerID; }
            set {
                fPlayerID = value;
                fFlags |= MsgFlags.kHasPlayerID;
            }
        }

        public bool ReliableSend {
            get { return ((fFlags & MsgFlags.kNeedsReliableSend) != 0); }
            set {
                if (ReliableSend && !value)
                    fFlags &= ~MsgFlags.kNeedsReliableSend;
                else if (!ReliableSend && value)
                    fFlags |= MsgFlags.kNeedsReliableSend;
            }
        }

        public bool SystemMsg {
            get { return ((fFlags & MsgFlags.kIsSystemMessage) != 0); }
            set {
                if (SystemMsg && !value)
                    fFlags &= ~MsgFlags.kIsSystemMessage;
                else if (!ReliableSend && value)
                    fFlags |= MsgFlags.kIsSystemMessage;
            }
        }

        public UnifiedTime TimeSent {
            get {
                if (fTimeSent == null) return new UnifiedTime();
                else return fTimeSent;
            }

            set {
                if (value == null) return;
                fTimeSent = value;

                bool hasTime = ((fFlags & MsgFlags.kHasTimeSent) != 0);
                if (hasTime && value.AtEpoch)
                    fFlags &= ~MsgFlags.kHasTimeSent;
                else if (!value.AtEpoch)
                    fFlags |= MsgFlags.kHasTimeSent;
            }
        }

        public uint TransID {
            get { return fTransID; }
            set {
                fTransID = value;
                if ((fFlags & MsgFlags.kHasTransactionID) == 0)
                    fFlags |= MsgFlags.kHasTransactionID;
            }
        }

        public bool WantVersion {
            get { return ((fFlags & MsgFlags.kHasVersion) != 0); }
            set {
                if (WantVersion && !value)
                    fFlags &= ~MsgFlags.kHasVersion;
                else if (!WantVersion && value)
                    fFlags |= MsgFlags.kHasVersion;
           }
        }
        #endregion

        public override void Read(UruStream s) {
            s.ReadUShort(); //Type (throw away)
            fFlags = (MsgFlags)s.ReadUInt();

            if ((fFlags & MsgFlags.kHasVersion) != 0) {
                if (s.ReadByte() != 12) return; //Major Version MUST be 12
                if (s.ReadByte() != 6) return;  //Minor Version MUST be  6
            }

            if ((fFlags & MsgFlags.kHasTimeSent) != 0)
                fTimeSent.Read(s);
            if ((fFlags & MsgFlags.kHasContext) != 0)
                fContext = s.ReadUInt();
            if ((fFlags & MsgFlags.kHasTransactionID) != 0)
                fTransID = s.ReadUInt();
            if ((fFlags & MsgFlags.kHasPlayerID) != 0)
                fPlayerID = s.ReadUInt();
            if ((fFlags & MsgFlags.kHasAcctUUID) != 0)
                fAcctUuid = new Guid(s.ReadBytes(16));
        }

        public override void Write(UruStream s) {
            s.WriteUShort((ushort)ClassIndex);
            s.WriteUInt((uint)fFlags);

            if ((fFlags & MsgFlags.kHasVersion) != 0) {
                s.WriteByte(12);
                s.WriteByte(6);
            }

            if ((fFlags & MsgFlags.kHasTimeSent) != 0)
                fTimeSent.Write(s);
            if ((fFlags & MsgFlags.kHasContext) != 0)
                s.WriteUInt(fContext);
            if ((fFlags & MsgFlags.kHasTransactionID) != 0)
                s.WriteUInt(fTransID);
            if ((fFlags & MsgFlags.kHasPlayerID) != 0)
                s.WriteUInt(fPlayerID);
            if ((fFlags & MsgFlags.kHasAcctUUID) != 0)
                s.WriteBytes(fAcctUuid.ToByteArray());
        }

        public byte[] ToArray() {
            MemoryStream ms = new MemoryStream();
            UruStream s = new UruStream(ms);

            Write(s);
            byte[] buf = ms.ToArray();

            s.Close();
            ms.Close();

            return buf;
        }
    }

    public sealed class NetMsgMembersListReq : NetMessage { }
}
