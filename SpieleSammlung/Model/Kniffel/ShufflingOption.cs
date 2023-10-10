using System.Collections.Generic;
using System.Text;
using SpieleSammlung.Model.Kniffel.Fields;

namespace SpieleSammlung.Model.Kniffel
{
    public class ShufflingOption
    {
        #region private member

        private Dice _dice;
        private List<WriteOption> _optionsWrite;
        private List<WriteOption> _optionsKill;
        private List<WriteOption> _optionsWriteOrdered;

        #endregion

        #region properties

        public int Count { private set; get; }
        public int WriteCount => _optionsWrite.Count;
        public double Sum { private set; get; }
        public double Average { private set; get; }
        public double AverageWrite { private set; get; }
        public WriteOption Max { private set; get; }
        public WriteOption MaxWithoutChance { private set; get; }
        public int[] ChosenIndexes => _dice.GetUnsetDiceIndex();
        public WriteOption this[int index] => _optionsWrite[index];

        #endregion

        #region contructor

        private void Initialize(Dice dice, List<WriteOption> options)
        {
            _dice = dice;
            _optionsWrite = new List<WriteOption>();
            _optionsKill = new List<WriteOption>();
            _optionsWriteOrdered = new List<WriteOption>();
            Sum = 0;
            Average = 0;
            AverageWrite = 0;
            Count = 0;
            foreach (WriteOption t in options)
            {
                AddOption(t);
            }
        }

        public ShufflingOption(Dice dice, List<WriteOption> options) => Initialize(dice, options);

        public ShufflingOption(Dice dice, List<WriteOption> options, List<int> killOptions)
        {
            Initialize(dice, options);
            foreach (var t in killOptions)
            {
                AddOption(new WriteOption(t, 0));
            }
        }

        private void AddOption(WriteOption option)
        {
            ++Count;
            if (option.ValueD > 0)
            {
                Sum += option.ValueD;
                _optionsWrite.Add(option);
                AverageWrite = Sum / _optionsWrite.Count;
                _optionsWriteOrdered.Add(option);
                int i = _optionsWriteOrdered.Count - 2;
                while (i >= 0 && option.ValueD > _optionsWriteOrdered[i].ValueD)
                {
                    _optionsWriteOrdered[i + 1] = _optionsWriteOrdered[i];
                    _optionsWriteOrdered[i] = option;
                    --i;
                }

                Max = _optionsWriteOrdered[0];
                MaxWithoutChance = _optionsWriteOrdered[0].Index == KniffelPointsTable.INDEX_CHANCE
                    ? _optionsWriteOrdered.Count > 1 ? _optionsWriteOrdered[1] : null
                    : _optionsWriteOrdered[0];
            }
            else
            {
                _optionsKill.Add(option);
            }

            Average = Sum / Count;
        }

        #endregion

        #region toString

        public override string ToString() => ToString(false);

        public string ToString(bool longForm, string separatorForList = ", ")
        {
            if (!longForm)
            {
                return
                    $"{{ {_dice}: M={{{Max}}}, MWOC={{{MaxWithoutChance}}}, A={Number(Average)}, AW={Number(AverageWrite)}, " +
                    $"S={Number(Sum)}, CountWrite={_optionsWrite.Count} }}";
            }

            StringBuilder bob = new StringBuilder();
            bob.Append(ToString());
            bob.Append("\nwritable fields: {\n").Append(string.Join(separatorForList, _optionsWriteOrdered));
            bob.Append("\n}\nkillable Fields: {\n").Append(string.Join(separatorForList, _optionsKill))
                .Append("\n}");
            return bob.ToString();
        }

        private static string Number(double number)
        {
            return number.ToString("N2");
        }

        #endregion

        #region operator

        public ShufflingOption MaxOptionWithoutChance(ShufflingOption other)
        {
            if (other == null) return this;
            if (MaxWithoutChance == null) return other;
            if (other.MaxWithoutChance == null) return this;
            int indexThis = 0;
            int indexOther = 0;
            while (indexThis < _optionsWriteOrdered.Count && indexOther < other._optionsWriteOrdered.Count)
            {
                if (other._optionsWriteOrdered[indexOther].Index == KniffelPointsTable.INDEX_CHANCE) ++indexOther;
                if (_optionsWriteOrdered[indexThis].Index == KniffelPointsTable.INDEX_CHANCE) ++indexThis;
                if (indexOther == other._optionsWriteOrdered.Count) return this;
                if (indexThis == _optionsWriteOrdered.Count) return other;
                if (_optionsWriteOrdered[indexThis].ValueD.Equals(other._optionsWriteOrdered[indexOther].ValueD))
                {
                    ++indexOther;
                    ++indexThis;
                }
                else
                {
                    return other._optionsWriteOrdered[indexOther].ValueD > _optionsWriteOrdered[indexThis].ValueD
                        ? other
                        : this;
                }
            }

            return indexOther == other._optionsWriteOrdered.Count ? this : other;
        }

        public ShufflingOption MaxOptionMax(ShufflingOption other)
        {
            if (other == null) return this;
            int index = 0;
            while (index < _optionsWriteOrdered.Count && index < other._optionsWriteOrdered.Count)
            {
                if (_optionsWriteOrdered[index].ValueD.Equals(other._optionsWriteOrdered[index].ValueD)) ++index;
                else
                {
                    return other._optionsWriteOrdered[index].ValueD > _optionsWriteOrdered[index].ValueD
                        ? other
                        : this;
                }
            }

            return index == other._optionsWriteOrdered.Count ? this : other;
        }

        public ShufflingOption MaxOptionMaxOrSum(ShufflingOption other)
        {
            if (other?.Max == null) return this;
            if (!Max.ValueD.Equals(other.Max.ValueD))
                return Max.ValueD < other.Max.ValueD ? other : this;
            return Sum < other.Sum ? other : this;
        }


        public ShufflingOption MaxOptionAverage(ShufflingOption other)
            => other == null || other.Average < Average ? this : other;

        public ShufflingOption MaxOptionSum(ShufflingOption other)
            => other == null || other.Sum < Sum ? this : other;

        public ShufflingOption MaxOptionWriteCount(ShufflingOption other)
            => other == null || other.WriteCount < WriteCount ? this : other;

        public ShufflingOption MaxOptionAverageWrite(ShufflingOption other)
            => other == null || other.AverageWrite < AverageWrite ? this : other;

        #endregion
    }
}