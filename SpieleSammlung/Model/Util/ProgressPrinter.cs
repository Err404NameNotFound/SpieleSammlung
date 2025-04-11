using System;
using System.Diagnostics;

namespace SpieleSammlung.Model.Util;

public class ProgressPrinter
{
    private const string CLEAR_STRING = "\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b" +
                                        "\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b" +
                                        "\b\b\b\b\b\b\b\b\b\b\b";

    private static readonly string FormatStringBetween = "{0,-" + CLEAR_STRING.Length + "}";

    private const string CLEAR_STRING_AFTER = "                           ";
    private const int BUFFER_SIZE = 60;
    private const long UPDATE_EVERY_SECOND = 1000;
    private const long DAY = 24 * 3600 * 1000;
    private const long HOUR = 3600 * 1000;
    private const long MINUTE = 60 * 1000;
    private const long SECOND = 1000;
    private readonly long _startTime;
    private readonly bool _byTime;
    private readonly long _end;
    private readonly RingBufferFifo<long> _timeBuffer;
    private readonly RingBufferFifo<long> _iterationBuffer;
    private readonly Stopwatch _watch;
    private readonly long _stepSize;
    private long _nextUpdate;

    private ProgressPrinter(long end, bool byTime)
    {
        _end = end;
        _watch = new Stopwatch();
        _startTime = _watch.ElapsedMilliseconds;
        _watch.Start();
        _timeBuffer = new RingBufferFifo<long>(BUFFER_SIZE, _startTime);
        _iterationBuffer = new RingBufferFifo<long>(BUFFER_SIZE, 0L);
        _byTime = byTime;
    }

    private ProgressPrinter(long end, long nextUpdate, bool byTime) : this(end, byTime)
    {
        if (byTime)
        {
            _stepSize = nextUpdate;
            _nextUpdate = nextUpdate + Now();
        }
        else
        {
            _stepSize = Math.Max(nextUpdate, 1);
            _nextUpdate = _stepSize - 1;
        }
    }

    public ProgressPrinter(long end) : this(end, UPDATE_EVERY_SECOND, true)
    {
    }

    public ProgressPrinter(long end, long stepSize) : this(end, stepSize, false)
    {
    }

    private long Now() => _watch.ElapsedMilliseconds;

    private static string TimeString(long epochSeconds)
    {
        long days = epochSeconds / DAY;
        epochSeconds -= DAY * days;
        long hours = epochSeconds / HOUR;
        epochSeconds -= hours * HOUR;
        long minutes = epochSeconds / MINUTE;
        long seconds = (epochSeconds - minutes * MINUTE) / SECOND;
        return $"{days}d{hours:00}:{minutes:00}:{seconds:00}";
    }

    public void PrintProgressIfNecessary(long i, string between = null)
    {
        if (_byTime && Now() >= _nextUpdate || !_byTime && i >= _nextUpdate)
        {
            PrintProgress(i, between);
            _nextUpdate += _stepSize;
        }
    }

    private void PrintProgress(long i, string between)
    {
        ++i;
        long now = Now();
        long elapsedTime = now - _startTime;
        long expectedTime = (now - _timeBuffer.Peek()) * (_end - i) / (i - _iterationBuffer.Peek());
        _timeBuffer.Insert(now);
        _iterationBuffer.Insert(i);
        Console.Write(CLEAR_STRING);
        if (between != null)
        {
            Console.WriteLine(FormatStringBetween, between);
        }

        Console.Write(CLEAR_STRING + "{0,5}% | elapsed: {1} | finished: {2}", $"{100.0 * i / _end:00.0}",
            TimeString(elapsedTime), TimeString(expectedTime));
    }

    public void ClearProgressAndPrintElapsedTime()
    {
        Console.WriteLine(CLEAR_STRING + "Elapsed time: {0}" + CLEAR_STRING_AFTER, TimeString(Now() - _startTime));
        _watch.Stop();
    }

    public string GetElapsedTime() => $"Elapsed time: {TimeString(Now() - _startTime)}\n";
}