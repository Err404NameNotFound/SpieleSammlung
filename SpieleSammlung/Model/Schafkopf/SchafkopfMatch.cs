﻿using SpieleSammlung.Model.Multiplayer;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using SpieleSammlung.Model.Util;

namespace SpieleSammlung.Model.Schafkopf
{
    public class SchafkopfMatch : SchafkopfMatchConfig
    {
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
        public string Color { get; private set; }
        public int AmountShuffle { get; private set; }
        public int PlayerIndex { get; private set; }
        public bool IsWegGelaufen { get; private set; }
        public IReadOnlyList<Card> LastCards { get; private set; }
        public IReadOnlyList<IReadOnlyList<bool>> PlayableCards { get; private set; }

        public int CurrentCardCount => CurrentRound.currentCards.Count;
        public SchafkopfRound CurrentRound => _rounds[_rounds.Count - 1];

        public SchafkopfRound PreviousRound => _rounds.Count < 2 ? null
            : _rounds.Count == 8 && CurrentCardCount == PLAYER_PER_ROUND ? CurrentRound : _rounds[_rounds.Count - 1];

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
        private string _personalPointsSummary;
        private string _generalPointsSummary;

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
                < 6 => new[] { 3, 0, 1, 2 }, //next match gleich zu begin ->  0, 1, 2, 3
                6 => new[] { 5, 1, 2, 4 }, //next match gleich zu begin ->  0, 2, 3, 5
                _ => new[] { 6, 1, 3, 5 } //next match gleich zu begin ->  0, 2, 4, 6
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
        }

        #endregion

        #region Public Methods

        public Tuple<bool, bool> ChoseGameMode(int index, SchafkopfMode mode, string color)
        {
            if (mode > MinimumGame)
            {
                MinimumGame = mode;
                PlayerIndex = index;
                Color = color;
            }

            if (mode == SchafkopfMode.Weiter) ++AmountShuffle;
            if (AmountShuffle == PLAYER_PER_ROUND) return new Tuple<bool, bool>(true, true);
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

            CurrentRound.SetNextPlayer();
            return new Tuple<bool, bool>(false, false);
        }

        // TODO make method private
        public IEnumerable<string> ShuffleCards(bool sameRound, string codeClientShuffledCards)
        {
            if (!_isHost) throw new NotSupportedException("A client cannot shuffle the cards.");
            AmountShuffle = 0;
            _rounds = new List<SchafkopfRound> { new() };
            ResetMode();
            for (int i = 0; i < PLAYER_PER_ROUND; ++i) CurrentPlayers[i].NewMatch(i, sameRound);
            List<int> cards = new List<int>();
            for (int i = 0; i < Card.ALL_CARDS.Count; ++i) cards.Add(i);

            List<string> msg = new List<string> { codeClientShuffledCards };
            for (int i = 0; i < Card.ALL_CARDS.Count; ++i)
            {
                int temp = _random.Next(0, cards.Count);
                CurrentPlayers[i % PLAYER_PER_ROUND].PlayableCards.Add(Card.GetCard(cards[temp]));
                msg.Add(cards[temp].ToString());
                cards.RemoveAt(temp);
            }

            foreach (var player in CurrentPlayers) player.UpdatePossibilities(this);

            msg.Add(sameRound.ToString());
            return msg;
        }

        public void ShuffleCards(IReadOnlyList<string> msgParts, bool sameRound)
        {
            AmountShuffle = 0;
            _rounds = new List<SchafkopfRound> { new() };
            ResetMode();
            for (int i = 0; i < PLAYER_PER_ROUND; ++i) CurrentPlayers[i].NewMatch(i, sameRound);
            for (int i = 0; i < Card.ALL_CARDS.Count; ++i)
            {
                CurrentPlayers[i % PLAYER_PER_ROUND].PlayableCards.Add(Card.GetCard(int.Parse(msgParts[i + 1])));
            }

            foreach (var player in CurrentPlayers) player.UpdatePossibilities(this);
        }

        public bool PlayCard(int player, int selected)
        {
            if (player != CurrentRound.CurrentPlayer || !CurrentPlayers[player].PlayCard(selected, this)) return false;
            UpdatePlayableCards();
            if (CurrentCardCount < PLAYER_PER_ROUND) CurrentRound.SetNextPlayer();
            else
            {
                LastCards = CurrentRound.currentCards.ToList();
                if (_rounds.Count < ROUNDS_COUNT) NextRound();
                else
                {
                    Evaluation();
                    return false;
                }
            }

            return true;
        }

