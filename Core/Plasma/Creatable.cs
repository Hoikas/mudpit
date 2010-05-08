using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MUd {

    public enum CreatableID {
    }

    public abstract class Creatable {
        public abstract void Read(UruStream s);
        public abstract void Write(UruStream s);

        public static Creatable Create(CreatableID pCre) {
            switch (pCre) {
                default:
                    string name = Enum.GetName(typeof(CreatableID), pCre);
                    if (name == null)
                        name = "0x" + pCre.ToString("X"); //Numerical value
                    throw new NotSupportedException("Plasma Creatable: " + name);
            }
        }
    }
}
