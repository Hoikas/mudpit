﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace MUd {
    public delegate void GameAgeJoined(ENetError result);

    public class GameClient : Cli2SrvBase {

        public event GameAgeJoined AgeJoined;

        private Guid fAcctUuid;
        public Guid AccountUUID {
            get { return fAcctUuid; }
            set { fAcctUuid = value; }
        }

        private Guid fAgeUuid;
        public Guid InstanceUUID {
            get { return fAgeUuid; }
            set { fAgeUuid = value; }
        }

        private uint fMcpID;
        public uint McpID {
            get { return fMcpID; }
            set { fMcpID = value; }
        }

        private uint fPlayerID;
        public uint PlayerID {
            get { return fPlayerID; }
            set { fPlayerID = value; }
        }

        public GameClient() : base() {
            fHeader.fType = EConnType.kConnTypeCliToGame;
        }

        public override bool Connect() {
            if (!base.Connect()) return false;

            //Send the GameConnectHeader
            UruStream s = new UruStream(new NetworkStream(fSocket, false));
            s.BufferWriter();
            fHeader.Write(s);
            s.WriteInt(36);
            s.WriteBytes(fAcctUuid.ToByteArray());
            s.WriteBytes(fAgeUuid.ToByteArray());
            s.FlushWriter();
            s.Close();

            //Init encryption
            if (!base.NetCliConnect())
                return false;

            //Only allow us to join one age per conneciton.
            Game_JoinAgeRequest req = new Game_JoinAgeRequest();
            req.fAcctUuid = fAcctUuid;
            req.fAgeMcpID = fMcpID;
            req.fPlayerID = fPlayerID;
            req.fTransID = IGetTransID();

            //Send the JAR (Note: this is not Java...)
            fStream.BufferWriter();
            fStream.WriteUShort((ushort)GameCli2Srv.JoinAgeRequest);
            req.Write(fStream);
            fStream.FlushWriter();

            fSocket.BeginReceive(new byte[2], 0, 2, SocketFlags.Peek, new AsyncCallback(IReceive), null);
            return true;
        }

        private void IReceive(IAsyncResult ar) {
            try {
                lock (fStream) {
                    fSocket.EndReceive(ar);
                    GameSrv2Cli msg = (GameSrv2Cli)fStream.ReadUShort();
                    switch (msg) {
                        case GameSrv2Cli.JoinAgeReply:
                            IJoinAgeReply();
                        default:
                            string test = Enum.GetName(typeof(GameSrv2Cli), msg);
                            throw new NotSupportedException(msg.ToString("X") + " - " + test);
                    }
                }

                fSocket.BeginReceive(new byte[2], 0, 2, SocketFlags.Peek, new AsyncCallback(IReceive), null);
            } catch (ObjectDisposedException) { } catch (SocketException) { fSocket.Close(); }
#if !DEBUG
            catch (Exception e) {
                if (Connected)
                    FireException(e);
            }
#endif
        }

        private void IJoinAgeReply() {
            Game_JoinAgeReply reply = new Game_JoinAgeReply();
            reply.Read(fStream);
            if (AgeJoined != null)
                AgeJoined(reply.fResult);
        }
    }
}
