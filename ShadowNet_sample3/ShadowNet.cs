using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

using ShadowNet_sample3.utils;
using ShadowNet_sample3.protocol;
using ShadowNet_sample3.protocol.packet;

namespace ShadowNet_sample3
{
    public class ShadowNet
    {
        private UdpClient client;
        public const int proxy_port = 19132;

        public bool enable = false;

        #region //List系
        private List<LoginSession> LoginSessionList = new List<LoginSession>();
        public List<ClientInfo> ClientList = new List<ClientInfo>();
        public List<IPEndPoint> ServerList = new List<IPEndPoint>();
        public List<ReceivePacket> ReceiveQueue = new List<ReceivePacket>();
        #endregion

        #region //LockObject系
        private ReaderWriterLock rwlock_enable = new ReaderWriterLock();
        public ReaderWriterLock rwlock_ClientList = new ReaderWriterLock();
        public ReaderWriterLock rwlock_ServerList = new ReaderWriterLock();
        public Object lockObj_ReceiveQueue = new Object();
        #endregion

        public void Start()
        {
            if (this.ServerList.Count == 0)
            {
                Console.WriteLine("サーバを追加してください。");
                return;
            }
            this.enable = true;
            //Task.Run(() => { });
            this.client = new UdpClient(proxy_port);
            this.client.Client.ReceiveBufferSize = 1024 * 1024 * 5;
            this.client.Client.SendBufferSize = 1024 * 1024 * 5;
            this.client.BeginReceive(this.ReceiveCallback, this.client);
        }
        public void Stop()
        {
            try
            {
                this.rwlock_enable.AcquireWriterLock(Timeout.Infinite);
                this.enable = false;
            }
            finally
            {
                this.rwlock_enable.ReleaseLock();
            }

            this.client.Close();
        }

