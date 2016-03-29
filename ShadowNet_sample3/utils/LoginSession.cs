using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace ShadowNet_sample3.utils
{
    public class LoginSession
    {
        public IPEndPoint address;
        public int SplitID;
        public int SplitLength;
        public int SplitCount;

        private byte[][] LoginPackets;

        public LoginSession(IPEndPoint address, int SplitID, int SplitLength)
        {
            this.address = new IPEndPoint(IPAddress.Parse(address.Address.ToString()), address.Port);
            this.SplitID = SplitID;
            this.SplitLength = SplitLength;
            this.SplitCount = 0;

            this.LoginPackets = new byte[this.SplitLength][];
        }

        public byte[][] addPacket(byte[] packet)
        {
            this.LoginPackets[this.SplitCount] = new byte[packet.Length];
            Buffer.BlockCopy(packet, 0, this.LoginPackets[this.SplitCount], 0, packet.Length);
            if (this.SplitLength <= ++this.SplitCount)
                return this.LoginPackets;
            return null;
        }
    }
}
