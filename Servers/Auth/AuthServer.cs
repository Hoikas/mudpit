using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace MUd {
    public class AuthServer {

        private List<AuthThread> fClients = new List<AuthThread>();
        private LogProcessor fLog = new LogProcessor("AuthServer");

        public AuthServer() {
            DbConnection db = Database.Connect();
            Database.ExecuteNonQuery(db, 
                                     @"CREATE TABLE IF NOT EXISTS `Accounts` (
                                     `Idx` int(10) unsigned NOT NULL AUTO_INCREMENT,
                                     `Name` char(64) NOT NULL,
                                     `HashedPassword` char(40) NOT NULL,
                                     `AcctUUID` char(36) NOT NULL,
                                     `PrivLevel` tinyint(1) NOT NULL,
                                      PRIMARY KEY (`Idx`)
                                      ) ENGINE=MyISAM  DEFAULT CHARSET=utf8;");

            Database.ExecuteNonQuery(db,
                                     @"CREATE TABLE IF NOT EXISTS `Players` (
                                     `Idx` int(10) unsigned NOT NULL AUTO_INCREMENT,
                                     `NodeIdx` int(10) unsigned NOT NULL,
                                     `Name` char(40) NOT NULL,
                                     `Model` char(64) NOT NULL,
                                     `AcctUUID` char(36) NOT NULL,
                                      PRIMARY KEY (`Idx`)
                                      ) ENGINE=MyISAM DEFAULT CHARSET=utf8;");
            db.Close();
        }

        public void Add(Socket c, ConnectHeader hdr) {
            AuthThread ft = new AuthThread(this, c, hdr, fLog);
            ft.Start();

            Monitor.Enter(fClients);
            fClients.Add(ft);
            Monitor.Exit(fClients);
        }

        public void Remove(AuthThread ft) {
            Monitor.Enter(fClients);
            fClients.Remove(ft);
            Monitor.Exit(fClients);
        }
    }
}
