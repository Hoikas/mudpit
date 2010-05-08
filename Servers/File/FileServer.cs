using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace MUd {
    public class FileServer {

        private List<FileThread> fClients = new List<FileThread>();
        private LogProcessor fLog = new LogProcessor("FileServer");
        private LookupClient fLookupCli = new LookupClient();

        public FileServer() {
            fLookupCli.Host = Configuration.GetString("lookup_addr", "192.168.1.2");
            fLookupCli.Port = Configuration.GetInteger("lookup_port", 14617);
            fLookupCli.ProductID = new Guid(Configuration.GetString("file_guid", Guid.Empty.ToString()));
            fLookupCli.Token = new Guid(Configuration.GetString("lookup_token", Guid.Empty.ToString()));

            fLookupCli.Connect();
            fLookupCli.SetNumClients(Configuration.GetString("public_addr", "127.0.0.1"), 0);
        }

        public void Add(Socket c, ConnectHeader hdr) {
            FileThread ft = new FileThread(this, c, hdr, fLog);
            ft.Start();

            lock (fClients) {
                fClients.Add(ft);
                fLookupCli.SetNumClients(Configuration.GetString("public_addr", "127.0.0.1"), (uint)fClients.Count);
            }
        }

        public void Remove(FileThread ft) {
            lock (fClients) {
                fClients.Remove(ft);
                fLookupCli.SetNumClients(Configuration.GetString("public_addr", "127.0.0.1"), (uint)fClients.Count);
            }
        }
    }
}
