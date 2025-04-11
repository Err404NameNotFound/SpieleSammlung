namespace SpieleSammlung.Model.Multiplayer;

public class MultiplayerPlayer
{
    public string Id { get; set; }
    public string Name { get; set; }

    public MultiplayerPlayer(string id, string name)
    {
        Id = id;
        Name = name;
    }
}