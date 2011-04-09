using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MUd {
    public enum EConnType {
        kConnTypeNil, kConnTypeDebug,
        kConnTypeCliToAuth = 10, kConnTypeCliToGame,
        kConnTypeCliToFile = 16, kConnTypeCliToGate = 22,

        kConnTypeSrvToVault = 100, 
        kConnTypeSrvToMaster,
        kConnTypeCliToAdmin,
    }

    public enum ENetError {
        kNetPending = -1,
        kNetSuccess = 0, kNetErrInternalError, kNetErrTimeout, kNetErrBadServerData,
        kNetErrAgeNotFound, kNetErrConnectFailed, kNetErrDisconnected,
        kNetErrFileNotFound, kNetErrOldBuildId, kNetErrRemoteShutdown,
        kNetErrTimeoutOdbc, kNetErrAccountAlreadyExists, kNetErrPlayerAlreadyExists,
        kNetErrAccountNotFound, kNetErrPlayerNotFound, kNetErrInvalidParameter,
        kNetErrNameLookupFailed, kNetErrLoggedInElsewhere, kNetErrVaultNodeNotFound,
        kNetErrMaxPlayersOnAcct, kNetErrAuthenticationFailed,
        kNetErrStateObjectNotFound, kNetErrLoginDenied, kNetErrCircularReference,
        kNetErrAccountNotActivated, kNetErrKeyAlreadyUsed, kNetErrKeyNotFound,
        kNetErrActivationCodeNotFound, kNetErrPlayerNameInvalid,
        kNetErrNotSupported, kNetErrServiceForbidden, kNetErrAuthTokenTooOld,
        kNetErrMustUseGameTapClient, kNetErrTooManyFailedLogins,
        kNetErrGameTapConnectionFailed, kNetErrGTTooManyAuthOptions,
        kNetErrGTMissingParameter, kNetErrGTServerError, kNetErrAccountBanned,
        kNetErrKickedByCCR, kNumNetErrors,

        //MUd Server Errors
        kNetErrServerTooBusy, kNetErrDbFail, kNetErrWhatIsThisIDontEven,
        kNetErrAintGotNone,
    }

    public struct ConnectHeader {
        public EConnType fType;
        public ushort fSockHeaderSize;
        public uint fBuildID;
        public NetCliBuildType fBuildType;
        public uint fBranchID;
        public Guid fProductID;

        public const int kSize = 31;

        public void Read(UruStream s) {
            fType = (EConnType)s.ReadByte();
            fSockHeaderSize = s.ReadUShort();
            fBuildID = s.ReadUInt();
            fBuildType = (NetCliBuildType)s.ReadUInt();
            fBranchID = s.ReadUInt();
            fProductID = new Guid(s.ReadBytes(16));
        }

        public void Write(UruStream s) {
            s.WriteByte((byte)fType);
            s.WriteUShort(fSockHeaderSize);
            s.WriteUInt(fBuildID);
            s.WriteUInt((uint)fBuildType);
            s.WriteUInt(fBranchID);
            s.WriteBytes(fProductID.ToByteArray());
        }
    }

    public enum NetCliConnectMsg {
        kNetCliConnect, kNetCliEncrypt, kNetCliError,
    }

    public enum NetCliBuildType {
        kDev  = 10,
        kQA   = 20,
        kTest = 30,
        kBeta = 40,
        kLive = 50,
    }

    public struct NetAgeInfo {
        public Guid fInstanceUuid;
        public string fFilename;     //Len   64
        public string fInstanceName; //Len   64
        public string fUserName;     //Len   64
        public string fDescription;  //Len 1024
        public uint fSequenceNumber;
        public int fLanguage;
        public uint fPopulation;
        public uint fCurrPopulation;

        public override bool Equals(object obj) {
            if (!(obj is NetAgeInfo)) 
                return false;

            NetAgeInfo cmp = (NetAgeInfo)obj;
            if (fFilename.Equals(cmp.fFilename))
                if (fInstanceUuid.Equals(cmp.fInstanceUuid))
                    return true;
            return false;
        }

        public void Read(UruStream s) {
            fInstanceUuid = new Guid(s.ReadBytes(16));
            fFilename = s.ReadUnicodeStringF(64);
            fInstanceName = s.ReadUnicodeStringF(64);
            fUserName = s.ReadUnicodeStringF(64);
            fDescription = s.ReadUnicodeStringF(1024);
            fSequenceNumber = s.ReadUInt();
            fLanguage = s.ReadInt();
            fPopulation = s.ReadUInt();
            fCurrPopulation = s.ReadUInt();
        }

        public void Write(UruStream s) {
            s.WriteBytes(fInstanceUuid.ToByteArray());
            s.WriteUnicodeStringF(fFilename, 64);
            s.WriteUnicodeStringF(fInstanceName, 64);
            s.WriteUnicodeStringF(fUserName, 64);
            s.WriteUnicodeStringF(fDescription, 1024);
            s.WriteUInt(fSequenceNumber);
            s.WriteInt(fLanguage);
            s.WriteUInt(fPopulation);
            s.WriteUInt(fCurrPopulation);
        }
    }
}
