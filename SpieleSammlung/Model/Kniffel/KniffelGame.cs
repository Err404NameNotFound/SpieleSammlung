using System;
using System.Collections.Generic;
using System.Linq;
using SpieleSammlung.Model.Kniffel.Bot;
using SpieleSammlung.Model.Kniffel.Fields;
using SpieleSammlung.Model.Util;

namespace SpieleSammlung.Model.Kniffel;

/// <summary>For playing a Kniffel match.</summary>
public class KniffelGame
{
    #region constants and static fields

    /// <summary>This is the minimum number of players for a game. Otherwise the game cannot be initiated.</summary>
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

    /// <value>States if the current player can roll the dice again.</value>
    public bool AreShufflesLeft => RemainingShuffles > 0;

    /// <value>States if the current player cannot roll the dice again.</value>
    public bool AreNoShufflesLeft => RemainingShuffles <= 0;

    /// <value>Players of this match.</value>
    public KniffelPlayer[] Players { get; }

    /// <summary>The Current Player of this match</summary>
    public KniffelPlayer CurrentPlayer => Players[ActivePlayer];

    /// <summary>A copy of this games' dice</summary>
    public FlatDice Dice => new(_dice);

    #endregion

    #region private members

    /// <summary>List of fields that can only be killed not written (-> value 0 is written in field).</summary>
    internal readonly List<int> IndexKillableField;

    /// <summary>List of fields that can be written with a value greater than 0.</summary>
    internal readonly List<WriteOption> IndexWritableField;

    /// <summary>Manages the dices.</summary>
    private readonly FlatDice _dice;

    /// <summary>For generating additional dice values without interfering with the rng of the actual game.</summary>
    private readonly FlatDice _diceGenerator;

    private readonly BotStrategy _botStrategy;

    #endregion

    #region constructors

    /// <summary>Creates a new instance and set up a playable game.</summary>
    /// <param name="names">Names of the players.</param>
    public KniffelGame(IReadOnlyCollection<Player> names) : this(names, new FlatDice())
    {
    }

    /// <summary>
    /// Starts a new Kniffel game with the given Seed.
    /// </summary>
    /// <param name="names">Player for this Kniffel game</param>
    /// <param name="seed">Seed for the RNG</param>
    public KniffelGame(IReadOnlyCollection<Player> names, int seed) : this(names, new FlatDice(seed))
    {
    }

    /// <summary>
    /// Starts a new Kniffel game with the given Seed.
    /// </summary>
    /// <param name="names">Player for this Kniffel game</param>
    /// <param name="rng">The random number generator that produces the values for the dice</param>
    public KniffelGame(IReadOnlyCollection<Player> names, Random rng) : this(names, new FlatDice(rng))
    {
    }

    /// <summary>
    /// Starts a new Kniffel game with the given Dice.
    /// </summary>
    /// <param name="names">The players participating in this match</param>
    /// <param name="dice">The DiceManager managing the dice during the match</param>
    private KniffelGame(IReadOnlyCollection<Player> names, FlatDice dice) : this(names, dice,
        new BotStrategy())
    {
    }

    public KniffelGame(IReadOnlyCollection<Player> names, BotStrategy bot) : this(names, new FlatDice(), bot)
    {
    }


    public KniffelGame(IReadOnlyCollection<Player> names, Random random, BotStrategy bot)
        : this(names, new FlatDice(random), bot)
    {
    }

    private KniffelGame(IReadOnlyCollection<Player> names, FlatDice dice, BotStrategy strategy)
    {
        _botStrategy = strategy;
        _diceGenerator = new FlatDice();
        _dice = dice;
        IndexKillableField = [];
        IndexWritableField = [];
        if (names.Count < MIN_PLAYER_COUNT)
            throw new ArgumentException("There are not enough players in this list. " +
                                        $"A kniffel game needs at least {MIN_PLAYER_COUNT} players");

        Players = ArrayHelp.CreateArray(names, name => new KniffelPlayer(name));

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
    public int KillableFieldsCount => IndexKillableField.Count;

    /// <summary>Count of the fields that can be written with a value greater than 0.</summary>
    public int WriteableFieldsCount => IndexWritableField.Count;

    /// <summary>
    /// Returns the index of a killable field that is at position index.
    /// </summary>
    /// <param name="index">Index of Field in the list of killable fields.</param>
    /// <returns>Index of the field.</returns>
    public int GetKillableFieldIndex(int index) => IndexKillableField[index];

    /// <summary>Returns the index of a writable field that is at position <paramref name="index"/>.</summary>
    /// <param name="index">Index of the field in the list of writeable fields.</param>
    /// <returns>Index of the field.</returns>
    public WriteOption GetWriteableFieldsIndex(int index) => IndexWritableField[index];

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
    public void KillFieldOption(int index) => WriteField(IndexKillableField[index], 0);

    /// <summary>Sets the value of the field at the given index to the possible value greater than 0.</summary>
    /// <param name="index">Index of the field to be written.</param>
    public void WriteField(int index) => WriteField(IndexWritableField[index]);

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
        ActivePlayer %= Players.Length;
        if (ActivePlayer == 0)
            ++Round;

        if (IsGameNotOver())
            Shuffle();
    }

    /// <summary>Rolls all dices.</summary>
    public void Shuffle() => Shuffle(null);

    /// <summary>Rolls the dices with the corresponding indexes.</summary>
    /// <param name="index">Indexes of the dices to be rolled.</param>
    public void Shuffle(int[] index)
    {
        if (RemainingShuffles <= 0)
            throw new IllegalMoveException("The current player has no shuffles left");

        if (index == null)
            _dice.Shuffle();
        else
            _dice.Shuffle(index);

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
            IndexWritableField.Add(new WriteOption(index, value));
        else
            IndexKillableField.Add(index);
    }

    private void UpdateOption(bool isPossible, int index, int value)
    {
        if (Players[ActivePlayer].Fields[index].IsEmpty())
            SetValue(isPossible, index, value);
    }

    private void CalculateOptions()
    {
        IndexKillableField.Clear();
        IndexWritableField.Clear();
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

    public int RandomDiceValue() => _diceGenerator.GenerateDiceValue();

    public void DoBotMoveInstant() => DoBotMove(_ => { }, () => { });

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