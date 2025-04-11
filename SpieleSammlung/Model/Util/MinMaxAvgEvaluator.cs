using System;

namespace SpieleSammlung.Model.Util;

public class MinMaxAvgEvaluator(bool canBeNegative)
{
    public long Min { get; private set; } = long.MaxValue;
    public long Max { get; private set; } = long.MinValue;
    public long Sum { get; private set; }
    public long Count { get; private set; }
    public long AbsMin { get; private set; }
    public long AbsMax { get; private set; }
    public long AbsSum { get; private set; }
    public long Avg => Count == 0 ? 0 : Sum / Count;
    public double AvgDouble => Count == 0 ? 0.0 : (double)Sum / Count;

    public static void PrintMultipleNonNegative(params MinMaxAvgEvaluator[] evaluators)
    {
        long maxSum = MathHelp.Max(i => evaluators[i].Sum, evaluators.Length);
        int digits = (int)ArrayPrinter.GetNeededDigitsSpaced(maxSum);
        PrintMultipleNonNegative(digits, evaluators);
    }

    public static void PrintMultipleNonNegative(int digits, params MinMaxAvgEvaluator[] evaluators)
    {
        PrintCount("count;", digits, evaluators);
        PrintMultipleNonNegative("min  ;", "max  ;", "avg  ;", digits, evaluators);
        PrintSum("sum  ;", digits, evaluators);
    }

    public static void PrintMultipleNonNegative(string min, string max, string avg, int digits,
        params MinMaxAvgEvaluator[] evaluators)
    {
        PrintAvg(avg, digits, evaluators);
        PrintMax(max, digits, evaluators);
        PrintMin(min, digits, evaluators);
    }

    private static void PrintSum(string sum, int digits, params MinMaxAvgEvaluator[] evaluators)
    {
        ArrayPrinter.PrintArray(sum,
            i => evaluators[i].Count == 0 ? "-" : $"{evaluators[i].Sum:0,0}",
            evaluators.Length, digits);
    }

    private static void PrintCount(string count, int digits, params MinMaxAvgEvaluator[] evaluators)
    {
        ArrayPrinter.PrintArray(count,
            i => evaluators[i].Count == 0 ? "-" : $"{evaluators[i].Count:0,0}", evaluators.Length,
            digits);
    }

    private static void PrintMin(string min, int digits, params MinMaxAvgEvaluator[] evaluators)
    {
        ArrayPrinter.PrintArray(min,
            i => evaluators[i].Count == 0 ? "-" : $"{evaluators[i].Min:0,0}",
            evaluators.Length, digits);
    }

    private static void PrintMax(string max, int digits, params MinMaxAvgEvaluator[] evaluators)
    {
        ArrayPrinter.PrintArray(max,
            i => evaluators[i].Count == 0 ? "-" : $"{evaluators[i].Max:0,0}",
            evaluators.Length, digits);
    }

    private static void PrintAvg(string avg, int digits, params MinMaxAvgEvaluator[] evaluators)
    {
        ArrayPrinter.PrintArray(avg, i => evaluators[i].Count == 0
                ? "-"
                : $"{Math.Round((float)evaluators[i].Sum / evaluators[i].Count):0,0}",
            evaluators.Length, digits);
    }

    public void Insert(long value)
    {
        ++Count;
        Sum += value;
        if (value > Max)
        {
            Max = value;
        }

        if (value < Min)
        {
            Min = value;
        }

        if (canBeNegative)
        {
            value = Math.Abs(value);
            AbsSum += value;
            if (value > AbsMax)
            {
                AbsMax = value;
            }

            if (value < AbsMin)
            {
                AbsMin = value;
            }
        }
    }

    public void PrintEvaluation()
    {
        if (Count == 0)
        {
            ModelLog.Append("Nothing to evaluate, because count == 0");
        }
        else
        {
            ModelLog.AppendLine("count; {0:0,0}", Count);
            ModelLog.AppendLine("min  ; {0:0,0}", Min);
            ModelLog.AppendLine("max  ; {0:0,0}", Max);
            ModelLog.AppendLine("sum  ; {0:0,0}", Sum);
            ModelLog.AppendLine("avg  ; {0:0,0}", Sum / Count);
            if (!canBeNegative) return;
            ModelLog.AppendLine("Stats of absolute values;");
            ModelLog.AppendLine("min  ; {0:0,0}", AbsMin);
            ModelLog.AppendLine("max  ; {0:0,0}", AbsMax);
            ModelLog.AppendLine("sum  ; {0:0,0}", AbsSum);
            ModelLog.AppendLine("avg  ; {0:0,0}", AbsSum / Count);
        }
    }

    public void Merge(MinMaxAvgEvaluator evaluator)
    {
        Min = Math.Min(Min, evaluator.Min);
        Max = Math.Max(Max, evaluator.Max);
        Sum += evaluator.Sum;
        Count += evaluator.Count;
        AbsMin = Math.Min(AbsMin, evaluator.AbsMin);
        AbsMax = Math.Max(AbsMax, evaluator.AbsMax);
        AbsSum += evaluator.AbsSum;
    }

    public void Merge(params MinMaxAvgEvaluator[] evaluators)
    {
        foreach (var evaluator in evaluators)
        {
            Merge(evaluator);
        }
    }
}