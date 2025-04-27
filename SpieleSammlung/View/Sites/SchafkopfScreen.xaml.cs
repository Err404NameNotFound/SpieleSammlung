using SpieleSammlung.Model.Multiplayer;
using SpieleSammlung.Model.Schafkopf;
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
using SpieleSammlung.View.Enums;
using SpieleSammlung.View.UserControls;
using SpieleSammlung.View.UserControls.Schafkopf;
using SpieleSammlung.View.Windows;
using static SpieleSammlung.Model.Schafkopf.SchafkopfMatch;

namespace SpieleSammlung.View.Sites;

/// <summary>
/// Interaktionslogik für SchafkopfScreen.xaml
/// </summary>
public partial class SchafkopfScreen
{
    #region Events

    public delegate void OnQuitMatch();

    public event OnQuitMatch QuitMatch;

    public delegate void OnLostConnectionToHost();

    public event OnLostConnectionToHost LostConnectionToHost;

    #endregion

    #region Quit Handling

    public bool QuitAfterMatch { get; private set; }
    public bool CanQuitNow { get; private set; }

    public void InvertQuitAfterMatch() => QuitAfterMatch = !QuitAfterMatch;

    #endregion

    #region Constants

    private const char SEPARATOR_REJOIN_INFO = '|';
    private const string SEPARATOR_REJOIN_INFO_STRING = "|";
    private const char SEPARATOR = ';';
    private const string CODE_HOST_READY_FOR_INFO = "o", CODE_HOST_READY_TO_START = "p";

    private const string CODE_CLIENT_KONTRA_RE_INVALID = "v",
        CODE_CLIENT_INFO_REJOIN = "x",
        CODE_CLIENT_SHUFFLED_CARDS = "y",
        CODE_CLIENT_LOST_CONNECTION = "z";

    private const string CODE_VIEW_CARD = "a",
        CODE_GAME_MODE = "b",
        CODE_KONTRA_RE = "c",
        CODE_PLAY_CARD = "d",
        CODE_CONTINUE = "e";

    #endregion

    #region Member

    private bool _isSpectating;
    private bool _allPlayerSeeAllCards;

    private bool AllPlayerSeeAllCards
    {
        get
        {
            if (_allPlayerSeeAllCards)
                return _allPlayerSeeAllCards;

            int i = 0;
            while (i < PLAYER_PER_ROUND)
            {
                if (_playerInfos[i].State.Equals(SkPlayerInfo.STATE_AUFSTELLEN))
                    break;

                ++i;
            }

            _allPlayerSeeAllCards = i == PLAYER_PER_ROUND;
            return _allPlayerSeeAllCards;
        }
    }

    private readonly MpConnection _connection;
    private readonly SchafkopfMatch _match;
    private readonly int _playerIndex;

    private int PlayerIndexCurRound => _match.Players[_playerIndex].Number;

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

