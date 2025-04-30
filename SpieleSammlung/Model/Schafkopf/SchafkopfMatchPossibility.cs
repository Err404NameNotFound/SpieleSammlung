using System.Collections.Generic;

namespace SpieleSammlung.Model.Schafkopf;

public class SchafkopfMatchPossibility(SchafkopfMode m, IReadOnlyList<CardColor?> c)
{
    public SchafkopfMode Mode { get; } = m;
    public readonly IReadOnlyList<CardColor?> Colors = c;

    public SchafkopfMatchPossibility(SchafkopfMode m) : this(m, new List<CardColor?> { null })
    {
    }

    public override string ToString()
    {
        if (Colors.Count == 1 && Colors[0] == null) return Mode.ToString();
        string tmp = Mode + ": " + string.Join(", ", Colors);
        return tmp;
    }
}