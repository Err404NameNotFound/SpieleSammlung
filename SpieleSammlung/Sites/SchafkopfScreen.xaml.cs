using SpieleSammlung.Model.Multiplayer;
using SpieleSammlung.Model.Schafkopf;
using SpieleSammlung.UserControls;
using SpieleSammlung.Windows;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using SpieleSammlung.UserControls.Schafkopf;

namespace SpieleSammlung.Sites
{
    /// <summary>
    /// Interaktionslogik für SchafkopfScreen.xaml
    /// </summary>
    public partial class SchafkopfScreen : UserControl
    {
        #region Events

        public delegate void OnQuitMatch();

        public event OnQuitMatch QuitMatch;

        public delegate void OnLostConnectionToHost();

        public event OnLostConnectionToHost LostConnectionToHost; //TODO: this should probably have the other type

        #endregion

        #region Quit Handling

        public bool QuitAfterMatch { get; private set; }
        public bool CanQuitNow { get; private set; }

        public void InvertQuitAfterMatch() => QuitAfterMatch = !QuitAfterMatch;

        #endregion

        #region Member und Konstanten

        private bool _isSpectating;
        private bool _allPlayerSeeAllCards;

        private bool AllPlayerSeeAllCards
        {
            get
            {
                if (!_allPlayerSeeAllCards)
                {
                    int i = 0;
                    while (i < 4)
                    {
                        if (_playerInfos[i].State.Equals(SkPlayerInfo.STATE_AUFSTELLEN))
                        {
                            break;
                        }

                        ++i;
                    }

                    _allPlayerSeeAllCards = i == 4;
                }

                return _allPlayerSeeAllCards;
            }
        }

        private readonly MpConnection _connection;
        private readonly SchafkopfMatch _match;
        private readonly int _playerIndex;

        private int PlayerIndexCurRound => _match.Players[_playerIndex].Number;

        private const char SEPARATOR_REJOIN_INFO = '|';
        private const string SEPARATOR_REJOIN_INFO_STRING = "|";
        private const char SEPARATOR = ';';
        private const string CODE_HOST_READY_FOR_INFO = "o", CODE_HOST_READY_TO_START = "p";

        private const string codeClientKontraReInvalid = "v",
            codeClientInfoRejoin = "x",
            codeClientShuffledCards = "y",
            codeClientLostConnection = "z";

        private const string codeViewCard = "a",
            codeGameMode = "b",
            codeKontraRe = "c",
            codePlayCard = "d",
            codeContinue = "e";

        #endregion

        #region Deklaration von Listen an UI Elemente

        private readonly List<SkPlayerInfo> _playerInfos;
        private readonly List<Label> _lblPlayerSummaryNames;
        private readonly List<Label> _lblPlayerSummaryPoints;
        private readonly List<ColumnDefinition> _gridColsSummary;
        private readonly List<CheckView> _cvNextMatch;
        private readonly List<CardHolder> _chSpectate;
        private readonly List<SkPlayerInfo> _playerInfosSpectate;
        private SchafkopfPoints _points;
        private Grid _gridBeforeConnectionError;
        private StichView _lastStichView;

        #endregion

        public SchafkopfScreen(List<MultiplayerPlayer> players, int index, MpConnection connection, bool reJoin = false)
        {
            InitializeComponent();

            //UI Playing
            _playerInfos = new List<SkPlayerInfo>
                { VisualPlayer, VisualPlayerLeft, VisualPlayerTop, VisualPlayerRight };
            ModeSelector.ColorChanged += ModeSelector_ColorChanged;
            ModeSelector.ModeSelected += ModeSelector_ModeSelected;
            //UI Summary
            _lblPlayerSummaryNames = new List<Label>
            {
                LblPlayer1Review, LblPlayer2Review, LblPlayer3Review, LblPlayer4Review, LblPlayer5Review,
                LblPlayer6Review, LblPlayer7Review
            };
            _lblPlayerSummaryPoints = new List<Label>
            {
                LblPlayer1Points, LblPlayer2Points, LblPlayer3Points, LblPlayer4Points, LblPlayer5Points,
                LblPlayer6Points, LblPlayer7Points
            };
            _cvNextMatch = new List<CheckView>
                { CvPlayer1, CvPlayer2, CvPlayer3, CvPlayer4, CvPlayer5, CvPlayer6, CvPlayer7 };
            _gridColsSummary = new List<ColumnDefinition>
            {
                SummaryColumnP1, SummaryColumnP2, SummaryColumnP3, SummaryColumnP4, SummaryColumnP5, SummaryColumnP6,
                SummaryColumnP7
            };
            //UI Spectate
            _chSpectate = new List<CardHolder> { ChPlayer1, ChPlayer2, ChPlayer3, ChPlayer4 };
            for (int i = 0; i < 4; ++i)
            {
                _chSpectate[i].BtnAufstellen.IsEnabled = _chSpectate[i].BtnShowRest.IsEnabled = false;
            }

            _playerInfosSpectate = new List<SkPlayerInfo> { PlayerInfo1, PlayerInfo2, PlayerInfo3, PlayerInfo4 };


            //Data
            _connection = connection;
            _playerIndex = index;
            if (connection.IsHost) connection.HostEvent += HostEvents;
            else connection.ClientEvent += ClientEvents;

            _match = new SchafkopfMatch(players);

            CanQuitNow = true;
            QuitAfterMatch = false;
            if (connection.IsHost)
            {
                ShuffleCards(false);
            }
            else if (reJoin)
            {
                SendMessage(new List<string> { CODE_HOST_READY_FOR_INFO });
            }
        }

