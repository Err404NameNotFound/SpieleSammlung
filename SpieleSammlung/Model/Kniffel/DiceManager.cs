using System;
using System.Collections.Generic;
using SpieleSammlung.Model.Kniffel.Count;
using SpieleSammlung.Model.Kniffel.Fields;

namespace SpieleSammlung.Model.Kniffel
{
    public class DiceManager : Dice
    {
        #region constants and static member

        /// <summary>Probability that a specific dice value occurs when rolling one dice.</summary>
        public const double PROBABILITY = 1.0d / VALUE_SPAN;

        /// <summary>Average value of a die.</summary>
        public const double AVERAGE_VALUE = ((HIGHEST_VALUE - 1) * HIGHEST_VALUE
                                             - (LOWEST_VALUE - 1) * LOWEST_VALUE) / (2.0d * VALUE_SPAN);

        /// <summary>Probability of a full house rolling all dices.</summary>
        private static readonly double ProbabilityFh;

        /// <summary>Probability of a small street when rolling all dices.</summary>
        private static readonly double ProbabilitySs;

        /// <summary>Probability of a big street when rolling all dices.</summary>
        private static readonly double ProbabilityBs;

        /// <summary>Probability of kniffel when rolling all dices.</summary>
        private static readonly double ProbabilityK;

        /// <summary>Expected value of the field pair with size three when rolling all dice once.</summary>
        private static readonly double ExpectedValueOfPair3;

        /// <summary>Expected value of the field pair with size four when rolling all dice once.</summary>
        private static readonly double ExpectedValueOfPair4;

        /// <summary>Expected value of the field Chance when rolling all dice once.</summary>
        public const double EXPECTED_VALUE_OF_CHANCE = AVERAGE_VALUE * DICE_COUNT;

        /// <summary>Template for indexes of all dices.</summary>
        private static readonly int[] AllDices;

        private static readonly double[][] ExpectedValues;

        private const int INDEX_PAIR3 = 6;
        private const int INDEX_PAIR4 = 7;
        private const int INDEX_FULL_HOUSE = 8;
        private const int INDEX_SMALL_STREET = 9;
        private const int INDEX_BIG_STREET = 10;
        private const int INDEX_KNIFFEL = 11;
        private const int INDEX_CHANCE = 12;
        #endregion

        #region private member

        private readonly Random _rng;

        private readonly int[] _countedDice;

        private readonly CountOrderedList _orderedByCount;

        private readonly ValueOrderedList _orderedByValue;

        #endregion

        #region properties

        /// <summary>Current sum of all dices.</summary>
        public int SumOfAllDices { get; private set; }

        /// <value>Count of the dice with the highest count.</value>
        private int HighestCount => _orderedByCount[0].Count;

        #endregion

        #region constructors

        static DiceManager()
        {
            AllDices = new int[DICE_COUNT];
            for (int i = 0; i < DICE_COUNT; ++i)
            {
                AllDices[i] = i;
            }

            double possibilities = Math.Pow(VALUE_SPAN, DICE_COUNT);
            ProbabilityK = VALUE_SPAN / possibilities;
            ProbabilityBs = (VALUE_SPAN - DICE_COUNT + 1) * Probabilities.Faculty(DICE_COUNT) / possibilities;

            ProbabilitySs = 1200 / possibilities;
            ProbabilityFh = Probabilities.Binomial(DICE_COUNT, 3) * VALUE_SPAN * (VALUE_SPAN - 1) / possibilities;
            ExpectedValueOfPair3 = 805.0 / 216;
            ExpectedValueOfPair4 = 455.0 / 1296;

            ExpectedValues = new double[(int)Math.Pow(VALUE_SPAN + 1, DICE_COUNT)][];
            DiceManager dice = new DiceManager();
            int[] divisors = new int[DICE_COUNT];
            const int mod = VALUE_SPAN + 1;
            for (int e = 0; e < DICE_COUNT; ++e)
            {
                divisors[e] = (int)Math.Pow(VALUE_SPAN + 1, DICE_COUNT - e - 1);
            }

            for (int i = 0; i < ExpectedValues.Length; ++i)
            {
                dice.Shuffle();
                for (int e = 0; e < DICE_COUNT; ++e)
                {
                    int value = i / divisors[e] % mod;
                    if (value == 0)
                    {
                        dice.Remove(e);
                    }
                    else
                    {
                        dice.SetIndexToNewValue(e, value);
                    }
                }

                ExpectedValues[i] = CalculateExpectedValues(dice);
            }
        }

