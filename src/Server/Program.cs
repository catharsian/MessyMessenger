using System;
using System.Threading.Tasks;
using System.Net;

namespace Messenger_Server
{
    class Program
    {
       
        static async Task Main(string[] args)
        {

            var s1 = new Server();
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
            var inps = inp.Split(' ');
            switch (inps[0])
            {
                case "cancel":
                    sv.PauseServer();
                    Environment.Exit(0);
                    break;
                case "kick":
                    if (sv.started)
                    {
                        if (inps.Length > 1)
                        {
                            sv.getUser(inps[1])?.Connection.CloseConn();
                        }
                    }
                    break;
                case "setip":
                    if (inps.Length > 1)
                    {
                        try
                        {
                            sv.ChangeIP(IPEndPoint.Parse(inps[1]));
                        }
                        catch (FormatException ex)
                        {
                            Console.WriteLine("IP was wrongly formatted. Good example: 127.0.0.1:6868");
                        }
                    }
                    break;
                case "viewip":
                    Console.WriteLine(sv.ViewIP());
                    break;
                case "pause":
                    if (sv.started)
                        sv.PauseServer();
                    else
                        Console.WriteLine("Server has not started yet.");
                    break;
                case "start":
                    sv.ContinueServer();
                    break;
            }
        }
    }

}
