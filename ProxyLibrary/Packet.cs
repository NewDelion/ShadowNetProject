using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace ProxyLibrary
{
    public class Packet
    {
        public byte[] buffer;
        public IPEndPoint address;

        public Packet(byte[] buffer, IPEndPoint address)
        {
            this.buffer = buffer;
            this.address = address;
        }
    }
}
