using MossLib.Tool;

namespace CustomFungamePack;

public static class Configs
{
    public static bool MoreLogs;

    public static void ReloadConfigs()
    {
        MoreLogs = Plugin.MoreLogs.Value;
    }
}