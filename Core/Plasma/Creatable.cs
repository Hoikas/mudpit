using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MUd {

    public enum CreatableID {
        SceneObject               = 0x0001,
        NetClientMgr              = 0x0052,
        AvatarMgr                 = 0x00F4,
        LoadCloneMsg              = 0x0253,
        NetMsgGameMessage         = 0x026B,
        NetMsgStream              = 0x026C,
        NetMsgGameMessageDirected = 0x032E,
        KIMsg                     = 0x0364,
        LoadAvatarMsg             = 0x03B1,
        NetMsgLoadClone           = 0x03B3,
        NetMsgPlayerPage          = 0x03B4,

        //Null Creatables do exist ^_^
        NULL                      = 0x8000,
    }

    public abstract class Creatable {

        public CreatableID ClassIndex {
            get {
                string debug = ToString().Substring(4);
                return (CreatableID)Enum.Parse(typeof(CreatableID), debug);
            }
        }

        public abstract void Read(UruStream s);
        public abstract void Write(UruStream s);
    }

    public static class Factory {
        public static Creatable Create(CreatableID pCre) {
            switch (pCre) {
                case CreatableID.NULL:
                    return null;
                default:
                    string name = Enum.GetName(typeof(CreatableID), pCre);
                    if (name == null)
                        name = "0x" + pCre.ToString("X"); //Numerical value
                    throw new NotSupportedException("Plasma Creatable: " + name);
            }
        }

        public static Creatable ReadCreatable(UruStream s) {
            CreatableID pCreID = (CreatableID)s.ReadUShort();
            Creatable pCre = Create(pCreID);
            if (pCre != null) pCre.Read(s);

            return pCre;
        }

        public static void WriteCreatable(UruStream s, Creatable pCre) {
            if (pCre == null) {
                s.WriteUShort((ushort)CreatableID.NULL);
            } else {
                s.WriteUShort((ushort)pCre.ClassIndex);
                pCre.Write(s);
            }
        }
    }
}
