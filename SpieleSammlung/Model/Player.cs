﻿#region

using System;

#endregion

namespace SpieleSammlung.Model;

/// <summary>
/// Player in a match.
/// </summary>
public class Player
{
    private const string BOT_INDICATOR = "[Bot]";

    private static readonly string[] BotNames = ["Bob", "Hans", "Otto", "Heinz", "Franz", "Josef"];

    private static readonly Random Rng = new();

    /// <summary>
    /// Creates a new instance and sets its values.
    /// </summary>
    /// <param name="name">Name of the player.</param>
    /// <param name="isBot">Flag if the player is a bot.</param>
    public Player(string name, bool isBot) => (IsBot, Name) = (isBot, name);

    public Player() => (IsBot, Name) = (true, GenerateRandomBotName());

    /// <summary>
    /// Name of the Player.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Flag if the player is a bot.
    /// </summary>
    public bool IsBot { private set; get; }

    private static string GenerateRandomBotName() => BOT_INDICATOR + " " + BotNames[Rng.Next(0, BotNames.Length)];

    public override string ToString() => $"{Name}";
}