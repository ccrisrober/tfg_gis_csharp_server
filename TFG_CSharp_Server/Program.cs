// Copyright (c) 2015, maldicion069 (Cristian Rodríguez) <ccrisrober@gmail.con>
//
// Permission to use, copy, modify, and/or distribute this software for any
// purpose with or without fee is hereby granted, provided that the above
// copyright notice and this permission notice appear in all copies.
//
// THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
// WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
// MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR
// ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
// WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN
// ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF
// OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.package com.example

using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using LitJson;
using System.IO;

using ConsoleApplication8;
using System.Collections.Concurrent;

namespace Multi_Server
{
    class Program
    {

        private static Socket _serverSocket;
        private static readonly List<Socket> _clientSockets = new List<Socket>();

        private static ConcurrentDictionary<string, ObjectUser> _positions = new ConcurrentDictionary<string, ObjectUser>();
        private static List<Map> _maps = new List<Map>();
        private static readonly ConcurrentDictionary<String, KeyObject> _keys = new ConcurrentDictionary<String, KeyObject>();

        private const int _BUFFER_SIZE = 8081;
        private const int _PORT = 8089;
        private static readonly byte[] _buffer = new byte[_BUFFER_SIZE];

        private static readonly Random _rnd = new Random();

        private static bool IsGame = false;

        private static ConcurrentDictionary<int, KeyObject> RealObjects = new ConcurrentDictionary<int, KeyObject>();

        static void Main()
        {
            // TODO (Seguro que hay una mejor forma para arreglar lo de los decimales)
            //Panel de Control / Idioma / Cambiar formate de fecha .. / Configuración adicional / Pasar "," a "." en los decimales


            Console.WriteLine("[S/s] Game Mode / [_] Test Mode");
            String opc = Console.ReadLine();
            if (opc.ToLower().CompareTo("s") == 0)
            {
                IsGame = true;
            }

            if (IsGame)
            {
                Console.WriteLine("Game Mode");
            }
            else
            {
                Console.WriteLine("Test Mode");
            }

            // load keys
            _keys.TryAdd("Red", new KeyObject(1, 5 * 64, 5 * 64, "Red"));
            _keys.TryAdd("Blue", new KeyObject(2, 6 * 64, 5 * 64, "Blue"));
            _keys.TryAdd("Yellow", new KeyObject(3, 7 * 64, 5 * 64, "Yellow"));
            _keys.TryAdd("Green", new KeyObject(4, 8 * 64, 5 * 64, "Green"));


            // read file
            string[] readText = File.ReadAllLines(@"data.json");
            string json_ = "";
            foreach (string s in readText)
            {
                json_ += s;
            }
            JsonData data = JsonMapper.ToObject(json_);

            int id = (int)data["id"];
            int width = (int)data["width"];
            int height = (int)data["height"];
            JsonData k_aux = data["keys"];
            JsonData m_aux = data["map"];

            string map = "";

            int k_size = k_aux.Count;
            int m_size = m_aux.Count;

            KeyObject[] keys_map = new KeyObject[k_size];

            for (int i = 0; i < k_size; i++)
            {
                keys_map[i] = _keys[k_aux[i].ToString()];
                RealObjects.TryAdd(keys_map[i].Id, keys_map[i]);
            }

            for (int i = 0; i < m_size; i++)
            {
                map += m_aux[i].ToString();
            }
            // load map
            _maps.Add(new Map(id, map, width, height, ref keys_map));

            SetupServer();
            Console.ReadLine(); // When we press enter close everything
            CloseAllSockets();
        }

        private static void SetupServer()
        {
            Console.WriteLine("Setting up server...");
            _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _serverSocket.Bind(new IPEndPoint(IPAddress.Any, _PORT));
            _serverSocket.Listen(5);    // Tiempo delay
            _serverSocket.BeginAccept(AcceptCallback, null);    // Ejecutamos AcceptCallback. No hay CallBack al finalizar
            Console.WriteLine("Server init complete");
        }

