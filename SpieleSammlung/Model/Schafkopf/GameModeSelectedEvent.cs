using System;

namespace SpieleSammlung.Model.Schafkopf;

public class GameModeSelectedEvent : EventArgs
{
    public SchafkopfMode Mode { get; }
    public string Color { get; }

    public GameModeSelectedEvent(SchafkopfMode mode, string color)
    {
        Mode = mode;
        Color = color;
    }
}