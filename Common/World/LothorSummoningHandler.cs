using Microsoft.Xna.Framework;

using RoA.Common.Networking;
using RoA.Common.Networking.Packets;
using RoA.Content.Biomes.Backwoods;
using RoA.Content.NPCs.Enemies.Bosses.Lothor;
using RoA.Content.NPCs.Enemies.Bosses.Lothor.Summon;
using RoA.Core;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;

using System;
using System.IO;

using Terraria;
using Terraria.Audio;
using Terraria.Chat;
using Terraria.GameContent;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace RoA.Common.World;

sealed class LothorSummoningHandler : ModSystem {
    private static float _preArrivedLothorBossTimer;
    private static bool _shake, _shake2;

    internal static bool _summonedNaturally;

    internal static float _alpha;

    internal static (bool, bool) PreArrivedLothorBoss;
    internal static (bool, bool, bool) ActiveMessages;

    internal static bool IsActive => _preArrivedLothorBossTimer >= 6f;

    public override void OnWorldLoad() => Reset();
    public override void OnWorldUnload() => Reset();

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
                _alpha -= TimeSystem.LogicDeltaTime * 0.7f;
                _alpha *= 0.99f;
            }
        }
        else {
            if (_alpha < 1f) {
                _alpha += TimeSystem.LogicDeltaTime * 0.27f;
                _alpha *= 1.01f;
            }
        }

        orig(info, out sunColor, out moonColor);
        return;
        //bool isDayTime = Main.dayTime;
        //double num = (float)MathHelper.Lerp((float)Main.time, 27000, _alpha);
        //Microsoft.Xna.Framework.Color bgColorToSet = Microsoft.Xna.Framework.Color.White;
        //sunColor = Microsoft.Xna.Framework.Color.White;
        //moonColor = Microsoft.Xna.Framework.Color.White;
        //bool isInGameMenuOrIsServer = info.isInGameMenuOrIsServer;
        //float num2 = 0f;
        //if (isDayTime) {
        //    if (num < 13500.0) {
        //        num2 = (float)(num / 13500.0);
        //        sunColor.R = (byte)(num2 * 200f + 55f);
        //        sunColor.G = (byte)(num2 * 180f + 75f);
        //        sunColor.B = (byte)(num2 * 250f + 5f);
        //        bgColorToSet.R = (byte)(num2 * 230f + 25f);
        //        bgColorToSet.G = (byte)(num2 * 220f + 35f);
        //        bgColorToSet.B = (byte)(num2 * 220f + 35f);
        //    }

        //    if (num > 45900.0) {
        //        num2 = (float)(1.0 - (num / 54000.0 - 0.85) * 6.666666666666667);
        //        sunColor.R = (byte)(num2 * 120f + 55f);
        //        sunColor.G = (byte)(num2 * 100f + 25f);
        //        sunColor.B = (byte)(num2 * 120f + 55f);
        //        bgColorToSet.R = (byte)(num2 * 200f + 35f);
        //        bgColorToSet.G = (byte)(num2 * 85f + 35f);
        //        bgColorToSet.B = (byte)(num2 * 135f + 35f);
        //    }
        //    else if (num > 37800.0) {
        //        num2 = (float)(1.0 - (num / 54000.0 - 0.7) * 6.666666666666667);
        //        sunColor.R = (byte)(num2 * 80f + 175f);
        //        sunColor.G = (byte)(num2 * 130f + 125f);
        //        sunColor.B = (byte)(num2 * 100f + 155f);
        //        bgColorToSet.R = (byte)(num2 * 20f + 235f);
        //        bgColorToSet.G = (byte)(num2 * 135f + 120f);
        //        bgColorToSet.B = (byte)(num2 * 85f + 170f);
        //    }
        //}

        //if (!isDayTime) {
        //    if (info.BloodMoonActive) {
        //        if (num < 16200.0) {
        //            num2 = (float)(1.0 - num / 16200.0);
        //            moonColor.R = (byte)(num2 * 10f + 205f);
        //            moonColor.G = (byte)(num2 * 170f + 55f);
        //            moonColor.B = (byte)(num2 * 200f + 55f);
        //            bgColorToSet.R = (byte)(40f - num2 * 40f + 35f);
        //            bgColorToSet.G = (byte)(num2 * 20f + 15f);
        //            bgColorToSet.B = (byte)(num2 * 20f + 15f);
        //        }
        //        else if (num >= 16200.0) {
        //            num2 = (float)((num / 32400.0 - 0.5) * 2.0);
        //            moonColor.R = (byte)(num2 * 50f + 205f);
        //            moonColor.G = (byte)(num2 * 100f + 155f);
        //            moonColor.B = (byte)(num2 * 100f + 155f);
        //            moonColor.R = (byte)(num2 * 10f + 205f);
        //            moonColor.G = (byte)(num2 * 170f + 55f);
        //            moonColor.B = (byte)(num2 * 200f + 55f);
        //            bgColorToSet.R = (byte)(40f - num2 * 40f + 35f);
        //            bgColorToSet.G = (byte)(num2 * 20f + 15f);
        //            bgColorToSet.B = (byte)(num2 * 20f + 15f);
        //        }
        //    }
        //    else if (num < 16200.0) {
        //        num2 = (float)(1.0 - num / 16200.0);
        //        moonColor.R = (byte)(num2 * 10f + 205f);
        //        moonColor.G = (byte)(num2 * 70f + 155f);
        //        moonColor.B = (byte)(num2 * 100f + 155f);
        //        bgColorToSet.R = (byte)(num2 * 30f + 5f);
        //        bgColorToSet.G = (byte)(num2 * 30f + 5f);
        //        bgColorToSet.B = (byte)(num2 * 30f + 5f);
        //    }
        //    else if (num >= 16200.0) {
        //        num2 = (float)((num / 32400.0 - 0.5) * 2.0);
        //        moonColor.R = (byte)(num2 * 50f + 205f);
        //        moonColor.G = (byte)(num2 * 100f + 155f);
        //        moonColor.B = (byte)(num2 * 100f + 155f);
        //        bgColorToSet.R = (byte)(num2 * 20f + 5f);
        //        bgColorToSet.G = (byte)(num2 * 30f + 5f);
        //        bgColorToSet.B = (byte)(num2 * 30f + 5f);
        //    }

        //    if (Main.dontStarveWorld)
        //        DontStarveSeed.ModifyNightColor(ref bgColorToSet, ref moonColor);
        //}

        //if (Main.cloudAlpha > 0f && !Main.remixWorld) {
        //    float num3 = 1f - Main.cloudAlpha * 0.9f * Main.atmo;
        //    bgColorToSet.R = (byte)((float)(int)bgColorToSet.R * num3);
        //    bgColorToSet.G = (byte)((float)(int)bgColorToSet.G * num3);
        //    bgColorToSet.B = (byte)((float)(int)bgColorToSet.B * num3);
        //}

        //if (info.GraveyardInfluence > 0f && !Main.remixWorld) {
        //    float num4 = 1f - info.GraveyardInfluence * 0.6f;
        //    bgColorToSet.R = (byte)((float)(int)bgColorToSet.R * num4);
        //    bgColorToSet.G = (byte)((float)(int)bgColorToSet.G * num4);
        //    bgColorToSet.B = (byte)((float)(int)bgColorToSet.B * num4);
        //}

        //if (isInGameMenuOrIsServer && !isDayTime) {
        //    bgColorToSet.R = 35;
        //    bgColorToSet.G = 35;
        //    bgColorToSet.B = 35;
        //}

        //if (info.CorruptionBiomeInfluence > 0f) {
        //    float num5 = info.CorruptionBiomeInfluence;
        //    if (num5 > 1f)
        //        num5 = 1f;

        //    int r = bgColorToSet.R;
        //    int g = bgColorToSet.G;
        //    int b = bgColorToSet.B;
        //    r -= (int)(90f * num5 * ((float)(int)bgColorToSet.R / 255f));
        //    g -= (int)(140f * num5 * ((float)(int)bgColorToSet.G / 255f));
        //    b -= (int)(70f * num5 * ((float)(int)bgColorToSet.B / 255f));
        //    if (r < 15)
        //        r = 15;

        //    if (g < 15)
        //        g = 15;

        //    if (b < 15)
        //        b = 15;

        //    DontStarveSeed.FixBiomeDarkness(ref bgColorToSet, ref r, ref g, ref b);
        //    bgColorToSet.R = (byte)r;
        //    bgColorToSet.G = (byte)g;
        //    bgColorToSet.B = (byte)b;
        //    r = sunColor.R;
        //    g = sunColor.G;
        //    b = sunColor.B;
        //    r -= (int)(100f * num5 * ((float)(int)sunColor.R / 255f));
        //    g -= (int)(100f * num5 * ((float)(int)sunColor.G / 255f));
        //    b -= (int)(0f * num5 * ((float)(int)sunColor.B / 255f));
        //    if (r < 15)
        //        r = 15;

        //    if (g < 15)
        //        g = 15;

        //    if (b < 15)
        //        b = 15;

        //    sunColor.R = (byte)r;
        //    sunColor.G = (byte)g;
        //    sunColor.B = (byte)b;
        //}

        //if (info.CrimsonBiomeInfluence > 0f) {
        //    float num6 = info.CrimsonBiomeInfluence;
        //    if (num6 > 1f)
        //        num6 = 1f;

        //    int r2 = bgColorToSet.R;
        //    int g2 = bgColorToSet.G;
        //    int b2 = bgColorToSet.B;
        //    r2 -= (int)(40f * num6 * ((float)(int)bgColorToSet.G / 255f));
        //    g2 -= (int)(110f * num6 * ((float)(int)bgColorToSet.G / 255f));
        //    b2 -= (int)(140f * num6 * ((float)(int)bgColorToSet.B / 255f));
        //    if (r2 < 15)
        //        r2 = 15;

        //    if (g2 < 15)
        //        g2 = 15;

        //    if (b2 < 15)
        //        b2 = 15;

        //    DontStarveSeed.FixBiomeDarkness(ref bgColorToSet, ref r2, ref g2, ref b2);
        //    bgColorToSet.R = (byte)r2;
        //    bgColorToSet.G = (byte)g2;
        //    bgColorToSet.B = (byte)b2;
        //    r2 = sunColor.R;
        //    g2 = sunColor.G;
        //    b2 = sunColor.B;
        //    g2 -= (int)(90f * num6 * ((float)(int)sunColor.G / 255f));
        //    b2 -= (int)(110f * num6 * ((float)(int)sunColor.B / 255f));
        //    if (r2 < 15)
        //        r2 = 15;

        //    if (g2 < 15)
        //        g2 = 15;

        //    if (b2 < 15)
        //        b2 = 15;

        //    sunColor.R = (byte)r2;
        //    sunColor.G = (byte)g2;
        //    sunColor.B = (byte)b2;
        //}

        //if (info.JungleBiomeInfluence > 0f) {
        //    float num7 = info.JungleBiomeInfluence;
        //    if (num7 > 1f)
        //        num7 = 1f;

        //    int r3 = bgColorToSet.R;
        //    int G = bgColorToSet.G;
        //    int b3 = bgColorToSet.B;
        //    r3 -= (int)(40f * num7 * ((float)(int)bgColorToSet.R / 255f));
        //    b3 -= (int)(70f * num7 * ((float)(int)bgColorToSet.B / 255f));
        //    if (G > 255)
        //        G = 255;

        //    if (G < 15)
        //        G = 15;

        //    if (r3 > 255)
        //        r3 = 255;

        //    if (r3 < 15)
        //        r3 = 15;

        //    if (b3 < 15)
        //        b3 = 15;

        //    DontStarveSeed.FixBiomeDarkness(ref bgColorToSet, ref r3, ref G, ref b3);
        //    bgColorToSet.R = (byte)r3;
        //    bgColorToSet.G = (byte)G;
        //    bgColorToSet.B = (byte)b3;
        //    r3 = sunColor.R;
        //    G = sunColor.G;
        //    b3 = sunColor.B;
        //    r3 -= (int)(30f * num7 * ((float)(int)sunColor.R / 255f));
        //    b3 -= (int)(10f * num7 * ((float)(int)sunColor.B / 255f));
        //    if (r3 < 15)
        //        r3 = 15;

        //    if (G < 15)
        //        G = 15;

        //    if (b3 < 15)
        //        b3 = 15;

        //    sunColor.R = (byte)r3;
        //    sunColor.G = (byte)G;
        //    sunColor.B = (byte)b3;
        //}

        //if (info.MushroomBiomeInfluence > 0f) {
        //    float mushroomBiomeInfluence = info.MushroomBiomeInfluence;
        //    int r4 = bgColorToSet.R;
        //    int g3 = bgColorToSet.G;
        //    int b4 = bgColorToSet.B;
        //    g3 -= (int)(250f * mushroomBiomeInfluence * ((float)(int)bgColorToSet.G / 255f));
        //    r4 -= (int)(250f * mushroomBiomeInfluence * ((float)(int)bgColorToSet.R / 255f));
        //    b4 -= (int)(250f * mushroomBiomeInfluence * ((float)(int)bgColorToSet.B / 255f));
        //    if (g3 < 15)
        //        g3 = 15;

        //    if (r4 < 15)
        //        r4 = 15;

        //    if (b4 < 15)
        //        b4 = 15;

        //    DontStarveSeed.FixBiomeDarkness(ref bgColorToSet, ref r4, ref g3, ref b4);
        //    bgColorToSet.R = (byte)r4;
        //    bgColorToSet.G = (byte)g3;
        //    bgColorToSet.B = (byte)b4;
        //    r4 = sunColor.R;
        //    g3 = sunColor.G;
        //    b4 = sunColor.B;
        //    g3 -= (int)(10f * mushroomBiomeInfluence * ((float)(int)sunColor.G / 255f));
        //    r4 -= (int)(30f * mushroomBiomeInfluence * ((float)(int)sunColor.R / 255f));
        //    b4 -= (int)(10f * mushroomBiomeInfluence * ((float)(int)sunColor.B / 255f));
        //    if (r4 < 15)
        //        r4 = 15;

        //    if (g3 < 15)
        //        g3 = 15;

        //    if (b4 < 15)
        //        b4 = 15;

        //    sunColor.R = (byte)r4;
        //    sunColor.G = (byte)g3;
        //    sunColor.B = (byte)b4;
        //    r4 = moonColor.R;
        //    g3 = moonColor.G;
        //    b4 = moonColor.B;
        //    g3 -= (int)(140f * mushroomBiomeInfluence * ((float)(int)moonColor.R / 255f));
        //    r4 -= (int)(170f * mushroomBiomeInfluence * ((float)(int)moonColor.G / 255f));
        //    b4 -= (int)(190f * mushroomBiomeInfluence * ((float)(int)moonColor.B / 255f));
        //    if (r4 < 15)
        //        r4 = 15;

        //    if (g3 < 15)
        //        g3 = 15;

        //    if (b4 < 15)
        //        b4 = 15;

        //    moonColor.R = (byte)r4;
        //    moonColor.G = (byte)g3;
        //    moonColor.B = (byte)b4;
        //}

        //byte minimalLight = 19;

        //if (Main.dontStarveWorld)
        //    DontStarveSeed.ModifyMinimumLightColorAtNight(ref minimalLight);

        //if (bgColorToSet.R < minimalLight)
        //    bgColorToSet.R = minimalLight;

        //if (bgColorToSet.G < minimalLight)
        //    bgColorToSet.G = minimalLight;

        //if (bgColorToSet.B < minimalLight)
        //    bgColorToSet.B = minimalLight;

        //if (info.BloodMoonActive) {
        //    if (bgColorToSet.R < 25)
        //        bgColorToSet.R = 25;

        //    if (bgColorToSet.G < 25)
        //        bgColorToSet.G = 25;

        //    if (bgColorToSet.B < 25)
        //        bgColorToSet.B = 25;
        //}

        //if (Main.eclipse && isDayTime) {
        //    float num8 = 1242f;
        //    Main.eclipseLight = (float)(num / (double)num8);
        //    if (Main.eclipseLight > 1f)
        //        Main.eclipseLight = 1f;
        //}
        //else if (Main.eclipseLight > 0f) {
        //    Main.eclipseLight -= 0.01f;
        //    if (Main.eclipseLight < 0f)
        //        Main.eclipseLight = 0f;
        //}

        //if (Main.eclipseLight > 0f) {
        //    float num9 = 1f - 0.925f * Main.eclipseLight;
        //    float num10 = 1f - 0.96f * Main.eclipseLight;
        //    float num11 = 1f - 1f * Main.eclipseLight;
        //    int num12 = (int)((float)(int)bgColorToSet.R * num9);
        //    int num13 = (int)((float)(int)bgColorToSet.G * num10);
        //    int num14 = (int)((float)(int)bgColorToSet.B * num11);
        //    bgColorToSet.R = (byte)num12;
        //    bgColorToSet.G = (byte)num13;
        //    bgColorToSet.B = (byte)num14;
        //    sunColor.R = byte.MaxValue;
        //    sunColor.G = 127;
        //    sunColor.B = 67;
        //    if (bgColorToSet.R < 20)
        //        bgColorToSet.R = 20;

        //    if (bgColorToSet.G < 10)
        //        bgColorToSet.G = 10;

        //    if (!Lighting.NotRetro) {
        //        if (bgColorToSet.R < 20)
        //            bgColorToSet.R = 20;

        //        if (bgColorToSet.G < 14)
        //            bgColorToSet.G = 14;

        //        if (bgColorToSet.B < 6)
        //            bgColorToSet.B = 6;
        //    }
        //}

        //if ((Main.remixWorld && !Main.gameMenu) || WorldGen.remixWorldGen) {
        //    bgColorToSet.R = 1;
        //    bgColorToSet.G = 1;
        //    bgColorToSet.B = 1;
        //}

        //if (Main.lightning > 0f) {
        //    float value = (float)(int)bgColorToSet.R / 255f;
        //    float value2 = (float)(int)bgColorToSet.G / 255f;
        //    float value3 = (float)(int)bgColorToSet.B / 255f;
        //    value = MathHelper.Lerp(value, 1f, Main.lightning);
        //    value2 = MathHelper.Lerp(value2, 1f, Main.lightning);
        //    value3 = MathHelper.Lerp(value3, 1f, Main.lightning);
        //    bgColorToSet.R = (byte)(value * 255f);
        //    bgColorToSet.G = (byte)(value2 * 255f);
        //    bgColorToSet.B = (byte)(value3 * 255f);
        //}

        //if (!info.BloodMoonActive)
        //    moonColor = Microsoft.Xna.Framework.Color.White;

        //Main.ColorOfTheSkies = bgColorToSet;
    }

    private static void Reset() {
        _alpha = 0f;
        _preArrivedLothorBossTimer = 0f;
        _shake = _shake2 = false;
        PreArrivedLothorBoss = (false, false);
        ActiveMessages = (false, false, false);
        LothorShake.shake = LothorShake.before = false;
    }

    public override void NetSend(BinaryWriter writer) {
        writer.Write(PreArrivedLothorBoss.Item1);
        //writer.Write(PreArrivedLothorBoss.Item2);
        writer.Write(_summonedNaturally);
    }

    public override void NetReceive(BinaryReader reader) {
        PreArrivedLothorBoss.Item1 = reader.ReadBoolean();
        //PreArrivedLothorBoss.Item2 = reader.ReadBoolean();
        _summonedNaturally = reader.ReadBoolean();
    }

    public override void PostUpdateEverything() {
        int type = ModContent.NPCType<Lothor>();
        bool flag = NPC.AnyNPCs(type);
        if (PreArrivedLothorBoss.Item2) {
            if (_preArrivedLothorBossTimer < 0f) {
                _preArrivedLothorBossTimer++;
            }
        }
        if (!PreArrivedLothorBoss.Item1 || PreArrivedLothorBoss.Item2 || flag) {
            if (PreArrivedLothorBoss.Item2 && !flag && _preArrivedLothorBossTimer >= 0f) {
                Reset();
                return;
            }
            if (!PreArrivedLothorBoss.Item1 && !PreArrivedLothorBoss.Item2 && flag) {
                _summonedNaturally = true;
                PreArrivedLothorBoss.Item1 = true;
                ActiveMessages.Item1 = true;
                ActiveMessages.Item2 = true;
                ActiveMessages.Item3 = true;
                _preArrivedLothorBossTimer = 0f;
            }

            return;
        }
        bool flag2 = _preArrivedLothorBossTimer >= 9.5f;
        bool flag3 = _preArrivedLothorBossTimer < 8f;
        if (PreArrivedLothorBoss.Item1 && !flag && flag3) {
            //if (Main.netMode != NetmodeID.Server) {
            //    Main.musicFade[Main.curMusic] = -0.25f;
            //}
        }
        if (!flag) {
            _preArrivedLothorBossTimer += TimeSystem.LogicDeltaTime * 1f;
        }
        Color color = new(160, 68, 234);
        if (_preArrivedLothorBossTimer >= 2.465f && !_shake) {
            _summonedNaturally = false;
            _shake = true;
            LothorShake.shake = true;

            if (Main.netMode == NetmodeID.SinglePlayer) {
                NPC.NewNPC(NPC.GetSource_NaturalSpawn(), (int)Main.LocalPlayer.Center.X, (int)Main.LocalPlayer.Center.Y, ModContent.NPCType<DruidSoul2>());
            }
            else {
                MultiplayerSystem.SendPacket(new SpawnDruidSoul2Packet(Main.LocalPlayer.Center));
            }

            if (Main.netMode == NetmodeID.Server) {
                NetMessage.SendData(MessageID.WorldData);
            }
        }
        else if (_preArrivedLothorBossTimer >= 3f && !ActiveMessages.Item1) {
            ActiveMessages.Item1 = true;
            string message = Language.GetText("Mods.RoA.World.LothorArrival1").ToString();
            if (Main.netMode != NetmodeID.MultiplayerClient)
                Helper.NewMessage($"{message}", color);
            Shake(10f, 5f);

            Vector2 stompPos = AltarHandler.GetAltarPosition().ToWorldCoordinates() + new Vector2(0, -1000f).RotatedByRandom(2);
            SoundEngine.PlaySound(SoundID.Item73 with { Volume = 0.6f, Pitch = -0.3f, PitchVariance = 0.1f }, stompPos);
            SoundEngine.PlaySound(new SoundStyle(ResourceManager.ItemSounds + "Leaves1") { Volume = 0.25f, Pitch = -0.3f, PitchVariance = 0.1f }, stompPos);
        }
        else if (_preArrivedLothorBossTimer >= 4.262f && !_shake2) {
            _shake2 = true;
            LothorShake.shake = true;
        }
        else if (_preArrivedLothorBossTimer >= 5f && !ActiveMessages.Item2) {
            ActiveMessages.Item2 = true;
            string message = Language.GetText("Mods.RoA.World.LothorArrival2").ToString();
            if (Main.netMode != NetmodeID.MultiplayerClient)
                Helper.NewMessage($"{message}", color);
            Shake(20f, 10f);

            Vector2 stompPos = AltarHandler.GetAltarPosition().ToWorldCoordinates() + new Vector2(0, -600f).RotatedByRandom(2);
            SoundEngine.PlaySound(SoundID.Item73 with { Volume = 0.6f, Pitch = -0.3f, PitchVariance = 0.1f }, stompPos);
            SoundEngine.PlaySound(new SoundStyle(ResourceManager.ItemSounds + "Leaves1") { Volume = 0.25f, Pitch = -0.3f, PitchVariance = 0.1f }, stompPos);
        }
        else if (_preArrivedLothorBossTimer >= 6f && !ActiveMessages.Item3) {
            ActiveMessages.Item3 = true;
            SoundEngine.PlaySound(new SoundStyle(ResourceManager.AmbientSounds + "LothorScream2") { Volume = 0.8f, Pitch = 0.5f },
                AltarHandler.GetAltarPosition().ToWorldCoordinates());
        }
        else if (flag2) {
            Player spawnPlayer = Main.LocalPlayer;
            float distance = -1f;
            Vector2 tileCoords = AltarHandler.GetAltarPosition().ToWorldCoordinates();
            foreach (Player player in Main.player) {
                if (player.active && player.InModBiome<BackwoodsBiome>()) {
                    if (distance < player.Distance(tileCoords)) {
                        distance = player.Distance(tileCoords);
                        spawnPlayer = player;
                    }
                }
            }
            if (Main.netMode != NetmodeID.MultiplayerClient) {
                if (!NPC.AnyNPCs(type)) {
                    Player player = spawnPlayer;
                    NPC.NewNPC(NPC.GetBossSpawnSource(player.whoAmI),
                        (int)player.Center.X + -(tileCoords.X - player.Center.X).GetDirection() * 1000,
                        (int)player.Center.Y - 500, type);
                    if (Main.netMode == 0)
                        Main.NewText(Language.GetTextValue("Announcement.HasAwoken", Language.GetTextValue("Mods.RoA.NPCs.Lothor.DisplayName")), 175, 75);
                    else if (Main.netMode == 2)
                        ChatHelper.BroadcastChatMessage(NetworkText.FromKey("Announcement.HasAwoken", Language.GetTextValue("Mods.RoA.NPCs.Lothor.DisplayName")), new Color(175, 75, 255));
                }
            }
            _preArrivedLothorBossTimer = -10f;
            PreArrivedLothorBoss.Item2 = true;
        }
        if (!flag && _summonedNaturally) {
            Reset();
        }
    }

    private static void Shake(float strength = 7.5f, float vibeStrength = 4.15f) {
        Vector2 tileCoords = AltarHandler.GetAltarPosition().ToWorldCoordinates();
        if (Main.netMode != NetmodeID.Server) {
            foreach (Player player in Main.player) {
                if (player.active && player.InModBiome<BackwoodsBiome>()) {
                    Vector2 position = player.GetPlayerCorePoint() - tileCoords;
                    position.Normalize();
                    PunchCameraModifier punchCameraModifier = new PunchCameraModifier(tileCoords, ((float)Math.Atan2(position.Y, position.X) + MathHelper.PiOver2).ToRotationVector2(), strength, vibeStrength, 20, 2000f, "Lothor Invasion");
                    Main.instance.CameraModifiers.Add(punchCameraModifier);
                }
            }
        }
    }
}
