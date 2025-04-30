using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Utils;

namespace CardboardPlayer
{
    public partial class CardboardPlayer
    {
        private void DebugPrint(string message)
        {
            if (Config.Debug)
            {
                Console.WriteLine(Localizer["core.debugprint"].Value.Replace("{message}", message));
            }
        }

        private static float GetVectorDistance(Vector a, Vector b)
        {
            float dx = a.X - b.X;
            float dy = a.Y - b.Y;
            float dz = a.Z - b.Z;
            return MathF.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        private static bool CheckIfVectorInsideMinsMaxs(Vector position, Vector mins, Vector maxs, float minHeight = 100f)
        {
            // Check if position is within the axis-aligned bounding box defined by mins and maxs,
            // and ensure the height of the box is at least minHeight
            bool insideBox = position.X >= mins.X && position.X <= maxs.X
                && position.Y >= mins.Y && position.Y <= maxs.Y
                && position.Z >= mins.Z && position.Z <= maxs.Z;
            return insideBox;
        }

        private static QAngle GetLookAtAngle(Vector source, Vector target)
        {
            // Calculate direction vector from source to target
            Vector direction = new Vector(target.X - source.X, target.Y - source.Y, target.Z - source.Z);

            // Calculate yaw angle (horizontal rotation)
            // atan2 returns angle in radians, convert to degrees
            float yaw = (float)(Math.Atan2(direction.Y, direction.X) * 180 / Math.PI);

            // Adjust to CS2's coordinate system
            yaw = (yaw + 90) % 360;

            // We're only rotating on the Y-axis (horizontal plane)
            // Keep pitch and roll at 0
            return new QAngle(0, yaw, 0);
        }

        private static CDynamicProp? CreateProp(string model)
        {
            // create pole prop
            CDynamicProp? prop = Utilities.CreateEntityByName<CDynamicProp>("prop_dynamic_override");
            if (prop == null
                || !prop.IsValid) return null;
            // set attributes
            prop.MoveType = MoveType_t.MOVETYPE_NONE;
            prop.Collision.SolidType = SolidType_t.SOLID_BSP;
            prop.Collision.CollisionGroup = (byte)CollisionGroup.COLLISION_GROUP_BREAKABLE_GLASS;
            prop.Collision.CollisionAttribute.CollisionGroup = (byte)CollisionGroup.COLLISION_GROUP_BREAKABLE_GLASS;
            prop.DispatchSpawn();
            prop.SetModel(model);
            return prop;
        }

        private static void RemoveProp(CDynamicProp? prop)
        {
            if (prop == null
                || !prop.IsValid)
            {
                return;
            }

            prop.AcceptInput("Kill");
            prop.Remove();
            prop = null;
        }
    }
}