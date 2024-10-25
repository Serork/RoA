using Microsoft.Xna.Framework;

using RoA.Common;
using RoA.Core.Utility;

using System;

using Terraria;
using Terraria.Audio;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;
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
        bool flag6 = false;
        if (!flag) {
            if (_altarStrength > 0f) {
                _altarStrength -= 0.01f;
            }
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
        float maxDist = 1000f;
        float distance2 = druidSoul.Distance(altarCoords);
        bool flag2 = distance2 < maxDist;
        bool flag3 = distance2 < radius;
        _altarFactor = MathHelper.SmoothStep(_altarFactor, flag3 ? 1f : flag2 ? (float)Math.Round(1f - (distance / maxDist) * 1.25f, 2) : 0f, Math.Max(0.1f, _altarFactor * 0.1f));
        _altarFactor = MathHelper.Clamp(_altarFactor, 0.01f, 1f);
        bool flag7 = druidSoul.As<DruidSoul>().ConsumesItself() && !flag6 && _altarFactor >= 0.85f;
        if (_altarStrength <= 0.6f && flag7 && _altarStrength >= 0.165f) {
            SoundEngine.PlaySound(SoundID.Zombie53 with { PitchVariance = 0.3f, Volume = 0.2f }, altarCoords);
            SoundEngine.PlaySound(SoundID.Zombie83 with { PitchVariance = 0.3f, Volume = 0.2f }, altarCoords);
        }
        _altarStrength += (float)Math.Round(TimeSystem.LogicDeltaTime / 5f * (flag7 ? 0.5f : (flag6 ? -1.65f : -2f)), 3);
        _altarStrength = MathHelper.Clamp(_altarStrength, 0f, 1f);
        if (_altarStrength >= 0.05f && _altarStrength <= 0.95f) {
            PunchCameraModifier punchCameraModifier = new(altarCoords, (Main.rand.NextFloat() * MathHelper.TwoPi).ToRotationVector2(), _altarStrength * 1.25f * Main.rand.NextFloat(5f, 10f), _altarStrength * 1.25f * Main.rand.NextFloat(3f, 4.5f), 20, 200f, "Lothor");
            Main.instance.CameraModifiers.Add(punchCameraModifier);
        }
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
