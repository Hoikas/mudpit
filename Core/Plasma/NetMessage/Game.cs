using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MUd {
    public class NetMsgLoadClone : NetMsgGameMessage {

        public Uoid fPlayerKey;
        public bool fIsPlayer = true;
        public bool fIsLoading = true;
        public bool fIsInitialState = false;

        public NetMsgLoadClone() : base() { }

        public override void Read(UruStream s) {
            base.Read(s);

            fPlayerKey = new Uoid(s);
            fIsPlayer = s.ReadBool();
            fIsLoading = s.ReadBool();
            fIsInitialState = s.ReadBool();
        }

        public override void Write(UruStream s) {
            base.Write(s);

            fPlayerKey.Write(s);
            s.WriteBool(fIsPlayer);
            s.WriteBool(fIsLoading);
            s.WriteBool(fIsInitialState);
        }
    }

    public class NetMsgGameMessage : NetMsgStream {

        public UnifiedTime fDeliveryTime;

        public Message GameMsg {
            get {
                if (fCompressed) Decompress();
                MemoryStream ms = new MemoryStream(fBuffer);
                UruStream s = new UruStream(ms);

                Message msg = Factory.ReadCreatable(s) as Message;
                s.Close();
                ms.Close();

                return msg;
            }

            set {
                MemoryStream ms = new MemoryStream();
                UruStream s = new UruStream(ms);

                Factory.WriteCreatable(s, value);
                fBuffer = ms.ToArray();
                fCompressed = false;
                fUncompressedSize = (int)ms.Length;

                s.Close();
                ms.Close();
            }
        }

        public NetMsgGameMessage() {
            fFlags |= MsgFlags.kNeedsReliableSend;
        }

        public override void Read(UruStream s) {
            base.Read(s);

            if (s.ReadBool())
                fDeliveryTime = new UnifiedTime(s);
        }

        public override void Write(UruStream s) {
            base.Write(s);

            if (fDeliveryTime != null) {
                s.WriteBool(true);
                fDeliveryTime.Write(s);
            } else s.WriteBool(false);
        }
    }

    public abstract class NetMsgStream : NetMessage {

        protected int fUncompressedSize;
        public bool fCompressed;
        public byte[] fBuffer;

        public int Length {
            get {
                if (fCompressed) return fUncompressedSize;
                else return fBuffer.Length;
            }

            set {
                if (fCompressed) fUncompressedSize = value;
                else throw new NotSupportedException();
            }
        }

        public void Compress() {
            throw new NotImplementedException();
        }

        public void Decompress() {
            throw new NotImplementedException();
        }

        public override void Read(UruStream s) {
            base.Read(s);

            fUncompressedSize = s.ReadInt();
            fCompressed = (s.ReadByte() == 2); //2 = Zlib Compressed; 0 = No Compression
            fBuffer = s.ReadBytes(s.ReadInt());
        }

        public override void Write(UruStream s) {
            base.Write(s);

            s.WriteInt(fUncompressedSize);
            s.WriteByte((fCompressed ? (byte)2 : (byte)0));
            s.WriteInt(fBuffer.Length);
            s.WriteBytes(fBuffer);
        }
    }
}
