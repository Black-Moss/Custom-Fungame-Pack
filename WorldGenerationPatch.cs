using System.Collections;
using System.Reflection;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace CustomFungamePack;

[HarmonyPatch(typeof(WorldGeneration))]
public class WorldGenerationPatch
{
    private static readonly ManualLogSource Logger = Plugin.Logger;

    // 强制开启教程世界
    [HarmonyPatch("Awake")]
    [HarmonyPostfix]
    public static void ForceEnableTutorial(WorldGeneration __instance)
    {
        __instance.biomeOverride = WorldGeneration.OverrideSceneType.Debug;
    }
    
    // // 跳过地形生成
    // [HarmonyPatch("WorldGenerateTerrain")]
    // [HarmonyPrefix]
    // public static bool SkipTerrainGeneration()
    // {
    //     return false;
    // }
    //
    // // 跳过结构生成
    // [HarmonyPatch("WorldGenerateStructures")]
    // [HarmonyPrefix]
    // public static bool SkipStructureGeneration()
    // {
    //     return false;
    // }
    //
    // // 跳过背景生成
    // [HarmonyPatch("WorldCreateBackground")]
    // [HarmonyPrefix]
    // public static bool SkipWorldCreateBackground()
    // {
    //     return false;
    // }
    // 跳过实体放置
    // [HarmonyPatch("WorldPlaceEntities")]
    // [HarmonyPrefix]
    // public static bool SkipEntityPlacement()
    // {
    //     return false;
    // }

    // 替换整个 GenerateWorld 协程
    // [HarmonyPatch("GenerateWorld")]
    // [HarmonyPrefix]
    // public static bool ReplaceGenerateWorld(WorldGeneration __instance)
    // {
    //     // 如果是教程模式，使用原版生成流程
    //     if (__instance.biomeOverride == WorldGeneration.OverrideSceneType.Tutorial)
    //     {
    //         Logger.LogInfo("[CustomFungamePack] 检测到教程模式，使用原版教程生成流程");
    //         // 通过反射调用私有的 GenerateWorld 方法
    //         var generateWorldMethod = typeof(WorldGeneration).GetMethod("GenerateWorld", 
    //             BindingFlags.NonPublic | BindingFlags.Instance);
    //         if (generateWorldMethod != null)
    //         {
    //             var coroutine = generateWorldMethod.Invoke(__instance, null) as IEnumerator;
    //             if (coroutine != null)
    //             {
    //                 __instance.StartCoroutine(coroutine);
    //             }
    //         }
    //         else
    //         {
    //             Logger.LogError("[CustomFungamePack] 未找到 GenerateWorld 方法");
    //         }
    //         return false;
    //     }
    //     
    //     // 非教程模式使用自定义生成
    //     __instance.StartCoroutine(CustomWorldGeneration(__instance));
    //     return false;
    // }    
    // // 跳过层级修饰符
    // [HarmonyPatch("ApplyLayerModifiers")]
    // [HarmonyPrefix]
    // public static bool SkipApplyLayerModifiers()
    // {
    //     return false;
    // }
    
    // // 跳过迷你桶
    // [HarmonyPatch("DistributeMiniBarrels")]
    // [HarmonyPrefix]
    // public static bool SkipDistributeMiniBarrels()
    // {
    //     return false;
    // }
    // private static IEnumerator CustomWorldGeneration(WorldGeneration worldGen)
    // {
    //     worldGen.loadingObject.SetActive(true);
    //     worldGen.generatingWorld = true;
    //
    //     yield return InvokeCoroutine(worldGen, "WorldPreprocess");
    //     yield return InvokeCoroutine(worldGen, "WorldCreateBackground");
    //     yield return null;
    //     if (PlayerCamera.main != null && PlayerCamera.main.body != null)
    //     {
    //         PlayerCamera.main.body.PlaceBody();
    //     }
    //     yield return null;
    //
    //     yield return InvokeCoroutine(worldGen, "FinishWorldGeneration");
    //     yield return null;
    //     PlaceCustomBlocks(worldGen);
    // }
    
    private static void PlaceCustomBlocks(WorldGeneration worldGen)
    {
        Logger.LogInfo("[CustomFungamePack] 开始放置自定义物块");
        
        uint width = worldGen.width;
        uint height = worldGen.height;
        
        int centerX = (int)(width / 2);
        int centerY = (int)(height / 2);
        
        for (int x = centerX - 10; x < centerX + 100; x++)
        {
            for (int y = centerY - 5; y < centerY + 50; y++)
            {
                if (x >= 0 && x < width && y >= 0 && y < height)
                {
                    worldGen.SetBlock(new Vector2Int(x, y), 6);
                }
            }
        }
        
        Logger.LogInfo("[CustomFungamePack] 自定义物块放置完成 - 放置了钢铁方块");
    }

    // private static IEnumerator InvokeCoroutine(WorldGeneration instance, string methodName)
    // {
    //     var method = typeof(WorldGeneration).GetMethod(methodName, 
    //         BindingFlags.NonPublic | BindingFlags.Instance);
    //     
    //     if (method != null)
    //     {
    //         var result = method.Invoke(instance, null);
    //         if (result is not IEnumerator coroutine) yield break;
    //         while (coroutine.MoveNext())
    //         {
    //             yield return coroutine.Current;
    //         }
    //     }
    //     else
    //     {
    //         Logger.LogError($"[CustomFungamePack] 未找到方法: {methodName}");
    //         yield return null;
    //     }
    // }
}
