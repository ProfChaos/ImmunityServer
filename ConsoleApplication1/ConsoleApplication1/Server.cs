using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Threading;
using System.IO;

namespace ConsoleApplication1
{
    class Server
    {
        public static Hashtable players;
        public static Hashtable playersByConnect;
        public static Hashtable lobbies;
        public static Hashtable playerInLobby;

        TcpListener listener;

        static void Main(string[] args)
        {
            Console.WriteLine("Starting server");
            Thread console = new Thread(new ThreadStart(ServerConsole));
            console.Start();
            Server server = new Server();
        }

        public Server()
        {
            players = new Hashtable(50);
            playersByConnect = new Hashtable(50);
            lobbies = new Hashtable(50);
            playerInLobby = new Hashtable(50);

            listener = new TcpListener(IPAddress.Any, 7707);

            Console.WriteLine("Listening for players..");
            while(true)
            {
                listener.Start();

                if (listener.Pending())
                {
                    TcpClient connection = listener.AcceptTcpClient();
                    Console.WriteLine("Someone connected");

                    Client client = new Client(connection);
                }
            }
        }
        public static void ServerConsole()
        {
            bool running = true;
            while (running)
            {
                string what = Console.ReadLine();

                switch (what)
                {
                    case "list":
                        Console.WriteLine(players.Count);
                        break;
                    case "stop":
                        running = false;
                        break;
                    case "announce":
                        what = null;
                        Console.WriteLine("What?");
                        what = Console.ReadLine();
                        Announce("sysmsg;" + what);
                        break;
                    case "lobby":
                        what = null;
                        Console.WriteLine("What?");
                        what = Console.ReadLine();
                        Announce("msglobby;" + what);
                        break;
                    default:
                        Console.WriteLine("No such command");
                        break;
                }
            }
        }

        public static void Announce(string msg)
        {
            Console.WriteLine("Starting to send announcement");
            StreamWriter writer;

            TcpClient[] clients = new TcpClient[players.Count];
            players.Values.CopyTo(clients, 0);

            for (int i = 0; i < players.Count; i++)
            {
                try
                {
                    writer = new StreamWriter(clients[i].GetStream());
                    writer.WriteLine(msg);
                    writer.Flush();
                    writer = null;
                }
                catch (Exception e)
                {
                    Console.WriteLine(players[i]+" Disconnected");
                }
            }
        }
    }
}
