using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MUd {
    public enum GameCli2Srv {
        PingRequest = 0,
        JoinAgeRequest,
        PropagateBuffer,
        GameMgrMsg,
    }

    public enum GameSrv2Cli {
        PingReply = 0,
        JoinAgeReply,
        PropagateBuffer,
        GameMgrMsg,
    }

    public struct Game_JoinAgeReply {
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

    public struct Game_JoinAgeRequest {
        public uint fTransID;
        public uint fAgeMcpID;
        public Guid fAcctUuid;
        public uint fPlayerID;

        public void Read(UruStream s) {
            fTransID = s.ReadUInt();
            fAgeMcpID = s.ReadUInt();
            fAcctUuid = new Guid(s.ReadBytes(16));
            fPlayerID = s.ReadUInt();
        }

        public void Write(UruStream s) {
            s.WriteUInt(fTransID);
            s.WriteUInt(fAgeMcpID);
            s.WriteBytes(fAcctUuid.ToByteArray());
            s.WriteUInt(fPlayerID);
        }
    }

    public struct Game_PingPong {
        public int fPingTime;

        public void Read(UruStream s) {
            fPingTime = s.ReadInt();
        }

        public void Write(UruStream s) {
            s.WriteInt(fPingTime);
        }
    }

    public struct Game_PropagateBuffer {
        public uint fMsgType;
        public byte[] fBuffer;

        public void Read(UruStream s) {
            fMsgType = s.ReadUInt();
            fBuffer = s.ReadBytes(s.ReadInt());
        }

        public void Write(UruStream s) {
            s.WriteUInt(fMsgType);

            if (fBuffer == null) fBuffer = new byte[0];
            s.WriteInt(fBuffer.Length);
            s.WriteBytes(fBuffer);
        }
    }
}
