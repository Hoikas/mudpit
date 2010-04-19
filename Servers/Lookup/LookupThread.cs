﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MUd {
    public class LookupThread : MasterBase {

        private LookupConnType fConnType;
        public LookupConnType ConnType {
            get { return fConnType; }
        }

        public LookupThread(LookupServer parent, LookupConnType type, Socket c, ConnectHeader hdr, LogProcessor log) 
            : base(parent, c, hdr, log) {
            fConnType = type;
        }

        protected override void Receive(IAsyncResult ar) {
            try {
                fSocket.EndReceive(ar);

                LookupCli2Srv msg = LookupCli2Srv.PingRequest;
                try {
                    msg = (LookupCli2Srv)fStream.ReadUShort();
                } catch (IOException) {
                    Verbose("Disconnected");
                }

                switch (msg) {
                    case LookupCli2Srv.ClientCount:
                        IUpdateCliCount();
                        break;
                    case LookupCli2Srv.PingRequest:
                        IPingPong();
                        break;
                    default:
                        string test = Enum.GetName(typeof(LookupCli2Srv), msg);
                        if (test != null && test != String.Empty) {
                            string type = "Cli2Lookup_" + test;
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

        private void IPingPong() {
            Lookup_PingPong ping = new Lookup_PingPong();
            ping.Read(fStream);

            fStream.BufferWriter();
            fStream.WriteUShort((ushort)LookupSrv2Cli.PingReply);
            ping.Write(fStream);
            fStream.FlushWriter();

            Verbose("Ping? PONG!");
        }

        private void IUpdateCliCount() {
            Lookup_ClientCount cc = new Lookup_ClientCount();
            cc.Read(fStream);

            fParent.UpdateCliCount(cc.fHost, fConnType, cc.fNumClients);
        }
    }
}