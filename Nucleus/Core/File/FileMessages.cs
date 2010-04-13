using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MUd {
    public enum FileCli2Srv {
        PingRequest = 0,
        BuildIdRequest = 10,
        ManifestRequest = 20,
        FileDownloadRequest = 21,
        ManifestEntryAck = 22,
        FileDownloadChunkAck = 23,
    }

    public enum FileSrv2Cli {
        PingReply = 0,
        BuildIdReply = 10,
        BuildIdUpdate = 11,
        ManifestReply = 20,
        FileDownloadReply = 21,
    }

    public struct File_AckData {
        public uint fTransID;
        public uint fReaderID;

        public void Read(UruStream s) {
            fTransID = s.ReadUInt();
            fReaderID = s.ReadUInt();
        }
    }

    public struct File_BuildIdReply {
        public uint fTransID;
        public ENetError fResult;
        public uint fBuildID;

        public void Read(UruStream s) {
            fTransID = s.ReadUInt();
            fResult = (ENetError)s.ReadInt();
            fBuildID = s.ReadUInt();
        }

        public void Write(UruStream s) {
            s.WriteUInt(fTransID);
            s.WriteInt((int)fResult);
            s.WriteUInt(fBuildID);
        }
    }

    public struct File_BuildIdRequest {
        public uint fTransID;

        public void Read(UruStream s) {
            fTransID = s.ReadUInt();
        }

        public void Write(UruStream s) {
            s.WriteUInt(fTransID);
        }
    }

    public struct File_DownloadReply {
        public uint fTransID;
        public ENetError fResult;
        public uint fReaderID;
        public uint fFileSize;
        public byte[] fData;

        public void Write(UruStream s) {
            if (fData == null) fData = new byte[0];
            s.WriteUInt(fTransID);
            s.WriteUInt((uint)fResult);
            s.WriteUInt(fReaderID);
            s.WriteUInt(fFileSize);
            s.WriteInt(fData.Length);
            s.WriteBytes(fData);
        }
    }

    public struct File_DownloadRequest {
        public uint fTransID;
        public string fFilename;
        public uint fBuildID;

        public void Read(UruStream s) {
            fTransID = s.ReadUInt();
            fFilename = s.ReadUnicodeStringF(260);
            fBuildID = s.ReadUInt();
        }
    }

    public struct File_ManifestReply {
        public uint fTransID;
        public ENetError fResult;
        public uint fReaderID;
        private int fNumFiles;
        private byte[] fData;

        public FileManifest Manifest {
            get { throw new NotImplementedException(); }
            set {
                //Some fake initializers.
                fNumFiles = 0;
                fData = new byte[0];

                //Sanity...
                if (value == null) {
                    fResult = ENetError.kNetErrInternalError;
                    return;
                }

                //Fill in
                fResult = value.Result;
                fNumFiles = value.Files.Count;
                fData = value.ToByteArray();
            }
        }

        public void Write(UruStream s) {
            s.WriteUInt(fTransID);
            s.WriteUInt((uint)fResult);
            s.WriteUInt(fReaderID);
            s.WriteInt(fNumFiles);
            s.WriteInt(fData.Length / 2); //Wide char buffer...
            s.WriteBytes(fData);
        }
    }

    public struct File_ManifestRequest {
        public uint fTransID;
        public string fGroup;
        public uint fBuildID;

        public void Read(UruStream s) {
            fTransID = s.ReadUInt();
            fGroup = s.ReadUnicodeStringF(260);
            fBuildID = s.ReadUInt();
        }
    }

    public struct File_PingPong {
        public int fPingTime;

        public void Read(UruStream s) {
            fPingTime = s.ReadInt();
        }

        public void Write(UruStream s) {
            s.WriteInt(fPingTime);
        }
    }
}
