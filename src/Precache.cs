using CounterStrikeSharp.API.Modules.Utils;

namespace CardboardPlayer
{
    public partial class CardboardPlayer
    {
        private readonly List<string> _precacheModels =
        [
            "models/cs2/cardboard/peeko_dummy_generic.vmdl",
            "models/cs2/cardboard/peeko_dummy_ct_1.vmdl",
            "models/cs2/cardboard/peeko_dummy_ct_2.vmdl",
            "models/cs2/cardboard/peeko_dummy_ct_3.vmdl",
            "models/cs2/cardboard/peeko_dummy_ct_4.vmdl",
            "models/cs2/cardboard/peeko_dummy_t_2.vmdl",
            "models/cs2/cardboard/peeko_dummy_t_3.vmdl",
            "models/cs2/cardboard/peeko_dummy_t_4.vmdl"
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
