using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using OpenSSL;

namespace MUd {
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
