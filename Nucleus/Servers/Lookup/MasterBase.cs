using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using OpenSSL;

namespace MUd {
    public abstract class MasterBase : Srv2CliBase {

        private byte[] fServerSeed;

        protected LookupServer fParent;
        protected UruStream fStream;

        protected MasterBase(LookupServer parent, Socket s, ConnectHeader hdr, LogProcessor log) : base(s, hdr, log) {
            fParent = parent;
        }

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
            if (!ISetupEncryption(y_data)) {
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

        private bool ISetupEncryption(byte[] y_data) {
            string dir = Configuration.GetString("enc_keys", "G:\\Plasma\\Servers\\Encryption Keys");
            string priv = Path.Combine(dir, "Master_Private.key");
            string pub = Path.Combine(dir, "Master_Public.key");

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

        protected abstract void Receive(IAsyncResult ar);

        public override void Stop() {
            fParent.Remove(this);
            fStream.Close();
            fSocket.Close();
        }
    }
}
