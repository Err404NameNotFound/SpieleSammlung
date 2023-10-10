using System.Collections.Generic;

namespace SpieleSammlung.Model.Schafkopf
{
    public class SchafkopfMatchPossibility
    {
        public readonly SchafkopfMode mode;
        public readonly List<string> colors;

        public SchafkopfMatchPossibility(SchafkopfMode m, List<string> c)
        {
            mode = m;
            colors = c;
        }

        public SchafkopfMatchPossibility(SchafkopfMode m)
        {
            mode = m;
            colors = new List<string> { "" };
        }

        public override string ToString()
        {
            string tmp = mode.ToString();
            if (!colors[0].Equals(""))
            {
                tmp += ": ";
                for (int i = 0; i < colors.Count; ++i)
                {
                    tmp += colors[i] + (i == colors.Count - 1 ? "" : ", ");
                }
            }

            return tmp;
        }
    }
}