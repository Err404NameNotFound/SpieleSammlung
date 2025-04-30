using System.Collections.Generic;
using System.Windows.Controls;
using SpieleSammlung.Model.Kniffel;

namespace SpieleSammlung.View.UserControls.Kniffel;

/// <summary>
/// Interaktionslogik für KniffelPointsVisual.xaml
/// </summary>
public partial class KniffelPointsVisual
{
    private readonly List<Label> _fields;
    private KniffelPlayer[] _players;

    public KniffelPointsVisual()
    {
        InitializeComponent();
        _fields =
        [
            LblValue1, LblValue2, LblValue3, LblValue4, LblValue5, LblValue6, LblValueSumTop,
            LblValueBonus, LblValueTriple, LblValueQuad,
            LblValueFullH, LblValueSmallStreet, LblValueBigStreet, LblValueKniffel, LblValueChance, LblValueSumBot,
            LblValueSumTopTotal, LblValueSumTotal
        ];
        CBoxPlayerNames.IsEnabled = false;
    }

    public void ShowPlayer(int index)
    {
        KniffelPlayer p = _players[index];
        for (int i = 0; i < _fields.Count; ++i)
        {
            if (p.Fields[i].IsEmpty())
                _fields[i].Content = "";
            else if (p.Fields[i].Value == 0)
                _fields[i].Content = "-";
            else
                _fields[i].Content = p.Fields[i].Value;
        }
    }

    public void FillPlayerList(KniffelPlayer[] p)
    {
        CBoxPlayerNames.Items.Clear();
        _players = p;
        foreach (KniffelPlayer player in p)
        {
            CBoxPlayerNames.Items.Add(player.Name);
        }
    }

    private void CBoxPlayerNames_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ShowPlayer(CBoxPlayerNames.SelectedIndex);
    }

    public void SelectHighest()
    {
        int max = 0;
        for (int i = 1; i < _players.Length; ++i)
        {
            if (_players[i].Fields[_fields.Count - 1].Value > _players[max].Fields[_fields.Count - 1].Value)
                max = i;
        }

        CBoxPlayerNames.SelectedIndex = max;
    }
}