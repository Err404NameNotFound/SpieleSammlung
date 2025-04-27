namespace SpieleSammlung.Model.Multiplayer;

/// <summary>
/// All types of events in a multiplayer match
/// </summary>
public enum MultiplayerEventTypes
{
    CClientCantJoin,
    HClientConnected,
    HClientDisconnected,
    HClientReConnected,
    CDuplicateNames,
    HostReceived,
    ClientReceived,
    CClientConnected,
    CClientDisconnected
}