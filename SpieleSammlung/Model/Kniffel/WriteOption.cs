#region

using SpieleSammlung.Model.Kniffel.Fields;

#endregion

namespace SpieleSammlung.Model.Kniffel;

/// <summary>
/// A key-value-pair for Kniffel matches. The key (Index) stands for the index of a field in a Kniffel match and the value
/// is the potential value of the field.
/// </summary>
public class WriteOption
{
    /// <summary>Creates a new instance and sets its values.</summary>
    /// <param name="index"><c>Index</c> of the field</param>
    /// <param name="value">Possible <c>value</c> for the field</param>
    public WriteOption(int index, int value) =>
        (Index, Value, ValueD, DoublePrecision) = (index, value, value, false);

    /// <inheritdoc cref="WriteOption(int, int)"/>
    public WriteOption(int index, double value) =>
        (Index, Value, ValueD, DoublePrecision) = (index, (int)value, value, true);

    /// <value>Index of the field.</value>
    public int Index { get; }

    /// <value>Possible Value for this field</value>
    public int Value { get; }

    /// <value>Possible Value for this field</value>
    public double ValueD { get; }

    private bool DoublePrecision { get; }

    public override string ToString()
    {
        return $"{KniffelPointsTable.FIELD_NAMES[Index]} " +
               $"-> {(DoublePrecision ? ValueD.ToString("N2") : Value + "")}";
    }
}