        #region Connection

        private void HostEvents(MultiplayerEvent e)
        {
            switch (e.Type)
            {
                case MultiplayerEventTypes.HClientConnected:
                    _connection.DisconnectPlayer(e.Sender); //client tries to join into running match               
                    break;
                case MultiplayerEventTypes.HClientReConnected:
                {
                    int w = 0;
                    while (!e.Message.Equals(_match.Players[w].name)) ++w;
                    List<string> message = new List<string> { MultiplayerLobby.CLIENT_LATE_JOIN, w.ToString() };
                    foreach (var player in _match.Players)
                    {
                        message.Add(player.name);
                        message.Add(player.id);
                    }

                    _connection.SendMessage(message, MultiplayerLobby.SEPARATOR, e.Sender);
                    break;
                }
                case MultiplayerEventTypes.HClientDisconnected:
                {
                    int i = 0;
                    while (i < _match.Players.Count)
                    {
                        if (e.Sender.Equals(_match.Players[i].id))
                        {
                            if (GridRoundSummary.Visibility == Visibility.Visible)
                            {
                                _connection.lostClients.Clear();
                            }

                            ApplyConnectionToUserLost(_connection.lostClients.Count, i,
                                MultiplayerPlayerState.LeftMatch,
                                "");
                            SendMessage(new List<string>
                            {
                                codeClientLostConnection, _connection.lostClients.Count.ToString(), i.ToString(),
                                MultiplayerPlayerState.LeftMatch.ToString(), ""
                            });
                            break;
                        }

                        ++i;
                    }

                    break;
                }
                case MultiplayerEventTypes.HostReceived:
                {
                    bool redirect = true;
                    bool newRound = false;
                    bool newGame = false;
                    bool playCard = true;
                    string[] msgParts = e.Message.Split(SEPARATOR);
                    switch (msgParts[0])
                    {
                        case CODE_HOST_READY_FOR_INFO:
                            redirect = false;
                            SendMessage(GenerateRejoinInfo(), e.Sender);
                            break;
                        case CODE_HOST_READY_TO_START:
                            redirect = false;
                            ApplyConnectionToUserLost(_connection.lostClients.Count, int.Parse(msgParts[1]),
                                MultiplayerPlayerState.Active, e.Sender);
                            SendMessage(new List<string>
                            {
                                codeClientLostConnection, _connection.lostClients.Count.ToString(), msgParts[1],
                                MultiplayerPlayerState.Active.ToString(), e.Sender
                            });
                            break;
                        case codeViewCard:
                            ApplyUserSeesFullCards(msgParts);
                            break;
                        case codeGameMode:
                            newRound = ApplyChoseGameOfOtherUser(msgParts);
                            break;
                        case codeKontraRe:
                            int w = 0;
                            int kontra = 0;
                            while (w < 4)
                            {
                                if (_match.CurrentPlayers[w].Kontra) ++kontra;
                                ++w;
                            }

                            if (kontra < 2) ApplyKontraReOftherUser(msgParts);
                            else
                            {
                                redirect = false;
                                _connection.SendMessage(codeClientKontraReInvalid, e.Sender);
                            }

                            break;
                        case codePlayCard:
                            playCard = ApplyCardClickOfOtherUser(msgParts);
                            break;
                        case codeContinue:
                            newGame = ApplyContinueOfOtherUser(msgParts);
                            break;
                        default:
                            redirect = false;
                            break;
                    }

                    if (redirect) _connection.RedirectClientMessage(e.Message, e.Sender);
                    if (newRound)
                    {
                        ModeSelector.State = GameSelectorState.Visible;
                        ShuffleCards(true);
                    }
                    else if (newGame) ShuffleCards(false);
                    else if (playCard) CardHolder_CardClicked(null, null);

                    break;
                }
            }

            DebugLog();
        }

