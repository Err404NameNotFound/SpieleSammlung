namespace SpieleSammlung.Model.Schafkopf;

public class PointsStorage(string name, int points)
{
    public int Points = points;
    public string Name { get; } = name;
}