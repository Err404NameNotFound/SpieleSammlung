namespace SpieleSammlung.Model.Multiplayer;

public class MultiplayerPlayer(string id, string name)
{
    public string Id { get; set; } = id;
    public string Name { get; set; } = name;
}