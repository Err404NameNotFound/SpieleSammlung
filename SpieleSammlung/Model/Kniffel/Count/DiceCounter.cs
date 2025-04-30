namespace SpieleSammlung.Model.Kniffel.Count;

/// <summary>
/// A counter that stores a value and a count. Can be used to count how may dices have a specific value.
/// </summary>
public class DiceCounter
{
    /// <summary>
    /// Initiates a new instance whith <c cref="Count">Count</c> 1 and <c cref="Value">Value</c> <paramref name="value"/>.
    /// </summary>
    /// <param name="value">Value of the dice.</param>
    /// <param name="count">Initial count of the dice.</param>
    public DiceCounter(int value, int count) => (Value, Count) = (value, count);

    /// <summary>Clones the given object.</summary>
    /// <param name="other">Object to be cloned.</param>
    public DiceCounter(DiceCounter other) => (Value, Count) = (other.Value, other.Count);

    /// <value>Value of the dice.</value>
    public int Value { get; }

    /// <value>Amount of times a die with this value occured.</value>
    public int Count { get; private set; }

    /// <summary>Increases the count by one.</summary>
    public void IncCount() => ++Count;

    /// <summary>Decreases the count by one.</summary>
    public void DecCount() => --Count;

    /// <summary>String representation in the form { Value = value, Count = count }.</summary>
    public override string ToString() => $"{{ Value={Value}, Count={Count} }}";
}