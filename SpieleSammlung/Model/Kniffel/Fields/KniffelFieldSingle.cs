#region

using System;

#endregion

namespace SpieleSammlung.Model.Kniffel.Fields;

/// <summary>Field in a Kniffel match that stores a value for an ordinary field.</summary>
public class KniffelFieldSingle : KniffelField
{
    private int _fieldValue;

    /// <summary>Creates a new empty field.</summary>
    public KniffelFieldSingle() => _fieldValue = EMPTY_FIELD;

    /// <value>Value of the field that can only change once.</value>
    public override int Value
    {
        set
        {
            if (!IsEmpty())
                throw new ArgumentException("Field has already been written.");

            _fieldValue = value;
        }
        get => _fieldValue;
    }
}