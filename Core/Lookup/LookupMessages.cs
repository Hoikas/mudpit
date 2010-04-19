using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MUd {
    public enum LookupCli2Srv {
        ClientCount,
        FindAgeRequest,
        PingRequest,
        StartAgeReply,
    }

    public enum LookupSrv2Cli {
        FindAgeReply,
        PingReply,
        StartAgeRequest,
    }

    public struct Lookup_ClientCount {
        public string fHost;      //Len 24
        public uint fNumClients;

        public void Read(UruStream s) {
            fHost = s.ReadUnicodeStringV16(24);
            fNumClients = s.ReadUInt();
        }

        public void Write(UruStream s) {
            s.WriteUnicodeStringV16(fHost, 24);
            s.WriteUInt(fNumClients);
        }
    }

    public struct Lookup_PingPong {
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
}
