using System;
using System.Collections.Generic;
using System.Linq;
using SpieleSammlung.Model.Util;

namespace SpieleSammlung.Model.Mancala
{
    public class MancalaGame : ICloneable
    {
        private readonly int _stoneCount;
        private readonly int _minStoneCountForWin;
        private readonly int[] _fields;
        private readonly bool _isCaptureMode;
        private readonly Action<int> _miniMoveAnimation;
        private readonly Action<int> _stealAnimation;

        public int this[int index] => _fields[index];
        public int Player1Index { get; }
        public int Player2Index { get; }
        public IReadOnlyList<int> OptionsOfCurrentPlayer { private set; get; }
        public bool CurrentIsFirst { get; private set; }
        public int CurrentPlayer => CurrentIsFirst ? Player1Index : Player2Index;
        public int PointsPlayer1 => _fields[Player1Index];
        public int PointsPlayer2 => _fields[Player2Index];
        public bool Player1IsWinner => _fields[Player1Index] > _minStoneCountForWin;
        public bool Player2IsWinner => _fields[Player2Index] > _minStoneCountForWin;
        public bool IsGameOver => _fields[Player1Index] + _fields[Player2Index] == _stoneCount;
        public int FieldsCount => _fields.Length;

        public MancalaGame(Action<int> betweenMoveAnimation, Action<int> stealAnimation, bool isCapture,
            int stonesPerField, int lengthHalfField)
        {
            _miniMoveAnimation = betweenMoveAnimation;
            _stealAnimation = stealAnimation;
            _isCaptureMode = isCapture;
            _stoneCount = stonesPerField * 2 * lengthHalfField;
            Player1Index = 0;
            Player2Index = lengthHalfField + 1;
            _fields = new int[2 * lengthHalfField + 2];
            for (int i = 0; i < _fields.Length; ++i)
                _fields[i] = stonesPerField;
            _fields[Player1Index] = 0;
            _fields[Player2Index] = 0;
            _minStoneCountForWin = _stoneCount / 2 + 1;
            CurrentIsFirst = true;
            UpdateOptions();
        }

        public MancalaGame(MancalaGame copy, bool copyAnimations = true)
        {
            if (copyAnimations)
            {
                _miniMoveAnimation = copy._miniMoveAnimation;
                _stealAnimation = copy._stealAnimation;
            }
            else
            {
                _miniMoveAnimation = i => { };
                _stealAnimation = i => { };
            }

            _isCaptureMode = copy._isCaptureMode;
            _stoneCount = copy._stoneCount;
            Player1Index = copy.Player1Index;
            Player2Index = copy.Player2Index;
            _fields = (int[])copy._fields.Clone();
            for (int i = 0; i < _fields.Length; ++i)
                _fields[i] = copy._fields[i];
            _minStoneCountForWin = copy._minStoneCountForWin;
            CurrentIsFirst = copy.CurrentIsFirst;
            OptionsOfCurrentPlayer = copy.OptionsOfCurrentPlayer;
        }

        public void DoMove(int indexOfField)
        {
            if (CurrentIsFirst)
                DoMove(indexOfField, Player2Index, Player1Index);
            else
                DoMove(indexOfField, Player1Index, Player2Index);
        }

        private void DoMove(int field, int skippedField, int pointsField)
        {
            int count = _fields[field];
            if (count == 0)
                throw new IllegalMoveException("This field cannot be selected");
            _fields[field] = 0;
            while (count > 0)
            {
                field = field == Player1Index ? _fields.Length - 1 : field - 1;
                if (field == skippedField)
                    field = field == Player1Index ? _fields.Length - 1 : field - 1;
                ++_fields[field];
                --count;
                _miniMoveAnimation(field);
            }

            if (field != pointsField)
            {
                if (_isCaptureMode)
                {
                    int indexOpponent = _fields.Length - field;
                    if (_fields[field] == 1 && _fields[indexOpponent] > 0
                                            && (CurrentIsFirst && field > Player1Index && field < Player2Index ||
                                                !CurrentIsFirst && field > Player2Index))
                    {
                        _fields[pointsField] += _fields[field] + _fields[indexOpponent];
                        _fields[field] = 0;
                        _fields[indexOpponent] = 0;
                        _stealAnimation(field);
                    }

                    CurrentIsFirst = !CurrentIsFirst;
                }
                else if (_fields[field] > 0)
                    DoMove(field, skippedField, pointsField);
            }

            UpdateOptions();
        }

        private void UpdateOptions()
        {
            int start, end, otherStart;
            if (CurrentIsFirst)
            {
                start = Player1Index + 1;
                end = Player2Index;
                otherStart = Player2Index + 1;
            }
            else
            {
                start = Player2Index + 1;
                end = _fields.Length;
                otherStart = Player1Index + 1;
            }

            bool otherEmpty = true;
            List<int> options = new List<int>();
            for (int i = start; i < end; i++)
            {
                if (_fields[i] > 0)
                    options.Add(i);
                otherEmpty = otherEmpty && _fields[otherStart++] == 0;
            }

            OptionsOfCurrentPlayer = options;
            if (options.Count > 0 && !otherEmpty) return;
            //A player has no options left -> game is over
            int indexWinner;
            if (CurrentIsFirst && options.Count == 0 || !CurrentIsFirst && otherEmpty)
            {
                start = Player2Index + 1;
                end = _fields.Length;
                indexWinner = Player2Index;
            }
            else
            {
                start = Player1Index + 1;
                end = Player2Index;
                indexWinner = Player1Index;
            }

            for (int i = start; i < end; i++)
            {
                _fields[indexWinner] += _fields[i];
                _fields[i] = 0;
            }

            OptionsOfCurrentPlayer = Array.Empty<int>();
        }

        public override string ToString()
        {
            int digits = (int)ArrayPrinter.GetNeededDigits(_fields);
            return ArrayPrinter.PaddedArrayString(i => _fields[i].ToString(), _fields.Length, digits);
        }

        public object Clone() => new MancalaGame(this, true);
    }
}