        private void ClientEvents(MultiplayerEvent e)
        {
            if (e.Type == MultiplayerEventTypes.ClientReceived)
            {
                string[] msgParts = e.Message.Split(SEPARATOR);
                switch (msgParts[0])
                {
                    case codeClientInfoRejoin:
                        RestoreFromInfo(msgParts);
                        SendMessage(new List<string> { CODE_HOST_READY_TO_START, _playerIndex.ToString() });
                        break;
                    case codeClientShuffledCards:
                        ShuffleCards(bool.Parse(msgParts[33]), msgParts);
                        break;
                    case codeViewCard:
                        ApplyUserSeesFullCards(msgParts);
                        break;
                    case codeGameMode:
                        ApplyChoseGameOfOtherUser(msgParts);
                        break;
                    case codeKontraRe:
                        ApplyKontraReOftherUser(msgParts);
                        break;
                    case codePlayCard:
                        if (ApplyCardClickOfOtherUser(msgParts))
                        {
                            CardHolder_CardClicked(null, null);
                        }

                        break;
                    case codeContinue:
                        ApplyContinueOfOtherUser(msgParts);
                        break;
                    case codeClientLostConnection:
                        ApplyConnectionToUserLost(msgParts);
                        break;
                    case codeClientKontraReInvalid:
                        _match.CurrentPlayers[PlayerIndexCurRound].Kontra = false;
                        PlayerInfo1.Kontra = false;
                        break;
                }

                DebugLog();
            }
            else if (e.Type == MultiplayerEventTypes.CClientDisconnected)
            {
                LblConnectionLost.Content = Properties.Resources.MP_LostConnectionToHost;
                GridRoundSummary.Visibility = Visibility.Collapsed;
                SpectatorView.Visibility = Visibility.Collapsed;
                ViewPlaying.Visibility = Visibility.Collapsed;
                GridConnectionLost.Visibility = Visibility.Visible;
                CanQuitNow = true;
                LostConnectionToHost?.Invoke();
            }
        }

        private void SendMessage(List<string> message, string id = null)
        {
            _connection.SendMessage(message, SEPARATOR, id);
        }

        public void EndConnection()
        {
            _connection.EndConnection();
        }

        private void DebugLog()
        {
            _connection.WriteLine(_match.WriteCurrentIndize().Append(", PlIndexCurRnd: ").Append(PlayerIndexCurRound)
                .ToString());
        }

        private List<string> GenerateRejoinInfo()
        {
            List<string> message = new List<string> { codeClientInfoRejoin };
            message.AddRange(_match.Players.Select(player => player.InfoForRejoin(SEPARATOR_REJOIN_INFO_STRING)));

            message.Add(_match.InfoForRejoin(SEPARATOR_REJOIN_INFO_STRING));
            StringBuilder bob = new StringBuilder(8);
            if (PlayerIndexCurRound == -1)
            {
                bob.Append(_playerInfosSpectate[0].State).Append(SEPARATOR_REJOIN_INFO_STRING);
                bob.Append(_playerInfosSpectate[1].State).Append(SEPARATOR_REJOIN_INFO_STRING);
                bob.Append(_playerInfosSpectate[2].State).Append(SEPARATOR_REJOIN_INFO_STRING);
                bob.Append(_playerInfosSpectate[3].State);
            }
            else
            {
                bob.Append(_playerInfos[0].State).Append(SEPARATOR_REJOIN_INFO_STRING);
                bob.Append(_playerInfos[1].State).Append(SEPARATOR_REJOIN_INFO_STRING);
                bob.Append(_playerInfos[2].State).Append(SEPARATOR_REJOIN_INFO_STRING);
                bob.Append(_playerInfos[3].State);
            }

            message.Add(bob.ToString());
            return message;
        }

