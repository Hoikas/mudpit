using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;

namespace MUd {
    public class Database {

        public static DbConnection Connect() {
            MySqlConnectionStringBuilder b = new MySqlConnectionStringBuilder();
            b.Database = Configuration.GetString("db_name", "mud");
            b.Password = Configuration.GetString("db_pass", "hogs_play_in_the_mud");
            b.Server = Configuration.GetString("db_host", "127.0.0.1");
            b.UserID = Configuration.GetString("db_user", "mud");

            MySqlConnection c = new MySqlConnection(b.GetConnectionString(true));
            c.Open();

            return c;
        }

        public static uint LastInsertID(DbConnection conn) {
            MySqlCommand cmd = new MySqlCommand("SELECT last_insert_id();", (MySqlConnection)conn);
            MySqlDataReader r = cmd.ExecuteReader();
            r.Read();
            uint id = r.GetUInt32(0);
            r.Close();

            return id;
        }

        public static void ExecuteNonQuery(DbConnection db, string query) {
            MySqlCommand cmd = new MySqlCommand(query, (MySqlConnection)db);
            cmd.ExecuteNonQuery();
        }

        public static string DateTime(DateTime dt) {
            return dt.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public static DateTime DateTime(string str) {
            return System.DateTime.ParseExact(str, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        }

        public static VaultNode RowToVaultNode(MySqlDataReader r) {
            VaultNode node = new VaultNode((ENodeType)r.GetInt32("NodeType"), VaultNode.ToDateTime(r.GetUInt32("CreateTime")), VaultNode.ToDateTime(r.GetUInt32("ModifyTime")));
            node.ID = r.GetUInt32("Idx");
            node.fCreateAgeName = r.GetString("CreateAgeName");
            node.fCreateAgeUuid = VaultNode.TryGetGuid(r.GetString("CreateAgeUuid"));
            node.fCreatorIdx = r.GetUInt32("CreatorIdx");
            node.fCreatorUuid = VaultNode.TryGetGuid(r.GetString("CreatorUuid"));
            node.fInt32[0] = r.GetInt32("Int32_1");
            node.fInt32[1] = r.GetInt32("Int32_2");
            node.fInt32[2] = r.GetInt32("Int32_3");
            node.fInt32[3] = r.GetInt32("Int32_4");
            node.fUInt32[0] = r.GetUInt32("UInt32_1");
            node.fUInt32[1] = r.GetUInt32("UInt32_2");
            node.fUInt32[2] = r.GetUInt32("UInt32_3");
            node.fUInt32[3] = r.GetUInt32("UInt32_4");
            node.fUuid[0] = VaultNode.TryGetGuid(r.GetString("Uuid_1"));
            node.fUuid[1] = VaultNode.TryGetGuid(r.GetString("Uuid_2"));
            node.fUuid[2] = VaultNode.TryGetGuid(r.GetString("Uuid_3"));
            node.fUuid[3] = VaultNode.TryGetGuid(r.GetString("Uuid_4"));
            node.fString64[0] = r.GetString("String64_1");
            node.fString64[1] = r.GetString("String64_2");
            node.fString64[2] = r.GetString("String64_3");
            node.fString64[3] = r.GetString("String64_4");
            node.fString64[4] = r.GetString("String64_5");
            node.fString64[5] = r.GetString("String64_6");
            node.fIString64[0] = r.GetString("IString64_1");
            node.fIString64[1] = r.GetString("IString64_2");
            node.fText[0] = r.GetString("Text_1");
            node.fText[1] = r.GetString("Text_2");
            node.fBlob[0] = Convert.FromBase64String(r.GetString("Blob_1"));
            node.fBlob[1] = Convert.FromBase64String(r.GetString("Blob_1"));

            return node;
        }
    }

    public abstract class SimpleQuery {

        protected string fTable;
        protected int fLimit = -1;
        
        public string Table {
            get { return fTable; }
            set { fTable = value; }
        }

        public int Limit {
            get { return fLimit; }
            set { fLimit = value; }
        }

        protected abstract string BuildString();

        public void ExcecuteNonQuery(DbConnection conn) {
            MySqlCommand cmd = new MySqlCommand(BuildString(), (MySqlConnection)conn);
            cmd.ExecuteNonQuery();
        }

        public MySqlDataReader ExecuteQuery(DbConnection conn) {
            MySqlCommand cmd = new MySqlCommand(BuildString(), (MySqlConnection)conn);
            return cmd.ExecuteReader();
        }
    }

    public sealed class InsertStatement : SimpleQuery {

        Dictionary<string, string> fInserts = new Dictionary<string, string>();

        public Dictionary<string, string> Insert {
            get { return fInserts; }
            set { fInserts = value; }
        }

        protected override string BuildString() {
            string fields = "";
            string values = "";
            foreach (KeyValuePair<string, string> kvp in fInserts) {
                fields += String.Format(",`{0}`", kvp.Key.Replace("`", "\\`"));
                values += String.Format(",'{0}'", kvp.Value.Replace("'", "\\'"));
            }

            string str = String.Format("INSERT INTO {0} ({1}) VALUES ({2});", fTable, fields.Substring(1), values.Substring(1));
            return str;
        }
    }

    public sealed class SelectStatement : SimpleQuery {

        bool fSelWildcard = false;
        List<string> fSelect = new List<string>();
        Dictionary<string, string> fWhere = new Dictionary<string,string>();

        public List<string> Select {
            get { return fSelect; }
        }

        public Dictionary<string, string> Where {
            get { return fWhere; }
            set { if (value != null) fWhere = value; }
        }

        public bool Wildcard {
            get { return fSelWildcard; }
            set { fSelWildcard = value; }
        }

        protected override string BuildString() {
            string str = "SELECT ";
            if (fSelWildcard) str += "*";
            else {
                bool comma = false;
                foreach (string sel in fSelect) {
                    if (comma) str += ",";
                    str += sel;
                    comma = true;
                }
            }

            str += " FROM " + fTable;
            if (fWhere.Count > 0) {
                str += " WHERE ";
                bool comma = false;
                foreach (KeyValuePair<string, string> kvp in fWhere) {
                    if (comma) str += " AND ";
                    str += String.Format("{0} = '{1}'", kvp.Key, kvp.Value);
                    comma = true;
                }
            }

            if (fLimit != -1)
                str += " LIMIT " + fLimit.ToString();

            return str;
        }
    }

    public sealed class UpdateStatement : SimpleQuery {

        private Dictionary<string, string> fWhere = new Dictionary<string, string>();
        public Dictionary<string, string> Where {
            get { return fWhere; }
            set { if (value != null) fWhere = value; }
        }

        private Dictionary<string, string> fData = new Dictionary<string, string>();
        public Dictionary<string, string> Data {
            get { return fData; }
            set { if (value != null) fData = value; }
        }

        protected override string BuildString() {
            string str = "UPDATE " + fTable;

            if (fData.Count > 0) {
                str += " SET ";
                bool comma = false;
                foreach (KeyValuePair<string, string> kvp in fData) {
                    if (comma) str += ", ";
                    str += String.Format("`{0}` = '{1}'", kvp.Key, kvp.Value.Replace("'", "\\'"));
                    comma = true;
                }
            }

            if (fWhere.Count > 0) {
                str += " WHERE ";
                bool comma = false;
                foreach (KeyValuePair<string, string> kvp in fWhere) {
                    if (comma) str += " AND ";
                    str += String.Format("{0} = '{1}'", kvp.Key, kvp.Value.Replace("'", "\\'"));
                    comma = true;
                }
            }

            return str;
        }
    }
}
