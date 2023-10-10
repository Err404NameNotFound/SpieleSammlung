namespace SpieleSammlung.Model.Multiplayer
{
    public class MultiplayerPlayer
    {
        public string id;
        public string name;

        public MultiplayerPlayer(string id, string name)
        {
            this.id = id;
            this.name = name;
        }
    }
}