        private void RestoreFromInfo(IReadOnlyList<string> msgParts)
        {
            for (int i = 0; i < _match.Players.Count; ++i)
            {
                _match.Players[i].RestoreFromInfo(msgParts[i + 1], SEPARATOR_REJOIN_INFO);
            }

            _match.RestoreFromInfo(msgParts[_match.Players.Count + 1], SEPARATOR_REJOIN_INFO);
            //unnötog durch umstellung auf playInCurRou { get {...}} playerIndexCurRound = match.players[playerIndex].number;
            if (_match.Rounds.Count == 8 && _match.CurrentRound.currentCards.Count == 4)
            {
                ShowSummary(false);
                if (_match.Players[_playerIndex].continueMatch == true)
                {
                    BtnPlayerNextMatch.Visibility = Visibility.Collapsed;
                    _cvNextMatch[_playerIndex].Visibility = Visibility.Visible;
                }
            }
            else
            {
                if (_match.Rounds.Count > 1)
                {
                    FillLastStich(_match.Rounds[_match.Rounds.Count - 2]);
                    BtnLastStich.IsEnabled = true;
                }

                string[] state = msgParts[_match.Players.Count + 2].Split(SEPARATOR_REJOIN_INFO);
                List<Card> cards = _match.CurrentRound.currentCards;
                int offset = _match.CurrentRound.StartPlayer;
                if (PlayerIndexCurRound == -1)
                {
                    CanQuitNow = false;
                    _isSpectating = true;
                    SpectatorView.Visibility = Visibility.Visible;
                    for (int i = 0; i < 4; ++i)
                    {
                        _playerInfosSpectate[i].Name = _match.CurrentPlayers[i].name;
                        _playerInfosSpectate[i].Aufgestellt = _match.CurrentPlayers[i].Aufgestellt;
                        _playerInfosSpectate[i].Kontra = _match.CurrentPlayers[i].Kontra;
                        if (state[i].Equals(SkPlayerInfo.STATE_EMPTY))
                        {
                            _playerInfosSpectate[i].EmptyState();
                        }
                        else
                        {
                            _playerInfosSpectate[i].State = state[i];
                        }

                        if (state[i].Equals(SkPlayerInfo.STATE_AUFSTELLEN))
                        {
                            _chSpectate[i].Reset();
                        }
                        else
                        {
                            _chSpectate[i].ShowAllCards(_match.CurrentPlayers[i].Aufgestellt);
                        }

                        _chSpectate[i].Cards = _match.CurrentPlayers[i].GetPlayableCards();
                    }

                    for (int i = 0; i < cards.Count; ++i)
                    {
                        CurrentCardsSpectate.AddCard(cards[i], _match.CurrentPlayers[(i + offset) % 4].Number);
                    }
                }
                else
                {
                    ViewPlaying.Visibility = Visibility.Visible;
                    _isSpectating = CanQuitNow = false;

                    for (int i = 0; i < 4; ++i)
                    {
                        var index = GetUiPlayerIndex(i);
                        _playerInfos[index].PlayerName = _match.CurrentPlayers[i].name;
                        _playerInfos[index].Aufgestellt = _match.CurrentPlayers[i].Aufgestellt;
                        _playerInfos[index].Kontra = _match.CurrentPlayers[i].Kontra;
                        if (state[i].Equals(SkPlayerInfo.STATE_EMPTY))
                        {
                            _playerInfos[i].EmptyState();
                        }
                        else _playerInfos[index].State = state[i];
                    }

                    if (_match.AmountShuffle == 3)
                    {
                        UpdateFocus(true);
                        ModeSelector.State = GameSelectorState.Hidden;
                        CardHolder.ShowAllCards(_match.Players[PlayerIndexCurRound].Aufgestellt);
                    }
                    else
                    {
                        UpdateFocus(false);
                        if (_playerInfos[0].State.Equals(SkPlayerInfo.STATE_AUFSTELLEN))
                        {
                            CardHolder.Reset();
                            ModeSelector.State = GameSelectorState.Hidden;
                        }
                        else
                        {
                            ApplyPossibilties();
                            if (_playerInfos[0].State.Equals(SkPlayerInfo.STATE_EMPTY))
                            {
                                ModeSelector.State = GameSelectorState.Visible;
                            }
                            else
                            {
                                if (_match.PlayerIndex == PlayerIndexCurRound)
                                {
                                    int w = 0;
                                    while (_match.Players[_playerIndex].Possibilities[w].Mode != _match.MinimumGame)
                                    {
                                        ++w;
                                    }

                                    ModeSelector.CbMode.SelectedIndex = w;
                                    int color = 0;
                                    while (!_match.Players[_playerIndex].Possibilities[w].colors[color]
                                               .Equals(_match.Color))
                                    {
                                        ++color;
                                    }

                                    ModeSelector.CbColor.SelectedIndex = color;
                                }
                                else
                                {
                                    ModeSelector.CbColor.SelectedIndex = 0;
                                    ModeSelector.CbMode.SelectedIndex = 0;
                                }

                                ModeSelector.State = GameSelectorState.Selected;
                            }

                            ModeSelector.State = _playerInfos[0].State.Equals(SkPlayerInfo.STATE_EMPTY)
                                ? GameSelectorState.Visible
                                : GameSelectorState.Selected;
                            CardHolder.ShowAllCards(_match.CurrentPlayers[PlayerIndexCurRound].Aufgestellt);
                        }
                    }

                    CardHolder.Cards = _match.CurrentPlayers[PlayerIndexCurRound].GetPlayableCards();
                    for (int i = 0; i < cards.Count; ++i)
                    {
                        CurrentCards.AddCard(cards[i],
                            GetUiPlayerIndex(_match.CurrentPlayers[(i + offset) % 4].Number));
                    }
                }
            }

            UpdateFocus(null);
        }

        private void ApplyKontraReOftherUser(IReadOnlyList<string> msgParts)
        {
            int index = int.Parse(msgParts[1]);
            _match.CurrentPlayers[index].Kontra = true;
            if (_isSpectating)
            {
                _playerInfosSpectate[index].Kontra = true;
            }
            else
            {
                _playerInfos[GetUiPlayerIndex(index)].Kontra = true;
                if (_match.CurrentPlayers[index].TeamIndex == _match.CurrentPlayers[PlayerIndexCurRound].TeamIndex)
                {
                    BtnKontra.Visibility = Visibility.Hidden;
                }
                else if (_match.CurrentPlayers[index].TeamIndex -
                         _match.CurrentPlayers[PlayerIndexCurRound].TeamIndex ==
                         1)
                {
                    BtnKontra.Visibility = Visibility.Visible;
                }
            }
        }

        private bool ApplyCardClickOfOtherUser(IReadOnlyList<string> msgParts)
        {
            int player = int.Parse(msgParts[1]);
            int selected = int.Parse(msgParts[2]);
            if (PlayCard(player, selected, _match.CurrentPlayers[player].PlayableCards[selected]))
            {
                if (_isSpectating)
                {
                    _chSpectate[player].RemoveCard(selected);
                    return false;
                }

                return true;
            }

            return false;
        }

        private bool ApplyChoseGameOfOtherUser(IReadOnlyList<string> msgParts)
        {
            if (ChoseGameMode(int.Parse(msgParts[1]), SchafkopfMatch.StringToSchafkopfMode(msgParts[2]), msgParts[3]))
            {
                return true;
            }

            if (!_isSpectating) ModeSelector.TrySelect();
            return false;
        }

