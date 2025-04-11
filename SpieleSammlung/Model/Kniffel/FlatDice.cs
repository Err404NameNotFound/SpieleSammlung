using System;
using System.Collections.Generic;
using System.Linq;
using SpieleSammlung.Model.Kniffel.Fields;

namespace SpieleSammlung.Model.Kniffel;

public class FlatDice
{
    #region private member

    private readonly int[] _dices = new int[Dice.DICE_COUNT];
    private readonly int[] _counters = new int[Dice.VALUE_SPAN];
    private int _index;

    private readonly Random _rng;

    /// <summary>Template for indexes of all dices.</summary>
    private static readonly int[] IndexAllDices;

    private static readonly int[] Divisors = [16, 8, 4, 2, 1];

    #endregion

    #region properties

    /// <summary>Copy of all dices indexes.</summary>
    public static int[] AllDices => (int[])IndexAllDices.Clone();

    /// <summary>Current sum of all dices.</summary>
    public int SumOfAllDices { get; private set; }

    public int this[int index] => _dices[index];

    #endregion

    #region constructors

    static FlatDice()
    {
        IndexAllDices = new int[Dice.DICE_COUNT];
        for (int i = 0; i < IndexAllDices.Length; i++)
        {
            IndexAllDices[i] = i;
        }
    }

    public FlatDice() : this(new Random())
    {
    }

    public FlatDice(int seed) : this(new Random(seed))
    {
    }

    public FlatDice(Random rng)
    {
        SumOfAllDices = 0;
        _rng = rng;
        foreach (int i in AllDices)
        {
            int newValue = GenerateDiceValue();
            _dices[i] = newValue;
            SumOfAllDices += newValue;
            ++_counters[newValue - Dice.LOWEST_VALUE];
        }

        _index = DiceManager.IndexOfDiceConfiguration(_dices);
    }

    public FlatDice(FlatDice other)
    {
        SumOfAllDices = other.SumOfAllDices;
        _index = other._index;
        _counters = (int[])other._counters.Clone();
        _dices = (int[])other._dices.Clone();
        _rng = new Random();
    }

    #endregion

    #region shuffling

    /// <summary>Rolls all dices.</summary>
    public void Shuffle() => Shuffle(IndexAllDices);

    /// <summary>Rolls the dices with the corresponding indexes.</summary>
    /// <param name="index">Indexes of the dices to be rolled.</param>
    public void Shuffle(IEnumerable<int> index)
    {
        foreach (int i in index)
        {
            int newValue = GenerateDiceValue();
            if (_dices[i] == newValue) continue;
            --_counters[_dices[i] - Dice.LOWEST_VALUE];
            SumOfAllDices -= _dices[i];
            _dices[i] = newValue;
            SumOfAllDices += newValue;
            ++_counters[newValue - Dice.LOWEST_VALUE];
        }

        _index = DiceManager.IndexOfDiceConfiguration(_dices);
    }

    /// <summary>Generates a new random value for a Dice. </summary>
    /// <returns>Random dice value.</returns>
    public int GenerateDiceValue() => _rng.Next(Dice.LOWEST_VALUE, Dice.HIGHEST_VALUE);

    #endregion

    #region calculation of options

    /// <summary>Count of the dice with value = <paramref name="index"/> + 1</summary>
    /// <param name="index">Index of the dice.</param>
    /// <returns>Count of the dice with value = <paramref name="index"/> + 1</returns>
    public int GetCountOfValue(int index) => _counters[index];

    /// <summary>Checks if enough dice have the same value.</summary>
    /// <param name="amount">Amount of dice that must have the same value</param>
    /// <returns><c>true</c> if <paramref name="amount"/> dice have the same value.</returns>
    public bool IsPairOfSizePossible(int amount) => _counters.Max() >= amount;

    /// <summary>Checks if a full house is possible.</summary>
    /// <returns><c>true</c> if three dice have the same value and the remaining two are equal too.</returns>
    public bool IsFullHousePossible() => DiceManager.EofFullHouse(_index) > 0;

