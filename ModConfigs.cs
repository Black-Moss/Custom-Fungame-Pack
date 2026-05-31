using System;

namespace CustomFungamePack;

public static class ModConfigs
{
    public static bool MoreLogs => Plugin.MoreLogs.Value;
    public static bool StartGameUseFungame => Plugin.StartGameUseFungame.Value;
    public static string FirstUseFungame => Plugin.FirstUseFungame.Value;

    public static void ReloadConfigs()
    {
        // ReloadConfigs();
    }

    public static void SetConfig(string key, object value)
    {
        switch (key)
        {
            case "more_logs":
                Plugin.MoreLogs.Value = Convert.ToBoolean(value);
                break;
            case "start_game_use_fungame":
                Plugin.StartGameUseFungame.Value = Convert.ToBoolean(value);
                break;
            case "first_use_fungame":
                Plugin.FirstUseFungame.Value = Convert.ToString(value);
                break;
        }
    }
}