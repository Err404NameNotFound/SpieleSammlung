using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using SpieleSammlung.Model;
using SpieleSammlung.Model.Multiplayer;
using SpieleSammlung.View.Enums;
using SpieleSammlung.View.Sites;

namespace SpieleSammlung.View.Windows;

/// <summary>
/// Interaktionslogik für MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    private readonly StartScreen _viewStart;
    private PlayerCreator _viewPlayerCreator;
    private MultiplayerLobby _viewMultiplayerLobby;
    private SchafkopfScreen _viewSchafkopfScreen;
    private KniffelScreen _viewKniffelScreen;
    private Connect4Screen _viewConnect4Screen;
    private MancalaScreen _viewMancalaScreen;

    private MainWindowView _view;

    private UserControl CurrentView
    {
        get
        {
            return _view switch
            {
                MainWindowView.StartScreen => _viewStart,
                MainWindowView.PlayerCreator => _viewPlayerCreator,
                MainWindowView.MultiplayerLobby => _viewMultiplayerLobby,
                MainWindowView.Schafkopf => _viewSchafkopfScreen,
                MainWindowView.Kniffel => _viewKniffelScreen,
                MainWindowView.VierGewinnt => _viewConnect4Screen,
                MainWindowView.Mancala => _viewMancalaScreen,
                _ => throw new ArgumentException("This game mode has not been fully implemented")
            };
        }
    }

    public MainWindow()
    {
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        InitializeComponent();
        MinWidth = 900;
        MinHeight = 555;
        //MinWidth = Width;
        //MinHeight = Height;
        //MaxWidth = Width;
        //MaxHeight = Height;
        _view = MainWindowView.StartScreen;
        _viewStart = new StartScreen();
        _viewStart.ChoseModeEvent += ViewStart_ChoseModeEvent;
        ShowUserControl(_viewStart);
        ModelLog.WriteToConsole = false;
        ModelLog.WriteToFile = true;
    }

    private void ShowUserControl(UIElement control)
    {
        MainWindowGrid.Children.Add(control);
        Grid.SetColumn(control, 0);
        Grid.SetRow(control, 0);
    }

    private void DataWindow_Closing(object sender, CancelEventArgs e) => CloseOpenConnection();


    private void CloseOpenConnection()
    {
        switch (_view)
        {
            case MainWindowView.VierGewinnt:
                _viewConnect4Screen.Shutdown();
                break;
            case MainWindowView.Schafkopf:
                _viewSchafkopfScreen.EndConnection();
                break;
            case MainWindowView.MultiplayerLobby:
                _viewMultiplayerLobby.EndConnection();
                break;
            case MainWindowView.StartScreen:
            case MainWindowView.PlayerCreator:
            case MainWindowView.Kniffel:
            case MainWindowView.Mancala:
                break; // Nothing to do
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private async void BtnCancel_ClickAsync(object sender, RoutedEventArgs e)
    {
        switch (_view)
        {
            case MainWindowView.StartScreen:
                break;
            case MainWindowView.PlayerCreator:
                ChooseScreen(MainWindowView.StartScreen);
                break;
            case MainWindowView.MultiplayerLobby:
            case MainWindowView.VierGewinnt:
            case MainWindowView.Kniffel:
            case MainWindowView.Schafkopf:
            case MainWindowView.Mancala:
            {
                bool showWindow = true;
                if (_view == MainWindowView.Schafkopf)
                {
                    showWindow = _viewSchafkopfScreen.CanQuitNow;
                    _viewSchafkopfScreen.InvertQuitAfterMatch();
                    if (!showWindow)
                    {
                        BtnCancel.Content = _viewSchafkopfScreen.QuitAfterMatch
                            ? Properties.Resources.MW_Btn_LeavesAfterMatch
                            : Properties.Resources.Main_Btn_LeaveMatch;
                    }
                }

                if (showWindow)
                {
                    WindowsMode error;
                    await ShowPopup(error = new WindowsMode(Properties.Resources.ConfirmLeave_Message));
                    if (error.Status)
                    {
                        CloseOpenConnection();
                        ChooseScreen(MainWindowView.StartScreen);
                        BtnCancel.Content = Properties.Resources.Main_Btn_LeaveMatch;
                    }
                }

                break;
            }
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private static Task ShowPopup<TPopup>(TPopup popup) where TPopup : Window
    {
        var task = new TaskCompletionSource<object>();
        popup.Owner = Application.Current.MainWindow;
        popup.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        popup.Closed += (_, _) => task.SetResult(null);
        popup.ShowDialog();
        popup.Focus();
        return task.Task;
    }

    private void UpdateCancelButton()
    {
        if (_view == MainWindowView.StartScreen) BtnCancel.Visibility = Visibility.Hidden;
        else
        {
            BtnCancel.Visibility = Visibility.Visible;
            BtnCancel.Content = _view == MainWindowView.PlayerCreator
                ? Properties.Resources.Main_LeavePlayerCreator
                : Properties.Resources.Main_Btn_LeaveMatch;
        }
    }

    private void ChooseScreen(MainWindowView nextView)
    {
        MainWindowGrid.Children.Remove(CurrentView);
        _view = nextView;
        ShowUserControl(CurrentView);
        UpdateCancelButton();
    }

    private void ViewStart_ChoseModeEvent(GameMode chosenMode, int min, int max)
    {
        if (chosenMode == GameMode.Schafkopf)
        {
            _viewMultiplayerLobby = new MultiplayerLobby(chosenMode, min, max);
            _viewMultiplayerLobby.StartMatch += StartOnlineMatch;
            ChooseScreen(MainWindowView.MultiplayerLobby);
        }
        else
        {
            _viewPlayerCreator = new PlayerCreator(chosenMode, min, max);
            _viewPlayerCreator.StartMatch += StartOfflineMatch;
            ChooseScreen(MainWindowView.PlayerCreator);
        }
    }

    private void StartOfflineMatch(GameMode mode, List<Player> playerNames)
    {
        switch (mode)
        {
            case GameMode.Zufallszahlen:
            case GameMode.Lotto:
            case GameMode.Maexchen:
                break;
            case GameMode.VierGewinnt:
                _viewConnect4Screen = playerNames.Count == 1
                    ? new Connect4Screen(playerNames[0])
                    : new Connect4Screen(playerNames[0], playerNames[1]);

                break;
            case GameMode.Kniffel:
                _viewKniffelScreen = new KniffelScreen(playerNames);
                break;
            case GameMode.Mancala:
                _viewMancalaScreen = new MancalaScreen(true);
                break;
            case GameMode.Schafkopf:
                throw new ArgumentOutOfRangeException(nameof(mode), mode,
                    Properties.Resources.Err_msg_not_a_single_player_game);
            default:
                throw new ArgumentOutOfRangeException(nameof(mode), mode, "");
        }

        ChooseScreen(Convert(mode));
    }

    private static MainWindowView Convert(GameMode mode)
    {
        return mode switch
        {
            GameMode.Kniffel => MainWindowView.Kniffel,
            GameMode.Schafkopf => MainWindowView.Schafkopf,
            GameMode.VierGewinnt => MainWindowView.VierGewinnt,
            GameMode.Mancala => MainWindowView.Mancala,
            _ => MainWindowView.StartScreen
        };
    }

    private void StartOnlineMatch(GameMode mode, List<MultiplayerPlayer> players, int index,
        MpConnection connection, bool reJoin)
    {
        _viewSchafkopfScreen = new SchafkopfScreen(players, index, connection, reJoin);
        _viewSchafkopfScreen.QuitMatch += QuitOnlineMatch;
        _viewSchafkopfScreen.LostConnectionToHost += LostConnectionToHost;
        ChooseScreen(MainWindowView.Schafkopf);
    }

    private void QuitOnlineMatch()
    {
        _viewSchafkopfScreen.EndConnection();
        _viewSchafkopfScreen = null;
        ChooseScreen(MainWindowView.StartScreen);
    }

    private void LostConnectionToHost() => BtnCancel.Content = Properties.Resources.Main_Btn_LeaveMatch;
}