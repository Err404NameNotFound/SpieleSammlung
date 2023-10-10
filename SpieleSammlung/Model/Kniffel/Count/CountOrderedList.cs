using System;

namespace SpieleSammlung.Model.Kniffel.Count
{
    public class CountOrderedList : OrderedList
    {
        public DiceCounter this[int index]
        {
            get
            {
                if (0 > index || index >= Count)
                {
                    throw new IndexOutOfRangeException();
                }

                return counters[index];
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
            {
                Add(value);
            }
            else
            {
                int i = 0;
                while (i < Count && counters[i].Value != value)
                {
                    ++i;
                }

                if (i == Count)
                {
                    Add(value);
                }
                else
                {
                    counters[i].IncCount();
                    while (i > 0 && counters[i].Count > counters[i - 1].Count)
                    {
                        Switch(i, i - 1);
                        --i;
                    }
                }
            }
        }

        private void Switch(int first, int second)
        {
            (counters[first], counters[second]) = (counters[second], counters[first]);
        }

        public void DecCount(int value)
        {
            int i = 0;
            while (counters[i].Value != value)
            {
                ++i;
            }

            counters[i].DecCount();
            if (counters[i].Count == 0)
            {
                counters[i] = null;
                --Count;
                while (i < Count)
                {
                    counters[i] = counters[i + 1];
                    ++i;
                }
            }
            else
            {
                while (i + 1 < Count && counters[i].Count < counters[i + 1].Count)
                {
                    Switch(i, i + 1);
                    ++i;
                }
            }
        }

        private void Add(int value)
        {
            counters[Count] = new DiceCounter(value, 1);
            ++Count;
        }

        public override string ToString() => "{ " + string.Join(", ", counters as object[]) + " }";
    }
}