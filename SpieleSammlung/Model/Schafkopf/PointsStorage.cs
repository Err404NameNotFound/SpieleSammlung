namespace SpieleSammlung.Model.Schafkopf
{
    public class PointsStorage
    {
        public readonly string name;
        public int points;

        public PointsStorage(string name, int points)
        {
            this.points = points;
            this.name = name;
        }
    }
}