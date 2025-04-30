#region

using System;
using System.Collections.Generic;

#endregion

namespace SpieleSammlungTests.Utils;

public class RandomStub : Random
{
    private readonly Queue<double> _doubles = new();
    private readonly Queue<int> _ints = new();
    private bool _constantDouble;
    private double _constantDoubleValue;
    private bool _constantInt;
    private int _constantIntValue;

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