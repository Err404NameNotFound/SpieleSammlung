using System;
using System.Text;

namespace SpieleSammlung.Model.Kniffel;

public class Dice
{
    #region constants

    /// <summary>Amount of dices in a Kniffel game.</summary>
    public const int DICE_COUNT = 5;

    /// <summary>Lowest value of a die.</summary>
    public const int LOWEST_VALUE = 1;

    /// <summary>Highest value of a die (excluded).</summary>
    public const int HIGHEST_VALUE = 7;

    /// <summary>Amount of different values possible for a dice.</summary>
    public const int VALUE_SPAN = HIGHEST_VALUE - LOWEST_VALUE;

    /// <summary>Amount of possible combinations to remove a die from a full Dice object.</summary>
    public static readonly int COMBINATIONS_UNSET_DICE = (int)Math.Pow(2, DICE_COUNT);

    /// <summary>If a die has this value it is not set.</summary>
    internal const int DICE_NOT_SET_VALUE = 0;

    #endregion

    #region members

    protected int UnSetCount;

    protected readonly int[] Dices;

    /// <summary>Value of the dice at <paramref name="index"/>.</summary>
    public int this[int index] => Dices[index];

    #endregion

    #region constructors

    protected Dice()
    {
        Dices = new int[DICE_COUNT];
        UnSetCount = DICE_COUNT;
    }

    protected Dice(Dice other) : this(other.Dices)
    {
    }

    private Dice(int[] dices)
    {
        Dices = (int[])dices.Clone();
        UnSetCount = 0;
        for (var index = 0; index < dices.Length; index++)
        {
            if (IsDiceNotSet(index))
            {
                ++UnSetCount;
            }
        }
    }

    #endregion

    #region methods

    protected void Remove(int i)
    {
        if (IsDiceNotSet(i))
            throw new ArgumentException("This dice has already been removed.");

        Dices[i] = DICE_NOT_SET_VALUE;
        ++UnSetCount;
    }

    /// <summary>
    /// Sets the dice to the new value.
    /// </summary>
    /// <param name="index">Index of the dice to change its value.</param>
    /// <param name="newValue">New value for the given dice</param>
    /// <returns><c>true</c> if the dice was already set</returns>
    protected bool SetIndexToNewValue(int index, int newValue)
    {
        if (IsDiceNotSet(index))
        {
            --UnSetCount;
            Dices[index] = newValue;
            return false;
        }

        Dices[index] = newValue;
        return true;
    }

    public int[] GetUnsetDiceIndex()
    {
        int[] unset = new int[UnSetCount];
        int w = 0;
        for (int i = 0; i < Dices.Length; ++i)
        {
            if (IsDiceNotSet(i))
                unset[w++] = i;
        }

        return unset;
    }

    private bool IsDiceNotSet(int index) => Dices[index] == DICE_NOT_SET_VALUE;

    internal static Dice CreateFromDice(int[] values) => new(values);

    /// <summary>Returns a string representation of this object representing the current dice values.</summary>
    /// <returns>String resembling the dice values.</returns>
    public override string ToString()
    {
        StringBuilder builder = new StringBuilder(15).Append("{ ");
        builder.Append(GetDiceRepresentation(0));
        for (int i = 1; i < Dices.Length; ++i)
        {
            builder.Append(", ").Append(GetDiceRepresentation(i));
        }

        return builder.Append(" }").ToString();
    }

    private string GetDiceRepresentation(int index) => IsDiceNotSet(index) ? "_" : Dices[index].ToString();

    #endregion
}