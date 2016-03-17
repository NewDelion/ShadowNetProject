using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

using ProxyLibrary;
using AdvancedProxyLibrary;
using MCPEPacketLib.raklib;
using MCPEPacketLib.raklib.protocol;
using MCPEPacketLib.utils;

namespace ShadowNetProject
{
    public class ShadowNet : AdvancedProxyBase
    {
        public List<ClientData> DataList = new List<ClientData>();

        public ShadowNet()
        {
            this.proxy_port = 19132;
        }

        public ClientData getData(Client client)
        {
            return this.DataList.Find((d) => d.client.address.Equals(client.address));
        }

        public override ProxyLibrary.AnalyzeResult AnalyzeServer(ProxyLibrary.Client client, ProxyLibrary.Packet packet)
        {
            ClientData data = this.getData(client);
            if (data == null)
            {
                this.DataList.Add(new ClientData(client.clone()));
                data = this.getData(client);
            }

            if (client.ConnectingServer.address.Equals(packet.address))
            {
                if (data.SessionStatus == 4)
                    return new AnalyzeResult(Command.Cancel, null);
                else if (PacketUtils.isDataPacket(packet.buffer[0]))
                {
                    DataPacket dataPacket = PacketUtils.getDataPacket_fromBinary(packet.buffer);
                    foreach (EncapsulatedPacket enpk in dataPacket.packets)
                    {
                        if (enpk.buffer == null)
                            continue;
                        if (enpk.buffer.Length < 2)
                            continue;
                        if (enpk.buffer[1] == 0x1b && !enpk.hasSplit)
                        {
                            MCPEPacketLib.mcpe.protocol.StrangePacket TransferPacket = new MCPEPacketLib.mcpe.protocol.StrangePacket();
                            TransferPacket.setBuffer(enpk.buffer.Skip(1).ToArray());
                            TransferPacket.decode();
                            data.cleanSession();
                            data.SessionStatus++;
                            data.TargetServer = new IPEndPoint(IPAddress.Parse(TransferPacket.address), TransferPacket.port);
                            this.TransferSession(data);
                        }
                    }
                }
            }
            else if (data.SessionStatus > 0)
            {
                if (packet.buffer[0] == OPEN_CONNECTION_REPLY1.ID)
                {
                    data.SessionStatus++;
                    this.TransferSession(data);
                }
                else if (packet.buffer[0] == OPEN_CONNECTION_REPLY2.ID)
                {
                    data.SessionStatus++;
                    this.TransferSession(data);
                }
                else if (PacketUtils.isDataPacket(packet.buffer[0]))
                {
                    DataPacket dp = PacketUtils.getDataPacket_fromBinary(packet.buffer);
                    this.sendACK(data.TargetServer, dp.seqNumber);
                    foreach (EncapsulatedPacket epk in dp.packets)
                    {
                        if (epk.buffer[0] == SERVER_HANDSHAKE_DataPacket.ID)
                        {
                            data.SessionStatus++;
                            this.TransferSession(data);
                        }
                    }
                }
                return new AnalyzeResult(Command.Cancel, null);
            }
            else
            {
                //Console.WriteLine("だれこいつ");
                return new AnalyzeResult(Command.Cancel, null);
            }

            return base.AnalyzeServer(client, packet);
        }

        public override AnalyzeResult AnalyzeClient(Client client, ProxyLibrary.Packet packet)
        {
            ClientData data = this.getData(client);
            if (data == null)
            {
                this.DataList.Add(new ClientData(client.clone()));
                data = this.getData(client);
            }

            if (PacketUtils.isDataPacket(packet.buffer[0]))
            {
                DataPacket dp = PacketUtils.getDataPacket_fromBinary(packet.buffer);
                foreach (EncapsulatedPacket epk in dp.packets)
                {
                    if (epk.buffer.Length == 1 && epk.buffer[0] == 0x15)
                    {
                        this.DataList.RemoveAll((d) => d.client.address.Equals(packet.address));
                        return new AnalyzeResult(Command.Send, new ProxyLibrary.Packet(packet.buffer, client.ConnectingServer.address), true);
                    }
                    else if (epk.buffer[1] == 0x8f && epk.hasSplit && data.loginPacket.Count == 0)
                    {
                        data.SplitId = epk.splitID;
                        data.SplitLength = epk.splitCount;
                        data.SplitCount = 1;
                        data.loginPacket.Add(packet.buffer);
                    }
                    else if (epk.hasSplit && data.SplitId == epk.splitID && data.SplitCount < data.SplitLength)
                    {
                        data.SplitCount++;
                        data.loginPacket.Add(packet.buffer);
                    }
                }
            }
            else
            {
                if (packet.buffer[0] == OPEN_CONNECTION_REQUEST2.ID)
                {
                    OPEN_CONNECTION_REQUEST2 req2 = new OPEN_CONNECTION_REQUEST2();
                    req2.buffer = packet.buffer;
                    req2.decode();
                    data.ClientID = req2.clientID;
                }
            }

            return base.AnalyzeClient(client, packet);
        }

        public void TransferSession(ClientData data)
        {
            switch (data.SessionStatus)
            {
                case 1:
                    OPEN_CONNECTION_REQUEST1 req1 = new OPEN_CONNECTION_REQUEST1();
                    req1.mtuSize = 1447;
                    req1.encode();
                    this.sendPayload(data.client.header.Concat(req1.buffer).ToArray(), data.TargetServer);
                    break;
                case 2:
                    OPEN_CONNECTION_REQUEST2 req2 = new OPEN_CONNECTION_REQUEST2();
                    req2.clientID = data.ClientID;
                    req2.mtuSize = 1447;
                    req2.serverAddress = data.TargetServer.Address.ToString();
                    req2.serverPort = data.TargetServer.Port;
                    req2.encode();
                    this.sendPayload(data.client.header.Concat(req2.buffer).ToArray(), data.TargetServer);
                    break;
                case 3:
                    CLIENT_CONNECT_DataPacket cc = new CLIENT_CONNECT_DataPacket();
                    cc.clientID = data.ClientID;
                    cc.sendPing = new Random().Next();
                    cc.encode();
                    this.sendPayload(data.getDataPacketPayload(this.packPacket(cc.buffer, data.messageIndex++, 0x02), true), data.TargetServer);
                    break;
                case 4:
                    CLIENT_HANDSHAKE_DataPacket ch = new CLIENT_HANDSHAKE_DataPacket();
                    ch.address = data.TargetServer.Address.ToString();
                    ch.port = data.TargetServer.Port;
                    ch.systemAddresses[0] = new IPEndPoint(IPAddress.Parse("127.0.0.1"), data.client.port);
                    for (int i = 1; i < 10; i++)
                        ch.systemAddresses[i] = new IPEndPoint(IPAddress.Parse("0.0.0.0"), 0);
                    ch.sendPing = new Random().Next();
                    ch.encode();
                    this.sendPayload(data.getDataPacketPayload(this.packPacket(ch.buffer, data.messageIndex++, 0x03), true), data.TargetServer);
                    this.createLoginPacketSender(data);
                    break;
            }
        }

        public void createLoginPacketSender(ClientData data)
        {
            Task.Run(() =>
            {
                foreach (byte[] pk in data.loginPacket)
                {
                    System.Threading.Thread.Sleep(30);

                    this.sendPayload(data.client.header.Concat(pk).ToArray(), data.TargetServer);
                }
                this.SwitchServer(data.client.address, new Server(data.TargetServer));
                data.cleanSession();
            });
        }
    }
}
