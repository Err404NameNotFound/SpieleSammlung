using System.Collections.Generic;
using SpieleSammlung.View.UserControls.BattleShips;

namespace SpieleSammlung.Model.Battleships
{
    public class BattleshipsPlayer : Player
    {
        public readonly BoatField[,] Field;
        public List<Boat> Boats;
        public Coordinate SelectedField;

        public BattleshipsPlayer(Player player) : base(player.Name, player.IsBot)
        {
            SelectedField = new Coordinate(-1, -1);
            Field = new BoatField[10, 10];
            for (int i = 0; i < 10; ++i)
            {
                for (int i1 = 0; i1 < 10; ++i1)
                {
                    Field[i, i1] = new BoatField();
                }
            }
        }
    }
}