using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpieleSammlung.Model.Connect4;

/// <summary>For a single player match of Connect 4 against a machine.</summary>
internal class Connect4 : Board
{
    /// <summary>
    /// Suggested difficulty for the Machine. The difficulty of the machine is the amount of steps the machine looks ahead for
    /// deciding its next move. The higher the value the better the machine plays.
    /// </summary>
    public const int SUGGESTED_MACHINE_LEVEL = 4;

    /// <summary>
    /// All possible directions for iterating through the field. For example for
    ///finding out how many consecutive tiles each player has in each direction
    ///or for calculating the witness of the winner.
    ///Both coordinates for the vector must be either -1, 0 or 1, so no fields
    ///are skipped while iterating through the field.
    /// </summary>
    private static readonly Coordinates2D[] Directions =
    [
        new(1, 0), //  moves right
        new(0, 1), //  moves up
        new(-1, 1), // moves up and left
        new(1, 1) //  moves up and right
    ];

    /// <summary>
    ///Index of the player used while calculating the value of the board.
    ///Must be either 0 or 1 and mustn't be the same as {@code INDEX_MACHINE}.
    /// </summary>
    private const int INDEX_PLAYER = 1;

    /// <summary>
    ///Index of the machine used while calculating the value of the board.
    ///Must be either 0 or 1 and mustn't be the same as {@code INDEX_PLAYER}.
    /// </summary>
    private const int INDEX_MACHINE = 0;

    /// <summary>Player that started the game.</summary>
    private readonly Connect4Tile _firstPlayer;

    /// <summary>Stores which slot is occupied by which player.</summary>
    private Connect4Tile[,] _board;

    /// <summary>The index of the player that has to make the next move.</summary>
    private Connect4Tile _currentPlayer;

    /// <summary>Actual difficulty level of the machine.</summary>
    private int _machineLevel;

    /// <summary>Flag if the board is full.</summary>
    private bool _allColsFull;

    /// <summary>Winner of the match.</summary>
    private Connect4Tile _winner;

    /// <summary>
    /// Witness of the winner. Is a list of coordinates that prove who won thegame.
    /// Is <c>Connect4Tile.NOBODY</c> until the winner is decided.
    /// </summary>
    private List<Coordinates2D> _witness;

    /// <summary>
    /// Stores which is the next free index in each column starting at value 0.
    /// If the column is full the value is {@code ROWS}.
    /// </summary>
    private int[] _columnFills;

    /// <summary>Stores how many tiles each player has in each column.</summary>
    private int[,] _columnCounts;

    /// <summary>
    /// Stores how many groups of tiles in a straight line each player has for each group size.
    /// Index 0 is for groups of the size 2, index 1 for size 3 and index 2 for size 4.
    /// </summary>
    private int[,] _groupCounts;

    /// <summary>Initiates a new Connect 4 game.</summary>
    /// <param name="starter">either <c>Connect4Tile.PLAYER</c> or <c>Connect4Tile.MACHINE</c></param>      
    /// <param name="machineLevel">Initial difficulty level of the machine (must be >0).</param>
    public Connect4(Connect4Tile starter, int machineLevel)
    {
        if (starter != Connect4Tile.Player && starter != Connect4Tile.Machine)
        {
            throw new ArgumentException("Invalid starter of the match.");
        }

        _firstPlayer = starter;
        _currentPlayer = _firstPlayer;
        SetLevel(machineLevel);
        _board = new Connect4Tile[ROWS, COLS];
        for (int row = 0; row < ROWS; ++row)
        {
            for (int col = 0; col < COLS; ++col)
            {
                _board[row, col] = Connect4Tile.Nobody;
            }
        }

        _columnFills = new int[COLS];
        _columnCounts = new int[COLS, 2];
        _groupCounts = new int[3, 2];
        _winner = Connect4Tile.Nobody;
    }

    /// <inheritdoc cref="Board"/>
    public sealed override void SetLevel(int level)
    {
        if (0 < level) _machineLevel = level;
        else throw new ArgumentException("Invalid difficulty level.");
    }

    /// <inheritdoc cref="Board"/>
    public override Connect4Tile GetFirstPlayer() => _firstPlayer;

    /// <inheritdoc cref="Board"/>
    public override Connect4Tile GetSlot(int row, int col)
    {
        if (!IsPointValid(row, col)) throw new ArgumentException("Invalid position.");
        return _board[row, col];
    }

    /// <inheritdoc cref="Board"/>
    public override bool IsGameOver() => _allColsFull || _winner != Connect4Tile.Nobody;

    /// <inheritdoc cref="Board"/>
    public override Connect4Tile GetWinner() => _winner;

    /// <inheritdoc cref="Board"/>
    public override List<Coordinates2D> GetWitness()
    {
        if (_witness == null) throw new NotSupportedException("There is no winner available");
        return _witness;
    }

    /// <inheritdoc cref="Board"/>
    public override Board Clone()
    {
        Connect4 ret = (Connect4)MemberwiseClone();
        ret._columnFills = (int[])_columnFills.Clone();
        ret._columnCounts = Clone(_columnCounts);
        ret._groupCounts = Clone(_groupCounts);
        ret._board = Clone(_board);
        if (_witness == null) return ret;
        ret._witness = [];
        ret._witness.AddRange(_witness);

        return ret;
    }