        private void ApplyUserSeesFullCards(IReadOnlyList<string> msgParts)
        {
            bool aufgestellt = bool.Parse(msgParts[2]);
            int index = int.Parse(msgParts[1]);
            _match.CurrentPlayers[index].Aufgestellt = aufgestellt;
            if (_isSpectating)
            {
                _playerInfosSpectate[index].Aufgestellt = aufgestellt;
                _chSpectate[index].ShowAllCards(aufgestellt);
            }
            else
            {
                _playerInfos[GetUiPlayerIndex(index)].Aufgestellt = aufgestellt;
            }
        }

        private bool ApplyContinueOfOtherUser(IReadOnlyList<string> msgParts)
        {
            int index = int.Parse(msgParts[1]);
            _match.Players[index].continueMatch = bool.Parse(msgParts[2]);
            _cvNextMatch[index].IsChecked = _match.Players[index].continueMatch;
            return _connection.IsHost && StartNextMatch();
        }

        private void ApplyConnectionToUserLost(int missingPlayerCount, int index, MultiplayerPlayerState state,
            string newId)
        {
            _match.Players[index].State = state;
            if (state == MultiplayerPlayerState.Active)
            {
                _match.Players[index].id = newId;
            }

            if (ViewPlaying.Visibility == Visibility.Visible)
            {
                _gridBeforeConnectionError = ViewPlaying;
            }
            else if (SpectatorView.Visibility == Visibility.Visible)
            {
                _gridBeforeConnectionError = ViewPlaying;
            }
            else
            {
                return;
            }

            if (missingPlayerCount > 0)
            {
                GridConnectionLost.Visibility = Visibility.Visible;
                _gridBeforeConnectionError.IsEnabled = false;
            }
            else
            {
                GridConnectionLost.Visibility = Visibility.Collapsed;
                _gridBeforeConnectionError.IsEnabled = true;
            }
            //TODO: find out why here was a return; statement
        }

        private void ApplyConnectionToUserLost(IReadOnlyList<string> msgParts)
        {
            ApplyConnectionToUserLost(int.Parse(msgParts[1]), int.Parse(msgParts[2]),
                SchafkopfPlayer.Convert(msgParts[3]), msgParts[4]);
        }

        #endregion

        #region UI Updates

        private void NewGame(bool samePlayer = false)
        {
            _allPlayerSeeAllCards = false;
            if (_isSpectating)
            {
                for (int i = 0; i < 4; ++i)
                {
                    if (samePlayer)
                    {
                        _playerInfosSpectate[i].NewMatch();
                    }
                    else
                    {
                        _playerInfosSpectate[i].NewMatch(_match.CurrentPlayers[i].name);
                    }

                    _match.CurrentPlayers[i].UpdatePossibilities(_match);
                }

                _playerInfosSpectate[_match.CurrentRound.CurrentPlayer].IsStartPlayer = true;
            }
            else
            {
                for (int i = 0; i < 4; ++i)
                {
                    if (samePlayer)
                    {
                        _playerInfos[GetUiPlayerIndex(i)].NewMatch();
                    }
                    else
                    {
                        _playerInfos[GetUiPlayerIndex(i)].NewMatch(_match.CurrentPlayers[i].name);
                    }

                    _match.CurrentPlayers[i].UpdatePossibilities(_match);
                }

                ApplyPossibilties();
                ModeSelector.State = GameSelectorState.Hidden;
                BtnKontra.Visibility = Visibility.Hidden;
                if (VisualPlayer.Aufgestellt)
                {
                    CardHolder_ShowsAllCards(null, null);
                }

                _playerInfos[GetUiPlayerIndex(_match.CurrentRound.CurrentPlayer)].IsStartPlayer = true;
            }

            UpdateFocus(false);
        }

        private void ApplyPossibilties()
        {
            ModeSelector.Source = _match.CurrentPlayers[PlayerIndexCurRound].Possibilities;
        }

        private void SortCards(int index, SchafkopfMatchConfig matchSort)
        {
            _match.CurrentPlayers[index].SortCards(matchSort);
            if (_isSpectating)
            {
                _chSpectate[index].Cards = _match.CurrentPlayers[index].GetPlayableCards();
            }
            else
            {
                CardHolder.Cards = _match.CurrentPlayers[PlayerIndexCurRound].GetPlayableCards();
            }
        }

        private void MarkPlayableCards(bool update)
        {
            if (update) _match.UpdatePlayableCards();
            if (!_isSpectating)
            {
                CardHolder.MarkSelectableCards(_match.PlayableCards[PlayerIndexCurRound]);
            }
        }

        private void UpdateCardVisual(bool forceReset, bool tryReset)
        {
            if (_isSpectating)
            {
                for (int i = 0; i < 4; ++i)
                {
                    if (forceReset || tryReset && !_chSpectate[i].Aufgestellt)
                    {
                        _chSpectate[i].Reset();
                    }

                    _chSpectate[i].Cards = _match.CurrentPlayers[i].GetPlayableCards();
                }
            }
            else
            {
                if (forceReset || tryReset && !CardHolder.Aufgestellt)
                {
                    CardHolder.Reset();
                }

                CardHolder.Cards = _match.CurrentPlayers[PlayerIndexCurRound].GetPlayableCards();
            }
        }

