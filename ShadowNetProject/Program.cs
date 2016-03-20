using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ShadowNetProject
{
    class Program
    {
        static void Main(string[] args)
        {
            ShadowNet shadow = new ShadowNet();
            shadow.addServer(new ProxyLibrary.Server("127.0.0.1", 19133));
            shadow.addServer(new ProxyLibrary.Server("127.0.0.1", 19134));
            shadow.Start();

            while (true)
            {
                string[] command = Console.ReadLine().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (command.Length == 0)
                    continue;
                if (command[0] == "quit")
                {
                    shadow.Stop();
                    break;
                }
                else if (command[0] == "clear")
                {
                    Console.Clear();
                }
                else if (command[0] == "addserver" || command[0] == "adds")
                {
                    shadow.addServer(new ProxyLibrary.Server(command[1], int.Parse(command[2])));
                    Console.WriteLine("サーバリストに{{{0}:{1}}}を追加しました。", command[1], command[2]);
                }
                else if (command[0] == "listserver" || command[0] == "ls")
                {
                    try
                    {
                        shadow.rwlock_ServerList.AcquireReaderLock(Timeout.Infinite);
                        foreach (ProxyLibrary.Server server in shadow.ServerList)
                            Console.WriteLine("{0}:{1}", server.ip, server.port);
                    }
                    finally
                    {
                        shadow.rwlock_ServerList.ReleaseLock();
                    }
                }
                else if (command[0] == "listclient" || command[0] == "lc")
                {
                    try
                    {
                        shadow.rwlock_ClientList.AcquireReaderLock(Timeout.Infinite);
                        foreach (ProxyLibrary.Client data in shadow.ClientList)
                        {
                            Console.WriteLine("Address: {0}:{1}", data.ip, data.port);
                            Console.WriteLine(">> Server : {0}:{1}", data.ConnectingServer.ip, data.ConnectingServer.port);
                            Console.WriteLine();
                        }
                    }
                    finally
                    {
                        shadow.rwlock_ClientList.ReleaseLock();
                    }
                }
            }

            Console.WriteLine("Press any key...");
            Console.ReadKey();
        }
    }
}
