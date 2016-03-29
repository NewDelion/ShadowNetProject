using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ShadowNet_sample3.protocol
{
    public abstract class AcknowledgePacket : Packet
    {
        public List<int> packets = new List<int>();

        public void encode()
        {
            base.encode();
            int[] packets = this.packets.ToArray();
            int count = packets.Length;
            short records = 0;
            using (MemoryStream payload = new MemoryStream())
            {
                if (count > 0)
                {
                    int pointer = 1;
                    int start = packets[0];
                    int last = packets[0];

                    while (pointer < count)
                    {
                        int current = packets[pointer++];
                        int diff = current - last;
                        if (diff == 1)
                        {
                            last = current;
                        }
                        else if (diff > 1)
                        {
                            if (start == last)
                            {
                                payload.WriteByte(0x01);
                                payload.Write(BitConverter.GetBytes(start), 0, 3);
                                last = current;
                                start = last;
                            }
                            else
                            {
                                payload.WriteByte(0x00);
                                payload.Write(BitConverter.GetBytes(start), 0, 3);
                                payload.Write(BitConverter.GetBytes(last), 0, 3);
                                last = current;
                                start = last;
                            }
                            ++records;
                        }
                    }

                    if (start == last)
                    {
                        payload.WriteByte(0x01);
                        payload.Write(BitConverter.GetBytes(start), 0, 3);
                    }
                    else
                    {
                        payload.WriteByte(0x00);
                        payload.Write(BitConverter.GetBytes(start), 0, 3);
                        payload.Write(BitConverter.GetBytes(last), 0, 3);
                    }
                    ++records;
                }

                this.putShort(records);
                this.buffer = this.buffer.Concat(payload.ToArray()).ToArray();
            }
        }

        public void decode()
        {
            base.decode();
            short count = this.getSignedShort();
            this.packets = new List<int>();
            int cnt = 0;
            for (int i = 0; i < count && !this.feof() && cnt < 4096; i++)
            {
                if (this.getByte() == 0x00)
                {
                    int start = this.getLTriad();
                    int end = this.getLTriad();
                    if ((end - start) > 512)
                    {
                        end = start + 512;
                    }
                    for (int c = start; c <= end; c++)
                    {
                        packets.Add(c);
                    }
                }
                else
                {
                    this.packets.Add(this.getLTriad());
                }
            }
        }
    }
}
