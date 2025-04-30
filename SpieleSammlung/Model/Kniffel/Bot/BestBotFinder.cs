#region

using System;
using SpieleSammlung.Model.Util;

#endregion

namespace SpieleSammlung.Model.Kniffel.Bot;

public static class BestBotFinder
{
    public static int TestAllCount;
    public static int TestOneCount;
    public static int Threads;
    public static int Repetitions;
    public static Func<bool> ShouldStop;

    static BestBotFinder()
    {
        TestAllCount = 10;
        TestOneCount = 10;
        Threads = EvaluatedBotStrategy.THREADS;
        Repetitions = EvaluatedBotStrategy.REPETITIONS;
        ShouldStop = () => Console.KeyAvailable;
    }

    public static void main()
    {
        bool console = ModelLog.WriteToConsole;
        bool file = ModelLog.WriteToFile;
        ModelLog.WriteToConsole = false;
        ModelLog.WriteToFile = false;
        TestAll(TestAllCount);
        OptimiseOneStrategy(TestOneCount);
        ModelLog.WriteToConsole = console;
        ModelLog.WriteToFile = file;
    }

    public static void OptimiseOneStrategy(int count = 100) =>
        OptimiseOneStrategy(new EvaluatedBotStrategy(), count);

    private static void OptimiseOneStrategy(EvaluatedBotStrategy start, int count)
    {
        ProgressPrinter printer = new ProgressPrinter(count, 1);
        EvaluatedBotStrategy best = start;
        Console.WriteLine(best.RecalculateFitness(Repetitions, Threads));
        for (int i = 1; i < count; i++)
        {
            if (ShouldStop()) break;
            EvaluatedBotStrategy next = best.MutateAndEvaluate(Repetitions, Threads);
            if (next.Fitness > best.Fitness) best = next;
            string between = $"{next.Fitness:000.00000}; {best.Fitness:000.00000}";
            printer.PrintProgressIfNecessary(i, between);
        }

        printer.ClearProgressAndPrintElapsedTime();

        Console.WriteLine(best);
    }

    public static void TestAll(int count = 10)
    {
        MinMaxAvgEvaluator[] evaluators = new MinMaxAvgEvaluator[BotStrategy.BEST_OPTION_COUNT];
        EvaluatedBotStrategy[] bests = new EvaluatedBotStrategy[evaluators.Length];
        for (int i = 0; i < evaluators.Length; ++i)
        {
            evaluators[i] = new MinMaxAvgEvaluator(false);
            bests[i] = new EvaluatedBotStrategy(i);
            bests[i].RecalculateFitness(Repetitions, Threads);
        }

        ProgressPrinter printer = new ProgressPrinter(count * evaluators.Length, 1);
        for (int i = 0; i < count; ++i)
        {
            if (ShouldStop()) break;
            for (int e = 0; e < evaluators.Length; e++)
            {
                EvaluatedBotStrategy next = bests[e].MutateAndEvaluate(Repetitions, Threads);
                if (next.Fitness > bests[e].Fitness) bests[e] = next;
                string between = $"{e}: {next.Fitness:000.00000}; {bests[e].Fitness:000.00000}";
                printer.PrintProgressIfNecessary(i * evaluators.Length + e, between);
                evaluators[e].Insert((int)next.Fitness);
            }
        }

        printer.ClearProgressAndPrintElapsedTime();
        ModelLog.WriteToConsole = true;
        MinMaxAvgEvaluator.PrintMultipleNonNegative(evaluators);
        ModelLog.WriteToConsole = false;
    }
}

/*
count;     01;    01;    01;    01;    01;    01
min  ;    190;   190;   184;   186;   153;   107
max  ;    190;   190;   184;   186;   153;   107
avg  ;    190;   190;   184;   186;   153;   107
sum  ;    190;   190;   184;   186;   153;   107
------------------------------------------
Elapsed time: 0d00:16:53
count;    10;    10;    10;    10;    10;    10;    10
avg  ;   214;   214;   217;   218;   218;   166;   159
max  ;   215;   216;   220;   218;   219;   169;   161
min  ;   212;   212;   214;   216;   217;   161;   156
sum  ; 2.138; 2.138; 2.167; 2.175; 2.177; 1.664; 1.593


Not reached: 0, 13, 9, 12, 10, 11, 8, 1, 2, 3, 4, 5
reached: 0, 1, 2, 13, 9, 12, 10, 11, 8, 3, 4, 5
Index: 2
Min values: {Chance 15, Pair 3: 14, Pair 4: 13}
Fitness: 219,7807

 */