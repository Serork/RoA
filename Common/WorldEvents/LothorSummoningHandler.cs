using Microsoft.Xna.Framework;

using RoA.Content.Biomes.Backwoods;
using RoA.Content.NPCs.Enemies.Bosses.Lothor;
using RoA.Core;
using RoA.Utilities;

using System;

using Terraria;
using Terraria.Audio;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.WorldEvents;

sealed class LothorSummoningHandler : ModSystem {
    private float _preArrivedLothorBossTimer;
    private bool _shake, _shake2;

    internal static (bool, bool) PreArrivedLothorBoss;
    internal static (bool, bool, bool) ActiveMessages;

    public override void PostUpdateEverything() {
        if (!PreArrivedLothorBoss.Item1 || PreArrivedLothorBoss.Item2 || NPC.AnyNPCs(ModContent.NPCType<Lothor>())) {
            return;
        }

        _preArrivedLothorBossTimer += TimeSystem.LogicDeltaTime * 1.5f;
        Color color = new (160, 68, 234);
        if (_preArrivedLothorBossTimer >= 2.4f && !_shake) {
            _shake = true;
            NPC.SetEventFlagCleared(ref LothorShake.shake, -1);
            if (Main.netMode == NetmodeID.Server) {
                NetMessage.SendData(MessageID.WorldData);
            }
        }
        else if (_preArrivedLothorBossTimer >= 3f && !ActiveMessages.Item1) {
            ActiveMessages.Item1 = true;
            string text = "You noticed the smell of blood...";
            Helper.NewMessage(text, color);
            Shake(10f, 5f);
        }
        else if (_preArrivedLothorBossTimer >= 4.3f && !_shake2) {
            _shake2 = true;
            NPC.SetEventFlagCleared(ref LothorShake.shake, -1);
            if (Main.netMode == NetmodeID.Server) {
                NetMessage.SendData(MessageID.WorldData);
            }
        }
        else if (_preArrivedLothorBossTimer >= 5f && !ActiveMessages.Item2) {
            ActiveMessages.Item2 = true;
            string text = "Something is coming...";
            Helper.NewMessage(text, color);
            Shake(20f, 10f);
        }
        else if (_preArrivedLothorBossTimer >= 7f && !ActiveMessages.Item3) {
            ActiveMessages.Item3 = true;
            SoundEngine.PlaySound(new SoundStyle(ResourceManager.AmbientSounds + "LothorScream") { Volume = 0.5f }, AltarHandler.GetAltarPosition().ToWorldCoordinates());
        }
        //else if (_preArrivedLothorBossTimer >= 9.5f && !flag2) {
        //    Player spawnPlayer = Main.LocalPlayer;
        //    int type = ModContent.NPCType<Lothor>();
        //    float distance = -1f;
        //    foreach (Player player in Main.player) {
        //        if (player.active && player.InModBiome<BackwoodsBiome>()) {
        //            if (distance < player.Distance(tileCoords)) {
        //                distance = player.Distance(tileCoords);
        //                spawnPlayer = player;
        //            }
        //        }
        //    }
        //    NPC.SpawnOnPlayer(spawnPlayer.whoAmI, type);
        //    if (Main.netMode == NetmodeID.Server) {
        //        NetMessage.SendData(MessageID.SpawnBossUseLicenseStartEvent, -1, -1, null, spawnPlayer.whoAmI, type);
        //    }
        //    NPC.SetEventFlagCleared(ref preArrivedLothorBoss.Item2, -1);
        //    if (Main.netMode == NetmodeID.Server) {
        //        NetMessage.SendData(MessageID.WorldData);
        //    }
        //}
    }

    private static void Shake(float strength = 7.5f, float vibeStrength = 4.15f) {
        Vector2 tileCoords = AltarHandler.GetAltarPosition().ToWorldCoordinates();
        if (Main.netMode != NetmodeID.Server) {
            foreach (Player player in Main.player) {
                if (player.active && player.InModBiome<BackwoodsBiome>()) {
                    Vector2 position = player.Center - tileCoords;
                    position.Normalize();
                    PunchCameraModifier punchCameraModifier = new PunchCameraModifier(tileCoords, ((float)Math.Atan2(position.Y, position.X) + MathHelper.PiOver2).ToRotationVector2(), strength, vibeStrength, 20, 2000f, "Lothor Invasion");
                    Main.instance.CameraModifiers.Add(punchCameraModifier);
                }
            }
        }
    }
}
