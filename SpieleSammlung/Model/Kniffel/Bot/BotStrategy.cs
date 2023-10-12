using System;
using System.Collections.Generic;
using System.Linq;
using SpieleSammlung.Model.Kniffel.Fields;

namespace SpieleSammlung.Model.Kniffel.Bot
{
    public class BotStrategy
    {
        #region Constructors

        /// <summary>
        /// Creates a new Instance with default values.
        /// </summary>
        public BotStrategy()
        {
        }

        /// <summary>
        /// Creates a new instance as a clone of the other instance.
        /// </summary>
        /// <param name="other">Instance which will be cloned.</param>
        protected BotStrategy(BotStrategy other)
        {
            bestIndexToKillBonusReached = (int[])other.bestIndexToKillBonusReached.Clone();
            bestIndexToKillBonusNotReached = (int[])other.bestIndexToKillBonusNotReached.Clone();
            indexBestOptionFinder = other.indexBestOptionFinder;
        }

        /// <summary>
        /// Creates a new Instance and sets the method of deciding which dices to shuffle.
        /// </summary>
        /// <param name="bestOptionFinder">Index of the possible methods</param>
        public BotStrategy(int bestOptionFinder)
        {
            indexBestOptionFinder = bestOptionFinder;
        }

        #endregion

        #region Fields

        protected readonly int[] bestIndexToKillBonusReached =
        {
            0, 1, 2, KniffelPointsTable.INDEX_KNIFFEL, KniffelPointsTable.INDEX_PAIR_SIZE_4,
            KniffelPointsTable.INDEX_BIG_STREET, KniffelPointsTable.INDEX_FULL_HOUSE,
            KniffelPointsTable.INDEX_SMALL_STREET,
            KniffelPointsTable.INDEX_PAIR_SIZE_3, 3, 4, 5
        };

        protected readonly int[] bestIndexToKillBonusNotReached =
        {
            0, KniffelPointsTable.INDEX_KNIFFEL, KniffelPointsTable.INDEX_PAIR_SIZE_4,
            KniffelPointsTable.INDEX_BIG_STREET, KniffelPointsTable.INDEX_FULL_HOUSE,
            KniffelPointsTable.INDEX_SMALL_STREET,
            KniffelPointsTable.INDEX_PAIR_SIZE_3, 1, 2, 3, 4, 5
        };

        protected int minFieldValuePair3 = 14;
        protected int minFieldValuePair4 = 13;
        protected int minFieldValueChance = 15;

        private static readonly Func<List<ShufflingOption>, ShufflingOption>[] BestOptionFinder =
        {
            options => options.Aggregate((max, x) => max.MaxOptionSum(x)),
            options => options.Aggregate((max, x) => max.MaxOptionAverage(x)),
            options => options.Aggregate((max, x) => max.MaxOptionWithoutChance(x)),
            options => options.Aggregate((max, x) => max.MaxOptionMax(x)),
            options => options.Aggregate((max, x) => max.MaxOptionMaxOrSum(x)),
            options => options.Aggregate((max, x) => max.MaxOptionAverageWrite(x)),
            options => options.Aggregate((max, x) => max.MaxOptionWriteCount(x))
        };

        public static readonly int BEST_OPTION_COUNT = BestOptionFinder.Length;

        protected int indexBestOptionFinder = 4;

        private Func<List<ShufflingOption>, ShufflingOption> BestOption => BestOptionFinder[indexBestOptionFinder];

        #endregion

        #region GeneratingTheNextSteps

        public int[] GenerateIndexToShuffleForNextBestMove(KniffelGame game)
        {
            if (game.AreNoShufflesLeft) throw new NotSupportedException("There are no Shuffles left.");

            return GenerateIndexToShuffleForNextBestMove(game.Dice, game.CurrentPlayer);
        }

