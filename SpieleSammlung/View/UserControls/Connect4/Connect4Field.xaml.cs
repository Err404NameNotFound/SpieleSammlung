using SpieleSammlung.Model.Connect4;

namespace SpieleSammlung.View.UserControls.Connect4;

/// <summary>
/// Interaktionslogik für Connect4Field.xaml
/// </summary>
public partial class Connect4Field
{
    public delegate void FieldClickedEvent(int column);

    public event FieldClickedEvent FieldClicked;

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

    private void Btn_Click(object sender, System.Windows.RoutedEventArgs e) => FieldClicked?.Invoke(Column);
}