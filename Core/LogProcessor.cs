﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace MUd {
    public class LogProcessor {

        private static string fLogDir = "log";
        public static string Destination {
            get { return fLogDir; }
            set { fLogDir = value; }
        }

        public bool IsEmpty {
            get { return (fWriter == null); }
        }

        private StreamWriter fWriter;
        private string fLocation;
        private ELogType fMinLevel;

        public ELogType LogLevel {
            get { return fMinLevel; }
            set { fMinLevel = value; }
        }

        private string fLogNow {
            get { return DateTime.UtcNow.ToString("dd-MM-yyyy HH:mm:ss"); }
        }

        private string fFolderMonth {
            get { return DateTime.UtcNow.ToString("MMM yyyy"); }
        }

        public LogProcessor(string name) {
            string path = Path.Combine("log", fFolderMonth);
            Directory.CreateDirectory(path);
            fLocation = Path.Combine(path, name);
        }

        public LogProcessor(string name, string group) {
            string path = Path.Combine(Path.Combine("log", fFolderMonth), group);
            Directory.CreateDirectory(path);
            fLocation = Path.Combine(path, name);

            //Allow us to set both kLogLeveXXX and xxx as log levels...
            //This is mostly so configuration files will look nicer :)
            string loglev = Configuration.GetString("log_level", "kLogWarning");
            if (Enum.IsDefined(typeof(ELogType), loglev))
                fMinLevel = (ELogType)Enum.Parse(typeof(ELogType), loglev);
            else if (loglev.ToLower() == "error")
                fMinLevel = ELogType.kLogError;
            else if (loglev.ToLower() == "warn")
                fMinLevel = ELogType.kLogWarning;
            else if (loglev.ToLower() == "info")
                fMinLevel = ELogType.kLogInfo;
            else if (loglev.ToLower() == "debug")
                fMinLevel = ELogType.kLogDebug;
            else if (loglev.ToLower() == "verbose")
                fMinLevel = ELogType.kLogVerbose;
            else {
                fMinLevel = ELogType.kLogWarning;
                Error(String.Format("MUd.conf has invalid log_level \"{0}\"", loglev));
            }
        }

        private void IInitLog() {
            if (fWriter != null) return;

            //Rotate logs
            for (int i = 3; i > -1; i--) {
                string name = fLocation + "." + i.ToString() + ".log";
                if (File.Exists(name)) {
                    if (i == 3) File.Delete(name);
                    else File.Move(name, fLocation + "." + (i + 1).ToString() + ".log");
                }
            }

            //Oh hai!
            fWriter = new StreamWriter(fLocation.Replace(':', '.') + ".0.log");
            lock (fWriter) {
                fWriter.WriteLine("MUd: MYST Uru Daemon");
                fWriter.WriteLine("By using this program, you are violating your EULA with CYAN Worlds, Inc.");
                fWriter.WriteLine("The MUd Development Team is not responsible for damages incurred by the use of the program.");
                fWriter.WriteLine();
                fWriter.WriteLine();
                fWriter.Flush();
            }
        }

        private void IWriteLine(string line) {
            IInitLog();
            lock (fWriter) {
                fWriter.WriteLine("[" + fLogNow + "] " + line);
                fWriter.Flush();
            }
        }

        public void DumpToFile(byte[] dump) {
            string dir = Path.Combine("log", "dumps");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            string filename = Path.GetRandomFileName();
            IWriteLine("DUMP: " + filename);

            FileStream s = new FileStream(Path.Combine(dir, filename), FileMode.Create, FileAccess.Write);
            s.Write(dump, 0, dump.Length);
            s.Close();
        }

        public void DumpToLog(string dump) {
            IWriteLine("------BEGIN DUMP------");
            Monitor.Enter(fWriter);
            fWriter.WriteLine(dump);
            Monitor.Exit(fWriter);
            IWriteLine("-------END DUMP-------");
            Monitor.Enter(fWriter);
            fWriter.Flush();
            Monitor.Exit(fWriter);
        }

        public void DumpToLog(string dump, ELogType level) {
            if (level <= fMinLevel) DumpToLog(dump);
        }

        public void DumpToLog(string dump, string msg, ELogType level) {
            if (level <= fMinLevel) {
                WriteLine(msg, level);
                DumpToLog(dump);
            }
        }

        public void DumpToLog(byte[] dump, string msg, ELogType level) {
            if (level <= fMinLevel) {
                WriteLine(msg, level);
                DumpToLog(BitConverter.ToString(dump).Replace('-', ' '));
            }
        }

        public void WriteLine(string line) {
            IWriteLine(" UNKNOWN: " + line);
        }

        public void WriteLine(string line, ELogType level) {
            if (level <= fMinLevel) {
                string levelstr = "[UNK] ";
                switch (level) {
                    case ELogType.kLogDebug:
                        levelstr = "[DBG] ";
                        break;
                    case ELogType.kLogError:
                        levelstr = "[ERR] ";
                        break;
                    case ELogType.kLogInfo:
                        levelstr = "[INF] ";
                        break;
                    case ELogType.kLogVerbose:
                        levelstr = "[VBS] ";
                        break;
                    case ELogType.kLogWarning:
                        levelstr = "[WRN] ";
                        break;
                }

                IWriteLine(levelstr + line);
            }
        }

        private void IFlush() {
            Monitor.Enter(fWriter);
            fWriter.Flush();
            Monitor.Exit(fWriter);
        }

        public void Close() {
            if (fWriter != null) {
                Monitor.Enter(fWriter);
                fWriter.Close();
                Monitor.Exit(fWriter);

                fWriter = null;
            }
        }

        #region Easy Log Functions
        public void Debug(string msg) { WriteLine(msg, ELogType.kLogDebug); }
        public void Error(string msg) { WriteLine(msg, ELogType.kLogError); }
        public void Info(string msg) { WriteLine(msg, ELogType.kLogInfo); }
        public void Verbose(string msg) { WriteLine(msg, ELogType.kLogVerbose); }
        public void Warn(string msg) { WriteLine(msg, ELogType.kLogWarning); }
        #endregion
    }

    public enum ELogType {
        kLogError,
        kLogWarning,
        kLogInfo,
        kLogDebug,
        kLogVerbose,
    }
}