        public void PacketSender()
        {
            while (true)
            {
                if (!this.isEnable())
                    break;

                ReceivePacket[] pcks = null;
                lock (this.lockObj_ReceiveQueue)
                {
                    pcks = this.ReceiveQueue.ToArray();
                    this.ReceiveQueue.Clear();
                }
                foreach (ReceivePacket packet in pcks)
                {
                    if (this.fromServer(packet.address))
                    {

                    }
                    #region //from client
                    else //from client
                    {
                        if (!this.RegisteredClientInfo(packet.address))
                            this.addClientInfo(new ClientInfo(packet.address, this.getDefaultServer()));
                        ClientInfo client = this.getClientInfo(packet.address);
                        if (0x80 <= packet.buffer[0] && packet.buffer[0] <= 0x8f)
                        {
                            DataPacket dp = new DataPacket();
                            dp.buffer = packet.buffer;
                            dp.decode();
                            foreach (EncapsulatedPacket epk in dp.packets)
                            {
                                if (epk.buffer.Length == 1 && epk.buffer[0] == 0x15)
                                {
                                    this.removeClientInfo(packet.address);
                                    this.sendPacket(client.ConnectingServer, client.header.Concat(packet.buffer).ToArray());
                                }
                                else if (epk.buffer[1] == 0x8f && epk.hasSplit && client.LoginPacket.Count == 0)
                                {
                                    this.LoginSessionList.Add(new LoginSession(client.address, epk.splitID, epk.splitCount));
                                    this.LoginSessionList.Find((d) => d.address.Equals(client.address)).addPacket(packet.buffer);
                                    this.sendPacket(client.ConnectingServer, client.header.Concat(packet.buffer).ToArray());
                                }
                                else
                                {
                                    LoginSession session = this.LoginSessionList.Find((d) => d.address.Equals(client.address));
                                    if (session != null)
                                    {
                                        if (epk.hasSplit && session.SplitID == epk.splitID)
                                        {
                                            byte[][] packets = session.addPacket(packet.buffer);
                                            if (packets != null)
                                            {
                                                client.LoginPacket.AddRange(packets);
                                                this.LoginSessionList.RemoveAll((d) => d.address.Equals(packet.address));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (packet.buffer[0] == OPEN_CONNECTION_REQUEST2.ID)
                            {
                                OPEN_CONNECTION_REQUEST2 req2 = new OPEN_CONNECTION_REQUEST2();
                                req2.buffer = packet.buffer;
                                req2.decode();
                                client.clientId = req2.clientID;
                            }
                        }
                    }
                    #endregion
                }
            }
        }

        public bool isEnable()
        {
            bool result = false;
            try
            {
                this.rwlock_enable.AcquireReaderLock(Timeout.Infinite);
                result = this.enable;
            }
            finally
            {
                this.rwlock_enable.ReleaseLock();
            }
            return result;
        }

        public void ReceiveCallback(IAsyncResult iar)
        {
            UdpClient client = (UdpClient)iar.AsyncState;
            IPEndPoint remoteEP = null;
            byte[] recBytes = null;
            try { recBytes = client.EndReceive(iar, ref remoteEP); }
            catch (SocketException ex)
            {
                Console.WriteLine("[ReceiveError] " + ex.Message);
                return;
            }
            catch (ObjectDisposedException ex)
            {
                if (this.isEnable())
                    Console.WriteLine("[SocketError] Socket Disposed...");
                return;
            }

            //少しでも受信開始を早めたい
            if (client.Client != null)
                client.BeginReceive(ReceiveCallback, client);

            lock (this.lockObj_ReceiveQueue)
                this.ReceiveQueue.Add(new ReceivePacket(remoteEP, recBytes));
        }

        public void sendPacket(IPEndPoint address, byte[] payload)
        {
            this.client.Send(payload, payload.Length, address);
        }

        public bool fromServer(IPEndPoint address)
        {
            bool result = false;
            try
            {
                this.rwlock_ServerList.AcquireReaderLock(Timeout.Infinite);
                result = this.ServerList.Exists((d) => d.Equals(address));
            }
            finally
            {
                this.rwlock_ServerList.ReleaseLock();
            }
            return result;
        }
        public void addServer(IPEndPoint address)
        {
            try
            {
                this.rwlock_ServerList.AcquireWriterLock(Timeout.Infinite);
                this.ServerList.Add(address);
            }
            finally
            {
                this.rwlock_ServerList.ReleaseLock();
            }
        }
        public void removeServer(IPEndPoint address)
        {
            try
            {
                this.rwlock_ServerList.AcquireWriterLock(Timeout.Infinite);
                this.ServerList.RemoveAll((d) => d.Equals(address));
            }
            finally
            {
                this.rwlock_ServerList.ReleaseLock();
            }
        }
        public IPEndPoint getDefaultServer()
        {
            IPEndPoint result = null;
            try
            {
                this.rwlock_ServerList.AcquireWriterLock(Timeout.Infinite);
                result = new IPEndPoint(IPAddress.Parse(this.ServerList[0].Address.ToString()), this.ServerList[0].Port);
            }
            finally
            {
                this.rwlock_ServerList.ReleaseLock();
            }
            return result;
        }

        public void addClientInfo(ClientInfo client)
        {
            try
            {
                this.rwlock_ClientList.AcquireWriterLock(Timeout.Infinite);
                this.ClientList.Add(client);
            }
            finally
            {
                this.rwlock_ClientList.ReleaseLock();
            }
        }
        public void removeClientInfo(IPEndPoint address)
        {
            try
            {
                this.rwlock_ClientList.AcquireWriterLock(Timeout.Infinite);
                this.ClientList.RemoveAll((d) => d.Equals(address));
            }
            finally
            {
                this.rwlock_ClientList.ReleaseLock();
            }
        }
        public ClientInfo getClientInfo(IPEndPoint address)
        {
            ClientInfo result = null;
            try
            {
                this.rwlock_ClientList.AcquireWriterLock(Timeout.Infinite);
                result = this.ClientList.Find((d) => d.address.Equals(address));
            }
            finally
            {
                this.rwlock_ClientList.ReleaseLock();
            }
            return result;
        }
        public bool RegisteredClientInfo(IPEndPoint address)
        {
            bool result = false;
            try
            {
                this.rwlock_ClientList.AcquireReaderLock(Timeout.Infinite);
                result = this.ClientList.Exists((d) => d.address.Equals(address));
            }
            finally
            {
                this.rwlock_ClientList.ReleaseLock();
            }
            return result;
        }
    }
}
