using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;

namespace MUd {
    public struct Callback {
        public Delegate fFunc;
        public object[] fMyArgs;

        public Callback(Delegate func) {
            fFunc = func;
            fMyArgs = new object[0];
        }

        public Callback(Delegate func, object[] args) {
            fFunc = func;
            fMyArgs = args;
        }
    }

    public class Player {
        public string fName;
        public uint fID;

        public Player(string name, uint id) {
            fName = name;
            fID = id;
        }

        public override string ToString() {
            return fName;
        }
    }

    public class Prefrences {
        private static RegistryKey BaseKey {
            get { return Registry.CurrentUser.CreateSubKey("Software\\MUd\\WhoM"); }
        }

        public static bool AutoConnect {
            get { return Convert.ToBoolean(BaseKey.GetValue("AutoConnect", false)); }
            set {
                RegistryKey key = BaseKey;
                key.SetValue("AutoConnect", value);
                key.Close();
            }
        }

        public static bool BuddyAlert {
            get { return Convert.ToBoolean(BaseKey.GetValue("BuddyAlert", true)); }
            set {
                RegistryKey key = BaseKey;
                key.SetValue("BuddyAlert", value);
                key.Close();
            }
        }

        public static uint LastAvatar {
            get { return Convert.ToUInt32(BaseKey.GetValue("LastAvatar", 0)); }
            set {
                RegistryKey key = BaseKey;
                key.SetValue("LastAvatar", value);
                key.Close();
            }
        }

        public static bool NeighborAlert {
            get { return Convert.ToBoolean(BaseKey.GetValue("NeighborAlert", false)); }
            set {
                RegistryKey key = BaseKey;
                key.SetValue("NeighborAlert", value);
                key.Close();
            }
        }

        public static string Password {
            get { return BaseKey.GetValue("SavedPassword").ToString(); }
            set {
                RegistryKey key = BaseKey;
                key.SetValue("SavedPassword", value, RegistryValueKind.String);
                key.Close();
            }
        }

        public static string Shard {
            get { return BaseKey.GetValue("LastShard", "184.73.198.22").ToString(); }
            set {
                RegistryKey key = BaseKey;
                key.SetValue("LastShard", value);
                key.Close();
            }
        }

        public static bool RememberLogin {
            get { return Convert.ToBoolean(BaseKey.GetValue("RememberLogin", false)); }
            set {
                RegistryKey key = BaseKey;
                key.SetValue("RememberLogin", value);
                key.Close();
            }
        }

        public static string Username {
            get { return BaseKey.GetValue("SavedUsername").ToString(); }
            set {
                RegistryKey key = BaseKey;
                key.SetValue("SavedUsername", value, RegistryValueKind.String);
                key.Close();
            }
        }
    }
}
