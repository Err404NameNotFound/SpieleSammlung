using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using SpieleSammlung.Model.Schafkopf;

namespace SpieleSammlung.View.Windows
{
    /// <summary>
    /// Interaktionslogik für SchafkopfPoints.xaml
    /// </summary>
    public partial class SchafkopfPoints
    {
        private DataTable _single;
        private DataTable _cumulated;
        private IReadOnlyList<PointsStorage> _playerPoints;

        private const char SEPARATOR = ';';
        private const string PUNKTE_CSV = "punkte.csv";
        private const string SPIELE_CSV = "spiele.csv";
        private const string KUMULIERT_CSV = "kumuliert.csv";
        private readonly bool[] _canPrint;

        private readonly List<Label> _lblNames;
        private readonly List<Label> _lblPoints;

        public SchafkopfPoints(IReadOnlyList<PointsStorage> playerPoints, DataTable single, DataTable cumulated)
        {
            InitializeComponent();
            _canPrint = new bool[3];
            _lblNames =
            [
                LblNamePlayer1,
                LblNamePlayer2,
                LblNamePlayer3,
                LblNamePlayer4,
                LblNamePlayer5,
                LblNamePlayer6,
                LblNamePlayer7
            ];
            _lblPoints =
            [
                LblPointsPlayer1,
                LblPointsPlayer2,
                LblPointsPlayer3,
                LblPointsPlayer4,
                LblPointsPlayer5,
                LblPointsPlayer6,
                LblPointsPlayer7
            ];
            Update(playerPoints, single, cumulated);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            switch (TabView.SelectedIndex)
            {
                case 0:
                    File.WriteAllLines(PUNKTE_CSV, EndResult());
                    break;
                case 1:
                    File.WriteAllLines(SPIELE_CSV, DataTableContentToLines(_single));
                    break;
                default:
                    File.WriteAllLines(KUMULIERT_CSV, DataTableContentToLines(_cumulated));
                    break;
            }

            BtnPrint.IsEnabled = _canPrint[TabView.SelectedIndex] = false;
        }

        private IEnumerable<string> EndResult()
        {
            List<string> ret = new List<string>(2);
            StringBuilder bob = new StringBuilder(_playerPoints.Count * 2);
            int i = 0;
            while (true)
            {
                bob.Append(_playerPoints[i].Name);
                if (++i != _playerPoints.Count) bob.Append(SEPARATOR);
                else break;
            }

            ret.Add(bob.ToString());
            bob.Clear();
            i = 0;
            while (true)
            {
                bob.Append(_playerPoints[i].Points);
                if (++i != _playerPoints.Count) bob.Append(SEPARATOR);
                else break;
            }

            ret.Add(bob.ToString());
            return ret;
        }

        public void Update(IReadOnlyList<PointsStorage> playerPoints, DataTable single, DataTable cumulated)
        {
            BtnPrint.IsEnabled = _canPrint[0] = _canPrint[1] = _canPrint[2] = true;
            _single = single;
            _cumulated = cumulated;
            _playerPoints = playerPoints;
            GridSingle.ItemsSource = _single.DefaultView;
            GridCumulated.ItemsSource = _cumulated.DefaultView;
            int i;
            for (i = 0; i < playerPoints.Count; ++i)
            {
                _lblNames[i].Content = playerPoints[i].Name;
                _lblPoints[i].Content = PointsToString(ref playerPoints[i].Points);
                _lblNames[i].Visibility = Visibility.Visible;
                _lblPoints[i].Visibility = Visibility.Visible;
            }

            for (; i < 7; ++i)
            {
                _lblNames[i].Visibility = Visibility.Hidden;
                _lblPoints[i].Visibility = Visibility.Hidden;
            }
        }

        private static string PointsToString(ref int points)
        {
            StringBuilder bob = new StringBuilder(5);
            if (points < 0)
            {
                bob.Append("-");
                points *= -1;
            }

            bob.Append(points / 100).Append(",").Append(points % 100).Append(" €");
            return bob.ToString();
        }

        private static IEnumerable<string> DataTableContentToLines(DataTable table)
        {
            List<string> ret = new List<string>(table.Rows.Count);
            int row = 0;
            int col = 0;
            StringBuilder bob = new StringBuilder(table.Columns.Count * 2);
            while (true)
            {
                bob.Append(table.Columns[col].ColumnName);
                if (++col != table.Columns.Count)
                {
                    bob.Append(SEPARATOR);
                }
                else
                {
                    break;
                }
            }

            ret.Add(bob.ToString());
            while (row != table.Rows.Count)
            {
                bob.Clear();
                col = 0;
                while (true)
                {
                    bob.Append(table.Rows[row][col]);
                    if (++col != table.Columns.Count)
                    {
                        bob.Append(SEPARATOR);
                    }
                    else
                    {
                        break;
                    }
                }

                ret.Add(bob.ToString());
                ++row;
            }

            return ret;
        }

        private void TabView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BtnPrint.IsEnabled = _canPrint[TabView.SelectedIndex];
        }
    }
}