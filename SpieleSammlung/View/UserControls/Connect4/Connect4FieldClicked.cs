using System;

namespace SpieleSammlung.UserControls.Connect4
{
    internal class Connect4FieldClicked : EventArgs
    {
        public Connect4FieldClicked(int column)
        {
            Column = column;
        }

        public int Column { private set; get; }
    }
}