        public int[] GenerateIndexToShuffleForNextBestMove(FlatDice dice, KniffelPlayer player)
        {
            if (dice.IsKniffelPossible() && player[KniffelPointsTable.INDEX_KNIFFEL].IsEmpty() ||
                dice.IsBigStreetPossible() && player[KniffelPointsTable.INDEX_BIG_STREET].IsEmpty())
            {
                return Array.Empty<int>(); // there is nothing left to improve
            }

            if (dice.IsSmallStreetPossible() && player[KniffelPointsTable.INDEX_BIG_STREET].IsEmpty())
            {
                return dice.IndexToFlipWhenOptimisingToBigStreet();
            }


            List<ShufflingOption> options = FlatDice.GenerateAllOptions(player, dice);
            ShufflingOption best = BestOption(options);
            if (ModelLog.Writes)
            {
                ModelLog.AppendLine("Current dice: {0}", dice);
                ModelLog.AppendLine("Best option: {0}", best.ToString(true, "\n"));
                ModelLog.AppendSeparatorLine();
            }

            return best.ChosenIndexes;
        }

        public void ChooseBestField(KniffelGame game)
        {
            ShufflingOption option = new(game.Dice.ToDice(), game.indexWritableField, game.indexKillableField);
            if (ModelLog.Writes)
            {
                ModelLog.AppendLine("Bot has/wants to pick from: " + option.ToString(true, "\n"));
                ModelLog.AppendSeparatorLine();
            }

            WriteOption max = option.Max;
            if (option.WriteCount == 0)
            {
                KillBestKillableField(game);
            }
            else if (option.WriteCount == 1 && option[0].Index == KniffelPointsTable.INDEX_CHANCE)
            {
                // only chance available -> take chance or kill another field 
                if (max.Value >= DiceManager.EXPECTED_VALUE_OF_CHANCE - 5 || game.KillableFieldsCount == 0)
                {
                    game.WriteField(max);
                }
                else KillBestKillableField(game);
            }
            else
            {
                switch (max.Index)
                {
                    case KniffelPointsTable.INDEX_FULL_HOUSE:
                    case KniffelPointsTable.INDEX_SMALL_STREET:
                    case KniffelPointsTable.INDEX_BIG_STREET:
                    case KniffelPointsTable.INDEX_KNIFFEL:
                        game.WriteField(max);
                        return;
                }

                WriteOption best = option[0];
                int valueDifMax = DifToMinFieldValue(option[0]);
                for (int i = 1; i < option.WriteCount; ++i)
                {
                    if (option[i].Index <= KniffelPointsTable.INDEX_PAIR_SIZE_4 ||
                        option[i].Index == KniffelPointsTable.INDEX_CHANCE)
                    {
                        int valueDif = DifToMinFieldValue(option[i]);
                        if (valueDif > valueDifMax)
                        {
                            valueDifMax = valueDif;
                            best = option[i];
                        }
                    }
                }

                game.WriteField(best);
            }
        }

        private void KillBestKillableField(KniffelGame game)
        {
            int[] index = game.CurrentPlayer.HasReachedBonus()
                ? bestIndexToKillBonusReached
                : bestIndexToKillBonusNotReached;
            int field = 0;
            while (!game.CurrentPlayer[index[field]].IsEmpty())
            {
                ++field;
            }

            ModelLog.AppendLine("Bot decided to remove index: {0}", index[field]);
            game.KillFieldGlobalIndex(index[field]);
        }

        private int DifToMinFieldValue(WriteOption option) => option.Value - MinFieldValue(option.Index);

        private int MinFieldValue(int index)
        {
            return index switch
            {
                < 6 => (index + 1) * 3,
                KniffelPointsTable.INDEX_PAIR_SIZE_3 => minFieldValuePair3,
                KniffelPointsTable.INDEX_PAIR_SIZE_4 => minFieldValuePair4,
                KniffelPointsTable.INDEX_CHANCE => minFieldValueChance,
            };
        }

        #endregion

        public override string ToString()
        {
            return "Not reached: " + string.Join(", ", bestIndexToKillBonusNotReached) +
                   "\nreached: " + string.Join(", ", bestIndexToKillBonusReached) +
                   "\nIndex: " + indexBestOptionFinder + "\nMin values: {Chance " + minFieldValueChance +
                   ", Pair 3: " + minFieldValuePair3 + ", Pair 4: " + minFieldValuePair4 + "}";
        }
    }
}