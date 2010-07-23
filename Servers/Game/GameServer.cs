using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MUd {
    public class GameServer {

        private List<GameThread> fClients = new List<GameThread>();

        public int CliCount {
            get { return fClients.Count; }
        }
        
        private LogProcessor fLogger;
        public LogProcessor Log {
            get { return fLogger; }
        }

        private string fAgeFilename;
        public string AgeFilename {
            get { return fAgeFilename; }
        }

        private Guid fAgeUuid;
        public Guid AgeUUID {
            get { return fAgeUuid; }
        }

        private uint fAgeVaultID;
        public uint AgeVaultID {
            get { return fAgeVaultID; }
        }

        private uint fMcpID;
        public uint McpID {
            get { return fMcpID; }
        }

        public GameServer(string filename, uint mcpid, Guid uuid, uint vaultID) {
            fAgeFilename = filename;
            fAgeUuid = uuid;
            fAgeVaultID = vaultID;
            fMcpID = mcpid;

            fLogger = new LogProcessor(uuid.ToString(), filename);
        }

        public void Add(GameThread gt) {
            lock (fClients)
                fClients.Add(gt);
        }

        public void Remove(GameThread gt) {
            lock (fClients)
                fClients.Remove(gt);
        }

        public void Stop() {
            lock (fClients) {
                GameThread[] copy = new GameThread[fClients.Count];
                fClients.CopyTo(copy);

                foreach (GameThread gt in copy)
                    gt.Stop();
            }

            fLogger.Close();
        }
    }
}
