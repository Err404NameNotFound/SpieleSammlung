using System.Collections.Generic;

namespace SpieleSammlung.Model.Schafkopf
{
    public class SchafkopfMatchPossibility(SchafkopfMode m, IReadOnlyList<string> c)
    {
        public SchafkopfMode Mode { get; } = m;
        public readonly IReadOnlyList<string> Colors = c;

        public SchafkopfMatchPossibility(SchafkopfMode m) : this(m, new List<string> { "" })
        {
        }

        public override string ToString()
        {
            if (Colors.Count == 1 && Colors[0].Equals("")) return Mode.ToString();
            string tmp = Mode + ": " + string.Join(", ", Colors);
            return tmp;
        }
    }
}