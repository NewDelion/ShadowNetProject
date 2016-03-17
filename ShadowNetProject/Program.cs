using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                string command = Console.ReadLine();
                if (command == "quit")
                {
                    shadow.Stop();
                    break;
                }
                else if (command == "clear")
                {
                    Console.Clear();
                }
            }

            Console.WriteLine("Press any key...");
            Console.ReadKey();
        }
    }
}
