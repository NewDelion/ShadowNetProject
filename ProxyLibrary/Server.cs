using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace ProxyLibrary
{
    public class Server
    {
        public string ip;
        public int port;
        public IPEndPoint address;

        public Server(string ip, int port)
        {
            this.ip = ip;
            this.port = port;
            this.address = new IPEndPoint(IPAddress.Parse(ip), port);
        }

        public Server(IPEndPoint address)
        {
            this.address = address;
            this.ip = address.Address.ToString();
            this.port = address.Port;
        }

        public Server clone()
        {
            return new Server(this.ip, this.port);
        }
    }
}
