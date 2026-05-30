using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace CustomFungamePack.Patch;

[HarmonyPatch(typeof(MineScript))]
public class MineScriptPatch
{
    private static Feature Feature => FungameCheck.CurrentFungame?.Feature;

    private static readonly FieldInfo PressedField = typeof(MineScript).GetField(
        "pressed",
        BindingFlags.NonPublic | BindingFlags.Instance);

    private static readonly FieldInfo ExplodedField = typeof(MineScript).GetField(
        "exploded",
        BindingFlags.NonPublic | BindingFlags.Instance);

    private static readonly ExplosionParams MineExplosionParams = new()
    {
        muscleDamage = Feature.MineExplosionParamsData.MuscleDamage,
        skinDamage = Feature.MineExplosionParamsData.SkinDamage,
        skinDamageChance = Feature.MineExplosionParamsData.SkinDamageChance,
        boneBreakChance = Feature.MineExplosionParamsData.BoneBreakChance,
        dislocationChance = Feature.MineExplosionParamsData.DislocationChance,
        disfigureChance = Feature.MineExplosionParamsData.DisfigureChance,
        bleedChance = Feature.MineExplosionParamsData.BleedChance,
        bleedAmount = Feature.MineExplosionParamsData.BleedAmount,
        structuralDamage = Feature.MineExplosionParamsData.StructuralDamage,
        range = Feature.MineExplosionParamsData.Range,
        velocity = Feature.MineExplosionParamsData.Velocity,
        shrapnelChance = Feature.MineExplosionParamsData.ShrapnelChance,
        sound = Feature.MineExplosionParamsData.Sound
    };

    [HarmonyPatch("Update")]
    [HarmonyPrefix]
    public static bool UpdatePrefix(MineScript __instance)
    {
        if (!(bool)PressedField.GetValue(__instance))
            return true;
        __instance.timeSincePressed += Time.deltaTime;

        bool exploded = (bool)ExplodedField.GetValue(__instance);
        if (exploded || !(__instance.timeSincePressed > 0.800000011920929f))
            return false;

        ExplodedField.SetValue(__instance, true);
        __instance.build.health = Feature.MineUndestroy
            ? __instance.build.health
            : 100;
        MineExplosionParams.position = __instance.transform.position + Vector3.up;
        WorldGeneration.CreateExplosion(MineExplosionParams);
        PressedField.SetValue(__instance, !Feature.MineUndestroy);
        ExplodedField.SetValue(__instance, !Feature.MineUndestroy);

        return false;
    }

    [HarmonyPatch("OnDestroy")]
    [HarmonyPrefix]
    public static bool OnDestroyPrefix(MineScript __instance)
    {
        if (__instance.build.health >= 0.5f || (bool)ExplodedField.GetValue(__instance))
            return false;

        MineExplosionParams.position = __instance.transform.position + Vector3.up;
        WorldGeneration.CreateExplosion(MineExplosionParams);

        return false;
    }
}