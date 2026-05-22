using System;
using BepInEx;
using BepInEx.Logging;
using MossLib.Tool;
using UnityEngine;
using Console = MossLib.Tool.Console;
using Debug = System.Diagnostics.Debug;

namespace CustomFungamePack;

[BepInDependency("com.Jimmyking.morestructures", BepInDependency.DependencyFlags.SoftDependency)]
public static class CustomStructuresLoader
{
    private const string LocaleKeyPre = "custom_structures_loader.";
    private static readonly ManualLogSource Logger = Plugin.Logger;

    public static void SpawnCustomStructures(Fungame fungame)
    {
        if (fungame is { MapData: not null, CustomStructures: null })
        {
            ModLocale.Log("map_loader.load_error");
            return;
        }
        
        try
        {
            Console.RunCommand($"structure {fungame.CustomStructures}");
            Debug.Assert(Camera.main != null, "Camera.main != null");
            var pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Player.Tp(pos);
            
            Info("loading", fungame.CustomStructures);
        }
        catch (Exception e)
        {
            Error("failed", fungame, e);
        }
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
}