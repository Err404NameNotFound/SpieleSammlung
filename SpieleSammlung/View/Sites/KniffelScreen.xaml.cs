using SpieleSammlung.Model;
using SpieleSammlung.Model.Kniffel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using SpieleSammlung.Model.Kniffel.Fields;
using SpieleSammlung.View.UserControls.Kniffel;

namespace SpieleSammlung.View.Sites
{
    /// <summary>
    /// Interaktionslogik für KniffelScreen.xaml
    /// </summary>
    public partial class KniffelScreen
    {
        #region private member

        /// <summary>
        /// UI Elements resembling the dices.
        /// </summary>
        private readonly List<SelectableDice> _dices;

        /// <summary>
        /// UI Elements active after the end of the match. Another display to compare the points of each player.
        /// </summary>
        private readonly List<KniffelPointsVisual> _afterMatchPoints;

        /// <summary>
        /// Model running the logic of a Kniffel match.
        /// </summary>
        private readonly KniffelGame _game;

        /// <summary>
        /// Timing for the shuffle animation.
        /// </summary>
        private readonly Stopwatch _watch;

        /// <summary>
        /// Stored for performance to prevent cloning for every access.
        /// </summary>
        private readonly int[] _allDices = FlatDice.AllDices;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new Instance and sets up a playable game. The first player in the list starts the game.
        /// </summary>
        /// <param name="names">Players of the game.</param>
        public KniffelScreen(List<Player> names)
        {
            InitializeComponent();
            _watch = new Stopwatch();
            _dices = [Dice1, Dice2, Dice3, Dice4, Dice5];
            _game = new KniffelGame(names);
            foreach (var dice in _dices) dice.IsChecked = false;
            foreach (var player in names) Fields.CBoxPlayerNames.Items.Add(player);
            Fields.FillPlayerList(_game.Players);
            FieldsChoose.FillPlayerList(_game.Players);
            FieldsChoose.CBoxPlayerNames.SelectedIndex = 0;
            FieldsChoose.CBoxPlayerNames.IsEnabled = true;
            _afterMatchPoints = [FieldsChangeable1, FieldsChangeable2, FieldsChangeable3];
            BtnNextPlayer.Visibility = Visibility.Hidden;
            BtnNewTry.IsEnabled = true;
            ShowShuffledDices();
            ShowCurrentPlayer();
            UpdateRoundNumber();
        }

        #endregion

        #region UI-Updates

        private void ShowPlayer(int index)
        {
            Fields.CBoxPlayerNames.SelectedIndex = index;
            LblPlayerNumber.Content = (index + 1).ToString();
            Fields.ShowPlayer(index);
            FieldsChoose.ShowPlayer(FieldsChoose.CBoxPlayerNames.SelectedIndex);
        }

        private void ShowCurrentPlayer()
        {
            ShowPlayer(_game.ActivePlayer);
        }

        private void RefreshPlayer()
        {
            ShowPlayer(Fields.CBoxPlayerNames.SelectedIndex);
        }

        private void NextPlayer()
        {
            BtnNextPlayer.Visibility = Visibility.Hidden;
            if (_game.IsGameNotOver())
            {
                BtnNewTry.IsEnabled = true;
                UpdateRoundNumber();
                ShowCurrentPlayer();
                ShuffleAnimation(_allDices);
                ShowShuffledDices();
                DoBotMoveIfRequired();
            }
            else
            {
                ShowGameOverScreen();
            }
        }

        private void DoBotSleep() => Thread.Sleep(2000);

        private void DoBotMoveIfRequired()
        {
            if (_game.CurrentPlayer.IsBot)
            {
                DoBotSleep();
                ModelLog.AppendLine("Move of Player \"" + _game.CurrentPlayer + "\" started");
                BtnNewTry.IsEnabled = false;
                AllowUiToUpdate();
                _game.DoBotMove(AnimationBetweenBotMoves, ShowShuffledDices);
                EnableNextPlayer();
                ModelLog.AppendLine("Move of the last player ended");
                ModelLog.AppendSeparatorLine(1);
            }
        }

        private void AnimationBetweenBotMoves(int[] selected)
        {
            foreach (var t in selected)
            {
                _dices[t].IsChecked = true;
            }

            ShuffleAnimation(selected);
            AllowUiToUpdate();
            DoBotSleep();
        }

        private void UpdateRoundNumber()
        {
            if (!Fields.CBoxPlayerNames.IsEnabled)
            {
                Fields.CBoxPlayerNames.SelectedIndex = _game.ActivePlayer;
                LblRoundNumber.Content = (_game.Round + 1).ToString();
            }
        }

        private void ShowGameOverScreen()
        {
            PlayingView.Visibility = Visibility.Collapsed;
            int index = 4;
            List<KniffelPlayer> player = _game.Players;
            int w = 2;
            while (player.Count < w + 3 && w >= 0)
            {
                --index;
                _afterMatchPoints[w].Visibility = Visibility.Collapsed;
                --w;
            }

            FieldsChoose.CBoxPlayerNames.SelectedIndex = index;
            w = 0;
            while (player.Count > w + 2 && w < _afterMatchPoints.Count)
            {
                _afterMatchPoints[w].FillPlayerList(player);
                _afterMatchPoints[w].CBoxPlayerNames.IsEnabled = true;
                _afterMatchPoints[w].CBoxPlayerNames.SelectedIndex = 0;
                ++w;
            }

            Fields.SelectHighest();
        }

