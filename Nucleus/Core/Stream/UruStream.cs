using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MUd {
    public class UruStream {

        private Stream fBaseStream;
        private BinaryReader fReader;
        private BinaryWriter fWriter;
        private bool fBuffering = false;

        public Stream BaseStream {
            get { return fBaseStream; }
        }

        public UruStream(Stream s) {
            fBaseStream = s;
            fReader = new BinaryReader(s);
            fWriter = new BinaryWriter(s);
        }

        public static UruStream CreateBuffer() {
            return new UruStream(new MemoryStream());
        }

        public void BufferWriter() {
            if (!fBuffering) {
                MemoryStream ms = new MemoryStream();
                fWriter = new BinaryWriter(ms);
                fBuffering = true;
            }
        }

        public void Close() {
            fReader.Close();
            fWriter.Close();
            fBaseStream.Close();
        }

        public void FlushWriter() {
            if (fBuffering) {
                ((MemoryStream)fWriter.BaseStream).WriteTo(fBaseStream);
                fWriter.Close();

                fWriter = new BinaryWriter(fBaseStream);
                fBuffering = false;
            }
        }

        public MemoryStream AsMemoryStream() {
            return fBaseStream as MemoryStream;
        }

        public bool ReadBool() {
            return fReader.ReadBoolean();
        }

        public UruStream ReadBuffer(int size) {
            MemoryStream ms = new MemoryStream(ReadBytes(size));
            UruStream s = new UruStream(ms);
            return s;
        }

        public byte ReadByte() {
            return fReader.ReadByte();
        }

        public byte[] ReadBytes(int count) {
            if (count == 0) return new byte[0];
            return fReader.ReadBytes(count);
        }

        public int ReadInt() {
            return fReader.ReadInt32();
        }

        public ushort ReadUShort() {
            return fReader.ReadUInt16();
        }

        public uint ReadUInt() {
            return fReader.ReadUInt32();
        }

        public ulong ReadULong() {
            return fReader.ReadUInt64();
        }

        public string ReadUnicodeString() {
            string str = String.Empty;
            while (true) {
                byte[] data = fReader.ReadBytes(2);
                if (data[0] == 0 && data[1] == 0) break;
                str += Encoding.Unicode.GetString(data);
            }

            return str;
        }

        /// <summary>
        /// Reads in a UTF16 string of specified size.
        /// </summary>
        /// <param name="maxsize">Maximum number of WIDE Characters</param>
        /// <returns>String Data</returns>
        public string ReadUnicodeStringF(int maxsize) {
            string data = Encoding.Unicode.GetString(fReader.ReadBytes(maxsize * 2));
            return data.Split(new string[] { "\0" }, StringSplitOptions.None)[0];
        }

        public string ReadUnicodeStringV16(int maxsize) {
            int size = (int)fReader.ReadInt16();
            if (size > maxsize) size = maxsize;
            if (size == 0) return String.Empty;
            return Encoding.Unicode.GetString(fReader.ReadBytes(size * 2));
        }

        public string ReadUnicodeStringV32() {
            byte[] data = fReader.ReadBytes(fReader.ReadInt32());
            return Encoding.Unicode.GetString(data).Replace("\0", null);
        }

        public void WriteBool(bool data) {
            fWriter.Write(data);
        }

        public void WriteByte(byte data) {
            fWriter.Write(data);
        }

        public void WriteBytes(byte[] data) {
            if (data == null) return;
            if (data.Length == 0) return;
            fWriter.Write(data);
        }

        public void WriteInt(int data) {
            fWriter.Write(data);
        }

        public void WriteUInt(uint data) {
            fWriter.Write(data);
        }

        public void WriteULong(ulong data) {
            fWriter.Write(data);
        }

        public void WriteUShort(ushort data) {
            fWriter.Write(data);
        }

        public void WriteUnicodeStringV16(string data, int maxsize) {
            if (data == null || data == String.Empty) {
                fWriter.Write((short)0);
                return;
            }

            byte[] buf = Encoding.Unicode.GetBytes(data);
            if (buf.Length > maxsize * 2) { //Truncate
                byte[] old = buf;
                Buffer.BlockCopy(old, 0, buf, 0, maxsize * 2);
            }

            fWriter.Write((short)(buf.Length / 2));
            fWriter.Write(buf);
        }

        public void WriteUnicodeStringV32(string data) {
            byte[] str = Encoding.Unicode.GetBytes(data);
            fWriter.Write(str.Length + 2);
            fWriter.Write(str);
            fWriter.Write((ushort)0);
        }
    }
}
