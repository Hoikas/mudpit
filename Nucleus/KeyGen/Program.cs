using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using OpenSSL;

namespace KeyGen {
    class Program {

        static Dictionary<string, byte[]> fX = new Dictionary<string, byte[]>();
        static Dictionary<string, byte[]> fN = new Dictionary<string, byte[]>();

        static void Main(string[] args) {
            if (args.Length == 0) {
                IDoHelp();
                return;
            }

            //Parse the arguments
            Dictionary<string, string> map = new Dictionary<string, string>();
            foreach (string arg in args) {
                if (arg.ToLower() == "--help") {
                    IDoHelp();
                    return;
                }

                if (arg.StartsWith("--")) {
                    string[] split = arg.Split(new string [] { "="}, StringSplitOptions.None);
                    map.Add(split[0].Substring(2).ToLower(), split[1]);
                }
            }

            Console.WriteLine("Generating...");
            IGenKeys("Auth");
            IGenKeys("Game");
            IGenKeys("Master");
            IGenKeys("Vault");
            Console.WriteLine("All keys gnerated!");

            if (map.ContainsKey("uru") && map.Count > 1) {
                Console.WriteLine();
                Console.WriteLine();

                FileStream fs = null;
                try {
                    fs = new FileStream(Path.Combine(map["uru"], "UruExplorer.exe"), FileMode.Open, FileAccess.Write);
                } catch (IOException) {
                    Console.WriteLine("ERROR: Could not open UruExplorer.exe!");
                    Console.WriteLine("Keys not embedded.");
                    return;
                }

                if (map.ContainsKey("auth")) {
                    if (map["auth"].StartsWith("0x"))
                        fs.Position = Convert.ToInt64(map["auth"].Substring(2), 16);
                    else 
                        fs.Position = Convert.ToInt64(map["auth"]);
                    fs.Write(fN["auth"], 0, 64);
                    fs.Write(fX["auth"], 0, 64);
                    Console.WriteLine("AUTH key embedded!");
                }

                if (map.ContainsKey("game")) {
                    if (map["auth"].StartsWith("0x"))
                        fs.Position = Convert.ToInt64(map["game"].Substring(2), 16);
                    else
                        fs.Position = Convert.ToInt64(map["game"]);
                    fs.Write(fN["game"], 0, 64);
                    fs.Write(fX["game"], 0, 64);
                    Console.WriteLine("GAME key embedded!");
                }

                if (fs != null) fs.Close();
            }
        }

        private static void IDoHelp() {
            Console.WriteLine("KeyGen.exe");
            Console.WriteLine("Usage: KeyGen.exe [--uru=path] [--auth=addr] [--game=addr] ");
            Console.WriteLine();
            Console.WriteLine("This tool generates Public, Private, and Shared keyfiles for MOUL Servers");
        }

        private static void IGenKeys(string keyname) {
            BigNum N = BigNum.GeneratePrime(512);
            BigNum K = BigNum.GeneratePrime(512);
            BigNum X = new BigNum(4).PowMod(K, N);

            fX.Add(keyname.ToLower(), X.ToArray());
            fN.Add(keyname.ToLower(), N.ToArray());

            string dir = MUd.Configuration.GetString("enc_keys", "G:\\Plasma\\Servers\\Encryption Keys");
            if (!Directory.Exists(dir)) {
                Directory.CreateDirectory(dir);
            }

            //Create Keys...
            FileStream fs = new FileStream(Path.Combine(dir, keyname + "_Public.key"), FileMode.Create, FileAccess.Write);
            fs.SetLength(0); //Truncate
            fs.Write(N.ToArray(), 0, 64);
            fs.Close();

            fs = new FileStream(Path.Combine(dir, keyname + "_Shared.key"), FileMode.Create, FileAccess.Write);
            fs.SetLength(0); //Truncate
            fs.Write(X.ToArray(), 0, 64);
            fs.Close();

            fs = new FileStream(Path.Combine(dir, keyname + "_Private.key"), FileMode.Create, FileAccess.Write);
            fs.SetLength(0); //Truncate
            fs.Write(K.ToArray(), 0, 64);
            fs.Close();

            Console.WriteLine(keyname.ToUpper() + " generated.");

            //Dispose
            N.Dispose();
            K.Dispose();
            X.Dispose();
        }
    }
}
