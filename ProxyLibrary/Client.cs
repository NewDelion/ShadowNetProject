using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace ProxyLibrary
{
    public class Client
    {
        public string ip;
        public int port;
        public IPEndPoint address;
        public byte[] header;
        public Server ConnectingServer;

        public Client(string ip, int port, Server server)
        {
            this.ip = ip;
            this.port = port;
            this.address = new IPEndPoint(IPAddress.Parse(ip), port);
            this.header = Header.encode(this.ip, this.port);
            this.ConnectingServer = server.clone();
        }

        public Client(IPEndPoint address, Server server)
        {
            this.address = address;
            this.ip = address.Address.ToString();
            this.port = address.Port;
            this.header = Header.encode(this.ip, this.port);
            this.ConnectingServer = server.clone();
        }

        public Client(Header header, Server server)
        {
            this.address = header.address;
            this.ip = header.ip;
            this.port = header.port;
            this.header = header.encode();
            this.ConnectingServer = server.clone();
        }

        public Client clone()
        {
            return new Client(this.ip, this.port, this.ConnectingServer.clone());
        }
    }
}
