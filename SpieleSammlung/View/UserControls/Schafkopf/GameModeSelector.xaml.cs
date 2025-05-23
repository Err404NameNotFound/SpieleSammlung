﻿#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using SpieleSammlung.Model.Schafkopf;
using SpieleSammlung.View.Enums;

#endregion

namespace SpieleSammlung.View.UserControls.Schafkopf;

/// <summary>
/// Interaktionslogik für GameModeSelector.xaml
/// </summary>
public partial class GameModeSelector
{
    public delegate void OnColorChanged(GameModeSelectedEvent e);

    public delegate void OnModeSelectedEvent(GameModeSelectedEvent e);

    private List<SchafkopfMatchPossibility> _possibilities;
    private GameSelectorState _state;

    public GameModeSelector() => InitializeComponent();

    private SchafkopfMode SelectedMode =>
        SchafkopfMatchConfig.StringToSchafkopfMode(CbMode.SelectedItem.ToString());

    public List<SchafkopfMatchPossibility> Source
    {
        set
        {
            _possibilities = value;
            List<string> temp = value.Select(possibility => possibility.Mode.ToString()).ToList();
            CbMode.ItemsSource = temp;
            CbMode.SelectedIndex = 0;
        }
    }

    public GameSelectorState State
    {
        set
        {
            _state = value;
            if (_state == GameSelectorState.Hidden)
            {
                CbMode.Visibility = Visibility.Hidden;
                CbColor.Visibility = Visibility.Hidden;
                BtnSelectMode.Visibility = Visibility.Hidden;
                BtnSelectMode.IsChecked = false;
            }
            else
            {
                CbColor.Visibility = Visibility.Visible;
                CbMode.Visibility = Visibility.Visible;
                if (_state == GameSelectorState.Selected)
                {
                    CbMode.IsEnabled = false;
                    CbColor.IsEnabled = false;
                    BtnSelectMode.Visibility = Visibility.Hidden;
                }
                else
                {
                    BtnSelectMode.Visibility = Visibility.Visible;
                    if (_state == GameSelectorState.Visible)
                        BtnSelectMode.IsChecked = false;

                    CbMode.IsEnabled = true;
                    CbColor.IsEnabled = true;
                }
            }
        }
    }

    public event OnModeSelectedEvent ModeSelected;

    public event OnColorChanged ColorChanged;


    private void CbMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CbMode.SelectedIndex != -1)
        {
            CbColor.ItemsSource = _possibilities[CbMode.SelectedIndex].Colors;
            CbColor.SelectedIndex = 0;
            ColorHasChanged();
        }
    }

    private void CbColor_SelectionChanged(object sender, SelectionChangedEventArgs e) => ColorHasChanged();

    private void BtnSelectMode_Click(object sender, RoutedEventArgs e)
    {
        if (BtnSelectMode.IsChecked == true)
        {
            if (ModeSelected == null)
                throw new NotSupportedException("This should have been set");

            ModeSelected(CreateEventArgs());
        }
        else
            State = GameSelectorState.Visible;
    }

    private GameModeSelectedEvent CreateEventArgs() => new(SelectedMode, CbColor.SelectedItem.ToString());


    public void TrySelect()
    {
        if (BtnSelectMode.IsChecked == true)
            BtnSelectMode_Click(BtnSelectMode, null);
    }

    private void ColorHasChanged()
    {
        if (_possibilities[CbMode.SelectedIndex].Mode != SchafkopfMode.Weiter && CbColor.SelectedIndex != -1)
            ColorChanged(CreateEventArgs());
    }

    public void CheckIfSelectedStillValid(SchafkopfMode mode, SchafkopfMatch match, SchafkopfPlayer player)
    {
        SchafkopfMode modePlayer = SelectedMode;
        string colorPlayer = CbColor.SelectedItem.ToString();
        if (mode > modePlayer && modePlayer != SchafkopfMode.Weiter)
            State = GameSelectorState.Visible;

        if (!CbMode.IsEnabled && modePlayer >= match.MinimumGame) return;
        player.RemovePossibility(SchafkopfMode.Sauspiel);
        if (mode == SchafkopfMode.Wenz || match.MinimumGame == SchafkopfMode.Solo ||
            match.MinimumGame == SchafkopfMode.WenzTout || match.MinimumGame == SchafkopfMode.SoloTout)
        {
            player.RemovePossibility(SchafkopfMode.Wenz);
            if (mode == SchafkopfMode.Solo || match.MinimumGame == SchafkopfMode.WenzTout ||
                match.MinimumGame == SchafkopfMode.SoloTout)
            {
                player.RemovePossibility(SchafkopfMode.Solo);
                if (mode == SchafkopfMode.WenzTout || match.MinimumGame == SchafkopfMode.SoloTout)
                {
                    player.RemovePossibility(SchafkopfMode.WenzTout);
                    if (mode == SchafkopfMode.SoloTout)
                        player.RemovePossibility(SchafkopfMode.SoloTout);
                }
            }
        }

        Source = player.Possibilities;
        if (modePlayer > mode)
        {
            CbMode.SelectedIndex = player.PossibilityIndexOf(modePlayer);
            CbColor.SelectedIndex =
                player.PossibilityIndexOf(CbMode.SelectedIndex, Card.ParseNullableColor(colorPlayer));
        }
    }

    public void SetGameSelectorFocus(bool focused)
    {
        if (focused && CbMode.Visibility == Visibility.Visible)
            ModeBorder.BorderBrush = Brushes.Red;
        else
            ModeBorder.BorderBrush = Brushes.Transparent;
    }
}