        public void NextMatch()
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

            // TODO check if the cards should be shuffled here
        }

        public string FullSummary(int playerIndex)
        {
            _personalPointsSummary ??= PointsSummary(_loser, playerIndex);
            return (_personalPointsSummary.Equals("") ? _generalPointsSummary : _personalPointsSummary) + _matchSummary;
        }

        public bool? HasPlayerWon(int player) => player == -1 ? null : CurrentPlayers[player].TeamIndex != _loser;

        #endregion

        #region Other gamelogic

        internal void SetIsWegGelaufen() => IsWegGelaufen = true;

        private void ChooseGame()
        {
            Mode = MinimumGame;
            if (Mode != SchafkopfMode.Sauspiel) SauspielFarbe = "";
            switch (Mode)
            {
                case SchafkopfMode.Sauspiel:
                    Trumpf = Card.HERZ;
                    SauspielFarbe = Color;
                    break;
                case SchafkopfMode.Solo:
                    Trumpf = Color;
                    break;
                case SchafkopfMode.Wenz:
                    Trumpf = "";
                    break;
                case SchafkopfMode.SoloTout:
                    Trumpf = Color;
                    break;
                case SchafkopfMode.WenzTout:
                    Trumpf = "";
                    break;
                case SchafkopfMode.Weiter:
                    //TODO handle this case, probably shuffle or nextMatch
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            for (int i = 0; i < PLAYER_PER_ROUND; ++i) CurrentPlayers[i].SortCards(this);

            UpdatePlayableCards();
            //TODO: A new round instance should probably be initiated here -> (SchafkopfRound.ResetRound can be removed)
        }

        private void NextRound()
        {
            CalculateStichPoints();
            ++_teamsAnzStiche[CurrentPlayers[CurrentRound.NextStartPlayer].TeamIndex];
            _rounds.Add(new SchafkopfRound(CurrentRound));
            UpdatePlayableCards();
        }

        private void ResetMode()
        {
            Mode = SchafkopfMode.Weiter;
            MinimumGame = SchafkopfMode.Weiter;
            SauspielFarbe = "";
            Trumpf = "";
        }

        private void UpdatePlayableCards()
        {
            PlayableCards = CurrentPlayers.Select(t => t.CheckPlayableCards(this)).ToList();
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

            List<int> team = new List<int>();
            for (int i = 0; i < PLAYER_PER_ROUND; ++i)
            {
                if (CurrentPlayers[i].TeamIndex == teamNumber) team.Add(i);
            }

            List<int> nextCards = new List<int>();
            for (int i = 0; i < team.Count; ++i) nextCards.Add(0);

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
            CalculateStichPoints();
            int[] tmp = new int[Players.Count];
            int score = ScoreOfThisMatch();
            for (int i = 0; i < Players.Count; ++i)
            {
                if (Players[i].Number == -1) tmp[i] = 0;
                else if (Players[i].TeamIndex == _loser) tmp[i] = -score;
                else tmp[i] = score;
                if (Mode != SchafkopfMode.Sauspiel && Players[i].TeamIndex == 0) tmp[i] *= 3;
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
            {
                summary.Append("Du hast ");
            }

            int teamIndex = CurrentPlayers[player].TeamIndex;
            summary.Append(Mode is SchafkopfMode.Wenz or SchafkopfMode.WenzTout ? "den " : "das ").Append(Mode);
            if (Mode == SchafkopfMode.Sauspiel)
            {
                summary.Append(" mit ");
                int w = 0;
                while (CurrentPlayers[w].TeamIndex != teamIndex || w == player)
                {
                    ++w;
                }

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
                        {
                            summary.Append(" und ");
                        }
                    }

                    ++w;
                }
            }

            summary.Append(" mit ").Append(_teams[teamIndex]).Append(" Augen ");
            summary.Append(loser == teamIndex ? "verloren" : "gewonnen").Append("\n\n");
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
                {
                    _loser = 0;
                }

                points *= 2;
                summary.Append("\n*2 tout");
            }
            else
            {
                count = CurrentPlayers.Count(t => t.Kontra);
                if ((count == 1 && _teams[0] < _teams[1]) || _teams[0] <= _teams[1]) _loser = 0;

                //if (loser == 0 && teams[loser] < 31 || loser == 1 && teams[loser] < 30)
                if (_teams[_loser] < 30 || _loser != count % 2 && _teams[_loser] < 31)
                {
                    points += 10;
                    if (_teamsAnzStiche[_loser] == 0)
                    {
                        points += 10;
                        summary.Append("\n+20 Schneiderschwarz");
                    }
                    else
                    {
                        summary.Append("\n+10 Schneider");
                    }
                }
            }

            for (int i = 0; i < PLAYER_PER_ROUND; ++i)
            {
                if (CurrentPlayers[i].Aufgestellt)
                {
                    points *= 2;
                    summary.Append("\n*2 aufgestellt");
                }
            }

            count = 0;
            for (int i = 0; i < PLAYER_PER_ROUND; ++i)
            {
                if (CurrentPlayers[i].Kontra)
                {
                    points *= 2;
                    summary.Append(++count == 1 ? "\n*2 Kontra" : "\n*2 Re");
                }
            }

            summary.Append("\n_________________\n").Append(points);
            _matchSummary = summary.ToString();
            _personalPointsSummary = null;
            _generalPointsSummary = PointsSummary(_loser, -1);
            return points;
        }

        private void CalculateStichPoints()
        {
            SchafkopfRound round = CurrentRound;
            int sum = 0;
            for (int i = 0; i < PLAYER_PER_ROUND; ++i)
            {
                sum += round.currentCards[i].Points;
            }

            _teams[CurrentPlayers[round.NextStartPlayer].TeamIndex] += sum;
        }

        #endregion

        #region Joining

        public string InfoForRejoin(string separator)
        {
            StringBuilder bob = new StringBuilder();
            for (int i = 0; i < PLAYER_PER_ROUND; ++i)
            {
                bob.Append(_currentPlayerIndexes[i]).Append(separator);
            }

            bob.Append(Mode.ToString()).Append(separator);
            bob.Append(Trumpf).Append(separator);
            bob.Append(SauspielFarbe).Append(separator);
            bob.Append(IsWegGelaufen.ToString()).Append(separator);
            bob.Append(MinimumGame.ToString()).Append(separator);
            bob.Append(Color).Append(separator);
            bob.Append(AmountShuffle).Append(separator);
            bob.Append(PlayerIndex).Append(separator);
            bob.Append(_teams[0]).Append(separator);
            bob.Append(_teams[1]).Append(separator);
            bob.Append(_teamsAnzStiche[0]).Append(separator);
            bob.Append(_teamsAnzStiche[1]).Append(separator);
            bob.Append(_laufende).Append(separator);
            bob.Append(_loser).Append(separator);
            if (LastCards == null || LastCards.Count < 1)
            {
                bob.Append("null").Append(separator);
            }
            else
            {
                foreach (var card in LastCards) bob.Append(card.Index).Append(separator);
            }

            bob.Append(_rounds.Count).Append(separator);
            foreach (var round in _rounds)
            {
                bob.Append(round.InfoForRejoin(separator)).Append(separator);
            }

            bob.Append(_matchSummary).Append(separator);
            bob.Append(_generalPointsSummary);
            return bob.ToString();
        }

        public void RestoreFromInfo(string info, char separator)
        {
            SchafkopfPlayer[] newList = new SchafkopfPlayer[PLAYER_PER_ROUND];
            foreach (var player in Players.Where(player => player.Number != -1)) newList[player.Number] = player;
            CurrentPlayers = newList;

            string[] msgParts = info.Split(separator);
            int index = -1;
            for (int i = 0; i < PLAYER_PER_ROUND; ++i)
            {
                _currentPlayerIndexes[i] = int.Parse(msgParts[++index]);
            }

            Mode = StringToSchafkopfMode(msgParts[++index]);
            Trumpf = msgParts[++index];
            SauspielFarbe = msgParts[++index];
            IsWegGelaufen = bool.Parse(msgParts[++index]);
            MinimumGame = StringToSchafkopfMode(msgParts[++index]);
            Color = msgParts[++index];
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
            _rounds = new List<SchafkopfRound>();
            for (int i = 0; i < count; ++i)
            {
                _rounds.Add(new SchafkopfRound(msgParts, ref index));
            }

            _matchSummary = msgParts[++index];
            _generalPointsSummary = msgParts[index + 1];
            if (Mode != SchafkopfMode.Weiter)
            {
                UpdatePlayableCards();
            }
        }

        #endregion

        public StringBuilder WriteCurrentIndices()
        {
            StringBuilder bob = new StringBuilder(20);
            bob.Append("[");
            bob.Append(string.Join(", ", _currentPlayerIndexes));
            bob.Append("] -> [");
            bob.Append(ArrayPrinter.ArrayString(i => CurrentPlayers[i].Name, CurrentPlayers.Count));
            bob.Append("]");
            return bob;
        }
    }
}