    private static T[,] Clone<T>(T[,] original)
    {
        T[,] ret = new T[original.GetLength(0), original.GetLength(1)];
        for (int i = 0; i < original.GetLength(0); ++i)
        {
            for (int j = 0; j < original.GetLength(1); ++j)
            {
                ret[i, j] = original[i, j];
            }
        }

        return ret;
    }

    /// <inheritdoc cref="Board"/>
    public override string ToString()
    {
        StringBuilder ret = new StringBuilder();

        for (int row = ROWS - 1; row >= 0; --row)
        {
            for (int col = 0; col < COLS; ++col)
            {
                ret.Append(_board[row, col]);
                ret.Append(" ");
            }

            UtilityStringBuilder.RemoveLastChar(ret);
            ret.Append("\n");
        }

        UtilityStringBuilder.RemoveLastChar(ret);
        return ret.ToString();
    }

    /// <summary>Checks if the given column is valid.</summary>
    /// <param name="col">Zero based index of the column.</param>
    /// <return>{@code true} if the index of the column is valid.</return>
    private static bool IsColValid(int col) => col is >= 0 and < COLS;

    private static bool IsRowValid(int row) => row is >= 0 and < ROWS;

    private void ThrowInCol(int col, bool updateWinner)
    {
        _board[_columnFills[col], col] = _currentPlayer;
        UpdateGroupCount(_columnFills[col], col, 1, updateWinner);
        ++_columnFills[col];
        ++_columnCounts[col, GetCalculationIndex(_currentPlayer)];
        SetNextPlayer();
    }

    private void SetNextPlayer() => _currentPlayer = _currentPlayer.Opponent();

    private void RemoveFromCol(int col)
    {
        SetNextPlayer(); // There are only two players. -> equal to going back.
        --_columnCounts[col, GetCalculationIndex(_currentPlayer)];
        --_columnFills[col];
        UpdateGroupCount(_columnFills[col], col, -1, false);
        _board[_columnFills[col], col] = Connect4Tile.Nobody;
    }

    private bool IsColNotFull(int col) => _columnFills[col] < ROWS;

    protected Board ExecuteMove(int col)
    {
        if (IsGameOver())
        {
            throw new IllegalMoveException("The game is over.");
        }

        if (IsColNotFull(col))
        {
            Connect4 ret = (Connect4)Clone();
            ret.ThrowInCol(col, true);
            ret.CheckIfAllColsFull();
            return ret;
        }

        return null;
    }

    /// <summary>
    /// Checks if the game is over because there is no free space left or someone won the game.
    /// If someone won, the winner and witness are set.
    /// </summary>
    private void CheckIfAllColsFull()
    {
        if (!_allColsFull)
        {
            _allColsFull = true;
            int col = 0;
            while (_allColsFull && col < COLS)
            {
                _allColsFull = !IsColNotFull(col);
                ++col;
            }
        }
    }

    /// <inheritdoc cref="Board"/>
    public override Board Move(int col)
    {
        if (_currentPlayer == Connect4Tile.Machine)
        {
            throw new IllegalMoveException("It is the machines turn.");
        }

        if (!IsColValid(col))
        {
            throw new ArgumentException("This column doesn't exist");
        }

        return ExecuteMove(col);
    }

    /// <inheritdoc cref="Board"/>
    public override Board MachineMove()
    {
        if (_currentPlayer == Connect4Tile.Player)
        {
            throw new IllegalMoveException("It is the players turn");
        }

        // don't show changing board during calculation to the outside
        int index = ((Connect4)Clone()).CalculateBestColumnForMachine();

        return ExecuteMove(index);
    }

    private int CalculateBestColumnForMachine()
    {
        int max = int.MinValue;
        int? bestColumnIndex = null;

        for (int i = 0; i < COLS; ++i)
        {
            if (!IsColNotFull(i)) continue;
            ThrowInCol(i, true);
            int value = LookAhead(_machineLevel - 1);
            RemoveFromCol(i);

            // reset witness and winner in case it was set
            _witness = null;
            _winner = Connect4Tile.Nobody;
            if (value <= max) continue;
            bestColumnIndex = i;
            max = value;
        }

        if (bestColumnIndex == null)
        {
            throw new Exception("Calculation failed.");
        }

        return bestColumnIndex.Value;
    }

    /// <summary>
    /// Calculates the given next best steps to see what the best possible column for the machine is.
    /// Tries putting a tile in every column and chooses the best option for the current "active" player
    /// (keyword minimax-algorithm).
    /// </summary>
    /// <param name="remainingSteps">Remaining depth in the lookup tree.</param>
    /// <return>Combined value of the field the method was called on and the best value of all of its children.</return>
    private int LookAhead(int remainingSteps)
    {
        if (remainingSteps == 0)
            return CalculateBoardValue(remainingSteps);
            
        int boardValue = CalculateBoardValue(remainingSteps);

        // Contains field values after a tile is thrown in a column.
        List<int> valuesNextStep = [];

        for (int i = 0; i < COLS; ++i)
        {
            if (!IsColNotFull(i)) continue;
            ThrowInCol(i, false);
            valuesNextStep.Add(LookAhead(remainingSteps - 1));
            RemoveFromCol(i);
        }

        if (valuesNextStep.Count > 0)
        {
            boardValue += _currentPlayer == Connect4Tile.Player
                ? valuesNextStep.Min()
                : valuesNextStep.Max();
        }

        return boardValue;
    }

