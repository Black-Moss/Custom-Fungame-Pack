using System;
using System.Collections.Generic;
using BepInEx.Logging;
using HarmonyLib;
using MossLib.Base;
using MossLib.Tool;
using UnityEngine;

namespace CustomFungamePack;

[HarmonyPatch(typeof(ConsoleScript))]
public class ModCommand : ModCommandBase
{
    private new static readonly ManualLogSource Logger = Plugin.Logger;
    private const string LocaleKeyPre = "mod_command.";

    [HarmonyPatch("RegisterAllCommands")]
    [HarmonyPostfix]
    public static void RegisterCustomCommands(ConsoleScript __instance)
    {
        try
        {
            void Action(string[] args)
            {
                switch (args[1])
                {
                    case "reload":
                        if (CheckWorld()) return;
                        CheckArg(args, 1);
                        MapLoader.ReloadMap(FungameCheck.CurrentFungame);
                        break;
                    case "info":
                        CheckArg(args, 1);
                        MapLoader.LogMapInfo();
                        break;
                    case "spawn":
                        if (CheckWorld()) return;
                        CheckArg(args, 1);
                        Spawn();
                        break;
                    case "select":
                        CheckArg(args, 2);
                        Select(args[2]);
                        break;
                    case "list":
                        CheckArg(args, 1);
                        if (args.Length > 2)
                        {
                            Select(args[2]);
                        }
                        else
                        {
                            MapLoader.LogFungameList();
                        }

                        break;
                    case "feature":
                        HandleFeature(args);
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
                        "spawn",
                        "select",
                        "list",
                        "feature"
                    ]
                }
            };
            (string, string)[] valueTupleArray =
            [
                ("string", Fungame("string")),
                ("string", Fungame("parameter")),
                ("string", Fungame("parameter"))
            ];
            ConsoleScript.Commands.Add(new Command(
                "fungame",
                Fungame("description"),
                Action,
                argAutofill2,
                valueTupleArray)
            );
            ConsoleScript.Commands.Add(new Command(
                "fg",
                Fungame("description"),
                Action,
                argAutofill2,
                valueTupleArray)
            );
        }
        catch (Exception ex)
        {
            Plugin.Logger.LogError($"Failed to register custom commands: {ex.Message}\n{ex.StackTrace}");
        }
    }

    private static void HandleFeature(string[] args)
    {
        if (CheckWorld()) return;

        var fungame = FungameCheck.CurrentFungame;
        if (fungame?.Feature == null)
        {
            Error("no_fungame");
            return;
        }

        CheckArg(args, 2);

        if (args.Length < 3)
        {
            Command("feature.no_subcommand");
            return;
        }

        var subCommand = args[2].ToLower();

        switch (subCommand)
        {
            case "list":
                ListFeatures(fungame.Feature);
                break;
            case "get":
                if (args.Length < 4)
                {
                    Fungame("feature.get_no_name");
                    return;
                }

                GetFeature(fungame.Feature, args[3]);
                break;
            case "set":
                if (args.Length < 5)
                {
                    Fungame("feature.set_missing_params");
                    return;
                }

                SetFeature(fungame.Feature, args[3], args[4]);
                break;
            default:
                Fungame("feature.unknown_subcommand", subCommand);
                break;
        }
    }

    private static void ListFeatures(Feature feature)
    {
        Log.Divider();
        InfoFungame("feature.list_header");

        var fields =
            typeof(Feature).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

        foreach (var field in fields)
        {
            var value = field.GetValue(feature);
            var displayName = GetFeatureDisplayName(field.Name);
            InfoFungame("feature.item", $"{displayName} ({field.Name})", value);
        }

        Log.Divider();
    }

    private static void GetFeature(Feature feature, string featureName)
    {
        var field = FindFeatureField(featureName);

        if (field == null)
        {
            ErrorFungame("feature.not_found", featureName);
            return;
        }

        var value = field.GetValue(feature);
        var displayName = GetFeatureDisplayName(field.Name);
        InfoFungame("feature.get_success", $"{displayName} ({field.Name})", value);
    }

    private static void SetFeature(Feature feature, string featureName, string valueStr)
    {
        var field = FindFeatureField(featureName);

        if (field == null)
        {
            ErrorFungame("feature.not_found", featureName);
            return;
        }

        try
        {
            var convertedValue = Convert.ChangeType(valueStr, field.FieldType);

            if (field.FieldType == typeof(float))
            {
                if (field.Name.ToLower() == "gravity")
                {
                    Physics2D.gravity = new Vector2(0, (float)convertedValue);
                }
            }

            field.SetValue(feature, convertedValue);
            var displayName = GetFeatureDisplayName(field.Name);
            InfoFungame("feature.set_success", $"{displayName} ({field.Name})", convertedValue);
        }
        catch (Exception)
        {
            ErrorFungame("feature.invalid_value", featureName, valueStr);
        }
    }

    private static System.Reflection.FieldInfo FindFeatureField(string featureName)
    {
        var fields =
            typeof(Feature).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

        foreach (var field in fields)
        {
            if (field.Name.Equals(featureName, StringComparison.OrdinalIgnoreCase))
            {
                return field;
            }
        }

        return null;
    }

    private static string GetFeatureDisplayName(string fieldName)
    {
        var localeKey = $"feature.{ConvertToSnakeCase(fieldName)}";
        var localized = ModLocale.GetFormat(localeKey);

        return localized.StartsWith("feature.") ? fieldName : localized;
    }

    private static string ConvertToSnakeCase(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;

        var result = new System.Text.StringBuilder();
        result.Append(char.ToLower(input[0]));

        for (int i = 1; i < input.Length; i++)
        {
            if (char.IsUpper(input[i]))
            {
                result.Append('_');
                result.Append(char.ToLower(input[i]));
            }
            else
            {
                result.Append(input[i]);
            }
        }

        return result.ToString();
    }

    private static bool CheckWorld()
    {
        if (HasWorldLoaded()) return false;
        Error("world_not_loaded");
        return true;
    }

    private static bool HasWorldLoaded()
    {
        try
        {
            return WorldGeneration.world != null && !WorldGeneration.world.generatingWorld;
        }
        catch
        {
            return false;
        }
    }

    private static void Select(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            Fungame("select.no_key");
            return;
        }

        if (FungameCheck.Fungames == null || FungameCheck.Fungames.Count == 0)
        {
            Fungame("list.empty");
            return;
        }

        Fungame fungame;

        if (int.TryParse(key, out int index))
        {
            if (index < 1 || index > FungameCheck.Fungames.Count)
            {
                Fungame("select.invalid_index", index, FungameCheck.Fungames.Count);
                return;
            }

            fungame = FungameCheck.Fungames[index - 1];
        }
        else
        {
            fungame = FungameCheck.Fungames.Find(f =>
                f != null &&
                (f.Id?.Equals(key, StringComparison.OrdinalIgnoreCase) == true ||
                 f.Name?.Equals(key, StringComparison.OrdinalIgnoreCase) == true));
        }

        if (fungame == null)
        {
            Fungame("select.not_found", key);
            return;
        }

        WorldGenerationPatch.CurrentFungame = fungame;

        Fungame("select.success", fungame.Name, fungame.Id);

        if (HasWorldLoaded())
        {
            MapLoader.ReloadMap(fungame);
        }
        else
        {
            Command("select.without_world", fungame.Name);
        }
    }

    private static void CheckArg(string[] args, int index)
    {
        Tools.CheckArgumentCount(args, index);
    }

    private static void Spawn()
    {
        var fungame = FungameCheck.CurrentFungame;
        LogConsole("spawn", fungame.SpawnPosition);
        Player.Tp(fungame.SpawnPosition);
    }

    private static string Locale(string key, params object[] args)
    {
        return ModLocale.GetFormat(key, args);
    }

    private static string Command(string key, params object[] args)
    {
        return Locale($"command.{key}", args);
    }

    private static string Fungame(string key, params object[] args)
    {
        return Command($"fungame.{key}", args);
    }

    private static void InfoFungame(string key, params object[] args)
    {
        var message = Fungame(key, args);
        Log.Info(message, Logger);
    }

    private static void ErrorFungame(string key, params object[] args)
    {
        var message = Fungame(key, args);
        Log.Error(message, Logger);
    }

    private static void LogConsole(string key, params object[] args)
    {
        var message = Locale(key, args);
        Log.Info(message, Logger);
    }

    private static void Info(string key, params object[] args)
    {
        var message = ModLocale.Log($"{LocaleKeyPre}{key}", args);
        Log.Info(message, Logger);
    }

    private static void Error(string key, params object[] args)
    {
        var message = ModLocale.Log($"{LocaleKeyPre}{key}", args);
        Log.Error(message, Logger);
    }

    private static void Warning(string key, params object[] args)
    {
        var message = ModLocale.Log($"{LocaleKeyPre}{key}", args);
        Log.Warning(message, Logger);
    }
}