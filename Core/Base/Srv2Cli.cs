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
    public abstract class Srv2CliBase {
        protected Socket fSocket;
        protected UruStream fStream;
        protected LogProcessor fLog;
        protected ConnectHeader fConn;
        protected byte[] fServerSeed = new byte[] { 0x4F, 0x17, 0xC8, 0x19, 0x3D, 0x08, 0xF3 };

        public Srv2CliBase(Socket s, ConnectHeader conn, LogProcessor logger) { fSocket = s; fConn = conn; fLog = logger; }
        public abstract void Start();
        public abstract void Stop();

        protected bool ISetupEncryption(byte[] y_data, byte[] priv, byte[] pub) {
            BigNum Y = new BigNum(y_data);
            BigNum K = new BigNum(priv);
            BigNum N = new BigNum(pub);

            BigNum client_seed = Y.PowMod(K, N);
            byte[] seed_data = client_seed.ToArray();
            byte[] key = new byte[7];

            fServerSeed = RNG.Random(7);
            for (int i = 0; i < key.Length; i++) {
                if (i >= seed_data.Length) key[i] = fServerSeed[i];
                else key[i] = (byte)(seed_data[i] ^ fServerSeed[i]);
            }

            fStream = new UruStream(new CryptoNetStream(key, fSocket));

            K.Dispose();
            N.Dispose();
            client_seed.Dispose();

            return true;
        }
    }

    public abstract class Srv2CliDaemonBase : Srv2CliBase {

        public Srv2CliDaemonBase(Socket s, ConnectHeader conn, LogProcessor logger)
            : base(s, conn, logger) { }

        protected bool ISetupEncryption(string srv, byte[] y_data) {
            string dir = Configuration.GetString("enc_keys", "G:\\Plasma\\Servers\\Encryption Keys");
            string priv = Path.Combine(dir, srv + "_Private.key");
            string pub = Path.Combine(dir, srv + "_Public.key");

            //Test for keys
            if (!File.Exists(pub) || !File.Exists(priv))
                return false;

            byte[] priv_data = new byte[64];
            FileStream fs = new FileStream(priv, FileMode.Open, FileAccess.Read);
            fs.Read(priv_data, 0, 64);
            fs.Close();

            byte[] pub_data = new byte[64];
            fs = new FileStream(pub, FileMode.Open, FileAccess.Read);
            fs.Read(pub_data, 0, 64);
            fs.Close();

            return ISetupEncryption(y_data, priv_data, pub_data);
        }

        protected void IHandleSocketException(SocketException e) {
            switch (e.SocketErrorCode) {
                case SocketError.ConnectionReset:
                    Warn("Connection Reset by Peer");
                    Stop();
                    break;
                default:
                    throw e;
            }
        }

        #region Log Methods
        //These will pretty-fy the message for you.
        //The logger is still directly accessible though...
        protected void Error(string msg) { fLog.Error(String.Format("[{0}] {1}", fSocket.RemoteEndPoint.ToString(), msg)); }
        protected void Warn(string msg) { fLog.Warn(String.Format("[{0}] {1}", fSocket.RemoteEndPoint.ToString(), msg)); }
        protected void Info(string msg) { fLog.Info(String.Format("[{0}] {1}", fSocket.RemoteEndPoint.ToString(), msg)); }
        protected void Debug(string msg) { fLog.Debug(String.Format("[{0}] {1}", fSocket.RemoteEndPoint.ToString(), msg)); }
        protected void Verbose(string msg) { fLog.Verbose(String.Format("[{0}] {1}", fSocket.RemoteEndPoint.ToString(), msg)); }
        #endregion

    }
}