        private void UpdateFieldComboBoxes()
        {
            CBoxKillField.Items.Clear();
            for (int i = 0; i < _game.KillableFieldsCount; ++i)
            {
                CBoxKillField.Items.Add(KniffelPointsTable.FIELD_NAMES[_game.GetKillableFieldIndex(i)]);
            }

            if (CBoxKillField.Items.Count > 0)
            {
                CBoxKillField.SelectedIndex = 0;
                CBoxKillField.IsEnabled = true;
                BtnKillField.IsEnabled = true;
            }
            else
            {
                CBoxKillField.SelectedIndex = -1;
                CBoxKillField.IsEnabled = false;
                BtnKillField.IsEnabled = false;
            }

            CBoxWriteField.Items.Clear();
            for (int i = 0; i < _game.WriteableFieldsCount; ++i)
            {
                WriteOption option = _game.GetWriteableFieldsIndex(i);
                CBoxWriteField.Items.Add(KniffelPointsTable.FIELD_NAMES[option.Index] + " -> " + option.Value);
            }

            if (CBoxWriteField.Items.Count > 0)
            {
                CBoxWriteField.SelectedIndex = 0;
                CBoxWriteField.IsEnabled = true;
                BtnWriteField.IsEnabled = true;
            }
            else
            {
                CBoxWriteField.SelectedIndex = -1;
                CBoxWriteField.IsEnabled = false;
                BtnWriteField.IsEnabled = false;
            }
        }

        private void EnableNextPlayer()
        {
            RefreshPlayer();
            CBoxKillField.IsEnabled = false;
            CBoxWriteField.IsEnabled = false;
            BtnKillField.IsEnabled = false;
            BtnWriteField.IsEnabled = false;
            BtnNewTry.IsEnabled = false;
            BtnNextPlayer.Visibility = Visibility.Visible;
        }

        private int[] GetSelectedDiceIndexes()
        {
            int w = 0;
            List<int> index = [];
            while (w < 5)
            {
                if (_dices[w].IsChecked)
                {
                    index.Add(w);
                }

                ++w;
            }

            return index.ToArray();
        }

        #endregion

        #region Shuffle Animation

        private void AllowUiToUpdate()
        {
            DispatcherFrame frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Render, new DispatcherOperationCallback(
                delegate
                {
                    frame.Continue = false;
                    return null;
                }), null);

            Dispatcher.PushFrame(frame);
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background,
                new Action(delegate { }));
        }

        private void ShuffleValues(IEnumerable<int> selectedIndexes)
        {
            foreach (var t in selectedIndexes)
            {
                _dices[t].Value = _game.RandomDiceValue();
            }
        }

        private void ShuffleAnimation(int[] index, int millis = 250)
        {
            BtnNewTry.IsEnabled = false;
            _watch.Restart();
            while (Mouse.LeftButton == MouseButtonState.Pressed || _watch.ElapsedMilliseconds < millis)
            {
                ShuffleValues(index);
                AllowUiToUpdate();
            }

            _watch.Stop();
            BtnNewTry.IsEnabled = true;
        }

        private void ShowShuffledDices()
        {
            for (int i = 0; i < Dice.DICE_COUNT; ++i)
            {
                _dices[i].Value = _game.GetDiceValue(i);
                _dices[i].IsChecked = false;
            }

            LblRemainingShuffle.Content = _game.RemainingShuffles.ToString();
            if (_game.RemainingShuffles <= 0)
            {
                BtnNewTry.IsEnabled = false;
            }

            UpdateFieldComboBoxes();
        }

        #endregion

        #region UI Listener

        private void BtnNewTry_Click(object sender, RoutedEventArgs e)
        {
            int[] index = GetSelectedDiceIndexes();
            if (index.Length > 0)
            {
                ShuffleAnimation(index);
                _game.Shuffle(index);
                ShowShuffledDices();
            }
            else
            {
                DoBotMoveIfRequired(); // TODO: find better solution to making first bot move
            }
        }

        private void BtnKillField_Click(object sender, RoutedEventArgs e)
        {
            if (CBoxKillField.SelectedIndex != -1)
            {
                _game.KillFieldOption(CBoxKillField.SelectedIndex);
                EnableNextPlayer();
            }
        }

        private void BtnWriteField_Click(object sender, RoutedEventArgs e)
        {
            if (CBoxWriteField.SelectedIndex != -1)
            {
                _game.WriteField(CBoxWriteField.SelectedIndex);
                EnableNextPlayer();
            }
        }

        private void BtnNextPlayer_Click(object sender, RoutedEventArgs e)
        {
            NextPlayer();
        }

        #endregion
    }
}