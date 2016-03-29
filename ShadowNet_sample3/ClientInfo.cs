using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using ShadowNet_sample3.protocol;

namespace ShadowNet_sample3
{
    public class ClientInfo
    {
        public IPEndPoint address;
        public IPEndPoint ConnectingServer;
        public long clientId;
        public List<byte[]> LoginPacket = new List<byte[]>();
        public byte[] header;
        public IPEndPoint TransferTargetServer;

        public ClientInfo(IPEndPoint address, IPEndPoint server)
        {
            this.address = new IPEndPoint(IPAddress.Parse(address.Address.ToString()), address.Port);
            this.ConnectingServer = new IPEndPoint(IPAddress.Parse(server.Address.ToString()), server.Port);
            this.header = utils.Header.encode(this.address);
        }

        public EncapsulatedPacket packPacket(byte[] buffer, byte reliability, int messageIndex = 3)
        {
            EncapsulatedPacket epk = new EncapsulatedPacket();
            epk.buffer = buffer;
            epk.reliability = reliability;
            epk.messageIndex = messageIndex;
            return epk;
        }

        public byte[] getDataPacketPayload(EncapsulatedPacket epk, int seqNumber, bool toServer = false)
        {
            DataPacket dp = new DataPacket();
            dp.seqNumber = seqNumber;
            dp.packets.Add(epk);
            dp.encode();
            if (toServer)
                return this.header.Concat(dp.buffer).ToArray();
            return dp.buffer;
        }
    }
}
