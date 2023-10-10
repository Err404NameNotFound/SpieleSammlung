namespace SpieleSammlung.Model.Connect4
{
    /// <summary>A Connect 4 match with to players.</summary>
    internal class Connect4Multiplayer : Connect4
    {
        /// <inheritdoc cref="Connect4"/>
        public Connect4Multiplayer(Connect4Tile starter, int level) : base(starter, level)
        {
        }

        /// <summary>Move a tile into the given column for the second player</summary>
        /// <param name="col">Column to throw the tile into.</param>
        /// <returns>A clone of this instance with the move executed.</returns>
        public Board MachineMove(int col) => ExecuteMove(col);
    }
}