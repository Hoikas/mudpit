using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace MUd {
    public delegate void GameAgeJoined(uint transID, ENetError result);
    public delegate void GamePong(int ms);
    public delegate void GameRawBuffer(NetMessage msg, bool handled);

    public class GameClient : Cli2SrvBase {

        public event GameAgeJoined AgeJoined;
        public event GameRawBuffer BufferPropagated;
        public event GamePong Pong;

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
            if (!base.NetCliConnect(73))
                return false;

            fSocket.BeginReceive(new byte[2], 0, 2, SocketFlags.Peek, new AsyncCallback(IReceive), null);
            return true;
        }

        protected override void RunIdleBehavior() {
            switch (fIdleBeh) {
                case IdleBehavior.Disconnect:
                    Disconnect();
                    break;
                case IdleBehavior.Ping:
                    Ping((int)DateTime.Now.Ticks);
                    break;
            }
        }

        public uint JoinAge() {
            Game_JoinAgeRequest req = new Game_JoinAgeRequest();
            req.fAcctUuid = fAcctUuid;
            req.fAgeMcpID = fMcpID;
            req.fPlayerID = fPlayerID;
            req.fTransID = IGetTransID();

            ResetIdleTimer();
            lock (fStream) {
                fStream.BufferWriter();
                fStream.WriteUShort((ushort)GameCli2Srv.JoinAgeRequest);
                req.Write(fStream);
                fStream.FlushWriter();
            }

            return req.fTransID;
        }

        public void LoadAvatar(Uoid player) {
            Uoid client_mgr = new Uoid();
            client_mgr.fClassType = CreatableID.NetClientMgr;
            client_mgr.fObjectName = "kNetClientMgr_KEY";

            Uoid av_mgr = new Uoid();
            av_mgr.fClassType = CreatableID.AvatarMgr;
            av_mgr.fObjectName = "kAvatarMgr_KEY";

            LoadAvatarMsg load_av = new LoadAvatarMsg();
            load_av.fBCastFlags |= Message.BCastFlags.kLocalPropagate | Message.BCastFlags.kNetPropagate;
            load_av.fCloneKey = player;
            load_av.fIsLoading = true;
            load_av.fIsPlayer = true;
            load_av.fOriginatingPlayerID = player.fClonePlayerID;
            load_av.fReceivers.Add(client_mgr);
            load_av.fRequestorKey = av_mgr;

            NetMsgLoadClone load_clone = new NetMsgLoadClone();
            load_clone.GameMsg = load_av;
            load_clone.PlayerID = player.fClonePlayerID;
            load_clone.fPlayerKey = player;
            load_clone.TimeSent = new UnifiedTime(DateTime.UtcNow);

            Game_PropagateBuffer buffer = new Game_PropagateBuffer();
            buffer.NetMsg = load_clone;

            ResetIdleTimer();
            lock (fStream) {
                fStream.BufferWriter();
                fStream.WriteUShort((ushort)GameCli2Srv.PropagateBuffer);
                buffer.Write(fStream);
                fStream.FlushWriter();
            }
        }

        public void PropagateBuffer(CreatableID pCre, byte[] buf) {
            Game_PropagateBuffer buffer = new Game_PropagateBuffer();
            buffer.fBuffer = buf;
            buffer.fMsgType = pCre;

            ResetIdleTimer();
            lock (fStream) {
                fStream.BufferWriter();
                fStream.WriteUShort((ushort)GameCli2Srv.PropagateBuffer);
                buffer.Write(fStream);
                fStream.FlushWriter();
            }
        }

        public void Ping(int ms) {
            Game_PingPong ping = new Game_PingPong();
            ping.fPingTime = ms;

            ResetIdleTimer();
            lock (fStream) {
                fStream.BufferWriter();
                fStream.WriteUShort((ushort)GameCli2Srv.PingRequest);
                ping.Write(fStream);
                fStream.FlushWriter();
            }
        }

        private void IReceive(IAsyncResult ar) {
            try {
                lock (fStream) {
                    fSocket.EndReceive(ar);
                    GameSrv2Cli msg = (GameSrv2Cli)fStream.ReadUShort();

                    ResetIdleTimer();
                    switch (msg) {
                        case GameSrv2Cli.JoinAgeReply:
                            IJoinAgeReply();
                            break;
                        case GameSrv2Cli.PingReply:
                            IPong();
                            break;
                        case GameSrv2Cli.PropagateBuffer:
                            IPropagateBuffer();
                            break;
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
                AgeJoined(reply.fTransID, reply.fResult);
        }

        private void IPong() {
            Game_PingPong pong = new Game_PingPong();
            pong.Read(fStream);
            if (Pong != null)
                Pong(pong.fPingTime);
        }

        private void IPropagateBuffer() {
            Game_PropagateBuffer buffer = new Game_PropagateBuffer();
            buffer.Read(fStream);

            bool handled = false;
            //TODO: Handle specific NetMessages
            //      Later....

            try {
                if (BufferPropagated != null)
                    BufferPropagated(buffer.NetMsg, handled);
            } catch (NotSupportedException) {
                //Unhandled buffer logic needed here...
            }
        }
    }
}
