﻿using NetComm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SpieleSammlung.Properties;

namespace SpieleSammlung.Model.Multiplayer
{
    public class MpConnection
    {
        private Host _host;
        private Client _client;
        public string Id { get; }

        public readonly List<MultiplayerPlayer> lostClients;
        private readonly List<MultiplayerPlayer> _activeClients;

        private readonly string _path;
        private const string pathHost = "host.txt";

        public delegate void OnClientEventHandler(MultiplayerEvent e);

        public event OnClientEventHandler ClientEvent;

        public delegate void OnHostEventHandler(MultiplayerEvent e);

        public event OnHostEventHandler HostEvent;

        private MultiplayerEvent _mpEvent;

        public bool IsHost { get; }

        public bool IsConnected => _client.isConnected;
        public bool IsConnecting { get; private set; }


        public MpConnection(string player, int port)
        {
            Id = player;
            IsHost = true;
            _host = new Host(port);
            _host.StartConnection();
            _host.onConnection += Server_onConnection;
            _host.lostConnection += Server_lostConnection;
            _host.DataReceived += Server_DataReceived;
            //Speeding up the connection
            _host.SendBufferSize = 400;
            _host.ReceiveBufferSize = 50;
            _host.NoDelay = true;
            lostClients = new List<MultiplayerPlayer>();
            _activeClients = new List<MultiplayerPlayer>();
            if (File.Exists(pathHost))
            {
                int n = 1;
                while (File.Exists("./LogMP/host" + n + ".txt")) ++n;
                _path = "./LogMP/host" + n + ".txt";
            }
            else
            {
                _path = pathHost;
            }
        }

        public MpConnection(string player, OnClientEventHandler handler, int port, string ip)
        {
            Id = player;
            IsHost = false;
            _path = string.Concat("./LogMP/client_", Id, ".txt");
            _client = new Client();
            _client.Connected += Client_Connected;
            _client.Disconnected += Client_Disconnected;
            _client.DataReceived += Client_DataReceived;
            //client.SendBufferSize = 400;
            //client.ReceiveBufferSize = 50;
            //client.NoDelay = true;
            ClientEvent += handler;
            IsConnecting = true;
            _client.Connect(ip, port, Id);
        }

        private static string ConvertBytesToString(byte[] bytes) => Encoding.ASCII.GetString(bytes);

        private static byte[] ConvertStringToBytes(string str) => Encoding.ASCII.GetBytes(str);

        private void Server_onConnection(string id)
        {
            _activeClients.Add(new MultiplayerPlayer(id, id.Substring(3, id.Length - 3)));
            int i = 0;
            while (i < lostClients.Count)
            {
                if (id.IndexOf(lostClients[i].Name) == 3)
                {
                    _mpEvent = new MultiplayerEvent(MultiplayerEventTypes.HClientReConnected, id, lostClients[i].Name);
                    EventLog();
                    HostEvent?.Invoke(_mpEvent);
                    lostClients.RemoveAt(i);
                    return;
                }

                ++i;
            }

            _mpEvent = new MultiplayerEvent(MultiplayerEventTypes.HClientConnected, id);
            EventLog();
            HostEvent?.Invoke(_mpEvent);
        }

        private void Server_lostConnection(string id)
        {
            int i = 0;
            while (!_activeClients[i].Id.Equals(id)) ++i;

            lostClients.Add(_activeClients[i]);
            _activeClients.RemoveAt(i);
            _mpEvent = new MultiplayerEvent(MultiplayerEventTypes.HClientDisconnected, id);
            EventLog();
            HostEvent?.Invoke(_mpEvent);
        }

        public void SendMessage(string message, string id = null)
        {
            WriteLine(string.Concat("sending -> ", id, " -> ", message));
            if (_host == null) _client.SendData(ConvertStringToBytes(message));
            else if (id == null) _host.Brodcast(ConvertStringToBytes(message));
            else _host.SendData(id, ConvertStringToBytes(message));
        }

        public void SendMessage(IEnumerable<string> values, char separator, string id = null)
        {
            SendMessage(string.Join(separator.ToString(), values), id);
        }


        private void Server_DataReceived(string id, byte[] data)
        {
            _mpEvent = new MultiplayerEvent(MultiplayerEventTypes.HostReceived, id, ConvertBytesToString(data));
            EventLog();
            HostEvent?.Invoke(_mpEvent);
        }

        private void Client_Connected()
        {
            IsConnecting = false;
            _mpEvent = new MultiplayerEvent(MultiplayerEventTypes.CClientConnected);
            EventLog();
            ClientEvent?.Invoke(_mpEvent);
        }

        private void Client_Disconnected()
        {
            IsConnecting = false;
            _mpEvent = new MultiplayerEvent(MultiplayerEventTypes.CClientDisconnected);
            EventLog();
            ClientEvent?.Invoke(_mpEvent);
        }

        private void Client_DataReceived(byte[] data, string id)
        {
            _mpEvent = new MultiplayerEvent(MultiplayerEventTypes.ClientReceived, null, ConvertBytesToString(data));
            EventLog();
            ClientEvent?.Invoke(_mpEvent);
        }

        public void DisconnectPlayer(string id) => _host.DisconnectUser(id);

        public void EndConnection()
        {
            if (_host == null)
            {
                _client.Disconnect();
                _client.Connected -= Client_Connected;
                _client.Disconnected -= Client_Disconnected;
                _client.DataReceived -= Client_DataReceived;
                _client = null;
            }
            else
            {
                foreach (var user in _host.Users)
                {
                    _host.DisconnectUser(user);
                }

                _host.CloseConnection();
                _host.onConnection -= Server_onConnection;
                _host.lostConnection -= Server_lostConnection;
                _host.DataReceived -= Server_DataReceived;
                _host = null;
            }
        }

        private void EventLog() =>
            WriteLine(string.Concat(_mpEvent.Type, " -> ", _mpEvent.Sender, " -> ", _mpEvent.Message));

        public void WriteLine(string line) => File.AppendAllText(_path, line + Resources.Newline);

        public void RedirectClientMessage(string message, string id)
        {
            if (_host == null) throw new Exception("Clients können nicht redirecten");

            foreach (string user in _host.Users.Where(s => !s.Equals(id)))
            {
                _host.SendData(user, ConvertStringToBytes(message));
            }
        }
    }
}