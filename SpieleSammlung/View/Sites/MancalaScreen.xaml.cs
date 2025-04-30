using System.Collections.Generic;
using System.Threading;
using System.Windows.Controls;
using SpieleSammlung.Model.Mancala;
using SpieleSammlung.View.UserControls.Mancala;

namespace SpieleSammlung.View.Sites;

public partial class MancalaScreen
{
    private readonly MancalaGame _mancala;
    private readonly MancalaField[] _fields;

    public MancalaScreen(bool isCapture = true, int stonesPerField = 4, int length = 6)
    {
        InitializeComponent();
        _mancala = new MancalaGame(ShowMove, ShowSteal, isCapture, stonesPerField, length);
        _fields = new MancalaField[_mancala.FieldsCount];
        for (int i = -2; i < length; ++i)
            GridBoardVisual.ColumnDefinitions.Add(new ColumnDefinition());

        SetField(0, 0, 0, false);
        SetField(_mancala.Player2Index, 0, _mancala.Player2Index, false);
        Grid.SetRowSpan(_fields[_mancala.Player1Index], 2);
        Grid.SetRowSpan(_fields[_mancala.Player2Index], 2);
        for (int i = _mancala.Player1Index + 1; i < _mancala.Player2Index; ++i)
            SetField(i, 0, i, _mancala.CurrentIsFirst);

        for (int i = _mancala.Player2Index + 1; i < _mancala.FieldsCount; ++i)
            SetField(i, 1, 2 * _mancala.Player2Index - i, !_mancala.CurrentIsFirst);
    }

    private void SetField(int index, int row, int colum, bool isSelectable)
    {
        MancalaField tempField = new MancalaField(index, _mancala[index]);
        tempField.FieldSelected += FieldSelected;
        tempField.IsSelectable = isSelectable;
        GridBoardVisual.Children.Add(tempField);
        Grid.SetColumn(tempField, colum);
        Grid.SetRow(tempField, row);
        _fields[index] = tempField;
    }

    private void ShowMove(int index)
    {
        for (int i = 0; i < _fields.Length; ++i)
            _fields[i].Count = _mancala[i];

        Util.AllowUiToUpdate();
        Thread.Sleep(500);
    }

    private void ShowSteal(int index)
    {
    }

    private void FieldSelected(MancalaFieldClickedEvent e)
    {
        GridBoardVisual.IsEnabled = false;
        _mancala.DoMove(e.Index);
        GridBoardVisual.IsEnabled = true;
        IReadOnlyList<int> options = _mancala.OptionsOfCurrentPlayer;
        int nextOption = 0;
        for (int i = 0; i < _fields.Length; ++i)
        {
            _fields[i].Count = _mancala[i];
            if (nextOption < options.Count && options[nextOption] == i)
            {
                _fields[i].IsSelectable = true;
                ++nextOption;
            }
            else
                _fields[i].IsSelectable = false;
        }

        if (!_mancala.CurrentIsFirst)
            HighlightBotSelection();
    }

    private void HighlightBotSelection()
    {
        var bot = new MancalaBot();
        int option = bot.CalculateIndexOfBestOption(_mancala);
        if (option != -1)
            _fields[_mancala.OptionsOfCurrentPlayer[option]].IsPreferredOptionByBot = true;
    }
}