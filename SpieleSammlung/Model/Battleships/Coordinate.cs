namespace SpieleSammlung.Model.Battleships
{
    public class Coordinate
    {
        public readonly int x;
        public readonly int y;
        public const int BATTLE_SHIPS_MAX_X = 9;
        public const int BATTLE_SHIPS_MAX_Y = 9;
        public const int CONNECT4_MAX_X = 7;
        public const int CONNECT4_MAX_Y = 6;

        public Coordinate(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public static bool InsideFieldBattleShips(int x, int y)
        {
            return InsideField(x, y, BATTLE_SHIPS_MAX_X, BATTLE_SHIPS_MAX_Y);
        }

        public bool InsideFieldBattleShips()
        {
            return InsideField(x, y, BATTLE_SHIPS_MAX_X, BATTLE_SHIPS_MAX_Y);
        }

        public static bool InsideFieldConnec4(int x, int y)
        {
            return InsideField(x, y, CONNECT4_MAX_X, CONNECT4_MAX_Y);
        }

        public bool InsideFieldConnect()
        {
            return InsideField(x, y, CONNECT4_MAX_X, CONNECT4_MAX_Y);
        }

        public static bool InsideField(int x, int y, int maxX, int maxY)
        {
            return x <= maxX && x >= 0 && y <= maxY && y >= 0;
        }
    }
}