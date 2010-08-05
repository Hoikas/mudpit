using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MUd {
    public class LoadAvatarMsg : LoadCloneMsg {

        public bool fIsPlayer;
        public Uoid fSpawnPoint;
        public AvTask fInitialTask;
        public string fUserStr;

        public override void Read(UruStream s) {
            base.Read(s);

            fIsPlayer = s.ReadBool();
            fSpawnPoint = Uoid.ReadKey(s);
            fInitialTask = Factory.ReadCreatable(s) as AvTask;
            fUserStr = s.ReadSafeString();
        }

        public override void Write(UruStream s) {
            base.Write(s);

            s.WriteBool(fIsPlayer);
            Uoid.WriteKey(s, fSpawnPoint);
            Factory.WriteCreatable(s, fInitialTask);
            s.WriteSafeString(fUserStr);
        }
    }

    public class LoadCloneMsg : Message {

        public Uoid fCloneKey, fRequestorKey;
        public uint fOriginatingPlayerID, fUserData;
        public bool fValidMsg, fIsLoading;
        public Message fTriggerMsg;

        public override void Read(UruStream s) {
            base.Read(s);

            fCloneKey = Uoid.ReadKey(s);
            fRequestorKey = Uoid.ReadKey(s);
            fOriginatingPlayerID = s.ReadUInt();
            fUserData = s.ReadUInt();
            fValidMsg = s.ReadBool();
            fIsLoading = s.ReadBool();
            fTriggerMsg = Factory.ReadCreatable(s) as Message;
        }

        public override void Write(UruStream s) {
            base.Write(s);

            Uoid.WriteKey(s, fCloneKey);
            Uoid.WriteKey(s, fRequestorKey);
            s.WriteUInt(fOriginatingPlayerID);
            s.WriteUInt(fUserData);
            s.WriteBool(fValidMsg);
            s.WriteBool(fIsLoading);
            Factory.WriteCreatable(s, fTriggerMsg);
        }
    }
}
