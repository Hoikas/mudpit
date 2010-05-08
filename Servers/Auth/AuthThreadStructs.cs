using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MUd {
    public partial class AuthThread {
        enum AcctPrivLevel {
            Banned = 0,
            Normal,
            Private,
            CCR,
            Admin,
        }

        struct AgeTag {
            public string fFilename;
            public Guid fInstance;

            public AgeTag(string name, Guid uuid) {
                fFilename = name;
                fInstance = uuid;
            }
        }

        struct Chunk {
            public byte[] fChunk;
            public uint fPos;

            public Chunk(byte[] chunk, uint pos) {
                fChunk = chunk;
                fPos = pos;
            }
        }
    }
}
