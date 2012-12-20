using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace ConsoleApplication1
{
    class Client
    {
        TcpClient theClient;
        User userdata;
        StreamWriter toClient;
        StreamReader fromClient;
        string playername = "noname";

        public Client(TcpClient client)
        {
            this.theClient = client;

            Thread clientThread = new Thread(new ThreadStart(startConversation));
            clientThread.Start();
        }

        public void startConversation()
        {
            Console.WriteLine("Starting conversation...");

            fromClient = new StreamReader(theClient.GetStream());

            toClient = new StreamWriter(theClient.GetStream());

            try
            {
                //set out line variable to an empty string
                string line = "";
                while (true)
                {
                    //read the curent line
                    line = fromClient.ReadLine();
                    
                    string[] action = line.Split(new string[] { ";" }, StringSplitOptions.None);
                    switch (action[0])
                    {
                        case "mes":
                            Console.WriteLine(playername+": "+action[1]);
                            toClient.WriteLine("msg;Message received");
                            break;
                        case "username":
                            if (Username(action[1]))
                            {
                                toClient.WriteLine("username;ok");
                                userdata = new User(action[1]);
                            }
                            else
                            {
                                toClient.WriteLine("username;nope");
                            }
                            break;
                        case "msglobby":
                            MessageLobby(playername, action[1]);
                            Console.WriteLine(playername+": "+action[1]);
                            break;
                        case "joinlobby":
                            toClient.WriteLine("startlobby;");
                            Console.WriteLine(playername + " joined lobby " + action[1]);
                            List<string> test = (List<string>)ConsoleApplication1.Server.lobbies[action[1]];
                            test.Add(playername);
                            ConsoleApplication1.Server.lobbies[action[1]] = test;
                            ConsoleApplication1.Server.playerInLobby.Add(playername, action[1]);
                            break;
                        case "createlobby":
                            toClient.WriteLine("startlobby;");
                            Console.WriteLine(playername + " started lobby " + action[1]);
                            List<string> userlist = new List<string>();
                            userlist.Add(playername);
                            ConsoleApplication1.Server.lobbies.Add(action[1], userlist);
                            ConsoleApplication1.Server.playerInLobby.Add(playername, action[1]);
                            break;
                        case "what":
                            Console.WriteLine(ConsoleApplication1.Server.playersByConnect[theClient]);
                            toClient.WriteLine("msg;Hi");
                            break;
                        default:
                            break;
                    }
                    toClient.Flush();
                    //send our message
                    //ConsoleApplication1.Server.SendMsgToAll(nickName, line);
                }
            }
            catch (Exception e44)
            {
                //Console.WriteLine(e44);
                ConsoleApplication1.Server.players.Remove(playername);
                ConsoleApplication1.Server.playersByConnect.Remove(theClient);
                Console.WriteLine(playername+" lost connection");
            }
        }
        private bool Username(string name)
        {
            if (ConsoleApplication1.Server.players.Contains(name))
            {
                Console.WriteLine("Username taken: " + name);
                toClient.WriteLine("nope");
                return false;
            }
            else if(playername == "noname")
            {
                playername = name;
                Console.WriteLine("Player is: " + name);
                ConsoleApplication1.Server.players.Add(playername, theClient);
                ConsoleApplication1.Server.playersByConnect.Add(theClient, playername);
                return true;
            }
            else
            {
                return false;
            }
        }
        private void MessageLobby(string player, string msg)
        {
            string lobbyname = (string)ConsoleApplication1.Server.playerInLobby[player];
            List<string> listofplayers = (List<string>)ConsoleApplication1.Server.lobbies[lobbyname];
            foreach(String user in listofplayers)
            {
                TcpClient client = (TcpClient)ConsoleApplication1.Server.players[user];
                toClient = new StreamWriter(client.GetStream());
                toClient.WriteLine("msglobby;"+player+": "+msg);
                toClient.Flush();
            }
        }
    }
}
