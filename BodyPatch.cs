using HarmonyLib;
using System.Reflection;
using MossLib.Tool;

namespace CustomFungamePack;

[HarmonyPatch(typeof(Body))]
public static class BodyPatch
{
    private static Feature Feature => FungameCheck.CurrentFungame?.Feature;

    private static readonly FieldInfo JumpCooldownField = typeof(Body).GetField(
        "jumpCooldown",
        BindingFlags.NonPublic | BindingFlags.Instance);

    private static readonly FieldInfo FirstWallJumpField = typeof(Body).GetField(
        "firstWallJump",
        BindingFlags.NonPublic | BindingFlags.Instance);

    private static bool IsPatchActive => Feature != null
                                         && JumpCooldownField != null
                                         && FirstWallJumpField != null;

    private static readonly bool JumpKey = Key.IsKeyDown("jump");

    private static int _jumpCount;
    private static int _climbCount;

    [HarmonyPostfix]
    [HarmonyPatch("Update")]
    public static void UpdatePostfix(Body __instance)
    {
        if (!IsPatchActive)
            return;

        HandleMultiJump(__instance);
        HandleMultiClimb(__instance);

        // 没落地
        // 初始化
        if (!__instance.grounded) return;
        _jumpCount = 0;
        _climbCount = 0;
    }

    private static void HandleMultiJump(Body __instance)
    {
        // 落地
        if (__instance.grounded)
            return;

        // 没按跳 到头了
        if (!JumpKey || _jumpCount >= Feature.JumpLimit)
            return;

        // 恢复
        JumpCooldownField.SetValue(__instance, 0f);
        __instance.grounded = true;
        __instance.Jump();
        _jumpCount++;
    }

    private static void HandleMultiClimb(Body __instance)
    {
        // // 落地
        if (__instance.grounded)
        {
            return;
        }

        // 没按跳 到头了
        if (!JumpKey || _climbCount >= Feature.ClimbLimit)
            return;

        // 恢复
        FirstWallJumpField.SetValue(__instance, true);
        __instance.grounded = true;
        _climbCount++;
    }
}