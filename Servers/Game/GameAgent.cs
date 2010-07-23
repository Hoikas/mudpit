using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MUd {
    public class GameAgent {

        private Dictionary<uint, GameServer> fGameServers = new Dictionary<uint, GameServer>();
        private List<GameThread> fInTransit = new List<GameThread>();
        private LogProcessor fLog = new LogProcessor("GameAgent");
        private LookupClient fLookupCli;
        private uint fClientCount = 0;
        private uint fMcpID = 0;

        public GameAgent() {
            fLookupCli = new LookupClient();
            fLookupCli.Host = Configuration.GetString("lookup_addr", "127.0.0.1");
            fLookupCli.Port = Configuration.GetInteger("lookup_port", 14617);
            fLookupCli.ProductID = new Guid(Configuration.GetString("game_guid", Guid.Empty.ToString()));
            fLookupCli.Token = new Guid(Configuration.GetString("lookup_token", Guid.Empty.ToString()));

            fLookupCli.StartAge += new LookupStartAgeCmd(IStartAge);

            fLookupCli.Connect();
            fLookupCli.DeclareHost(Configuration.GetString("public_addr", "127.0.0.1"));
        }

        public void Add(Socket c, ConnectHeader hdr) {
            UruStream s = new UruStream(new NetworkStream(c, false));

            //We cannot trust the GameConnectHeader
            //UruExplorer.exe sends garbage
            int size = s.ReadInt() - 4;
            s.ReadBytes(size); //Throw it away

            lock (fInTransit) {
                GameThread gt = new GameThread(this, c, hdr);
                gt.Start();
            }

            lock (this) {
                fClientCount++;
                fLookupCli.SetNumClients(fClientCount);
            }
        }

        private uint IGetMcpID() {
            lock (this) {
                uint id = fMcpID;
                fMcpID++;
                return id;
            }
        }

        public GameServer JoinAge(uint mcpid, GameThread gt) {
            GameServer retval = null;
            lock (fGameServers) {
                if (fGameServers.ContainsKey(mcpid)) {
                    fGameServers[mcpid].Add(gt);
                    retval = fGameServers[mcpid];
                }
            }

            lock (fInTransit)
                fInTransit.Remove(gt);

            return retval;
        }

        private void IStartAge(string age, Guid uuid, uint vaultID) {
            uint mcpid = IGetMcpID();
            GameServer game = new GameServer(age, mcpid, uuid, vaultID);
            

            lock (fGameServers)
                fGameServers.Add(mcpid, game);

            fLookupCli.NotifyAgeStarted(ENetError.kNetSuccess, age, uuid, vaultID, mcpid);
            fLog.Debug(String.Format("Started GameSrv [AGE: {0}] [UUID: {1}]", age, uuid));
        }

        public void Remove(GameThread gt) {
            lock (this) {
                fClientCount--;
                fLookupCli.SetNumClients(fClientCount);
            }

            lock (fGameServers) {
                if (gt.Parent.CliCount == 0) {
                    fLookupCli.NotifyAgeDestroyed(gt.Parent.AgeUUID);
                    fGameServers.Remove(gt.Parent.McpID);
                    gt.Parent.Stop();
                    fLog.Debug(String.Format("Stopped Empty GameSrv [AGE: {0}] [UUID: {1}]", gt.Parent.AgeFilename, gt.Parent.AgeUUID));
                }
            }
        }
    }
}
