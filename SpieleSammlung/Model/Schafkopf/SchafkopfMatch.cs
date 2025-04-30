#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using SpieleSammlung.Model.Multiplayer;
using SpieleSammlung.Model.Util;
using static SpieleSammlung.Model.Schafkopf.CardColor;

#endregion

namespace SpieleSammlung.Model.Schafkopf;

public class SchafkopfMatch : SchafkopfMatchConfig
{
    public StringBuilder WriteCurrentIndices()
    {
        StringBuilder bob = new StringBuilder(20);
        bob.Append("curPlayers: [");
        bob.Append(string.Join(", ", _currentPlayerIndexes));
        bob.Append("] -> curNames: [");
        bob.Append(ArrayPrinter.ArrayString(i => CurrentPlayers[i].Name, CurrentPlayers.Count));
        bob.Append("]");
        return bob;
    }

    #region Constants

    public const int PLAYER_PER_ROUND = 4;
    private const int ROUNDS_COUNT = 8;

    #endregion

    #region Properties

    public IReadOnlyList<SchafkopfPlayer> Players { get; }
    public IReadOnlyList<SchafkopfPlayer> CurrentPlayers { get; private set; }

    public IReadOnlyList<PointsStorage> PlayerPoints => Players.Select(player => player.PlayerPoints).ToList();
    public DataTable PointsSingle { get; }
    public DataTable PointsCumulated { get; }

    public SchafkopfMode MinimumGame { get; private set; }
    public CardColor? Color { get; private set; }
    public int AmountShuffle { get; private set; }
    public int PlayerIndex { get; private set; }
    public bool IsWegGelaufen { get; private set; }
    public IReadOnlyList<Card> LastCards { get; private set; }
    public IReadOnlyList<IReadOnlyList<bool>> PlayableCards { get; private set; }

    public int CurrentCardCount => CurrentRound.CurrentCards.Count;
    public SchafkopfRound CurrentRound => _rounds[_rounds.Count - 1];

    public SchafkopfRound PreviousRound =>
        _rounds.Count < 2 ? null : IsGameOver ? CurrentRound : _rounds[_rounds.Count - 2];

    public bool IsGameOver => _rounds.Count == ROUNDS_COUNT && CurrentCardCount == PLAYER_PER_ROUND;
    public bool HasLastStich => !IsGameOver && _rounds.Count > 1;

    #endregion

    #region Private Fields

    private readonly Random _random;
    private readonly bool _isHost;
    private List<SchafkopfRound> _rounds;
    private readonly int[] _currentPlayerIndexes;
    private readonly int[] _teams;
    private readonly int[] _teamsAnzStiche;
    private int _laufende;
    private int _loser;
    private string _matchSummary;
    private string _generalPointsSummary;
    private bool _isShufflePossible;

    #endregion

    #region Constructor

    public SchafkopfMatch(IEnumerable<MultiplayerPlayer> players, bool isHost) : this(players, new Random(), isHost)
    {
    }

    public SchafkopfMatch(IEnumerable<MultiplayerPlayer> players, Random random, bool isHost)
    {
        Players = players.Select(t => new SchafkopfPlayer(t.Id, t.Name)).ToList();

        _isHost = isHost;
        _random = random;
        ResetMode();
        _currentPlayerIndexes = Players.Count switch
        {
            < 6 => [Players.Count - 1, 0, 1, 2], //next match gleich zu begin ->  0, 1, 2, 3
            6 => [5, 1, 2, 4], //next match gleich zu begin ->  0, 2, 3, 5
            _ => [6, 1, 3, 5] //next match gleich zu begin ->  0, 2, 4, 6
        };

        SchafkopfPlayer[] newList = new SchafkopfPlayer[PLAYER_PER_ROUND];
        for (int i = 0; i < PLAYER_PER_ROUND; ++i) newList[i] = Players[_currentPlayerIndexes[i]];
        CurrentPlayers = newList;

        PlayableCards = new IReadOnlyList<bool>[PLAYER_PER_ROUND];
        _teams = new int[2];
        _teamsAnzStiche = new int[2];
        PointsSingle = new DataTable();
        PointsCumulated = new DataTable();
        foreach (var player in Players)
        {
            PointsSingle.Columns.Add(player.Name);
            PointsCumulated.Columns.Add(player.Name);
        }

        _isShufflePossible = true;
    }

    #endregion

    #region Public Methods

    public Tuple<bool, bool> ChoseGameMode(SchafkopfMode mode, string color, int index)
        => ChoseGameMode(mode, Card.ParseNullableColor(color), index);

