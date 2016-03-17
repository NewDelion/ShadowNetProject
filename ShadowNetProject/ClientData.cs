using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

using ProxyLibrary;
using MCPEPacketLib.raklib;
using MCPEPacketLib.raklib.protocol;
using MCPEPacketLib.mcpe;
using MCPEPacketLib.mcpe.protocol;
using MCPEPacketLib.utils;

namespace ShadowNetProject
{
    public class ClientData
    {
        public Client client;
        public long ClientID;
        public int seqNumber = 0;
        public int messageIndex = 0;
        public int SessionStatus = 0;
        public IPEndPoint TargetServer = null;
        //public byte[] loginPacket = null;
        public List<byte[]> loginPacket = new List<byte[]>();
        public int SplitId;
        public int SplitLength;
        public int SplitCount;

        public ClientData(Client client)
        {
            this.client = client;
        }

        public EncapsulatedPacket packPacket(byte[] buffer, byte reliability)
        {
            EncapsulatedPacket epk = new EncapsulatedPacket();
            epk.buffer = buffer;
            epk.reliability = reliability;
            epk.messageIndex = this.messageIndex++;
            return epk;
        }

        public byte[] getDataPacketPayload(EncapsulatedPacket epk, bool toServer = false)
        {
            DATA_PACKET_4 dp = new DATA_PACKET_4();
            dp.seqNumber = this.seqNumber++;
            dp.packets.Add(epk);
            dp.encode();
            if (toServer)
                return this.client.header.Concat(dp.buffer).ToArray();
            return dp.buffer;
        }

        public void cleanSession()
        {
            this.SessionStatus = 0;
            this.seqNumber = 0;
            this.messageIndex = 0;
            this.TargetServer = null;
        }
    }
}