    /// <summary>
    /// Calculates the value of the board with the Bachmeier-Algorithm.
    /// Evaluates the field by counting how many consecutive tiles and how many
    /// tiles in each column each player has. The bigger the value the better
    /// for the machine. The result depends on the current step, because a direct
    /// win is only possible for the first move of the machine.
    /// </summary>
    /// <param name="step">Remaining steps for the current machine level.</param>
    /// <return>Value of this field according to the Bachmeier-Algorithm.</return>
    private int CalculateBoardValue(int step)
    {
        int boardValue = 50;

        // Calculate P value
        boardValue += _groupCounts[0, INDEX_MACHINE] - _groupCounts[0, INDEX_PLAYER];
        boardValue += 4 * _groupCounts[1, INDEX_MACHINE] - 4 * _groupCounts[1, INDEX_PLAYER];
        boardValue += 5000 * _groupCounts[2, INDEX_MACHINE] - 500000 * _groupCounts[2, INDEX_PLAYER];

        // Calculate Q value
        boardValue += _columnCounts[1, INDEX_MACHINE] - _columnCounts[1, INDEX_PLAYER];
        boardValue += 2 * (_columnCounts[2, INDEX_MACHINE] - _columnCounts[2, INDEX_PLAYER]);
        boardValue += 3 * (_columnCounts[3, INDEX_MACHINE] - _columnCounts[3, INDEX_PLAYER]);
        boardValue += 2 * (_columnCounts[4, INDEX_MACHINE] - _columnCounts[4, INDEX_PLAYER]);
        boardValue += _columnCounts[5, INDEX_MACHINE] - _columnCounts[5, INDEX_PLAYER];

        // Calculate R value
        if (step != _machineLevel - 1) return boardValue;
        if (GetWinner() == Connect4Tile.Machine) boardValue += 5000000;

        return boardValue;
    }

    private static int GetCalculationIndex(Connect4Tile field) =>
        field == Connect4Tile.Machine ? INDEX_MACHINE : INDEX_PLAYER;

    /// <summary>
    /// Updates the member {@code groupCounts} after a tile was thrown in the
    /// board at the given position. Should also be called, when a tile is about
    /// to be removed from the board.
    /// </summary>
    ///<param name="row">Row of the new/old tile</param> 
    ///<param name="col">Column of the new/old tile</param>
    ///<param name="dif">Direction for counting. -1 for removed tile, 1 for added tile</param>
    ///<param name="update">Flag if the winner and witness should be set if encountered</param>
    private void UpdateGroupCount(int row, int col, int dif, bool update)
    {
        /*
         * Look in both ways for each direction and merge/split the groups for
         * the new tile that was just added/removed to/from the field.
         */
        foreach (Coordinates2D d in Directions)
        {
            int index = GetCalculationIndex(_board[row, col]);
            int sumUp = CheckDirection(row, col, d.Y, d.X, index, dif);
            int sumDown = CheckDirection(row, col, -d.Y, -d.X, index, dif);
            int sum = sumUp + sumDown + 1;

            if (sum >= CONNECT)
            {
                if (update)
                    SetWitness(row, col, d);

                sum -= CONNECT;
                _groupCounts[CONNECT - 2, index] += dif;
            }

            if (sum > 1)
                _groupCounts[sum - 2, index] += dif;
        }
    }

    private int CheckDirection(int row, int col, int vectorY, int vectorX, int index, int countingDirection)
    {
        int sum = 0;
        Connect4Tile start = _board[row, col];

        row += vectorY;
        col += vectorX;
        while (IsPointValid(row, col) && start == _board[row, col])
        {
            ++sum;
            row += vectorY;
            col += vectorX;
        }

        if (sum > CONNECT)
        {
            // group is big enough for connect + at least 2=1+1 (new tile).
            sum -= CONNECT;
        }

        if (sum > 1)
        {
            // group was at least two but is now bigger. -> reduce count.
            _groupCounts[sum - 2, index] -= countingDirection;
        }

        return sum;
    }

    private static bool IsPointValid(int row, int col) => IsRowValid(row) && IsColValid(col);

    private void SetWitness(int row, int col, Coordinates2D direction)
    {
        int y = row - direction.Y;
        int x = col - direction.X;

        _winner = _board[row, col];
        _witness = [];

        // go "back" to 1 before first tile of witness. -> witness is sorted.
        while (IsPointValid(y, x) && _board[y, x] == _board[row, col])
        {
            y -= direction.Y;
            x -= direction.X;
        }

        while (_witness.Count < CONNECT)
        {
            y += direction.Y;
            x += direction.X;
            _witness.Add(new Coordinates2D(x, y));
        }
    }
}