        /// <summary>
        /// Close all connected client (we do not need to shutdown the server socket as its connections
        /// are already closed with the clients)
        /// </summary>
        private static void CloseAllSockets()
        {
            foreach (Socket socket in _clientSockets)
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }

            _serverSocket.Close();
        }

        private static void AcceptCallback(IAsyncResult AR)
        {
            Socket socket;

            try
            {
                socket = _serverSocket.EndAccept(AR);
            }
            catch (ObjectDisposedException) // Se ha producido un error o se ha cerrado el socket
            {
                return;
            }
            _clientSockets.Add(socket);
            socket.BeginReceive(_buffer, 0, _BUFFER_SIZE, SocketFlags.None, ReceiveCallback, socket);

            Console.WriteLine("Client connected, waiting for request...");
            //http://codeidol.com/csharp/csharp-network/Asynchronous-Sockets/Using-Asynchronous-Sockets/
            _serverSocket.BeginAccept(AcceptCallback, null);
        }

        protected static int RandomInRange(int min, int max) {
            return _rnd.Next(max) + min;
        }

        protected static void SendFightToAnotherClient(int emisor, int receiver)
        {
            byte[] retOthers = Encoding.ASCII.GetBytes(@"{""Action"": ""hide"", ""Ids"": [" + emisor + @", " + receiver + "]}");

            // Save Dice roll value from emisor_id
            _positions[emisor.ToString()].RollDice = RandomInRange(1, 6);
            int port = -1;
            foreach (Socket socket in _clientSockets)
            {
                port = ((System.Net.IPEndPoint)socket.RemoteEndPoint).Port;
                if (port != receiver)
                {
                    byte[] ret = Encoding.ASCII.GetBytes(@"{""Action"": ""fight"", ""Id_enemy"": " + emisor + ")");
                    _positions[receiver.ToString()].RollDice = RandomInRange(1, 6);
                    socket.Send(ret);
                }
                else if (port != emisor)
                {
                    socket.Send(retOthers);
                }
            }
        }

        protected static void SendDiePlayerAndWinnerToShow(ref Socket s, int emisor, int receiver)
        {
            int winner = -1;
            int valueC = -1;
            int valueE = -1;

            if (!_positions.ContainsKey(emisor.ToString()))
            {
                winner = receiver;
                valueC = _positions[receiver.ToString()].RollDice;
            }
            else if (!_positions.ContainsKey(receiver.ToString()))
            {
                winner = emisor;
                valueE = _positions[emisor.ToString()].RollDice;
            }
            else if (_positions[emisor.ToString()].RollDice > _positions[receiver.ToString()].RollDice)
            {
                winner = emisor;
                valueC = _positions[receiver.ToString()].RollDice;
                valueE = _positions[emisor.ToString()].RollDice;
            }
            else if (_positions[receiver.ToString()].RollDice > _positions[emisor.ToString()].RollDice)
            {
                winner = receiver;
                valueC = _positions[receiver.ToString()].RollDice;
                valueE = _positions[emisor.ToString()].RollDice;
            }

            string ret = @"{""Action"": ""finish"", ""ValueClient"": " + valueC + @", ""ValueEnemy"": " + valueE + @", ""Winner"": " + winner + "}";
            s.Send(Encoding.ASCII.GetBytes(ret));
        }