        private void UpdateFocus(bool? cardHolder)
        {
            int current = _match.CurrentRound.CurrentPlayer;
            if (_isSpectating)
            {
                for (int i = 0; i < 4; ++i)
                {
                    _playerInfosSpectate[i].Focused = i == current;
                }

                if (cardHolder == true)
                {
                    for (int i = 0; i < 4; ++i)
                    {
                        _chSpectate[i].Focused = i == current;
                    }
                }
            }
            else
            {
                for (int i = 0; i < 4; ++i)
                {
                    _playerInfos[GetUiPlayerIndex(i)].Focused = i == current;
                }

                if (cardHolder == true)
                {
                    CardHolder.Focused = current == PlayerIndexCurRound;
                }
                else if (cardHolder.HasValue)
                {
                    ModeSelector.SetGameSelectorFocus(current == PlayerIndexCurRound);
                }
            }
        }

        private void FillLastStich(SchafkopfRound round)
        {
            _lastStichView = new StichView();
            if (_isSpectating)
            {
                for (int i = 0; i < 4; ++i)
                {
                    _lastStichView.AddCard(round.currentCards[i],
                        _match.CurrentPlayers[(round.StartPlayer + i) % 4].Number);
                }
            }
            else
            {
                for (int i = 0; i < 4; ++i)
                {
                    _lastStichView.AddCard(round.currentCards[i],
                        GetUiPlayerIndex(_match.CurrentPlayers[(round.StartPlayer + i) % 4].Number));
                }
            }
        }

        private void ShowSummary(bool evaluation = true)
        {
            if (evaluation)
            {
                bool? tmp = _match.Evalution(PlayerIndexCurRound);
                BorderSummary.BorderBrush =
                    tmp.HasValue ? tmp.Value ? Brushes.Green : Brushes.Red : Brushes.Transparent;
            }

            CanQuitNow = true;
            TbPointsSummary.Text = _match.FullSummary();
            int i;
            for (i = 0; i < _match.Players.Count; ++i)
            {
                if (evaluation)
                {
                    _match.Players[i].continueMatch = null;
                }

                _cvNextMatch[i].IsChecked = _match.Players[i].continueMatch;
                _lblPlayerSummaryNames[i].Content = _match.Players[i].name;
                _lblPlayerSummaryPoints[i].Content = _match.Players[i].Points;
                _gridColsSummary[i].Width = new GridLength(1, GridUnitType.Star);
            }

            for (; i < 7; ++i)
            {
                _gridColsSummary[i].Width = new GridLength(0, GridUnitType.Star);
            }

            ViewPlaying.Visibility = Visibility.Collapsed;
            SpectatorView.Visibility = Visibility.Collapsed;
            Grid.SetColumn(BtnPlayerNextMatch, _playerIndex + 1);
            BtnPlayerNextMatch.Visibility = Visibility.Visible;
            _cvNextMatch[_playerIndex].Visibility = Visibility.Collapsed;
            GridRoundSummary.Visibility = Visibility.Visible;
            if (_points != null)
            {
                UpdatePoints();
            }

            if (QuitAfterMatch)
            {
                BtnPoints_Click(null, null);
                QuitMatch?.Invoke();
            }

            BtnPoints.IsEnabled = true;
        }

        private void AllowUiToUpdate()
        {
            DispatcherFrame frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Render, new DispatcherOperationCallback(
                delegate
                {
                    frame.Continue = false;
                    return null;
                }), null);

