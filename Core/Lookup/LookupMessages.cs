using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace MUd {
    public enum LookupCli2Srv {
        AgeDestroyed,
        ClientCount,
        DeclareHost,
        FindAgeRequest,
        PingRequest,
    }

    public enum LookupSrv2Cli {
        FindAgeReply,
        PingReply,
    }

    public struct Lookup_AgeDestroyed {
        public string fAgeFilename;
        public Guid fAgeInstanceUuid;
        public uint fAgeVaultID;

        public void Read(UruStream s) {
            fAgeFilename = s.ReadUnicodeStringV16(40);
            fAgeInstanceUuid = new Guid(s.ReadBytes(16));
            fAgeVaultID = s.ReadUInt();
        }

        public void Write(UruStream s) {
            s.WriteUnicodeStringV16(fAgeFilename, 40);
            s.WriteBytes(fAgeInstanceUuid.ToByteArray());
            s.WriteUInt(fAgeVaultID);
        }
    }

    public struct Lookup_AgeReply {
        public uint fTransID;
        public ENetError fResult;
        public Guid fAgeInstanceUuid;
        public uint fAgeVaultID;
        public IPAddress fGameServerIP;

        public void Read(UruStream s) {
            fTransID = s.ReadUInt();
            fResult = (ENetError)s.ReadInt();
            fAgeInstanceUuid = new Guid(s.ReadBytes(16));
            fAgeVaultID = s.ReadUInt();

            byte[] game = s.ReadBytes(4);
            Array.Reverse(game);
            fGameServerIP = new IPAddress(game);
        }

        public void Write(UruStream s) {
            s.WriteUInt(fTransID);
            s.WriteInt((int)fResult);
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

    public struct Lookup_AgeRequest {
        public uint fTransID;
        public string fAgeFilename;
        public Guid fAgeInstanceUuid;
        public uint fAgeVaultID;

        public void Read(UruStream s) {
            fTransID = s.ReadUInt();
            fAgeFilename = s.ReadUnicodeStringV16(40);
            fAgeInstanceUuid = new Guid(s.ReadBytes(16));
            fAgeVaultID = s.ReadUInt();
        }

        public void Write(UruStream s) {
            s.WriteUInt(fTransID);
            s.WriteUnicodeStringV16(fAgeFilename, 40);
            s.WriteBytes(fAgeInstanceUuid.ToByteArray());
            s.WriteUInt(fAgeVaultID);
        }
    }

    public struct Lookup_ClientCount {
        public uint fNumClients;

        public void Read(UruStream s) {
            fNumClients = s.ReadUInt();
        }

        public void Write(UruStream s) {
            s.WriteUInt(fNumClients);
        }
    }

    public struct Lookup_DeclareHost {
        public string fHost;      //Len 1024

        public void Read(UruStream s) {
            fHost = s.ReadUnicodeStringV16(1024);
        }

        public void Write(UruStream s) {
            s.WriteUnicodeStringV16(fHost, 1024);
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
