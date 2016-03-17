using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace ProxyLibrary
{
    public abstract class ProxyBase
    {
        public UdpClient client;
        public int proxy_port = 19132;

        public bool enable = false;
        public ReaderWriterLock rwlock_enable = new ReaderWriterLock();

        public List<Server> ServerList = new List<Server>();
        public List<Client> ClientList = new List<Client>();
        public ReaderWriterLock rwlock_ServerList = new ReaderWriterLock();
        public ReaderWriterLock rwlock_ClientList = new ReaderWriterLock();

        public List<Packet> ReceiveQueue = new List<Packet>();
        public Object lockObj_ReceiveQueue = new Object();

        public ProxyBase() { }
        public ProxyBase(int proxy_port)
        {
            this.proxy_port = proxy_port;
        }

        public void Start()
        {
            this.enable = true;
            Task.Run(() => { this.PacketSender(); });
            this.client = new UdpClient(this.proxy_port);
            this.client.Client.ReceiveBufferSize = 1024 * 1024 * 10;
            this.client.Client.SendBufferSize = 1024 * 1024 * 10;
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

        public void PacketSender()
        {
            while (true)
            {
                if (!this.isEnable())
                    break;

                Packet[] pcks = null;
                lock (this.lockObj_ReceiveQueue)
                {
                    pcks = this.ReceiveQueue.ToArray();
                    this.ReceiveQueue.Clear();
                }
                List<IPEndPoint> ThroughList = new List<IPEndPoint>();
                foreach (Packet packet in pcks)
                {
                    if (ThroughList.Contains(packet.address))
                        continue;
                    AnalyzeResult result = null;
                    if (this.fromServer(packet))
                    {
                        Header header = Header.decode(packet.buffer.Take(6).ToArray());
                        packet.buffer = packet.buffer.Skip(6).ToArray();
                        if (!this.RegisteredClient(header.address))
                            this.addClient(new Client(header, this.getServer(0)));
                        result = this.AnalyzeServer(this.getClient(header.address), packet);
                    }
                    else
                    {
                        if (!this.RegisteredClient(packet.address))
                            this.addClient(new Client(packet.address, this.getServer(0)));
                        Client client = this.getClient(packet.address);
                        result = this.AnalyzeClient(client, packet);
                        if (result != null)
                            if (result.packet != null)
                                result.packet.buffer = client.header.Concat(result.packet.buffer).ToArray();
                        if (result.logout)
                        {
                            ThroughList.Add(client.address);
                            this.removeClient(client.address);
                        }
                    }
                    if (result == null)
                        continue;
                    if (result.result == Command.Send)
                    {
                        this.sendPacket(result.packet);
                    }
                }
            }
            Console.WriteLine("Sender stopped...");
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

            lock (this.lockObj_ReceiveQueue)
                this.ReceiveQueue.Add(new Packet(recBytes, remoteEP));

            if (client.Client != null)
                client.BeginReceive(ReceiveCallback, client);

        }

        public void sendPacket(Packet packet)
        {
            this.client.Send(packet.buffer, packet.buffer.Length, packet.address);
        }

        public bool fromServer(Packet packet)
        {
            bool result = false;
            try
            {
                this.rwlock_ServerList.AcquireReaderLock(Timeout.Infinite);
                result = this.ServerList.Exists((d) => d.address.Equals(packet.address));
            }
            finally
            {
                this.rwlock_ServerList.ReleaseLock();
            }
            return result;
        }

        public bool RegisteredClient(IPEndPoint address)
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
        public bool RegisteredClient(string ip, int port)
        {
            bool result = false;
            try
            {
                this.rwlock_ClientList.AcquireReaderLock(Timeout.Infinite);
                result = this.ClientList.Exists((d) => d.ip.Equals(ip) && d.port.Equals(port));
            }
            finally
            {
                this.rwlock_ClientList.ReleaseLock();
            }
            return result;
        }

        /// <summary>
        /// このメソッドを使用する前に登録されていないことを確認してください。
        /// </summary>
        public void addClient(Client client)
        {
            try
            {
                this.rwlock_ClientList.AcquireWriterLock(Timeout.Infinite);
                this.ClientList.Add(client.clone());
            }
            finally
            {
                this.rwlock_ClientList.ReleaseLock();
            }
        }
        public void removeClient(IPEndPoint address)
        {
            try
            {
                this.rwlock_ClientList.AcquireWriterLock(Timeout.Infinite);
                this.ClientList.RemoveAll((d) => d.address.Equals(address));
            }
            finally
            {
                this.rwlock_ClientList.ReleaseLock();
            }
        }
        public void addServer(Server server)
        {
            try
            {
                this.rwlock_ServerList.AcquireWriterLock(Timeout.Infinite);
                this.ServerList.Add(server.clone());
            }
            finally
            {
                this.rwlock_ServerList.ReleaseLock();
            }
        }

        public Server getServer(int index)
        {
            Server result = null;
            try
            {
                this.rwlock_ServerList.AcquireReaderLock(Timeout.Infinite);
                result = this.ServerList[index].clone();
            }
            finally
            {
                this.rwlock_ServerList.ReleaseLock();
            }
            return result;
        }
        public Client getClient(IPEndPoint address)
        {
            Client result = null;
            try
            {
                this.rwlock_ClientList.AcquireReaderLock(Timeout.Infinite);
                result = this.ClientList.Find((d) => d.address.Equals(address)).clone();
            }
            finally
            {
                this.rwlock_ClientList.ReleaseLock();
            }
            return result;
        }

        public void SwitchServer(IPEndPoint client_address, Server server)
        {
            try
            {
                this.rwlock_ClientList.AcquireWriterLock(Timeout.Infinite);
                Client c = this.ClientList.Find((d) => d.address.Equals(client_address));
                c.ConnectingServer = server.clone();
            }
            finally
            {
                this.rwlock_ClientList.ReleaseLock();
            }
        }

        public virtual AnalyzeResult AnalyzeServer(Client client, Packet packet)
        {
            return new AnalyzeResult(Command.Send, new Packet(packet.buffer, client.address));
        }

        public virtual AnalyzeResult AnalyzeClient(Client client, Packet packet)
        {
            return new AnalyzeResult(Command.Send, new Packet(packet.buffer, client.ConnectingServer.address));
        }
    }
}
