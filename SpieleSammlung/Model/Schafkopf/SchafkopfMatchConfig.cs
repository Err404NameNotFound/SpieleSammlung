using System;
using SpieleSammlung.Properties;

namespace SpieleSammlung.Model.Schafkopf;

public class SchafkopfMatchConfig
{
    public string Trumpf { get; protected set; }
    public string SauspielFarbe { get; protected set; }
    public SchafkopfMode Mode { get; protected set; }

    protected SchafkopfMatchConfig()
    {
    }

    public SchafkopfMatchConfig(SchafkopfMode mode, string color)
    {
        Mode = mode;
        if (mode == SchafkopfMode.Sauspiel)
        {
            Trumpf = Card.HERZ;
            SauspielFarbe = color;
        }
        else
            Trumpf = color;
    }

    public override string ToString()
    {
        return Mode switch
        {
            SchafkopfMode.Wenz or SchafkopfMode.WenzTout or SchafkopfMode.Weiter => Mode.ToString(),
            SchafkopfMode.Solo or SchafkopfMode.SoloTout => $"{Trumpf} {Mode}",
            SchafkopfMode.Sauspiel => $"{Resources.SK_PrefixSauspielToString} {SauspielFarbe}",
            _ => throw new NotSupportedException("This mode has not been implemented yet")
        };
    }

    public static SchafkopfMode StringToSchafkopfMode(string game)
    {
        return game switch
        {
            "Sauspiel" => SchafkopfMode.Sauspiel,
            "Solo" => SchafkopfMode.Solo,
            "Wenz" => SchafkopfMode.Wenz,
            "SoloTout" => SchafkopfMode.SoloTout,
            "WenzTout" => SchafkopfMode.WenzTout,
            _ => SchafkopfMode.Weiter
        };
    }
}