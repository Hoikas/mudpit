using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MUd {
    public abstract class Message : Creatable {

        [Flags]
        public enum BCastFlags {
            kBCastNone = 0x0,
            kBCastByType = 0x1,
            kBCastUNUSED_0 = 0x2,
            kPropagateToChildren = 0x4,
            kBCastByExactType = 0x8,
            kPropagateToModifiers = 0x10,
            kClearAfterBCast = 0x20,
            kNetPropagate = 0x40,
            kNetSent = 0x80,
            kNetUseRelevanceRegions = 0x100,
            kNetForce = 0x200,
            kNetNonLocal = 0x400,
            kLocalPropagate = 0x800,
            kNetNonDeterministic = 0x200,
            kMsgWatch = 0x1000,
            kNetStartCascade = 0x2000,
            kNetAllowInterAge = 0x4000,
            kNetSendUnreliable = 0x8000,
            kCCRSendToAllPlayers = 0x10000,
            kNetCreatedRemotely = 0x20000
        }

        public BCastFlags fBCastFlags;
        public Uoid fSender;
        public List<Uoid> fReceivers = new List<Uoid>();
        public double fTimeStamp;

        public override void Read(UruStream s) {
            fSender = Uoid.ReadKey(s);
            int count = s.ReadInt();
            for (int i = 0; i < count; i++)
                fReceivers.Add(Uoid.ReadKey(s));
            fTimeStamp = s.ReadDouble();
            fBCastFlags = (BCastFlags)s.ReadInt();
        }

        public override void Write(UruStream s) {
            Uoid.WriteKey(s, fSender);
            s.WriteInt(fReceivers.Count);
            foreach (Uoid key in fReceivers)
                Uoid.WriteKey(s, key);
            s.WriteDouble(fTimeStamp);
            s.WriteInt((int)fBCastFlags);
        }
    }
}