    public Tuple<bool, bool> ChoseGameMode(SchafkopfMode mode, CardColor? color, int index)
    {
        if (index != CurrentRound.CurrentPlayer)
            throw new IllegalMoveException("It is not this players turn");

        return ChoseGameMode(mode, color);
    }

    public Tuple<bool, bool> ChooseGameMode(SchafkopfMode mode, string color) =>
        ChoseGameMode(mode, Card.ParseNullableColor(color));

    public Tuple<bool, bool> ChoseGameMode(SchafkopfMode mode, CardColor? color)
    {
        if (mode > MinimumGame)
        {
            MinimumGame = mode;
            PlayerIndex = CurrentRound.CurrentPlayer;
            Color = color;
        }

        CurrentRound.SetNextPlayer();
        if (mode == SchafkopfMode.Weiter) ++AmountShuffle;
        if (AmountShuffle == PLAYER_PER_ROUND)
        {
            _isShufflePossible = true;
            return new Tuple<bool, bool>(true, true);
        }

        if (AmountShuffle == 3 && MinimumGame != SchafkopfMode.Weiter)
        {
            ChooseGame();
            CurrentRound.ResetNextPlayer();
            CurrentPlayers[PlayerIndex].TeamIndex = 0;
            int end = PlayerIndex + PLAYER_PER_ROUND;
            if (Mode == SchafkopfMode.Sauspiel)
            {
                for (int i = PlayerIndex + 1; i < end; ++i)
                {
                    CurrentPlayers[i % PLAYER_PER_ROUND].TeamIndex =
                        CurrentPlayers[i % PLAYER_PER_ROUND].HasGesuchte(this) ? 0 : 1;
                }
            }
            else
            {
                for (int i = PlayerIndex + 1; i < end; ++i)
                {
                    CurrentPlayers[i % PLAYER_PER_ROUND].TeamIndex = 1;
                }
            }

            CalculateLaufende();
            return new Tuple<bool, bool>(false, true);
        }

        return new Tuple<bool, bool>(false, false);
    }

    public IEnumerable<string> ShuffleCards(bool sameRound = false, string codeClientShuffledCards = "")
    {
        if (!_isHost) throw new IllegalMoveException("A client cannot shuffle the cards.");
        PrepareShuffle(sameRound);
        List<int> cards = [];
        for (int i = 0; i < Card.ALL_CARDS.Count; ++i)
            cards.Add(i);

        List<string> msg = [codeClientShuffledCards];
        for (int i = 0; i < Card.ALL_CARDS.Count; ++i)
        {
            int temp = _random.Next(0, cards.Count);
            CurrentPlayers[i % PLAYER_PER_ROUND].PlayableCards.Add(Card.ALL_CARDS[cards[temp]]);
            msg.Add(cards[temp].ToString());
            cards.RemoveAt(temp);
        }

        foreach (var player in CurrentPlayers)
            player.UpdatePossibilities(this);
        msg.Add(sameRound.ToString());
        return msg;
    }

    public void ShuffleCards(IReadOnlyList<string> msgParts, bool sameRound)
    {
        PrepareShuffle(sameRound);
        for (int i = 0; i < Card.ALL_CARDS.Count; ++i)
        {
            CurrentPlayers[i % PLAYER_PER_ROUND].PlayableCards.Add(Card.GetCard(int.Parse(msgParts[i + 1])));
        }

        foreach (var player in CurrentPlayers)
            player.UpdatePossibilities(this);
    }

    public bool PlayCard(int selected, int player) => player == CurrentRound.CurrentPlayer && PlayCard(selected);

    public bool PlayCard(int selected)
    {
        if (!CurrentPlayers[CurrentRound.CurrentPlayer].PlayCard(selected, this))
            return false;

        UpdatePlayableCards();
        if (CurrentCardCount < PLAYER_PER_ROUND)
            CurrentRound.SetNextPlayer();
        else
        {
            LastCards = CurrentRound.CurrentCards.ToList();
            NextRound();
            if (IsGameOver)
                Evaluation();
        }

        return true;
    }

    private void NextMatch()
    {
        ResetMode();
        _teams[0] = _teams[1] = _teamsAnzStiche[0] = _teamsAnzStiche[1] = 0;
        SchafkopfPlayer[] newList = new SchafkopfPlayer[PLAYER_PER_ROUND];
        for (int i = 0; i < PLAYER_PER_ROUND; ++i)
        {
            CurrentPlayers[i].Number = -1;
            _currentPlayerIndexes[i] = (_currentPlayerIndexes[i] + 1) % Players.Count;
            newList[i] = Players[_currentPlayerIndexes[i]];
        }

        CurrentPlayers = newList;
    }

