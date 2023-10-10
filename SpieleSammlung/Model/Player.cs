using System;

namespace SpieleSammlung.Model
{
    /// <summary>
    /// Player in a match.
    /// </summary>
    public class Player
    {
        /// <summary>
        /// Name of the Player.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Flag if the player is a bot.
        /// </summary>
        public bool IsBot { private set; get; }

        /// <summary>
        /// Creates a new instance and sets its values.
        /// </summary>
        /// <param name="name">Name of the player.</param>
        /// <param name="isBot">Flag if the player is a bot.</param>
        public Player(string name, bool isBot) => (IsBot, Name) = (isBot, name);

        public Player() => (IsBot, Name) = (true, GenerateRandomBotName());

        private const string botIndicator = "[Bot]";

        private static readonly string[] BotNames = { "Bob", "Hans", "Otto", "Heinz", "Franz", "Josef" };

        private static readonly Random Rng = new();
        private static string GenerateRandomBotName() => botIndicator + " " + BotNames[Rng.Next(0, BotNames.Length)];

        public override string ToString() => $"{Name}";
    }
}