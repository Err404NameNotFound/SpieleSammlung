using System;
using System.Collections.Generic;

namespace SpieleSammlungTests.Utils
{
    public class RandomStub : Random
    {
        private readonly Queue<int> _queue = new();
        private bool _constant;
        private int _constantValue;

        public RandomStub(params int[] values) => SetNext(values);
        public RandomStub(IEnumerable<int> values) => SetNext(values);

        public void SetNext(params int[] values)
        {
            foreach (int value in values) _queue.Enqueue(value);
        }

        private void SetNext(IEnumerable<int> values)
        {
            foreach (int value in values) _queue.Enqueue(value);
        }

        public void SetOutputConstant(int value)
        {
            _constant = true;
            _constantValue = value;
        }

        public void ClearOutputConstant()
        {
            _constant = false;
        }

        public override int Next(int minValue, int maxValue)
        {
            return _constant ? _constantValue : _queue.Count > 0 ? _queue.Dequeue() : base.Next(minValue, maxValue);
        }

        public void ClearQueue()
        {
            _queue.Clear();
        }
    }
}