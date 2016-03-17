using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

using ProxyLibrary;
using MCPEPacketLib.raklib;
using MCPEPacketLib.raklib.protocol;
using MCPEPacketLib.utils;

namespace AdvancedProxyLibrary
{
    public abstract class AdvancedProxyBase : ProxyBase
    {
        public int currentSeqNumber = 0;

        public void sendPayload(byte[] payload, IPEndPoint address)
        {
            this.client.Send(payload, payload.Length, address);
        }

        public EncapsulatedPacket packPacket(byte[] payload, int messageIndex, byte reliability = 0x03)
        {
            EncapsulatedPacket result = new EncapsulatedPacket();
            result.buffer = new byte[payload.Length];
            Array.Copy(payload, result.buffer, payload.Length);
            result.messageIndex = messageIndex;
            result.reliability = reliability;
            return result;
        }

        public void sendDataPacket(EncapsulatedPacket[] packets, IPEndPoint address, int type = 4, bool incSeq = true)
        {
            DataPacket dp = null;
            switch (type)
            {
                case 0:
                    dp = new DATA_PACKET_0();
                    break;
                case 1:
                    dp = new DATA_PACKET_1();
                    break;
                case 2:
                    dp = new DATA_PACKET_2();
                    break;
                case 3:
                    dp = new DATA_PACKET_3();
                    break;
                case 4:
                    dp = new DATA_PACKET_4();
                    break;
                case 5:
                    dp = new DATA_PACKET_5();
                    break;
                case 6:
                    dp = new DATA_PACKET_6();
                    break;
                case 7:
                    dp = new DATA_PACKET_7();
                    break;
                case 8:
                    dp = new DATA_PACKET_8();
                    break;
                case 9:
                    dp = new DATA_PACKET_9();
                    break;
                case 10:
                    dp = new DATA_PACKET_A();
                    break;
                case 11:
                    dp = new DATA_PACKET_B();
                    break;
                case 12:
                    dp = new DATA_PACKET_C();
                    break;
                case 13:
                    dp = new DATA_PACKET_D();
                    break;
                case 14:
                    dp = new DATA_PACKET_E();
                    break;
                case 15:
                    dp = new DATA_PACKET_F();
                    break;
            }
            foreach (EncapsulatedPacket epk in packets)
                dp.packets.Add(epk);
            dp.seqNumber = this.currentSeqNumber;
            if (incSeq) this.currentSeqNumber++;
            dp.encode();
            this.sendPayload(dp.buffer, address);
        }
        public void sendDataPacket(EncapsulatedPacket packet, IPEndPoint address, int type = 4, bool incSeq = true)
        {
            DataPacket dp = null;
            switch (type)
            {
                case 0:
                    dp = new DATA_PACKET_0();
                    break;
                case 1:
                    dp = new DATA_PACKET_1();
                    break;
                case 2:
                    dp = new DATA_PACKET_2();
                    break;
                case 3:
                    dp = new DATA_PACKET_3();
                    break;
                case 4:
                    dp = new DATA_PACKET_4();
                    break;
                case 5:
                    dp = new DATA_PACKET_5();
                    break;
                case 6:
                    dp = new DATA_PACKET_6();
                    break;
                case 7:
                    dp = new DATA_PACKET_7();
                    break;
                case 8:
                    dp = new DATA_PACKET_8();
                    break;
                case 9:
                    dp = new DATA_PACKET_9();
                    break;
                case 10:
                    dp = new DATA_PACKET_A();
                    break;
                case 11:
                    dp = new DATA_PACKET_B();
                    break;
                case 12:
                    dp = new DATA_PACKET_C();
                    break;
                case 13:
                    dp = new DATA_PACKET_D();
                    break;
                case 14:
                    dp = new DATA_PACKET_E();
                    break;
                case 15:
                    dp = new DATA_PACKET_F();
                    break;
            }
            dp.packets.Add(packet);
            dp.seqNumber = this.currentSeqNumber;
            if (incSeq) this.currentSeqNumber++;
            dp.encode();
            this.sendPayload(dp.buffer, address);
        }

        public void sendACK(IPEndPoint address, int seqNumber)
        {
            ACK ack = new ACK();
            ack.packets.Add(seqNumber);
            ack.encode();
            this.sendPayload(ack.buffer, address);
        }
    }
}
