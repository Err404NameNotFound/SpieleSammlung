namespace SpieleSammlung.Model.Schafkopf
{
    public class PointsStorage(string name, int points)
    {
        public string Name { get; } = name;
        public int Points = points;
    }
}