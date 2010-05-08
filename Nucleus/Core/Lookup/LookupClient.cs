using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MUd {
    public delegate void LookupPong(uint transID, uint pingTime, byte[] payload);

    public class LookupClient : Cli2SrvBase {

        public event LookupPong Pong;

        public bool Connect(uint buildID, uint branchID, Guid productUUID, Guid token) {
            base.Connect(buildID, branchID, productUUID, EConnType.kConnTypeSrvToLookup);

            //Send the LookupConnectHeader
            UruStream s = new UruStream(new NetworkStream(fSocket, false));
            s.BufferWriter();
            fHeader.Write(s);
            s.WriteInt(20);
            s.WriteBytes(token.ToByteArray());
            s.FlushWriter();
            s.Close();

            //Init encryption
            if (!base.NetCliConnect(4))
                return false;

            Ping((uint)DateTime.UtcNow.Ticks, Encoding.UTF8.GetBytes("Hello, Mr. Lookup!"));
            
            //Begin receiving from the server
            fSocket.BeginReceive(new byte[2], 0, 2, SocketFlags.Peek, new AsyncCallback(IReceive), null);
            return true;
        }

        public uint Ping(uint pingTime, byte[] payload) {
            Lookup_PingPong ping = new Lookup_PingPong();
            ping.fPayload = payload;
            ping.fPingTime = pingTime;
            ping.fTransID = IGetTransID();

            lock (fStream) {
                fStream.BufferWriter();
                fStream.WriteUShort((ushort)LookupCli2Srv.PingRequest);
                ping.Write(fStream);
                fStream.FlushWriter();
            }

            return ping.fTransID;
        }

        public void SetNumClients(string host, uint num) {
            Lookup_ClientCount cc = new Lookup_ClientCount();
            cc.fHost = host;
            cc.fNumClients = num;

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
                    switch (msg) {
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

        private void IPong() {
            Lookup_PingPong pong = new Lookup_PingPong();
            pong.Read(fStream);
            if (Pong != null)
                Pong(pong.fTransID, pong.fPingTime, pong.fPayload);
        }
    }
}
