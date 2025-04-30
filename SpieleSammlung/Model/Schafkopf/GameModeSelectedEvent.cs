#region

using System;

#endregion

namespace SpieleSammlung.Model.Schafkopf;

public class GameModeSelectedEvent(SchafkopfMode mode, string color) : EventArgs
{
    public SchafkopfMode Mode { get; } = mode;
    public string Color { get; } = color;
}