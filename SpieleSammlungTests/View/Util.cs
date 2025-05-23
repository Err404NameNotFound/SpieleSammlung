#nullable enable

#region

using System.Collections.Generic;
using TestStack.White;
using TestStack.White.Factory;
using TestStack.White.UIItems;
using TestStack.White.UIItems.WindowItems;

#endregion

namespace SpieleSammlungTests.View;

public class Util
{
    private bool _notClosed = true;
    private Application App { get; } = Application.Launch("SpieleSammlung.exe");
    public Window Window => App.GetWindow("Spiele Sammlung", InitializeOption.NoCache);

    public bool KeepAlive { get; set; }

    ~Util() => Close();

    public void Close()
    {
        if (!_notClosed || KeepAlive) return;
        App.Close();
        _notClosed = false;
    }

    public void AddPlayer(string name)
    {
        var textBox = Window.Get<TextBox>("PcTxtBoxPlayerName");
        textBox.Text = name;
        var btn = Window.Get<Button>("PcBtnAddPlayer");
        btn.Click();
    }

    public void HostMatch(string name, string port)
    {
        TextBox playerName = Window.Get<TextBox>("MpTxtBoxPlayerName");
        playerName.Text = name;
        TextBox portNumber = Window.Get<TextBox>("MpTxtBoxHostPort");
        portNumber.Text = port;
        Button btn = Window.Get<Button>("MpBtnHost");
        btn.Click();
    }

    public void JoinMatch(string name, string ip = "127.0.0.1", string port = "54321")
    {
        TextBox playerName = Window.Get<TextBox>("MpTxtBoxPlayerName");
        playerName.Text = name;
        Button btn = Window.Get<Button>("MpBtnJoin");
        btn.Click();
        TextBox ipBox = Window.Get<TextBox>("MpTxtBoxIp");
        ipBox.Text = ip;
        TextBox portNumber = Window.Get<TextBox>("MpTxtBoxPort");
        portNumber.Text = port;
        btn = Window.Get<Button>("MpBtnTryJoin");
        btn.Click();
    }

    public void AddPlayers(IEnumerable<string> players)
    {
        foreach (var name in players)
            AddPlayer(name);
    }

    public void LeavePlayerCreator() => Window.Get<Button>("PcBtnPlayerDone").Click();
}