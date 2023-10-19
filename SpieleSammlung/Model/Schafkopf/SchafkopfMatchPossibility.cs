using System.Collections.Generic;

namespace SpieleSammlung.Model.Schafkopf
{
    public class SchafkopfMatchPossibility
    {
        public SchafkopfMode Mode { get; }
        public readonly IReadOnlyList<string> colors;

        public SchafkopfMatchPossibility(SchafkopfMode m, IReadOnlyList<string> c)
        {
            Mode = m;
            colors = c;
        }

        public SchafkopfMatchPossibility(SchafkopfMode m)
        {
            Mode = m;
            colors = new List<string> { "" };
        }

        public override string ToString()
        {
            if (colors.Count == 1 && colors[0].Equals("")) return Mode.ToString();
            string tmp = Mode + ": " + string.Join(", ", colors);
            return tmp;
        }
    }
}