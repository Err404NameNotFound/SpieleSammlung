#nullable enable
using System.Linq;
using Castle.Core.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpieleSammlung.Model.Connect4;
using TestStack.White.UIItems;
using TestStack.White.UIItems.Finders;
using Assert = NUnit.Framework.Assert;
using Button = TestStack.White.UIItems.Button;
using Label = TestStack.White.UIItems.Label;

namespace SpieleSammlungTests.View.Windows;

[TestClass]
public class Connect4Test
{
    private const string PLAYER_NAME = "somePlayerName47230";
    private const string BOT_NAME = "bot";
    private IUIItem[,] _fields = new IUIItem[0,0];

    private static Util SetUpMatch(string player1, string? player2 = null)
    {
        Util util = new();
        var window = util.Window;

        window.Get<Button>("BtnChooseConnect4").Click();
        util.AddPlayer(player1);
        if (player2 != null)
            util.AddPlayer(player2);

        util.LeavePlayerCreator();
        return util;
    }

    private static IUIItem[,] FindUiFields(Util util)
    {
        IUIItem[]? fields = util.Window.GetMultiple(SearchCriteria.All)
            .FindAll(c => c.PrimaryIdentification == "Connect4Field");
        IOrderedEnumerable<IUIItem> sortedFields = fields.OrderBy(f => f.Bounds.Bottom).ThenByDescending(f => f.Bounds.Left);
        IUIItem[,] ret = new IUIItem[Board.ROWS, Board.COLS];
        int r = 0, c = 0;
        foreach (var c4F in sortedFields)
        {
            if (c == ret.GetLength(r))
            {
                c = 0;
                ++r;
            }
            ret[r, c] = c4F;
        }
        
        return ret;
    }

    [TestMethod]
    public void TestPlayerLabelsHaveCorrectValuesSinglePlayer()
    {
        Util util = SetUpMatch(PLAYER_NAME);

        var playerNameLeft = util.Window.Get<Label>("C4SLblPlayerLeft");
        Assert.AreEqual(PLAYER_NAME, playerNameLeft.Text);

        var playerNameRight = util.Window.Get<Label>("C4SLblPlayerRight");
        Assert.AreEqual(BOT_NAME, playerNameRight.Text);
    }

    [TestMethod]
    public void TestPlayerLabelsHaveCorrectValuesMultiplayer()
    {
        const string secondPlayer = "completelyDifferentString";
        Util util = SetUpMatch(PLAYER_NAME, secondPlayer);

        var playerNameLeft = util.Window.Get<Label>("C4SLblPlayerLeft");
        Assert.AreEqual(PLAYER_NAME, playerNameLeft.Text);

        var playerNameRight = util.Window.Get<Label>("C4SLblPlayerRight");
        Assert.AreEqual(secondPlayer, playerNameRight.Text);
    }

    [TestMethod]
    public void TestRestart()
    {
        Util util = SetUpMatch(PLAYER_NAME);
        KlickField(util, 0, 0);
    }

    private void KlickField(Util util, int row, int col)
    {
        if (_fields.IsNullOrEmpty()) 
            _fields = FindUiFields(util);
        var field = _fields[row,col];
        field.Click();
    }
}