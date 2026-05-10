using System;
using BepInEx.Logging;
using HarmonyLib;
using MossLib;
using UnityEngine;
using System.Reflection;
using Debug = System.Diagnostics.Debug;

namespace CustomFungamePack;

[HarmonyPatch(typeof(WorldGeneration))]
public class WorldGenerationPatch
{
    private static readonly ManualLogSource Logger = Plugin.Logger;
    internal static WorldGeneration WorldGeneration;

    [HarmonyPatch("Awake")]
    [HarmonyPostfix]
    public static void ForceEnableTutorial(WorldGeneration __instance)
    {
        WorldGeneration = __instance;
        __instance.biomeOverride = WorldGeneration.OverrideSceneType.Tutorial;
        Info("已强制修改为教程世界");
    }

    [HarmonyPatch("FinishWorldGeneration")]
    [HarmonyPostfix]
    public static void InitializationWorld(WorldGeneration __instance)
    {
        for (int x = -76; x < 74; x++)
        {
            for (int y = -120; y < 58; y++)
            {
                SetBlock(x, y, 0);
                Info($"{x}, {y}");
            }
        }
        var chunksField = typeof(WorldGeneration).GetField("chunks", BindingFlags.NonPublic | BindingFlags.Instance);
        Debug.Assert(chunksField != null, nameof(chunksField) + " != null");
        if (chunksField.GetValue(__instance) is not Array chunks) return;
        int upperBound1 = chunks.GetUpperBound(0);
        int upperBound2 = chunks.GetUpperBound(1);
            
        for (int lowerBound1 = chunks.GetLowerBound(0); lowerBound1 <= upperBound1; ++lowerBound1)
        {
            for (int lowerBound2 = chunks.GetLowerBound(1); lowerBound2 <= upperBound2; ++lowerBound2)
            {
                var chunk = chunks.GetValue(lowerBound1, lowerBound2) as UnityEngine.Tilemaps.Tilemap;
                if (chunk != null)
                {
                    __instance.CreateBackground("steelBackground", chunk);
                }
            }
        }
    }
    
    public static void SetBlock(int x, int y, ushort block)
    {
        Vector2 vector2 = new(x, y);
        SetBlock(vector2, block);
    }
    
    public static void SetBlock(Vector2 vector2, ushort block)
    {
        Tools.CheckForWorld();
        try
        {
            WorldGeneration.world.SetBlock(WorldGeneration.world.WorldToBlockPos(vector2), block);
        }
        catch (Exception e)
        { 
            Error($"在 {vector2} 生成 {block} 失败：{e}");
        }
    }
    
    private static void Info(string text)
    {
        Tools.LogInfo(text, Logger);
    }
    
    private static void Error(string text)
    {
        Tools.LogError(text, Logger);
    }
}
