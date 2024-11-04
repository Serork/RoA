using Microsoft.Xna.Framework;

using RoA.Content.Biomes.Backwoods;
using RoA.Content.NPCs.Enemies.Bosses.Lothor;
using RoA.Core;
using RoA.Utilities;

using System;

using Terraria;
using Terraria.Audio;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.WorldEvents;

sealed class LothorSummoningHandler : ModSystem {
    private float _preArrivedLothorBossTimer;
    private bool _shake, _shake2;
    private float _alpha;

    internal static (bool, bool) PreArrivedLothorBoss;
    internal static (bool, bool, bool) ActiveMessages;

    public override void Load() {
        On_Main.SetBackColor += On_Main_SetBackColor;
    }

    private void On_Main_SetBackColor(On_Main.orig_SetBackColor orig, Main.InfoToSetBackColor info, out Color sunColor, out Color moonColor) {
        if ((!(PreArrivedLothorBoss.Item1 || PreArrivedLothorBoss.Item2) || !Main.LocalPlayer.InModBiome<BackwoodsBiome>()) && _alpha <= 0f) {
            orig(info, out sunColor, out moonColor);
            return;
        }

        bool flag = Main.LocalPlayer.InModBiome<BackwoodsBiome>();
        if (!flag) {
            if (_alpha > 0f) {
                _alpha -= TimeSystem.LogicDeltaTime * 2f;
            }
        }
        else {
            if (_alpha < 1f) {
                _alpha += TimeSystem.LogicDeltaTime;
            }
        }
        double num = (float)MathHelper.Lerp((float)Main.time, 16200, _alpha);
        bool isDayTime = flag || Main.dayTime;
        Microsoft.Xna.Framework.Color bgColorToSet = Microsoft.Xna.Framework.Color.White;
        sunColor = Microsoft.Xna.Framework.Color.White;
        moonColor = Microsoft.Xna.Framework.Color.White;
        float num2 = 0f;
        bool isInGameMenuOrIsServer = info.isInGameMenuOrIsServer;
        if (isDayTime) {
            if (num < 13500.0) {
                num2 = (float)(num / 13500.0);
                sunColor.R = (byte)(num2 * 200f + 55f);
                sunColor.G = (byte)(num2 * 180f + 75f);
                sunColor.B = (byte)(num2 * 250f + 5f);
                bgColorToSet.R = (byte)(num2 * 230f + 25f);
                bgColorToSet.G = (byte)(num2 * 220f + 35f);
                bgColorToSet.B = (byte)(num2 * 220f + 35f);
            }

            if (num > 45900.0) {
                num2 = (float)(1.0 - (num / 54000.0 - 0.85) * 6.666666666666667);
                sunColor.R = (byte)(num2 * 120f + 55f);
                sunColor.G = (byte)(num2 * 100f + 25f);
                sunColor.B = (byte)(num2 * 120f + 55f);
                bgColorToSet.R = (byte)(num2 * 200f + 35f);
                bgColorToSet.G = (byte)(num2 * 85f + 35f);
                bgColorToSet.B = (byte)(num2 * 135f + 35f);
            }
            else if (num > 37800.0) {
                num2 = (float)(1.0 - (num / 54000.0 - 0.7) * 6.666666666666667);
                sunColor.R = (byte)(num2 * 80f + 175f);
                sunColor.G = (byte)(num2 * 130f + 125f);
                sunColor.B = (byte)(num2 * 100f + 155f);
                bgColorToSet.R = (byte)(num2 * 20f + 235f);
                bgColorToSet.G = (byte)(num2 * 135f + 120f);
                bgColorToSet.B = (byte)(num2 * 85f + 170f);
            }
        }

        if (!isDayTime) {
            if (info.BloodMoonActive) {
                if (num < 16200.0) {
                    num2 = (float)(1.0 - num / 16200.0);
                    moonColor.R = (byte)(num2 * 10f + 205f);
                    moonColor.G = (byte)(num2 * 170f + 55f);
                    moonColor.B = (byte)(num2 * 200f + 55f);
                    bgColorToSet.R = (byte)(40f - num2 * 40f + 35f);
                    bgColorToSet.G = (byte)(num2 * 20f + 15f);
                    bgColorToSet.B = (byte)(num2 * 20f + 15f);
                }
                else if (num >= 16200.0) {
                    num2 = (float)((num / 32400.0 - 0.5) * 2.0);
                    moonColor.R = (byte)(num2 * 50f + 205f);
                    moonColor.G = (byte)(num2 * 100f + 155f);
                    moonColor.B = (byte)(num2 * 100f + 155f);
                    moonColor.R = (byte)(num2 * 10f + 205f);
                    moonColor.G = (byte)(num2 * 170f + 55f);
                    moonColor.B = (byte)(num2 * 200f + 55f);
                    bgColorToSet.R = (byte)(40f - num2 * 40f + 35f);
                    bgColorToSet.G = (byte)(num2 * 20f + 15f);
                    bgColorToSet.B = (byte)(num2 * 20f + 15f);
                }
            }
            else if (num < 16200.0) {
                num2 = (float)(1.0 - num / 16200.0);
                moonColor.R = (byte)(num2 * 10f + 205f);
                moonColor.G = (byte)(num2 * 70f + 155f);
                moonColor.B = (byte)(num2 * 100f + 155f);
                bgColorToSet.R = (byte)(num2 * 30f + 5f);
                bgColorToSet.G = (byte)(num2 * 30f + 5f);
                bgColorToSet.B = (byte)(num2 * 30f + 5f);
            }
            else if (num >= 16200.0) {
                num2 = (float)((num / 32400.0 - 0.5) * 2.0);
                moonColor.R = (byte)(num2 * 50f + 205f);
                moonColor.G = (byte)(num2 * 100f + 155f);
                moonColor.B = (byte)(num2 * 100f + 155f);
                bgColorToSet.R = (byte)(num2 * 20f + 5f);
                bgColorToSet.G = (byte)(num2 * 30f + 5f);
                bgColorToSet.B = (byte)(num2 * 30f + 5f);
            }

            if (Main.dontStarveWorld)
                DontStarveSeed.ModifyNightColor(ref bgColorToSet, ref moonColor);
        }

        if (Main.cloudAlpha > 0f && !Main.remixWorld) {
            float num3 = 1f - Main.cloudAlpha * 0.9f * Main.atmo;
            bgColorToSet.R = (byte)((float)(int)bgColorToSet.R * num3);
            bgColorToSet.G = (byte)((float)(int)bgColorToSet.G * num3);
            bgColorToSet.B = (byte)((float)(int)bgColorToSet.B * num3);
        }

        if (info.GraveyardInfluence > 0f && !Main.remixWorld) {
            float num4 = 1f - info.GraveyardInfluence * 0.6f;
            bgColorToSet.R = (byte)((float)(int)bgColorToSet.R * num4);
            bgColorToSet.G = (byte)((float)(int)bgColorToSet.G * num4);
            bgColorToSet.B = (byte)((float)(int)bgColorToSet.B * num4);
        }

        if (isInGameMenuOrIsServer && !isDayTime) {
            bgColorToSet.R = 35;
            bgColorToSet.G = 35;
            bgColorToSet.B = 35;
        }

        byte minimalLight = 15;
        switch (Main.GetMoonPhase()) {
            case MoonPhase.Empty:
                minimalLight = 11;
                break;
            case MoonPhase.QuarterAtLeft:
            case MoonPhase.QuarterAtRight:
                minimalLight = 13;
                break;
            case MoonPhase.HalfAtLeft:
            case MoonPhase.HalfAtRight:
                minimalLight = 15;
                break;
            case MoonPhase.ThreeQuartersAtLeft:
            case MoonPhase.ThreeQuartersAtRight:
                minimalLight = 17;
                break;
            case MoonPhase.Full:
                minimalLight = 19;
                break;
        }

        if (Main.dontStarveWorld)
            DontStarveSeed.ModifyMinimumLightColorAtNight(ref minimalLight);

        if (bgColorToSet.R < minimalLight)
            bgColorToSet.R = minimalLight;

        if (bgColorToSet.G < minimalLight)
            bgColorToSet.G = minimalLight;

        if (bgColorToSet.B < minimalLight)
            bgColorToSet.B = minimalLight;

        if (info.BloodMoonActive) {
            if (bgColorToSet.R < 25)
                bgColorToSet.R = 25;

            if (bgColorToSet.G < 25)
                bgColorToSet.G = 25;

            if (bgColorToSet.B < 25)
                bgColorToSet.B = 25;
        }

        if (Main.eclipse && isDayTime) {
            float num8 = 1242f;
            Main.eclipseLight = (float)(num / (double)num8);
            if (Main.eclipseLight > 1f)
                Main.eclipseLight = 1f;
        }
        else if (Main.eclipseLight > 0f) {
            Main.eclipseLight -= 0.01f;
            if (Main.eclipseLight < 0f)
                Main.eclipseLight = 0f;
        }

        if (Main.eclipseLight > 0f) {
            float num9 = 1f - 0.925f * Main.eclipseLight;
            float num10 = 1f - 0.96f * Main.eclipseLight;
            float num11 = 1f - 1f * Main.eclipseLight;
            int num12 = (int)((float)(int)bgColorToSet.R * num9);
            int num13 = (int)((float)(int)bgColorToSet.G * num10);
            int num14 = (int)((float)(int)bgColorToSet.B * num11);
            bgColorToSet.R = (byte)num12;
            bgColorToSet.G = (byte)num13;
            bgColorToSet.B = (byte)num14;
            sunColor.R = byte.MaxValue;
            sunColor.G = 127;
            sunColor.B = 67;
            if (bgColorToSet.R < 20)
                bgColorToSet.R = 20;

            if (bgColorToSet.G < 10)
                bgColorToSet.G = 10;

            if (!Lighting.NotRetro) {
                if (bgColorToSet.R < 20)
                    bgColorToSet.R = 20;

                if (bgColorToSet.G < 14)
                    bgColorToSet.G = 14;

                if (bgColorToSet.B < 6)
                    bgColorToSet.B = 6;
            }
        }

        if ((Main.remixWorld && !Main.gameMenu) || WorldGen.remixWorldGen) {
            bgColorToSet.R = 1;
            bgColorToSet.G = 1;
            bgColorToSet.B = 1;
        }

        if (Main.lightning > 0f) {
            float value = (float)(int)bgColorToSet.R / 255f;
            float value2 = (float)(int)bgColorToSet.G / 255f;
            float value3 = (float)(int)bgColorToSet.B / 255f;
            value = MathHelper.Lerp(value, 1f, Main.lightning);
            value2 = MathHelper.Lerp(value2, 1f, Main.lightning);
            value3 = MathHelper.Lerp(value3, 1f, Main.lightning);
            bgColorToSet.R = (byte)(value * 255f);
            bgColorToSet.G = (byte)(value2 * 255f);
            bgColorToSet.B = (byte)(value3 * 255f);
        }

        if (!info.BloodMoonActive)
            moonColor = Microsoft.Xna.Framework.Color.White;

        Main.ColorOfTheSkies = bgColorToSet;
    }

    public override void PostUpdateEverything() {
        if (!PreArrivedLothorBoss.Item1 || PreArrivedLothorBoss.Item2 || NPC.AnyNPCs(ModContent.NPCType<Lothor>())) {
            return;
        }

        _preArrivedLothorBossTimer += TimeSystem.LogicDeltaTime * 1f;
        Color color = new (160, 68, 234);
        if (_preArrivedLothorBossTimer >= 2.465f && !_shake) {
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
        else if (_preArrivedLothorBossTimer >= 4.262f && !_shake2) {
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
        else if (_preArrivedLothorBossTimer >= 6f && !ActiveMessages.Item3) {
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
