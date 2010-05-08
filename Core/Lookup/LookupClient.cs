using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MUd {
    public delegate void LookupAgeFound(uint transID, ENetError result, Guid uuid, uint ageVault, IPAddress gameIP);
    public delegate void LookupPong(uint transID, uint pingTime, byte[] payload);

    public class LookupClient : Srv2SrvBase {

        public event LookupAgeFound AgeFound;
        public event LookupPong Pong;

        public LookupClient() : base("Master") {
            fHeader.fType = EConnType.kConnTypeSrvToLookup;
        }

        public override bool Connect() {
            if (!base.Connect()) return false;

            //Send the LookupConnectHeader
            UruStream s = new UruStream(new NetworkStream(fSocket, false));
            s.BufferWriter();
            fHeader.Write(s);
            s.WriteInt(20);
            s.WriteBytes(fToken.ToByteArray());
            s.FlushWriter();
            s.Close();

            //Init encryption
            if (!base.NetCliConnect(4))
                return false;
            
            //Begin receiving from the server
            fSocket.BeginReceive(new byte[2], 0, 2, SocketFlags.Peek, new AsyncCallback(IReceive), null);
            return true;
        }

        protected override void RunIdleBehavior() {
            switch (fIdleBeh) {
                case IdleBehavior.Disconnect:
                    Disconnect();
                    break;
                case IdleBehavior.Ping:
                    Ping((uint)DateTime.Now.Ticks, Encoding.UTF8.GetBytes("IDLE"));
                    break;
            }
        }

        public void DeclareHost(string host) {
            Lookup_DeclareHost cc = new Lookup_DeclareHost();
            cc.fHost = host;

            ResetIdleTimer();
            lock (fStream) {
                fStream.BufferWriter();
                fStream.WriteUShort((ushort)LookupCli2Srv.DeclareHost);
                cc.Write(fStream);
                fStream.FlushWriter();
            }
        }

        public uint FindAge(string age, Guid uuid, uint ageVault) {
            Lookup_AgeRequest req = new Lookup_AgeRequest();
            req.fAgeFilename = age;
            req.fAgeInstanceUuid = uuid;
            req.fAgeVaultID = ageVault;
            req.fTransID = IGetTransID();

            ResetIdleTimer();
            lock (fStream) {
                fStream.BufferWriter();
                fStream.WriteUShort((ushort)LookupCli2Srv.FindAgeRequest);
                req.Write(fStream);
                fStream.FlushWriter();
            }

            return req.fTransID;
        }

        public void NotifyAgeDestroyed(string age, Guid uuid, uint ageVault) {
            Lookup_AgeDestroyed notify = new Lookup_AgeDestroyed();
            notify.fAgeFilename = age;
            notify.fAgeInstanceUuid = uuid;
            notify.fAgeVaultID = ageVault;

            ResetIdleTimer();
            lock (fStream) {
                fStream.BufferWriter();
                fStream.WriteUShort((ushort)LookupCli2Srv.AgeDestroyed);
                notify.Write(fStream);
                fStream.FlushWriter();
            }
        }

        public uint Ping(uint pingTime, byte[] payload) {
            Lookup_PingPong ping = new Lookup_PingPong();
            ping.fPayload = payload;
            ping.fPingTime = pingTime;
            ping.fTransID = IGetTransID();

            ResetIdleTimer();
            lock (fStream) {
                fStream.BufferWriter();
                fStream.WriteUShort((ushort)LookupCli2Srv.PingRequest);
                ping.Write(fStream);
                fStream.FlushWriter();
            }

            return ping.fTransID;
        }

        public void SetNumClients(uint num) {
            Lookup_ClientCount cc = new Lookup_ClientCount();
            cc.fNumClients = num;

            ResetIdleTimer();
            lock (fStream) {
                fStream.BufferWriter();
                fStream.WriteUShort((ushort)LookupCli2Srv.ClientCount);
                cc.Write(fStream);
                fStream.FlushWriter();
            }
        }

        private void IReceive(IAsyncResult ar) {
            try {
                lock (fStream) {
                    fSocket.EndReceive(ar);
                    LookupSrv2Cli msg = (LookupSrv2Cli)fStream.ReadUShort();

                    ResetIdleTimer();
                    switch (msg) {
                        case LookupSrv2Cli.FindAgeReply:
                            IFindAgeReply();
                            break;
                        case LookupSrv2Cli.PingReply:
                            IPong();
                            break;
                        default:
                            string test = Enum.GetName(typeof(LookupSrv2Cli), msg);
                            throw new NotSupportedException(msg.ToString("X") + " - " + test);
                    }
                }

                fSocket.BeginReceive(new byte[2], 0, 2, SocketFlags.Peek, new AsyncCallback(IReceive), null);
            } catch (ObjectDisposedException) { } catch (SocketException) { fSocket.Close(); } catch (Exception e) {
                FireException(e);
            }
        }

        private void IFindAgeReply() {
            Lookup_AgeReply reply = new Lookup_AgeReply();
            reply.Read(fStream);
            if (AgeFound != null)
                AgeFound(reply.fTransID, reply.fResult, reply.fAgeInstanceUuid, reply.fAgeVaultID, reply.fGameServerIP);
        }

        private void IPong() {
            Lookup_PingPong pong = new Lookup_PingPong();
            pong.Read(fStream);
            if (Pong != null)
                Pong(pong.fTransID, pong.fPingTime, pong.fPayload);
        }
    }
}
