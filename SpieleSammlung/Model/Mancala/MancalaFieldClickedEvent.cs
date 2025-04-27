using System;

namespace SpieleSammlung.Model.Mancala;

public class MancalaFieldClickedEvent(int index) : EventArgs
{
    public int Index { get; } = index;
}