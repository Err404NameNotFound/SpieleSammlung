using System.Collections.Generic;
using System.Text;

namespace SpieleSammlung.Model.Schafkopf
{
    public class SchafkopfRound
    {
        public List<Card> currentCards;
        public string semiTrumpf;
        public int nextStartPlayer;
        public int highestValue;
        private int _currentPlayer;
        public int StartPlayer { get; }

        public int CurrentPlayer => _currentPlayer % 4;

        public SchafkopfRound()
        {
            StartPlayer = nextStartPlayer = 0;
            Initialize();
        }

        public SchafkopfRound(SchafkopfRound previousRound)
        {
            StartPlayer = nextStartPlayer = previousRound.nextStartPlayer;
            Initialize();
        }

        public SchafkopfRound(IReadOnlyList<string> msgParts, ref int index)
        {
            int count = int.Parse(msgParts[++index]);
            currentCards = new List<Card>(count);
            for (int i = 0; i < count; ++i)
            {
                currentCards.Add(Card.GetCard(int.Parse(msgParts[++index])));
            }

            semiTrumpf = msgParts[++index];
            nextStartPlayer = int.Parse(msgParts[++index]);
            highestValue = int.Parse(msgParts[++index]);
            _currentPlayer = int.Parse(msgParts[++index]);
            StartPlayer = int.Parse(msgParts[++index]);
        }

        private void Initialize()
        {
            currentCards = new List<Card>();
            semiTrumpf = "";
            _currentPlayer = nextStartPlayer;
            highestValue = 0;
        }

        public void NewHighestCard(int index, int value)
        {
            nextStartPlayer = index;
            highestValue = value;
        }

        public void SetNextPlayer()
        {
            ++_currentPlayer;
        }

        public void ResetNextPlayer()
        {
            _currentPlayer = StartPlayer;
        }

        public string InfoForRejoin(string separator)
        {
            StringBuilder bob = new StringBuilder();
            bob.Append(currentCards.Count).Append(separator);
            foreach (var card in currentCards)
            {
                bob.Append(card.Index).Append(separator);
            }

            bob.Append(semiTrumpf).Append(separator);
            bob.Append(nextStartPlayer).Append(separator);
            bob.Append(highestValue).Append(separator);
            bob.Append(_currentPlayer).Append(separator);
            bob.Append(StartPlayer);
            return bob.ToString();
        }
    }
}