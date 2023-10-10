using System;
using System.Collections.Generic;
using SpieleSammlung.Model.Kniffel.Bot;
using SpieleSammlung.Model.Kniffel.Fields;

namespace SpieleSammlung.Model.Kniffel
{
    /// <summary>For playing a Kniffel match.</summary>
    public class KniffelGame
    {
        #region constants and static fields

        /// <summary>This is the minimum amount of players for a game. Otherwise the game cannot be initiated.</summary>
        public const int MIN_PLAYER_COUNT = 2;

        /// <summary>Amount of times a player can shuffle the dices.</summary>
        public const int INITIAL_SHUFFLE_COUNT = 3;

        public const int VALUE_FULL_HOUSE = 25;
        public const int VALUE_SMALL_STREET = 30;
        public const int VALUE_BIG_STREET = 40;
        public const int VALUE_KNIFFEL = 50;

        #endregion

        #region properties

        /// <value>Number of the current round.</value>
        public int Round { private set; get; }

        /// <value>Index of the current player.</value>
        public int ActivePlayer { private set; get; }

        /// <value>Amount of times the current player can roll the dice again.</value>
        public int RemainingShuffles { private set; get; }

        /// <value>States if current player can roll the dice again.</value>
        public bool AreShufflesLeft => RemainingShuffles > 0;

        /// <value>States if current player can not roll the dice again.</value>
        public bool AreNoShufflesLeft => RemainingShuffles <= 0;

        /// <value>Players of this match.</value>
        public List<KniffelPlayer> Players { get; }

        /// <summary>The Current Player of this match</summary>
        public KniffelPlayer CurrentPlayer => Players[ActivePlayer];

        /// <summary>A copy of the Dice of this game</summary>
        public DiceManager Dice => new(_dice);

        #endregion

        #region private members

        /// <summary>List of fields that can only be killed not written (-> value 0 is written in field).</summary>
        internal readonly List<int> indexKillableField;

        /// <summary>List of fields that can be written with a value greater than 0.</summary>
        internal readonly List<WriteOption> indexWritableField;

        /// <summary>Manages the dices.</summary>
        private readonly DiceManager _dice;

        /// <summary>For generating additional dice values without interfering with the rng of the actual game.</summary>
        private readonly DiceManager _diceGenerator;

        private readonly BotStrategy _botStrategy;

        #endregion

        #region constructors

        /// <summary>Creates a new instance and set up a playable game.</summary>
        /// <param name="names">Names of the players.</param>
        public KniffelGame(IReadOnlyCollection<Player> names) : this(names, new DiceManager())
        {
        }

        /// <summary>
        /// Starts a new Kniffel game with the given Seed.
        /// </summary>
        /// <param name="names">Player for this Kniffel game</param>
        /// <param name="seed">Seed for the RNG</param>
        public KniffelGame(IReadOnlyCollection<Player> names, int seed) : this(names, new DiceManager(seed))
        {
        }

        /// <summary>
        /// Starts a new Kniffel game with the given Seed.
        /// </summary>
        /// <param name="names">Player for this Kniffel game</param>
        /// <param name="rng">The random number generator that produces the values for the dice</param>
        public KniffelGame(IReadOnlyCollection<Player> names, Random rng) : this(names, new DiceManager(rng))
        {
        }

        /// <summary>
        /// Starts a new Kniffel game with the given DiceManager.
        /// </summary>
        /// <param name="names">The players participating in this match</param>
        /// <param name="manager">The DiceManager managing the dice during the match</param>
        private KniffelGame(IReadOnlyCollection<Player> names, DiceManager manager) : this(names, manager, new BotStrategy())
        {
        }

        public KniffelGame(IReadOnlyCollection<Player> names, BotStrategy bot) : this(names, new DiceManager(), bot)
        {
        }


        public KniffelGame(IReadOnlyCollection<Player> names, Random random, BotStrategy bot)
            : this(names, new DiceManager(random), bot)
        {
        }

        private KniffelGame(IReadOnlyCollection<Player> names, DiceManager manager, BotStrategy strategy)
        {
            _botStrategy = strategy;
            _diceGenerator = new DiceManager();
            _dice = manager;
            indexKillableField = new List<int>();
            indexWritableField = new List<WriteOption>();
            if (names.Count < MIN_PLAYER_COUNT)
            {
                throw new ArgumentException("There are not enough players in this list. " +
                                            $"A kniffel game needs at least {MIN_PLAYER_COUNT} players");
            }

            Players = new List<KniffelPlayer>();
            foreach (var player in names)
            {
                Players.Add(new KniffelPlayer(player));
            }

            Round = 0;
            ActivePlayer = 0;
            RemainingShuffles = INITIAL_SHUFFLE_COUNT;
            Shuffle();
        }

