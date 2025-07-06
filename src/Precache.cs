using CounterStrikeSharp.API.Modules.Utils;

namespace CardboardPlayer
{
    public partial class CardboardPlayer
    {
        private readonly List<string> _precacheModels =
        [
            "models/cs2/cardboard/dummy_ct_1.vmdl",
            "models/cs2/cardboard/dummy_ct_2.vmdl",
            "models/cs2/cardboard/dummy_ct_3.vmdl",
            "models/cs2/cardboard/dummy_ct_4.vmdl",
            "models/cs2/cardboard/dummy_t_1.vmdl",
            "models/cs2/cardboard/dummy_t_2.vmdl",
            "models/cs2/cardboard/dummy_t_3.vmdl",
            "models/cs2/cardboard/dummy_t_4.vmdl",
            "models/cs2/kandru/motorbike.vmdl"
        ];

        private void OnServerPrecacheResources(ResourceManifest manifest)
        {
            foreach (string model in _precacheModels)
            {
                manifest.AddResource(model);
            }
        }
    }
}
