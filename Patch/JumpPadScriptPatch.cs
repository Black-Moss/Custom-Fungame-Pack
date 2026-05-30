using System.Reflection;
using HarmonyLib;
using UnityEngine.Rendering.Universal;

namespace CustomFungamePack.Patch;

[HarmonyPatch(typeof(JumpPadScript))]
public class JumpPadScriptPatch
{
    private static JumpPadData JumpPadData => FungameCheck.CurrentFungame?.Feature?.JumpPadData;

    private static readonly FieldInfo CooldownField = typeof(JumpPadScript).GetField(
        "cooldown",
        BindingFlags.NonPublic | BindingFlags.Instance);

    private static readonly FieldInfo LightField = typeof(JumpPadScript).GetField(
        "light",
        BindingFlags.NonPublic | BindingFlags.Instance);

    [HarmonyPatch("OnCollisionEnter2D")]
    [HarmonyPostfix]
    public static void OnCollisionPostfix(JumpPadScript __instance)
    {
        var data = JumpPadData;
        if (data == null) return;

        CooldownField.SetValue(__instance, data.Cooldown);

        if (data.NoLight && LightField.GetValue(__instance) is Light2D light)
            light.intensity = 0f;
    }
}