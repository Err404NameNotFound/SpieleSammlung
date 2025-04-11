using SpieleSammlung.Model.Multiplayer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SpieleSammlung.View.Sites
{
    /// <summary>
    /// Interaktionslogik für MultiplayerLobby.xaml
    /// </summary>
    public partial class MultiplayerLobby
    {
        private bool _ipOk;
        private bool _portOk;
        private bool _hostPortOk;
        private bool _nameOk;
        private readonly GameMode _mode;
        private readonly List<MultiplayerPlayer> _players;
        private int _currentPlayer;
        private readonly int _min;
        private readonly int _max;
        private int _port;
        private string _ip;
        private MpConnection _connection;
        public const char SEPARATOR = ';';
        public const string CLIENT_LATE_JOIN = "6";
        private const string PATH_LAST_IP = "lastIP.txt";

        public delegate void OnStartMatch(GameMode mode, List<MultiplayerPlayer> players, int index,
            MpConnection connection, bool rejoin);

        public event OnStartMatch StartMatch;

        public MultiplayerLobby(GameMode m, int minP, int maxP)
        {
            InitializeComponent();
            _players = new List<MultiplayerPlayer>();

            _mode = m;
            _min = minP;
            _max = maxP;
            LblMaxPlayer.Content = _max;
            LblMinPlayer.Content = _min;

            //reset
            _ipOk = false;
            _portOk = false;
            _hostPortOk = false;

            ChangeCurrentPlayer(0);
        }

        private void Got_Loaded(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus(PlayerName);
        }

        private void BtnHost_Click(object sender, RoutedEventArgs e)
        {
            GridStateDecision.Visibility = Visibility.Collapsed;
            MpViewHost.Visibility = Visibility.Visible;
            _connection = new MpConnection("host", _port);
            _connection.HostEvent += HostEvents;
            AddPlayer(PlayerName.Text, _connection.Id);
            PlayerList.SelectedIndex = 0;
            BtnStartMatch.Focus();
        }

        private void HostEvents(MultiplayerEvent e)
        {
            if (e.Type == MultiplayerEventTypes.HClientDisconnected)
            {
                RemovePlayer(e.Sender);
                _connection.SendMessage(string.Concat("2", SEPARATOR, e.Sender));
            }
            else if (e.Type == MultiplayerEventTypes.HostReceived)
            {
                string[] msgParts = e.Message.Split(';');
                switch (msgParts[0])
                {
                    case "1":
                        //1: client wants to tell his name
                        AddPlayer(msgParts[1], e.Sender);
                        string temp = string.Concat(_players[0].Name, SEPARATOR, _players[0].Id);
                        for (int i = 1; i < _players.Count; ++i)
                        {
                            temp = string.Concat(temp, SEPARATOR, _players[i].Name, SEPARATOR, _players[i].Id);
                            if (!_players[i].Id.Equals(e.Sender))
                            {
                                _connection.SendMessage(string.Concat("1", SEPARATOR, msgParts[1], SEPARATOR, e.Sender),
                                    _players[i].Id);
                            }
                        }

                        _connection.SendMessage("1" + SEPARATOR + temp, e.Sender);
                        break;
                }
            }
        }

        private void ClientEvents(MultiplayerEvent e)
        {
            if (e.Type == MultiplayerEventTypes.ClientReceived)
            {
                //1: client name or list of client names
                //2: client to be removed
                //3: start match
                string[] msgParts = e.Message.Split(SEPARATOR);
                switch (msgParts[0])
                {
                    case "1":
                        for (int i = 1; i < msgParts.Length; i += 2)
                        {
                            AddPlayer(msgParts[i], msgParts[i + 1]);
                        }

                        break;
                    case "2":
                        RemovePlayer(msgParts[1]);
                        break;
                    case "3":
                        switch (_mode)
                        {
                            case GameMode.Schafkopf:
                                _connection.ClientEvent -= ClientEvents;
                                StartMatch(GameMode.Schafkopf, _players, int.Parse(msgParts[1]), _connection, false);
                                break;
                        }

                        break;
                    case "4":
                        RenamePlayer(int.Parse(msgParts[1]), msgParts[2]);
                        break;
                    case "5":
                        SwitchIndex(int.Parse(msgParts[1]), int.Parse(msgParts[2]));
                        break;
                    case CLIENT_LATE_JOIN:
                        for (int i = 2; i < msgParts.Length; i += 2)
                        {
                            AddPlayer(msgParts[i], msgParts[i + 1]);
                        }

                        _connection.ClientEvent -= ClientEvents;
                        StartMatch(GameMode.Schafkopf, _players, int.Parse(msgParts[1]), _connection, true);
                        break;
                }
            }
            else if (e.Type == MultiplayerEventTypes.CClientConnected)
            {
                BtnStartMatch.IsEnabled = false;
                BtnStartMatch.Visibility = Visibility.Hidden;
                HostTools.Visibility = Visibility.Hidden;
                MpViewClient.Visibility = Visibility.Collapsed;
                MpViewHost.Visibility = Visibility.Visible;
                _connection.SendMessage("1" + SEPARATOR + PlayerName.Text);
                File.WriteAllText(PATH_LAST_IP, _ip + Properties.Resources.Newline + _port);
            }
            else if (e.Type == MultiplayerEventTypes.CClientDisconnected)
            {
                MpViewHost.Visibility = Visibility.Collapsed;
                MpViewClient.Visibility = Visibility.Collapsed;
                ConnectionLost.Visibility = Visibility.Visible;
            }
        }

        private void AddPlayer(string name, string id)
        {
            PlayerList.Items.Add(name);
            _players.Add(new MultiplayerPlayer(id, name));
            ChangeCurrentPlayer(_currentPlayer + 1);
        }

        private void RemovePlayer(string id)
        {
            int w = 0;
            while (w < _players.Count && !_players[w].Id.Equals(id))
            {
                ++w;
            }

            if (w == _players.Count)
            {
                //Error Handling
                /*
                string error = string.Concat("could not find ", id, " in players [(", players[0].id, ", ", players[0].id, ")");
                for (int i = 1; i < players.Count; ++i) { error = string.Concat(error, ", (", players[i].id, ", ", players[i].id, ")"); }
                connection.WriteLine(string.Concat(error, "]"));
                //*/
            }
            else
            {
                PlayerList.Items.RemoveAt(w);
                _players.RemoveAt(w);
                ChangeCurrentPlayer(_currentPlayer - 1);
            }
        }

        private void BtnJoin_Click(object sender, RoutedEventArgs e)
        {
            GridStateDecision.Visibility = Visibility.Collapsed;
            MpViewClient.Visibility = Visibility.Visible;
            Ip.Focus();
            if (File.Exists(PATH_LAST_IP))
            {
                string[] lines = File.ReadAllLines(PATH_LAST_IP);
                try
                {
                    Ip.Text = lines[0];
                    Port.Text = lines[1];
                }
                catch (Exception)
                {
                    //could probably not read the file or it had the wrong format
                }
            }
        }

        private void BtnTryJoin_Click(object sender, RoutedEventArgs e)
        {
            _connection = new MpConnection(new Random().Next(100, 999) + PlayerName.Text,
                ClientEvents, _port, _ip);
            LblError.Content = Properties.Resources.MP_ConnectionToHost;
            LblError.Visibility = Visibility.Visible;
        }

        private void CheckIfJoinPossible()
        {
            BtnTryJoin.IsEnabled = _ipOk && _portOk;
        }

        private bool IsIp(string ip)
        {
            if (IPAddress.TryParse(ip, out IPAddress address))
            {
                switch (address.AddressFamily)
                {
                    case System.Net.Sockets.AddressFamily.InterNetwork:
                        _ip = ip;
                        return true;
                    default:
                        return false;
                }
            }

            return false;
        }

        private bool IsPort(string port)
        {
            try
            {
                int number = int.Parse(port);
                _port = number;
                return number is > 0 and < 65536;
            }
            catch
            {
                return false;
            }
        }

        private void Name_TextChanged(object sender, TextChangedEventArgs e)
        {
            _nameOk = !PlayerName.Text.Equals("");
            CheckPossibleMpStates();
        }

        private void CheckPossibleMpStates()
        {
            BtnHost.IsEnabled = _nameOk && _hostPortOk;
            BtnJoin.IsEnabled = _nameOk && !_hostPortOk;
        }

        private void IP_TextChanged(object sender, TextChangedEventArgs e)
        {
            _ipOk = IsIp(Ip.Text);
            CheckIfJoinPossible();
        }

        private void Port_TextChanged(object sender, TextChangedEventArgs e)
        {
            _portOk = IsPort(Port.Text);
            CheckIfJoinPossible();
        }

        private void ChangeCurrentPlayer(int count)
        {
            _currentPlayer = count;
            LblCurPlayer.Content = count;
            if (_currentPlayer >= _min && _currentPlayer <= _max && _connection.IsHost)
            {
                BtnStartMatch.IsEnabled = true;
                BtnStartMatch.Content = Properties.Resources.MP_StartMatch;
            }
            else if (BtnStartMatch.IsEnabled)
            {
                BtnStartMatch.IsEnabled = false;
                BtnStartMatch.Content = Properties.Resources.MP_WaitForOtherPlayers;
            }
        }

        private void RenamePlayer(int index, string newName)
        {
            _players[index].Name = newName;
            PlayerList.Items[index] = newName;
            PlayerList.SelectedIndex = index;
        }

        private void SwitchIndex(int first, int second)
        {
            string temp = _players[first].Id;
            _players[first].Id = _players[second].Id;
            _players[second].Id = temp;
            temp = _players[first].Name;
            _players[first].Name = _players[second].Name;
            _players[second].Name = temp;
            PlayerList.Items[first] = _players[first].Name;
            PlayerList.Items[second] = _players[second].Name;
        }

        private void BtnStartMatch_Click(object sender, RoutedEventArgs e)
        {
            switch (_mode)
            {
                case GameMode.Schafkopf:
                    _connection.HostEvent -= HostEvents;
                    for (int i = 1; i < _players.Count; ++i)
                    {
                        _connection.SendMessage(new List<string> { "3", i.ToString() }, SEPARATOR, _players[i].Id);
                    }

                    StartMatch(GameMode.Schafkopf, _players, 0, _connection, false);
                    break;
            }
        }

        private void HostPort_TextChanged(object sender, TextChangedEventArgs e)
        {
            _hostPortOk = IsPort(HostPort.Text);
            CheckPossibleMpStates();
        }

        public void EndConnection() => _connection?.EndConnection();

        private void PlayerList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool valid = PlayerList.SelectedIndex > -1;
            BtnKickPlayer.IsEnabled = valid && !_players[PlayerList.SelectedIndex].Id.Equals("host");
            BtnMovePlayerUp.IsEnabled = PlayerList.SelectedIndex > 1;
            BtnMovePlayerDown.IsEnabled = BtnKickPlayer.IsEnabled && PlayerList.SelectedIndex < _players.Count - 1;
            BtnRenamePlayer.IsEnabled = valid;
            if (valid)
            {
                LblChangedName.Text = PlayerList.SelectedItem.ToString();
                LblChangedName.Focus();
            }
        }

        private void BtnKickPlayer_Click(object sender, RoutedEventArgs e)
        {
            _connection.DisconnectPlayer(_players[PlayerList.SelectedIndex].Id);
        }

        private void BtnRenamePlayer_Click(object sender, RoutedEventArgs e)
        {
            RenamePlayer(PlayerList.SelectedIndex, LblChangedName.Text);
            _connection.SendMessage(new List<string> { "4", PlayerList.SelectedIndex.ToString(), LblChangedName.Text },
                SEPARATOR);
        }

        private void PlayerName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return && _nameOk)
            {
                if (_hostPortOk)
                {
                    BtnHost_Click(BtnHost, null);
                }
                else
                {
                    {
                        BtnJoin_Click(BtnJoin, null);
                    }
                }
            }
        }

        private void HostPort_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return && _nameOk && _hostPortOk)
            {
                BtnHost_Click(BtnHost, null);
            }
        }

        private void LblChangedName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return && !LblChangedName.Text.Equals(""))
            {
                BtnRenamePlayer_Click(BtnRenamePlayer, null);
            }
        }

        private void IP_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return && _ipOk)
            {
                if (_portOk)
                {
                    BtnTryJoin_Click(BtnTryJoin, null);
                }
                else
                {
                    Port.Focus();
                }
            }
        }

        private void Port_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return && _portOk)
            {
                if (_ipOk)
                {
                    BtnTryJoin_Click(BtnTryJoin, null);
                }
                else
                {
                    Ip.Focus();
                }
            }
        }

        private void BtnMovePlayerUp_Click(object sender, RoutedEventArgs e)
        {
            BtnMove(PlayerList.SelectedIndex, PlayerList.SelectedIndex - 1);
        }

        private void BtnMovePlayerDown_Click(object sender, RoutedEventArgs e)
        {
            BtnMove(PlayerList.SelectedIndex, PlayerList.SelectedIndex + 1);
        }

        private void BtnMove(int first, int second)
        {
            SwitchIndex(first, second);
            _connection.SendMessage(new List<string> { "5", first.ToString(), second.ToString() }, SEPARATOR);
        }
    }
}