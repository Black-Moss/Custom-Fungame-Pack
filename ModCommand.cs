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
        void Action(string[] args)
        {
            Tools.CheckArgumentCount(args, 1);
            switch (args[1])
            {
                case "reload":
                    Info("fungame.reloaded");
                    ReloadMap();
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
                    "reload"
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

    private static void ReloadMap()
    {
        try
        {
            var currentFungame = WorldGenerationPatch.CurrentFungame;

            if (currentFungame == null)
            {
                Error("no_current_fungame");
                return;
            }

            Info("restarting_scene");
            RestartScene();
        }
        catch (Exception ex)
        {
            Error("reload_failed", ex.Message);
        }
    }

    private static void RestartScene()
    {
        try
        {
            var currentScene = SceneManager.GetActiveScene();
            Info("scene_reloading", currentScene.name);

            SceneManager.LoadScene(currentScene.buildIndex);

            Info("scene_reloaded");
        }
        catch (Exception ex)
        {
            Error("scene_reload_failed", ex.Message);
        }
    }
}