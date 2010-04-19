using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace MUd {

    [XmlRoot("shards")]
    public class ShardList {

        [XmlElement("shard")]
        public Shard[] fShards;

        public static ShardList Create(Stream s) {
            StreamReader reader = new StreamReader(s);
            XmlSerializer serializer = new XmlSerializer(typeof(ShardList));
            return serializer.Deserialize(reader) as ShardList;
        }
    }

    public class Shard {

        [XmlElement("name")]
        public string fName;

        [XmlElement("auth")]
        public ServerEntry fAuth;

        [XmlElement("game")]
        public ServerEntry fGame;

        [XmlElement("gate")]
        public ServerEntry fGate;

        public override string ToString() {
            return fName;
        }
    }

    public class ServerEntry {

        [XmlElement("host")]
        public string fHost;

        [XmlElement("nkey")]
        public string fNKey;

        [XmlElement("xkey")]
        public string fXKey;

        public byte[] N {
            get { return IStringToByteArray(fNKey); }
        }

        public byte[] X {
            get { return IStringToByteArray(fXKey); }
        }

        private byte[] IStringToByteArray(string data) {
            if (data.Length % 2 != 0) throw new ArgumentException("String length is not a multiple of 2");
            byte[] result = new byte[data.Length / 2];
            for (int i = 0; i < data.Length / 2; i ++) {
                string temp = data.Substring(i * 2, 2);
                result[i] = Convert.ToByte(temp, 16);
            }

            return result;
        }
    }
}
