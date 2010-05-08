using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MUd {
    public delegate void FileBuildIdReply(uint transID, ENetError result, uint buildID);

    public class FileClient : Cli2SrvBase {

        public event FileBuildIdReply GotBuildID;

        public FileClient() : base() {
            fHeader.fType = EConnType.kConnTypeCliToFile;
        }

        public override bool Connect() {
            if (!base.Connect()) return false;

            //Send the FileConnectHeader
            UruStream s = new UruStream(new NetworkStream(fSocket, false));
            s.BufferWriter();
            fHeader.Write(s);
            s.WriteUInt(12); //Size
            s.WriteUInt(0);
            s.WriteUInt(0);
            s.FlushWriter();
            s.Close();

            fSocket.BeginReceive(new byte[4], 0, 4, SocketFlags.Peek, new AsyncCallback(IReceive), null);
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

        public void Ping(int time) {
            File_PingPong ping = new File_PingPong();
            ping.fPingTime = time;

            ResetIdleTimer();
            lock (fSocket) {
                MemoryStream ms = new MemoryStream();
                UruStream s = new UruStream(ms);

                s.WriteUInt(12);
                s.WriteInt((int)FileCli2Srv.PingRequest);
                ping.Write(s);

                fSocket.Send(ms.ToArray());

                s.Close();
                ms.Close();
            }
        }

        public uint RequestBuildID() {
            File_BuildIdRequest req = new File_BuildIdRequest();
            req.fTransID = IGetTransID();

            ResetIdleTimer();
            lock (fSocket) {
                MemoryStream ms = new MemoryStream();
                UruStream s = new UruStream(ms);

                s.WriteUInt(12);
                s.WriteInt((int)FileCli2Srv.BuildIdRequest);
                req.Write(s);

                fSocket.Send(ms.ToArray());

                s.Close();
                ms.Close();
            }

            return req.fTransID;
        }

        private void IReceive(IAsyncResult ar) {
            try {
                lock (fSocket) {
                    fSocket.EndReceive(ar);
                    
                    ResetIdleTimer();

                    //Size
                    byte[] buf = new byte[4];
                    fSocket.Receive(buf);

                    //Message
                    buf = new byte[BitConverter.ToInt32(buf, 0) - 4];
                    fSocket.Receive(buf);
                    fStream = new UruStream(new MemoryStream(buf));

                    FileSrv2Cli msg = (FileSrv2Cli)fStream.ReadInt();
                    switch (msg) {
                        case FileSrv2Cli.BuildIdReply:
                            IGotBuildID();
                            break;
                        default:
                            string test = Enum.GetName(typeof(FileSrv2Cli), msg);
                            throw new NotSupportedException(msg.ToString("X") + " - " + test);
                    }

                    fStream.Close();
                }

                fSocket.BeginReceive(new byte[2], 0, 2, SocketFlags.Peek, new AsyncCallback(IReceive), null);
            } catch (ObjectDisposedException) { } catch (SocketException) { fSocket.Close(); } catch (Exception e) {
                FireException(e);
            }
        }

        private void IGotBuildID() {
            File_BuildIdReply reply = new File_BuildIdReply();
            reply.Read(fStream);
            if (BuildID != null)
                GotBuildID(reply.fTransID, reply.fResult, reply.fBuildID);
        }
    }
}