    public SchafkopfScreen(IEnumerable<MultiplayerPlayer> players, int index, MpConnection connection,
        bool reJoin = false)
    {
        InitializeComponent();

        //UI Playing
        _playerInfos = [VisualPlayer, VisualPlayerLeft, VisualPlayerTop, VisualPlayerRight];
        ModeSelector.ColorChanged += ModeSelector_ColorChanged;
        ModeSelector.ModeSelected += ModeSelector_ModeSelected;
        //UI Summary
        _lblPlayerSummaryNames =
        [
            LblPlayer1Review, LblPlayer2Review, LblPlayer3Review, LblPlayer4Review, LblPlayer5Review,
            LblPlayer6Review, LblPlayer7Review
        ];
        _lblPlayerSummaryPoints =
        [
            LblPlayer1Points, LblPlayer2Points, LblPlayer3Points, LblPlayer4Points, LblPlayer5Points,
            LblPlayer6Points, LblPlayer7Points
        ];
        _cvNextMatch = [CvPlayer1, CvPlayer2, CvPlayer3, CvPlayer4, CvPlayer5, CvPlayer6, CvPlayer7];
        _gridColsSummary =
        [
            SummaryColumnP1, SummaryColumnP2, SummaryColumnP3, SummaryColumnP4, SummaryColumnP5, SummaryColumnP6,
            SummaryColumnP7
        ];
        //UI Spectate
        _chSpectate = [ChPlayer1, ChPlayer2, ChPlayer3, ChPlayer4];
        for (int i = 0; i < PLAYER_PER_ROUND; ++i)
        {
            _chSpectate[i].BtnAufstellen.IsEnabled = _chSpectate[i].BtnShowRest.IsEnabled = false;
        }

        _playerInfosSpectate = [PlayerInfo1, PlayerInfo2, PlayerInfo3, PlayerInfo4];


        //Data
        _connection = connection;
        _playerIndex = index;
        if (connection.IsHost) connection.HostEvent += HostEvents;
        else connection.ClientEvent += ClientEvents;

        _match = new SchafkopfMatch(players, connection.IsHost);

        CanQuitNow = true;
        QuitAfterMatch = false;
        if (connection.IsHost)
            ShuffleCards(false);
        else if (reJoin)
            SendMessage(CODE_HOST_READY_FOR_INFO);
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
                while (!e.Message.Equals(_match.Players[w].Name)) ++w;
                List<string> message = [MultiplayerLobby.CLIENT_LATE_JOIN, w.ToString()];
                foreach (var player in _match.Players)
                {
                    message.Add(player.Name);
                    message.Add(player.Id);
                }

                _connection.SendMessage(message, MultiplayerLobby.SEPARATOR, e.Sender);
                break;
            }
            case MultiplayerEventTypes.HClientDisconnected:
            {
                int i = 0;
                while (i < _match.Players.Count)
                {
                    if (e.Sender.Equals(_match.Players[i].Id))
                    {
                        if (GridRoundSummary.Visibility == Visibility.Visible)
                            _connection.LostClients.Clear();

                        ApplyConnectionToUserLost(_connection.LostClients.Count, i, MultiplayerPlayerState.LeftMatch,
                            "");
                        SendMessage(new List<string>
                        {
                            CODE_CLIENT_LOST_CONNECTION, _connection.LostClients.Count.ToString(), i.ToString(),
                            nameof(MultiplayerPlayerState.LeftMatch), ""
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
                        SendMessage(e.Sender, GenerateRejoinInfo());
                        break;
                    case CODE_HOST_READY_TO_START:
                        redirect = false;
                        ApplyConnectionToUserLost(_connection.LostClients.Count, int.Parse(msgParts[1]),
                            MultiplayerPlayerState.Active, e.Sender);
                        SendMessage(CODE_CLIENT_LOST_CONNECTION, _connection.LostClients.Count.ToString(), msgParts[1],
                            MultiplayerPlayerState.Active.ToString(), e.Sender);
                        break;
                    case CODE_VIEW_CARD:
                        ApplyUserSeesFullCards(msgParts);
                        break;
                    case CODE_GAME_MODE:
                        newRound = ApplyChoseGameOfOtherUser(msgParts);
                        break;
                    case CODE_KONTRA_RE:
                        int kontra = _match.CurrentPlayers.Count(t => t.Kontra);
                        if (kontra < 2) ApplyKontraReOfOtherUser(msgParts);
                        else
                        {
                            redirect = false;
                            _connection.SendMessage(CODE_CLIENT_KONTRA_RE_INVALID, e.Sender);
                        }

                        break;
                    case CODE_PLAY_CARD:
                        playCard = ApplyCardClickOfOtherUser(msgParts);
                        break;
                    case CODE_CONTINUE:
                        newGame = ApplyContinueOfOtherUser(msgParts);
                        break;
                    default:
                        redirect = false;
                        break;
                }

                if (redirect)
                    _connection.RedirectClientMessage(e.Message, e.Sender);
                if (newRound)
                {
                    ModeSelector.State = GameSelectorState.Visible;
                    ShuffleCards(true);
                }
                else if (newGame)
                    ShuffleCards(false);
                else if (playCard)
                    CardHolder_CardClicked(null, null);

                break;
            }
            case MultiplayerEventTypes.CClientCantJoin or MultiplayerEventTypes.CDuplicateNames or
                MultiplayerEventTypes.ClientReceived or MultiplayerEventTypes.CClientConnected or
                MultiplayerEventTypes.CClientDisconnected:
            default:
                throw new ArgumentOutOfRangeException();
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
                case CODE_CLIENT_INFO_REJOIN:
                    RestoreFromInfo(msgParts);
                    SendMessage(new List<string> { CODE_HOST_READY_TO_START, _playerIndex.ToString() });
                    break;
                case CODE_CLIENT_SHUFFLED_CARDS:
                    ShuffleCards(bool.Parse(msgParts[33]), msgParts);
                    break;
                case CODE_VIEW_CARD:
                    ApplyUserSeesFullCards(msgParts);
                    break;
                case CODE_GAME_MODE:
                    ApplyChoseGameOfOtherUser(msgParts);
                    break;
                case CODE_KONTRA_RE:
                    ApplyKontraReOfOtherUser(msgParts);
                    break;
                case CODE_PLAY_CARD:
                    if (ApplyCardClickOfOtherUser(msgParts))
                        CardHolder_CardClicked(null, null);

                    break;
                case CODE_CONTINUE:
                    ApplyContinueOfOtherUser(msgParts);
                    break;
                case CODE_CLIENT_LOST_CONNECTION:
                    ApplyConnectionToUserLost(msgParts);
                    break;
                case CODE_CLIENT_KONTRA_RE_INVALID:
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

    private void SendMessage(IEnumerable<string> message) => _connection.SendMessage(message, SEPARATOR);

    private void SendMessage(string id, IEnumerable<string> message) => _connection.SendMessage(message, SEPARATOR, id);

    private void SendMessage(params string[] message) => _connection.SendMessage(message, SEPARATOR);

    public void EndConnection() => _connection.EndConnection();

    private void DebugLog() => _connection.WriteLine(
        _match.WriteCurrentIndices().Append(", PlIndexCurRnd: ").Append(PlayerIndexCurRound).ToString());

    private IEnumerable<string> GenerateRejoinInfo()
    {
        List<string> message =
        [
            CODE_CLIENT_INFO_REJOIN,
            _match.InfoForRejoin(SEPARATOR, SEPARATOR_REJOIN_INFO_STRING)
        ];
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
        _match.RestoreFromInfo(msgParts, SEPARATOR_REJOIN_INFO);
        if (_match.IsGameOver)
        {
            ShowSummary(false);
            if (_match.Players[_playerIndex].ContinueMatch == true)
            {
                BtnPlayerNextMatch.Visibility = Visibility.Collapsed;
                _cvNextMatch[_playerIndex].Visibility = Visibility.Visible;
            }
        }
        else
        {
            if (_match.HasLastStich)
            {
                FillLastStich(_match.PreviousRound);
                BtnLastStich.IsEnabled = true;
            }

            string[] state = msgParts[_match.Players.Count + 2].Split(SEPARATOR_REJOIN_INFO);
            List<Card> cards = _match.CurrentRound.CurrentCards;
            int offset = _match.CurrentRound.StartPlayer;
            if (PlayerIndexCurRound == -1)
            {
                CanQuitNow = false;
                _isSpectating = true;
                SpectatorView.Visibility = Visibility.Visible;
                for (int i = 0; i < PLAYER_PER_ROUND; ++i)
                {
                    _playerInfosSpectate[i].Name = _match.CurrentPlayers[i].Name;
                    _playerInfosSpectate[i].Aufgestellt = _match.CurrentPlayers[i].Aufgestellt;
                    _playerInfosSpectate[i].Kontra = _match.CurrentPlayers[i].Kontra;
                    if (state[i].Equals(SkPlayerInfo.STATE_EMPTY))
                        _playerInfosSpectate[i].EmptyState();
                    else
                        _playerInfosSpectate[i].State = state[i];

                    if (state[i].Equals(SkPlayerInfo.STATE_AUFSTELLEN))
                        _chSpectate[i].Reset();
                    else
                        _chSpectate[i].ShowAllCards(_match.CurrentPlayers[i].Aufgestellt);

                    _chSpectate[i].Cards = _match.CurrentPlayers[i].GetPlayableCards();
                }

                for (int i = 0; i < cards.Count; ++i)
                {
                    CurrentCardsSpectate.AddCard(cards[i],
                        _match.CurrentPlayers[(i + offset) % PLAYER_PER_ROUND].Number);
                }
            }
            else
            {
                ViewPlaying.Visibility = Visibility.Visible;
                _isSpectating = CanQuitNow = false;

                for (int i = 0; i < PLAYER_PER_ROUND; ++i)
                {
                    var index = GetUiPlayerIndex(i);
                    _playerInfos[index].PlayerName = _match.CurrentPlayers[i].Name;
                    _playerInfos[index].Aufgestellt = _match.CurrentPlayers[i].Aufgestellt;
                    _playerInfos[index].Kontra = _match.CurrentPlayers[i].Kontra;
                    if (state[i].Equals(SkPlayerInfo.STATE_EMPTY))
                        _playerInfos[i].EmptyState();
                    else
                        _playerInfos[index].State = state[i];
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
                        ApplyPossibilities();
                        if (_playerInfos[0].State.Equals(SkPlayerInfo.STATE_EMPTY))
                            ModeSelector.State = GameSelectorState.Visible;
                        else
                        {
                            if (_match.PlayerIndex == PlayerIndexCurRound)
                            {
                                int w = 0;
                                while (_match.Players[_playerIndex].Possibilities[w].Mode != _match.MinimumGame)
                                    ++w;

                                ModeSelector.CbMode.SelectedIndex = w;
                                int color = 0;
                                while (!_match.Players[_playerIndex].Possibilities[w].Colors[color]
                                           .Equals(_match.Color))
                                    ++color;

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
                        GetUiPlayerIndex(_match.CurrentPlayers[(i + offset) % PLAYER_PER_ROUND].Number));
                }
            }
        }

        UpdateFocus(null);
    }

    private void ApplyKontraReOfOtherUser(IReadOnlyList<string> msgParts)
    {
        int index = int.Parse(msgParts[1]);
        _match.CurrentPlayers[index].Kontra = true;
        if (_isSpectating)
            _playerInfosSpectate[index].Kontra = true;
        else
        {
            _playerInfos[GetUiPlayerIndex(index)].Kontra = true;
            if (_match.CurrentPlayers[index].TeamIndex == _match.CurrentPlayers[PlayerIndexCurRound].TeamIndex)
                BtnKontra.Visibility = Visibility.Hidden;
            else if (_match.CurrentPlayers[index].TeamIndex -
                     _match.CurrentPlayers[PlayerIndexCurRound].TeamIndex == 1)
                BtnKontra.Visibility = Visibility.Visible;
        }
    }

    private bool ApplyCardClickOfOtherUser(IReadOnlyList<string> msgParts)
    {
        int player = int.Parse(msgParts[1]);
        int selected = int.Parse(msgParts[2]);
        if (!PlayCard(player, selected))
            return false;

        if (_isSpectating)
        {
            _chSpectate[player].RemoveCard(selected);
            return false;
        }

        return !_match.IsGameOver;
    }

    private bool ApplyChoseGameOfOtherUser(IReadOnlyList<string> msgParts)
    {
        if (ChoseGameMode(int.Parse(msgParts[1]), SchafkopfMatchConfig.StringToSchafkopfMode(msgParts[2]),
                msgParts[3]))
            return true;

        if (!_isSpectating)
            ModeSelector.TrySelect();
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
            _playerInfos[GetUiPlayerIndex(index)].Aufgestellt = aufgestellt;
    }

    private bool ApplyContinueOfOtherUser(IReadOnlyList<string> msgParts)
    {
        int index = int.Parse(msgParts[1]);
        _match.Players[index].ContinueMatch = bool.Parse(msgParts[2]);
        _cvNextMatch[index].IsChecked = _match.Players[index].ContinueMatch;
        return _connection.IsHost && StartNextMatch();
    }

    private void ApplyConnectionToUserLost(int missingPlayerCount, int index, MultiplayerPlayerState state,
        string newId)
    {
        _match.Players[index].State = state;
        if (state == MultiplayerPlayerState.Active)
            _match.Players[index].Id = newId;
        if (ViewPlaying.Visibility == Visibility.Visible)
            _gridBeforeConnectionError = ViewPlaying;
        else if (SpectatorView.Visibility == Visibility.Visible)
            _gridBeforeConnectionError = ViewPlaying; // TODO should these two be the same
        else
            return;

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
            for (int i = 0; i < _playerInfosSpectate.Count; ++i)
            {
                if (samePlayer)
                    _playerInfosSpectate[i].NewMatch();
                else
                    _playerInfosSpectate[i].NewMatch(_match.CurrentPlayers[i].Name);
            }

            _playerInfosSpectate[_match.CurrentRound.CurrentPlayer].IsStartPlayer = true;
        }
        else
        {
            for (int i = 0; i < _playerInfos.Count; ++i)
            {
                if (samePlayer)
                    _playerInfos[GetUiPlayerIndex(i)].NewMatch();
                else
                    _playerInfos[GetUiPlayerIndex(i)].NewMatch(_match.CurrentPlayers[i].Name);
            }

            ApplyPossibilities();
            ModeSelector.State = GameSelectorState.Hidden;
            BtnKontra.Visibility = Visibility.Hidden;
            if (VisualPlayer.Aufgestellt)
                CardHolder_ShowsAllCards(null, null);
            _playerInfos[GetUiPlayerIndex(_match.CurrentRound.CurrentPlayer)].IsStartPlayer = true;
        }

        UpdateFocus(false);
    }

    private void ApplyPossibilities() => ModeSelector.Source = _match.CurrentPlayers[PlayerIndexCurRound].Possibilities;

    private void MarkPlayableCards()
    {
        if (!_isSpectating)
            CardHolder.MarkSelectableCards(_match.PlayableCards[PlayerIndexCurRound]);
    }

    private void UpdateCardVisual(bool forceReset, bool tryReset)
    {
        if (_isSpectating)
        {
            for (int i = 0; i < PLAYER_PER_ROUND; ++i)
            {
                if (forceReset || tryReset && !_chSpectate[i].Aufgestellt)
                    _chSpectate[i].Reset();
                _chSpectate[i].Cards = _match.CurrentPlayers[i].GetPlayableCards();
            }
        }
        else
        {
            if (forceReset || tryReset && !CardHolder.Aufgestellt)
                CardHolder.Reset();
            CardHolder.Cards = _match.CurrentPlayers[PlayerIndexCurRound].GetPlayableCards();
        }
    }

    private void UpdateFocus(bool? cardHolder)
    {
        int current = _match.CurrentRound.CurrentPlayer;
        if (_isSpectating)
        {
            for (int i = 0; i < _playerInfosSpectate.Count; ++i)
                _playerInfosSpectate[i].Focused = i == current;

            if (cardHolder != true)
                return;

            for (int i = 0; i < _chSpectate.Count; ++i)
                _chSpectate[i].Focused = i == current;
        }
        else
        {
            for (int i = 0; i < _playerInfos.Count; ++i)
                _playerInfos[GetUiPlayerIndex(i)].Focused = i == current;

            if (cardHolder == true)
                CardHolder.Focused = current == PlayerIndexCurRound;
            else if (cardHolder.HasValue)
                ModeSelector.SetGameSelectorFocus(current == PlayerIndexCurRound);
        }
    }

    private void FillLastStich(SchafkopfRound round)
    {
        _lastStichView = new StichView();
        if (_isSpectating)
        {
            for (int i = 0; i < PLAYER_PER_ROUND; ++i)
            {
                _lastStichView.AddCard(round.CurrentCards[i],
                    _match.CurrentPlayers[(round.StartPlayer + i) % PLAYER_PER_ROUND].Number);
            }
        }
        else
        {
            for (int i = 0; i < PLAYER_PER_ROUND; ++i)
            {
                _lastStichView.AddCard(round.CurrentCards[i],
                    GetUiPlayerIndex(_match.CurrentPlayers[(round.StartPlayer + i) % PLAYER_PER_ROUND].Number));
            }
        }
    }

    private void ShowSummary(bool evaluation = true)
    {
        bool? tmp = _match.HasPlayerWon(PlayerIndexCurRound);
        BorderSummary.BorderBrush =
            tmp.HasValue ? tmp.Value ? Brushes.Green : Brushes.Red : Brushes.Transparent;

        CanQuitNow = true;
        TbPointsSummary.Text = _match.FullSummary(PlayerIndexCurRound);
        int i;
        for (i = 0; i < _match.Players.Count; ++i)
        {
            if (evaluation) _match.Players[i].ContinueMatch = null;
            _cvNextMatch[i].IsChecked = _match.Players[i].ContinueMatch;
            _lblPlayerSummaryNames[i].Content = _match.Players[i].Name;
            _lblPlayerSummaryPoints[i].Content = _match.Players[i].Points;
            _gridColsSummary[i].Width = new GridLength(1, GridUnitType.Star);
        }

        for (; i < _match.Players.Count; ++i)
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
            UpdatePoints();

        if (QuitAfterMatch)
        {
            BtnPoints_Click(null, null);
            QuitMatch?.Invoke();
        }

        BtnPoints.IsEnabled = true;
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
            BtnLastStich.IsEnabled = false;

        GridRoundSummary.Visibility = Visibility.Collapsed;
        if (_connection.IsHost)
            SendMessage(_match.ShuffleCards(sameRound, CODE_CLIENT_SHUFFLED_CARDS));
        else
            _match.ShuffleCards(msgParts, sameRound);

        //unnötig durch umstellung auf playInCurRou { get {...}} playerIndexCurRound = match.players[playerIndex].number;
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
        return (index + PLAYER_PER_ROUND - PlayerIndexCurRound) % PLAYER_PER_ROUND;
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

        Tuple<bool, bool> result = _match.ChoseGameMode(mode, color, index);
        if (result.Item1) return true;
        if (result.Item2)
        {
            ModeSelector.State = GameSelectorState.Hidden;
            MarkPlayableCards();
            CardHolder.CanClickCards = _match.CurrentRound.CurrentPlayer == PlayerIndexCurRound;
            UpdateCardVisual(false, false);
            UpdateFocus(true);
            if (_isSpectating)
            {
                for (int i = 0; i < PLAYER_PER_ROUND; ++i)
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
                for (int i = 0; i < PLAYER_PER_ROUND; ++i)
                {
                    _playerInfos[i].EmptyState();
                }

                _playerInfos[uiIndex].State = _match.ToString();
                _playerInfos[GetUiPlayerIndex(_match.CurrentRound.CurrentPlayer)].IsStartPlayer = false;
            }
        }
        else
        {
            if (!_isSpectating && mode != SchafkopfMode.Weiter)
                ModeSelector.CheckIfSelectedStillValid(mode, _match, _match.CurrentPlayers[PlayerIndexCurRound]);
        }

        UpdateFocus(false);
        return false;
    }

    /// <summary>
    /// Plays the given card by the given player if possible.
    /// </summary>
    /// <param name="player">Index of the player in the array of current players.</param>
    /// <param name="selected">Index of the card the player wants to play of his playable cards.</param>
    /// <returns><c>true</c> if the card was played</returns>
    private bool PlayCard(int player, int selected)
    {
        Card card = _match.CurrentPlayers[player].PlayableCards[selected];
        if (!_match.PlayCard(selected, player))
            return false;

        if (_match.CurrentCardCount == 1)
        {
            if (_isSpectating) CurrentCardsSpectate.Reset();
            else
            {
                CurrentCards.Reset();
                CardHolder.CanClickCards = _match.CurrentRound.CurrentPlayer != PlayerIndexCurRound;
            }

            MarkPlayableCards();
        }

        if (_isSpectating)
            CurrentCardsSpectate.AddCard(card, _match.CurrentPlayers[player].Number);
        else
            CurrentCards.AddCard(card, GetUiPlayerIndex(_match.CurrentPlayers[player].Number));

        if (_match.CurrentCardCount is 0)
        {
            FillLastStich(_match.PreviousRound);
            BtnLastStich.IsEnabled = true;
        }


        if (_match.IsGameOver)
        {
            FillLastStich(_match.PreviousRound);
            CardHolder.Focused = false;
            Util.AllowUiToUpdate();
            Thread.Sleep(1500);
            ShowSummary();
            return true;
        }

        if (_match.CurrentCardCount == 0)
        {
            MarkPlayableCards();
            CardHolder.CanClickCards = _match.CurrentRound.CurrentPlayer == PlayerIndexCurRound;
        }

        UpdateFocus(true);
        return true;
    }

    private bool StartNextMatch()
    {
        int w = 0;
        while (w < _match.Players.Count && _match.Players[w].ContinueMatch == true)
            ++w;

        return w == _match.Players.Count;
        //TODO handle player leaving the match
    }

    private void UpdatePoints() => _points.Update(_match.PlayerPoints, _match.PointsSingle, _match.PointsCumulated);

    #endregion

    #region UI-Listener

    private void BtnKontra_Click(object sender, RoutedEventArgs e)
    {
        _match.Players[_playerIndex].Kontra = true;
        VisualPlayer.Kontra = true;
        BtnKontra.Visibility = Visibility.Hidden;
        SendMessage(CODE_KONTRA_RE, PlayerIndexCurRound.ToString());
    }

    private void CardHolder_ShowsAllCards(object sender, EventArgs e)
    {
        VisualPlayer.Aufgestellt = _match.Players[_playerIndex].Aufgestellt =
            CardHolder.Aufgestellt || _match.Players[_playerIndex].Aufgestellt;
        SendMessage(CODE_VIEW_CARD, PlayerIndexCurRound.ToString(), CardHolder.Aufgestellt.ToString());
        ApplyPossibilities();
        ModeSelector.State = GameSelectorState.Visible;
        UpdateFocus(false);
    }

    private void CardHolder_CardClicked(object sender, EventArgs e)
    {
        if (CardHolder.SelectedCard == -1 || _match.CurrentRound.CurrentPlayer != PlayerIndexCurRound)
            return;

        if (BtnKontra.Visibility == Visibility.Visible)
            BtnKontra.Visibility = Visibility.Hidden;

        int selected = CardHolder.SelectedCard;
        SendMessage(new List<string> { CODE_PLAY_CARD, PlayerIndexCurRound.ToString(), selected.ToString() });
        if (PlayCard(PlayerIndexCurRound, selected))
            CardHolder.RemoveSelectedCard();

        if (_match.CurrentCardCount != 0)
            CardHolder.CanClickCards = false;
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
            UpdatePoints();

        _points.Show();
    }

    private void PointsWindowClosed(object sender, CancelEventArgs e) => _points = null;

    private void BtnPlayerNextMatch_Click(object sender, RoutedEventArgs e)
    {
        BtnPlayerNextMatch.Visibility = Visibility.Collapsed;
        _cvNextMatch[_playerIndex].Visibility = Visibility.Visible;
        _cvNextMatch[_playerIndex].IsChecked = true;
        _match.Players[_playerIndex].ContinueMatch = true;
        SendMessage(new List<string> { CODE_CONTINUE, _playerIndex.ToString(), "true" });
        if (_connection.IsHost && StartNextMatch())
            ShuffleCards(false);
    }

    private void BtnPlayerQuit_Click(object sender, RoutedEventArgs e)
    {
        _match.Players[_playerIndex].ContinueMatch = false;
        if (_connection.IsHost)
        {
            /* TODO host wants to end the game */
        }
    }

    private void ModeSelector_ColorChanged(GameModeSelectedEvent e)
    {
        int index = PlayerIndexCurRound;
        _match.CurrentPlayers[index].SortCards(new SchafkopfMatchConfig(e.Mode, e.Color));
        if (_isSpectating)
            _chSpectate[index].Cards = _match.CurrentPlayers[index].GetPlayableCards();
        else
            CardHolder.Cards = _match.CurrentPlayers[index].GetPlayableCards();
    }

    private void ModeSelector_ModeSelected(GameModeSelectedEvent e)
    {
        if (_match.CurrentRound.CurrentPlayer != PlayerIndexCurRound) return;

        if (AllPlayerSeeAllCards)
        {
            ModeSelector.State = GameSelectorState.Selected;
            SendMessage(new List<string>
                { CODE_GAME_MODE, PlayerIndexCurRound.ToString(), e.Mode.ToString(), e.Color });
            if (ChoseGameMode(PlayerIndexCurRound, e.Mode, e.Color))
            {
                ModeSelector.State = GameSelectorState.Visible;
                if (_connection.IsHost)
                    ShuffleCards(true);
            }
        }
        else
            ModeSelector.BtnSelectMode.IsChecked = false;
    }

    #endregion
}