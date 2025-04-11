using System.Collections.Generic;

namespace SpieleSammlung.Model.Connect4;

/// <summary>
/// The Connect Four game originally published by Milton Bradley (MB) in 1974.
/// The game is also known as Four in a Row, Four in a Line, Lineup Four, Four
/// Wins, or Captain's Mistress.
/// 
/// A human plays against the machine.
/// </summary>
public abstract class Board
{
    /// <summary>The number of rows of the game grid. Originally 6.</summary>
    public const int ROWS = 6;

    /// <summary>The number of columns of the game grid. Originally 7.</summary>
    public const int COLS = 7;

    /// <summary>The number of how many tiles must be lined up to win. Originally 4.</summary>
    protected const int CONNECT = 4;

    /// <summary>Gets the player who should start or already has started the game.</summary>
    /// <returns>The player who makes the initial move.</returns>
    public abstract Connect4Tile GetFirstPlayer();

    /// <summary>
    /// Executes a human move. This method does not change the state of this
    /// instance, which is treated here as immutable. Instead, a new board/game
    /// is returned, which is a copy of {@code this} with the move executed.
    /// </summary>
    /// <param name="col">The column where to put the tile of the human.</param>
    /// <returns>A new board with the move executed. If the move is not valid,
    /// i.e., {@code col} was full before, then {@code null} will be returned.
    /// </returns>
    public abstract Board Move(int col);

    /// <summary>
    /// Executes a machine move. This method does not change the state of this
    /// instance, which is treated here as immutable. Instead, a new board/game
    /// is returned, which is a copy of {@code this} with the move executed.
    /// </summary>
    /// <returns>A new board with the move executed.</returns>
    public abstract Board MachineMove();

    /// <summary>Sets the skill level of the machine.</summary>
    /// <param name="level">The skill as number, must be at least 1.</param>
    public abstract void SetLevel(int level);

    /// <summary>
    /// Checks if game is over. Either one player has won or there is a tie and
    /// all slots are filled with tiles.
    /// </summary>
    /// <return>{@code true} if and only if the game is over.</return> 
    public abstract bool IsGameOver();

    /// <summary>Checks if the game state is won.</summary>
    /// <return>The winner or {@code null} in case of a tie or if the game is not finished yet.</return>
    public abstract Connect4Tile GetWinner();

    /// <summary>
    /// Gets the coordinates of the {@code CONNECT} tiles which are in a line,
    /// i.e., a witness of victory. The left lower corner has the smallest
    /// coordinates. Should only be called if {@link #getWinner()} returns a
    /// value unequal {@code null}. Coordinates are 2-tuples of rows x columns.
    /// 
    /// The result may not be unique!
    /// </summary>
    /// <return>The list of coordinates.</return>
    public abstract List<Coordinates2D> GetWitness();

    /// <summary>
    /// Gets the content of the slot at the specified coordinates. Either it
    /// contains a tile of one of the two players already or it is empty.
    /// </summary>
    /// <param name="row">The row of the slot in the game grid.</param>
    /// <param name="col">The column of the slot in the game grid.</param>
    /// <return>The slot's content.</return>
    public abstract Connect4Tile GetSlot(int row, int col);

    /// <summary>Creates and returns a deep copy of this board.</summary>
    /// <return>A clone.</return>
    public abstract Board Clone();

    /// <summary>
    /// Gets the string representation of this board as row x column matrix. Each
    /// slot is represented by one the three chars '.', 'X', or 'O'. '.' means
    /// that the slot currently contains no tile. 'X' means that it contains a
    /// tile of the human player. 'O' means that it contains a machine tile. In
    /// contrast to the rows, the columns are whitespace separated.
    /// </summary>
    /// <return>The string representation of the current Connect Four game.</return>
    public abstract override string ToString();
}