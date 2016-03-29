using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShadowNet_sample3.protocol
{
    public class DataPacket : Packet
    {
        public List<object> packets = new List<object>();

        public int seqNumber;
        public byte ID = 0x80;

        public new void encode()
        {
            base.encode();
            this.putLTriad(this.seqNumber);
            foreach (object packet in this.packets)
            {
                this.put(packet is EncapsulatedPacket ? ((EncapsulatedPacket)packet).toBinary() : (byte[])packet);
            }
        }

        public int length()
        {
            int length = 4;
            foreach (object packet in this.packets)
            {
                length += packet is EncapsulatedPacket ? ((EncapsulatedPacket)packet).getTotalLength() : ((byte[])packet).Length;
            }

            return length;
        }

        public new void decode()
        {
            ID = this.getByte();
            this.seqNumber = this.getLTriad();

            while (!this.feof())
            {
                byte[] data = this.buffer.Skip(this.offset).ToArray();
                EncapsulatedPacket packet = EncapsulatedPacket.fromBinary(data, false);
                this.offset += packet.getOffset();
                if (packet.buffer.Length == 0)
                    break;
                this.packets.Add(packet);
            }
        }

        public new Packet clean()
        {
            this.packets.Clear();
            this.seqNumber = 0;
            return base.clean();
        }
    }
}
