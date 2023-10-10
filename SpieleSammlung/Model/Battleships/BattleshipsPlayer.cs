using System.Collections.Generic;
using SpieleSammlung.UserControls.BattleShips;

namespace SpieleSammlung.Model.Battleships
{
    public class BattleshipsPlayer : Player
    {
        public readonly BoatField[,] field;
        public List<Boat> boats;
        public Coordinate selectedField;

        public BattleshipsPlayer(Player player) : base(player.Name, player.IsBot)
        {
            selectedField = new Coordinate(-1, -1);
            field = new BoatField[10, 10];
            for (int i = 0; i < 10; ++i)
            {
                for (int i1 = 0; i1 < 10; ++i1)
                {
                    field[i, i1] = new BoatField();
                }
            }
        }
    }
}