        #endregion

        #region getter

        /// <summary>Value of the dice at the given index.</summary>
        /// <param name="i">Index of the dice</param>
        /// <returns>Value of the dice</returns>
        public int GetDiceValue(int i) => _dice[i];

        /// <value>Count of the fields that can be killed / written with value 0.</value>
        public int KillableFieldsCount => indexKillableField.Count;

        /// <summary>Count of the fields that can be written with a value greater than 0.</summary>
        public int WriteableFieldsCount => indexWritableField.Count;

        /// <summary>
        /// Returns the index of a killable field that is at position index.
        /// </summary>
        /// <param name="index">Index of Field in the list of killable fields.</param>
        /// <returns>Index of the field.</returns>
        public int GetKillableFieldIndex(int index) => indexKillableField[index];

        /// <summary>Returns the index of a writable field that is at position <paramref name="index"/>.</summary>
        /// <param name="index">Index of the field in the list of writeable fields.</param>
        /// <returns>Index of the field.</returns>
        public WriteOption GetWriteableFieldsIndex(int index) => indexWritableField[index];

        /// <summary>Flag if the game is still running.</summary>
        /// <returns><c>true</c> if the game is still running.</returns>
        public bool IsGameNotOver() => Round <= 12;

        #endregion

        #region game logic

        /// <summary>Sets the value of the field at the given index to 0.</summary>
        /// <param name="index">Index of the field to be killed.</param>
        public void KillFieldGlobalIndex(int index) => WriteField(index, 0);

        /// <summary>Sets the value of the field at the given index to 0.</summary>
        /// <param name="index">Index of the fields within the index of all fields that can only be killed.</param>
        public void KillFieldOption(int index) => WriteField(indexKillableField[index], 0);

        /// <summary>Sets the value of the field at the given index to the possible value greater than 0.</summary>
        /// <param name="index">Index of the field to be written.</param>
        public void WriteField(int index) => WriteField(indexWritableField[index]);

        internal void WriteField(WriteOption option) => WriteField(option.Index, option.Value);

        private void WriteField(int index, int value)
        {
            Players[ActivePlayer].Fields[index].Value = value;
            NextPlayer();
        }


        /// <summary>Switches to the next Player.</summary>
        private void NextPlayer()
        {
            Players[ActivePlayer].Fields.UpdateSums();
            RemainingShuffles = INITIAL_SHUFFLE_COUNT;
            ++ActivePlayer;
            ActivePlayer %= Players.Count;
            if (ActivePlayer == 0)
            {
                ++Round;
            }

            if (IsGameNotOver())
            {
                Shuffle();
            }
        }

        /// <summary>Rolls all dices.</summary>
        public void Shuffle() => Shuffle(null);

        /// <summary>Rolls the dices with the corresponding indexes.</summary>
        /// <param name="index">Indexes of the dices to be rolled.</param>
        public void Shuffle(int[] index)
        {
            if (RemainingShuffles <= 0)
            {
                throw new IllegalMoveException("The current player has no shuffles left");
            }

            if (index == null)
            {
                _dice.Shuffle();
            }
            else
            {
                _dice.Shuffle(index);
            }

            --RemainingShuffles;
            CalculateOptions();
        }

        #endregion

        #region calculation of options

        /// <summary>Adds a new entry to the killable/writeable fields.</summary>
        /// <param name="canWrite"><c>true</c> if field can be written with a value greater than 0.</param>
        /// <param name="index">Index of the new entry.</param>
        /// <param name="value">Value of the new entry.</param>
        private void SetValue(bool canWrite, int index, int value)
        {
            if (canWrite)
            {
                indexWritableField.Add(new WriteOption(index, value));
            }
            else
            {
                indexKillableField.Add(index);
            }
        }

        private void UpdateOption(bool isPossible, int index, int value)
        {
            if (Players[ActivePlayer].Fields[index].IsEmpty())
            {
                SetValue(isPossible, index, value);
            }
        }

        private void CalculateOptions()
        {
            indexKillableField.Clear();
            indexWritableField.Clear();
            for (int i = 0; i < 6; i++)
            {
                int sum = _dice.GetCountOfValue(i) * (i + 1);
                UpdateOption(sum != 0, i, sum);
            }

            UpdateOption(_dice.IsPairOfSizePossible(3), KniffelPointsTable.INDEX_PAIR_SIZE_3, _dice.SumOfAllDices);
            UpdateOption(_dice.IsPairOfSizePossible(4), KniffelPointsTable.INDEX_PAIR_SIZE_4, _dice.SumOfAllDices);
            UpdateOption(_dice.IsFullHousePossible(), KniffelPointsTable.INDEX_FULL_HOUSE, VALUE_FULL_HOUSE);
            UpdateOption(_dice.IsSmallStreetPossible(), KniffelPointsTable.INDEX_SMALL_STREET, VALUE_SMALL_STREET);
            UpdateOption(_dice.IsBigStreetPossible(), KniffelPointsTable.INDEX_BIG_STREET, VALUE_BIG_STREET);
            UpdateOption(_dice.IsKniffelPossible(), KniffelPointsTable.INDEX_KNIFFEL, VALUE_KNIFFEL);
            UpdateOption(true, KniffelPointsTable.INDEX_CHANCE, _dice.SumOfAllDices);
        }

