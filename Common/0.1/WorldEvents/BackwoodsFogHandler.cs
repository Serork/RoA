using Microsoft.Xna.Framework;

using RoA.Common.Players;
using RoA.Common.VisualEffects;
using RoA.Content.Achievements;
using RoA.Content.Biomes.Backwoods;
using RoA.Core.Utility;

using System;
using System.IO;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace RoA.Common.WorldEvents;

sealed class BackwoodsFogHandler : ModSystem {
    // private class ActivateFog : ModCommand {
    //    public override CommandType Type => CommandType.World;
    //    public override string Command => "togglebackwoodsfog";
    //    public override string Usage => "/togglebackwoodsfog";

    //    public override void Action(CommandCaller caller, string input, string[] args) => ToggleBackwoodsFog(false);
    //}

    public override void Load() {
        On_Main.StopRain += On_Main_StopRain;
        On_Main.DrawInterface += On_Main_DrawInterface;
    }

    private void On_Main_DrawInterface(On_Main.orig_DrawInterface orig, Main self, GameTime gameTime) {
        orig(self, gameTime);
    }

    private void On_Main_StopRain(On_Main.orig_StopRain orig) {
        orig();
        if (Main.rand.NextBool() && !IsFogActive) {
            ToggleBackwoodsFog(false);
        }
    }

    private static float _fogTime;
    private static Vector2 _oldPosition;

    public static bool IsFogActive { get; private set; } = false;
    public static float Opacity { get; internal set; } = 0f;
    public static float Opacity2 { get; internal set; } = 0f;

    public override void OnWorldLoad() => Reset();
    public override void OnWorldUnload() => Reset();

    public override void SaveWorldData(TagCompound tag) {
        tag[RoA.ModName + "backwoods" + nameof(IsFogActive)] = IsFogActive;
        tag[RoA.ModName + "backwoods" + nameof(Opacity)] = Opacity;
        tag[RoA.ModName + "backwoods" + nameof(Opacity2)] = Opacity2;
        tag[RoA.ModName + "backwoods" + nameof(_fogTime)] = _fogTime;
    }

    public override void LoadWorldData(TagCompound tag) {
        IsFogActive = tag.GetBool(RoA.ModName + "backwoods" + nameof(IsFogActive));
        Opacity = tag.GetFloat(RoA.ModName + "backwoods" + nameof(Opacity));
        Opacity2 = tag.GetFloat(RoA.ModName + "backwoods" + nameof(Opacity2));
        _fogTime = tag.GetFloat(RoA.ModName + "backwoods" + nameof(_fogTime));
    }

    private static void Reset() {
        IsFogActive = false;
        Opacity = 0f;
        Opacity2 = 0f;
        _fogTime = 0f;
    }

    public override void NetSend(BinaryWriter writer) {
        writer.Write(IsFogActive);
        writer.Write(_fogTime);
    }

    public override void NetReceive(BinaryReader reader) {
        IsFogActive = reader.ReadBoolean();
        _fogTime = reader.ReadSingle();
    }

