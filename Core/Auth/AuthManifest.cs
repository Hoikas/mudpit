using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MUd {
    public class AuthManifest {

        public List<AuthFileEntry> fFiles = new List<AuthFileEntry>();

        public AuthManifest() { }
        public AuthManifest(string dir, string[] files) {
            foreach (string file in files) {
                AuthFileEntry afe = new AuthFileEntry();
                afe.fName = dir + "\\" + Path.GetFileName(file);
                afe.fSize = new FileInfo(file).Length;
                fFiles.Add(afe);
            }
        }

        public void Read(UruStream s) {
            while (true) {
                AuthFileEntry entry = new AuthFileEntry();
                entry.fName = s.ReadUnicodeString();
                if (entry.fName == String.Empty) break;
                entry.fSize = (long)(s.ReadUShort() << 16 | s.ReadUShort() & 0xFFFF);
                s.ReadUShort(); //NULL

                fFiles.Add(entry);
            }
        }

        public byte[] ToArray() {
            MemoryStream ms = new MemoryStream();
            UruStream s = new UruStream(ms);
            Write(s);

            byte[] buf = ms.ToArray();
            s.Close();
            ms.Close();

            return buf;
        }

        public void Write(UruStream s) {
            foreach (AuthFileEntry file in fFiles) {
                s.WriteBytes(Encoding.Unicode.GetBytes(file.fName));
                s.WriteUShort(0);

                s.WriteUShort((ushort)(file.fSize >> 16));
                s.WriteUShort((ushort)(file.fSize & 0xFFFF));
                s.WriteUShort(0);
            }

            s.WriteUShort(0);
        }
    }

    public struct AuthFileEntry {

        public string fName;
        public long fSize;

        public AuthFileEntry(string name, long size) {
            fName = name;
            fSize = size;
        }
    }
}