        public DiceManager() : this(new Random())
        {
        }

        public DiceManager(Random rng)
        {
            _countedDice = new int[HIGHEST_VALUE - LOWEST_VALUE];
            _orderedByCount = new CountOrderedList();
            _orderedByValue = new ValueOrderedList();
            SumOfAllDices = 0;
            _rng = rng;
            UnSetCount = DICE_COUNT;
            Shuffle();
        }

        private DiceManager(DiceManager other) : base(other) //TODO decide whether this should be tested
        {
            _countedDice = (int[])other._countedDice.Clone();
            _rng = new Random();
            _orderedByCount = new CountOrderedList(other._orderedByCount);
            _orderedByValue = new ValueOrderedList(other._orderedByValue);
            UnSetCount = other.UnSetCount;
            SumOfAllDices = other.SumOfAllDices;
        }

        #endregion

        #region shuffling

        /// <summary>Rolls all dices.</summary>
        public void Shuffle() => Shuffle(AllDices);

        /// <summary>Rolls the dices with the corresponding indexes.</summary>
        /// <param name="index">Indexes of the dices to be rolled.</param>
        public void Shuffle(IEnumerable<int> index)
        {
            foreach (int i in index)
            {
                SetIndexToNewValue(i, GenerateDiceValue());
            }
        }

        private void IncreaseCounters(int value)
        {
            _orderedByValue.IncCount(value);
            _orderedByCount.IncCount(value);
            SumOfAllDices += value;
            ++_countedDice[value - 1];
        }

        private void DecreaseCounters(int value)
        {
            --_countedDice[value - 1];
            SumOfAllDices -= value;
            _orderedByCount.DecCount(value);
            _orderedByValue.DecCount(value);
        }

        private new void SetIndexToNewValue(int index, int newValue)
        {
            if (newValue != Dices[index])
            {
                var previousValue = Dices[index];
                if (base.SetIndexToNewValue(index, newValue)) DecreaseCounters(previousValue);
                IncreaseCounters(newValue);
            }
        }

        /// <summary>Generates a new random value for a Dice. </summary>
        /// <returns>Random dice value.</returns>
        public int GenerateDiceValue() => _rng.Next(LOWEST_VALUE, HIGHEST_VALUE);

        #endregion

        #region calculation of options

        /// <summary>Count of the dice with value = <paramref name="index"/> + 1</summary>
        /// <param name="index">Index of the dice.</param>
        /// <returns>Count of the dice with value = <paramref name="index"/> + 1</returns>
        public int GetCountOfValue(int index) => _countedDice[index];

        /// <summary>Checks if enough dice have the same value.</summary>
        /// <param name="amount">Amount of dice that must have the same value</param>
        /// <returns><c>true</c> if <paramref name="amount"/> dice have the same value.</returns>
        public bool IsPairOfSizePossible(int amount) => HighestCount >= amount;

        /// <summary>Checks if a full house is possible.</summary>
        /// <returns><c>true</c> if three dice have the same value and the remaining two are equal too.</returns>
        public bool IsFullHousePossible()
        {
            return _orderedByCount.Count >= 2 && HighestCount == 3 && _orderedByCount[1].Count == 2;
        }

