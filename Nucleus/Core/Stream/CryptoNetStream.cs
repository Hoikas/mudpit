using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using OpenSSL;

namespace MUd {
    public class CryptoNetStream : Stream {

        private IRC4 fEncrypt, fDecrypt;
        private Socket fSocket;

        public override bool CanRead {
            get { return true; }
        }

        public override bool CanSeek {
            get { return false; }
        }

        public override bool CanWrite {
            get { return true; }
        }

        public override long Length {
            get { throw new NotSupportedException(); }
        }

        public override long Position {
            get {
                throw new NotSupportedException();
            }
            set {
                throw new NotSupportedException();
            }
        }

        public CryptoNetStream(byte[] key, Socket s) {
            fSocket = s;
            IInit(key); 
        }

        private void IInit(byte[] key) {
            fEncrypt = new ManagedRC4(key);
            fDecrypt = new ManagedRC4(key);
        }

        public override void Flush() {
            throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count) {
            int size = fSocket.Receive(buffer, offset, count, SocketFlags.None);
            fDecrypt.Transform(buffer, offset, size);
            return size;
        }

        public override long Seek(long offset, SeekOrigin origin) {
            throw new NotSupportedException();
        }

        public override void SetLength(long value) {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count) {
            fEncrypt.Transform(buffer, offset, count);
            fSocket.Send(buffer, offset, count, SocketFlags.None);
        }
    }

    public class ManagedRC4 : IRC4 {

        byte[] fState = new byte[256];
        int fX = 0, fY = 0;

        public ManagedRC4(byte[] key) {
            for (int i = 0; i < 256; i++) fState[i] = (byte)i;

            int x = 0;
            for (int i = 0; i < 256; i++) {
                x = (key[i % key.Length] + fState[i] + x) & 0xFF;

                //Swap
                byte i1 = fState[i];
                byte x1 = fState[x];

                fState[x] = i1;
                fState[i] = x1;
            }
        }

        public void Transform(byte[] buffer, int offset, int size) {
            if (offset != 0) {
                byte[] temp = new byte[size];
                Buffer.BlockCopy(buffer, offset, temp, 0, size);
                Transform(temp, 0, size);
                Buffer.BlockCopy(temp, 0, buffer, offset, size);
            } else {
                for (int i = offset; i < offset + size; i++) {
                    fX = (fX + 1) & 0xFF;
                    fY = (fState[fX] + fY) & 0xFF;

                    //Swap
                    byte y = fState[fY];
                    byte x = fState[fX];

                    fState[fX] = y;
                    fState[fY] = x;

                    buffer[i] = (byte)(buffer[i] ^ fState[(fState[fX] + fState[fY]) & 0xFF]);
                }
            }
        }
    }
}
