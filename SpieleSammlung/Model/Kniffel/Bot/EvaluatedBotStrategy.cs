using System;
using System.Collections.Generic;
using System.Threading;
using SpieleSammlung.Model.Kniffel.Fields;
using SpieleSammlung.Model.Util;

namespace SpieleSammlung.Model.Kniffel.Bot;

public class EvaluatedBotStrategy : BotStrategy
{
    #region Static fields and constants

    public const int REPETITIONS = 50000;
    public const int THREADS = 6;
    private static readonly List<Player> Players = [new(), new()];
    public static Random rng = new();

    #endregion

    #region Private Members

    private MinMaxAvgEvaluator Scores { get; } = new(false);

    #endregion

    #region Constructors

    public EvaluatedBotStrategy()
    {
    }

    public EvaluatedBotStrategy(int bestOptionFinder) : base(bestOptionFinder)
    {
    }

    private EvaluatedBotStrategy(BotStrategy other) : base(other)
    {
    }

    #endregion

    #region Fitness

    public double Fitness => FitnessReady ? Scores.AvgDouble : RecalculateFitness();

    private bool FitnessReady => Scores.Count > 0;

    public double RecalculateFitness(int repetitions = REPETITIONS, int threads = THREADS, Random random = null)
    {
        Func<Random> randomGenerator = random == null ? () => new Random() : () => random;
        if (threads == 1) return RecalculateFitnessSingleThread(repetitions, randomGenerator);
        Thread[] threadPool = new Thread[threads];
        MinMaxAvgEvaluator[] evaluators = new MinMaxAvgEvaluator[threadPool.Length];
        for (int i = 0; i < threadPool.Length; ++i)
        {
            var i1 = i;
            evaluators[i] = new MinMaxAvgEvaluator(false);
            threadPool[i] = new Thread(() =>
                EvaluateNRounds(repetitions / threadPool.Length, evaluators[i1], randomGenerator));
            threadPool[i].Start();
        }

        // Wait for all the threads to finish.
        foreach (Thread thread in threadPool)
            thread.Join();

        Scores.Merge(evaluators);
        return Fitness;
    }

    private double RecalculateFitnessSingleThread(int repetitions, Func<Random> random)
    {
        ProgressPrinter printer = new ProgressPrinter(repetitions);
        for (int i = 0; i < repetitions; ++i)
        {
            EvaluateOneRound(Scores, random());
            printer.PrintProgressIfNecessary(i);
        }

        printer.ClearProgressAndPrintElapsedTime();
        return Fitness;
    }

    private void EvaluateNRounds(int n, MinMaxAvgEvaluator evaluator, Func<Random> random)
    {
        for (int i = 0; i < n; ++i)
        {
            EvaluateOneRound(evaluator, random());
        }
    }

    private void EvaluateOneRound(MinMaxAvgEvaluator evaluator, Random random)
    {
        var game = new KniffelGame(Players, random, this);
        while (game.IsGameNotOver())
            game.DoBotMoveInstant();

        foreach (var player in game.Players)
        {
            evaluator.Insert(player.Fields[KniffelPointsTable.INDEX_SUM].Value);
        }
    }

    #endregion

    #region Mutation

    public EvaluatedBotStrategy CreateMutantWithoutEvaluation()
    {
        var ret = new EvaluatedBotStrategy(this);
        const double probability = 1.0 / 5;
        if (rng.NextDouble() < probability) ret.MutateBestIndexToKillBonusReached();
        if (rng.NextDouble() < probability) ret.MutateBestIndexToKillBonusNotReached();
        if (rng.NextDouble() < probability) MutateValueByDifference(ref ret.MinFieldValueChance, 1);
        if (rng.NextDouble() < probability) MutateValueByDifference(ref ret.MinFieldValuePair3, 1);
        if (rng.NextDouble() < probability) MutateValueByDifference(ref ret.MinFieldValuePair4, 1);
        if (rng.NextDouble() < 0.99) ret.MutateBestOptionFinder();
        return ret;
    }

    private static void MutateValueByDifference(ref int value, int difference)
    {
        if (rng.NextDouble() < 0.5) value += difference;
        else value -= difference;
    }

    public EvaluatedBotStrategy MutateAndEvaluate(int repetitions = REPETITIONS,
        int threads = THREADS,
        Random random = null)
    {
        EvaluatedBotStrategy mutant = CreateMutantWithoutEvaluation();
        mutant.RecalculateFitness(repetitions: repetitions, threads: threads, random: random);
        return mutant;
    }

    private void MutateBestOptionFinder()
    {
        IndexBestOptionFinder = DistinctRandomInt(IndexBestOptionFinder, 5);
    }

    private void MutateBestIndexToKillBonusNotReached()
    {
        int n = rng.Next(1, 6);
        for (int i = 0; i < n; ++i) SwitchTwoRandomIndices(BestIndexToKillBonusReached);
    }

    private void MutateBestIndexToKillBonusReached()
    {
        int n = rng.Next(1, 4);
        for (var i = 0; i < n; ++i) SwitchTwoRandomIndices(BestIndexToKillBonusNotReached);
    }

    private static void SwitchTwoRandomIndices(int[] array)
    {
        int index = rng.Next(0, array.Length);
        int other = DistinctRandomInt(array, index);
        (array[index], array[other]) = (array[other], array[index]);
    }

    private static int DistinctRandomInt(IReadOnlyCollection<int> array, int index) =>
        DistinctRandomInt(index, array.Count);

    private static int DistinctRandomInt(int current, int length) => (rng.Next(1, length) + current) % length;

    #endregion

    public override string ToString() => ToString(FitnessReady);

    public string ToString(bool containFitness)
    {
        return containFitness ? base.ToString() + "\nFitness: " + Fitness : base.ToString();
    }
}