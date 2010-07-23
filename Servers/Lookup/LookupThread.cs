using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace MUd {
    public class LookupThread : MasterBase {

        private LookupConnType fConnType;
        public LookupConnType ConnType {
            get { return fConnType; }
        }

        private string fSrvHost;
        public string SrvHost {
            get { return fSrvHost; }
        }

        private ManualResetEvent fAgeMut = new ManualResetEvent(false);

        public LookupThread(LookupServer parent, LookupConnType type, Socket c, ConnectHeader hdr, LogProcessor log) 
            : base(parent, c, hdr, log) {
            fConnType = type;
        }

        private Dictionary<uint, object> fTransTags = new Dictionary<uint, object>();

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
            if (!ISetupEncryption("Master", y_data)) {
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
            fSocket.BeginReceive(new byte[2], 0, 2, SocketFlags.Peek, new AsyncCallback(Receive), null);
            s.Close();
        }

        public void StartAge(string name, Guid uuid, uint vaultID) {
            if (fConnType != LookupConnType.kGameSrv)
                throw new NotSupportedException();

            Lookup_StartAgeCmd cmd = new Lookup_StartAgeCmd();
            cmd.fAgeFilename = name;
            cmd.fAgeInstanceUuid = uuid;
            cmd.fAgeVaultID = vaultID;

            lock (fStream) {
                fStream.BufferWriter();
                fStream.WriteUShort((ushort)LookupSrv2Cli.StartAgeCmd);
                cmd.Write(fStream);
                fStream.FlushWriter();
            }

            fAgeMut.Reset();
            fAgeMut.WaitOne();
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
                    case LookupCli2Srv.AgeDestroyed:
                        IAgeDestroyed();
                        break;
                    case LookupCli2Srv.ClientCount:
                        IUpdateCliCount();
                        break;
                    case LookupCli2Srv.DeclareHost:
                        IDeclareHost();
                        break;
                    case LookupCli2Srv.FindAgeRequest:
                        IFindAge();
                        break;
                    case LookupCli2Srv.PingRequest:
                        IPingPong();
                        break;
                    case LookupCli2Srv.AgeStarted:
                        IAgeStarted();
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

        private void IAgeDestroyed() {
            Lookup_AgeDestroyed destroyed = new Lookup_AgeDestroyed();
            destroyed.Read(fStream);

            if (fConnType == LookupConnType.kGameSrv)
                fParent.RemoveAge(destroyed.fAgeInstanceUuid);
        }

        private void IAgeStarted() {
            Lookup_AgeStarted started = new Lookup_AgeStarted();
            started.Read(fStream);

            if (fConnType == LookupConnType.kGameSrv)
                if (started.fResult == ENetError.kNetSuccess)
                    fParent.AddAge(started.fAgeFilename, started.fAgeInstanceUuid, started.fAgeVaultID, started.fAgeMcpID);
                else
                    Warn(String.Format("StartAgeCmd failed [AGE: {0}] [RESULT: {1}] [UUID: {2}]", started.fAgeFilename, started.fResult.ToString().Substring(4), started.fAgeInstanceUuid));

            //Release any waiting threads
            fAgeMut.Set();
        }

        private void IDeclareHost() {
            Lookup_DeclareHost req = new Lookup_DeclareHost();
            req.Read(fStream);

            if (fSrvHost == null) {
                fSrvHost = req.fHost;
                fParent.UpdateCliCount(req.fHost, fConnType, 0);
            }
        }

        private void IFindAge() {
            Lookup_AgeRequest req = new Lookup_AgeRequest();
            req.Read(fStream);

            Lookup_AgeReply reply = new Lookup_AgeReply();
            reply.fAgeInstanceUuid = req.fAgeInstanceUuid;
            reply.fAgeVaultID = req.fAgeVaultID;
            reply.fTransID = req.fTransID;

            //Actually start the GameSrv... If needed...
            if (!fParent.HasAge(req.fAgeFilename, req.fAgeInstanceUuid, req.fAgeVaultID))
                if (fParent.StartAge(req.fAgeFilename, req.fAgeInstanceUuid, req.fAgeVaultID))
                    reply.fResult = ENetError.kNetSuccess;
                else
                    reply.fResult = ENetError.kNetErrInternalError;
            else
                reply.fResult = ENetError.kNetSuccess;

            //Now we're certain that we know about the GameSrv
            if (reply.fResult == ENetError.kNetSuccess) {
                reply.fGameServerIP = fParent.GetAgeIP(req.fAgeFilename, req.fAgeInstanceUuid, req.fAgeVaultID);
                reply.fAgeMcpID = fParent.GetAgeMcpID(req.fAgeFilename, req.fAgeInstanceUuid, req.fAgeVaultID);
                if (reply.fGameServerIP == null) reply.fResult = ENetError.kNetErrNameLookupFailed;
            }

            fStream.BufferWriter();
            fStream.WriteUShort((ushort)LookupSrv2Cli.FindAgeReply);
            reply.Write(fStream);
            fStream.FlushWriter();
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

            if (fSrvHost != null)
                fParent.UpdateCliCount(fSrvHost, fConnType, cc.fNumClients);
        }
    }
}
