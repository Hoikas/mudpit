using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using MySql.Data.MySqlClient;

namespace MUd {
    public class VaultServer {

        private List<VaultThread> fClients = new List<VaultThread>();
        private LogProcessor fLog = new LogProcessor("VaultServer");

        public VaultServer() {
            DbConnection db = Database.Connect();
            Database.ExecuteNonQuery(db, @"CREATE TABLE IF NOT EXISTS `Nodes` (
                                          `Idx` int(10) unsigned NOT NULL AUTO_INCREMENT,
                                          `CreateTime` int(11) unsigned NOT NULL,
                                          `ModifyTime` int(11) unsigned NOT NULL,
                                          `CreateAgeName` char(64) NOT NULL,
                                          `CreateAgeUuid` char(36) NOT NULL,
                                          `CreatorUuid` char(36) NOT NULL,
                                          `CreatorIdx` int(10) unsigned NOT NULL,
                                          `NodeType` int(11) NOT NULL,
                                          `Int32_1` int(11) NOT NULL,
                                          `Int32_2` int(11) NOT NULL,
                                          `Int32_3` int(11) NOT NULL,
                                          `Int32_4` int(11) NOT NULL,
                                          `UInt32_1` int(10) unsigned NOT NULL,
                                          `UInt32_2` int(10) unsigned NOT NULL,
                                          `UInt32_3` int(10) unsigned NOT NULL,
                                          `UInt32_4` int(10) unsigned NOT NULL,
                                          `Uuid_1` char(36) NOT NULL,
                                          `Uuid_2` char(36) NOT NULL,
                                          `Uuid_3` char(36) NOT NULL,
                                          `Uuid_4` char(36) NOT NULL,
                                          `String64_1` char(64) NOT NULL,
                                          `String64_2` char(64) NOT NULL,
                                          `String64_3` char(64) NOT NULL,
                                          `String64_4` char(64) NOT NULL,
                                          `String64_5` char(64) NOT NULL,
                                          `String64_6` char(64) NOT NULL,
                                          `IString64_1` char(64) NOT NULL,
                                          `IString64_2` char(64) NOT NULL,
                                          `Text_1` char(255) NOT NULL,
                                          `Text_2` char(255) NOT NULL,
                                          `Blob_1` text NOT NULL,
                                          `Blob_2` text NOT NULL,
                                          PRIMARY KEY (`Idx`)
                                        ) ENGINE=MyISAM DEFAULT CHARSET=utf8;");

            Database.ExecuteNonQuery(db, @"CREATE TABLE IF NOT EXISTS `NodeRefs` (
                                          `Idx` int(10) unsigned NOT NULL AUTO_INCREMENT,
                                          `ParentIdx` int(10) unsigned NOT NULL,
                                          `ChildIdx` int(10) unsigned NOT NULL,
                                          `SaverIdx` int(10) unsigned NOT NULL,
                                           PRIMARY KEY (`Idx`)
                                           ) ENGINE=MyISAM DEFAULT CHARSET=utf8;");

            //Look for SYSTEM node
            VaultNode system = new VaultNode(ENodeType.kNodeSystem);
            system.fInt32[0] = (int)EStandardNode.kSystemNode;
            if (!INodeExists(db, system)) {
                fLog.Info("Creating SYSTEM Node");
                ICreateNode(db, system);
            }

            VaultFolderNode ginbox = new VaultFolderNode();
            ginbox.FolderType = EStandardNode.kGlobalInboxFolder;
            if (!INodeExists(db, ginbox.BaseNode)) {
                fLog.Info("Creating GlobalInboxFolder");
                ICreateNode(db, ginbox.BaseNode);
                ICreateNodeRef(db, system.ID, ginbox.BaseNode.ID);
            }
        }

        public void Add(Socket c, ConnectHeader hdr) {
            VaultThread ft = new VaultThread(this, c, hdr, fLog);
            ft.Start();

            Monitor.Enter(fClients);
            fClients.Add(ft);
            Monitor.Exit(fClients);
        }

        private void ICreateNode(DbConnection db, VaultNode node) {
            Dictionary<string, string> data = node.NodeData;
            if (data.ContainsKey("Idx")) data.Remove("Idx");
            if (!data.ContainsKey("CreateTime")) data.Add("CreateTime", VaultNode.ToUnixTime(node.CreateTime).ToString());
            if (!data.ContainsKey("ModifyTime")) data.Add("ModifyTime", VaultNode.ToUnixTime(node.ModifyTime).ToString());

            //Stupid bug somewhere in MySQL...
            //Must init all Guid fields to Guid.Empty
            string emptyUuid = Guid.Empty.ToString();
            if (!data.ContainsKey("CreateAgeUuid")) data.Add("CreateAgeUuid", emptyUuid);
            if (!data.ContainsKey("CreatorUuid")) data.Add("CreatorUuid", emptyUuid);
            if (!data.ContainsKey("Uuid_1")) data.Add("Uuid_1", emptyUuid);
            if (!data.ContainsKey("Uuid_2")) data.Add("Uuid_2", emptyUuid);
            if (!data.ContainsKey("Uuid_3")) data.Add("Uuid_3", emptyUuid);
            if (!data.ContainsKey("Uuid_4")) data.Add("Uuid_4", emptyUuid);

            InsertStatement insert = new InsertStatement();
            insert.Insert = data;
            insert.Table = "Nodes";
            insert.ExcecuteNonQuery(db);

            node.ID = Database.LastInsertID(db);
        }

        private void ICreateNodeRef(DbConnection db, uint parent, uint child) {
            InsertStatement i = new InsertStatement();
            i.Insert.Add("ParentIdx", parent.ToString());
            i.Insert.Add("ChildIdx", child.ToString());
            i.Insert.Add("SaverIdx", "0");
            i.Table = "NodeRefs";

            i.ExcecuteNonQuery(db);
        }

        private bool INodeExists(DbConnection db, VaultNode node) {
            SelectStatement sel = new SelectStatement();
            sel.Limit = 1;
            sel.Select.Add("Idx");
            sel.Table = "Nodes";
            sel.Where = node.NodeData;

            MySqlDataReader r = sel.ExecuteQuery(db);
            bool exists = r.Read();
            if (exists) node.ID = r.GetUInt32("Idx");
            r.Close();
            return exists;
        }

        public void NodeAdded(Vault_AddNodeNotify notify) {
            lock (fClients) {
                foreach (VaultThread vt in fClients)
                    vt.NodeAdded(notify);
            }
        }

        public void NodeChanged(Vault_NodeChanged notify) {
            lock (fClients) {
                foreach (VaultThread vt in fClients)
                    vt.NodeChanged(notify);
            }
        }

        public void Remove(VaultThread ft) {
            Monitor.Enter(fClients);
            fClients.Remove(ft);
            Monitor.Exit(fClients);
        }
    }
}