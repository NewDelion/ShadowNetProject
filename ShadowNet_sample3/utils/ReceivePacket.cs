using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace ShadowNet_sample3.utils
{
    public class ReceivePacket
    {
        public IPEndPoint address;
        public byte[] buffer;

        public ReceivePacket(IPEndPoint address, byte[] buffer)
        {
            this.address = address;
            this.buffer = buffer;
        }
    }
}
