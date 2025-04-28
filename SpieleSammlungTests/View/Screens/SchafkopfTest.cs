using System.Collections.Generic;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestStack.White.UIItems;

namespace SpieleSammlungTests.View.Screens;

[TestClass]
public class SchafkopfTest
{
    private static List<Util> SetUpMatch(string hostName, string hostPort, IEnumerable<string> players)
    {
        Util util = new();
        List<Util> utilList = [util];
        util.Window.Get<Button>("BtnChooseSchafkopf").Click();
        util.HostMatch(hostName, hostPort);
        foreach (var player in players)
        {
            Util tmp = new();
            tmp.Window.Get<Button>("BtnChooseSchafkopf").Click();
            tmp.JoinMatch(player);
            utilList.Add(tmp);
        }
        return utilList;
    }

    [TestMethod]
    public void TestCanStartMatch()
    {
        List<Util> games = SetUpMatch("Hans", "54321", ["Max", "Josef", "Peter"]);
        games[0].Window.Get<Button>("MpBtnStartMatch").Click();
        Thread.Sleep(20000);
        games.ForEach(u => u.Close());
    }
}