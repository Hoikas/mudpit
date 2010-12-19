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

        public long FlushWriter() {
            long len = 0;

            if (fBuffering) {
                ((MemoryStream)fWriter.BaseStream).WriteTo(fBaseStream);
                len = fWriter.BaseStream.Length;
                fWriter.Close();

                fWriter = new BinaryWriter(fBaseStream);
                fBuffering = false;
            }

            return len;
        }

        public bool ReadBool() {
            return fReader.ReadBoolean();
        }

        public byte ReadByte() {
            return fReader.ReadByte();
        }

        public byte[] ReadBytes(int count) {
            if (count == 0) return new byte[0];
            return fReader.ReadBytes(count);
        }

        public double ReadDouble() {
            return fReader.ReadDouble();
        }

        public string ReadSafeString() {
            short info = fReader.ReadInt16();
            if ((info & 0xF000) == 0) fReader.ReadInt16(); //Garbage

            int size = (info & 0x0FFF);
            if (size > 0) {
                byte[] data = ReadBytes(size);
                if ((data[0] & 0x80) != 0)
                    for (int i = 0; i < size; i++)
                        data[i] = (byte)(~data[i]);
                return Encoding.UTF8.GetString(data);
            } else return String.Empty;
            
        }

        public string ReadSafeWString() {
            int size = (int)(fReader.ReadInt16() & 0x0FFF);
            byte[] data = ReadBytes(size * 2);
            if ((data[0] & 0x80) != 0)
                for (int i = 0; i < data.Length; i++)
                    data[i] = (byte)(~data[i]);
            return Encoding.Unicode.GetString(data);
        }

        public short ReadShort() {
            return fReader.ReadInt16();
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

        public void WriteDouble(double data) {
            fWriter.Write(data);
        }

        public void WriteInt(int data) {
            fWriter.Write(data);
        }

        public void WriteSafeString(string data) {
            if (data == null)
                data = String.Empty;
            
            byte[] buf = Encoding.UTF8.GetBytes(data);
            short info = (short)(buf.Length & 0x0FFF);
            fWriter.Write((short)(info | 0xF000));

            byte[] str = new byte[info];
            for (int i = 0; i < info; i++)
                str[i] = (byte)(~buf[i]);
            WriteBytes(str);
        }

        public void WriteSafeWString(string data) {
            if (data == null)
                data = String.Empty;

            byte[] buf = Encoding.Unicode.GetBytes(data);
            short info = (short)((buf.Length / 2) | 0xF000);
            for (int i = 0; i < buf.Length; i++)
                buf[i] = (byte)(~buf[i]);
            WriteBytes(buf);
            fWriter.Write((short)0);
        }

        public void WriteShort(short data) {
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

        public void WriteUnicodeStringF(string data, int size) {
            if (data.Length > size)
                data = data.Remove(size - 1);

            fWriter.Write(Encoding.Unicode.GetBytes(data));
            if (data.Length < size)
                for (int i = 0; i < (size - data.Length); i++)
                    fWriter.Write((ushort)0);
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
