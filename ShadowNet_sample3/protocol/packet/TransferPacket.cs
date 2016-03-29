using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace ShadowNet_sample3.protocol.packet
{
    public class TransferPacket : Packet
    {
        public static byte NETWORK_ID = 0x1b;

        public string address;
        public int port = 19132;

        public override byte getID()
        {
            return NETWORK_ID;
        }

        public void putAddress(string addr, int port, byte version = 0x04)
        {
            this.putByte(version);
            if (version == 0x04)
            {
                for (int i = 0; i < addr.Length; i++)
                    this.putByte((byte)(0xff & ~Convert.ToInt32(addr[i])));
                this.putShort((short)port);
            }
            else
            {
                //IPv6
            }
        }
        public IPEndPoint getAddress()
        {
            int version = this.getByte();
            if (version == 0x04)
            {
                string address = String.Format("{0}.{1}.{2}.{3}", 0xff & ~this.getByte(), 0xff & ~this.getByte(), 0xff & ~this.getByte(), 0xff & ~this.getByte());
                int port = this.getSignedShort() & 0xffff;
                return new IPEndPoint(IPAddress.Parse(address), port);
            }
            else
            {
                return null;
            }
        }

        public override void encode()
        {
            base.encode();
            this.putAddress(this.address, this.port);
        }

        public override void decode()
        {
            base.decode();
            IPEndPoint addr = this.getAddress();
            if (addr != null)
            {
                this.address = addr.Address.ToString();
                this.port = addr.Port;
            }
        }
    }
}
