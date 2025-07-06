using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace CardboardPlayer
{
    public partial class CardboardPlayer : BasePlugin
    {
        public override string ModuleName => "CS2 CardboardPlayer";
        public override string ModuleAuthor => "Kalle <kalle@kandru.de>";

        private readonly Random _random = new();
        private List<CDynamicProp> _props = [];
        private IEnumerable<CBombTarget> _bombspots = [];
        private readonly List<string> _ctModels =
        [
            "models/cs2/cardboard/dummy_ct_1.vmdl",
            "models/cs2/cardboard/dummy_ct_3.vmdl",
            "models/cs2/cardboard/dummy_ct_4.vmdl"
        ];
        private readonly List<string> _ctBombModels =
        [
            "models/cs2/cardboard/dummy_ct_2.vmdl"
        ];
        private readonly List<string> _tModels =
        [
            "models/cs2/cardboard/dummy_t_1.vmdl",
            "models/cs2/cardboard/dummy_t_3.vmdl",
            "models/cs2/cardboard/dummy_t_4.vmdl"
        ];
        private readonly List<string> _tBombModels =
        [
            "models/cs2/cardboard/dummy_t_2.vmdl"
        ];
        private readonly List<string> _funModels =
        [
            "models/cs2/kandru/motorbike.vmdl"
        ];
        private readonly Dictionary<string, (string, float)> _soundList = new()
        {
            { "models/cs2/kandru/motorbike.vmdl", ("Motorbike.Engine01", 3f) }
        };

        public override void Load(bool hotReload)
        {
            RegisterEventHandler<EventRoundStart>(OnRoundStart);
            RegisterEventHandler<EventDecoyStarted>(OnDecoyStarted);
            RegisterListener<Listeners.OnServerPrecacheResources>(OnServerPrecacheResources);
            if (hotReload)
            {
                _bombspots = Utilities.FindAllEntitiesByDesignerName<CBombTarget>("func_bomb_target");
            }
        }

        public override void Unload(bool hotReload)
        {
            DeregisterEventHandler<EventRoundStart>(OnRoundStart);
            DeregisterEventHandler<EventDecoyStarted>(OnDecoyStarted);
            RemoveListener<Listeners.OnTick>(OnTick);
            RemoveListener<Listeners.OnServerPrecacheResources>(OnServerPrecacheResources);
        }

        private HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
        {
            DebugPrint("OnRoundStart");
            _bombspots = Utilities.FindAllEntitiesByDesignerName<CBombTarget>("func_bomb_target");
            return HookResult.Continue;
        }

        private HookResult OnDecoyStarted(EventDecoyStarted @event, GameEventInfo info)
        {
            DebugPrint("OnDecoyStarted");
            CCSPlayerController? player = @event.Userid;
            CDecoyProjectile? decoyProtectile = Utilities.GetEntityFromIndex<CDecoyProjectile>(@event.Entityid);
            if (decoyProtectile == null
                || !decoyProtectile.IsValid
                || decoyProtectile.AbsOrigin == null
                || player == null
                || !player.IsValid
                || player.PlayerPawn == null
                || !player.PlayerPawn.IsValid
                || player.PlayerPawn.Value == null)
            {
                DebugPrint("DecoyProjectile and/or player is null");
                return HookResult.Continue;
            }
            // get our prop
            string model = "";
            // check if decoyProjectile is inside a bomb spot
            foreach (CBombTarget bombspot in _bombspots)
            {
                if (bombspot.AbsOrigin == null) continue;
                // check if decoy grenade is inside bomb spot area
                if (CheckIfVectorInsideMinsMaxs(decoyProtectile.AbsOrigin, bombspot.AbsOrigin + bombspot.Collision.Mins, bombspot.AbsOrigin + bombspot.Collision.Maxs))
                {
                    if (player.Team == CsTeam.Terrorist)
                    {
                        model = _tBombModels[_random.Next(_tBombModels.Count)];
                    }
                    else if (player.Team == CsTeam.CounterTerrorist)
                    {
                        model = _ctBombModels[_random.Next(_ctBombModels.Count)];
                    }

                    break;
                }
            }
            // use normal model if not inside bomb spot
            if (string.IsNullOrEmpty(model))
            {
                if (player.Team == CsTeam.Terrorist)
                {
                    model = _tModels[_random.Next(_tModels.Count)];
                }
                else if (player.Team == CsTeam.CounterTerrorist)
                {
                    model = _ctModels[_random.Next(_ctModels.Count)];
                }
            }
            // randomly use a fun model
            if (_funModels.Count > 0 && _random.Next(100) < (Config.FunModelChance ?? 10))
            {
                model = _funModels[_random.Next(_funModels.Count)];
            }
            // stop if no model found
            if (string.IsNullOrEmpty(model))
            {
                return HookResult.Continue;
            }
            // create our prop
            CDynamicProp? prop;
            prop = CreateProp(model);
            // check prop
            if (prop == null
                || !prop.IsValid)
            {
                return HookResult.Continue;
            }
            // teleport prop to our location
            QAngle vAngle = player.PlayerPawn.Value.V_angle;
            QAngle propAngles = new(0, vAngle.Y + 180, 0);
            prop.Teleport(decoyProtectile.AbsOrigin, propAngles);
            // set prop team
            prop.TeamNum = player.Team == CsTeam.Terrorist ? (byte)(int)CsTeam.CounterTerrorist : (byte)(int)CsTeam.Terrorist;
            // give prop some health
            if (Config.PropHealth > 0)
            {
                prop.MaxHealth = Config.PropHealth;
                prop.Health = Config.PropHealth;
                Utilities.SetStateChanged(prop, "CBaseEntity", "m_iHealth");
                Utilities.SetStateChanged(prop, "CBaseEntity", "m_iMaxHealth");
                prop.TakesDamage = true;
                prop.TakeDamageFlags = TakeDamageFlags_t.DFLAG_ALWAYS_FIRE_DAMAGE_EVENTS;
            }
            // add prop to list a little bit later to let prop spawn
            _ = AddTimer(0.1f, () =>
            {
                if (prop == null
                    || !prop.IsValid)
                {
                    return;
                }
                if (!_funModels.Contains(model))
                {
                    // start ontick listener
                    if (_props.Count == 0)
                    {
                        RegisterListener<Listeners.OnTick>(OnTick);
                    }
                    _props.Add(prop);
                }
                // play sound if available
                if (_soundList.TryGetValue(model, out (string sound, float delay) soundInfo))
                {
                    _ = AddTimer(soundInfo.delay, () =>
                    {
                        if (prop == null
                            || !prop.IsValid)
                        {
                            return;
                        }
                        prop.EmitSound(soundInfo.sound);
                    });
                }
            });
            // remove grenade
            decoyProtectile.AcceptInput("kill");
            decoyProtectile.Remove();
            return HookResult.Continue;
        }

        private void OnTick()
        {
            // remove listener if no props are left
            if (_props.Count == 0)
            {
                RemoveListener<Listeners.OnTick>(OnTick);
                return;
            }
            // copy prop list
            List<CDynamicProp> props = [.. _props];
            // Get AbsOrigin from each valid player and return as list of Vectors
            Dictionary<int, List<Vector>> playerPositions = [];
            foreach (CCSPlayerController? player in Utilities.GetPlayers()
                .Where(static p => p.IsValid && !p.IsHLTV && p.PlayerPawn != null && p.PlayerPawn.IsValid && p.PlayerPawn.Value != null))
            {
                int teamNum = player.TeamNum;
                Vector? pos = player.PlayerPawn.Value!.AbsOrigin;
                if (!playerPositions.TryGetValue(teamNum, out List<Vector>? value))
                {
                    value = [];
                    playerPositions[teamNum] = value;
                }

                value.Add(pos!);
            }
            foreach (CDynamicProp prop in props)
            {
                // remove prop if not valid anymore
                if (prop == null
                    || !prop.IsValid
                    || prop.AbsOrigin == null
                    || !playerPositions.ContainsKey(prop.TeamNum))
                {
                    _ = _props.Remove(prop!);
                    continue;
                }
                // Find the closest player position to this prop
                Vector? closestPlayer = null;
                float minDistance = Config.MaxDistance;
                foreach (Vector playerPos in playerPositions[prop.TeamNum])
                {
                    float distance = GetVectorDistance(prop.AbsOrigin, playerPos);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        closestPlayer = playerPos;
                    }
                }
                if (closestPlayer == null)
                {
                    continue;
                }
                // look at the player
                QAngle baseAngle = GetLookAtAngle(prop.AbsOrigin, closestPlayer);
                QAngle lookAngle = new(baseAngle.X, baseAngle.Y, baseAngle.Z);
                // change angle of prop
                prop.Teleport(prop.AbsOrigin, lookAngle);
            }
        }
    }
}
