using System;
using System.Collections.Generic;

namespace SpieleSammlungTests.Utils
{
    public class RandomStub : Random
    {
        private readonly Queue<int> _ints = new();
        private readonly Queue<double> _doubles = new();
        private bool _constantInt;
        private bool _constantDouble;
        private int _constantIntValue;
        private double _constantDoubleValue;

        public RandomStub(params int[] values) => SetNext(values);
        public RandomStub(IEnumerable<int> values) => SetNext(values);

        public void SetNext(params int[] values)
        {
            foreach (int value in values) _ints.Enqueue(value);
        }

        public void SetNext(params double[] values)
        {
            foreach (double value in values) _doubles.Enqueue(value);
        }

        private void SetNext(IEnumerable<int> values)
        {
            foreach (int value in values) _ints.Enqueue(value);
        }

        public void SetOutputConstant(int value)
        {
            _constantInt = true;
            _constantIntValue = value;
        }

        public void SetNextDoubleOutputConstant(double value)
        {
            _constantDouble = true;
            _constantDoubleValue = value;
        }

        public void ClearOutputConstant()
        {
            _constantInt = false;
            _constantDouble = false;
        }

        public override int Next(int minValue, int maxValue)
        {
            return _constantInt ? _constantIntValue : _ints.Count > 0 ? _ints.Dequeue() : base.Next(minValue, maxValue);
        }

        public override double NextDouble()
        {
            return _constantDouble ? _constantDoubleValue : _doubles.Count > 0 ? _doubles.Dequeue() : base.Next();
        }

        public void ClearQueues()
        {
            _ints.Clear();
            _doubles.Clear();
        }
    }
}