        /// <summary>Checks if dice form a small street.</summary>
        /// <returns><c>true</c> if four dice are different and consecutive.</returns>
        public bool IsSmallStreetPossible()
        {
            bool possible = _orderedByCount.Count == 4 && DiceAreConsecutive(0, 3);
            if (!possible && _orderedByCount.Count == 5)
            {
                possible = DiceAreConsecutive(1, 3) && (DiceAreConsecutive(0, 1) || DiceAreConsecutive(3, 4));
            }

            return possible;
        }

        /// <summary>Checks if dice form a big street.</summary>
        /// <returns><c>true</c> if all dice are different and consecutive.</returns>
        public bool IsBigStreetPossible() => _orderedByCount.Count == DICE_COUNT && DiceAreConsecutive(0, 4);

        /// <summary>Checks if dice form a Kniffel.</summary>
        /// <returns><c>true</c> if all dice have the same value.</returns>
        public bool IsKniffelPossible() => IsPairOfSizePossible(DICE_COUNT);

        private bool DiceAreConsecutive(int start, int end)
        {
            bool correct = true;
            while (correct && start < end)
            {
                correct = _orderedByValue[start].Value == _orderedByValue[start + 1].Value - 1;
                ++start;
            }

            return correct;
        }

        #endregion

        #region ecpected values of single fields

        public double EOfTop6(int value) => value * (_countedDice[value - 1] + UnSetCount * PROBABILITY);

        public double EOfPair(int pairSize)
        {
            switch (UnSetCount)
            {
                case DICE_COUNT:
                    return pairSize == 3 ? ExpectedValueOfPair3 : ExpectedValueOfPair4;
                case 0:
                    return HighestCount >= pairSize ? SumOfAllDices : 0;
            }

            if (HighestCount + UnSetCount < pairSize)
            {
                return 0;
            }

            int[] values = new int[UnSetCount];
            int[] unset = GetUnsetDiceIndex();
            for (int i = 0; i < values.Length; ++i)
            {
                values[i] = LOWEST_VALUE;
            }

            int ret = 0;
            int differentValues = (int)CurrentPossibilities;
            DiceManager clone = new DiceManager(this);
            for (int i = 0; i < differentValues; ++i)
            {
                SetDiceValues(unset, values, clone);
                if (clone.IsPairOfSizePossible(pairSize))
                {
                    ret += clone.SumOfAllDices;
                }

                SetToNextValuePair(values);
            }

            return ret * Math.Pow(PROBABILITY, UnSetCount);
        }

        public double POfFullHouse()
        {
            switch (UnSetCount)
            {
                case 0: return IsFullHousePossible() ? 1 : 0;
                case 1:
                    if (_orderedByCount.Count == 2)
                    {
                        return HighestCount == 3 ? PROBABILITY : 2 * PROBABILITY;
                    }

                    return 0;
                case DICE_COUNT: return ProbabilityFh;
                default:
                    if (HighestCount == 3)
                    {
                        return PROBABILITY;
                    }

                    if (_orderedByCount.Count > 2)
                    {
                        return 0;
                    }

                    double optionMoreOfFirst = Probabilities.Binomial(UnSetCount, 3 - HighestCount);
                    double possibilities = optionMoreOfFirst
                                           + Probabilities.Binomial(UnSetCount, UnSetCount - (2 - HighestCount));
                    if (_orderedByCount.Count == 1)
                    {
                        possibilities *= 5;
                    }

                    return possibilities / CurrentPossibilities;
            }
        }

        public double POfSmallStreet()
        {
            switch (UnSetCount)
            {
                case 0:
                    return IsSmallStreetPossible() ? 1 : 0;
                case DICE_COUNT:
                    return ProbabilitySs;
            }

            if (UnSetCount + _orderedByCount.Count < 4)
            {
                return 0;
            }

            int[] values = new int[UnSetCount];
            int[] unset = GetUnsetDiceIndex();
            for (int i = 0; i < values.Length; ++i)
            {
                values[i] = LOWEST_VALUE;
            }

            int sumOfPossible = 0;
            int differentValues = (int)CurrentPossibilities;
            DiceManager clone = new DiceManager(this);
            for (int i = 0; i < differentValues; ++i)
            {
                SetDiceValues(unset, values, clone);
                if (clone.IsSmallStreetPossible())
                {
                    ++sumOfPossible;
                }

                SetToNextValuePair(values);
            }

            return sumOfPossible / (double)differentValues;
        }

