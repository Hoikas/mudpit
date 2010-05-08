using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace MUd {
    public partial class AuthThread {

        Dictionary<uint, uint> fLookupToAuthMap = new Dictionary<uint, uint>();

        private uint ILookupPopTransID(uint transID) {
            lock (fLookupToAuthMap) {
                uint trans = fLookupToAuthMap[transID];
                fLookupToAuthMap.Remove(transID);
                return trans;
            }
        }

        private void ILookupAgeFound(uint transID, ENetError result, Guid uuid, uint ageVault, IPAddress gameIP) {
            Auth_AgeReply reply = new Auth_AgeReply();
            reply.fAgeInstanceUuid = uuid;
            reply.fAgeMcpID = 0;
            reply.fAgeVaultID = ageVault;
            reply.fGameServerIP = gameIP;
            reply.fResult = result;
            reply.fTransID = ILookupPopTransID(transID);

            lock (fStream) {
                fStream.BufferWriter();
                fStream.WriteUShort((ushort)AuthSrv2Cli.AgeReply);
                reply.Write(fStream);
                fStream.FlushWriter();
            }
        }
    }
}
