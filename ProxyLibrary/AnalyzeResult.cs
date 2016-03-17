using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace ProxyLibrary
{
    public enum Command
    {
        Send = 0x01,
        Cancel = 0x02
    }

    public class AnalyzeResult
    {
        public Command result = Command.Send;
        public Packet packet;
        public bool logout = false;

        public AnalyzeResult(Command result, Packet packet, bool logout = false)
        {
            this.result = result;
            this.packet = packet;
            this.logout = logout;
        }
    }
}
