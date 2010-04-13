using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MUd {
    public class FileManifest {

        private List<FileManifestEntry> fEntries = new List<FileManifestEntry>();
        public List<FileManifestEntry> Files {
            get { return fEntries; }
        }

        private ENetError fResult = ENetError.kNetPending;
        public ENetError Result {
            get { return fResult; }
        }

        private LogProcessor fLog;

        public FileManifest(LogProcessor log) { fLog = log; }

        public void ReadFile(string file) {
            fLog.Debug(String.Format("MFS Parse Request \"{0}\"", file));
            if (File.Exists(file)) {
                FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read);
                StreamReader r = new StreamReader(fs);
                while (!r.EndOfStream) {
                    string o = r.ReadLine();
                    if (o.StartsWith("#")) continue;
                    if (o.Equals(String.Empty)) continue;

                    string[] line = o.Split(new char[] { ',' });
                    if (line.Length != 7) {
                        fLog.DumpToLog(o, "Malformed manifest line!", ELogType.kLogError);
                        fResult = ENetError.kNetErrInternalError;
                    }

                    FileManifestEntry e = new FileManifestEntry();
                    e.fFileName = line[0];
                    e.fDownloadName = line[1];
                    e.fHash = line[2];
                    e.fCompressedHash = line[3];
                    e.fFileSize = Convert.ToUInt32(line[4]);
                    e.fCompressedSize = Convert.ToUInt32(line[5]);
                    e.fFlags = Convert.ToUInt32(line[6]);

                    //We succeeded...
                    fEntries.Add(e);
                    fResult = ENetError.kNetSuccess;
                }

                r.Close();
                fs.Close();
            } else {
                fLog.Error(file + " <--- NOT FOUND");
                fResult = ENetError.kNetErrFileNotFound;
            }
        }

        public void Read(UruStream s) {
            while (true) {
                FileManifestEntry me = new FileManifestEntry();
                me.fFileName = s.ReadUnicodeString();
                if (me.fFileName == String.Empty) break; //The end of the manifest is an empty string

                me.fDownloadName = s.ReadUnicodeString();
                me.fHash = s.ReadUnicodeString();
                me.fCompressedHash = s.ReadUnicodeString();

                me.fFileSize = (uint)(s.ReadUShort() << 16 | s.ReadUShort() & 0xFFFF);
                s.ReadUShort(); //NULL

                me.fCompressedSize = (uint)(s.ReadUShort() << 16 | s.ReadUShort() & 0xFFFF);
                s.ReadUShort(); //NULL

                me.fFlags = (uint)(s.ReadUShort() << 16 | s.ReadUShort() & 0xFFFF);
                s.ReadUShort(); //NULL

                fEntries.Add(me);
            }
        }

        public byte[] ToByteArray() {
            MemoryStream ms = new MemoryStream();
            UruStream w = new UruStream(ms);

            foreach (FileManifestEntry e in fEntries) {
                //Filename
                w.WriteBytes(Encoding.Unicode.GetBytes(e.fFileName));
                w.WriteUShort((ushort)0);

                //Download
                w.WriteBytes(Encoding.Unicode.GetBytes(e.fDownloadName));
                w.WriteUShort((ushort)0);

                //Hash
                w.WriteBytes(Encoding.Unicode.GetBytes(e.fHash));
                w.WriteUShort((ushort)0);

                //Compressed Hash
                w.WriteBytes(Encoding.Unicode.GetBytes(e.fCompressedHash));
                w.WriteUShort((ushort)0);

                //File Size
                w.WriteUShort((ushort)(e.fFileSize >> 16));
                w.WriteUShort((ushort)(e.fFileSize & 0xFFFF));
                w.WriteUShort((ushort)0);

                //Compressed File Size
                w.WriteUShort((ushort)(e.fCompressedSize >> 16));
                w.WriteUShort((ushort)(e.fCompressedSize & 0xFFFF));
                w.WriteUShort((ushort)0);

                //Flags
                w.WriteUShort((ushort)(e.fFlags >> 16));
                w.WriteUShort((ushort)(e.fFlags & 0xFFFF));
                w.WriteUShort((ushort)0);
            }

            w.WriteUShort((ushort)0);

            byte[] rtn = ms.ToArray();
            w.Close();
            ms.Close();
            return rtn;
        }
    }

    public struct FileManifestEntry {

        public string fFileName;
        public string fDownloadName;
        public string fHash;
        public string fCompressedHash;
        public uint fFileSize;
        public uint fCompressedSize;
        public uint fFlags;
    }
}
