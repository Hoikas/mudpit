using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MUd.Newport {
    class Program {
        static void Main(string[] args) {
            Console.WriteLine("MUd: MYST Uru Daemon");

            bool auth = false;
            bool file = false;
            bool game = false;
            bool gate = false;
            bool vault = false;

            foreach (string arg in args) {
                if (arg == "--help") {
                    IDoHelp();
                    return;
                }

                string test = arg.ToLower();
                if (test == "auth")
                    auth = true;
                else if (test == "file")
                    file = true;
                else if (test == "game")
                    game = true;
                else if (test == "gate")
                    gate = true;
                else if (test == "vault")
                    vault = true;
                else
                    Console.WriteLine(String.Format("WARNING: Unknown service [{0}]", arg));
            }

            if (!auth && !file && !game && !gate && !vault) {
                Console.WriteLine("ERROR: No serives specified!");
                Console.WriteLine("Please use \"MUd.Newport.exe --help\" for more information");
                return;
            }

            Console.WriteLine();
            Console.WriteLine("AuthSrv:  " + auth.ToString());
            Console.WriteLine("FileSrv:  " + file.ToString());
            Console.WriteLine("GameSrv:  " + game.ToString());
            Console.WriteLine("GateSrv:  " + gate.ToString());
            Console.WriteLine("VaultSrv: " + vault.ToString());
            Console.WriteLine();
            Console.WriteLine("=-=-=-=-=-=DISPATCHING=-=-=-=-=-=");

            Dispatch d = new Dispatch(auth, file, game, gate, vault);
            d.Run();
        }

        static void IDoHelp() {
            Console.WriteLine("Usage: MUd.Newport.exe [services]");
            Console.WriteLine();
            Console.WriteLine("Services Available:");
            Console.WriteLine("\tauth\t\tA MOUL compatible Auth Server");
            Console.WriteLine("\tfile\t\tA MOUL compatible File Server");
            Console.WriteLine("\tgame\t\tA MOUL compatible GameServer and MUd GameAgent");
            Console.WriteLine("\tgate\t\tA MOUL compatible GateKeeper and MUd Lookup Server");
            Console.WriteLine("\tvault\t\tA MUd Vault Server");
        }
    }
}