    internal static void ToggleBackwoodsFog(bool naturally = true) {
        if (!IsFogActive) {
            if ((naturally && Main.rand.NextChance(0.33)) || !naturally) {
                var rand = Main.rand;
                int num = 86400;
                int num2 = num / 24;
                int num3 = rand.Next(num2 * 8, num);
                if (rand.Next(3) == 0)
                    num3 += rand.Next(0, num2);

                if (rand.Next(4) == 0)
                    num3 += rand.Next(0, num2 * 2);

                if (rand.Next(5) == 0)
                    num3 += rand.Next(0, num2 * 2);

                if (rand.Next(6) == 0)
                    num3 += rand.Next(0, num2 * 3);

                if (rand.Next(7) == 0)
                    num3 += rand.Next(0, num2 * 4);

                if (rand.Next(8) == 0)
                    num3 += rand.Next(0, num2 * 5);

                float num4 = 1f;
                if (rand.Next(2) == 0)
                    num4 += 0.05f;

                if (rand.Next(3) == 0)
                    num4 += 0.1f;

                if (rand.Next(4) == 0)
                    num4 += 0.15f;

                if (rand.Next(5) == 0)
                    num4 += 0.2f;

                _fogTime = (int)((float)num3 * num4) * 0.5f;

                //string message = Language.GetText("Mods.RoA.World.BackwoodsFog").ToString();
                //Helper.NewMessage($"{message}...", Helper.EventMessageColor);
                IsFogActive = true;

                //foreach (Player player in Main.ActivePlayers) {
                //    if (!player.InModBiome<BackwoodsBiome>()) {
                //        continue;
                //    }
                //    if (player.whoAmI == Main.myPlayer) {
                //        ModContent.GetInstance<SurviveBackwoodsFog>().SurviveBackwoodsFogCondition.Complete();
                //        //RoA.CompleteAchievement("SurviveBackwoodsFog");
                //        //player.GetModPlayer<RoAAchievementInGameNotification.RoAAchievementStorage_Player>().SurviveBackwoodsFog = true;
                //    }
                //}
            }
        }
        else {
            IsFogActive = false;
            _fogTime = 0f;

            foreach (Player player in Main.ActivePlayers) {
                if (!player.InModBiome<BackwoodsBiome>()) {
                    continue;
                }
                if (player.whoAmI == Main.myPlayer) {
                    ModContent.GetInstance<SurviveBackwoodsFog>().SurviveBackwoodsFogCondition.Complete();
                    //RoA.CompleteAchievement("SurviveBackwoodsFog");
                    //player.GetModPlayer<RoAAchievementInGameNotification.RoAAchievementStorage_Player>().SurviveBackwoodsFog = true;
                }
            }
        }
        if (Main.netMode == NetmodeID.Server) {
            NetMessage.SendData(MessageID.WorldData);
        }
    }

    public override void PostUpdateNPCs() {
        if (IsFogActive) {
            _fogTime -= (float)Main.desiredWorldEventsUpdateRate;
            if (_fogTime <= 0f) {
                ToggleBackwoodsFog();
            }
        }
        //if (Main.dayTime && IsFogActive) {
        //    //if (Main.time < 1 || (Main.IsFastForwardingTime() && Main.time < 61)) {
        //    //    ToggleBackwoodsFog();
        //    //}
        //}
    }

    public override void PostUpdatePlayers() {
        if (Opacity > 0f) {
            Player player = Main.LocalPlayer;
            VignettePlayer localVignettePlayer = player.GetModPlayer<VignettePlayer>();
            if (!player.InModBiome<BackwoodsBiome>() && Opacity < 0.01f) {
                Opacity = 0f;
                if (Main.netMode != NetmodeID.Server) {
                    localVignettePlayer.SetVignette(0, 0, Opacity * Opacity2, Color.Gray * Opacity * Opacity2, Vector2.Zero);
                }
                return;
            }
            if (Main.netMode != NetmodeID.Server) {
                if (player.position.Y / 16 < Main.worldSurface) {
                    if (Opacity2 < 0.75f) {
                        Opacity2 += 0.0175f * 0.15f;
                    }
                    else {
                        Opacity2 = 0.75f;
                    }
                }
                else {
                    if (Opacity2 > 0f) {
                        Opacity2 -= 0.005f * 0.25f;
                    }
                    else {
                        Opacity2 = 0f;
                    }
                }
                localVignettePlayer.SetVignette(0, 0, Opacity * Opacity2, Color.Gray * Opacity * Opacity2, Vector2.Zero);
            }

            Rectangle tileWorkSpace = GetTileWorkSpace();
            int num = tileWorkSpace.X + tileWorkSpace.Width;
            int num2 = tileWorkSpace.Y + tileWorkSpace.Height;
            for (int i = tileWorkSpace.X; i < num; i++) {
                for (int j = tileWorkSpace.Y; j < num2; j++) {
                    if (j < Main.worldSurface) {
                        TrySpawnFog(i, j);
                    }
                }
            }

            if (player.whoAmI == Main.myPlayer && Vector2.Distance(_oldPosition, player.position) > 600f && !player.InModBiome<BackwoodsBiome>()) {
                Opacity = Opacity2 = 0.01f;
            }
        }

        if (!BackwoodsBiome.IsActiveForFogEffect || !IsFogActive) {
            if (Opacity > 0f) {
                Opacity -= 0.005f * 0.25f;
            }
            else {
                Opacity = 0f;
            }

            if (Opacity2 > 0f) {
                Opacity2 -= 0.005f * 0.25f;
            }
            else {
                Opacity2 = 0f;
            }

            return;
        }

        if (Opacity < 0.75f) {
            Opacity += 0.0175f * 0.15f;
        }
        else {
            Opacity = 0.75f;
        }

        _oldPosition = Main.LocalPlayer.position;
    }

