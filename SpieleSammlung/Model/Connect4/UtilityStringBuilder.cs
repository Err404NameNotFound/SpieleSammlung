#region

using System;
using System.Text;

#endregion

namespace SpieleSammlung.Model.Connect4;

/// <summary>Can be used for removing chars at the end of a StringBuilder.</summary>
public static class UtilityStringBuilder
{
    /// <summary>Removes the last char from a StringBuilder.</summary>
    /// <param name="text">StringBuilder with at least one char.</param>
    /// <return>The same StringBuilder but with its last char removed.</return>
    public static void RemoveLastChar(StringBuilder text) => RemoveLastChars(text, 1);

    /// <summary>Removes the given amount of chars from the end of a StringBuilder.</summary>
    /// <param name="text"> StringBuilder with at least <paramref name="count"/> chars.</param>
    /// <param name="count"> Amount of chars to be deleted starting from the last char.</param>
    /// <returns>The same StringBuilder but with its lasts chars removed.</returns>
    private static void RemoveLastChars(StringBuilder text, int count)
    {
        if (text == null) throw new ArgumentException("Argument was null.");
        if (text.Length < count) throw new ArgumentException("Text isn't long enough.");
        text.Remove(text.Length - count, text.Length);
    }
}