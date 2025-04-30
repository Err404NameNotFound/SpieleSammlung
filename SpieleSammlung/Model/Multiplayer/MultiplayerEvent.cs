#region

using System;

#endregion

namespace SpieleSammlung.Model.Multiplayer;

public class MultiplayerEvent(MultiplayerEventTypes type, string sender = null, string message = null)
    : EventArgs
{
    public MultiplayerEventTypes Type { get; } = type;

    public string Message { get; } = message;

    public string Sender { get; } = sender;
}