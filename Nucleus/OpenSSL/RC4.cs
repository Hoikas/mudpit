using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace OpenSSL {

    public interface IRC4 {
        void Transform(byte[] buffer, int offset, int size);
    }

    public class RC4 : IDisposable, IRC4 {

        private IntPtr fKey;
        private bool fDisposed;

        public bool Disposed {
            get { return fDisposed; }
        }

        public RC4(byte[] key) {
            RC4_KEY mkey = new RC4_KEY();
            fKey = Marshal.AllocHGlobal(258);
            Marshal.StructureToPtr(mkey, fKey, false);

            OpenSSL.RC4_set_key(fKey, key.Length, key);
        }

        ~RC4() { if (!fDisposed) Dispose(); }

        public void Dispose() {
            if (fDisposed) throw new ObjectDisposedException("fKey");

            GC.SuppressFinalize(this);
            try {
                OpenSSL.CRYPTO_free(fKey);
            } catch (AccessViolationException) { }
            fDisposed = true;
        }

        public byte[] Transform(byte[] inbuf) {
            if (fDisposed) throw new ObjectDisposedException("fKey");

            byte[] outbuf = new byte[inbuf.Length];
            OpenSSL.RC4(fKey, (uint)inbuf.Length, inbuf, outbuf);
            return outbuf;
        }

        public void Transform(byte[] buffer, int offset, int size) {
            if (fDisposed) throw new ObjectDisposedException("fKey");

            byte[] inbuf = new byte[size];
            byte[] outbuf = new byte[size];

            Buffer.BlockCopy(buffer, offset, inbuf, 0, size);
            OpenSSL.RC4(fKey, (uint)size, inbuf, outbuf);
            Buffer.BlockCopy(outbuf, 0, buffer, offset, size);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct RC4_KEY {
        public byte x, y;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst=256)]
        public byte[] data;
    }

    #region Old Code
    /*
    public class RC4 : SymmetricAlgorithm {

        private byte[] fKey;

        public override byte[] Key {
            get { return fKey; }
            set { fKey = value; }
        }

        public override ICryptoTransform CreateDecryptor() {
            return new RC4Transform(fKey);
        }

        public override ICryptoTransform CreateDecryptor(byte[] rgbKey, byte[] rgbIV) {
            return new RC4Transform(rgbKey);
        }

        public override ICryptoTransform CreateEncryptor() {
            return new RC4Transform(fKey);
        }

        public override ICryptoTransform CreateEncryptor(byte[] rgbKey, byte[] rgbIV) {
            return new RC4Transform(rgbKey);
        }

        public override void GenerateIV() {
            throw new NotImplementedException();
        }

        public override void GenerateKey() {
            throw new NotImplementedException();
        }
    }

    public class RC4Transform : ICryptoTransform {

        private bool fDisposed = false;
        private IntPtr fKeyPtr;

        ~RC4Transform() { if (!fDisposed) Dispose(); }

        public bool CanReuseTransform {
            get { return true; }
        }

        public bool CanTransformMultipleBlocks {
            get { return true;  }
        }

        public int InputBlockSize {
            get { throw new NotImplementedException(); }
        }

        public int OutputBlockSize {
            get { throw new NotImplementedException(); }
        }

        internal RC4Transform(byte[] key) {
            OpenSSL.RC4_set_key(ref fKeyPtr, key.Length, key);
        }

        public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset) {
            if (fDisposed)
                throw new ObjectDisposedException("this");

            byte[] in_buf = null;
            if (inputOffset != 0) {
                in_buf = new byte[inputCount];
                Array.Copy(inputBuffer, inputOffset, in_buf, 0, inputCount);
            } else in_buf = inputBuffer;

            byte[] out_buf = new byte[outputBuffer.Length - outputOffset];
            OpenSSL.RC4(fKeyPtr, (uint)in_buf.Length, in_buf, out_buf);
            Array.Copy(out_buf, 0, outputBuffer, outputOffset, out_buf.Length);
            return out_buf.Length;
        }

        public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount) {
            if (fDisposed)
                throw new ObjectDisposedException("this");

            byte[] in_buf = null;
            if (inputOffset != 0) {
                in_buf = new byte[inputCount];
                Array.Copy(inputBuffer, inputOffset, in_buf, 0, inputCount);
            } else in_buf = inputBuffer;

            byte[] out_buf = new byte[inputCount];
            OpenSSL.RC4(fKeyPtr, (uint)inputCount, in_buf, out_buf);
            return out_buf;
        }

        public void Dispose() {
            if (!fDisposed) {
                OpenSSL.CRYPTO_free(fKeyPtr);
                fDisposed = true;
                GC.SuppressFinalize(this);
            }
        }
    }
    */
    #endregion
}
