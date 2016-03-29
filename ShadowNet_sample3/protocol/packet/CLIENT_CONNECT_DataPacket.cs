using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShadowNet_sample3.protocol.packet
{
    public class CLIENT_CONNECT_DataPacket : Packet
    {
        public static byte ID = 0x09;

        public override byte getID()
        {
            return ID;
        }

        public long clientID;
        public long sendPing;
        public bool useSecurity = false;

        public void encode()
        {
            base.encode();
            this.putLong(this.clientID);
            this.putLong(this.sendPing);
            this.putByte(this.useSecurity ? (byte)0x01 : (byte)0x00);
        }

        public void decode()
        {
            base.decode();
            this.clientID = this.getLong();
            this.sendPing = this.getLong();
            this.useSecurity = this.getByte() > 0;
        }
    }
}
