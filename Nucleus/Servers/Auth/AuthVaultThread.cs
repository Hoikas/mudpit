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

        private void IVaultOnAgeCreated(uint transID, ENetError result, uint ageID, uint infoID) {
            Auth_InitAgeReply reply = new Auth_InitAgeReply();
            reply.fAgeNodeID = ageID;
            reply.fInfoNodeID = infoID;
            reply.fResult = result;
            lock (fVaultToAuthMap) {
                reply.fTransID = fVaultToAuthMap[transID];
                fVaultToAuthMap.Remove(transID);
            }

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
            lock (fVaultToAuthMap) {
                reply.fTransID = fVaultToAuthMap[transID];
                fVaultToAuthMap.Remove(transID);
            }

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
            lock (fVaultToAuthMap) {
                reply.fTransID = fVaultToAuthMap[transID];
                fVaultToAuthMap.Remove(transID);
            }

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
            lock (fVaultToAuthMap) {
                reply.fTransID = fVaultToAuthMap[transID];
                fVaultToAuthMap.Remove(transID);
            }

            lock (fStream) {
                fStream.BufferWriter();
                fStream.WriteUShort((ushort)AuthSrv2Cli.VaultNodeFetched);
                reply.Write(fStream);
                fStream.FlushWriter();
            }
        }

        private void IVaultOnNodeFound(uint transID, ENetError result, uint[] nodes) {
            Auth_VaultNodeFindReply reply = new Auth_VaultNodeFindReply();
            reply.fNodeIDs = nodes;
            reply.fResult = result;
            lock (fVaultToAuthMap) {
                reply.fTransID = fVaultToAuthMap[transID];
                fVaultToAuthMap.Remove(transID);
            }

            lock (fStream) {
                fStream.BufferWriter();
                fStream.WriteUShort((ushort)AuthSrv2Cli.VaultNodeFindReply);
                reply.Write(fStream);
                fStream.FlushWriter();
            }
        }

        private void IVaultOnNodeRefsFetched(uint transID, ENetError result, VaultNodeRef[] refs) {
            Auth_VaultNodeRefsFetched reply = new Auth_VaultNodeRefsFetched();
            reply.fRefs = refs;
            reply.fResult = result;
            lock (fVaultToAuthMap) {
                reply.fTransID = fVaultToAuthMap[transID];
                fVaultToAuthMap.Remove(transID);
            }

            lock (fStream) {
                fStream.BufferWriter();
                fStream.WriteUShort((ushort)AuthSrv2Cli.VaultNodeRefsFetched);
                reply.Write(fStream);
                fStream.FlushWriter();
            }

            Debug(String.Format("Sent VaultTree [COUNT: {0}] [RESULT: {1}]", refs.Length, result.ToString().Substring(1)));
        }

        private void IVaultOnNodeSaved(uint transID, ENetError result) {
            Auth_VaultNodeSaveReply reply = new Auth_VaultNodeSaveReply();
            reply.fResult = result;
            lock (fVaultToAuthMap) {
                reply.fTransID = fVaultToAuthMap[transID];
                fVaultToAuthMap.Remove(transID);
            }

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
            lock (fVaultToAuthMap) {
                reply.fTransID = fVaultToAuthMap[transID];
                fVaultToAuthMap.Remove(transID);
            }

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

            Info(String.Format("Created Player [ID: {0}] [Name: {1}] [Shape: {2}]", playerID, playerName, model));
        }

        private void IVaultOnPong(uint transID, uint pingTime, byte[] payload) {
            Verbose("[VaultCli] PONG!");
        }
    }
}
