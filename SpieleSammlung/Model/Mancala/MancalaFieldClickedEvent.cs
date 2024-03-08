using System;

namespace SpieleSammlung.Model.Mancala
{
    public class MancalaFieldClickedEvent : EventArgs
    {
        public int Index { get; }

        public MancalaFieldClickedEvent(int index)
        {
            Index = index;
        }
    }
}