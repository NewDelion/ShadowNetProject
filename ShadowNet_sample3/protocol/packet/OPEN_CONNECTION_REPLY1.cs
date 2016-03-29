using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShadowNet_sample3.protocol.packet
{
    public class OPEN_CONNECTION_REPLY1 : Packet
    {
        public static byte ID = 0x06;

        public override byte getID()
        {
            return ID;
        }

        public long serverID;
        public short mtuSize;

        public void encode()
        {
            base.encode();
            this.put(RakNet.MAGIC);
            this.putLong(this.serverID);
            this.putByte(0x00);//server security
            this.putShort(this.mtuSize);
        }

        public void decode()
        {
            base.decode();
            this.offset += 16;//skip magic bytes
            this.serverID = this.getLong();
            this.getByte();//skip security
            this.mtuSize = this.getSignedShort();
        }
    }
}
