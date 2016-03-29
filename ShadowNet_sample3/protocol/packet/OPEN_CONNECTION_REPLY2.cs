using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace ShadowNet_sample3.protocol.packet
{
    public class OPEN_CONNECTION_REPLY2 : Packet
    {
        public static byte ID = 0x08;

        public override byte getID()
        {
            return ID;
        }

        public long serverID;
        public string clientAddress;
        public int clientPort;
        public short mtuSize;

        public void encode()
        {
            base.encode();
            this.put(RakNet.MAGIC);
            this.putLong(this.serverID);
            this.putAddress(this.clientAddress, this.clientPort);
            this.putShort(this.mtuSize);
            this.putByte(0x00);
        }

        public void decode()
        {
            base.decode();
            this.offset += 16;
            this.serverID = this.getLong();
            IPEndPoint address = this.getAddress();
            this.clientAddress = address.Address.ToString();
            this.clientPort = (short)address.Port;
            this.mtuSize = this.getSignedShort();
        }
    }
}
