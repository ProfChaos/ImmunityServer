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

        /// <summary>
        /// A console thread that will handle input comming in from the console.
        /// Server object is made.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            Console.WriteLine("Starting server");
            Thread console = new Thread(new ThreadStart(ServerConsole));
            console.Start();
            Server server = new Server();
        }

        /// <summary>
        /// The object creates all the hashtables that will hold the information about the player
        /// and the connection. Also who is in what lobby.
        /// We then start listening for players and it will not go past AcceptTcpClient()
        /// unless someone is connecting. When someone connects we create a client object.
        /// </summary>
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

        /// <summary>
        /// Takes input from the console to do different actions.
        /// </summary>
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

        /// <summary>
        /// sends out a message to all the connected parties.
        /// And will show up in the to left corner for the clients
        /// </summary>
        /// <param name="msg"></param>
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