            Dispatcher.PushFrame(frame);
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background,
                new Action(delegate { }));
        }

        #endregion

        #region Logic

        /// <summary>
        /// Shuffles the cards if player is host or reads cards from message if player is client. Sets up the UI for selecting the game mode.
        /// </summary>
        /// <param name="sameRound"></param> indicates if the round has changed or all players didn't want to play the previous round.
        /// <param name="msgParts"></param> message for clients
        private void ShuffleCards(bool sameRound, IReadOnlyList<string> msgParts = null)
        {
            if (!sameRound)
            {
                BtnLastStich.IsEnabled = false;
                _match.NextMatch();
            }

            GridRoundSummary.Visibility = Visibility.Collapsed;
            _match.PrepareShuffleCards(sameRound);
            if (_connection.IsHost)
            {
                SendMessage(_match.ShuffleCards(sameRound, true, codeClientShuffledCards));
            }
            else
            {
                for (int i = 0; i < 32; ++i)
                {
                    _match.CurrentPlayers[i % 4].PlayableCards.Add(Card.GetCard(int.Parse(msgParts[i + 1])));
                }
            }

            //unnötog durch umstellung auf playInCurRou { get {...}} playerIndexCurRound = match.players[playerIndex].number;
            if (_match.Players[_playerIndex].Number == -1)
            {
                _isSpectating = true;
                CurrentCardsSpectate.Reset();
                ViewPlaying.Visibility = Visibility.Collapsed;
                SpectatorView.Visibility = Visibility.Visible;
            }
            else
            {
                _isSpectating = false;
                CurrentCards.Reset();
                CanQuitNow = false;
                CardHolder.Focused = false;
                CardHolder.CanClickCards = false;
                ModeSelector.State = GameSelectorState.Hidden;
                ViewPlaying.Visibility = Visibility.Visible;
                SpectatorView.Visibility = Visibility.Collapsed;
            }

            NewGame(sameRound);
            UpdateCardVisual(!sameRound, true);
        }

        /// <summary>
        /// Calculates the index of the UI Elements that represents a specific player.
        /// </summary>
        /// <param name="index"></param> index of the player in the current players.
        /// <returns></returns> index of the player for its UI-elements.
        private int GetUiPlayerIndex(int index)
        {
            return (index + 4 - PlayerIndexCurRound) % 4;
        }

        /// <summary>
        /// Handles 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="mode"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        private bool ChoseGameMode(int index, SchafkopfMode mode, string color)
        {
            if (_isSpectating) _playerInfosSpectate[index].State = mode.ToString();
            else _playerInfos[GetUiPlayerIndex(index)].State = mode.ToString();

            if (mode > _match.MinimumGame)
            {
                _match.MinimumGame = mode;
                _match.PlayerIndex = index;
                _match.Color = color;
            }

            if (mode == SchafkopfMode.Weiter) ++_match.AmountShuffle;

            if (_match.AmountShuffle == 4) return true;
            if (_match.AmountShuffle == 3 && _match.MinimumGame != SchafkopfMode.Weiter)
            {
                _match.ChooseGame();
                _match.CurrentRound.ResetNextPlayer();
                ModeSelector.State = GameSelectorState.Hidden;
                _match.CurrentPlayers[_match.PlayerIndex].TeamIndex = 0;
                int end = _match.PlayerIndex + 4;
                if (_match.Mode == SchafkopfMode.Sauspiel)
                {
                    for (int i = _match.PlayerIndex + 1; i < end; ++i)
                    {
                        _match.CurrentPlayers[i % 4].TeamIndex =
                            _match.CurrentPlayers[i % 4].HasGesuchte(_match) ? 0 : 1;
                    }
                }
                else
                {
                    for (int i = _match.PlayerIndex + 1; i < end; ++i)
                    {
                        _match.CurrentPlayers[i % 4].TeamIndex = 1;
                    }
                }

                _match.CalculateLaufende();
                MarkPlayableCards(false);
                CardHolder.CanClickCards = _match.CurrentRound.CurrentPlayer == PlayerIndexCurRound;
                UpdateCardVisual(false, false);
                UpdateFocus(true);
                if (_isSpectating)
                {
                    for (int i = 0; i < 4; ++i)
                    {
                        _playerInfosSpectate[i].EmptyState();
                    }

                    _playerInfosSpectate[_match.PlayerIndex].State = _match.ToString();
                    _playerInfosSpectate[_match.CurrentRound.CurrentPlayer].IsStartPlayer = false;
                }
                else
                {
                    if (_match.CurrentPlayers[PlayerIndexCurRound].TeamIndex == 1)
                    {
                        BtnKontra.Content = Properties.Resources.SK_Btn_Kontra;
                        BtnKontra.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        BtnKontra.Content = Properties.Resources.SK_Btn_KontraRe;
                        BtnKontra.Visibility = Visibility.Hidden;
                    }

                    int uiIndex = GetUiPlayerIndex(_match.PlayerIndex);
                    for (int i = 0; i < 4; ++i)
                    {
                        _playerInfos[i].EmptyState();
                    }

                    _playerInfos[uiIndex].State = _match.ToString();
                    _playerInfos[GetUiPlayerIndex(_match.CurrentRound.CurrentPlayer)].IsStartPlayer = false;
                }
            }
            else
            {
                _match.CurrentRound.SetNextPlayer();
                if (!_isSpectating && mode != SchafkopfMode.Weiter)
                {
                    ModeSelector.CheckIfSelectedStillValid(mode, _match, _match.CurrentPlayers[PlayerIndexCurRound]);
                }
            }

            UpdateFocus(false);
            return false;
        }

        private bool PlayCard(int player, int selected, Card card)
        {
            if (_match.CurrentPlayers[player].PlayCard(selected, _match))
            {
                if (_match.CurrentRound.currentCards.Count == 1)
                {
                    if (_isSpectating)
                    {
                        CurrentCardsSpectate.Reset();
                    }
                    else
                    {
                        CurrentCards.Reset();
                        CardHolder.CanClickCards = _match.CurrentRound.CurrentPlayer != PlayerIndexCurRound;
                    }

                    MarkPlayableCards(true);
                }

                if (_isSpectating)
                {
                    CurrentCardsSpectate.AddCard(card, _match.CurrentPlayers[player].Number);
                }
                else
                {
                    CurrentCards.AddCard(card, GetUiPlayerIndex(_match.CurrentPlayers[player].Number));
                }

                if (_match.CurrentRound.currentCards.Count < 4)
                {
                    _match.CurrentRound.SetNextPlayer();
                }
                else
                {
                    for (int i = 0; i < 4; ++i)
                    {
                        _match.LastCards[i] = _match.CurrentRound.currentCards[i];
                    }

                    FillLastStich(_match.CurrentRound);
                    BtnLastStich.IsEnabled = true;
                    if (_match.Rounds.Count < 8)
                    {
                        _match.NextRound();
                        MarkPlayableCards(false);
                        CardHolder.CanClickCards = _match.CurrentRound.CurrentPlayer == PlayerIndexCurRound;
                    }
                    else
                    {
                        CardHolder.Focused = false;
                        AllowUiToUpdate();
                        Thread.Sleep(1500);
                        ShowSummary();
                        return false;
                    }
                }

                UpdateFocus(true);
                return true;
            }

            return false;
        }

        private bool StartNextMatch()
        {
            int w = 0;
            while (w < _match.Players.Count && _match.Players[w].continueMatch == true)
            {
                ++w;
            }

            return w == _match.Players.Count;
            //TODO handle player leaving the match
        }

        private void UpdatePoints()
        {
            _points.Update(_match.PlayerPoints, _match.PointsSingle, _match.PointsCumulated);
        }

        #endregion

        #region UI-Listener

        private void BtnKontra_Click(object sender, RoutedEventArgs e)
        {
            _match.Players[_playerIndex].Kontra = true;
            VisualPlayer.Kontra = true;
            BtnKontra.Visibility = Visibility.Hidden;
            SendMessage(new List<string> { codeKontraRe, PlayerIndexCurRound.ToString() });
        }

        private void CardHolder_ShowsAllCards(object sender, EventArgs e)
        {
            VisualPlayer.Aufgestellt = _match.Players[_playerIndex].Aufgestellt =
                CardHolder.Aufgestellt || _match.Players[_playerIndex].Aufgestellt;
            SendMessage(new List<string>
                { codeViewCard, PlayerIndexCurRound.ToString(), CardHolder.Aufgestellt.ToString() });
            ApplyPossibilties();
            ModeSelector.State = GameSelectorState.Visible;
            UpdateFocus(false);
        }

        private void CardHolder_CardClicked(object sender, EventArgs e)
        {
            if (_match.CurrentRound.CurrentPlayer == PlayerIndexCurRound && CardHolder.SelectedCard != -1)
            {
                if (BtnKontra.Visibility == Visibility.Visible)
                {
                    BtnKontra.Visibility = Visibility.Hidden;
                }

                int selected = CardHolder.SelectedCard;
                Card card = CardHolder.RemoveSelectedCard();
                SendMessage(new List<string> { codePlayCard, PlayerIndexCurRound.ToString(), selected.ToString() });
                PlayCard(PlayerIndexCurRound, selected, card);
                if (_match.CurrentRound.currentCards.Count != 0)
                {
                    CardHolder.CanClickCards = false;
                }
            }
        }

        private void BtnLastStich_Click(object sender, RoutedEventArgs e)
        {
            LastStich stich = new LastStich(_lastStichView);
            stich.ShowDialog();
        }

        private void BtnPoints_Click(object sender, RoutedEventArgs e)
        {
            if (_points == null)
            {
                _points = new SchafkopfPoints(_match.PlayerPoints, _match.PointsSingle, _match.PointsCumulated);
                _points.Closing += PointsWindowClosed;
            }
            else
            {
                UpdatePoints();
            }

            _points.Show();
        }

        private void PointsWindowClosed(object sender, CancelEventArgs e)
        {
            _points = null;
        }

        private void BtnPlayerNextMatch_Click(object sender, RoutedEventArgs e)
        {
            BtnPlayerNextMatch.Visibility = Visibility.Collapsed;
            _cvNextMatch[_playerIndex].Visibility = Visibility.Visible;
            _cvNextMatch[_playerIndex].IsChecked = true;
            _match.Players[_playerIndex].continueMatch = true;
            SendMessage(new List<string> { codeContinue, _playerIndex.ToString(), "true" });
            if (_connection.IsHost)
            {
                if (StartNextMatch())
                {
                    ShuffleCards(false);
                }
            }
        }

        private void BtnPlayerQuit_Click(object sender, RoutedEventArgs e)
        {
            _match.Players[_playerIndex].continueMatch = false;
            if (_connection.IsHost)
            {
                /* TODO host wants to end the game */
            }
        }

        private void ModeSelector_ColorChanged(GameModeSelectedEvent e)
        {
            SortCards(PlayerIndexCurRound, new SchafkopfMatch(e.Mode, e.Color));
        }

        private void ModeSelector_ModeSelected(GameModeSelectedEvent e)
        {
            if (_match.CurrentRound.CurrentPlayer == PlayerIndexCurRound)
            {
                if (AllPlayerSeeAllCards)
                {
                    ModeSelector.State = GameSelectorState.Selected;
                    SendMessage(new List<string>
                        { codeGameMode, PlayerIndexCurRound.ToString(), e.Mode.ToString(), e.Color });
                    if (ChoseGameMode(PlayerIndexCurRound, e.Mode, e.Color))
                    {
                        ModeSelector.State = GameSelectorState.Visible;
                        if (_connection.IsHost)
                        {
                            ShuffleCards(true);
                        }
                    }
                }
                else
                {
                    ModeSelector.BtnSelectMode.IsChecked = false;
                }
            }
        }

        #endregion
    }
}