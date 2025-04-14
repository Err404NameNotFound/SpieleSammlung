using System.Collections.Generic;
using TestStack.White;
using TestStack.White.Factory;
using TestStack.White.UIItems;
using TestStack.White.UIItems.WindowItems;

namespace SpieleSammlungTests.View;

public class Util
{
    public Application App { get; } = Application.Launch("SpieleSammlung.exe");
    public Window Window => App.GetWindow("Spiele Sammlung", InitializeOption.NoCache);

    ~Util() => App.Close();

    public void AddPlayer(string name)
    {
        var textBox = Window.Get<TextBox>("PcTxtBoxPlayerName");
        textBox.Text = name;
        var btn = Window.Get<Button>("PcBtnAddPlayer");
        btn.Click();
    }

    public void AddPlayers(IEnumerable<string> players)
    {
        foreach (var name in players)
          AddPlayer(name);
    }

    public void LeavePlayerCreator() => Window.Get<Button>("PcBtnPlayerDone").Click();
}