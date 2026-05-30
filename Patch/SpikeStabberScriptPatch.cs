using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using MossLib.Tool;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace CustomFungamePack.Patch;

[HarmonyPatch(typeof(SpikeStabberScript))]
public class SpikeStabberScriptPatch
{
    private static SpikeStabberData SpikeStabberData => FungameCheck.CurrentFungame?.Feature?.SpikeStabberData;

    private static readonly FieldInfo ActivatedField = typeof(SpikeStabberScript).GetField(
        "activated", BindingFlags.NonPublic | BindingFlags.Instance);

    private static readonly FieldInfo LightField = typeof(SpikeStabberScript).GetField(
        "light", BindingFlags.NonPublic | BindingFlags.Instance);

    private static readonly Dictionary<SpikeStabberScript, float> LastTriggerTime = new();

    [HarmonyPatch("OnTriggerEnter2D")]
    [HarmonyPrefix]
    public static bool OnTriggerPrefix(SpikeStabberScript __instance, Collider2D collision)
    {
        var data = SpikeStabberData;
        if (data == null)
            return true;

        try
        {
            __instance.damageMult = data.DamageMult;

            if (!string.IsNullOrEmpty(data.Sound))
                __instance.sound = data.Sound;

            if (data.Undestroy && data.Cooldown > 0f &&
                LastTriggerTime.TryGetValue(__instance, out float lastTime) &&
                Time.time - lastTime < data.Cooldown)
                return false;

            return true;
        }
        catch
        {
            return true;
        }
    }

    [HarmonyPatch("OnTriggerEnter2D")]
    [HarmonyPostfix]
    public static void OnTriggerPostfix(SpikeStabberScript __instance)
    {
        var data = SpikeStabberData;
        if (data == null) return;

        try
        {
            if (data.Undestroy && data.Cooldown > 0f &&
                LastTriggerTime.TryGetValue(__instance, out float lastTime) &&
                Time.time - lastTime < data.Cooldown)
                return;

            if (data.Undestroy)
            {
                LastTriggerTime[__instance] = Time.time;

                __instance.CheckStab();

                __instance.StartCoroutine(ResetSpikeCoroutine(__instance));
            }

            if (data.NoLight)
            {
                if (LightField?.GetValue(__instance) is Light2D light)
                    light.enabled = false;
                if (__instance.bonusLight != null)
                    __instance.bonusLight.enabled = false;
            }
        }
        catch
        {
            // Silently ignore
        }
    }

    private static IEnumerator ResetSpikeCoroutine(SpikeStabberScript instance)
    {
        var data = SpikeStabberData;
        float cooldown = data?.Cooldown ?? 0f;

        if (cooldown > 0f)
            yield return new WaitForSeconds(cooldown);

        if (instance == null || WorldGeneration.world == null) yield break;

        Plugin.Logger.LogInfo(instance.transform.localPosition);

        Object.Destroy(instance.gameObject);
    }
}