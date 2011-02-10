using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MUd {
    public class Configuration {

        private static bool fInitialized = false;
        private static Dictionary<string, string> fConfig = new Dictionary<string, string>();

        public static void Initialize(string file) { IReadConfig(file); }
        private static void IReadConfig() { IReadConfig("MUd.conf"); }
        private static void IReadConfig(string file) {
            StreamReader r = null;
            try {
                r = new StreamReader(file);
            } catch {
                return;
            }

            while (!r.EndOfStream) {
                string line = r.ReadLine();
                if (line.Equals(String.Empty) || line.StartsWith("#"))
                    continue;

                //Hack
                string[] hack = line.Split(new char[] { '#' });
                if (hack.Length > 0)
                    line = hack[0];

                string[] cfg = line.Split(new char[] { '=' });
                if (cfg.Length == 2) {
                    fConfig.Add(cfg[0].TrimEnd(), cfg[1].TrimStart());
                }
            }

            r.Close();
            fInitialized = true;
        }

        public static bool GetBoolean(string key, bool def) {
            if (!fInitialized) IReadConfig();
            if (fConfig.ContainsKey(key)) {
                if (fConfig[key] == "1") return true;
                else if (fConfig[key] == "0") return false;
                else return Convert.ToBoolean(fConfig[key]);
            } else return def;
        }

        public static int GetEnumInteger(string key, object def, Type type) {
            if (!fInitialized) IReadConfig();
            if (fConfig.ContainsKey(key))
                try { //Did they supply an integer?
                    return Convert.ToInt32(fConfig[key]);
                } catch { //No. Parse the enum value.
                    return (int)Enum.Parse(type, fConfig[key]);
                } else
                return (int)def;

        }

        public static Guid GetGuid(string key) {
            if (!fInitialized) IReadConfig();
            if (fConfig.ContainsKey(key)) {
                return new Guid(fConfig[key]);
            } else {
                return Guid.Empty;
            }
        }

        public static int GetInteger(string key, int def) {
            if (!fInitialized) IReadConfig();
            if (fConfig.ContainsKey(key))
                return Convert.ToInt32(fConfig[key]);
            else
                return def;
        }

        public static uint GetUInteger(string key, uint def) {
            if (!fInitialized) IReadConfig();
            if (fConfig.ContainsKey(key))
                return Convert.ToUInt32(fConfig[key], 16);
            else
                return def;
        }

        public static string GetString(string key, string def) {
            if (!fInitialized) IReadConfig();
            if (fConfig.ContainsKey(key))
                return fConfig[key];
            else
                return def;
        }
    }
}
