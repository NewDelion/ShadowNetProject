using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace ProxyLibrary
{
    public class Header
    {
        public string ip;
        public int port;
        public IPEndPoint address;

        public Header(string ip, int port)
        {
            this.ip = ip;
            this.port = port;
            this.address = new IPEndPoint(IPAddress.Parse(ip), port);
        }

        public Header(IPEndPoint address)
        {
            this.address = address;
            this.ip = address.Address.ToString();
            this.port = address.Port;
        }

        public string ToString()
        {
            return string.Format("{0}:{1}", this.ip, this.port);
        }

        public byte[] encode()
        {
            byte[] result = new byte[6];
            string[] ipex = this.ip.Split('.');
            result[0] = BitConverter.GetBytes(~int.Parse(ipex[0]) & 0xff)[0];
            result[1] = BitConverter.GetBytes(~int.Parse(ipex[1]) & 0xff)[0];
            result[2] = BitConverter.GetBytes(~int.Parse(ipex[2]) & 0xff)[0];
            result[3] = BitConverter.GetBytes(~int.Parse(ipex[3]) & 0xff)[0];
            byte[] portbyte = BitConverter.GetBytes((short)this.port).Reverse().ToArray();
            result[4] = portbyte[0];
            result[5] = portbyte[1];
            return result;
        }

        public static byte[] encode(Header header)
        {
            byte[] result = new byte[6];
            string[] ipex = header.ip.Split('.');
            result[0] = BitConverter.GetBytes(~int.Parse(ipex[0]) & 0xff)[0];
            result[1] = BitConverter.GetBytes(~int.Parse(ipex[1]) & 0xff)[0];
            result[2] = BitConverter.GetBytes(~int.Parse(ipex[2]) & 0xff)[0];
            result[3] = BitConverter.GetBytes(~int.Parse(ipex[3]) & 0xff)[0];
            byte[] portbyte = BitConverter.GetBytes((short)header.port).Reverse().ToArray();
            result[4] = portbyte[0];
            result[5] = portbyte[1];
            return result;
        }

        public static byte[] encode(string ip, int port)
        {
            byte[] result = new byte[6];
            string[] ipex = ip.Split('.');
            result[0] = BitConverter.GetBytes(~int.Parse(ipex[0]) & 0xff)[0];
            result[1] = BitConverter.GetBytes(~int.Parse(ipex[1]) & 0xff)[0];
            result[2] = BitConverter.GetBytes(~int.Parse(ipex[2]) & 0xff)[0];
            result[3] = BitConverter.GetBytes(~int.Parse(ipex[3]) & 0xff)[0];
            byte[] portbyte = BitConverter.GetBytes((short)port).Reverse().ToArray();
            result[4] = portbyte[0];
            result[5] = portbyte[1];
            return result;
        }

        public static Header decode(byte[] binary)
        {
            int x0 = 0xff & ~(int)BitConverter.ToInt16(new byte[] { binary[0], 0x00 }, 0);
            int x1 = 0xff & ~(int)BitConverter.ToInt16(new byte[] { binary[1], 0x00 }, 0);
            int x2 = 0xff & ~(int)BitConverter.ToInt16(new byte[] { binary[2], 0x00 }, 0);
            int x3 = 0xff & ~(int)BitConverter.ToInt16(new byte[] { binary[3], 0x00 }, 0);
            string ip = String.Format("{0}.{1}.{2}.{3}", x0, x1, x2, x3);
            short port = BitConverter.ToInt16(new byte[] { binary[5], binary[4] }, 0);
            return new Header(ip, port);
        }
    }
}
