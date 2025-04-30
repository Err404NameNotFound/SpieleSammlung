#region

using System.Collections.Generic;
using System.Windows.Controls;
using SpieleSammlung.Model.Mancala;

#endregion

namespace SpieleSammlung.View.UserControls.Mancala;

public partial class MancalaBoard
{
    private readonly MancalaField[] _fields;
    private readonly MancalaGame _mancala;

    public MancalaBoard(int stonesPerField = 4, int length = 6)
    {
        InitializeComponent();
        _mancala = new MancalaGame(ShowMove, ShowSteal, true, stonesPerField, length);
        _fields = new MancalaField[_mancala.FieldsCount];
        SetField(0, 0, 0);
        for (int i = _mancala.Player1Index + 1; i < _mancala.Player2Index; ++i)
            SetField(i, 0, i);

        for (int i = _mancala.Player2Index + 1; i < _mancala.FieldsCount; ++i)
            SetField(i, 1, 2 * _mancala.Player2Index - i);
    }

    private void SetField(int index, int row, int colum)
    {
        MancalaField tempField = new MancalaField(index, _mancala[index]);
        tempField.FieldSelected += FieldSelected;
        GridBoardVisual.Children.Add(tempField);
        Grid.SetColumn(tempField, colum);
        Grid.SetRow(tempField, row);
        _fields[index] = tempField;
    }

    private void ShowMove(int index)
    {
    }

    private void ShowSteal(int index)
    {
    }

    private void FieldSelected(MancalaFieldClickedEvent e)
    {
        _mancala.DoMove(e.Index);
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
    }
}