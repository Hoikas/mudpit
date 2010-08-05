using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MUd {
    public class UnifiedTime {

        private uint fSeconds;
        private uint fMicroSecs;

        public bool AtEpoch {
            get { return (DateTime == Epoch); }
        }

        public DateTime DateTime {
            get {
                DateTime dt = Epoch.AddSeconds(Convert.ToDouble(fSeconds));
                dt.AddTicks(Convert.ToInt64(fMicroSecs / 100));
                return dt;
            }

            set {
                TimeSpan ts = (value - Epoch);
                fSeconds = Convert.ToUInt32(ts.TotalSeconds);
                //Too lazy to use microseconds. Maybe later.
            }
        }

        private DateTime Epoch {
            get { return new DateTime(1970, 1, 1, 0, 0, 0); }
        }

        public UnifiedTime() { this.DateTime = Epoch; }
        public UnifiedTime(DateTime dt) { this.DateTime = dt; }
        public UnifiedTime(UruStream s) { Read(s); }

        public void Read(UruStream bs) {
            fSeconds = bs.ReadUInt();
            fMicroSecs = bs.ReadUInt();
        }

        public void Write(UruStream bs) {
            bs.WriteUInt(fSeconds);
            bs.WriteUInt(fMicroSecs);
        }
    }
}
