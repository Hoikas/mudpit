using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace OpenSSL {
    internal static class OpenSSL {

        public static bool IsDllPresent {
            get {
                switch (DllType) {
                    case DLL.LibCRYPTO:
                        if (File.Exists("libcrypto.so")) return true;
                        if (File.Exists("/usr/lib/libcrypto.so")) return true;
                        if (File.Exists("/usr/local/lib/libcrypto.so")) return true;
                        return false;
                    case DLL.LibEAY32:
                        return File.Exists("libeay32.dll");
                    case DLL.LibEAY64:
                        return File.Exists("libeay64.dll");
                }

                return false;
            }
        }

        enum DLL {
            LibEAY32,
            LibEAY64,
            LibCRYPTO,
        }

        private static DLL DllType {
            get {
                switch (Environment.OSVersion.Platform) {
                    case PlatformID.Win32NT:
                    case PlatformID.Win32Windows:
                        if (IntPtr.Size == 4)
                            return DLL.LibEAY32;
                        else if (IntPtr.Size == 8)
                            return DLL.LibEAY64;
                        else
                            throw new NotSupportedException("IntPtr.Size = " + IntPtr.Size.ToString());
                    case PlatformID.MacOSX:
                    case PlatformID.Unix:
                        return DLL.LibCRYPTO;
                    default:
                        throw new PlatformNotSupportedException(Environment.OSVersion.Platform.ToString());
                }
            }
        }

        #region BigNum
        public static IntPtr BN_new() {
            switch (DllType) {
                case DLL.LibCRYPTO:
                    return ImpUnix.BN_new();
                case DLL.LibEAY32:
                    return ImpWin32.BN_new();
                case DLL.LibEAY64:
                    return ImpWin64.BN_new();
            }

            return IntPtr.Zero;
        }

        public static void BN_free(IntPtr a) {
            switch (DllType) {
                case DLL.LibCRYPTO:
                    ImpUnix.BN_free(a);
                    break;
                case DLL.LibEAY32:
                    ImpWin32.BN_free(a);
                    break;
                case DLL.LibEAY64:
                    ImpWin64.BN_free(a);
                    break;
            }
        }

        public static void BN_clear(IntPtr a) {
            switch (DllType) {
                case DLL.LibCRYPTO:
                    ImpUnix.BN_clear(a);
                    break;
                case DLL.LibEAY32:
                    ImpWin32.BN_clear(a);
                    break;
                case DLL.LibEAY64:
                    ImpWin64.BN_clear(a);
                    break;
            }
        }

        public static void BN_clear_free(IntPtr a) {
            switch (DllType) {
                case DLL.LibCRYPTO:
                    ImpUnix.BN_clear_free(a);
                    break;
                case DLL.LibEAY32:
                    ImpWin32.BN_clear_free(a);
                    break;
                case DLL.LibEAY64:
                    ImpWin64.BN_clear_free(a);
                    break;
            }
        }

        public static int BN_num_bits(IntPtr a) {
            switch (DllType) {
                case DLL.LibCRYPTO:
                    return ImpUnix.BN_num_bits(a);
                case DLL.LibEAY32:
                    return ImpWin32.BN_num_bits(a);
                case DLL.LibEAY64:
                    return ImpWin64.BN_num_bits(a);
            }

            return -1;
        }

        public static int BN_add(IntPtr r, IntPtr a, IntPtr b) {
            switch (DllType) {
                case DLL.LibCRYPTO:
                    return ImpUnix.BN_add(r, a, b);
                case DLL.LibEAY32:
                    return ImpWin32.BN_add(r, a, b);
                case DLL.LibEAY64:
                    return ImpWin64.BN_add(r, a, b);
            }

            return -1;
        }

        public static int BN_sub(IntPtr r, IntPtr a, IntPtr b) {
            switch (DllType) {
                case DLL.LibCRYPTO:
                    return ImpUnix.BN_sub(r, a, b);
                case DLL.LibEAY32:
                    return ImpWin32.BN_sub(r, a, b);
                case DLL.LibEAY64:
                    return ImpWin64.BN_sub(r, a, b);
            }

            return -1;
        }

        public static int BN_mul(IntPtr r, IntPtr a, IntPtr b, IntPtr ctx) {
            switch (DllType) {
                case DLL.LibCRYPTO:
                    return ImpUnix.BN_mul(r, a, b, ctx);
                case DLL.LibEAY32:
                    return ImpWin32.BN_mul(r, a, b, ctx);
                case DLL.LibEAY64:
                    return ImpWin64.BN_mul(r, a, b, ctx);
            }

            return -1;
        }

        public static int BN_div(IntPtr dv, IntPtr rem, IntPtr a, IntPtr d, IntPtr ctx) {
            switch (DllType) {
                case DLL.LibCRYPTO:
                    return ImpUnix.BN_div(dv, rem, a, d, ctx);
                case DLL.LibEAY32:
                    return ImpWin32.BN_div(dv, rem, a, d, ctx);
                case DLL.LibEAY64:
                    return ImpWin64.BN_div(dv, rem, a, d, ctx);
            }

            return -1;
        }


        public static int BN_mod_exp(IntPtr r, IntPtr a, IntPtr p, IntPtr m, IntPtr ctx) {
            switch (DllType) {
                case DLL.LibCRYPTO:
                    return ImpUnix.BN_mod_exp(r, a, p, m, ctx);
                case DLL.LibEAY32:
                    return ImpWin32.BN_mod_exp(r, a, p, m, ctx);
                case DLL.LibEAY64:
                    return ImpWin64.BN_mod_exp(r, a, p, m, ctx);
            }

            return -1;
        }

        public static int BN_cmp(IntPtr a, IntPtr b) {
            switch (DllType) {
                case DLL.LibCRYPTO:
                    return ImpUnix.BN_cmp(a, b);
                case DLL.LibEAY32:
                    return ImpWin32.BN_cmp(a, b);
                case DLL.LibEAY64:
                    return ImpWin64.BN_cmp(a, b);
            }

            return -1;
        }

        public static int BN_rand(IntPtr rnd, int bits, int top, int bottom) {
            switch (DllType) {
                case DLL.LibCRYPTO:
                    return ImpUnix.BN_rand(rnd, bits, top, bottom);
                case DLL.LibEAY32:
                    return ImpWin32.BN_rand(rnd, bits, top, bottom);
                case DLL.LibEAY64:
                    return ImpWin64.BN_rand(rnd, bits, top, bottom);
            }

            return -1;
        }

        public static IntPtr BN_generate_prime(IntPtr ret, int bits, int safe, IntPtr add, IntPtr rem, PrimeGenerator cb, IntPtr cb_args) {
            switch (DllType) {
                case DLL.LibCRYPTO:
                    return ImpUnix.BN_generate_prime(ret, bits, safe, add, rem, cb, cb_args);
                case DLL.LibEAY32:
                    return ImpWin32.BN_generate_prime(ret, bits, safe, add, rem, cb, cb_args);
                case DLL.LibEAY64:
                    return ImpWin64.BN_generate_prime(ret, bits, safe, add, rem, cb, cb_args);
            }

            return IntPtr.Zero;
        }

        public static int BN_bn2bin(IntPtr a, byte[] to) {
            switch (DllType) {
                case DLL.LibCRYPTO:
                    return ImpUnix.BN_bn2bin(a, to);
                case DLL.LibEAY32:
                    return ImpWin32.BN_bn2bin(a, to);
                case DLL.LibEAY64:
                    return ImpWin64.BN_bn2bin(a, to);
            }

            return -1;
        }

        public static IntPtr BN_bin2bn(byte[] s, int len, IntPtr ret) {
            switch (DllType) {
                case DLL.LibCRYPTO:
                    return ImpUnix.BN_bin2bn(s, len, ret);
                case DLL.LibEAY32:
                    return ImpWin32.BN_bin2bn(s, len, ret);
                case DLL.LibEAY64:
                    return ImpWin64.BN_bin2bn(s, len, ret);
            }

            return IntPtr.Zero;
        }

        public static IntPtr BN_bn2hex(IntPtr a) {
            switch (DllType) {
                case DLL.LibCRYPTO:
                    return ImpUnix.BN_bn2hex(a);
                case DLL.LibEAY32:
                    return ImpWin32.BN_bn2hex(a);
                case DLL.LibEAY64:
                    return ImpWin64.BN_bn2hex(a);
            }

            return IntPtr.Zero;
        }

        public static IntPtr BN_bn2dec(IntPtr a) {
            switch (DllType) {
                case DLL.LibCRYPTO:
                    return ImpUnix.BN_bn2dec(a);
                case DLL.LibEAY32:
                    return ImpWin32.BN_bn2dec(a);
                case DLL.LibEAY64:
                    return ImpWin64.BN_bn2dec(a);
            }

            return IntPtr.Zero;
        }

        public static int BN_hex2bn(ref IntPtr a, byte[] str) {
            switch (DllType) {
                case DLL.LibCRYPTO:
                    return ImpUnix.BN_hex2bn(ref a, str);
                case DLL.LibEAY32:
                    return ImpWin32.BN_hex2bn(ref a, str);
                case DLL.LibEAY64:
                    return ImpWin64.BN_hex2bn(ref a, str);
            }

            return -1;
        }

        public static int BN_dec2bn(ref IntPtr a, byte[] str) {
            switch (DllType) {
                case DLL.LibCRYPTO:
                    return ImpUnix.BN_dec2bn(ref a, str);
                case DLL.LibEAY32:
                    return ImpWin32.BN_dec2bn(ref a, str);
                case DLL.LibEAY64:
                    return ImpWin64.BN_dec2bn(ref a, str);
            }

            return -1;
        }
        #endregion

        #region BigNum CTX
        public static IntPtr BN_CTX_new() {
            switch (DllType) {
                case DLL.LibCRYPTO:
                    return ImpUnix.BN_CTX_new();
                case DLL.LibEAY32:
                    return ImpWin32.BN_CTX_new();
                case DLL.LibEAY64:
                    return ImpWin64.BN_CTX_new();
            }

            return IntPtr.Zero;
        }

        public static IntPtr BN_CTX_free(IntPtr a) {
            switch (DllType) {
                case DLL.LibCRYPTO:
                    return ImpUnix.BN_CTX_free(a);
                case DLL.LibEAY32:
                    return ImpWin32.BN_CTX_free(a);
                case DLL.LibEAY64:
                    return ImpWin64.BN_CTX_free(a);
            }

            return IntPtr.Zero;
        }
        #endregion

        #region ERR
        public static uint ERR_get_error() {
            switch (DllType) {
                case DLL.LibCRYPTO:
                    return ImpUnix.ERR_get_error();
                case DLL.LibEAY32:
                    return ImpWin32.ERR_get_error();
                case DLL.LibEAY64:
                    return ImpWin64.ERR_get_error();
            }

            return 0;
        }

        public static byte[] ERR_get_error_string(uint e, byte[] buf) {
            switch (DllType) {
                case DLL.LibCRYPTO:
                    return ImpUnix.ERR_get_error_string(e, buf);
                case DLL.LibEAY32:
                    return ImpWin32.ERR_get_error_string(e, buf);
                case DLL.LibEAY64:
                    return ImpWin64.ERR_get_error_string(e, buf);
            }

            return new byte[0];
        }
        #endregion

        #region RAND
        public static int RAND_bytes(byte[] buf, int num) {
            switch (DllType) {
                case DLL.LibCRYPTO:
                    return ImpUnix.RAND_bytes(buf, num);
                case DLL.LibEAY32:
                    return ImpWin32.RAND_bytes(buf, num);
                case DLL.LibEAY64:
                    return ImpWin64.RAND_bytes(buf, num);
            }

            return -1;
        }

        public static void RAND_cleanup() {
            switch (DllType) {
                case DLL.LibCRYPTO:
                    ImpUnix.RAND_cleanup();
                    break;
                case DLL.LibEAY32:
                    ImpWin32.RAND_cleanup();
                    break;
                case DLL.LibEAY64:
                    ImpWin64.RAND_cleanup();
                    break;
            }
        }

        public static int RAND_load_file(byte[] filename, int max_bytes) {
            switch (DllType) {
                case DLL.LibCRYPTO:
                    return ImpUnix.RAND_load_file(filename, max_bytes);
                case DLL.LibEAY32:
                    return ImpWin32.RAND_load_file(filename, max_bytes);
                case DLL.LibEAY64:
                    return ImpWin64.RAND_load_file(filename, max_bytes);
            }

            return -1;
        }

        public static void RAND_screen() {
            switch (DllType) {
                case DLL.LibCRYPTO:
                    throw new PlatformNotSupportedException();
                case DLL.LibEAY32:
                    ImpWin32.RAND_screen();
                    break;
                case DLL.LibEAY64:
                    ImpWin64.RAND_screen();
                    break;
            }
        }

        public static void RAND_seed(byte[] buf, int num) {
            switch (DllType) {
                case DLL.LibCRYPTO:
                    ImpUnix.RAND_seed(buf, num);
                    break;
                case DLL.LibEAY32:
                    ImpWin32.RAND_seed(buf, num);
                    break;
                case DLL.LibEAY64:
                    ImpWin64.RAND_seed(buf, num);
                    break;
            }
        }
        #endregion

        #region Misc
        public static void CRYPTO_free(IntPtr p) {
            switch (DllType) {
                case DLL.LibCRYPTO:
                    ImpUnix.CRYPTO_free(p);
                    break;
                case DLL.LibEAY32:
                    ImpWin32.CRYPTO_free(p);
                    break;
                case DLL.LibEAY64:
                    ImpWin64.CRYPTO_free(p);
                    break;
            }
        }

        public static void CRYPTO_malloc_init() {
            switch (DllType) {
                case DLL.LibCRYPTO:
                    ImpUnix.CRYPTO_malloc_init();
                    break;
                case DLL.LibEAY32:
                    ImpWin32.CRYPTO_malloc_init();
                    break;
                case DLL.LibEAY64:
                    ImpWin64.CRYPTO_malloc_init();
                    break;
            }
        }
        #endregion

        #region RC4 Encryption
        public static void RC4_set_key(IntPtr key, int len, byte[] data) {
            switch (DllType) {
                case DLL.LibCRYPTO:
                    ImpUnix.RC4_set_key(key, len, data);
                    break;
                case DLL.LibEAY32:
                    ImpWin32.RC4_set_key(key, len, data);
                    break;
                case DLL.LibEAY64:
                    ImpWin64.RC4_set_key(key, len, data);
                    break;
            }
        }

        public static void RC4(IntPtr key, uint len, byte[] indata, byte[] outdata) {
            switch (DllType) {
                case DLL.LibCRYPTO:
                    ImpUnix.RC4(key, len, indata, outdata);
                    break;
                case DLL.LibEAY32:
                    ImpWin32.RC4(key, len, indata, outdata);
                    break;
                case DLL.LibEAY64:
                    ImpWin64.RC4(key, len, indata, outdata);
                    break;
            }
        }
        #endregion

        #region SHA Hashing
        public static IntPtr SHA(byte[] d, uint n, byte[] md) {
            switch (DllType) {
                case DLL.LibCRYPTO:
                    return ImpUnix.SHA(d, n, md);
                case DLL.LibEAY32:
                    return ImpWin32.SHA(d, n, md);
                case DLL.LibEAY64:
                    return ImpWin64.SHA(d, n, md);
            }

            return IntPtr.Zero;
        }

        public static IntPtr SHA1(byte[] d, uint n, byte[] md) {
            switch (DllType) {
                case DLL.LibCRYPTO:
                    return ImpUnix.SHA1(d, n, md);
                case DLL.LibEAY32:
                    return ImpWin32.SHA1(d, n, md);
                case DLL.LibEAY64:
                    return ImpWin64.SHA1(d, n, md);
            }

            return IntPtr.Zero;
        }
        #endregion
    }

    public class OpenSSLException : Exception {

        internal OpenSSLException() : base("Generic OpenSSL Exception") { }
        internal OpenSSLException(string message) : base(message) { }
        internal OpenSSLException(string message, Exception innerException) : base(message, innerException) { }
    }
}
