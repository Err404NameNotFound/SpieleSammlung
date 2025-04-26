using System;

namespace SpieleSammlung.Model.Kniffel.Count;

public class CountOrderedList : OrderedList
{
    public DiceCounter this[int index]
    {
        get
        {
            if (0 > index || index >= Count)
                throw new IndexOutOfRangeException();

            return Counters[index];
        }
    }

    public CountOrderedList()
    {
    }

    public CountOrderedList(OrderedList other) : base(other)
    {
    }

    public void IncCount(int value)
    {
        if (Count == 0)
            Add(value);
        else
        {
            int i = 0;
            while (i < Count && Counters[i].Value != value)
            {
                ++i;
            }

            if (i == Count)
                Add(value);
            else
            {
                Counters[i].IncCount();
                while (i > 0 && Counters[i].Count > Counters[i - 1].Count)
                {
                    Switch(i, i - 1);
                    --i;
                }
            }
        }
    }

    private void Switch(int first, int second)
    {
        (Counters[first], Counters[second]) = (Counters[second], Counters[first]);
    }

    public void DecCount(int value)
    {
        int i = 0;
        while (Counters[i].Value != value)
        {
            ++i;
        }

        Counters[i].DecCount();
        if (Counters[i].Count == 0)
        {
            Counters[i] = null;
            --Count;
            while (i < Count)
            {
                Counters[i] = Counters[i + 1];
                ++i;
            }
        }
        else
        {
            while (i + 1 < Count && Counters[i].Count < Counters[i + 1].Count)
            {
                Switch(i, i + 1);
                ++i;
            }
        }
    }

    private void Add(int value)
    {
        Counters[Count] = new DiceCounter(value, 1);
        ++Count;
    }

    public override string ToString() => "{ " + string.Join(", ", Counters as object[]) + " }";
}