        private static void ReceiveCallback(IAsyncResult AR)
        {
            Socket current = (Socket)AR.AsyncState;


            int port = ((System.Net.IPEndPoint)current.RemoteEndPoint).Port;

            int received;

            try
            {
                received = current.EndReceive(AR);
            }
            catch (SocketException)
            {
                Console.WriteLine("Client forcefully disconnected");
                current.Close();
                int idx = _clientSockets.IndexOf(current);
                ObjectUser ou;
                _positions.TryRemove(port.ToString(), out ou);
                _clientSockets.Remove(current);
                return;
            }

            byte[] recBuf = new byte[received];
            Array.Copy(_buffer, recBuf, received);
            string text = Encoding.ASCII.GetString(recBuf);

            Boolean send = true;

            try
            {
                Console.WriteLine(text);
                JsonData json = JsonMapper.ToObject(text);
                Console.WriteLine(json["Action"]);
                string action = json["Action"].ToString();
                if (action.CompareTo("initWName") == 0)
                {
                    // Add socket to positions!!
                    ObjectUser user = new ObjectUser(port, 320, 320);

                    // Send map to socket
                    var javaScriptSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();

                    String json_map = @"{""Action"": ""sendMap"", ""Map"": " + javaScriptSerializer.Serialize(_maps[0]);
                    json_map += @", ""X"": " + user.PosX + @", ""Y"": " + user.PosY + @", ""Id"": " + user.Id + @", ""Users"":";

                    json_map += javaScriptSerializer.Serialize(_positions);

                    json_map += "}\n";

                    _positions.TryAdd(port.ToString(), user);

                    current.Send(Encoding.ASCII.GetBytes(json_map));

                    text = @"{""Action"":""new"", ""Id"": " + user.Id + @", ""PosX"":" + user.PosX + @", ""PosY"":" + user.PosY + "}";
                }
                else if (action.CompareTo("move") == 0)
                {
                    float px = json["Pos"]["X"].GetFloat();
                    float py = json["Pos"]["Y"].GetFloat();
                    _positions[port.ToString()].SetPosition(px, py);
                    if (!IsGame)
                    {
                        /*byte[] data = Encoding.ASCII.GetBytes(text);
                        current.Send(data);*/

                        var javaScriptSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                        current.Send(Encoding.ASCII.GetBytes(javaScriptSerializer.Serialize(_positions[port.ToString()])+"\n"));
                    }
                }
                else if (action.CompareTo("position") == 0)
                {
                    // NOT-TODO: Not used now
                    var javaScriptSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                    current.Send(Encoding.ASCII.GetBytes(javaScriptSerializer.Serialize(_positions[port.ToString()])));
                    return;
                }
                else if (action.CompareTo("fight") == 0)
                {
                    SendFightToAnotherClient(port, json["Id_enemy"].GetInt());
                    return;
                }
                else if (action.CompareTo("finishBattle") == 0)
                {
                    SendDiePlayerAndWinnerToShow(ref current, port, json["Id_enemy"].GetInt());
                    return;
                }
                else if (action.CompareTo("getObj") == 0)
                {
                    KeyObject ko = _maps[0].RemoveKey(json["Id_obj"].GetInt());

                    _positions[json["Id_user"].GetInt().ToString()].AddObject(ko.Id);
                    text = @"{""Action"": ""remObj"", ""Id_obj"": " + ko.Id + "}";
                }
                else if (action.CompareTo("freeObj") == 0)
                {
                    var obj = json["Obj"];
                    KeyObject ko = RealObjects[obj["Id_obj"].GetInt()];
                    ko.PosX = obj["PosX"].GetFloat();
                    ko.PosY = obj["PosY"].GetFloat();
                    _maps[0].AddKey(ko);
                    _positions[json["Id_user"].GetInt().ToString()].RemoveObject(ko.Id);
                    var javaScriptSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();

                    text = @"{""Action"": ""addObj"", ""Obj"": " + javaScriptSerializer.Serialize(ko) + "}";
                }
                else if (action.CompareTo("exit")== 0)
                {
                    Console.WriteLine("Cortando");
                    int idx = _clientSockets.IndexOf(current);
                    ObjectUser ou;
                    _positions.TryRemove(idx.ToString(), out ou);
                    _clientSockets.Remove(current);

                    if (!IsGame)
                    {
                        text = @"{""Action"":""exit"", ""Id"": ""Me""}\n";
                        current.Send(Encoding.ASCII.GetBytes(text));
                    }
                    current.Close();
                    send = false;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error : " + e);
            }

            if (IsGame)
            {
                byte[] data = Encoding.ASCII.GetBytes(text);
                foreach (Socket socket in _clientSockets)
                {
                    if (socket != current)
                    {
                        socket.Send(data);
                    }
                }
            }
            if (send)
            {
                current.BeginReceive(_buffer, 0, _BUFFER_SIZE, SocketFlags.None, ReceiveCallback, current);
            }
        }
    }
}
