using SpieleSammlung.Model.Kniffel.Fields;

namespace SpieleSammlung.Model.Kniffel
{
    /// <summary>Player for a Kniffel match.</summary>
    public class KniffelPlayer : Player
    {
        /// <inheritdoc cref="Fields"/>
        public KniffelField this[int index] => Fields[index];

        /// <summary>Fields of the player. Initially all fields are empty.</summary>
        public KniffelPointsTable Fields { get; }

        /// <summary>
        /// Initiates a new player for a Kniffel match.
        /// </summary>
        /// <param name="player">A Player of any game which is converted into a Kniffelplayer</param>
        public KniffelPlayer(Player player) : base(player.Name, player.IsBot) => Fields = new KniffelPointsTable();

        /// <summary>
        /// Indicates if the player has enough points in the fields on top to receive the bonus
        /// </summary>
        /// <returns><c>true</c> if the Player has at least <c>MIN_TOP6_FOR_BONUS</c> points in the top fields</returns>
        public bool HasReachedBonus() =>
            Fields[KniffelPointsTable.INDEX_SUM_TOP].Value >= KniffelPointsTable.MIN_TOP6_FOR_BONUS;
    }
}