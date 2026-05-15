using BepInEx.Logging;
using HarmonyLib;
using MossLib;
using System.IO;

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
        __instance.biomeOverride = WorldGeneration.OverrideSceneType.Debug;
    }
    
    // 跳过地形生成
    [HarmonyPatch("WorldGenerateTerrain")]
    [HarmonyPrefix]
    public static bool SkipTerrainGeneration()
    {
        return false;
    }

    // // 跳过结构生成
    // [HarmonyPatch("WorldGenerateStructures")]
    // [HarmonyPrefix]
    // public static bool SkipStructureGeneration()
    // {
    //     return false;
    // }
    
    // // 跳过背景生成
    // [HarmonyPatch("WorldCreateBackground")]
    // [HarmonyPrefix]
    // public static bool SkipWorldCreateBackground()
    // {
    //     return false;
    // }
    
    [HarmonyPatch("FinishWorldGeneration")]
    [HarmonyPostfix]
    public static void InitializationWorld(WorldGeneration __instance)
    {
        __instance.loadingText.text = "初始化Fungame地图...";

        if (FungameCheck.ValidDirectories.Count > 0)
        {
            var firstFungameDir = FungameCheck.ValidDirectories[0];
            var fungameFilePath = Path.Combine(firstFungameDir, "fungame.json");
            
            if (File.Exists(fungameFilePath))
            {
                var jsonContent = File.ReadAllText(fungameFilePath);
                var fungame = Newtonsoft.Json.JsonConvert.DeserializeObject<Fungame>(jsonContent);
                
                if (fungame?.Map != null)
                {
                    Info($"正在加载Fungame地图: {fungame.Name}");
                    __instance.loadingText.text = $"正在加载Fungame地图: {fungame.Name}";
                    MapLoader.LoadAndApplyMapFromFungame(fungame);
                    
                    string authors = fungame.Author != null && fungame.Author.Count > 0 
                        ? string.Join(", ", fungame.Author) 
                        : "未知作者";
                    Tools.LogCla($"{fungame.Name} v{fungame.Version}\nby {authors}", Logger, true);
                    Tools.LogCla($"{fungame.Description}", Logger, false, 6f);
                    Tools.SetBlock(0, 0, 6);
                }
                else
                {
                    Warning($"Fungame {fungame?.Name ?? "未知"} 不包含地图数据");
                }
            }
            else
            {
                Error($"找不到 fungame.json 文件: {fungameFilePath}");
            }
        }
        else
        {
            Error("没有有效的Fungame目录，请检查 Fungames 文件夹");
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

    private static void Warning(string text)
    {
        Tools.LogWarning(text, Logger);
    }
}