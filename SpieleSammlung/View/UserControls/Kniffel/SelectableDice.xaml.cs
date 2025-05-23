﻿#region

using System;
using System.Windows.Media.Imaging;

#endregion

namespace SpieleSammlung.View.UserControls.Kniffel;

/// <summary>
/// Interaktionslogik für SelectableDice.xaml
/// </summary>
public partial class SelectableDice
{
    private int _diceValue = 1;
    public SelectableDice() => InitializeComponent();

    public bool IsChecked
    {
        get => ToggleBtn.IsChecked == true;
        set => ToggleBtn.IsChecked = value;
    }

    public int Value
    {
        get => _diceValue;
        set
        {
            _diceValue = value;
            BtnImage.Source =
                new BitmapImage(new Uri(@"../../Images/Kniffel/Dice" + value + ".png", UriKind.Relative));
        }
    }
}