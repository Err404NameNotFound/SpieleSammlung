using System;

namespace SpieleSammlung.Model.Connect4
{
    /// <summary>
    /// Should be thrown when someone tries to make a move in a {@code Board} object,
    /// although it is not his turn. Player tries to make a move when it is the
    /// machines turn or vise versa. The other possible cause is when a move is about
    /// to be executed on a board with a finished game.
    /// </summary>
    public class IllegalMoveException : Exception
    {
        /// <summary>Initiates a new instance.</summary>
        public IllegalMoveException()
        {
        }

        /// <summary>Initiates a new instance and sets its message.</summary>
        /// <param name="message">Message of the new Instance</param>
        public IllegalMoveException(string message) : base(message)
        {
        }
    }
}