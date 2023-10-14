namespace SpieleSammlung.Model.Schafkopf
{
    public class PointsStorage
    {
        public string Name { get; }
        public int points;

        public PointsStorage(string name, int points)
        {
            this.points = points;
            Name = name;
        }
    }
}