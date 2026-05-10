// using System.IO;
// using BepInEx.Logging;
// using HarmonyLib;
// using MossLib;
//
// namespace CustomFungamePack;
//
// [HarmonyPatch(typeof(Body))]
// public class BodyPatch
// {
//     private static readonly ManualLogSource Logger = Plugin.Logger;
//     [HarmonyPatch("Start")]
//     [HarmonyPrefix]
//     public static void Start(WorldGeneration __instance)
//     {
//         var firstFungameDir = FungameCheck.ValidDirectories[0];
//         var fungameFilePath = Path.Combine(firstFungameDir, "fungame.json");
//         var jsonContent = File.ReadAllText(fungameFilePath);
//         var fungame = Newtonsoft.Json.JsonConvert.DeserializeObject<Fungame>(jsonContent);
//
//         Tools.LogCla($"{fungame.Name} v{fungame.Version} by {fungame.Author}", Logger, true);
//         Tools.LogCla($"{fungame.Description}", Logger);
//     }
// }