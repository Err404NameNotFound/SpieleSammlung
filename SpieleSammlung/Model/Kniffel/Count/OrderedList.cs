namespace SpieleSammlung.Model.Kniffel.Count;

public abstract class OrderedList
{
    protected DiceCounter[] Counters;

    public int Count { get; protected set; }

    protected OrderedList()
    {
        Count = 0;
        Counters = new DiceCounter[Dice.DICE_COUNT];
    }

    protected OrderedList(OrderedList other)
    {
        Count = other.Count;
        Counters = new DiceCounter[other.Counters.Length];
        for (int i = 0; i < Counters.Length; ++i)
        {
            if (other.Counters[i] != null)
                Counters[i] = new DiceCounter(other.Counters[i]);
        }
    }

    /// <summary>String representation in form { Counter 1, Counter 2, ... }.</summary>
    public abstract override string ToString();
}