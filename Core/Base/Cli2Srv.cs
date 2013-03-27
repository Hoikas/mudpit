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
            fHeader.fBuildType = NetCliBuildType.kLive;
            fHeader.fSockHeaderSize = 31;
        }

        #region Connect
        public virtual bool Connect() {
            IPAddress ip = IPAddress.Parse(fHost);
            fSocket = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            fSocket.Connect(ip, fPort);
            SetIdleBehavior(fIdleBeh, fIdleMs);
            return true;
        }

        protected virtual bool NetCliConnect(int g) {
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
            if (fSocket != null && fSocket.Connected)
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
            BigNum b = new BigNum(Helpers.StrongRandom(64));
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
            //Sometimes, we might get exceptions due to a disconnect
            //Eg. Client tried to send data immediately after we DCed
            //We'll eat those exceptions in Release mode
#if !DEBUG
            if (Connected)
#endif
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

        public void WriteToStream(byte[] buf) {
            lock (fStream) { fStream.WriteBytes(buf); }
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
}
