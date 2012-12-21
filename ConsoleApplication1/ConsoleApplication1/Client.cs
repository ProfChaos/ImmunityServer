using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Collections;

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
                            toClient.Flush();
                            break;
                        case "username":
                            if (Username(action[1]))
                            {
                                toClient.WriteLine("username;ok");
                                toClient.Flush();
                                userdata = new User(action[1]);
                            }
                            else
                            {
                                toClient.WriteLine("username;nope");
                                toClient.Flush();
                            }
                            break;
                        case "msglobby":
                            MessageLobby(action[1]);
                            Console.WriteLine(playername+": "+action[1]);
                            break;
                        case "joinlobby":
                            if (playername != "noname")
                            {
                                toClient.WriteLine("startlobby;");
                                toClient.Flush();
                                Console.WriteLine(playername + " joined lobby " + action[1]);
                                List<string> test = (List<string>)ConsoleApplication1.Server.lobbies[action[1]];
                                test.Add(playername);
                                ConsoleApplication1.Server.lobbies[action[1]] = test;
                                ConsoleApplication1.Server.playerInLobby.Add(playername, action[1]);
                                MessageLobby("*joined lobby*");
                            }
                            else
                            {
                                toClient.WriteLine("needusername;");
                            }
                            break;
                        case "createlobby":
                            toClient.WriteLine("startlobby;");
                            toClient.Flush();
                            Console.WriteLine(playername + " started lobby " + action[1]);
                            List<string> userlist = new List<string>();
                            userlist.Add(playername);
                            ConsoleApplication1.Server.lobbies.Add(action[1], userlist);
                            ConsoleApplication1.Server.playerInLobby.Add(playername, action[1]);
                            break;
                        case "leavelobby":
                            LeaveLobby();
                            break;
                        case "listlobby":
                            Console.WriteLine(playername + " fetching lobbies");
                            toClient.WriteLine("listlobby" + FetchLobbies());
                            toClient.Flush();
                            break;
                        case "what":
                            Console.WriteLine(ConsoleApplication1.Server.playersByConnect[theClient]);
                            toClient.WriteLine("msg;Hi");
                            toClient.Flush();
                            break;
                        default:
                            break;
                    }
                    //send our message
                    //ConsoleApplication1.Server.SendMsgToAll(nickName, line);
                }
            }
            catch (Exception e44)
            {
                //Console.WriteLine(e44);
                ConsoleApplication1.Server.players.Remove(playername);
                ConsoleApplication1.Server.playersByConnect.Remove(theClient);

                LeaveLobby();
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
        private void MessageLobby(string msg)
        {
            string lobbyname = (string)ConsoleApplication1.Server.playerInLobby[playername];
            List<string> listofplayers = (List<string>)ConsoleApplication1.Server.lobbies[lobbyname];
            foreach(String user in listofplayers)
            {
                TcpClient client = (TcpClient)ConsoleApplication1.Server.players[user];
                toClient = new StreamWriter(client.GetStream());
                toClient.WriteLine("msglobby;"+playername+": "+msg);
                toClient.Flush();
            }
        }
        private string FetchLobbies()
        {
            string lobbies = "";
            foreach (DictionaryEntry entry in ConsoleApplication1.Server.lobbies)
            {
                List<string> players = (List<string>)entry.Value;
                lobbies += ";"+entry.Key + ";" + players.Count;
            }
            if (lobbies == "")
                return ";";
            else
                return lobbies;
        }
        public void LeaveLobby()
        {
            string lobby = (string)ConsoleApplication1.Server.playerInLobby[playername];
            if (lobby != null)
            {
                ConsoleApplication1.Server.playerInLobby.Remove(playername);
                Console.WriteLine(playername + " left lobby " + lobby);
                List<string> players = (List<string>)ConsoleApplication1.Server.lobbies[lobby];
                if (players.Count == 1)
                {
                    ConsoleApplication1.Server.lobbies.Remove(lobby);
                    Console.WriteLine("Lobby " + lobby + " removed");
                }
            }
        }
    }
}
