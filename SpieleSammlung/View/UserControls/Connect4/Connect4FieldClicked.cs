using System;

namespace SpieleSammlung.View.UserControls.Connect4;

internal class Connect4FieldClicked(int column) : EventArgs
{
    public int Column { private set; get; } = column;
}