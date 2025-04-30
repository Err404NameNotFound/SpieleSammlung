#region

using System.Windows;
using SpieleSammlung.Model.Connect4;

#endregion

namespace SpieleSammlung.View.UserControls.Connect4;

/// <summary>
/// Interaktionslogik für Connect4Field.xaml
/// </summary>
public partial class Connect4Field
{
    public delegate void FieldClickedEvent(int column);

    public Connect4Field(int col, Connect4Tile color = Connect4Tile.Nobody)
    {
        InitializeComponent();
        Column = col;
        Color = color;
        Highlighted = false;
    }

    private int Column { get; }

    public Connect4Tile Color
    {
        get => BtnImage.Color;
        set => BtnImage.Color = value;
    }

    public bool Highlighted
    {
        get => BtnImage.Highlighted;
        set => BtnImage.Highlighted = value;
    }

    public event FieldClickedEvent FieldClicked;

    private void Btn_Click(object sender, RoutedEventArgs e) => FieldClicked?.Invoke(Column);
}