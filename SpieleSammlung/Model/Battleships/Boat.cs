using System;
using System.Collections.Generic;
using SpieleSammlung.UserControls.BattleShips;

namespace SpieleSammlung.Model.Battleships
{
    public class Boat
    {
        public readonly int width;
        public readonly List<Coordinate> coordinates;
        public int lives;
        public bool dead;
        public readonly bool horizontal;

        private Boat(int width, Coordinate startPoint, bool horizont)
        {
            this.width = width;
            lives = this.width;
            coordinates = new List<Coordinate> { startPoint };
            for (int i = 1; i < this.width; ++i)
            {
                coordinates.Add(horizontal
                    ? new Coordinate(startPoint.x + i, startPoint.y)
                    : new Coordinate(startPoint.x, startPoint.y + i));
            }

            dead = false;
            horizontal = horizont;
        }

        public static bool CheckIfPositionIsPossible(BattleshipsPlayer player, Coordinate startPoint, int width,
            bool horizontal)
        {
            if (startPoint.InsideFieldBattleShips())
            {
                var endPoint = horizontal
                    ? new Coordinate(startPoint.x + width - 1, startPoint.y)
                    : new Coordinate(startPoint.x, startPoint.y + width - 1);

                if (endPoint.InsideFieldBattleShips())
                {
                    bool possible = true;
                    //Überprüfung ob Schiff an der Stelle platziert werden kann
                    int i = startPoint.x - 1;
                    while (i <= endPoint.x + 1 && possible)
                    {
                        int i1 = startPoint.y;
                        while (i1 <= endPoint.x + 1 && possible)
                        {
                            if (i < 0 || i > 9 || i1 < 0 || i1 > 9)
                            {
                                //Werte die Außerhalb des Feldes liegen müssen nicht überprüft werden
                            }
                            else if (player.field[i, i1].IsBoat())
                            {
                                possible = false; //es wurde bereits ein Schiff an der Stelle platziert
                            }

                            ++i1;
                        }

                        ++i;
                    }

                    return possible;
                }

                return false; //Startpunkt liegt im Feld aber Endpunkt liegt außerhalb
            }

            return false; //liegt außerhab vom Feld
        }

        public Boat CreateBoatIfPossible(BattleshipsPlayer player, Coordinate startPoint, int width, bool horizontal)
        {
            if (CheckIfPositionIsPossible(player, startPoint, width, horizontal))
            {
                Boat temp = new Boat(width, startPoint, horizontal);

                //Schiffe platzieren
                int counter = 0;
                for (int i = temp.coordinates[0].x; i <= temp.coordinates[temp.coordinates.Count - 1].x; i++)
                {
                    for (int i1 = temp.coordinates[0].y; i1 <= temp.coordinates[temp.coordinates.Count - 1].y; i1++)
                    {
                        if (!player.field[i, i1].IsBoat())
                        {
                            player.field[i, i1] =
                                new BoatField(temp, counter++); //es ist noch kein schiff da also kann platziert werden
                        }
                        else
                        {
                            throw new Exception("Trying to place although it is not possible");
                        }
                    }
                }

                return temp;
            }

            return null;
        }
    }
}