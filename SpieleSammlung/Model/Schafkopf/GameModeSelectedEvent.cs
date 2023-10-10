using System;

namespace SpieleSammlung.Model.Schafkopf
{
    public class GameModeSelectedEvent : EventArgs
    {
        public readonly SchafkopfMode mode;
        public readonly string color;

        public GameModeSelectedEvent(SchafkopfMode mode, string color)
        {
            this.mode = mode;
            this.color = color;
        }
    }
}