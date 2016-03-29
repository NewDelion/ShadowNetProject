using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShadowNet_sample3.protocol.packet
{
    public class ACK : AcknowledgePacket
    {
        public static byte ID = 0xc0;

        public override byte getID()
        {
            return ID;
        }
    }
}
