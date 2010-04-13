using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace MUd {
    public class FileThread : Srv2CliBase {

        private FileServer fParent;
        private uint fBuildID;
        private uint fNextReader = 0;
        private UruStream fStream;

        public FileThread(FileServer parent, Socket s, ConnectHeader hdr, LogProcessor log) : base(s, hdr, log) {
            fParent = parent;
        }

        public override void Start() {
            fStream = new UruStream(new NetworkStream(fSocket, false));

            //Eat the connection header
            fStream.ReadUInt(); //Header Size
            fBuildID = fStream.ReadUInt();
            fStream.ReadUInt(); //Server Type

            //Register for Async Receive
            fSocket.BeginReceive(new byte[4], 0, 4, SocketFlags.None, new AsyncCallback(IFactorizeMessage), null);
        }

        private void IFactorizeMessage(IAsyncResult ar) {
            try {
                fSocket.EndReceive(ar);

                //Attempt to read a message header...
                //If we get an IOException, the client DC'ed.
                //Note: We don't check the msg sizes because M$ is gay.
                //      NetworkStream.Position and NetworkStream.Length == EXCEPTION
                //      Futher Note: As of Feb. 14, 2010 we do things "ASYNC" (but not really), so we just eat the size
                FileCli2Srv msg = FileCli2Srv.PingRequest;
                try {
                    msg = (FileCli2Srv)fStream.ReadInt();
                } catch (IOException) {
                    Verbose("Disconnected");
                }

                //Factorize
                switch (msg) {
                    case FileCli2Srv.FileDownloadChunkAck:
                        IAckChunk();
                        break;
                    case FileCli2Srv.FileDownloadRequest:
                        ISendFile();
                        break;
                    case FileCli2Srv.ManifestEntryAck:
                        IAckManifest();
                        break;
                    case FileCli2Srv.ManifestRequest:
                        ISendManifest();
                        break;
                    case FileCli2Srv.PingRequest:
                        IPingPong();
                        break;
                    default:
                        Warn(String.Format("Received unimplemented message {0}", msg.ToString()));
                        break;
                }

                fSocket.BeginReceive(new byte[4], 0, 4, SocketFlags.None, new AsyncCallback(IFactorizeMessage), null);
            } catch (SocketException e) {
                IHandleSocketException(e);
            } catch (IOException) {
                Debug("Disconnected");
                Stop();
            }
        }

        public override void Stop() {
            if (fStream != null) fStream.Close();
            fSocket.Close();
            fParent.Remove(this);
        }

        private uint IGetNextReader() {
            uint r = fNextReader;
            fNextReader += 1;
            return r;
        }

        #region File Message Handlers
        private void IAckChunk() {
            File_AckData ack = new File_AckData();
            ack.Read(fStream);

            //TODO: Implement something awesome to ensure all chunks are acked.
        }

        private void IAckManifest() {
            File_AckData ack = new File_AckData();
            ack.Read(fStream);

            //TODO: Implement something awesome to ensure all MFS are acked.
        }

        private void IPingPong() {
            //Just read the ping data and bounce it back to the client...
            //Pretty silly how many pings Cyan sends...
            File_PingPong ping = new File_PingPong();
            ping.Read(fStream);

            //Silly, hateful header.
            fStream.WriteInt(12); //int x3
            fStream.WriteInt((int)FileSrv2Cli.PingReply);
            ping.Write(fStream);
        }

        private void ISendFile() {
            File_DownloadRequest req = new File_DownloadRequest();
            req.Read(fStream);

            //Security
            req.fFilename = req.fFilename.Replace("../", null);

            string path = Path.Combine(Configuration.GetString("mfs_source", "G:\\Plasma\\Servers\\Manifest Data"), req.fFilename);
            if (File.Exists(path)) {
                //Open the file and grab the max chunk size.
                int bufsize = Configuration.GetInteger("chunk_size", 16384); //Default = 16KB
                FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                Verbose(String.Format("Sending {0} in {1}KB chunks", req.fFilename, bufsize / 1024));

                int rem = (int)(fs.Length - fs.Position);
                while (fs.Length > fs.Position) {
                    //Alloc the buffer
                    byte[] buf = null;
                    if (rem > bufsize) buf = new byte[bufsize];
                    else buf = new byte[rem];

                    //Read in the file data
                    fs.Read(buf, 0, buf.Length);

                    //Prepare the reply
                    File_DownloadReply reply = new File_DownloadReply();
                    reply.fData = buf;
                    reply.fFileSize = (uint)fs.Length;
                    reply.fReaderID = IGetNextReader();
                    reply.fResult = ENetError.kNetSuccess;
                    reply.fTransID = req.fTransID;

                    //Hateful buffering.
                    UruStream temp = new UruStream(new MemoryStream());
                    reply.Write(temp);

                    //Actually send the response
                    fStream.WriteInt((int)(8 + temp.BaseStream.Length));
                    fStream.WriteInt((int)FileSrv2Cli.FileDownloadReply);
                    fStream.WriteBytes(((MemoryStream)temp.BaseStream).ToArray());

                    //Sigh x(INTEGER_OVERFLOW)
                    temp.Close();
                }

                //Clean up, clean up...
                fs.Close();
            } else {
                Error(String.Format("The requested file [{0}] does not exist!", req.fFilename));

                //Create a FAIL
                File_DownloadReply reply = new File_DownloadReply();
                reply.fFileSize = 0;
                reply.fReaderID = IGetNextReader();
                reply.fResult = ENetError.kNetErrFileNotFound;
                reply.fTransID = req.fTransID;

                //Hateful buffering.
                UruStream temp = new UruStream(new MemoryStream());
                reply.Write(temp);

                //Actually send the response
                fStream.WriteInt((int)(8 + temp.BaseStream.Length));
                fStream.WriteInt((int)FileSrv2Cli.FileDownloadReply);
                fStream.WriteBytes(((MemoryStream)temp.BaseStream).ToArray());

                //Sigh x(INTEGER_OVERFLOW)
                temp.Close();
            }
        }

        private void ISendManifest() {
            File_ManifestRequest req = new File_ManifestRequest();
            req.Read(fStream);

            //Create the response
            File_ManifestReply reply = new File_ManifestReply();
            reply.fTransID = req.fTransID;
            reply.fReaderID = IGetNextReader();

            //Read in the manifest and throw it into the response
            FileManifest mfs = new FileManifest(fLog);
            mfs.ReadFile(Path.Combine(Configuration.GetString("mfs_source", "G:\\Plasm\\Servers\\Manifest Data"), req.fGroup + ".mfs"));
            reply.Manifest = mfs;

            //We'll stuff the output into a holding pen to size it.
            //Stupid file header. I don't care about the size >.>
            UruStream buf = new UruStream(new MemoryStream());
            reply.Write(buf);

            //Now spit out some garbage...
            fStream.WriteInt((int)(buf.BaseStream.Length + 8));
            fStream.WriteInt((int)FileSrv2Cli.ManifestReply);
            fStream.WriteBytes(((MemoryStream)buf.BaseStream).ToArray());

            //Clean up.
            buf.Close();

            //If they request an invalid manifest, the patcher will just sit there forever,
            //So, let's close this up...
            if (reply.fResult != ENetError.kNetSuccess)
                Stop();
        }
        #endregion
    }
}
