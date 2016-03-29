using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace ShadowNet_sample3.protocol.packet
{
    public class CLIENT_HANDSHAKE_DataPacket : Packet
    {
        public static byte ID = 0x13;

        public override byte getID()
        {
            return ID;
        }

        public string address;
        public int port;
        public IPEndPoint[] systemAddresses = new IPEndPoint[10];

        public long sendPing;
        public long sendPong;

        public void encode()
        {
            base.encode();
            this.putAddress(this.address, this.port);
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
            IPEndPoint addr = this.getAddress();
            this.address = addr.Address.ToString();
            this.port = addr.Port;

            for (int i = 0; i < 10; i++)
            {
                this.systemAddresses[i] = this.getAddress();
            }

            this.sendPing = this.getLong();
            this.sendPong = this.getLong();
        }
    }
}