    private Rectangle GetTileWorkSpace() {
        Point point = Main.LocalPlayer.Center.ToTileCoordinates();
        int num = 120;
        int num2 = 70;
        return new Rectangle(point.X - num / 2, point.Y - num2 / 2, num, num2);
    }

    private void TrySpawnFog(int x, int y) {
        Tile tile = WorldGenHelper.GetTileSafely(x, y);
        //if (y >= Main.worldSurface) {
        //    return;
        //}
        if (!tile.HasTile/* || tile.Slope > 0 || tile.IsHalfBlock*/ || !Main.tileSolid[tile.TileType]) {
            return;
        }
        ////if (TileID.Sets.Platforms[tile.TileType]) {
        ////    return;
        ////}
        //tile = WorldGenHelper.GetTileSafely(x, y + 1);
        ////if (!tile.AnyWall() && !WorldGenHelper.GetTileSafely(x, y).AnyWall()) {
        ////    return;
        ////}
        tile = WorldGenHelper.GetTileSafely(x, y - 1);
        //int type = ModContent.TileType<OvergrownAltar>();

        //bool flag = true;
        ////for (int i = -1; i < 2; i++) {
        ////    for (int j = -1; j < 2; j++) {
        ////        if (i != 0 || j != 0) {
        ////            if (WorldGenHelper.GetTileSafely(x + i, y + j).TileType == type) {
        ////                flag = false;
        ////                break;
        ////            }
        ////        }
        ////    }
        ////}
        if (!WorldGen.SolidTile(tile) && Main.rand.NextBool((int)(20 + 40 * (1f - Math.Clamp(Opacity / 0.75f, 0f, 1f))))) {
            SpawnFloorCloud(x, y);
            if (Main.rand.NextBool(3)) {
                SpawnFloorCloud(x, y - 1);
            }
        }
    }

    private void SpawnFloorCloud(int x, int y) {
        Color lightColor = Lighting.GetColor(x, y);
        float brightness = (lightColor.R / 255f + lightColor.G / 255f + lightColor.B / 255f) / 3f;
        float brightness2 = MathHelper.Clamp((brightness - 0.6f) * 5f, 0f, 1f);
        if (Main.rand.NextChance(1f - brightness2)) {
            Point16 tilePosition = new(x, y - 1);
            if (Main.wallHouse[WorldGenHelper.GetTileSafely(x, y - 1).WallType]) {
                return;
            }
            Vector2 position = tilePosition.ToWorldCoordinates();
            float num = 16f * Main.rand.NextFloat();
            position.Y -= num;
            float num2 = 0.4f;
            float scale = 0.8f + Main.rand.NextFloat() * 0.2f;

            if (Main.netMode != NetmodeID.Server) {
                VisualEffectSystem.New<Content.VisualEffects.Fog>(VisualEffectLayer.ABOVEPLAYERS)?.
                    Setup(position + Main.rand.RandomPointInArea(5f, 5f), new Vector2(num2 * Main.WindForVisuals, 0f), scale: scale);
            }
        }
    }
}
