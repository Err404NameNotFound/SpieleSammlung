using System.Text;

namespace SpieleSammlung.Model.Connect4
{
    /// <summary>Represents a point in a 2-dimensional coordinate system with integers.</summary>
    public class Coordinates2D
    {
        /// <value>X-coordinate of the point.</value>
        public int X { get; }

        /// <value>Y-coordinate of the point.</value>
        public int Y { get; }

        /// <summary>Initiates a new Instance</summary>
        /// <param name="x">X-coordinate of the point.</param>
        /// <param name="y">Y-coordinate of the point.</param>
        public Coordinates2D(int x, int y)
        {
            X = x;
            Y = y;
        }


        /// <summary>
        /// Creates a String representation of the coordinate in the form (y,x) with an offset for each axis.
        /// </summary>
        /// <param name="offsetX">Offset for the x-axis.</param>
        /// <param name="offsetY">Offset for the y-axis.</param>
        /// <return>String representation in the form (y,x)</return>
        public string ToString(int offsetX, int offsetY)
        {
            StringBuilder ret = new StringBuilder();
            ret.Append("(");
            ret.Append(Y + offsetY);
            ret.Append(", ");
            ret.Append(X + offsetX);
            ret.Append(")");
            return ret.ToString();
        }
    }
}