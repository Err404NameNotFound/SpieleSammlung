namespace SpieleSammlung.Model.Mancala;

public class MancalaBot
{
    public int Level { get; set; }

    public MancalaBot(int level = 6)
    {
        Level = level;
    }

    public int CalculateIndexOfBestOption(MancalaGame game)
    {
        if (game.OptionsOfCurrentPlayer.Count == 0 || game.IsGameOver)
            return -1;
        if (game.OptionsOfCurrentPlayer.Count == 1)
            return 0;
        int playerIndex, opponentIndex;
        if (game.CurrentIsFirst)
        {
            playerIndex = game.Player1Index;
            opponentIndex = game.Player2Index;
        }
        else
        {
            playerIndex = game.Player2Index;
            opponentIndex = game.Player1Index;
        }

        int bestIndex = -1, bestValue = int.MinValue;
        for (int i = 0; i < game.OptionsOfCurrentPlayer.Count; ++i)
        {
            int next = ValueOfRecursiveStep(game, Level, playerIndex, opponentIndex, i);
            if (next > bestValue)
            {
                bestIndex = i;
                bestValue = next;
            }
        }

        return bestIndex;
    }

    private int RecursiveCalculation(MancalaGame game, int level, int playerIndex, int opponentIndex)
    {
        if (level == 0 || game.IsGameOver)
            return ValueOfGame(game, playerIndex, opponentIndex);
        bool maximise = game.CurrentPlayer == playerIndex;
        int bestValue = maximise ? int.MinValue : int.MaxValue;
        for (int i = 0; i < game.OptionsOfCurrentPlayer.Count; ++i)
        {
            int next = ValueOfRecursiveStep(game, level, playerIndex, opponentIndex, i);
            if (maximise && next > bestValue || !maximise && next < bestValue)
                bestValue = next;
        }

        return bestValue;
    }
        
    private int ValueOfRecursiveStep(MancalaGame game, int level, int playerIndex, int opponentIndex, int index)
    {
        MancalaGame clone = new MancalaGame(game, false);
        clone.DoMove(clone.OptionsOfCurrentPlayer[index]);
        int nextLevel = clone.CurrentPlayer == game.CurrentPlayer ? level : level - 1;
        return RecursiveCalculation(clone, nextLevel, playerIndex, opponentIndex);
    }

    private static int ValueOfGame(MancalaGame game, int playerIndex, int opponentIndex)
    {
        return game[playerIndex] - game[opponentIndex];
    }
}