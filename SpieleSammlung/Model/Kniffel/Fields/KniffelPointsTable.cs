using SpieleSammlung.Properties;

namespace SpieleSammlung.Model.Kniffel.Fields;

/// <summary>
/// Table for storing the points of a Kniffel player for each different field.
/// </summary>
public class KniffelPointsTable
{
    #region constants and static readonly fields.

    /// <summary>The minimum sum for the top 6 fields for receiving the bonus.</summary>
    public const int MIN_TOP6_FOR_BONUS = 63;

    /// <summary>Bonus for reaching a high enough sum with the first 6 fields.</summary>
    public const int BONUS_TOP6 = 35;

    /// <summary>Index of the field for the sum of the top 6 fields</summary>
    public const int INDEX_SUM_TOP = 6;

    /// <summary>Index of the field for the bonus.</summary>
    public const int INDEX_BONUS = 7;

    /// <summary>Index of the field for a pair of size 3.</summary>
    public const int INDEX_PAIR_SIZE_3 = 8;

    /// <summary>Index of the field for a pair of size 4.</summary>
    public const int INDEX_PAIR_SIZE_4 = 9;

    /// <summary>Index of the field for a "full house".</summary>
    public const int INDEX_FULL_HOUSE = 10;

    /// <summary>/// Index of the field for a "small street".</summary>
    public const int INDEX_SMALL_STREET = 11;

    /// <summary>Index of the field for a "big street".</summary>
    public const int INDEX_BIG_STREET = 12;

    /// <summary>Index of the field for a "Kniffel".</summary>
    public const int INDEX_KNIFFEL = 13;

    /// <summary>Index of the field for the "chance".</summary>
    public const int INDEX_CHANCE = 14;

    /// <summary>Index of the field for the sum of the top 6 + bonus.</summary>
    public const int INDEX_SUM_TOP_BONUS = 15;

    /// <summary>Index of the field for the sum of the bottom half.</summary>
    public const int INDEX_SUM_BOTTOM = 16;

    /// <summary>Index of the field for the sum of all fields.</summary>
    public const int INDEX_SUM = 17;

    /// <summary>Amount of Fields that can be written once.
    /// This number is equal to the amount of rounds in the corresponding Kniffel game.</summary>
    public static readonly int WRITEABLE_FIELDS_COUNT;

    /// <summary>Display names for all fields.</summary>
    public static readonly string[] FIELD_NAMES;

    /// <summary>Indexes of the fields that can change value.</summary>
    private static readonly int[] ChangeableFields;

    /// <summary>Indexes of the fields that can change their value only once.</summary>
    private static readonly int[] UnChangeableFields;

    #endregion

    #region private member

    private readonly KniffelField[] _fields;

    #endregion

    #region constructor

    static KniffelPointsTable()
    {
        ChangeableFields = [INDEX_SUM_TOP, INDEX_BONUS, INDEX_SUM_TOP_BONUS, INDEX_SUM_BOTTOM, INDEX_SUM];
        FIELD_NAMES = new string[18];
        for (int i = 0; i < 6; ++i)
        {
            FIELD_NAMES[i] = i + 1 + Resources.Knif_PointsTable_Top6AdditionEndOfNumber;
        }

        FIELD_NAMES[ChangeableFields[0]] = Resources.Knif_PointsTable_SumTop;
        FIELD_NAMES[ChangeableFields[1]] = Resources.Knif_PointsTable_Bonus;
        FIELD_NAMES[ChangeableFields[2]] = Resources.Knif_PointsTable_SumTop;
        FIELD_NAMES[ChangeableFields[3]] = Resources.Knif_PointsTable_SumBottom;
        FIELD_NAMES[ChangeableFields[4]] = Resources.Knif_PointsTable_Sum;
        FIELD_NAMES[INDEX_PAIR_SIZE_3] = Resources.Knif_PointsTable_PairSize3;
        FIELD_NAMES[INDEX_PAIR_SIZE_4] = Resources.Knif_PointsTable_PairSize4;
        FIELD_NAMES[INDEX_FULL_HOUSE] = Resources.Knif_PointsTable_FullHouse;
        FIELD_NAMES[INDEX_SMALL_STREET] = Resources.Knif_PointsTable_SmallStreet;
        FIELD_NAMES[INDEX_BIG_STREET] = Resources.Knif_PointsTable_BigStreet;
        FIELD_NAMES[INDEX_KNIFFEL] = Resources.Knif_PointsTable_Kniffel;
        FIELD_NAMES[INDEX_CHANCE] = Resources.Knif_PointsTable_Chance;
        UnChangeableFields = new int[FIELD_NAMES.Length - ChangeableFields.Length];
        int value = 0;
        int next = 0;
        // fill indexes of unchangeable fields
        for (int i = 0; i < UnChangeableFields.Length; ++i)
        {
            while (next < UnChangeableFields.Length && ChangeableFields[next] == value)
            {
                ++next;
                ++value;
            }

            UnChangeableFields[i] = value;
            ++value;
        }

        WRITEABLE_FIELDS_COUNT = UnChangeableFields.Length;
    }

    /// <summary>Creates a new Instance.</summary>
    public KniffelPointsTable()
    {
        _fields = new KniffelField[FIELD_NAMES.Length];
        foreach (var field in ChangeableFields)
            _fields[field] = new KniffelFieldSum();
        foreach (var field in UnChangeableFields)
            _fields[field] = new KniffelFieldSingle();
    }

    #endregion

    #region public methods

    /// <summary>Field in this table with index <paramref name="index"/></summary>
    /// <param name="index">Index of the field</param>
    /// <returns>Field at the given index.</returns>
    public KniffelField this[int index] => _fields[index];

    /// <summary>Updates the bonus, the sums of the top, the sum of the buttom and the overall sum.</summary>
    public void UpdateSums()
    {
        _fields[ChangeableFields[0]].Value = 0;
        for (int i = 0; i < 6; ++i)
        {
            if (!_fields[i].IsEmpty())
            {
                _fields[ChangeableFields[0]].Value += _fields[i].Value;
            }
        }

        _fields[ChangeableFields[1]].Value =
            _fields[ChangeableFields[0]].Value >= MIN_TOP6_FOR_BONUS ? BONUS_TOP6 : 0;
        _fields[ChangeableFields[2]].Value = 0;
        for (int i = 8; i < 15; ++i)
        {
            if (!_fields[i].IsEmpty())
            {
                _fields[ChangeableFields[2]].Value += _fields[i].Value;
            }
        }

        _fields[ChangeableFields[3]].Value =
            _fields[ChangeableFields[0]].Value + _fields[ChangeableFields[1]].Value;
        _fields[ChangeableFields[4]].Value =
            _fields[ChangeableFields[2]].Value + _fields[ChangeableFields[3]].Value;
    }

    #endregion
}