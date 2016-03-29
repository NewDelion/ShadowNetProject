using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace ShadowNet_sample3.protocol.packet
{
    public class OPEN_CONNECTION_REQUEST2 : Packet
    {
        public static byte ID = 0x07;

        public override byte getID()
        {
            return ID;
        }

        public long clientID;
        public string serverAddress;
        public int serverPort;
        public short mtuSize;

        public void encode()
        {
            base.encode();
            this.put(RakNet.MAGIC);
            this.putAddress(this.serverAddress, this.serverPort);
            this.putShort(this.mtuSize);
            this.putLong(this.clientID);
        }

        public void decode()
        {
            base.decode();
            this.offset += 16;
            IPEndPoint address = this.getAddress();
            this.serverAddress = address.Address.ToString();
            this.serverPort = (short)address.Port;
            this.mtuSize = this.getSignedShort();
            this.clientID = this.getLong();
        }
    }
}
