using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace CustomFungamePack;

public class Fungame
{
    [JsonProperty("name")] public string Name { get; set; }
    [JsonProperty("id")] public string Id { get; set; }
    [JsonProperty("version")] public string Version { get; set; }
    [JsonProperty("author")] public List<string> Author { get; set; }
    [JsonProperty("description")] public string Description { get; set; }
    [JsonProperty("feature")] public Feature Feature { get; set; } = new();
    [JsonProperty("waypoints")] public List<WaypointData> Waypoints { get; set; } = [];
    [JsonProperty("items")] public List<ItemData> Items { get; set; } = [];
    [JsonProperty("spawn")] public float[] Spawn { get; set; } = [0, 0];
    [JsonProperty("x")] public int X { get; set; }
    [JsonProperty("y")] public int Y { get; set; }
    [JsonProperty("xp")] public XpData XpData { get; set; }

    [JsonProperty("type")]
    [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
    public WorldGeneration.OverrideSceneType Type { get; set; } = WorldGeneration.OverrideSceneType.Debug;

    [JsonIgnore] public Vector2 MapPosition => new(X, Y);

    [JsonProperty("map_data", NullValueHandling = NullValueHandling.Ignore)]
    public MapData MapData { get; set; }

    [JsonProperty("command", NullValueHandling = NullValueHandling.Ignore)]
    public CommandData CommandData { get; set; }

    [JsonProperty("waypoint", NullValueHandling = NullValueHandling.Ignore)]
    public WaypointData WaypointData { get; set; }

    [JsonProperty("custom_structures", NullValueHandling = NullValueHandling.Ignore)]
    public string CustomStructures;

    [JsonProperty("build_mode_save", NullValueHandling = NullValueHandling.Ignore)]
    public string BuildModeSave;

    [JsonProperty("skip_terrain")] public bool SkipTerrain { get; set; } = true;
    [JsonProperty("skip_structures")] public bool SkipStructures { get; set; } = true;
    [JsonProperty("skip_background")] public bool SkipBackground { get; set; } = true;

    [JsonIgnore] public string DirectoryPath { get; set; }

    [JsonIgnore]
    public string Authors => Author is { Count: > 0 }
        ? string.Join(", ", Author)
        : "Unknown";

    [JsonIgnore]
    public string Features => Feature != null
        ? string.Join(", ", typeof(Feature).GetProperties().Select(prop =>
            $"{prop.Name}={prop.GetValue(Feature)}"))
        : "None";

    [JsonIgnore]
    public Vector2 SpawnPosition => new(
        Spawn is { Length: >= 2 }
            ? Spawn[0]
            : 0,
        Spawn is { Length: >= 2 }
            ? Spawn[1]
            : 0);
}

[UsedImplicitly]
public class MapData
{
    [JsonProperty("map")] public string[] Map { get; set; } = [];
    [JsonProperty("key")] public Dictionary<string, object> Key { get; set; } = new();
}

[UsedImplicitly]
public class Feature
{
    public bool Fullbright { get; set; } = true;
    public bool ForgivingLevel { get; set; }
    public float Gravity { get; set; } = Physics2D.gravity.y;
    public int JumpLimit { get; set; } = 0;
    public int ClimbLimit { get; set; } = 0;
}

[UsedImplicitly]
public class CommandData
{
    [JsonProperty("once_commands")] public List<string> OnceCommands;
    [JsonProperty("loop_commands")] public List<string> LoopCommands;
    [JsonProperty("loop_interval")] public float LoopInterval;
}

[UsedImplicitly]
public class WaypointData
{
    public string Id { get; set; }
    public Vector2 Position => new(X, Y);
    public float X { get; set; }
    public float Y { get; set; }
}

[UsedImplicitly]
public class ItemData
{
    public string Id { get; set; }
    public int Slot { get; set; }
    public bool Force { get; set; }
}

[UsedImplicitly]
public class XpData
{
    private readonly int[] _base = Skills.BaseSkills(0);

    [JsonProperty("str_xp")] public int StrXp { get; set; }
    [JsonProperty("res_xp")] public int ResXp { get; set; }
    [JsonProperty("int_xp")] public int IntXp { get; set; }

    [JsonProperty("exp_str")] public float ExpStr { get; set; }
    [JsonProperty("exp_res")] public float ExpRes { get; set; }
    [JsonProperty("exp_int")] public float ExpInt { get; set; }

    [JsonProperty("min_str")] public int MinStr { get; set; }
    [JsonProperty("max_str")] public int MaxStr { get; set; }

    [JsonProperty("min_res")] public int MinRes { get; set; }
    [JsonProperty("max_res")] public int MaxRes { get; set; }

    [JsonProperty("min_int")] public int MinInt { get; set; }
    [JsonProperty("max_int")] public int MaxInt { get; set; }

    [JsonProperty("xp_multiple")] public float XpMultiple { get; set; } = 1f;

    public XpData()
    {
        ResetToDefaults();
    }

    private void ResetToDefaults()
    {
        StrXp = _base[0];
        ResXp = _base[1];
        IntXp = _base[2];
        RecalculateMinMax();
    }

    private void RecalculateMinMax()
    {
        MinStr = Skills.GetExperienceForLevel(StrXp);
        MaxStr = Skills.GetExperienceForLevel(StrXp + 1);
        MinRes = Skills.GetExperienceForLevel(ResXp);
        MaxRes = Skills.GetExperienceForLevel(ResXp + 1);
        MinInt = Skills.GetExperienceForLevel(IntXp);
        MaxInt = Skills.GetExperienceForLevel(IntXp + 1);

        ExpStr = MinStr;
        ExpRes = MinRes;
        ExpInt = MinInt;
    }

    [OnDeserialized]
    private void OnDeserialized(StreamingContext context)
    {
        RecalculateMinMax();
    }
}