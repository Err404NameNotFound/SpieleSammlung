using SpieleSammlung.Model.Multiplayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpieleSammlung.Model.Schafkopf
{
    public class SchafkopfPlayer : MultiplayerPlayer
    {
        private readonly List<Card> _playedCards;
        public readonly List<Card> playableCards;
        public List<SchafkopfMatchPossibility> possibilities;
        public int points;
        public bool aufgestellt;
        public bool kontra;
        public int number;
        public int teamIndex;
        public bool? continueMatch;
        public MultiplayerPlayerState state;

        public PointsStorage PlayerPoints => new(name, points);

        public SchafkopfPlayer(string id, string name) : base(id, name)
        {
            playableCards = new List<Card>();
            _playedCards = new List<Card>();
            points = 0;
            NewMatch(-1, false);
            state = MultiplayerPlayerState.Active;
            possibilities = new List<SchafkopfMatchPossibility>();
        }

        public void NewMatch(int n, bool sameRound)
        {
            aufgestellt = aufgestellt && sameRound;
            kontra = false;
            number = n;
            playableCards.Clear();
            _playedCards.Clear();
            teamIndex = -1;
        }

        public void UpdatePossibilities(SchafkopfMatch match)
        {
            possibilities = new List<SchafkopfMatchPossibility> { new(SchafkopfMode.Weiter) };
            if (match.MinimumGame != SchafkopfMode.SoloTout)
            {
                List<string> solo = SoloPossibilities();
                if (match.MinimumGame != SchafkopfMode.WenzTout)
                {
                    if (match.MinimumGame != SchafkopfMode.Solo)
                    {
                        List<string> temp;
                        if (match.MinimumGame != SchafkopfMode.Wenz)
                        {
                            if (match.MinimumGame != SchafkopfMode.Sauspiel)
                            {
                                temp = new List<string>();
                                for (int i = 0; i < 4; ++i)
                                {
                                    if (CanPlaySauspielWithColor(Card.ColorNames[i]))
                                    {
                                        temp.Add(Card.ColorNames[i]);
                                    }

                                    if (i == 1) i = 2;
                                }

                                if (temp.Count > 0)
                                {
                                    possibilities.Add(new SchafkopfMatchPossibility(SchafkopfMode.Sauspiel, temp));
                                }
                            }

                            possibilities.Add(new SchafkopfMatchPossibility(SchafkopfMode.Wenz));
                        }

                        temp = solo.ToList();

                        possibilities.Add(new SchafkopfMatchPossibility(SchafkopfMode.Solo, temp));
                    }

                    possibilities.Add(new SchafkopfMatchPossibility(SchafkopfMode.WenzTout));
                }

                possibilities.Add(new SchafkopfMatchPossibility(SchafkopfMode.SoloTout, solo));
            }
        }

        private List<string> SoloPossibilities()
        {
            List<string> temp = new List<string>();
            for (int i = 0; i < 4; ++i)
            {
                if (CanPlaySoloWithColor(Card.ColorNames[i]))
                {
                    temp.Add(Card.ColorNames[i]);
                }
            }

            if (temp.Count == 0)
            {
                temp = new List<string> { "Eichel", "Gras", "Herz", "Schelle" };
            }

            return temp;
        }

        public int PossibilityIndexOf(SchafkopfMode mode)
        {
            int w = 0;
            while (w < possibilities.Count)
            {
                if (possibilities[w].mode == mode) return w;
                ++w;
            }

            return -1;
        }

        public int PossibilityIndexOf(int index, string color)
        {
            int w = 0;
            while (w < possibilities[index].colors.Count)
            {
                if (possibilities[index].colors[w].Equals(color)) return w;
                ++w;
            }

            return -1;
        }

        public void RemovePossibility(SchafkopfMode mode)
        {
            int tmp = PossibilityIndexOf(mode);
            if (tmp != -1)
            {
                possibilities.RemoveAt(tmp);
            }
        }

        public bool CanPlaySoloWithColor(string color)
        {
            int w = 0;
            while (w < 8)
            {
                if (playableCards[w].color.Equals(color) &&
                    !(playableCards[w].number.Equals("Ober") || playableCards[w].number.Equals("Unter")))
                {
                    return true;
                }

                ++w;
            }

            return false;
        }

        public bool CanPlaySauspielWithColor(string color)
        {
            int w = 0;
            bool hasColor = false;
            while (w < 8)
            {
                if (playableCards[w].color.Equals(color))
                {
                    if (playableCards[w].number.Equals("Sau")) return false;
                    if (!(playableCards[w].number.Equals("Ober") || playableCards[w].number.Equals("Unter")))
                    {
                        hasColor = true;
                    }
                }

                ++w;
            }

            return hasColor;
        }

        public List<Card> GetPlayableCards() => playableCards.ToList();

        public int[] SortCards(SchafkopfMatch match)
        {
            int[] values = new int[8];
            for (int i = 0; i < 8; ++i)
            {
                values[i] = playableCards[i].GetSortValueOfThisCard(match);
            }

            for (int i = 0; i < 7; ++i)
            {
                for (int j = i + 1; j < 8; ++j)
                {
                    if (values[j] > values[i])
                    {
                        SwitchVariables(ref values[j], ref values[i]);
                        SwitchCardIndex(j, i);
                    }
                }
            }

            return values;
        }

        public void SortCards(int m, int c)
        {
            SortCards(new SchafkopfMatch(possibilities[m].mode, possibilities[m].colors[c]));
        }

        private static void SwitchVariables(ref int i, ref int j)
        {
            (i, j) = (j, i);
        }

        private void SwitchCardIndex(int i, int j)
        {
            (playableCards[i], playableCards[j]) = (playableCards[j], playableCards[i]);
        }

        public bool PlayCard(int index, SchafkopfMatch match)
        {
            if (number == match.CurrentRound.CurrentPlayer)
            {
                Card card = playableCards[index];
                match.CurrentRound.currentCards.Add(card);
                int value = card.GetValueOfThisCard(match);
                if (match.CurrentRound.currentCards.Count == 1)
                {
                    match.CurrentRound.semiTrumpf = card.color;
                    match.CurrentRound.NewHighestCard(number, card.GetValueOfThisCard(match));
                    if (card.color.Equals(match.SauspielFarbe) && !card.number.Equals("Sau") && HasGesuchte(match) &&
                        KannWeglaufen(match))
                    {
                        match.IsWegGelaufen = true;
                    }
                }
                else if (value > match.CurrentRound.highestValue)
                {
                    match.CurrentRound.NewHighestCard(number, value);
                }

                RemovePlayableCard(index);
                return true;
            }

            return false;
        }

        public int FirstIndexOfColor(string color, SchafkopfMatch match)
        {
            int w = 0;
            while (w < playableCards.Count)
            {
                if (playableCards[w].color.Equals(color) && !playableCards[w].IsTrumpf(match))
                {
                    return w;
                }

                ++w;
            }

            return -1;
        }

        public bool HasColor(string color, SchafkopfMatch match)
        {
            return FirstIndexOfColor(color, match) != -1;
        }

        public bool HasTrumpf(SchafkopfMatch match)
        {
            int w = 0;
            while (w < playableCards.Count)
            {
                if (playableCards[w].IsTrumpf(match))
                {
                    return true;
                }

                ++w;
            }

            return false;
        }

        public bool HasGesuchte(SchafkopfMatch match)
        {
            if (match.Mode == SchafkopfMode.Sauspiel)
            {
                int index = FirstIndexOfColor(match.SauspielFarbe, match);
                if (index == -1)
                {
                }
                else if (playableCards[index].number.Equals("Sau"))
                {
                    return true;
                }
            }

            return false;
        }

        public bool KannWeglaufen(SchafkopfMatch match)
        {
            int index = FirstIndexOfColor(match.SauspielFarbe, match);
            int end = index + 4;
            if (index != -1 && playableCards.Count >= end)
            {
                bool has4 = true;
                int w = index + 1;
                while (w < end && has4)
                {
                    has4 = playableCards[w].color.Equals(match.SauspielFarbe);
                    ++w;
                }

                return has4;
            }

            return false;
        }

        public List<bool> CheckPlayableCards(SchafkopfMatch match)
        {
            if (playableCards.Count == 1) return new List<bool> { true };

            bool hasGesuchte = HasGesuchte(match);
            bool hasTrumpf = HasTrumpf(match);
            bool kannWeglaufen = hasGesuchte && KannWeglaufen(match);
            List<bool> playable = new List<bool>();
            if (match.CurrentRound.currentCards.Count == 0)
            {
                foreach (var card in playableCards)
                {
                    if (card.IsTrumpf(match) || !card.color.Equals(match.SauspielFarbe) ||
                        match.Mode != SchafkopfMode.Sauspiel)
                    {
                        playable.Add(true);
                    }
                    else if (!hasGesuchte)
                    {
                        playable.Add(true);
                    }
                    else if (card.number.Equals("Sau") || kannWeglaufen)
                    {
                        playable.Add(true);
                    }
                    else
                    {
                        playable.Add(false);
                    }
                }
            }
            else
            {
                bool firstCardTrumpf = Card.IsTrumpf(match.CurrentRound.currentCards[0], match);
                bool hasFirstCardColor = HasColor(match.CurrentRound.semiTrumpf, match);
                foreach (var card in playableCards)
                {
                    if (firstCardTrumpf)
                    {
                        if (card.IsTrumpf(match))
                        {
                            playable.Add(true);
                        }
                        else if (hasTrumpf)
                        {
                            playable.Add(false);
                        }
                        else if (match.Mode != SchafkopfMode.Sauspiel)
                        {
                            playable.Add(true);
                        }
                        else if (!card.ToString().Equals(match.SauspielFarbe + " Sau") || match.IsWegGelaufen)
                        {
                            playable.Add(true);
                        }
                        else
                        {
                            playable.Add(false);
                        }
                    }
                    else if (card.color.Equals(match.CurrentRound.semiTrumpf) && !card.IsTrumpf(match))
                    {
                        if (match.Mode != SchafkopfMode.Sauspiel || !card.color.Equals(match.SauspielFarbe))
                        {
                            playable.Add(true);
                        }
                        else if (!hasGesuchte || card.number.Equals("Sau") || match.IsWegGelaufen)
                        {
                            playable.Add(true);
                        }
                        else
                        {
                            playable.Add(false);
                        }
                    }
                    else
                    {
                        if (hasFirstCardColor)
                        {
                            playable.Add(false);
                        }
                        else if (match.Mode != SchafkopfMode.Sauspiel)
                        {
                            playable.Add(true);
                        }
                        else if (!card.ToString().Equals(match.SauspielFarbe + " Sau") || match.IsWegGelaufen)
                        {
                            playable.Add(true);
                        }
                        else
                        {
                            playable.Add(false);
                        }
                    }
                }
            }

            return playable;
        }

        private void RemovePlayableCard(int index)
        {
            _playedCards.Add(playableCards[index]);
            playableCards.RemoveAt(index);
        }

        public string InfoForRejoin(string separator)
        {
            StringBuilder bob = new StringBuilder();
            bob.Append(playableCards.Count).Append(separator);
            foreach (var card in playableCards)
            {
                bob.Append(card.index).Append(separator);
            }

            bob.Append(_playedCards.Count).Append(separator);
            foreach (var card in _playedCards)
            {
                bob.Append(card.index).Append(separator);
            }

            bob.Append(possibilities.Count).Append(separator);
            foreach (var possibility in possibilities)
            {
                bob.Append(possibility.mode).Append(separator).Append(possibility.colors.Count)
                    .Append(separator);
                foreach (var color in possibility.colors)
                {
                    bob.Append(color).Append(separator);
                }
            }

            bob.Append(points).Append(separator);
            bob.Append(aufgestellt).Append(separator);
            bob.Append(kontra).Append(separator);
            bob.Append(number).Append(separator);
            bob.Append(teamIndex).Append(separator);
            bob.Append(continueMatch.HasValue ? continueMatch.Value.ToString() : "null").Append(separator);
            bob.Append(state);
            return bob.ToString();
        }

        public void RestoreFromInfo(string info, char separator)
        {
            string[] msgParts = info.Split(separator);
            int index;
            int length = int.Parse(msgParts[0]) + 1;
            for (index = 1; index < length; ++index)
            {
                playableCards.Add(Card.allCards[int.Parse(msgParts[index])]);
            }

            length = int.Parse(msgParts[index]) + index + 1;
            for (++index; index < length; ++index)
            {
                _playedCards.Add(Card.allCards[int.Parse(msgParts[index])]);
            }

            possibilities = new List<SchafkopfMatchPossibility>();
            length = int.Parse(msgParts[index]);
            for (int i = 0; i < length; ++i)
            {
                SchafkopfMatchPossibility possibility =
                    new SchafkopfMatchPossibility(SchafkopfMatch.StringToSchafkopfMode(msgParts[++index]));
                possibility.colors.Clear();
                int colorCount = int.Parse(msgParts[++index]);
                for (int color = 0; color < colorCount; ++color)
                {
                    possibility.colors.Add(msgParts[++index]);
                }

                possibilities.Add(possibility);
            }

            points = int.Parse(msgParts[++index]);
            aufgestellt = bool.Parse(msgParts[++index]);
            kontra = bool.Parse(msgParts[++index]);
            number = int.Parse(msgParts[++index]);
            teamIndex = int.Parse(msgParts[++index]);
            ++index;
            if (msgParts[index].Equals("null"))
            {
                continueMatch = null;
            }
            else
            {
                continueMatch = bool.Parse(msgParts[index]);
            }

            state = Convert(msgParts[++index]);
        }

        public static MultiplayerPlayerState Convert(string state)
        {
            if (state.Equals(MultiplayerPlayerState.Active.ToString())) return MultiplayerPlayerState.Active;
            if (state.Equals(MultiplayerPlayerState.LeftMatch.ToString())) return MultiplayerPlayerState.LeftMatch;
            if (state.Equals(MultiplayerPlayerState.Inactive.ToString())) return MultiplayerPlayerState.Inactive;
            throw new ArgumentException("Der String konnte nicht umgewandelt werden");
        }
    }
}