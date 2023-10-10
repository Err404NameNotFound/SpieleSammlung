using System;

namespace SpieleSammlung.Model
{
    /// <summary>
    /// This Exception signalises that a move tried to be executed on a game is against the rules of the specific game.
    /// One possibe Scenario is rolling the dice again, although the current player has no shuffles left.
    /// </summary>
    public class IllegalMoveException : Exception
    {
        /// <summary>Initialises a new Instance.</summary>
        public IllegalMoveException()
        {
        }

        /// <summary>Initialises a new Instance and sets the execption message.</summary>
        public IllegalMoveException(string message) : base(message)
        {
        }

        /// <summary>Initialises a new Instance and sets the execption message and the cause of the exception.</summary>
        public IllegalMoveException(string message, Exception exception) : base(message, exception)
        {
        }
    }
}