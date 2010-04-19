using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MUd {
    public class GateThread : MasterBase {
        public GateThread(LookupServer parent, Socket c, ConnectHeader hdr, LogProcessor log) : base(parent, c, hdr, log) { }

        protected override void Receive(IAsyncResult ar) {
            try {
                fSocket.EndReceive(ar);

                GateCli2Srv msg = GateCli2Srv.PingRequest;
                try {
                    msg = (GateCli2Srv)fStream.ReadUShort();
                } catch (IOException) {
                    Verbose("Disconnected");
                }

                switch (msg) {
                    case GateCli2Srv.FileSrvIpAddressRequest:
                        IGetFileSrv();
                        break;
                    case GateCli2Srv.PingRequest:
                        IPingPong();
                        break;
                    default:
                        string test = Enum.GetName(typeof(GateCli2Srv), msg);
                        if (test != null && test != String.Empty) {
                            string type = "Cli2Gate_" + test;
                            if (type == null) type = "0x" + msg.ToString("X");
                            Error("Unimplemented message type " + type);
                            Stop();
                        } else {
                            Warn(String.Format("Received garbage... [0x{0}] Expected: MSG_TYPE", msg.ToString("X")));
                            Stop();
                        }

                        break;
                }

                fSocket.BeginReceive(new byte[2], 0, 2, SocketFlags.Peek, new AsyncCallback(Receive), null);
            } catch (SocketException e) {
                IHandleSocketException(e);
            } catch (IOException) {
                Debug("Disconnected");
                Stop();
            } catch (ObjectDisposedException) { }
        }

        private void IGetFileSrv() {
            Gate_FileSrvRequest req = new Gate_FileSrvRequest();
            req.Read(fStream);

            Gate_FileSrvReply reply = new Gate_FileSrvReply();
            reply.fHost = fParent.GetBestServer(LookupConnType.kFileSrv);
            reply.fTransID = req.fTransID;

            //Kill everything if there are no file servers around...
            if (reply.fHost == null) {
                Error("Requested a FileSrv, but no FileSrvs are connected!");
                Stop();
                return;
            }

            fStream.BufferWriter();
            fStream.WriteUShort((ushort)GateSrv2Cli.FileSrvIpAddressReply);
            reply.Write(fStream);
            fStream.FlushWriter();
        }

        private void IPingPong() {
            Gate_PingPong ping = new Gate_PingPong();
            ping.Read(fStream);

            fStream.BufferWriter();
            fStream.WriteUShort((ushort)GateSrv2Cli.PingReply);
            ping.Write(fStream);
            fStream.FlushWriter();

            Verbose("Ping? PONG!");
        }
    }
}
