using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MUd {
    public enum LookupConnType {
        kAuthSrv,
        kFileSrv,
        kGameSrv,
        kUnknown,
    }

    public class LookupServer {

        const int kMasterHeaderSize = 20;

        private List<MasterBase> fClients = new List<MasterBase>();
        private LogProcessor fLog = new LogProcessor("LookupServer");

        private Dictionary<string, uint> fAuthSrvs = new Dictionary<string, uint>();
        private Dictionary<string, uint> fFileSrvs = new Dictionary<string, uint>();
        private Dictionary<string, uint> fGameSrvs = new Dictionary<string, uint>();

        public void Add(Socket c, ConnectHeader hdr) {
            UruStream s = new UruStream(new NetworkStream(c, false));

            int size = s.ReadInt();
            if (size != kMasterHeaderSize) {
                fLog.Error(String.Format("[Client: {0}] Invalid connect header size (0x{1} bytes). Kicked.", c.RemoteEndPoint.ToString(), size.ToString("X")));
                s.Close();
                c.Close();
                return;
            }

            Guid token = new Guid(s.ReadBytes(size - 4));
            MasterBase mb = null;
            switch (hdr.fType) {
                case EConnType.kConnTypeCliToGate:
                    if (token != Guid.Empty)
                        fLog.Warn(String.Format("[Client: {0}] Got an interesting token [{1}] for GateKeeper", c.RemoteEndPoint.ToString(), token.ToString()));
                    mb = new GateThread(this, c, hdr, fLog);
                    break;
                case EConnType.kConnTypeSrvToLookup:
                    LookupConnType type = LookupConnType.kUnknown;
                    if (hdr.fProductID == new Guid(Configuration.GetString("auth_guid", Guid.Empty.ToString())))
                        type = LookupConnType.kAuthSrv;
                    else if (hdr.fProductID == new Guid(Configuration.GetString("file_guid", Guid.Empty.ToString())))
                        type = LookupConnType.kFileSrv;
                    else if (hdr.fProductID == new Guid(Configuration.GetString("game_guid", Guid.Empty.ToString())))
                        type = LookupConnType.kGameSrv;
                    else {
                        fLog.Warn(String.Format("[Client: {0}] Got an interesting ProductUUID [{1}] for Lookup", c.RemoteEndPoint.ToString(), hdr.fProductID.ToString()));
                    }

                    if (token == new Guid(Configuration.GetString("lookup_token", Guid.Empty.ToString())))
                        mb = new LookupThread(this, type, c, hdr, fLog);
                    else
                        fLog.Error(String.Format("[Client: {0}] Got an interesting token [{1}] for Lookup... Goodbye.", c.RemoteEndPoint.ToString(), token.ToString()));

                    break;
            }

            if (mb != null) {
                lock (fClients)
                    fClients.Add(mb);
                mb.Start();
                s.Close();
            } else {
                s.Close();
                c.Close();
            }
        }

        public string GetBestServer(LookupConnType type) {
            KeyValuePair<string, uint> winner = new KeyValuePair<string, uint>(null, UInt32.MaxValue);

            switch (type) {
                case LookupConnType.kAuthSrv:
                    lock (fAuthSrvs) {
                        foreach (KeyValuePair<string, uint> kvp in fAuthSrvs) {
                            if (kvp.Value < winner.Value)
                                winner = kvp;
                        }
                    }

                    break;
                case LookupConnType.kFileSrv:
                    lock (fFileSrvs) {
                        foreach (KeyValuePair<string, uint> kvp in fFileSrvs) {
                            if (kvp.Value < winner.Value)
                                winner = kvp;
                        }
                    }

                    break;
                case LookupConnType.kGameSrv:
                    lock (fGameSrvs) {
                        foreach (KeyValuePair<string, uint> kvp in fGameSrvs) {
                            if (kvp.Value < winner.Value)
                                winner = kvp;
                        }
                    }

                    break;
            }

            return winner.Key;
        }

        public void Remove(MasterBase mb) {
            lock (fClients) {
                if (fClients.Contains(mb))
                    fClients.Remove(mb);
            }
        }

        public void UpdateCliCount(string host, LookupConnType type, uint count) {
            switch (type) {
                case LookupConnType.kAuthSrv:
                    lock (fAuthSrvs) {
                        if (!fAuthSrvs.ContainsKey(host)) fAuthSrvs.Add(host, count);
                        else fAuthSrvs[host] = count;
                    }

                    break;
                case LookupConnType.kFileSrv:
                    lock (fFileSrvs) {
                        if (!fFileSrvs.ContainsKey(host)) fFileSrvs.Add(host, count);
                        else fFileSrvs[host] = count;
                    }

                    break;
                case LookupConnType.kGameSrv:
                    lock (fGameSrvs) {
                        if (!fGameSrvs.ContainsKey(host)) fGameSrvs.Add(host, count);
                        else fGameSrvs[host] = count;
                    }

                    break;
            }
        }
    }
}
