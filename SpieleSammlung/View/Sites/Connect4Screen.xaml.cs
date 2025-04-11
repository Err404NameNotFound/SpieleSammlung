using SpieleSammlung.Model;
using SpieleSammlung.Model.Connect4;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using SpieleSammlung.View.UserControls.Connect4;

namespace SpieleSammlung.View.Sites;

/// <summary>
/// Interaktionslogik für Connect4Screen.xaml
/// </summary>
public partial class Connect4Screen
{
    private const string NAME_BOT = "bot";

    private readonly Connect4Field[,] _fields;
    private readonly Connect4Visual[][,] _stack;
    private int _nextStackTile;

    private Board _game;
    private Connect4Multiplayer _mpGame;
    private readonly bool _isSinglePlayer;
    private Connect4Tile _currentPlayer;

    private Task _calculation;
    private CancellationTokenSource _tokenSource;
    private CancellationToken _token;


    public Connect4Screen(Player player) : this(player.Name, NAME_BOT, true)
    {
    }

    public Connect4Screen(Player player1, Player player2) : this(player1.Name, player2.Name, false)
    {
    }

    private Connect4Screen(string player1, string player2, bool isSinglePlayer)
    {
        InitializeComponent();
        InitializeBoard(out _fields);
        InitializeStack(out _stack);
        InitializeLevelSelector();
        LblPlayerLeft.Content = player1;
        LblPlayerRight.Content = player2;
        _currentPlayer = Connect4Tile.Player;
        _isSinglePlayer = isSinglePlayer;
        StartNewGame(_currentPlayer);
    }

    public void Shutdown()
    {
        if (IsCalculationRunning())
        {
            _tokenSource.Cancel();
        }
    }

    private bool IsCalculationRunning() => _calculation is { Status: TaskStatus.Running };

    private void InitializeBoard(out Connect4Field[,] fields)
    {
        fields = new Connect4Field[Board.ROWS, Board.COLS];
        for (int row = 0; row < Board.ROWS; ++row)
        {
            for (int col = 0; col < Board.COLS; ++col)
            {
                Connect4Field tempField = new Connect4Field(col);
                tempField.FieldClicked += FieldsKlicked;
                FieldGrid.Children.Add(tempField);
                Grid.SetColumn(tempField, col);
                Grid.SetRow(tempField, Board.ROWS - row - 1);
                fields[row, col] = tempField;
            }
        }
    }

    private void InitializeStack(out Connect4Visual[][,] stack)
    {
        stack =
        [
            new Connect4Visual[3, 7],
            new Connect4Visual[3, 7]
        ];
        _nextStackTile = 0;

        for (int row = 0; row < stack[0].GetLength(0); ++row)
        {
            for (int col = 0; col < stack[0].GetLength(1); ++col)
            {
                AddVisualToStack(0, LeftGrid, Connect4Tile.Player, col, row);
                AddVisualToStack(1, RightGrid, Connect4Tile.Machine, col, row);
            }
        }
    }

    private void InitializeLevelSelector()
    {
        for (int i = 1; i < 10; ++i)
        {
            CBoxLevelSelection.Items.Add(i);
        }

        CBoxLevelSelection.SelectedIndex = Connect4.SUGGESTED_MACHINE_LEVEL - 1;
    }

    private void AddVisualToStack(int indexStack, Panel grid, Connect4Tile value, int row, int col)
    {
        Connect4Visual tempField = new Connect4Visual(value);
        grid.Children.Add(tempField);
        Grid.SetColumn(tempField, col);
        Grid.SetRow(tempField, row + 1);
        tempField.Height = 55;
        tempField.Width = 55;
        _stack[indexStack][col, row] = tempField;
    }

