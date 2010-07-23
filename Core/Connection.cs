using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using OpenSSL;

using System.Timers;
using Timer = System.Timers.Timer;

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

        protected bool ISetupEncryption(string srv, byte[] y_data) {
            string dir = Configuration.GetString("enc_keys", "G:\\Plasma\\Servers\\Encryption Keys");
            string priv = Path.Combine(dir, srv + "_Private.key");
            string pub = Path.Combine(dir, srv + "_Public.key");

            //Test for keys
            if (!File.Exists(pub) || !File.Exists(priv))
                return false;

            BigNum Y = new BigNum(y_data);
            byte[] data = new byte[64];

            FileStream fs = new FileStream(priv, FileMode.Open, FileAccess.Read);
            fs.Read(data, 0, 64);
            fs.Close();
            BigNum K = new BigNum(data);

            fs = new FileStream(pub, FileMode.Open, FileAccess.Read);
            fs.Read(data, 0, 64);
            fs.Close();
            BigNum N = new BigNum(data);

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

    public enum IdleBehavior {
        Disconnect,
        DoNothing,
        Ping,
    }

    public delegate void ExceptionArgs(Exception e);
    public abstract class Cli2SrvBase {
        public event ExceptionArgs ExceptionHandler;

        protected Socket fSocket;
        protected ConnectHeader fHeader;
        protected UruStream fStream;

        protected IdleBehavior fIdleBeh = IdleBehavior.DoNothing;
        private uint fIdleMs;
        private Timer fIdleTimer = new Timer();

        byte[] fDhData, fClientSeed;
        uint fTransID = 0;

        public bool Connected {
            get { return (fSocket != null && fSocket.Connected); }
        }

        private string fHost = "184.73.198.22";
        public string Host {
            set { fHost = value; }
        }

        private int fPort = 14617;
        public int Port {
            set { fPort = value; }
        }

        protected byte[] fN = new byte[] { 
                     0xA7, 0x72, 0xE4, 0x39, 0xE9, 0x8D, 0x2E, 0x13,
                     0x41, 0x08, 0x92, 0x54, 0x29, 0xE6, 0x38, 0x58,
                     0xEF, 0x81, 0xD9, 0x86, 0x7D, 0x77, 0x4E, 0x5E,
                     0x69, 0x1D, 0x91, 0x12, 0x65, 0x12, 0xC8, 0x10,
                     0x1C, 0x73, 0xB8, 0x90, 0x5D, 0x7A, 0x7B, 0x4E,
                     0xB6, 0x7C, 0xF9, 0xFE, 0x11, 0x30, 0xDB, 0xFF,
                     0xED, 0x10, 0xCE, 0x12, 0xA0, 0xF4, 0xFF, 0x32,
                     0xBA, 0x3A, 0x8D, 0x44, 0xF5, 0x62, 0xBA, 0x8B 
        };

        protected byte[] fX = new byte[] {
                     0xF1, 0x80, 0x28, 0xB1, 0xAD, 0x66, 0xBB, 0xCB,
                     0x6F, 0xCD, 0x53, 0x0F, 0x0F, 0x0A, 0x1A, 0xF4,
                     0xB1, 0x26, 0x3E, 0xB5, 0x03, 0x96, 0xE7, 0xAC,
                     0x1D, 0x58, 0x5F, 0x47, 0x18, 0x9F, 0x08, 0x72,
                     0xA3, 0x51, 0x39, 0x45, 0xF0, 0x8F, 0xD5, 0x36,
                     0x3A, 0x00, 0xD9, 0x17, 0x03, 0x69, 0x93, 0x2A,
                     0x65, 0x21, 0x29, 0x45, 0x04, 0xC8, 0x58, 0xE9,
                     0x50, 0xB6, 0xC9, 0x25, 0x74, 0x80, 0x6D, 0x47
        };

        public byte[] N {
            set { fN = value; }
        }

        public byte[] X {
            set { fX = value; }
        }

        public uint BranchID {
            get { return fHeader.fBranchID; }
            set { fHeader.fBranchID = value; }
        }

        public uint BuildID {
            get { return fHeader.fBuildID; }
            set { fHeader.fBuildID = value; }
        }

        public Guid ProductID {
            get { return fHeader.fProductID; }
            set { fHeader.fProductID = value; }
        }

        protected Cli2SrvBase() {
            fIdleTimer.Elapsed += new ElapsedEventHandler(IIdleTimerFired);
            SetIdleBehavior(IdleBehavior.DoNothing, 30000);

            fHeader = new ConnectHeader();
            fHeader.fBuildType = 50;
            fHeader.fSockHeaderSize = 31;
        }

        #region Connect
        public virtual bool Connect() {
            fSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            fSocket.Connect(fHost, fPort);
            SetIdleBehavior(fIdleBeh, fIdleMs);
            return true;
        }

        protected bool NetCliConnect(int g) {
            UruStream s = new UruStream(new NetworkStream(fSocket, false));

            //NetCliConnect
            ISetupKeys(g);
            s.BufferWriter();
            s.WriteByte((byte)NetCliConnectMsg.kNetCliConnect);
            s.WriteByte(66);
            s.WriteBytes(fDhData);
            s.FlushWriter();

            //Recv NetCliEncrypt
            if (s.ReadByte() != (byte)NetCliConnectMsg.kNetCliEncrypt) { fSocket.Close(); return false; }
            byte[] seed = s.ReadBytes((int)(s.ReadByte() - 2));
            ISetupEncryption(seed);

            s.Close();
            return true;
        }

        public void Disconnect() {
            fIdleTimer.Stop(); //Don't run idle timer after DC
            fSocket.Shutdown(SocketShutdown.Both);
        }

        private void IIdleTimerFired(object sender, ElapsedEventArgs e) {
            if (!Connected) return;
            RunIdleBehavior();
        }

        private void ISetupEncryption(byte[] seed) {
            byte[] key = new byte[7];
            for (int i = 0; i < 7; i++) {
                if (i >= fClientSeed.Length) key[i] = seed[i];
                else key[i] = (byte)(fClientSeed[i] ^ seed[i]);
            }

            fStream = new UruStream(new CryptoNetStream(key, fSocket));
        }

        private bool ISetupKeys(int g) {
            BigNum b = BigNum.Random(512);
            BigNum N = new BigNum(fN);
            BigNum X = new BigNum(fX);

            //Calculate seeds
            BigNum client_seed = X.PowMod(b, N);
            BigNum server_seed = new BigNum(g).PowMod(b, N);

            //Dump data
            fDhData = server_seed.ToArray();
            fClientSeed = client_seed.ToArray();

            //Explicitly dispose unmanaged OpenSSL resources
            b.Dispose();
            N.Dispose();
            X.Dispose();
            client_seed.Dispose();
            server_seed.Dispose();
            return true;
        }
        #endregion

        protected void FireException(Exception e) {
            ExceptionHandler(e);
        }

        public void SetIdleBehavior(IdleBehavior beh, uint ms) {
            fIdleBeh = beh;
            fIdleMs = ms;

            fIdleTimer.Stop();
            if (beh != IdleBehavior.DoNothing) {
                fIdleTimer.AutoReset = true;
                fIdleTimer.Interval = Convert.ToDouble(ms);
                fIdleTimer.Start();
            }
        }

        protected void ResetIdleTimer() {
            if (fIdleBeh != IdleBehavior.DoNothing) {
                fIdleTimer.Stop();
                fIdleTimer.Interval = Convert.ToDouble(fIdleMs);
                fIdleTimer.Start();
            }
        }

        protected abstract void RunIdleBehavior();

        protected uint IGetTransID() {
            uint trans = 0;
            lock (this) {
                trans = fTransID;
                fTransID++;
            }

            return trans;
        }
    }

    public abstract class Srv2SrvBase : Cli2SrvBase {

        protected Guid fToken;
        public Guid Token {
            set { fToken = value; }
        }

        public Srv2SrvBase(string type) : base() {
            string dir = Configuration.GetString("enc_keys", "G:\\Plasma\\Servers\\Encryption Keys");
            string pubkey = Path.Combine(dir, type + "_Public.key");
            string shared = Path.Combine(dir, type + "_Shared.key");

            FileStream fs = new FileStream(pubkey, FileMode.Open, FileAccess.Read);
            fs.Read(fN, 0, 64);
            fs.Close();

            fs = new FileStream(shared, FileMode.Open, FileAccess.Read);
            fs.Read(fX, 0, 64);
            fs.Close();
        }
    }

    public class ShaHash {

        uint[] fHash;

        public byte[] CsHash {
            get {
                byte[] hash = new byte[20];
                for (int i = 0; i < fHash.Length; i++) {
                    byte[] temp = new byte[4];
                    temp = BitConverter.GetBytes(fHash[i]);
                    Array.Reverse(temp);
                    Buffer.BlockCopy(temp, 0, hash, i * 4, 4);
                }

                return hash;
            }
        }

        public uint[] UruHash {
            get {
                uint[] good = new uint[5];
                for (int i = 0; i < 5; i++) {
                    byte[] temp = BitConverter.GetBytes(fHash[i]);
                    Array.Reverse(temp);
                    good[i] = BitConverter.ToUInt32(temp, 0);
                }

                return good;
            }
        }

        public ShaHash(uint[] hash) {
            fHash = new uint[5];
            for (int i = 0; i < 5; i++) {
                byte[] temp = BitConverter.GetBytes(hash[i]);
                Array.Reverse(temp);
                fHash[i] = BitConverter.ToUInt32(temp, 0);
            }
        }

        public ShaHash(byte[] data, bool sha1) {
            byte[] thash = null;
            if (sha1) thash = Hash.SHA1(data);
            else thash = Hash.SHA0(data);

            //MOUL sends the SHA hash as uint[5]
            //C# likes a byte[20]
            fHash = new uint[5];
            for (int i = 0; i < fHash.Length; i++) {
                byte[] temp = new byte[4];
                Buffer.BlockCopy(thash, i * 4, temp, 0, 4);
                Array.Reverse(temp);
                fHash[i] = BitConverter.ToUInt32(temp, 0);
            }
        }

        public static ShaHash HashAcctPW(string acct, string pw) {
            byte[] acctData = Encoding.Unicode.GetBytes(acct.ToLower());
            byte[] pwData = Encoding.Unicode.GetBytes(pw);

            //StrCopy FAIL
            acctData[acctData.Length - 1] = 0;
            acctData[acctData.Length - 2] = 0;

            pwData[pwData.Length - 1] = 0;
            pwData[pwData.Length - 2] = 0;

            byte[] data = new byte[acctData.Length + pwData.Length];
            Buffer.BlockCopy(pwData, 0, data, 0, pwData.Length);
            Buffer.BlockCopy(acctData, 0, data, pwData.Length, acctData.Length);

            ShaHash sha = new ShaHash(data, false);
            return sha;
        }

        public static ShaHash HashLoginInfo(string acct, string pw, int clientChallenge, uint serverChallenge) {
            ShaHash namepass = HashAcctPW(acct, pw);
            byte[] buf = new byte[namepass.CsHash.Length + 8];

            Buffer.BlockCopy(BitConverter.GetBytes(clientChallenge), 0, buf, 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(serverChallenge), 0, buf, 4, 4);
            Buffer.BlockCopy(namepass.CsHash, 0, buf, 8, namepass.CsHash.Length);

            ShaHash sha = new ShaHash(buf, false);
            return sha;
        }

        public static ShaHash HashPW(string pw) {
            return new ShaHash(Encoding.Unicode.GetBytes(pw), true);
        }

        public override string ToString() {
            StringBuilder sb = new StringBuilder();
            foreach (uint b in fHash)
                sb.AppendFormat("{0:x8}", b);

            return sb.ToString();
        }
    }
}
