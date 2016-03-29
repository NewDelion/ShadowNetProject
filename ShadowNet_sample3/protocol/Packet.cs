using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace ShadowNet_sample3.protocol
{
    public abstract class Packet
    {
        protected int offset = 0;
        public byte[] buffer;
        public long sendTime;

        public abstract byte getID();

        protected byte[] get(int len)
        {
            if (len < 0)
            {
                this.offset = this.buffer.Length - 1;
                return new byte[0];
            }

            byte[] buffer = new byte[len];
            for (int i = 0; i < len; i++)
            {
                buffer[i] = this.buffer[this.offset++];
            }
            return buffer;
        }

        protected byte[] getAll()
        {
            return this.get();
        }

        protected byte[] get()
        {
            byte[] result = new byte[this.buffer.Length - 1];
            Buffer.BlockCopy(this.buffer, this.offset, result, 0, result.Length);
            return result;
            //return this.buffer.Skip(this.offset).Take(this.buffer.Length - 1).ToArray();
        }

        protected long getLong()
        {
            return BitConverter.ToInt64(this.get(8).Reverse().ToArray(), 0);
        }

        protected int getInt()
        {
            return BitConverter.ToInt32(this.get(4).Reverse().ToArray(), 0);
        }

        protected short getSignedShort()
        {
            return this.getShort();
        }

        protected short getShort()
        {
            return BitConverter.ToInt16(this.get(2).Reverse().ToArray(), 0);
        }

        protected int getTriad()
        {
            return BitConverter.ToInt32(this.get(3).Reverse().Concat(new byte[] { 0x00 }).ToArray(), 0);
        }

        protected int getLTriad()
        {
            return BitConverter.ToInt32(this.get(3).Concat(new byte[] { 0x00 }).ToArray(), 0);
        }

        protected byte getByte()
        {
            return this.buffer[this.offset++];
        }

        protected string getString()
        {
            return BitConverter.ToString(this.get(this.getSignedShort()));
        }

        protected IPEndPoint getAddress()
        {
            byte version = this.getByte();
            if (version == 0x04)
            {
                string address = String.Format("{0}.{1}.{2}.{3}", 0xff & ~this.getByte(), 0xff & ~this.getByte(), 0xff & ~this.getByte(), 0xff & ~this.getByte());
                int port = this.getSignedShort() & 0xffff;
                return new IPEndPoint(IPAddress.Parse(address), port);
            }
            else
            {
                return null;
            }
        }

        protected bool feof()
        {
            return !(this.offset >= 0 && this.offset + 1 <= this.buffer.Length);
        }

        protected void put(byte[] b)
        {
            this.buffer = this.buffer.Concat(b).ToArray();
        }

        protected void putLong(long v)
        {
            this.put(BitConverter.GetBytes(v).Reverse().ToArray());
        }

        protected void putInt(int v)
        {
            this.put(BitConverter.GetBytes(v).Reverse().ToArray());
        }

        protected void putShort(short v)
        {
            this.put(BitConverter.GetBytes(v).Reverse().ToArray());
        }

        protected void putSignedShort(short v)
        {
            this.put(BitConverter.GetBytes(v & 0xffff).Reverse().ToArray());
        }

        protected void putTriad(int v)
        {
            this.put(BitConverter.GetBytes(v).Reverse().Skip(1).ToArray());
        }

        protected void putLTriad(int v)
        {
            this.put(BitConverter.GetBytes(v).Take(3).ToArray());
        }

        protected void putByte(byte b)
        {
            this.buffer = this.buffer.Concat(new byte[] { b }).ToArray();
        }

        protected void putString(string str)
        {
            byte[] b = Encoding.UTF8.GetBytes(str);
            this.putShort((short)b.Length);
            this.put(b);
        }

        protected void putAddress(string addr, int port)
        {
            this.putAddress(addr, port, 0x04);
        }

        protected void putAddress(string addr, int port, byte version)
        {
            this.putByte(version);
            if (version == 0x04)
            {
                for (int i = 0; i < addr.Length; i++)
                    this.putByte((byte)(0xff & ~Convert.ToInt32(addr[i])));
                this.putShort((short)port);
            }
            else
            {

            }
        }

        protected void putAddress(IPEndPoint address)
        {
            this.putAddress(address.Address.ToString(), address.Port);
        }

        public void encode()
        {
            this.buffer = new byte[] { getID() };
        }

        public void decode()
        {
            this.offset = 1;
        }

        public Packet clean()
        {
            this.buffer = null;
            this.offset = 0;
            this.sendTime = -1;
            return this;
        }
    }
}