    private void FieldsKlicked(int col)
    {
        if (!_game.IsGameOver() && !IsCalculationRunning())
        {
            if (_isSinglePlayer)
            {
                Board temp = _game.Move(col);
                if (temp != null)
                {
                    _game = temp;
                    HideStackTile();
                }

                if (!_game.IsGameOver())
                {
                    MachineMove();
                }
            }
            else
            {
                bool left = _currentPlayer == Connect4Tile.Player;
                Board temp = left ? _mpGame.Move(col) : _mpGame.MachineMove(col);
                if (temp != null)
                {
                    _mpGame = (Connect4Multiplayer)temp;
                    _game = _mpGame;
                    HideStackTile();
                    _currentPlayer = _currentPlayer.Opponent();
                }
            }

            UpdateUiAfterMachine(false);
        }
    }

    private async void MachineMove()
    {
        _game.SetLevel(CBoxLevelSelection.SelectedIndex + 1);
        _tokenSource = new CancellationTokenSource();
        _token = _tokenSource.Token;
        try
        {
            _calculation = Task.Run(() => _game = _game.MachineMove(), _token);
            await _calculation.ContinueWith(_ => Dispatcher.Invoke(() => UpdateUiAfterMachine(true)), _token);
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            _tokenSource.Dispose();
        }
    }

    private void UpdateUiAfterMachine(bool hideTile)
    {
        if (hideTile)
        {
            HideStackTile();
        }

        if (_game.IsGameOver() && _game.GetWinner() != Connect4Tile.Nobody)
        {
            SetWinner(_game.GetWitness());
        }

        UpdateField();
    }

    private void UpdateField()
    {
        for (int row = 0; row < Board.ROWS; ++row)
        {
            for (int col = 0; col < Board.COLS; ++col)
            {
                _fields[row, col].Color = _game.GetSlot(row, col);
            }
        }
    }

    private void HideStackTile()
    {
        _stack[_nextStackTile % 2][_nextStackTile / 2 % 3, _nextStackTile / 6].Color = Connect4Tile.Nobody;
        ++_nextStackTile;
    }

    private void SetWinner(List<Coordinates2D> winningFields)
    {
        foreach (Coordinates2D fields in winningFields)
        {
            _fields[fields.Y, fields.X].Highlighted = true;
        }

        SetFieldEnable(false);
    }

    private void SetFieldEnable(bool value)
    {
        for (int row = 0; row < Board.ROWS; ++row)
        {
            for (int col = 0; col < Board.COLS; ++col)
            {
                _fields[row, col].IsEnabled = value;
            }
        }
    }

    private void ResetField()
    {
        for (int row = 0; row < Board.ROWS; ++row)
        {
            for (int col = 0; col < Board.COLS; ++col)
            {
                _fields[row, col].IsEnabled = true;
                _fields[row, col].Color = _game.GetSlot(row, col);
                _fields[row, col].Highlighted = false;
            }
        }

        _nextStackTile = 0;
        for (int row = 0; row < _stack[0].GetLength(0); ++row)
        {
            for (int col = 0; col < _stack[0].GetLength(1); ++col)
            {
                _stack[0][row, col].Color = Connect4Tile.Player;
                _stack[1][row, col].Color = Connect4Tile.Machine;
            }
        }
    }

    private void StartNewGame(Connect4Tile player)
    {
        Shutdown();
        if (_isSinglePlayer)
        {
            _game = new Connect4(player, Connect4.SUGGESTED_MACHINE_LEVEL);
            if (player == Connect4Tile.Machine)
            {
                MachineMove();
            }
            _currentPlayer = Connect4Tile.Player;
                
        }
        else
        {
            _mpGame = new Connect4Multiplayer(player, Connect4.SUGGESTED_MACHINE_LEVEL);
            _game = _mpGame;
            _currentPlayer = _game.GetFirstPlayer();
        }
            
        ResetField();
    }

    private void Button_New_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        StartNewGame(_game.GetFirstPlayer());
    }

    private void Button_Switch_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        StartNewGame(_game.GetFirstPlayer().Opponent());
    }
}