using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MUd {
    public delegate void GateIP(uint transID, string ip);
    public delegate void GatePong(uint transID, uint pingTime, byte[] payload);

    public class GateClient : Cli2SrvBase {

        public event GateIP FileSrvIP;
        public event GatePong Pong;

        public bool Connect(Guid productUUID) {
            base.Connect(0, 0, productUUID, EConnType.kConnTypeCliToGate);

            //Send the GateConnectHeader
            UruStream s = new UruStream(new NetworkStream(fSocket, false));
            s.BufferWriter();
            fHeader.Write(s);
            s.WriteInt(20);
            s.WriteBytes(Guid.Empty.ToByteArray());
            s.FlushWriter();
            s.Close();

            //Init encryption
            if (!base.NetCliConnect(4))
                return false;

            Ping((uint)DateTime.UtcNow.Ticks, Encoding.UTF8.GetBytes("Hello, Mr. GateKeeper!"));

            fSocket.BeginReceive(new byte[2], 0, 2, SocketFlags.Peek, new AsyncCallback(IReceive), null);
            return true;
        }

        public uint GetFileHost(bool usePool) {
            Gate_FileSrvRequest req = new Gate_FileSrvRequest();
            req.fTransID = IGetTransID();
            req.fUsePool = usePool;

            lock (fStream) {
                fStream.BufferWriter();
                fStream.WriteUShort((ushort)GateCli2Srv.FileSrvIpAddressRequest);
                req.Write(fStream);
                fStream.FlushWriter();
            }

            return req.fTransID;
        }

        public uint Ping(uint pingTime, byte[] payload) {
            Gate_PingPong ping = new Gate_PingPong();
            ping.fPayload = payload;
            ping.fPingTime = pingTime;
            ping.fTransID = IGetTransID();

            lock (fStream) {
                fStream.BufferWriter();
                fStream.WriteUShort((ushort)GateCli2Srv.PingRequest);
                ping.Write(fStream);
                fStream.FlushWriter();
            }

            return ping.fTransID;
        }

        private void IReceive(IAsyncResult ar) {
            try {
                lock (fStream) {
                    fSocket.EndReceive(ar);
                    GateSrv2Cli msg = (GateSrv2Cli)fStream.ReadUShort();
                    switch (msg) {
                        case GateSrv2Cli.FileSrvIpAddressReply:
                            IGotFileIP();
                            break;
                        case GateSrv2Cli.PingReply:
                            IPong();
                            break;
                        default:
                            string test = Enum.GetName(typeof(GateSrv2Cli), msg);
                            throw new NotSupportedException(msg.ToString("X") + " - " + test);
                    }
                }

                fSocket.BeginReceive(new byte[2], 0, 2, SocketFlags.Peek, new AsyncCallback(IReceive), null);
            } catch (ObjectDisposedException) { } catch (SocketException) { fSocket.Close(); } catch (Exception e) {
                FireException(e);
            }
        }

        private void IGotFileIP() {
            Gate_FileSrvReply reply = new Gate_FileSrvReply();
            reply.Read(fStream);
            if (FileSrvIP != null)
                FileSrvIP(reply.fTransID, reply.fHost);
        }

        private void IPong() {
            Gate_PingPong pong = new Gate_PingPong();
            pong.Read(fStream);
            if (Pong != null)
                Pong(pong.fTransID, pong.fPingTime, pong.fPayload);
        }
    }
}
