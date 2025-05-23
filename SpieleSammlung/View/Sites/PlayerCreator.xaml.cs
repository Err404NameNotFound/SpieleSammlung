﻿#region

using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using SpieleSammlung.Model;
using SpieleSammlung.View.Enums;

#endregion

namespace SpieleSammlung.View.Sites;

/// <summary>
/// Interaktionslogik für PlayerCreator.xaml
/// </summary>
public partial class PlayerCreator
{
    public delegate void OnStartMatch(GameMode mode, List<Player> players);

    private readonly int _maxPlayer;
    private readonly int _minPlayer;
    private readonly GameMode _mode;
    private readonly List<Player> _players;

    public PlayerCreator(GameMode mode, int min, int max)
    {
        InitializeComponent();
        PcBtnAddBot.Visibility = mode == GameMode.Kniffel ? Visibility.Visible : Visibility.Hidden;
        _mode = mode;
        _minPlayer = min;
        _maxPlayer = max;
        _players = [];
        Update();
    }

    public event OnStartMatch StartMatch;

    private void BtnPlayerAdd_Click(object sender, RoutedEventArgs e) => AddPlayer();

    private void BtnPlayerRemove_Click(object sender, RoutedEventArgs e)
    {
        int temp = LBoxPlayerNames.SelectedIndex;
        if (temp == -1) return;
        _players.RemoveAt(temp);
        LBoxPlayerNames.Items.RemoveAt(temp);
        LBoxPlayerNames.SelectedIndex = temp - 1;
        if (LBoxPlayerNames.SelectedIndex == -1 && LBoxPlayerNames.Items.Count != 0)
            LBoxPlayerNames.SelectedIndex = 0;

        Update();
    }

    private void BtnPlayerUp_Click(object sender, RoutedEventArgs e)
    {
        int temp = LBoxPlayerNames.SelectedIndex;
        if (temp != -1 && temp > 0)
            Switch(temp, temp - 1);
    }

    private void BtnPlayerDown_Click(object sender, RoutedEventArgs e)
    {
        int selected = LBoxPlayerNames.SelectedIndex;
        if (selected != -1 && selected < LBoxPlayerNames.Items.Count - 1)
            Switch(selected, selected + 1);
    }

    private void Switch(int first, int second)
    {
        (_players[first], _players[second]) = (_players[second], _players[first]);
        (LBoxPlayerNames.Items[first], LBoxPlayerNames.Items[second])
            = (LBoxPlayerNames.Items[second], LBoxPlayerNames.Items[first]);
        // LBoxPlayerNames.ItemsSource = players;
        LBoxPlayerNames.SelectedIndex = second;
        Update();
    }

    private void OnKeyDownHandler(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Return)
            AddPlayer();
    }

    private void AddPlayer(Player player = null)
    {
        if (LBoxPlayerNames.Items.Count >= _maxPlayer) return;
        bool add = false;
        if (player != null)
            add = true;
        else if (PcTxtBoxPlayerName.Text.Length > 0)
        {
            if (IsNameNotAlreadyInList(PcTxtBoxPlayerName.Text))
            {
                player = new Player(PcTxtBoxPlayerName.Text, false);
                PcTxtBoxPlayerName.Clear();
                add = true;
            }
        }

        if (!add)
            return;

        _players.Add(player);
        LBoxPlayerNames.Items.Add(player);
        PcTxtBoxPlayerName.Focus();
        Update();
    }

    private bool IsNameNotAlreadyInList(string name)
    {
        int w = 0;
        while (w < LBoxPlayerNames.Items.Count)
        {
            if (LBoxPlayerNames.Items[w].Equals(name)) return false;
            ++w;
        }

        return true;
    }

    private void Update() => PcBtnPlayerDone.IsEnabled = LBoxPlayerNames.Items.Count >= _minPlayer;

    private void BtnPlayerDone_Click(object sender, RoutedEventArgs e)
    {
        if (LBoxPlayerNames.Items.Count >= _minPlayer && LBoxPlayerNames.Items.Count <= _maxPlayer)
            StartMatch?.Invoke(_mode, _players);
    }

    private void LblPlayerName_Loaded(object sender, RoutedEventArgs e) => PcTxtBoxPlayerName.Focus();

    private void BtnAddBot_Click(object sender, RoutedEventArgs e) => AddPlayer(new Player());
}