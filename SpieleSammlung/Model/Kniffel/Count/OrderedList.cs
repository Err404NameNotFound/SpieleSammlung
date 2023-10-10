namespace SpieleSammlung.Model.Kniffel.Count
{
    public abstract class OrderedList
    {
        protected DiceCounter[] counters;

        public int Count { get; protected set; }

        protected OrderedList()
        {
            Count = 0;
            counters = new DiceCounter[Dice.DICE_COUNT];
        }

        protected OrderedList(OrderedList other)
        {
            Count = other.Count;
            counters = new DiceCounter[other.counters.Length];
            for (int i = 0; i < counters.Length; ++i)
            {
                if (other.counters[i] != null)
                {
                    counters[i] = new DiceCounter(other.counters[i]);
                }
            }
        }

        /// <summary>String representation in form { Counter 1, Counter 2, ... }.</summary>
        public abstract override string ToString();
    }
}