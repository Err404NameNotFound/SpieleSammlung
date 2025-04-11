using System;

namespace SpieleSammlung.Model.Connect4;

/// <summary>
/// All possible states for a classic "Connect Four" field. Field can be empty or occupied
/// by either the player or the machine.
/// </summary>
public enum Connect4Tile
{
    /// <summary>Value for a field that is not occupied.</summary>
    Nobody,

    /// <summary>Value for a field that is occupied by the player.</summary>
    Player,

    /// <summary>Value for a field that is occupied by the machine.</summary>
    Machine
}

/// <summary>Extensions for class Connect4Tile</summary>
public static class Extension
{
    /// <summary>
    /// For retrieving the opponent of a player. Can only be called from MACHINE and PLAYER.
    /// NOBODY does not have an opponent.
    /// </summary>
    /// <returns>opponent of the Player</returns>
    public static Connect4Tile Opponent(this Connect4Tile player)
    {
        return player switch
        {
            Connect4Tile.Player => Connect4Tile.Machine,
            Connect4Tile.Machine => Connect4Tile.Player,
            _ => throw new NotSupportedException("There is no opponent for NOBODY.")
        };
    }

    public static string ToString(this Connect4Tile player)
    {
        return player switch
        {
            Connect4Tile.Nobody => ".",
            Connect4Tile.Player => "X",
            Connect4Tile.Machine => "O",
            _ => throw new Exception()
        };
    }
}