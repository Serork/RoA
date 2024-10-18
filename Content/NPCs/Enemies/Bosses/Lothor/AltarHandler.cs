using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace RoA.Content.NPCs.Enemies.Bosses.Lothor;

sealed class AltarHandler : ModSystem {
    private static Point _altarPosition;
    private static float _altarStrength;

    internal static void SetPosition(Point altarPosition) {
        if (!WorldGen.gen) {
            return;
        }

        _altarPosition = altarPosition;
    }

    public static Point GetAltarPosition() => _altarPosition;
    public static float GetAltarStrength() => _altarStrength;

    public override void PostUpdateNPCs() {
        
    }

    public override void OnWorldLoad() => Reset();
    public override void OnWorldUnload() => Reset();
    private static void Reset() {
        _altarPosition = Point.Zero;
        _altarStrength = 0f;
    }

    public override void SaveWorldData(TagCompound tag) {
        tag.Add(nameof(_altarPosition) + "x", _altarPosition.X);
        tag.Add(nameof(_altarPosition) + "y", _altarPosition.Y);
        tag.Add(nameof(_altarStrength), _altarStrength);
    }

    public override void LoadWorldData(TagCompound tag) {
        _altarPosition = new Point(tag.GetInt(nameof(_altarPosition) + "x"), tag.GetInt(nameof(_altarPosition) + "y"));
        _altarStrength = tag.GetFloat(nameof(_altarStrength));
    }
}
