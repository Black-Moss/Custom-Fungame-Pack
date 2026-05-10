using System;
using System.Globalization;
using BepInEx.Logging;
using HarmonyLib;
using KrokoshaCasualtiesMP;
using MossLib;
using MossLib.Base;
using UnityEngine;

namespace CustomFungamePack;

public class ModCommand : ModCommandBase
{
    private static ModCommand _instance;
    private static readonly ManualLogSource Logger = Plugin.Logger;
    private static readonly WorldGeneration WorldGeneration = WorldGenerationPatch.WorldGeneration;

    private static ModCommand Instance { get; set; } = new();

    public void Initialize()
    {
        if (_instance != null)
            return;
        _instance = new ModCommand();
        Instance = _instance;
        _instance.Initialize();
    }

    [HarmonyPatch(typeof (ConsoleScript), "RegisterAllCommands")]
    public class ConsoleScriptRegisterAllCommandsPatcher
    {
        [HarmonyPostfix]
        public static void RegisterCustomCommands()
        {
            ConsoleScript.Commands.Add(new Command(
                "tiletest", 
                "testhello.description", _ => 
                { 
                    Tools.CheckForWorld();
                    for (int x = 0; x < 10; x++)
                    {
                        for (int y = 0; y < 10; y++)
                        {
                            var pos = new Vector2(x,y);
                            Info(pos.ToString());
                            var blockPos = WorldGeneration.world.WorldToBlockPos(pos);
                            WorldGeneration.world.SetBlock(blockPos, 6);
                        }
                    }
                }, 
                null
                )
            );
        }
    }
    
    private static void Info(string text)
    {
        Tools.LogInfo(text, Logger);
    }
}
