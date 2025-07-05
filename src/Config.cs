using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Extensions;
using System.Text.Json.Serialization;

namespace CardboardPlayer
{
    public class PluginConfig : BasePluginConfig
    {
        // disabled
        [JsonPropertyName("enabled")] public bool Enabled { get; set; } = true;
        // debug prints
        [JsonPropertyName("debug")] public bool Debug { get; set; } = false;
        // prop health
        [JsonPropertyName("prop_health")] public int PropHealth { get; set; } = 25;
        // maximum distance to prop for movement
        [JsonPropertyName("max_distance")] public float MaxDistance { get; set; } = 300f;
        // chance to use a fun model (default 10%)
        [JsonPropertyName("fun_model_chance")] public int? FunModelChance { get; set; } = 10;
    }

    public partial class CardboardPlayer : BasePlugin, IPluginConfig<PluginConfig>
    {
        public required PluginConfig Config { get; set; }

        public void OnConfigParsed(PluginConfig config)
        {
            Config = config;
            // update config and write new values from plugin to config file if changed after update
            Config.Update();
            Console.WriteLine(Localizer["core.config"]);
        }
    }
}
