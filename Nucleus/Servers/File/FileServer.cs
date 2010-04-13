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

        public void Add(Socket c, ConnectHeader hdr) {
            FileThread ft = new FileThread(this, c, hdr, fLog);
            ft.Start();

            Monitor.Enter(fClients);
            fClients.Add(ft);
            Monitor.Exit(fClients);
        }

        public void Remove(FileThread ft) {
            Monitor.Enter(fClients);
            fClients.Remove(ft);
            Monitor.Exit(fClients);
        }
    }
}
