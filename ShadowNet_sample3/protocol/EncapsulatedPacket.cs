using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;

namespace ShadowNet_sample3.protocol
{
    public class EncapsulatedPacket
    {
        public byte reliability;
        public bool hasSplit = false;
        public int length = 0;
        public int messageIndex = -1;
        public int orderIndex = -1;
        public int orderChannel = -1;
        public int splitCount = -1;
        public int splitID = -1;
        public int splitIndex = -1;
        public byte[] buffer;
        public bool needACK = false;
        public int identifierACK = -1;

        private int offset;

        public int getOffset()
        {
            return this.offset;
        }

        public static EncapsulatedPacket fromBinary(byte[] binary)
        {
            return fromBinary(binary, false);
        }

        public static EncapsulatedPacket fromBinary(byte[] binary, bool @internal)
        {
            EncapsulatedPacket packet = new EncapsulatedPacket();

            byte flags = binary[0];

            packet.reliability = (byte)((flags & 0xe0) >> 5);
            packet.hasSplit = (flags & 0x10) > 0;
            int length, offset;
            if (@internal)
            {
                length = BitConverter.ToInt32(new byte[] { binary[4], binary[3], binary[2], binary[1] }, 0);
                packet.identifierACK = BitConverter.ToInt32(new byte[] { binary[8], binary[7], binary[6], binary[5] }, 0);//Reverse
                offset = 9;
            }
            else
            {
                length = (int)Math.Ceiling(((double)BitConverter.ToInt16(new byte[] { binary[2], binary[1] }, 0) / 8.0));
                offset = 3;
                packet.identifierACK = -1;
            }

            if (packet.reliability > 0)
            {
                if (packet.reliability >= 2 && packet.reliability != 5)
                {
                    packet.messageIndex = BitConverter.ToInt32(new byte[] { binary[offset], binary[offset + 1], binary[offset + 2], 0x00 }, 0);
                    offset += 3;
                }
                if (packet.reliability <= 4 && packet.reliability != 2)
                {
                    packet.orderIndex = BitConverter.ToInt32(new byte[] { binary[offset], binary[offset + 1], binary[offset + 2], 0x00 }, 0);
                    offset += 3;
                    packet.orderChannel = binary[offset++] & 0xff;
                }
            }

            if (packet.hasSplit)
            {
                packet.splitCount = BitConverter.ToInt32(new byte[] { binary[offset + 3], binary[offset + 2], binary[offset + 1], binary[offset] }, 0);
                offset += 4;
                packet.splitID = BitConverter.ToInt16(new byte[] { binary[offset + 1], binary[offset] }, 0);
                offset += 2;
                packet.splitIndex = BitConverter.ToInt32(new byte[] { binary[offset + 3], binary[offset + 2], binary[offset + 1], binary[offset] }, 0);
                offset += 4;
            }

            
            packet.buffer = new byte[length];
            Buffer.BlockCopy(binary, offset, packet.buffer, 0, length);
            offset += length;
            packet.offset = offset;

            return packet;
        }

        public int getTotalLength()
        {
            return 3 + this.buffer.Length + (this.messageIndex != -1 ? 3 : 0) + (this.orderIndex != -1 ? 4 : 0) + (this.hasSplit ? 10 : 0);
        }

        public byte[] toBinary()
        {
            return toBinary(false);
        }
        public byte[] toBinary(bool @internal)
        {
            MemoryStream bb = new MemoryStream(23 + buffer.Length);
            bb.WriteByte((byte)((byte)(reliability << 5) | (hasSplit ? 0x10 : 0)));
            if (@internal)
            {
                bb.Write(BitConverter.GetBytes(buffer.Length).Reverse().ToArray(), 0, 4);
                bb.Write(BitConverter.GetBytes(identifierACK == -1 ? 0 : identifierACK).Reverse().ToArray(), 0, 4);
            }
            else
            {
                bb.Write(BitConverter.GetBytes((short)(buffer.Length << 3)).Reverse().ToArray(), 0, 2);
            }

            if (reliability > 0)
            {
                if (reliability >= 2 && reliability != 5)
                {
                    bb.Write(BitConverter.GetBytes(messageIndex == -1 ? 0 : messageIndex), 0, 3);
                }
                if (reliability <= 4 && reliability != 2)
                {
                    bb.Write(BitConverter.GetBytes(orderIndex), 0, 3);
                    bb.WriteByte((byte)(orderChannel & 0xff));
                }
            }

            if (hasSplit)
            {
                bb.Write(BitConverter.GetBytes(splitCount).Reverse().ToArray(), 0, 4);
                bb.Write(BitConverter.GetBytes((short)splitID).Reverse().ToArray(), 0, 2);
                bb.Write(BitConverter.GetBytes(splitIndex).Reverse().ToArray(), 0, 4);
            }

            bb.Write(buffer, 0, buffer.Length);
            return bb.ToArray();
        }
    }
}
