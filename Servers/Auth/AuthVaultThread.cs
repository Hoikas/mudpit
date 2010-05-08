using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Timers;
using OpenSSL;

namespace MUd {
    public partial class AuthThread : Srv2CliBase {

        Dictionary<uint, uint> fVaultToAuthMap = new Dictionary<uint, uint>();
        Dictionary<uint, object> fVaultTransTags = new Dictionary<uint, object>();

        private object IVaultPopTag(uint transID) {
            lock (fVaultTransTags) {
                if (fVaultTransTags.ContainsKey(transID)) {
                    object tag = fVaultTransTags[transID];
                    fVaultTransTags.Remove(transID);
                    return tag;
                } else
                    return null;
            }
        }

        private uint IVaultPopTransID(uint transID) {
            lock (fVaultToAuthMap) {
                uint authTrans = fVaultToAuthMap[transID];
                fVaultToAuthMap.Remove(transID);
                return authTrans;
            }
        }

        private void IVaultOnAgeCreated(uint transID, ENetError result, uint ageID, uint infoID) {
            Auth_InitAgeReply reply = new Auth_InitAgeReply();
            reply.fAgeNodeID = ageID;
            reply.fInfoNodeID = infoID;
            reply.fResult = result;
            reply.fTransID = IVaultPopTransID(transID);

            lock (fStream) {
                fStream.BufferWriter();
                fStream.WriteUShort((ushort)AuthSrv2Cli.VaultInitAgeReply);
                reply.Write(fStream);
                fStream.FlushWriter();
            }
        }

        private void IVaultOnNodeAdded(uint parentID, uint childID, uint saverID) {
            Auth_VaultNodeAdded notify = new Auth_VaultNodeAdded();
            notify.fChildID = childID;
            notify.fParentID = parentID;
            notify.fSaverID = saverID;

            lock (fStream) {
                fStream.BufferWriter();
                fStream.WriteUShort((ushort)AuthSrv2Cli.VaultNodeAdded);
                notify.Write(fStream);
                fStream.FlushWriter();
            }
        }

        private void IVaultOnNodeAddReply(uint transID, ENetError result) {
            Auth_VaultNodeAddReply reply = new Auth_VaultNodeAddReply();
            reply.fResult = result;
            reply.fTransID = IVaultPopTransID(transID);

            lock (fStream) {
                fStream.BufferWriter();
                fStream.WriteUShort((ushort)AuthSrv2Cli.VaultAddNodeReply);
                reply.Write(fStream);
                fStream.FlushWriter();
            }
        }

        private void IVaultOnNodeChanged(uint nodeID, Guid revID) {
            Auth_VaultNodeChanged notify = new Auth_VaultNodeChanged();
            notify.fNodeID = nodeID;
            notify.fRevisionUuid = revID;

            lock (fStream) {
                fStream.BufferWriter();
                fStream.WriteUShort((ushort)AuthSrv2Cli.VaultNodeChanged);
                notify.Write(fStream);
                fStream.FlushWriter();
            }
        }

        private void IVaultOnNodeCreated(uint transID, ENetError result, uint nodeID) {
            Auth_VaultNodeCreated reply = new Auth_VaultNodeCreated();
            reply.fNodeID = nodeID;
            reply.fResult = result;
            reply.fTransID = IVaultPopTransID(transID);

            lock (fStream) {
                fStream.BufferWriter();
                fStream.WriteUShort((ushort)AuthSrv2Cli.VaultNodeCreated);
                reply.Write(fStream);
                fStream.FlushWriter();
            }
        }

        private void IVaultOnNodeFetched(uint transID, ENetError result, byte[] nodeData) {
            Auth_VaultNodeFetched reply = new Auth_VaultNodeFetched();
            reply.fNodeData = nodeData;
            reply.fResult = result;
            reply.fTransID = IVaultPopTransID(transID);

            lock (fStream) {
                fStream.BufferWriter();
                fStream.WriteUShort((ushort)AuthSrv2Cli.VaultNodeFetched);
                reply.Write(fStream);
                fStream.FlushWriter();
            }
        }

