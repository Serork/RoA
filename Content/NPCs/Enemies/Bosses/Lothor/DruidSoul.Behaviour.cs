using Microsoft.Xna.Framework;

using RoA.Common;
using RoA.Common.VisualEffects;
using RoA.Content.Biomes.Backwoods;
using RoA.Content.Tiles.Ambient;
using RoA.Content.VisualEffects;
using RoA.Core.Utility;
using RoA.Utilities;

using System;

using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace RoA.Content.NPCs.Enemies.Bosses.Lothor;

sealed partial class DruidSoul : RoANPC {
    private const float OPACITYACC = 0.005f;

    public override bool PreAI() {
        KillNPCIfIsntInBackwoods();

        return base.PreAI();
    }

    public override void AI() {
        if (Appearance()) {
            return;
        }

        UpdatePositionsAndRotation();
        NormalBehaviourHandler();
        AbsorbSoulHandler();
    }

    private bool Appearance() {
        if (StateTimer < 1f) {
            StateTimer += TimeSystem.LogicDeltaTime / 2f;
            float factor = Helper.EaseInOut3(StateTimer);
            NPC.Opacity = Math.Min(1f, factor / 2f);

            return true;
        }

        return false;
    }

    private bool ConsumesItself() {
        Vector2 altarPosition = GetAltarPosition();
        Vector2 npcCenter = NPC.Center;
        bool closeToAltar = Math.Abs(altarPosition.X - npcCenter.X) < 40f && altarPosition.Y - npcCenter.Y < 80f;
        return closeToAltar;
    }

    private static Vector2 GetAltarPosition() => AltarHandler.GetAltarPosition().ToWorldCoordinates();

    private void KillNPCIfIsntInBackwoods() {
        int target = NPC.target;
        if (target == 255 || target < 0 || Main.player[target].dead || !Main.player[target].active) {
            NPC.TargetClosest(false);
        }
        target = NPC.target;
        Player player = Main.player[target];
        if (player.dead || !player.InModBiome<BackwoodsBiome>() || NPC.CountNPCS(Type) > 1) {
            NPC.KillNPC();
            if (Main.netMode != NetmodeID.Server) {
                SoundEngine.PlaySound(SoundID.NPCDeath6 with { Pitch = 0.2f, Volume = 0.5f }, NPC.Center);
            }
        }
    }

    private void UpdatePositionsAndRotation() {
        float rotation = NPC.velocity.X * 0.05f;
        NPC.rotation = rotation;

        for (int i = 0; i < NPC.oldPos.Length; i++) {
            float randomOffset = Helper.Wave(0.25f, 1.5f, Main.rand.NextFloat(1f, 3f), 0.5f);
            Vector2 randomness = Main.rand.Random2(randomOffset);
            NPC.oldPos[i] += randomness;
        }
    }

    private void NormalBehaviourHandler() {
        if (ConsumesItself()) {
            return;
        }

        float minOpacity = 0.4f;
        if (NPC.Opacity > minOpacity) {
            NPC.Opacity -= OPACITYACC;
        }
    }

    private void AbsorbSoulHandler() {
        if (!ConsumesItself()) {
            return;
        }

        NPC.velocity *= 0.8f;

        SpawnEffects();
    }

    private void SpawnEffects() {
        Vector2 towards = GetAltarPosition() + new Vector2(4f, 40f);
        if (Main.netMode != NetmodeID.Server) {
            if (!Main.rand.NextBool(2)) {
                Vector2 center = NPC.position + new Vector2(3f + Main.rand.Next(NPC.width - 3), NPC.height / 2f + 8f);
                center.X += Main.rand.Next(-100, 100) * 0.05f;
                center.Y += Main.rand.Next(-100, 100) * 0.05f;
                VisualEffectSystem.New<SoulPart>(VisualEffectLayer.BEHINDNPCS).
                      SetupPart(1,
                                Vector2.Zero,
                                center + NPC.velocity,
                                towards + Main.rand.Random2(15f),
                                Main.rand.Next(70, 85) * Main.rand.NextFloat(0.01f, 0.015f),
                                0.8f);
            }
            if (!Main.rand.NextBool(3)) {
                Vector2 center = NPC.position + new Vector2(6f + Main.rand.Next(NPC.width - 6), NPC.height / 2f + 10f);
                center.X += Main.rand.Next(-100, 100) * 0.05f;
                center.Y += Main.rand.Next(-100, 100) * 0.05f;
                VisualEffectSystem.New<SoulPart>(VisualEffectLayer.BEHINDNPCS).
                      SetupPart(0,
                                Vector2.Zero,
                                center + NPC.velocity,
                                towards,
                                Main.rand.Next(70, 85) * 0.01f,
                                NPC.Opacity + 0.15f);
            }
        }
    }
}