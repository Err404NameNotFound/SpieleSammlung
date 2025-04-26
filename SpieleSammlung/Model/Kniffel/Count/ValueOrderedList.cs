using System;
using System.Text;

namespace SpieleSammlung.Model.Kniffel.Count;

public class ValueOrderedList : OrderedList
{
    public DiceCounter this[int index]
    {
        get
        {
            if (0 > index || index >= Count)
                throw new IndexOutOfRangeException();

            int ret = GetNextNonEmpty(0);
            for (int i = 0; i < index; ++i)
            {
                ret = GetNextNonEmpty(ret + 1);
            }

            return Counters[ret];
        }
    }

    private int GetNextNonEmpty(int i)
    {
        while (Counters[i].Count == 0)
        {
            ++i;
        }

        return i;
    }

    public ValueOrderedList()
    {
        Count = 0;
        Counters = new DiceCounter[Dice.HIGHEST_VALUE - Dice.LOWEST_VALUE];
        for (int i = 0; i < Counters.Length; ++i)
        {
            Counters[i] = new DiceCounter(i + 1, 0);
        }
    }

    public ValueOrderedList(OrderedList other) : base(other)
    {
    }

    public void IncCount(int value)
    {
        if (Counters[value - 1].Count == 0) 
            ++Count;

        Counters[value - 1].IncCount();
    }

    public void DecCount(int value)
    {
        Counters[value - 1].DecCount();
        if (Counters[value - 1].Count == 0) 
            --Count;
    }

    public override string ToString()
    {
        StringBuilder builder = new StringBuilder().Append("{ ");
        if (Count != 0)
        {
            int start = GetNextNonEmpty(0);
            builder.Append(Counters[start]);
            for (int i = 1; i < Count; ++i)
            {
                start = GetNextNonEmpty(start + 1);
                builder.Append(", ").Append(Counters[start]);
            }
        }

        return builder.Append(" }").ToString();
    }
}