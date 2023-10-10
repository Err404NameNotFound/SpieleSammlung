using System;

namespace SpieleSammlung.Model.Multiplayer
{
    public class MultiplayerEvent : EventArgs
    {
        public MultiplayerEvent(MultiplayerEventTypes type, string sender = null, string message = null)
        {
            Type = type;
            Message = message;
            Sender = sender;
        }

        public MultiplayerEventTypes Type { get; }

        public string Message { get; }

        public string Sender { get; }
    }
}