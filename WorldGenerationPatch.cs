using BepInEx.Logging;
using HarmonyLib;
using MossLib;

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
        __instance.loadingText.text = "初始化Fungame地图...";

        const int startX = -76;
        const int endX = 74;
        const int startY = -120;
        const int endY = 58;
            
        for (int x = startX; x < endX; x++)
        {
            for (int y = startY; y < endY; y++)
            {
                Tools.SetBlock(x, y, 0);
            }
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
