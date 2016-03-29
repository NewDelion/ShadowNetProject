using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShadowNet_sample3.protocol.packet
{
    public class OPEN_CONNECTION_REQUEST1 : Packet
    {
        public static byte ID = 0x05;

        public override byte getID()
        {
            return ID;
        }

        public byte protocol = RakNet.PROTOCOL;
        public short mtuSize;

        public void encode()
        {
            base.encode();
            this.put(RakNet.MAGIC);
            this.putByte(this.protocol);
            this.put(new byte[this.mtuSize - 18]);
        }

        public void decode()
        {
            base.decode();
            this.offset += 16;
            this.protocol = this.getByte();
            this.mtuSize = (short)(this.get().Length + 18);
        }
    }
}
