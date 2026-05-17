using System;
using System.Collections.Generic;
using BepInEx.Logging;
using HarmonyLib;
using MossLib.Base;
using MossLib.Tool;
using UnityEngine.SceneManagement;

namespace CustomFungamePack;

[HarmonyPatch(typeof(ConsoleScript))]
public class ModCommand : ModCommandBase
{
    private new static readonly ManualLogSource Logger = Plugin.Logger;
    private const string LocaleKeyPre = "log.modcommand.";

    [HarmonyPatch("RegisterAllCommands")]
    [HarmonyPostfix]
    public static void RegisterCustomCommands(ConsoleScript __instance)
    {
        var fungame = FungameCheck.RunningFungame;

        void Action(string[] args)
        {
            Tools.CheckArgumentCount(args, 1);
            switch (args[1])
            {
                case "reload":
                    Locale("fungame.reload");
                    MapLoader.ReloadMap();
                    break;
                case "info":
                    Locale("fungame.info.name", fungame.Name);
                    Locale("fungame.info.id", fungame.Id);
                    Locale("fungame.info.version", fungame.Version);
                    Locale("fungame.info.authors", fungame.Authors);
                    Locale("fungame.info.description", fungame.Description);
                    Locale("fungame.info.feature", fungame.Feature);
                    Locale("fungame.info.spawn", fungame.Spawn);
                    break;
                case "spawn":
                    Locale("fungame.spawn");
                    Player.Tp(fungame.SpawnPosition);
                    break;
                default:
                    Warning("empty_type");
                    break;
            }
        }

        Dictionary<int, List<string>> argAutofill2 = new Dictionary<int, List<string>>
        {
            {
                0,
                [
                    "reload",
                    "info",
                    "spawn"
                ]
            }
        };
        (string, string)[] valueTupleArray =
        [
            ("string", Locale("fungame.string"))
        ];
        ConsoleScript.Commands.Add(new Command(
            "fungame",
            Locale("fungame.description"),
            Action,
            argAutofill2,
            valueTupleArray)
        );
    }

    private static string Locale(string key, params object[] args)
    {
        return ModLocale.GetFormat($"command.{key}", args);
    }

    private static void Info(string key, params object[] args)
    {
        var message = ModLocale.GetFormat($"{LocaleKeyPre}{key}", args);
        Log.Info(message, Logger);
    }

    private static void Error(string key, params object[] args)
    {
        var message = ModLocale.GetFormat($"{LocaleKeyPre}{key}", args);
        Log.Error(message, Logger);
    }

    private static void Warning(string key, params object[] args)
    {
        var message = ModLocale.GetFormat($"{LocaleKeyPre}{key}", args);
        Log.Warning(message, Logger);
    }
}