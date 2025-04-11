using System;
using System.Collections.Generic;
using SpieleSammlung.View.UserControls.BattleShips;

namespace SpieleSammlung.Model.Battleships;

public class Boat
{
    public readonly int Width;
    public readonly List<Coordinate> Coordinates;
    public int Lives;
    public bool Dead;
    public readonly bool Horizontal;

    private Boat(int width, Coordinate startPoint, bool horizont)
    {
        Width = width;
        Lives = Width;
        Coordinates = [startPoint];
        for (int i = 1; i < Width; ++i)
        {
            Coordinates.Add(Horizontal
                ? new Coordinate(startPoint.X + i, startPoint.Y)
                : new Coordinate(startPoint.X, startPoint.Y + i));
        }

        Dead = false;
        Horizontal = horizont;
    }

    public static bool CheckIfPositionIsPossible(BattleshipsPlayer player, Coordinate startPoint, int width,
        bool horizontal)
    {
        if (!startPoint.InsideFieldBattleShips()) return false; //liegt außerhab vom Feld
        var endPoint = horizontal
            ? new Coordinate(startPoint.X + width - 1, startPoint.Y)
            : new Coordinate(startPoint.X, startPoint.Y + width - 1);

        if (!endPoint.InsideFieldBattleShips())
            return false; //Startpunkt liegt im Feld aber Endpunkt liegt außerhalb
        bool possible = true;
        //Überprüfung ob Schiff an der Stelle platziert werden kann
        int i = startPoint.X - 1;
        while (i <= endPoint.X + 1 && possible)
        {
            int i1 = startPoint.Y;
            while (i1 <= endPoint.X + 1 && possible)
            {
                if (i < 0 || i > 9 || i1 < 0 || i1 > 9)
                {
                    //Werte die Außerhalb des Feldes liegen müssen nicht überprüft werden
                }
                else if (player.Field[i, i1].IsBoat())
                {
                    possible = false; //es wurde bereits ein Schiff an der Stelle platziert
                }

                ++i1;
            }

            ++i;
        }

        return possible;

    }

    public Boat CreateBoatIfPossible(BattleshipsPlayer player, Coordinate startPoint, int width, bool horizontal)
    {
        if (!CheckIfPositionIsPossible(player, startPoint, width, horizontal)) return null;
        Boat temp = new Boat(width, startPoint, horizontal);

        //Schiffe platzieren
        int counter = 0;
        for (int i = temp.Coordinates[0].X; i <= temp.Coordinates[temp.Coordinates.Count - 1].X; i++)
        {
            for (int i1 = temp.Coordinates[0].Y; i1 <= temp.Coordinates[temp.Coordinates.Count - 1].Y; i1++)
            {
                if (!player.Field[i, i1].IsBoat())
                {
                    player.Field[i, i1] =
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
}