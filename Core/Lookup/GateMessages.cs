using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MUd {
    public enum GateCli2Srv {
        PingRequest,
        FileSrvIpAddressRequest,
        AuthSrvIpAddressRequest
    }

    public enum GateSrv2Cli {
        PingReply,
        FileSrvIpAddressReply,
        AuthSrvIpAddressReply,
    }

    public struct Gate_PingPong {
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

    public struct Gate_FileSrvRequest {
        public uint fTransID; //???
        public bool fUsePool; //???

        public void Read(UruStream s) {
            fTransID = s.ReadUInt();
            fUsePool = s.ReadBool();
        }

        public void Write(UruStream s) {
            s.WriteUInt(fTransID);
            s.WriteBool(fUsePool);
        }
    }

    public struct Gate_FileSrvReply {
        public uint fTransID; //???
        public string fHost;  //Len 24

        public void Read(UruStream s) {
            fTransID = s.ReadUInt();
            fHost = s.ReadUnicodeStringV16(24);
        }

        public void Write(UruStream s) {
            s.WriteUInt(fTransID);
            s.WriteUnicodeStringV16(fHost, 24);
        }
    }
}