    /// <summary>Checks if dice form a small street.</summary>
    /// <returns><c>true</c> if four dice are different and consecutive.</returns>
    public bool IsSmallStreetPossible() => DiceManager.EofSmallStreet(_index) > 0;

    /// <summary>Checks if dice form a big street.</summary>
    /// <returns><c>true</c> if all dice are different and consecutive.</returns>
    public bool IsBigStreetPossible() => DiceManager.EofBigStreet(_index) > 0;

    /// <summary>Checks if dice form a Kniffel.</summary>
    /// <returns><c>true</c> if all dice have the same value.</returns>
    public bool IsKniffelPossible() => DiceManager.EofKniffel(_index) > 0;

    #endregion

    #region ecpected values

    public static List<WriteOption> CalculateExpectedValues(KniffelPlayer player, FlatDice dice)
    {
        return CalculateExpectedValues(player, dice._index);
    }

    private static List<WriteOption> CalculateExpectedValues(KniffelPlayer player, int index)
    {
        List<WriteOption> options = [];
        for (int i = 0; i < 6; ++i)
        {
            Add(player, options, i, DiceManager.EofTop6(index, i));
        }

        Add(player, options, KniffelPointsTable.INDEX_PAIR_SIZE_3, DiceManager.EofPairSize3(index));
        Add(player, options, KniffelPointsTable.INDEX_PAIR_SIZE_4, DiceManager.EofPairSize4(index));
        Add(player, options, KniffelPointsTable.INDEX_FULL_HOUSE, DiceManager.EofFullHouse(index));
        Add(player, options, KniffelPointsTable.INDEX_SMALL_STREET, DiceManager.EofSmallStreet(index));
        Add(player, options, KniffelPointsTable.INDEX_BIG_STREET, DiceManager.EofBigStreet(index));
        Add(player, options, KniffelPointsTable.INDEX_KNIFFEL, DiceManager.EofKniffel(index));
        Add(player, options, KniffelPointsTable.INDEX_CHANCE, DiceManager.EofChance(index));
        return options;
    }

    private static void Add(KniffelPlayer player, ICollection<WriteOption> options, int index, double value)
    {
        if (player[index].IsEmpty())
        {
            options.Add(new WriteOption(index, value));
        }
    }

    public static List<ShufflingOption> GenerateAllOptions(KniffelPlayer player, FlatDice dice)
    {
        List<ShufflingOption> ret = [];
        int combinations = Dice.COMBINATIONS_UNSET_DICE;
        int[] values = (int[])dice._dices.Clone();
        for (int i = 0; i < combinations; ++i)
        {
            for (int d = 0; d < Dice.DICE_COUNT; ++d)
            {
                values[d] = i / Divisors[d] % 2 == 0 ? 0 : dice[d];
            }

            var index = DiceManager.IndexOfDiceConfiguration(values);
            ret.Add(new ShufflingOption(Dice.CreateFromDice(values), CalculateExpectedValues(player, index)));
        }

        return ret;
    }

    public Dice ToDice() => Dice.CreateFromDice(_dices);

    public int[] IndexToFlipWhenOptimisingToBigStreet()
    {
        for (int i = 0; i < Dice.DICE_COUNT; ++i)
        {
            int previous = this[i];
            _dices[i] = Dice.DICE_NOT_SET_VALUE;
            int index = DiceManager.IndexOfDiceConfiguration(_dices);
            _dices[i] = previous;
            if ((int)DiceManager.EofSmallStreet(index) == KniffelGame.VALUE_SMALL_STREET
                && DiceManager.EofBigStreet(index) > 0)
            {
                return [i];
            }
        }

        throw new ArgumentException("The change of one dice does not suffice to get a big street");
    }

    #endregion

    public override string ToString() => "{ " + string.Join(", ", _dices) + " }";
}