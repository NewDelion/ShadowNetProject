using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace ShadowNet_sample3.protocol.packet
{
    public class SERVER_HANDSHAKE_DataPacket : Packet
    {
        public static byte ID = 0x10;

        public override byte getID()
        {
            return ID;
        }

        public string address;
        public int port;
        public IPEndPoint[] systemAddresses = new IPEndPoint[]
        {
            new IPEndPoint(IPAddress.Parse("127.0.0.1"),0),
            new IPEndPoint(IPAddress.Parse("0.0.0.0"),0),
            new IPEndPoint(IPAddress.Parse("0.0.0.0"),0),
            new IPEndPoint(IPAddress.Parse("0.0.0.0"),0),
            new IPEndPoint(IPAddress.Parse("0.0.0.0"),0),
            new IPEndPoint(IPAddress.Parse("0.0.0.0"),0),
            new IPEndPoint(IPAddress.Parse("0.0.0.0"),0),
            new IPEndPoint(IPAddress.Parse("0.0.0.0"),0),
            new IPEndPoint(IPAddress.Parse("0.0.0.0"),0),
            new IPEndPoint(IPAddress.Parse("0.0.0.0"),0),
        };

        public long sendPing;
        public long sendPong;

        public void encode()
        {
            base.encode();
            this.putAddress(new IPEndPoint(IPAddress.Parse(this.address), this.port));
            this.putShort(0);
            for (int i = 0; i < 10; i++)
            {
                this.putAddress(this.systemAddresses[i]);
            }

            this.putLong(this.sendPing);
            this.putLong(this.sendPong);
        }

        public void decode()
        {
            base.decode();
        }
    }
}
