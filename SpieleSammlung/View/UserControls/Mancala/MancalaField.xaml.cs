using System;
using System.Windows;
using System.Windows.Media;
using SpieleSammlung.Model.Mancala;

namespace SpieleSammlung.View.UserControls.Mancala;

public partial class MancalaField
{
    public delegate void OnFieldSelectedEvent(MancalaFieldClickedEvent e);

    public event OnFieldSelectedEvent FieldSelected;

    private int _count;
    private readonly int _index;

    public bool IsSelectable
    {
        get => FieldValueDisplay.IsEnabled;
        set
        {
            if (value)
            {
                FieldBorder.BorderBrush = Brushes.LawnGreen;
                FieldValueDisplay.IsEnabled = true;
            }
            else
            {
                FieldBorder.BorderBrush = Brushes.Transparent;
                FieldValueDisplay.IsEnabled = false;
            }
        }
    }

    public bool IsPreferredOptionByBot
    {
        get => FieldBorder.BorderBrush == Brushes.Cyan;
        set
        {
            if (value)
                FieldBorder.BorderBrush = Brushes.Cyan;
            else
                FieldBorder.BorderBrush = IsSelectable ? Brushes.LawnGreen : Brushes.Transparent;
        }
    }

    public int Count
    {
        get => _count;
        set
        {
            _count = value;
            FieldValueDisplay.Content = value;
        }
    }

    public MancalaField(int index = -1, int count = -1)
    {
        InitializeComponent();
        _index = index;
        Count = count;
    }

    private void FieldValueDisplay_OnClick(object sender, RoutedEventArgs e)
    {
        if (FieldSelected == null)
            throw new NotSupportedException("This should have been set");
        if (IsSelectable)
            FieldSelected(new MancalaFieldClickedEvent(_index));
    }
}