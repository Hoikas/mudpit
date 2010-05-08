using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MUd {
    public class UnifiedTime {

        DateTime fTime = DateTime.UtcNow;
        public DateTime Value {
            get { return fTime; }
            set { fTime = value; }
        }

        public void Read(UruStream s) {
            uint secs = s.ReadUInt();
            uint micros = s.ReadUInt();

            //secs == Seconds since January 1, 1970
            //microseconds == microseconds to add to secs
            //    To convert, to DateTime, we must add to the Unix epoch
            //    Micros need to be converted to milliseconds
            //    Note: Use double type to try to preserve data until we must finally round
            DateTime epoch = new DateTime(1970, 1, 1);
            TimeSpan ts = new TimeSpan((Convert.ToInt64(secs) * TimeSpan.TicksPerSecond) + 
                                       Convert.ToInt64((Convert.ToDouble(micros) / 1000.0) * Convert.ToDouble(TimeSpan.TicksPerMillisecond)));

            //Now create the time
            fTime = epoch.Add(ts);
        }

        public void Write(UruStream s) {
            TimeSpan ts = fTime.Subtract(new DateTime(1970, 1, 1));

            //Easy enough :)
            s.WriteUInt(Convert.ToUInt32(ts.Seconds));
            s.WriteUInt(Convert.ToUInt32(ts.Milliseconds * 1000)); 
        }
    }
}
