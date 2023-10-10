namespace SpieleSammlung.Model.Kniffel.Fields
{
    /// <summary>A Kniffel field that is used for storing sums of other fields.</summary>
    internal class KniffelFieldSum : KniffelField
    {
        /// <inheritdoc cref="KniffelField.Value"/>
        public override int Value { set; get; } = 0;

        /// <summary>Creates a new empty field.</summary>
        public KniffelFieldSum()
        {
        }
    }
}