namespace SpieleSammlung.Model.Battleships;

public class Coordinate(int x, int y)
{
    public readonly int X = x;
    public readonly int Y = y;
    private const int BATTLE_SHIPS_MAX_X = 9;
    private const int BATTLE_SHIPS_MAX_Y = 9;
    private const int CONNECT4_MAX_X = 7;
    private const int CONNECT4_MAX_Y = 6;

    public static bool InsideFieldBattleShips(int x, int y)
    {
        return InsideField(x, y, BATTLE_SHIPS_MAX_X, BATTLE_SHIPS_MAX_Y);
    }

    public bool InsideFieldBattleShips()
    {
        return InsideField(X, Y, BATTLE_SHIPS_MAX_X, BATTLE_SHIPS_MAX_Y);
    }

    public static bool InsideFieldConnec4(int x, int y)
    {
        return InsideField(x, y, CONNECT4_MAX_X, CONNECT4_MAX_Y);
    }

    public bool InsideFieldConnect()
    {
        return InsideField(X, Y, CONNECT4_MAX_X, CONNECT4_MAX_Y);
    }

    private static bool InsideField(int x, int y, int maxX, int maxY)
    {
        return x <= maxX && x >= 0 && y <= maxY && y >= 0;
    }
}