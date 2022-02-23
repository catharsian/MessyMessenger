using System;

namespace Messenger_Server
{
    class Program
    {
       
        static void Main(string[] args)
        {
            Server s1 = new Server();
            Console.WriteLine("Server has ended its service. Press ENTER to close the app.");
            Console.ReadLine();
        }
    }

}
