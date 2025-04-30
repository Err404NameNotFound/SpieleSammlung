#region

using System;
using System.Collections.Generic;
using System.IO;

#endregion

namespace SpieleSammlung.Model;

public static class ModelLog
{
    public const string SEPARATOR0 = "********************";
    public const string SEPARATOR1 = "--------------------";
    public const string SEPARATOR2 = "++++++++++++++++++++";
    public const string SEPARATOR_DEFAULT = "####################";
    public const string NEWLINE = "\r\n";
    public static readonly string PATH;

    private static bool _writeToConsole;

    private static bool _writeToFile;

    static ModelLog()
    {
        if (!Directory.Exists("./Log"))
            Directory.CreateDirectory("./Log");
        PATH = "./Log/Log_" + DateTime.Now.ToString("dd_MM_yy-HH_mm_ss") + ".txt";
    }

    public static bool Writes { private set; get; } = _writeToConsole || _writeToFile;

    public static bool WriteToConsole
    {
        get => _writeToConsole;
        set
        {
            _writeToConsole = value;
            if (value)
                Writes = true;
            else if
                (!_writeToFile) Writes = false;
        }
    }

    public static bool WriteToFile
    {
        get => _writeToFile;
        set
        {
            _writeToFile = value;
            if (value)
                Writes = true;
            else if (!_writeToConsole)
                Writes = false;
        }
    }

    public static void Write(string message) => PrintAllText(message, false);
    public static void Write(string message, params object[] args) => PrintTextFormatted(message, false, args);
    public static void Write(List<string> messages) => PrintAllLines(messages, false);
    public static void Append(string message) => PrintAllText(message, true);

    public static void Append(string message, params object[] args) => PrintTextFormatted(message, true, args);

    public static void AppendLine(string message, params object[] args) =>
        PrintTextFormatted(message + NEWLINE, true, args);

    public static void AppendLine(string message = "") => PrintAllText(message + NEWLINE, true);
    public static void Append(List<string> messages) => PrintAllLines(messages, true);

    public static void AppendSeparatorLine(int character = 0)
    {
        string text = character switch
        {
            0 => "********************",
            1 => "--------------------",
            2 => "++++++++++++++++++++",
            _ => "####################"
        };
        AppendLine(text);
    }

    private static void PrintAllText(string text, bool append)
    {
        if (WriteToFile)
        {
            if (append)
                File.AppendAllText(PATH, text);
            else
                File.WriteAllText(PATH, text);
        }

        if (WriteToConsole)
            Console.Write(text);
    }

    private static void PrintAllLines(List<string> texts, bool append)
    {
        if (WriteToFile)
        {
            if (append)
                File.AppendAllLines(PATH, texts);
            else
                File.WriteAllLines(PATH, texts);
        }

        if (WriteToConsole)
            texts.ForEach(Console.WriteLine);
    }

    private static void PrintTextFormatted(string text, bool append, params object[] args)
    {
        if (WriteToFile)
        {
            if (append)
                File.AppendAllText(PATH, string.Format(text, args));
            else
                File.WriteAllText(PATH, string.Format(text, args));
        }

        if (WriteToConsole)
            Console.Write(text, args);
    }

    public static void DeleteLogFile()
    {
        if (File.Exists(PATH))
            File.Delete(PATH);
    }
}