    public string FullSummary(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex >= Players.Count)
            throw new ArgumentException("This player does not exist");
        return Players[playerIndex].Number == -1 ? _generalPointsSummary : PointsSummary(_loser, playerIndex);
    }

    public bool? HasPlayerWon(int player) => player == -1 ? null : CurrentPlayers[player].TeamIndex != _loser;

    #endregion

    #region Other gamelogic

    internal void SetIsWegGelaufen() => IsWegGelaufen = true;

    private void PrepareShuffle(bool sameRound)
    {
        if (!_isShufflePossible)
            throw new IllegalMoveException("The current game is not finished yet.");


        _isShufflePossible = false;
        if (!sameRound)
            NextMatch();
        AmountShuffle = 0;
        _rounds = [new SchafkopfRound()];
        ResetMode();
        for (int i = 0; i < PLAYER_PER_ROUND; ++i)
            CurrentPlayers[i].NewMatch(i, sameRound);
    }

    private void ChooseGame()
    {
        Mode = MinimumGame;
        if (Mode != SchafkopfMode.Sauspiel)
            SauspielFarbe = null;
        switch (Mode)
        {
            case SchafkopfMode.Sauspiel:
                Trumpf = Herz;
                SauspielFarbe = Color;
                break;
            case SchafkopfMode.Solo or SchafkopfMode.SoloTout:
                Trumpf = Color;
                break;
            case SchafkopfMode.Wenz or SchafkopfMode.WenzTout:
                Trumpf = null;
                break;
            case SchafkopfMode.Weiter:
                //TODO handle this case, probably shuffle or nextMatch
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        for (int i = 0; i < PLAYER_PER_ROUND; ++i)
            CurrentPlayers[i].SortCards(this);
        UpdatePlayableCards();
    }

    private void NextRound()
    {
        CalculateStichPoints();
        ++_teamsAnzStiche[CurrentPlayers[CurrentRound.NextStartPlayer].TeamIndex];
        if (_rounds.Count < ROUNDS_COUNT)
            _rounds.Add(new SchafkopfRound(CurrentRound));
        UpdatePlayableCards();
    }

    private void ResetMode()
    {
        Mode = SchafkopfMode.Weiter;
        MinimumGame = SchafkopfMode.Weiter;
        SauspielFarbe = null;
        Trumpf = null;
    }

    private void UpdatePlayableCards()
    {
        PlayableCards = CurrentPlayers.Select(player => player.CheckPlayableCards(this)).ToList();
    }

    private void CalculateLaufende()
    {
        int[,] values = new int[PLAYER_PER_ROUND, 8];
        for (int i = 0; i < PLAYER_PER_ROUND; ++i)
        {
            for (int j = 0; j < 8; ++j)
            {
                values[i, j] = CurrentPlayers[i].PlayableCards[j].GetValueOfThisCard(this);
            }
        }

        int maxValue = Mode is SchafkopfMode.Wenz or SchafkopfMode.WenzTout ? 12 : 20;

        int teamNumber = -1;
        int w = 0;
        while (w < 4)
        {
            if (values[w, 0] == maxValue)
            {
                teamNumber = CurrentPlayers[w].TeamIndex;
                break;
            }

            ++w;
        }

        List<int> team = [];
        for (int i = 0; i < CurrentPlayers.Count; ++i)
        {
            if (CurrentPlayers[i].TeamIndex == teamNumber)
                team.Add(i);
        }

        List<int> nextCards = [];
        for (int i = 0; i < team.Count; ++i)
            nextCards.Add(0);

        _laufende = 0;
        w = 0;
        while (w < 8 && _laufende < 8)
        {
            int e = 0;
            while (e < team.Count)
            {
                if (values[team[e], nextCards[e]] == maxValue)
                {
                    --maxValue;
                    ++_laufende;
                    ++nextCards[e];
                    break;
                }

                ++e;
            }

            if (_laufende <= w) break;
            ++w;
        }

        if (Mode is SchafkopfMode.Wenz or SchafkopfMode.WenzTout && _laufende < 2 || _laufende < 3)
        {
            _laufende = 0;
        }
    }

    private void Evaluation()
    {
        _isShufflePossible = true;
        int[] tmp = new int[Players.Count];
        int score = ScoreOfThisMatch();
        for (int i = 0; i < Players.Count; ++i)
        {
            if (Players[i].Number == -1)
                tmp[i] = 0;
            else if (Players[i].TeamIndex == _loser)
                tmp[i] = -score;
            else
                tmp[i] = score;
            if (Mode != SchafkopfMode.Sauspiel && Players[i].TeamIndex == 0)
                tmp[i] *= 3;
            Players[i].Points += tmp[i];
        }

        int row = PointsSingle.Rows.Count;
        PointsSingle.Rows.Add();
        PointsCumulated.Rows.Add();
        for (int i = 0; i < Players.Count; ++i)
        {
            int col = PointsSingle.Columns[Players[i].Name].Ordinal;
            PointsSingle.Rows[row][col] = tmp[i];
            PointsCumulated.Rows[row][col] = Players[i].Points;
        }
    }

    private string PointsSummary(int loser, int player)
    {
        StringBuilder summary = new StringBuilder();
        if (player == -1)
        {
            summary.Append(CurrentPlayers[PlayerIndex].Name).Append(" hat ");
            player = PlayerIndex;
        }
        else
            summary.Append("Du hast ");

        int teamIndex = CurrentPlayers[player].TeamIndex;
        summary.Append(Mode is SchafkopfMode.Wenz or SchafkopfMode.WenzTout ? "den " : "das ").Append(Mode);
        if (Mode == SchafkopfMode.Sauspiel)
        {
            summary.Append(" mit ");
            int w = 0;
            while (CurrentPlayers[w].TeamIndex != teamIndex || w == player)
                ++w;

            summary.Append(CurrentPlayers[w].Name);
        }
        else if (teamIndex != 0)
        {
            summary.Append(" mit ");
            int w = 0;
            int index = 0;
            while (index < 2)
            {
                if (CurrentPlayers[w].TeamIndex == teamIndex && w != player)
                {
                    summary.Append(CurrentPlayers[w].Name);
                    if (1 == ++index)
                        summary.Append(" und ");
                }

                ++w;
            }
        }

        summary.Append(" mit ").Append(_teams[teamIndex]).Append(" Augen");
        if (Mode is SchafkopfMode.SoloTout or SchafkopfMode.WenzTout && _teamsAnzStiche[1] > 0)
        {
            if (player == -1 || CurrentPlayers[player].TeamIndex == 0)
                summary.Append(" und ").Append(_teamsAnzStiche[1]).Append(" Gegenstich");
            else
                summary.Append(" und ").Append(_teamsAnzStiche[1]).Append(" Stich");
            if (_teamsAnzStiche[1] > 1)
                summary.Append("en");
        }

        summary.Append(loser == teamIndex ? " verloren" : " gewonnen").Append("\n\n");
        summary.Append(_matchSummary);
        return summary.ToString();
    }

    private int ScoreOfThisMatch()
    {
        int count;
        StringBuilder summary = new StringBuilder();
        int points = Mode == SchafkopfMode.Sauspiel ? 20 : 50;

        summary.Append("+").Append(points).Append(" ").Append(Mode);
        if (_laufende > 0)
        {
            _laufende *= 10;
            points += _laufende;
            summary.Append("\n+").Append(_laufende).Append(" Laufende");
        }

        _loser = 1;
        if (Mode is SchafkopfMode.WenzTout or SchafkopfMode.SoloTout)
        {
            if (_teamsAnzStiche[1] > 0)
                _loser = 0;
            points *= 2;
            summary.Append("\n*2 tout");
        }
        else
        {
            count = CurrentPlayers.Count(player => player.Kontra);
            if ((count == 1 && _teams[0] < _teams[1]) || _teams[0] <= _teams[1])
                _loser = 0;

            // if (_teams[_loser] < 30 || _loser != count % 2 && _teams[_loser] < 31)
            if (_loser == 0 && _teams[_loser] < 31 || _loser == 1 && _teams[_loser] < 30)
            {
                if (_teamsAnzStiche[_loser] == 0)
                {
                    points += 20;
                    summary.Append("\n+20 Schneiderschwarz");
                }
                else
                {
                    points += 10;
                    summary.Append("\n+10 Schneider");
                }
            }
        }

        foreach (var _ in CurrentPlayers.Where(player => player.Aufgestellt))
        {
            points *= 2;
            summary.Append("\n*2 aufgestellt");
        }

        count = 0;
        foreach (var _ in CurrentPlayers.Where(player => player.Kontra))
        {
            points *= 2;
            summary.Append(++count == 1 ? "\n*2 Kontra" : "\n*2 Re");
        }

        summary.Append("\n_________________\n").Append(points);
        _matchSummary = summary.ToString();
        _generalPointsSummary = PointsSummary(_loser, -1);
        return points;
    }

    private void CalculateStichPoints()
    {
        int sum = CurrentRound.CurrentCards.Sum(card => card.Points);
        _teams[CurrentPlayers[CurrentRound.NextStartPlayer].TeamIndex] += sum;
    }

    #endregion

    #region Joining

    public string InfoForRejoin(char separator, string fineSeparator)
    {
        StringBuilder bob = new StringBuilder();
        foreach (var player in Players)
            bob.Append(player.InfoForRejoin(fineSeparator)).Append(separator);

        foreach (var index in _currentPlayerIndexes)
            bob.Append(index).Append(fineSeparator);

        bob.Append(Mode.ToString()).Append(fineSeparator);
        bob.Append(Card.StringifyColor(Trumpf)).Append(fineSeparator);
        bob.Append(Card.StringifyColor(SauspielFarbe)).Append(fineSeparator);
        bob.Append(IsWegGelaufen.ToString()).Append(fineSeparator);
        bob.Append(MinimumGame.ToString()).Append(fineSeparator);
        bob.Append(Card.StringifyColor(Color)).Append(fineSeparator);
        bob.Append(AmountShuffle).Append(fineSeparator);
        bob.Append(PlayerIndex).Append(fineSeparator);
        bob.Append(_teams[0]).Append(fineSeparator);
        bob.Append(_teams[1]).Append(fineSeparator);
        bob.Append(_teamsAnzStiche[0]).Append(fineSeparator);
        bob.Append(_teamsAnzStiche[1]).Append(fineSeparator);
        bob.Append(_laufende).Append(fineSeparator);
        bob.Append(_loser).Append(fineSeparator);
        if (LastCards == null || LastCards.Count < 1)
            bob.Append("null").Append(fineSeparator);
        else
        {
            foreach (var card in LastCards)
                bob.Append(card.Index).Append(fineSeparator);
        }

        bob.Append(_rounds.Count).Append(fineSeparator);
        foreach (var round in _rounds)
        {
            bob.Append(round.InfoForRejoin(fineSeparator)).Append(fineSeparator);
        }

        bob.Append(_matchSummary).Append(fineSeparator);
        bob.Append(_generalPointsSummary);
        return bob.ToString();
    }

    public void RestoreFromInfo(IReadOnlyList<string> msgParts, char fineSeparator)
    {
        for (int i = 0; i < Players.Count; ++i)
        {
            Players[i].RestoreFromInfo(msgParts[i + 1], fineSeparator);
        }

        RestoreFromInfo(msgParts[Players.Count + 1], fineSeparator);
    }

    private void RestoreFromInfo(string info, char separator)
    {
        SchafkopfPlayer[] newList = new SchafkopfPlayer[PLAYER_PER_ROUND];
        foreach (var player in Players.Where(player => player.Number != -1))
            newList[player.Number] = player;
        CurrentPlayers = newList;

        string[] msgParts = info.Split(separator);
        int index = -1;
        for (int i = 0; i < _currentPlayerIndexes.Length; ++i)
        {
            _currentPlayerIndexes[i] = int.Parse(msgParts[++index]);
        }

        Mode = StringToSchafkopfMode(msgParts[++index]);
        Trumpf = Card.ParseNullableColor(msgParts[++index]);
        SauspielFarbe = Card.ParseNullableColor(msgParts[++index]);
        IsWegGelaufen = bool.Parse(msgParts[++index]);
        MinimumGame = StringToSchafkopfMode(msgParts[++index]);
        Color = Card.ParseNullableColor(msgParts[++index]);
        AmountShuffle = int.Parse(msgParts[++index]);
        PlayerIndex = int.Parse(msgParts[++index]);
        _teams[0] = int.Parse(msgParts[++index]);
        _teams[1] = int.Parse(msgParts[++index]);
        _teamsAnzStiche[0] = int.Parse(msgParts[++index]);
        _teamsAnzStiche[1] = int.Parse(msgParts[++index]);
        _laufende = int.Parse(msgParts[++index]);
        _loser = int.Parse(msgParts[++index]);
        ++index;
        if (msgParts[index].Equals("null"))
        {
            ++index;
        }
        else
        {
            Card[] cards = new Card[PLAYER_PER_ROUND];
            for (int i = 0; i < PLAYER_PER_ROUND; ++i)
                cards[i] = Card.ALL_CARDS[int.Parse(msgParts[index++])];
            LastCards = cards;
        }

        int count = int.Parse(msgParts[index]);
        _rounds = [];
        for (int i = 0; i < count; ++i)
            _rounds.Add(new SchafkopfRound(msgParts, ref index));

        _matchSummary = msgParts[++index];
        _generalPointsSummary = msgParts[index + 1];
        if (Mode != SchafkopfMode.Weiter)
            UpdatePlayableCards();
    }

    #endregion
}