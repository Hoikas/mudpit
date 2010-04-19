using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MUd.Newport {
    class Program {
        static void Main(string[] args) {
            Dispatch d = new Dispatch(true, true, true, true, true);
            d.Run();
        }
    }
}
