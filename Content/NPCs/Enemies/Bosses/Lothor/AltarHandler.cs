using Microsoft.Xna.Framework;

using System;

using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace RoA.Content.NPCs.Enemies.Bosses.Lothor;

sealed class AltarHandler : ModSystem {
    private static Point _altarPosition;
    private static float _altarStrength, _altarFactor;

    internal static void SetPosition(Point altarPosition) {
        if (!WorldGen.gen) {
            return;
        }

        _altarPosition = altarPosition;
    }

    public static Point GetAltarPosition() => _altarPosition;
    public static float GetAltarStrength() => _altarStrength;
    public static float GetAltarFactor() => _altarFactor;

    public override void PostUpdateNPCs() {
        if (!NPC.downedBoss2) {
            return;
        }

        Vector2 altarCoords = GetAltarPosition().ToWorldCoordinates();
        int type = ModContent.NPCType<DruidSoul>();
        NPC druidSoul = null;
        bool flag = NPC.AnyNPCs(type);
        if (!flag) {
            if (_altarFactor > 0f) {
                _altarFactor -= 0.01f;
            }
            return;
        }

        foreach (NPC npc in Main.ActiveNPCs) {
            if (npc.active && npc.type == type) {
                druidSoul = npc;
                break;
            }
        }
        if (druidSoul is null) {
            return;
        }
        Vector2 npcCenter = druidSoul.Center;
        float radius = 100f;
        float x = npcCenter.X - altarCoords.X;
        float y = npcCenter.Y - altarCoords.Y;
        float distance = (float)Math.Sqrt(x * x + y * y);
        float maxDist = 750f;
        float distance2 = druidSoul.Distance(altarCoords);
        bool flag2 = distance2 < maxDist;
        bool flag3 = distance2 < radius;
        _altarFactor = MathHelper.Lerp(_altarFactor, flag3 ? 1f : flag2 ? (float)Math.Round(1f - (distance / maxDist) * 1.25f, 2) : 0f, Math.Max(0.01f, _altarFactor * 0.1f));
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
