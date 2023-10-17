using SpieleSammlung.Model.Multiplayer;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace SpieleSammlung.Model.Schafkopf
{
    public class SchafkopfMatch : SchafkopfMatchConfig
    {
        public List<SchafkopfPlayer> Players { get; }
        public SchafkopfPlayer[] CurrentPlayers { get; }
        private readonly int[] _currentPlayerIndexes;

        public List<PointsStorage> PlayerPoints => Players.Select(player => player.PlayerPoints).ToList();

        public DataTable PointsSingle { get; }
        public DataTable PointsCumulated { get; }

        public bool IsWegGelaufen { get; set; }

        public SchafkopfMode MinimumGame { get; set; }
        public string Color { get; set; }
        public int AmountShuffle { get; set; }
        public int PlayerIndex { get; set; }

        public List<SchafkopfRound> Rounds { get; private set; }
        public Card[] LastCards { get; }
        public IReadOnlyList<bool>[] PlayableCards { get; }

        private readonly int[] _teams;
        private readonly int[] _teamsAnzStiche;
        private int _laufende;
        private int _loser;
        private string _matchSummary;
        private string _personalPointsSummary;
        private string _generalPointsSummary;
        private readonly Random _random;

        public SchafkopfMatch(List<MultiplayerPlayer> playerNames)
        {
            Players = new List<SchafkopfPlayer>();
            foreach (MultiplayerPlayer t in playerNames)
            {
                Players.Add(new SchafkopfPlayer(t.Id, t.Name));
            }

            _random = new Random();
            ResetMode();
            _currentPlayerIndexes = Players.Count switch
            {
                < 6 => new[] { 3, 0, 1, 2 }, //next match gleich zu begin ->  0, 1, 2, 3
                6 => new[] { 5, 1, 2, 4 }, //next match gleich zu begin ->  0, 2, 3, 5
                _ => new[] { 6, 1, 3, 5 } //next match gleich zu begin ->  0, 2, 4, 6
            };

            CurrentPlayers = new SchafkopfPlayer[4];
            for (int i = 0; i < 4; ++i)
            {
                CurrentPlayers[i] = Players[_currentPlayerIndexes[i]];
            }

            LastCards = new Card[4];
            PlayableCards = new List<bool>[4];
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

        public SchafkopfMatch(SchafkopfMode mode, string color) => SetMode(mode, color);

        private void SetMode(SchafkopfMode mode, string color)
        {
            Mode = mode;
            if (mode == SchafkopfMode.Sauspiel)
            {
                Trumpf = Card.HERZ;
                SauspielFarbe = color;
            }
            else
            {
                Trumpf = color;
                SauspielFarbe = "";
            }
        }

        public SchafkopfRound CurrentRound => Rounds[Rounds.Count - 1];

        public static SchafkopfMode StringToSchafkopfMode(string game)
        {
            return game switch
            {
                "Sauspiel" => SchafkopfMode.Sauspiel,
                "Solo" => SchafkopfMode.Solo,
                "Wenz" => SchafkopfMode.Wenz,
                "SoloTout" => SchafkopfMode.SoloTout,
                "WenzTout" => SchafkopfMode.WenzTout,
                _ => SchafkopfMode.Weiter
            };
        }

        public void PrepareShuffleCards(bool sameRound)
        {
            AmountShuffle = 0;
            Rounds = new List<SchafkopfRound> { new() };
            ResetMode();
            for (int i = 0; i < 4; ++i) CurrentPlayers[i].NewMatch(i, sameRound);
        }

        public List<string> ShuffleCards(bool sameRound, bool isHost, string codeClientShuffledCards)
        {
            if (!isHost) return null;
            List<int> cards = new List<int>();
            for (int i = 0; i < 32; ++i) cards.Add(i);

            List<string> msg = new List<string> { codeClientShuffledCards };
            for (int i = 0; i < 32; ++i)
            {
                var temp = _random.Next(0, cards.Count);
                CurrentPlayers[i % 4].PlayableCards.Add(Card.GetCard(cards[temp]));
                msg.Add(cards[temp].ToString());
                cards.RemoveAt(temp);
            }

            msg.Add(sameRound.ToString());
            return msg;
        }

        public void ChooseGame()
        {
            Mode = MinimumGame;
            if (Mode != SchafkopfMode.Sauspiel)
            {
                SauspielFarbe = "";
            }

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
            }

            for (int i = 0; i < 4; ++i) CurrentPlayers[i].SortCards(this);

            UpdatePlayableCards();
            //TODO: A new round instance should probably be initiated here -> (SchafkopfRound.ResetRound can be removed)
        }

        public void CalculateLaufende()
        {
            int[,] values = new int[4, 8];
            for (int i = 0; i < 4; ++i)
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
            for (int i = 0; i < 4; ++i)
            {
                if (CurrentPlayers[i].TeamIndex == teamNumber) team.Add(i);
            }

            List<int> nextCards = new List<int>();
            for (int i = 0; i < team.Count; ++i)
            {
                nextCards.Add(0);
            }

            _laufende = 0;
            w = 0;
            while (w < 8 && _laufende < 8)
            {
                var e = 0;
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

        public void NextRound()
        {
            CalculateStichPoints();
            ++_teamsAnzStiche[CurrentPlayers[CurrentRound.NextStartPlayer].TeamIndex];
            Rounds.Add(new SchafkopfRound(CurrentRound));
            UpdatePlayableCards();
        }

        public void NextMatch()
        {
            ResetMode();
            _teams[0] = _teams[1] = _teamsAnzStiche[0] = _teamsAnzStiche[1] = 0;
            for (int i = 0; i < 4; ++i)
            {
                CurrentPlayers[i].Number = -1;
                CurrentPlayers[i] = Players[_currentPlayerIndexes[i] = (_currentPlayerIndexes[i] + 1) % Players.Count];
            }
        }

        public bool? Evalution(int player)
        {
            CalculateStichPoints();
            int[] tmp = new int[Players.Count];
            int score = ScoreOfThisMatch(player);
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

            return player == -1 ? null : CurrentPlayers[player].TeamIndex != _loser;
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

        private int ScoreOfThisMatch(int player)
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

            for (int i = 0; i < 4; ++i)
            {
                if (CurrentPlayers[i].Aufgestellt)
                {
                    points *= 2;
                    summary.Append("\n*2 aufgestellt");
                }
            }

            count = 0;
            for (int i = 0; i < 4; ++i)
            {
                if (CurrentPlayers[i].Kontra)
                {
                    points *= 2;
                    summary.Append(++count == 1 ? "\n*2 Kontra" : "\n*2 Re");
                }
            }

            summary.Append("\n_________________\n").Append(points);
            _matchSummary = summary.ToString();
            _personalPointsSummary = PointsSummary(_loser, player);
            _generalPointsSummary = PointsSummary(_loser, -1);
            return points;
        }

        public string FullSummary()
        {
            return (_personalPointsSummary.Equals("") ? _generalPointsSummary : _personalPointsSummary) + _matchSummary;
        }

        private void ResetMode()
        {
            Mode = SchafkopfMode.Weiter;
            MinimumGame = SchafkopfMode.Weiter;
            SauspielFarbe = "";
            Trumpf = "";
        }

        public void UpdatePlayableCards()
        {
            for (int i = 0; i < 4; ++i)
            {
                PlayableCards[i] = CurrentPlayers[i].CheckPlayableCards(this);
            }
        }

        private void CalculateStichPoints()
        {
            SchafkopfRound round = CurrentRound;
            int sum = 0;
            for (int i = 0; i < 4; ++i)
            {
                sum += round.currentCards[i].Points;
            }

            _teams[CurrentPlayers[round.NextStartPlayer].TeamIndex] += sum;
        }

        // public override string ToString()
        // {
        //     return Mode switch
        //     {
        //         SchafkopfMode.Sauspiel => "auf " + SauspielFarbe,
        //         SchafkopfMode.Solo => Trumpf + " Solo",
        //         SchafkopfMode.Wenz => "Wenz",
        //         SchafkopfMode.SoloTout => Trumpf + " SoloTout",
        //         SchafkopfMode.WenzTout => "WenzTout",
        //         _ => throw new ArgumentOutOfRangeException()
        //     };
        // }

        public string InfoForRejoin(string separator)
        {
            StringBuilder bob = new StringBuilder();
            for (int i = 0; i < 4; ++i)
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
            if (LastCards[0] == null)
            {
                bob.Append("null").Append(separator);
            }
            else
            {
                for (int i = 0; i < 4; ++i)
                {
                    bob.Append(LastCards[i].Index).Append(separator);
                }
            }

            bob.Append(Rounds.Count).Append(separator);
            foreach (var round in Rounds)
            {
                bob.Append(round.InfoForRejoin(separator)).Append(separator);
            }

            bob.Append(_matchSummary).Append(separator);
            bob.Append(_generalPointsSummary);
            return bob.ToString();
        }

        public void RestoreFromInfo(string info, char separator)
        {
            foreach (var player in Players.Where(player => player.Number != -1))
            {
                CurrentPlayers[player.Number] = player;
            }

            string[] msgParts = info.Split(separator);
            int index = -1;
            for (int i = 0; i < 4; ++i)
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
                for (int i = 0; i < 4; ++i)
                {
                    LastCards[i] = Card.GetCard(int.Parse(msgParts[index++]));
                }
            }

            int count = int.Parse(msgParts[index]);
            Rounds = new List<SchafkopfRound>();
            for (int i = 0; i < count; ++i)
            {
                Rounds.Add(new SchafkopfRound(msgParts, ref index));
            }

            _matchSummary = msgParts[++index];
            _generalPointsSummary = msgParts[index + 1];
            if (Mode != SchafkopfMode.Weiter)
            {
                UpdatePlayableCards();
            }
        }

        public StringBuilder WriteCurrentIndize()
        {
            StringBuilder bob = new StringBuilder(10);
            bob.Append("[");
            for (int i = 0; i < 4; ++i)
            {
                bob.Append(_currentPlayerIndexes[i]);
                if (i != 3)
                {
                    bob.Append(", ");
                }
            }

            bob.Append("] -> [");
            for (int i = 0; i < 4; ++i)
            {
                bob.Append(CurrentPlayers[i].Name);
                if (i != 3)
                {
                    bob.Append(", ");
                }
            }

            bob.Append("]");
            return bob;
        }
    }
}