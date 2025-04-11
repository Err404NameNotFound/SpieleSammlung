namespace SpieleSammlung;

/// <summary>
/// All playable games for this UI
/// </summary>
public enum GameMode
{
    Zufallszahlen,
    Lotto,
    SchiffeVersenken,
    Maexchen,
    VierGewinnt,
    Kniffel,
    Schafkopf,
    Mancala
}

public enum MainWindowView
{
    StartScreen,
    PlayerCreator,
    MultiplayerLobby,
    SchiffeVersenken,
    VierGewinnt,
    Kniffel,
    Schafkopf,
    Mancala
}

/// <summary>
/// State of a battle ship game.
/// </summary>
public enum BattleshipsMode
{
    PlaceShips,
    Shoot,
    DoNothing
}

/// <summary>
/// All possible game modes for a Schafkopf match.
/// </summary>
public enum SchafkopfMode
{
    Weiter,
    Sauspiel,
    Wenz,
    Solo,
    WenzTout,
    SoloTout
}

/// <summary>
/// States of a GameSelector in a Schafkopf match.
/// </summary>
public enum GameSelectorState
{
    Visible,
    Selected,
    Hidden
}

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

/// <summary>
/// States for a multiplayer connection.
/// </summary>
public enum MultiplayerPlayerState
{
    Active,
    Inactive,
    LeftMatch
}