        public double POfBigStreet()
        {
            switch (UnSetCount)
            {
                case 0:
                    return IsBigStreetPossible() ? 1 : 0;
                case DICE_COUNT:
                    return ProbabilityBs;
            }

            if (_countedDice[0] > 0 && _countedDice[_countedDice.Length - 1] > 0 || HighestCount > 1)
            {
                return 0;
            }

            double ret = Probabilities.Faculty(UnSetCount) / CurrentPossibilities;
            if (_countedDice[0] == 0 && _countedDice[_countedDice.Length - 1] == 0)
            {
                ret *= 2;
            }

            return ret;
        }

        public double POfKniffel()
        {
            return UnSetCount switch
            {
                0 => IsKniffelPossible() ? 1 : 0,
                DICE_COUNT => ProbabilityK,
                _ => _orderedByCount.Count > 1 ? 0 : Math.Pow(PROBABILITY, UnSetCount)
            };
        }

        public double EOfChance() => SumOfAllDices + AVERAGE_VALUE * UnSetCount;

        #endregion

        #region generation of options
        
        private static int IndexOfDiceConfiguration(DiceManager dice) => IndexOfDiceConfiguration(dice.Dices);

        public static int IndexOfDiceConfiguration(IReadOnlyList<int> dice)
        {
            const int factor = VALUE_SPAN + 1;
            int index = 0;
            for (int i = 0; i < DICE_COUNT; ++i)
            {
                index = factor * index + (dice[i] == DICE_NOT_SET_VALUE ? 0 : dice[i]);
            }

            return index;
        }

        private double CurrentPossibilities => Math.Pow(VALUE_SPAN, UnSetCount);

        private static double[] CalculateExpectedValues(DiceManager dice)
        {
            double[] ret = new double[KniffelPointsTable.WRITEABLE_FIELDS_COUNT];
            for (int i = 0; i < 6; ++i)
            {
                ret[i] = dice.EOfTop6(i + 1);
            }

            ret[INDEX_PAIR3] = dice.EOfPair(3);
            ret[INDEX_PAIR4] = dice.EOfPair(4);
            ret[INDEX_FULL_HOUSE] = dice.POfFullHouse() * KniffelGame.VALUE_FULL_HOUSE;
            ret[INDEX_SMALL_STREET] = dice.POfSmallStreet() * KniffelGame.VALUE_SMALL_STREET;
            ret[INDEX_BIG_STREET] = dice.POfBigStreet() * KniffelGame.VALUE_BIG_STREET;
            ret[INDEX_KNIFFEL] = dice.POfKniffel() * KniffelGame.VALUE_KNIFFEL;
            ret[INDEX_CHANCE] = dice.EOfChance();
            return ret;
        }

        public static List<WriteOption> CalculateExpectedValues(KniffelPlayer player, DiceManager dice)
        {
            return CalculateExpectedValues(player, IndexOfDiceConfiguration(dice));
        }

        private static List<WriteOption> CalculateExpectedValues(KniffelPlayer player, int index)
        {
            List<WriteOption> options = [];
            double[] expected = ExpectedValues[index];
            for (int i = 0; i < 6; ++i)
            {
                Add(player, options, i, expected[i]);
            }

            Add(player, options, KniffelPointsTable.INDEX_PAIR_SIZE_3, expected[INDEX_PAIR3]);
            Add(player, options, KniffelPointsTable.INDEX_PAIR_SIZE_4, expected[INDEX_PAIR4]);
            Add(player, options, KniffelPointsTable.INDEX_FULL_HOUSE, expected[INDEX_FULL_HOUSE]);
            Add(player, options, KniffelPointsTable.INDEX_SMALL_STREET, expected[INDEX_SMALL_STREET]);
            Add(player, options, KniffelPointsTable.INDEX_BIG_STREET, expected[INDEX_BIG_STREET]);
            Add(player, options, KniffelPointsTable.INDEX_KNIFFEL, expected[INDEX_KNIFFEL]);
            Add(player, options, KniffelPointsTable.INDEX_CHANCE, expected[INDEX_CHANCE]);
            return options;
        }

