using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MUd {
    public class GameThread : Srv2CliBase {

        private GameAgent fGrandParent;
        private GameServer fParent;
        private Guid fAcctUuid;
        private uint fPlayerID;
        private bool fJoinedAge = false;

        public GameServer Parent {
            get { return fParent; }
        }

        public GameThread(GameAgent grandma, Socket c, ConnectHeader hdr) 
            : base(c, hdr, null) {
            fGrandParent = grandma;
        }

        public override void Start() {
            UruStream s = new UruStream(new NetworkStream(fSocket, false));

            //NetCliConnect
            byte[] y_data = null;
            if (s.ReadByte() != (byte)NetCliConnectMsg.kNetCliConnect) {
                Error("FATAL: Invalid NetCliConnect");
                Stop();
            } else {
                int size = (int)s.ReadByte();
                y_data = s.ReadBytes(size - 2);

                if (y_data.Length > 64) {
                    Warn("YData too big. Truncating.");
                    byte[] old = y_data;
                    y_data = new byte[64];
                    Buffer.BlockCopy(old, 0, y_data, 0, 64);
                }
            }

            //Handoff
            if (!ISetupEncryption("Game", y_data)) {
                Error("Cannot setup encryption keys!");
                Stop();
                return;
            }

            //Send the NetCliEncrypt response
            s.BufferWriter();
            s.WriteByte((byte)NetCliConnectMsg.kNetCliEncrypt);
            s.WriteByte(9);
            s.WriteBytes(fServerSeed);
            s.FlushWriter();

            //Begin receiving data
            fSocket.BeginReceive(new byte[2], 0, 2, SocketFlags.Peek, new AsyncCallback(IReceive), null);
            s.Close();
        }

        public override void Stop() {
            if (fStream != null) fStream.Close();
            try { fSocket.Close(); } catch (ObjectDisposedException) { }
            if (fParent != null) fParent.Remove(this);
            fGrandParent.Remove(this);
        }

        #region Message Handlers
        private void IReceive(IAsyncResult ar) {
            try {
                fSocket.EndReceive(ar);

                lock (fStream) {
                    GameCli2Srv msg = GameCli2Srv.PingRequest;
                    try {
                        msg = (GameCli2Srv)fStream.ReadUShort();
                    } catch (IOException) { }

                    switch (msg) {
                        case GameCli2Srv.JoinAgeRequest:
                            IJoinAge();
                            break;
                        default:
                            string test = Enum.GetName(typeof(GameCli2Srv), msg);
                            if (test != null && test != String.Empty) {
                                string type = "Cli2Game_" + test;
                                if (type == null) type = "0x" + msg.ToString("X");
                                Error("Unimplemented message type " + type);
                                Stop();
                            } else {
                                Warn(String.Format("Received garbage... [0x{0}] Expected: MSG_TYPE", msg.ToString("X")));
                                Stop();
                            }

                            break;
                    }
                }

                fSocket.BeginReceive(new byte[2], 0, 2, SocketFlags.Peek, new AsyncCallback(IReceive), null);
            } catch (SocketException e) {
                IHandleSocketException(e);
            } catch (IOException) {
                Stop();
            } catch (ObjectDisposedException) {
                Stop();
            }
        }

        private void IJoinAge() {
            Game_JoinAgeRequest req = new Game_JoinAgeRequest();
            req.Read(fStream);

            fAcctUuid = req.fAcctUuid;
            fPlayerID = req.fPlayerID;

            if (fJoinedAge) {
                fLog.Error("Attempted to join an age, but we are already joined!");
                Stop();
            } else {
                Game_JoinAgeReply reply = new Game_JoinAgeReply();
                reply.fTransID = req.fTransID;

                fParent = fGrandParent.JoinAge(req.fAgeMcpID, this);
                if (fParent == null) {
                    reply.fResult = ENetError.kNetErrAgeNotFound;
                } else {
                    reply.fResult = ENetError.kNetSuccess;
                    fLog = fParent.Log;
                }
            }
        }

        #endregion
    }
}
