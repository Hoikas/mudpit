using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using OpenSSL;

namespace MUd {
    public class Dispatch {

        private Socket fSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private LogProcessor fLog = new LogProcessor("MUd");

        private AuthServer fAuthServer;
        private FileServer fFileServer;
        private LookupServer fLookupServer;
        private VaultServer fVaultServer;

        private bool fAuth = false;
        private bool fFile = false;
        private bool fGame = false;
        private bool fLookup = false;
        private bool fVault = false;

        public Dispatch(bool auth, bool file, bool game, bool lookup, bool vault) {
            fAuth = auth;
            fFile = file;
            fGame = game;
            fLookup = lookup;
            fVault = vault;
        }

        public void Run() {
            //Setup the main socket
            fSocket.Bind(new IPEndPoint(IPAddress.Parse(Configuration.GetString("bindaddr", "127.0.0.1")), Configuration.GetInteger("listen_port", 14617)));
            fSocket.Listen(10);
            fSocket.BeginAccept(new AsyncCallback(IAcceptConnection), null);

            //It is important that lookup be started first.
            if (fLookup && fLookupServer == null) fLookupServer = new LookupServer();

            //Start or stop any services that need to be running...
            if (fAuth && fAuthServer == null) fAuthServer = new AuthServer();
            if (fFile && fFileServer == null) fFileServer = new FileServer();
            if (fVault && fVaultServer == null) fVaultServer = new VaultServer();

            while (true) {
                //Occupy the main thread with drudgery :(
                Thread.Sleep(100);
            }
        }

        private void IAcceptConnection(IAsyncResult ar) {
            Guid auth_guid = new Guid(Configuration.GetString("auth_guid", Guid.Empty.ToString()));

            Socket c = fSocket.EndAccept(ar);

            //Read the connect header...
            UruStream r = new UruStream(new NetworkStream(c, false));
            ConnectHeader hdr = new ConnectHeader();
            hdr.Read(r);
            r.Close();

            //Factorize the connection.
            switch (hdr.fType) {
                case EConnType.kConnTypeCliToAuth:
                    if (fFile) {
                        fLog.Verbose(String.Format("Incoming AUTH connection [{0}]", c.RemoteEndPoint.ToString()));
                        fAuthServer.Add(c, hdr);
                    } else
                        fLog.Warn(String.Format("Incoming AUTH connection [{0}], but we aren't listening for AUTH!", c.RemoteEndPoint.ToString()));
                    break;
                case EConnType.kConnTypeCliToFile:
                    if (fFile) {
                        fLog.Verbose(String.Format("Incoming FILE connection [{0}]", c.RemoteEndPoint.ToString()));
                        fFileServer.Add(c, hdr);
                    } else
                        fLog.Warn(String.Format("Incoming FILE connection [{0}], but we aren't listening for FILE!", c.RemoteEndPoint.ToString()));
                    break;
                case EConnType.kConnTypeCliToGame:
                    throw new NotImplementedException();
                case EConnType.kConnTypeCliToGate:
                    if (fLookup) {
                        fLog.Verbose(String.Format("Incoming GATEKEEPER connection [{0}]", c.RemoteEndPoint.ToString()));
                        fLookupServer.Add(c, hdr);
                    } else
                        fLog.Warn(String.Format("Incoming GATEKEEPER connection [{0}], but we aren't listening for GATEKEEPER!", c.RemoteEndPoint.ToString()));
                    break;
                case EConnType.kConnTypeSrvToLookup:
                    if (fLookup) {
                        fLog.Verbose(String.Format("Incoming LOOKUP connection [{0}]", c.RemoteEndPoint.ToString()));
                        fLookupServer.Add(c, hdr);
                    } else
                        fLog.Warn(String.Format("Incoming LOOKUP connection [{0}], but we aren't listening for LOOKUP!", c.RemoteEndPoint.ToString()));
                    break;
                case EConnType.kConnTypeSrvToVault:
                    if (fVault) {
                        if (hdr.fProductID != auth_guid) {
                            fLog.Warn(String.Format("Vault Client [{0}] supplied invalid ProductUUID ({1})", c.RemoteEndPoint.ToString(), hdr.fProductID.ToString()));
                        } else {
                            fLog.Verbose(String.Format("Incoming VAULT connection [{0}]", c.RemoteEndPoint.ToString()));
                            fVaultServer.Add(c, hdr);
                        }
                    } else fLog.Warn(String.Format("Incoming VAULT connection [{0}], but we aren't listening for VAULT!", c.RemoteEndPoint.ToString()));
                    break;
            }

            fSocket.BeginAccept(new AsyncCallback(IAcceptConnection), null);
        }
    }
}
