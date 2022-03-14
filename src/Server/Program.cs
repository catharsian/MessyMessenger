using System;
using System.Threading.Tasks;

namespace Messenger_Server
{
    class Program
    {
       
        static async Task Main(string[] args)
        {

            var s1 = new Server();
            var _ = s1.ListenAsync();
            while (true)
            {
                var lol = async () =>
                {
                    var inp = Console.ReadLine();
                    return inp;
                };
                var inp = await lol();
                Handle(inp.TrimEnd().ToLower(), s1);
            }
            Console.WriteLine("Server has ended its service. Press ENTER to close the app.");
            Console.ReadLine();
            return;
        }

        static void Handle(string inp, Server sv)
        {
            switch (inp)
            {
                case "cancel":
                    sv.TryToStop();
                    Environment.Exit(0);
                    break;
            }
        }
    }

}
