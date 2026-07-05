using Dalamud.Configuration;
using ECommons.DalamudServices;
using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace YhumiTweaks;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public bool AutoGlamWeddingRing { get; set; } = false;
    public int AutoGlamWeddingRingThrottleMs { get; set; } = 50;

    public bool AutoCorrectCameraHeight { get; set; } = false;
    public float SavedInstanceHeight { get; set; } = 60;
    public float SavedOutOfInstanceHeight { get; set; } = 20;

    // The below exists just to make saving less cumbersome
    public void Save()
    {
        Svc.Log.Info($"Saving config.");
        Svc.PluginInterface.SavePluginConfig(this);
    }

    public static Configuration Load()
    {
        try
        {
            var contents = File.ReadAllText(Svc.PluginInterface.ConfigFile.FullName);
            var json = JObject.Parse(contents);
            var version = (int?)json["Version"] ?? 0;
            return json.ToObject<Configuration>() ?? new();
        }
        catch (Exception e)
        {
            Svc.Log.Error($"Failed to load config from {Svc.PluginInterface.ConfigFile.FullName}: {e}");
            return new();
        }
    }
}