        private static void Add(KniffelPlayer player, ICollection<WriteOption> options, int index, double value)
        {
            if (player[index].IsEmpty())
            {
                options.Add(new WriteOption(index, value));
            }
        }

        public static List<ShufflingOption> GenerateAllOptions(KniffelPlayer player, DiceManager dice)
        {
            List<ShufflingOption> ret = [];
            int combinations = COMBINATIONS_UNSET_DICE;
            int[] values = (int[])dice.Dices.Clone();
            int[] divisors = [16, 8, 4, 2, 1];
            for (int i = 0; i < combinations; ++i)
            {
                for (int d = 0; d < DICE_COUNT; ++d)
                {
                    values[d] = i / divisors[d] % 2 == 0 ? 0 : dice[d];
                }

                var index = IndexOfDiceConfiguration(values);
                ret.Add(new ShufflingOption(CreateFromDice(values), CalculateExpectedValues(player, index)));
            }

            return ret;
        }
        
        public static double EofTop6(int index, int value) => ExpectedValues[index][value];
        public static double EofPairSize3(int index) => ExpectedValues[index][INDEX_PAIR3];
        public static double EofPairSize4(int index) => ExpectedValues[index][INDEX_PAIR4];
        public static double EofSmallStreet(int index) => ExpectedValues[index][INDEX_SMALL_STREET];
        public static double EofBigStreet(int index) => ExpectedValues[index][INDEX_BIG_STREET];
        public static double EofFullHouse(int index) => ExpectedValues[index][INDEX_FULL_HOUSE];
        public static double EofKniffel(int index) => ExpectedValues[index][INDEX_KNIFFEL];
        public static double EofChance(int index) => ExpectedValues[index][INDEX_CHANCE];

        // public static void FillAllOptions(ShufflingOption[] options, KniffelPlayer player, DiceManager dice)
        // {
        //     int[] values = (int[])dice.dices.Clone();
        //     int[] divisors = { 16, 8, 4, 2, 1 };
        //     for (int i = 0; i < COMBINATIONS_UNSET_DICE; ++i)
        //     {
        //         for (int d = 0; d < DICE_COUNT; ++d)
        //         {
        //             values[d] = i / divisors[d] % 2 == 0 ? 0 : dice[d];
        //         }
        //
        //         var index = IndexOfDiceConfiguration(values);
        //         options[i] = new ShufflingOption(CreateFromDice(values), CalculateExpectedValues(player, index));
        //     }
        // }

        #endregion

        #region removing dice and iterating through unset clones

        public void Remove(params int[] indexes)
        {
            foreach (int i in indexes)
            {
                Remove(i);
            }
        }

        public new void Remove(int i)
        {
            var value = Dices[i];
            base.Remove(i);
            DecreaseCounters(value);
        }

        private static void SetDiceValues(IReadOnlyList<int> indexes, IReadOnlyList<int> values, DiceManager clone)
        {
            for (int dice = 0; dice < indexes.Count; ++dice)
            {
                clone.SetIndexToNewValue(indexes[dice], values[dice]);
            }
        }

        private static void SetToNextValuePair(IList<int> values)
        {
            int w = 0;
            ++values[w];
            while (values[w] == HIGHEST_VALUE)
            {
                values[w] = LOWEST_VALUE;
                ++w;
                if (w == values.Count) break;
                ++values[w];
            }
        }

        #endregion
    }
}