        private void IVaultOnNodeFound(uint transID, ENetError result, uint[] nodes) {
            object tag = IVaultPopTag(transID);

            //What kind of notification is this?
            //  TAG == null: Normal NodeFind from the client.
            //  TAG is AgeTag, then we're finding an age.
            if (tag == null) {
                Auth_VaultNodeFindReply reply = new Auth_VaultNodeFindReply();
                reply.fNodeIDs = nodes;
                reply.fResult = result;
                reply.fTransID = IVaultPopTransID(transID);

                lock (fStream) {
                    fStream.BufferWriter();
                    fStream.WriteUShort((ushort)AuthSrv2Cli.VaultNodeFindReply);
                    reply.Write(fStream);
                    fStream.FlushWriter();
                }
            } else if (tag is AgeTag) {
                //   ---Find Age Process---
                //Step #2: Ask the LookupServer for a GameServer
                
                AgeTag age = (AgeTag)tag;
                if (nodes.Length > 1)
                    Warn(String.Format("Multiple AgeNodes found! Choosing first found. [AGE: {0}] [UUID: {1}]", age.fFilename, age.fInstance));
                
                //None found? O.o
                ENetError err = ENetError.kNetPending;
                if (nodes.Length == 0) {
                    Error(String.Format("Zero AgeNodes found! [AGE: {0}] [UUID: {1}]", age.fFilename, age.fInstance));
                    err = ENetError.kNetErrAgeNotFound;
                } else {
                    if (!fLookupCli.Connected) {
                        if (!fLookupCli.Connect()) {
                            err = ENetError.kNetErrInternalError;
                            Error("Could not connect to LookupSrv!");
                        } else {
                            err = ENetError.kNetSuccess;
                            Verbose("Connected to LookupSrv");
                        }
                    } else
                        err = ENetError.kNetSuccess;
                }

                if (err == ENetError.kNetSuccess) {
                    //Send off the FindAgeReq
                    uint trans = fLookupCli.FindAge(age.fFilename, age.fInstance, nodes[0]);
                    lock (fLookupToAuthMap)
                        fLookupToAuthMap.Add(trans, IVaultPopTransID(transID));
                } else {
                    Auth_AgeReply reply = new Auth_AgeReply();
                    reply.fResult = err;
                    reply.fTransID = IVaultPopTransID(transID);

                    fStream.BufferWriter();
                    fStream.WriteUShort((ushort)AuthSrv2Cli.AgeReply);
                    reply.Write(fStream);
                    fStream.FlushWriter();
                }
            }
        }

        private void IVaultOnNodeRefsFetched(uint transID, ENetError result, VaultNodeRef[] refs) {
            Auth_VaultNodeRefsFetched reply = new Auth_VaultNodeRefsFetched();
            reply.fRefs = refs;
            reply.fResult = result;
            reply.fTransID = IVaultPopTransID(transID);

            lock (fStream) {
                fStream.BufferWriter();
                fStream.WriteUShort((ushort)AuthSrv2Cli.VaultNodeRefsFetched);
                reply.Write(fStream);
                fStream.FlushWriter();
            }
        }

        private void IVaultOnNodeSaved(uint transID, ENetError result) {
            Auth_VaultNodeSaveReply reply = new Auth_VaultNodeSaveReply();
            reply.fResult = result;
            reply.fTransID = IVaultPopTransID(transID);

            lock (fStream) {
                fStream.BufferWriter();
                fStream.WriteUShort((ushort)AuthSrv2Cli.VaultSaveNodeReply);
                reply.Write(fStream);
                fStream.FlushWriter();
            }
        }

        private void IVaultOnPlayerCreate(uint transID, uint playerID, string playerName, string model) {
            Auth_PlayerCreateReply reply = new Auth_PlayerCreateReply();
            reply.fExplorer = 1;
            reply.fModel = model;
            reply.fName = playerName;
            reply.fPlayerID = playerID;
            reply.fResult = ENetError.kNetSuccess;
            reply.fTransID = IVaultPopTransID(transID);

            lock (fStream) {
                InsertStatement ins = new InsertStatement();
                ins.Insert.Add("NodeIdx", reply.fPlayerID.ToString());
                ins.Insert.Add("Name", reply.fName);
                ins.Insert.Add("Model", reply.fModel);
                ins.Insert.Add("AcctUUID", fAcctUUID.ToString().ToLower());
                ins.Table = "Players";
                ins.ExcecuteNonQuery(fDB);

                fPlayers.Add(reply.fPlayerID);

                fStream.BufferWriter();
                fStream.WriteUShort((ushort)AuthSrv2Cli.PlayerCreateReply);
                reply.Write(fStream);
                fStream.FlushWriter();
            }
        }

        private void IVaultOnPong(uint transID, uint pingTime, byte[] payload) {
            Verbose("[VaultCli] PONG!");
        }
    }
}
