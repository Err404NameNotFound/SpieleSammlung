namespace SpieleSammlung.Model.Kniffel.Fields
{
    /// <summary>Field in the table of points in a Kniffel match.</summary>
    public abstract class KniffelField
    {
        /// <summary>Value for an empty field that is yet to be written.</summary>
        protected const int EMPTY_FIELD = -1;

        /// <value>Value of the field.</value>
        public abstract int Value { get; set; }

        /// <value>Flag is the field is empty.</value>
        public bool IsEmpty() => Value == EMPTY_FIELD;
    }
}