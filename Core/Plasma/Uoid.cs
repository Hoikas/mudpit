using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MUd {
    public struct Location {

        [Flags]
        public enum LocFlags {
            kBuiltIn = 8,
            kItinerant = 0x10,
            kLocalOnly = 1,
            kReserved = 4,
            kVolatile = 2
        }

        public int fSeqPrefix;
        public int fPageID;
        public LocFlags fFlags;

        public Location(UruStream s) {
            this.fSeqPrefix = -1;
            this.fPageID = -1;
            this.fFlags = (LocFlags)0;
            this.Read(s);
        }

        public Location(uint seq) {
            this.fSeqPrefix = -1;
            this.fPageID = -1;
            this.fFlags = (LocFlags)0;
            this.IParseSeq(seq);
        }

        public Location(int prefix, int suffix) {
            this.fSeqPrefix = prefix;
            this.fPageID = suffix;
            this.fFlags = (LocFlags)0;
        }

        public Location(int prefix, int suffix, LocFlags flags) {
            this.fSeqPrefix = prefix;
            this.fPageID = suffix;
            this.fFlags = flags;
        }

        public bool Equals(Location obj) {
            return ((this.fSeqPrefix == obj.fSeqPrefix) && (this.fPageID == obj.fPageID));
        }

        public void Read(UruStream s) {
            this.IParseSeq(s.ReadUInt());
            this.fFlags = (LocFlags)s.ReadUShort();
        }

        public void Write(UruStream s) {
            s.WriteUInt(this.IUnParseSeq());
            s.WriteShort((short)this.fFlags);
        }

        private uint IUnParseSeq() {
            if ((this.fPageID == -1) && (this.fSeqPrefix == -1)) {
                return uint.MaxValue;
            }

            uint fSeqPrefix = (uint)this.fSeqPrefix;
            if (fSeqPrefix < 0) {
                fSeqPrefix = Convert.ToUInt32((uint)((fSeqPrefix & 0xffffff00) | (0x100 - fSeqPrefix)));
            }

            if (this.fPageID < 0) {
                fSeqPrefix++;
            }

            return (Convert.ToUInt32((uint)(fSeqPrefix << 0x10) + Convert.ToUInt32((int)(this.fPageID + ((this.fSeqPrefix < 0) ? 1 : 0x21)))));
        }

        private void IParseSeq(uint id) {
            if (id == uint.MaxValue) {
                this.fPageID = -1;
                this.fSeqPrefix = -1;
            } else {
                this.fPageID = Convert.ToInt32((long)(id & 0xffff));
                if ((id & 0x80000000) != 0) {
                    id--;
                    this.fPageID--;
                } else {
                    id -= 0x21;
                    this.fPageID -= 0x21;
                }

                this.fSeqPrefix = Convert.ToInt32((uint)(id >> 0x10));
                if ((id & 0x80000000) != 0) {
                    this.fSeqPrefix = Convert.ToInt32((long)((this.fSeqPrefix & ((long)0xffffff00L)) | (0x100 - this.fSeqPrefix)));
                }
            }
        }

        public static explicit operator uint(Location pid) {
            return pid.IUnParseSeq();
        }

        public static explicit operator Location(uint pid) {
            return new Location(pid);
        }

        public string ToString(string format) {
            return this.IUnParseSeq().ToString(format);
        }
    }


    public class Uoid {
        enum ContentsFlags {
            kHasCloneIDs = 0x1,
            kHasLoadMask = 0x2
        }

        public Location fLocation;
        public byte fLoadMask;
        public CreatableID fClassType;
        public string fObjectName;
        public uint fObjectID;
        public uint fClonePlayerID;
        public uint fCloneID;

        public Uoid() { }
        public Uoid(UruStream bs) { Read(bs); }

        public static Uoid ReadKey(UruStream bs) {
            if (bs.ReadBool()) return new Uoid(bs);
            else return null;
        }

        public static void WriteKey(UruStream bs, Uoid key) {
            if (key == null) {
                bs.WriteBool(false);
            } else {
                bs.WriteBool(true);
                key.Write(bs);
            }
        }

        public void Read(UruStream bs) {
            ContentsFlags cf = (ContentsFlags)bs.ReadByte();
            fLocation = new Location(bs);
            if ((cf & ContentsFlags.kHasLoadMask) != 0)
                fLoadMask = bs.ReadByte();
            fClassType = (CreatableID)bs.ReadUShort();
            fObjectID = bs.ReadUInt();
            fObjectName = bs.ReadSafeString();
            if ((cf & ContentsFlags.kHasCloneIDs) != 0) {
                fCloneID = bs.ReadUInt();
                fClonePlayerID = bs.ReadUInt();
            }
        }

        public void Write(UruStream bs) {
            ContentsFlags cf = (ContentsFlags)0;
            if (fLoadMask != 0xFF) cf |= ContentsFlags.kHasLoadMask;
            if (fCloneID != 0 || fClonePlayerID != 0) cf |= ContentsFlags.kHasCloneIDs;
            bs.WriteByte((byte)cf);
            fLocation.Write(bs);
            if (fLoadMask != 0xFF) bs.WriteByte(fLoadMask);
            bs.WriteUShort((ushort)fClassType);
            bs.WriteUInt(fObjectID);
            bs.WriteSafeString(fObjectName);
            if ((cf & ContentsFlags.kHasCloneIDs) != 0) {
                bs.WriteUInt(fCloneID);
                bs.WriteUInt(fClonePlayerID);
            }
        }

        public static bool operator ==(Uoid u1, Uoid u2) {
            try {
                if (u1.fClassType == u2.fClassType)
                    if (u1.fCloneID == u2.fCloneID)
                        if (u1.fClonePlayerID == u2.fClonePlayerID)
                            if (u1.fLocation.Equals(u2.fLocation))
                                if (u1.fObjectName == u2.fObjectName)
                                    return true;
            } catch (NullReferenceException) {
                //Yay, test for null
                bool u1_null = false;
                try {
                    if (u1.fClassType == CreatableID.NULL) { }
                } catch (NullReferenceException) {
                    u1_null = true;
                }

                bool u2_null = false;
                try {
                    if (u2.fClassType == CreatableID.NULL) { }
                } catch (NullReferenceException) {
                    u2_null = true;
                }

                return (u1_null == u2_null);
            }

            return false;
        }

        public static bool operator !=(Uoid u1, Uoid u2) {
            return !(u1 == u2); //Talk about milking it
        }
    }
}