        #endregion

        #region finding best options for the bot

        // public int[] GenerateIndexToShuffleForNextBestMove()
        // {
        //     if (RemainingShuffles <= 0)
        //     {
        //         throw new NotSupportedException("There are no Shuffles left.");
        //     }
        //
        //     List<ShufflingOption> options = GenerateAllOptions();
        //
        //     // TODO entfernen start 
        //     //ShufflingOption bestMax = options.Aggregate((max, x) => x.Max.ValueD > max.Max.ValueD ? x : max);
        //     //ShufflingOption bestAverage = BestOptionAverage(options);
        //     //ShufflingOption bestAverageWrite = BestOptionAverageWrite(options);
        //     //ShufflingOption bestWriteCount = options.Aggregate((max, x) => x.WriteCount > max.WriteCount ? x : max);
        //     //ShufflingOption bestSum = options.Aggregate((max, x) => x.Sum > max.Sum ? x : max);
        //     // TODO entfernen ende
        //     ShufflingOption bestMaxWOChance = BestOptionMaxWOChance(options);
        //     ModelLog.AppendLine("Current dice: " + dice);
        //     ModelLog.AppendLine("Best option: " + bestMaxWOChance.ToString(true, "\n"));
        //     ModelLog.AppendSeparatorLine();
        //     return bestMaxWOChance.ChosenIndexes;
        // }
        //
        // public void ChooseBestField()
        // {
        //     ShufflingOption option = new ShufflingOption(dice, indexWritableField, indexKillableField);
        //     ModelLog.AppendLine("Algorithm chose option: " + option.ToString(true, "\n"));
        //     ModelLog.AppendSeparatorLine();
        //     WriteOption max = option.MaxWithoutChance;
        //     if (max == null)
        //     {
        //         KillBestKillableField();
        //     }
        //     else if (max.Index == KniffelPointsTable.INDEX_CHANCE)
        //     {
        //         // only chance available -> take chance or kill another field 
        //         if (max.Value >= DiceManager.EXPECTED_VALUE_OF_CHANCE - 5)
        //         {
        //             WriteField(max);
        //         }
        //         else
        //         {
        //             KillBestKillableField();
        //         }
        //     }
        //     else
        //     {
        //         switch (max.Index)
        //         {
        //             case KniffelPointsTable.INDEX_FULL_HOUSE:
        //             case KniffelPointsTable.INDEX_SMALL_STREET:
        //             case KniffelPointsTable.INDEX_BIG_STREET:
        //             case KniffelPointsTable.INDEX_KNIFFEL:
        //                 WriteField(max);
        //                 return;
        //         }
        //
        //         WriteOption best = option[0];
        //         int valueDifMax = MinFieldValue(option[0].Index);
        //         for (int i = 0; i < option.WriteCount; ++i)
        //         {
        //             if (option[i].Index < KniffelPointsTable.INDEX_PAIR_SIZE_4 ||
        //                 option[i].Index == KniffelPointsTable.INDEX_CHANCE)
        //             {
        //                 int valueDif = option[i].Value - MinFieldValue(option[i].Index);
        //                 if (valueDif > valueDifMax)
        //                 {
        //                     valueDifMax = valueDif;
        //                     best = option[i];
        //                 }
        //             }
        //         }
        //
        //         WriteField(best);
        //     }
        // }
        //
        // private void KillBestKillableField()
        // {
        //     int[] index;
        //     if (CurrentPlayer[KniffelPointsTable.INDEX_SUM_TOP].Value > KniffelPointsTable.MIN_TOP6_FOR_BONUS)
        //     {
        //         index = new[]
        //         {
        //             0, 1, 2, KniffelPointsTable.INDEX_KNIFFEL, KniffelPointsTable.INDEX_PAIR_SIZE_4,
        //             KniffelPointsTable.INDEX_BIG_STREET, KniffelPointsTable.INDEX_FULL_HOUSE,
        //             KniffelPointsTable.INDEX_SMALL_STREET,
        //             KniffelPointsTable.INDEX_PAIR_SIZE_3, 3, 4, 5
        //         };
        //     }
        //     else
        //     {
        //         index = new[]
        //         {
        //             0, KniffelPointsTable.INDEX_KNIFFEL, KniffelPointsTable.INDEX_PAIR_SIZE_4,
        //             KniffelPointsTable.INDEX_BIG_STREET, KniffelPointsTable.INDEX_FULL_HOUSE,
        //             KniffelPointsTable.INDEX_SMALL_STREET,
        //             KniffelPointsTable.INDEX_PAIR_SIZE_3, 1, 2, 3, 4, 5
        //         };
        //     }
        //
        //     int field = 0;
        //     while (CurrentPlayer[index[field]].IsEmpty())
        //     {
        //         ++field;
        //     }
        //
        //     WriteField(index[field], 0);
        // }
        //
        // private int MinFieldValue(int index)
        // {
        //     return index switch
        //     {
        //         < 6 => (index + 1) * 3,
        //         KniffelPointsTable.INDEX_PAIR_SIZE_3 => 20,
        //         KniffelPointsTable.INDEX_PAIR_SIZE_4 => 19,
        //         KniffelPointsTable.INDEX_CHANCE => DiceManager.LOWEST_VALUE * DiceManager.DICE_COUNT,
        //         _ => throw new ArgumentException("this field is not supported by this method")
        //     };
        // }
        //
        // private ShufflingOption BestOptionAverageWrite(List<ShufflingOption> options)
        // {
        //     return options.Aggregate((max, x) => x.Average > max.Average ? x : max);
        // }
        //
        // private ShufflingOption BestOptionAverage(List<ShufflingOption> options)
        // {
        //     return options.Aggregate((max, x) => x.Average > max.Average ? x : max);
        // }
        //
        // private ShufflingOption BestOptionMaxWOChance(List<ShufflingOption> options)
        // {
        //     return options.Aggregate((max, x) => max.MaxOption(x));
        // }
        //
        // private List<ShufflingOption> GenerateAllOptions(DiceManager dice, List<ShufflingOption> list, int start)
        // {
        //     list.Add(new ShufflingOption(dice, CalculateExpectedValues(dice)));
        //     for (int i = start; i < DiceManager.DICE_COUNT; ++i)
        //     {
        //         GenerateAllOptions(new DiceManager(dice).Remove(i), list, i + 1);
        //     }
        //
        //     return list;
        // }
        //
        // public List<ShufflingOption> GenerateAllOptions()
        // {
        //     if (RemainingShuffles > 0)
        //     {
        //         return GenerateAllOptions(dice, new List<ShufflingOption>(POSSIBLE_COMBINATIONS), 0);
        //     }
        //
        //     List<ShufflingOption> ret = new List<ShufflingOption> { new(dice, CalculateExpectedValues(dice)) };
        //     return ret;
        // }
        //
        // public List<WriteOption> CalculateExpectedValues(DiceManager dice)
        // {
        //     List<WriteOption> options = new List<WriteOption>();
        //     for (int i = 0; i < 6; ++i)
        //     {
        //         Add(options, i, dice.EOfTop6(i + 1));
        //     }
        //
        //     Add(options, KniffelPointsTable.INDEX_PAIR_SIZE_3, dice.EOfPair(3));
        //     Add(options, KniffelPointsTable.INDEX_PAIR_SIZE_4, dice.EOfPair(4));
        //     Add(options, KniffelPointsTable.INDEX_FULL_HOUSE, dice.POfFullHouse() * VALUE_FULL_HOUSE);
        //     Add(options, KniffelPointsTable.INDEX_SMALL_STREET, dice.POfSmallStreet() * VALUE_SMALL_STREET);
        //     Add(options, KniffelPointsTable.INDEX_BIG_STREET, dice.POfBigStreet() * VALUE_BIG_STREET);
        //     Add(options, KniffelPointsTable.INDEX_KNIFFEL, dice.POfKniffel() * VALUE_KNIFFEL);
        //     Add(options, KniffelPointsTable.INDEX_CHANCE, dice.EOfChance());
        //     return options;
        // }
        //
        // private void Add(List<WriteOption> options, int index, double value)
        // {
        //     if (CurrentPlayer[index].IsEmpty())
        //     {
        //         options.Add(new WriteOption(index, value));
        //     }
        // }

        #endregion

        public int RandomDiceValue()
        {
            return _diceGenerator.GenerateDiceValue();
        }

        public void DoBotMoveInstant()
        {
            DoBotMove(_ => { }, () => { });
        }

        public void DoBotMove(Action<int[]> shuffleAnimation, Action displayShuffledDice)
        {
            int[] selected;
            while (AreShufflesLeft && (selected = _botStrategy.GenerateIndexToShuffleForNextBestMove(this)).Length > 0)
            {
                shuffleAnimation(selected);
                Shuffle(selected);
                displayShuffledDice();
            }

            _botStrategy.ChooseBestField(this);
        }
    }
}