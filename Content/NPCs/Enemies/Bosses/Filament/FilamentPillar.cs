using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using ReLogic.Content;

using RoA.Common;
using RoA.Common.BossBars;
using RoA.Common.Configs;
using RoA.Common.World;
using RoA.Content.Dusts;
using RoA.Content.Items.Materials;
using RoA.Core;
using RoA.Core.Utility.Vanilla;

using System;
using System.Collections.Generic;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.ItemDropRules;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.WorldBuilding;

namespace RoA.Content.NPCs.Enemies.Bosses.Filament;

[AutoloadBossHead]
sealed class FilamentPillar : ModNPC {
    private static Asset<Texture2D> _glowMaskTexture = null!;

    public static int ShieldStrengthTowerFilamentTower = 0;
    public static bool TowerActiveFilament;

    public override void SaveData(TagCompound tag) {
        if (TowerActiveFilament) {
            tag[nameof(TowerActiveFilament)] = true;
        }
        tag[nameof(ShieldStrengthTowerFilamentTower)] = ShieldStrengthTowerFilamentTower;
    }

    public override void LoadData(TagCompound tag) {
        TowerActiveFilament = tag.ContainsKey(nameof(TowerActiveFilament));
        ShieldStrengthTowerFilamentTower = tag.GetInt(nameof(ShieldStrengthTowerFilamentTower));
    }

    public override void SetStaticDefaults() {
        NPCID.Sets.ShouldBeCountedAsBoss[Type] = true;
        NPCID.Sets.DangerThatPreventsOtherDangers[Type] = true;

        if (!Main.dedServ) {
            _glowMaskTexture = ModContent.Request<Texture2D>(Texture + "_Glow");
        }
    }

    public override bool NeedSaving() => true;

    public override void Load() {
        On_NPC.getTenthAnniversaryAdjustments += On_NPC_getTenthAnniversaryAdjustments;
        On_NPC.DoesntDespawnToInactivity += On_NPC_DoesntDespawnToInactivity;

        On_WorldGen.TriggerLunarApocalypse += On_WorldGen_TriggerLunarApocalypse;

        On_Main.CheckMonoliths += On_Main_CheckMonoliths;

        On_Main.ClearVisualPostProcessEffects += On_Main_ClearVisualPostProcessEffects;
        On_Player.UpdateBiomes += On_Player_UpdateBiomes;

        WorldCommon.ClearWorldEvent += WorldCommon_ClearWorldEvent;

        On_WorldGen.UpdateLunarApocalypse += On_WorldGen_UpdateLunarApocalypse;
        On_WorldGen.MessageLunarApocalypse += On_WorldGen_MessageLunarApocalypse;
    }

    private void On_WorldGen_MessageLunarApocalypse(On_WorldGen.orig_MessageLunarApocalypse orig) {
        if (!ModContent.GetInstance<RoAServerConfig>().ChangeLunarPillarLogic) {
            if (NPC.LunarApocalypseIsUp) {
                int num = 0;
                for (int i = 0; i < WorldCommon.ActivatedLunarPillars_VanillaLogic.Length; i++) {
                    if (WorldCommon.ActivatedLunarPillars_VanillaLogic[i] == NPCID.LunarTowerSolar && !NPC.TowerActiveSolar) {
                        num++;
                    }
                    if (WorldCommon.ActivatedLunarPillars_VanillaLogic[i] == NPCID.LunarTowerVortex && !NPC.TowerActiveVortex) {
                        num++;
                    }
                    if (WorldCommon.ActivatedLunarPillars_VanillaLogic[i] == NPCID.LunarTowerNebula && !NPC.TowerActiveNebula) {
                        num++;
                    }
                    if (WorldCommon.ActivatedLunarPillars_VanillaLogic[i] == NPCID.LunarTowerStardust && !NPC.TowerActiveStardust) {
                        num++;
                    }
                    if (WorldCommon.ActivatedLunarPillars_VanillaLogic[i] == ModContent.NPCType<FilamentPillar>() && !TowerActiveFilament) {
                        num++;
                    }
                }

                WorldGen.BroadcastText(NetworkText.FromKey(Lang.misc[43 + num].Key), 175, 75, 255);
            }

            return;
        }
        if (NPC.LunarApocalypseIsUp) {
            int num = 0;
            if (!NPC.TowerActiveSolar)
                num++;

            if (!NPC.TowerActiveVortex)
                num++;

            if (!NPC.TowerActiveNebula)
                num++;

            if (!NPC.TowerActiveStardust)
                num++;

            if (!TowerActiveFilament)
                num++;

            if (num == 1) {
                WorldGen.BroadcastText(NetworkText.FromKey("Mods.RoA.World.CelestialPillar1Destroyed"), 175, 75, 255);
            }
            else {
                num -= 1;
                WorldGen.BroadcastText(NetworkText.FromKey(Lang.misc[43 + num].Key), 175, 75, 255);
            }
        }
    }

    private void On_WorldGen_UpdateLunarApocalypse(On_WorldGen.orig_UpdateLunarApocalypse orig) {
        if (!NPC.LunarApocalypseIsUp)
            return;

        bool flag = false;
        bool flag2 = false;
        bool flag3 = false;
        bool flag4 = false;
        bool flag5 = false;
        bool flag6 = false;
        ushort filament = (ushort)ModContent.NPCType<FilamentPillar>();
        for (int i = 0; i < 200; i++) {
            if (Main.npc[i].active) {
                switch (Main.npc[i].type) {
                    case NPCID.MoonLordCore:
                        flag = true;
                        break;
                    case 517:
                        flag2 = true;
                        break;
                    case 422:
                        flag3 = true;
                        break;
                    case 507:
                        flag4 = true;
                        break;
                    case 493:
                        flag5 = true;
                        break;
                }
                if (Main.npc[i].type == filament) {
                    flag6 = true;
                }
            }
        }

        if (!flag2)
            NPC.TowerActiveSolar = false;

        if (!flag3)
            NPC.TowerActiveVortex = false;

        if (!flag4)
            NPC.TowerActiveNebula = false;

        if (!flag5)
            NPC.TowerActiveStardust = false;

        if (!flag6)
            TowerActiveFilament = false;

        if (!NPC.TowerActiveSolar && !NPC.TowerActiveVortex && !NPC.TowerActiveNebula && !NPC.TowerActiveStardust && !TowerActiveFilament && !flag)
            WorldGen.StartImpendingDoom(3600);
    }

    private void WorldCommon_ClearWorldEvent() {
        NPC.LunarShieldPowerNormal = ModContent.GetInstance<RoAServerConfig>().ChangeLunarPillarLogic ? 80 : 100;
    }

    private void On_Player_UpdateBiomes(On_Player.orig_UpdateBiomes orig, Player self) {
        self.GetCommon().ZoneFilament = false;

        Vector2 vector = Vector2.Zero;
        for (int i = 0; i < 200; i++) {
            if (!Main.npc[i].active)
                continue;

            if (Main.npc[i].type == ModContent.NPCType<FilamentPillar>()) {
                if (self.Distance(Main.npc[i].Center) <= 4000f) {
                    self.GetCommon().ZoneFilament = true;
                    vector = Main.npc[i].Center;
                }
            }
        }
        self.ManageSpecialBiomeVisuals(ShaderLoader.Filament, self.GetCommon().ZoneFilament, vector - new Vector2(0f, 10f));

        orig(self);
    }

    private void On_Main_ClearVisualPostProcessEffects(On_Main.orig_ClearVisualPostProcessEffects orig) {
        orig();

        string key = ShaderLoader.Filament;
        if (SkyManager.Instance[key] != null && SkyManager.Instance[key].IsActive())
            SkyManager.Instance[key].Deactivate();

        if (Overlays.Scene[key] != null && Overlays.Scene[key].IsVisible())
            Overlays.Scene[key].Deactivate();

        if (Terraria.Graphics.Effects.Filters.Scene[key] != null && Terraria.Graphics.Effects.Filters.Scene[key].IsActive())
            Terraria.Graphics.Effects.Filters.Scene[key].Deactivate();
    }

    private void On_Main_CheckMonoliths(On_Main.orig_CheckMonoliths orig) {
        orig();
    }

    private void On_WorldGen_TriggerLunarApocalypse(On_WorldGen.orig_TriggerLunarApocalypse orig) {
        if (!ModContent.GetInstance<RoAServerConfig>().ChangeLunarPillarLogic) {
            VanillaLunarEventTrigger();

            return;
        }

        RemadeLunarEventTrigger();
    }

    private void RemadeLunarEventTrigger() {
        int currentAddedPillarIndex = 0;
        List<int> list = new List<int> {
            517,
            422,
            507,
            493,
            ModContent.NPCType<FilamentPillar>()
        };

        int[] array = new int[5];
        for (int i = 0; i < 5; i++) {
            array[i] = list[Main.rand.Next(list.Count)];
            list.Remove(array[i]);
        }

        int sectionCount = 7;
        int step = Main.maxTilesX / sectionCount;
        HashSet<Point16> possibleTowerPositions = [];

        bool onRightSide = Main.rand.NextBool();

        for (int currentSection = 0; currentSection < sectionCount; currentSection++) {
            if (currentSection == 3 || currentSection == 6) {
                continue;
            }

            int x = step * (1 + currentSection) - step / 2;

            x += step / 2;

            if (currentSection == 2 && onRightSide) {
                x += step + step / sectionCount;
            }
            if (currentSection > 2 && onRightSide) {
                x += step / sectionCount;
            }

            //if (currentSection == 4 || currentSection == 5) {
            //    x += step;
            //}

            possibleTowerPositions.Add(new Point16(x, (int)Main.worldSurface));
        }
        foreach (Point16 pillarPosition in possibleTowerPositions) {
            int num3 = pillarPosition.X;
            int num2 = pillarPosition.Y;
            bool flag = false;

            int pillarNPCType = array[currentAddedPillarIndex];

            for (int k = 0; k < 30; k++) {
                int num4 = Main.rand.Next(-100, 101) / 2;
                if (Main.remixWorld && Main.getGoodWorld) {
                    int num5 = Main.rand.Next((int)Main.worldSurface, Main.maxTilesY - 350);
                    if (!WorldGen.PlayerLOS(num3 + num4 - 10, num5) && !WorldGen.PlayerLOS(num3 + num4 + 10, num5) && !WorldGen.PlayerLOS(num3 + num4 - 10, num5 - 20) && !WorldGen.PlayerLOS(num3 + num4 + 10, num5 - 20)) {
                        int num6 = NPC.NewNPC(new EntitySource_WorldEvent(), (num3 + num4) * 16, num5 * 16, pillarNPCType);
                        if (Main.netMode == 2 && num6 < 200)
                            NetMessage.SendData(23, -1, -1, null, num6);

                        flag = true;

                        currentAddedPillarIndex++;
                        break;
                    }

                    continue;
                }

                for (int num7 = num2; num7 > 100; num7--) {
                    if (!Collision.SolidTiles(num3 + num4 - 10, num3 + num4 + 10, num7 - 20, num7 + 15) && !WorldGen.PlayerLOS(num3 + num4 - 10, num7) && !WorldGen.PlayerLOS(num3 + num4 + 10, num7) && !WorldGen.PlayerLOS(num3 + num4 - 10, num7 - 20) && !WorldGen.PlayerLOS(num3 + num4 + 10, num7 - 20)) {
                        int num8 = NPC.NewNPC(new EntitySource_WorldEvent(), (num3 + num4) * 16, num7 * 16, pillarNPCType);
                        if (Main.netMode == 2 && num8 < 200)
                            NetMessage.SendData(23, -1, -1, null, num8);

                        flag = true;

                        currentAddedPillarIndex++;
                        break;
                    }
                }

                if (flag)
                    break;
            }

            if (!flag) {
                NPC.NewNPC(new EntitySource_WorldEvent(), num3 * 16, (num2 - 40) * 16, pillarNPCType);
                currentAddedPillarIndex++;
            }
        }

        TowerActiveFilament = NPC.TowerActiveVortex = (NPC.TowerActiveNebula = (NPC.TowerActiveSolar = (NPC.TowerActiveStardust = true)));
        NPC.LunarApocalypseIsUp = true;
        ShieldStrengthTowerFilamentTower = NPC.ShieldStrengthTowerSolar = (NPC.ShieldStrengthTowerVortex = (NPC.ShieldStrengthTowerNebula = (NPC.ShieldStrengthTowerStardust = NPC.ShieldStrengthTowerMax)));

        // TODO: add support
        NetMessage.SendData(101);
        WorldGen.MessageLunarApocalypse();
    }

    private void VanillaLunarEventTrigger() {
        //List<int> list = new List<int> {
        //    517,
        //    422,
        //    507,
        //    493
        //};
        List<int> shuffleList = [517, 422, 507, 493, ModContent.NPCType<FilamentPillar>()];

        int[] array = new int[4];
        for (int i = 0; i < 4; i++) {
            array[i] = shuffleList[Main.rand.Next(shuffleList.Count)];
            shuffleList.Remove(array[i]);
        }

        int num = Main.maxTilesX / 5;
        int num2 = (int)Main.worldSurface;
        for (int j = 0; j < 4; j++) {
            int num3 = num * (1 + j);
            bool flag = false;
            for (int k = 0; k < 30; k++) {
                int num4 = Main.rand.Next(-100, 101);
                if (Main.remixWorld && Main.getGoodWorld) {
                    int num5 = Main.rand.Next((int)Main.worldSurface, Main.maxTilesY - 350);
                    if (!WorldGen.PlayerLOS(num3 + num4 - 10, num5) && !WorldGen.PlayerLOS(num3 + num4 + 10, num5) && !WorldGen.PlayerLOS(num3 + num4 - 10, num5 - 20) && !WorldGen.PlayerLOS(num3 + num4 + 10, num5 - 20)) {
                        int num6 = NPC.NewNPC(new EntitySource_WorldEvent(), (num3 + num4) * 16, num5 * 16, array[j]);
                        if (Main.netMode == 2 && num6 < 200)
                            NetMessage.SendData(23, -1, -1, null, num6);

                        flag = true;
                        break;
                    }

                    continue;
                }

                for (int num7 = num2; num7 > 100; num7--) {
                    if (!Collision.SolidTiles(num3 + num4 - 10, num3 + num4 + 10, num7 - 20, num7 + 15) && !WorldGen.PlayerLOS(num3 + num4 - 10, num7) && !WorldGen.PlayerLOS(num3 + num4 + 10, num7) && !WorldGen.PlayerLOS(num3 + num4 - 10, num7 - 20) && !WorldGen.PlayerLOS(num3 + num4 + 10, num7 - 20)) {
                        int num8 = NPC.NewNPC(new EntitySource_WorldEvent(), (num3 + num4) * 16, num7 * 16, array[j]);
                        if (Main.netMode == 2 && num8 < 200)
                            NetMessage.SendData(23, -1, -1, null, num8);

                        flag = true;
                        break;
                    }
                }

                if (flag)
                    break;
            }

            if (!flag)
                NPC.NewNPC(new EntitySource_WorldEvent(), num3 * 16, (num2 - 40) * 16, array[j]);
        }

        WorldCommon.ActivatedLunarPillars_VanillaLogic = new int[5];

        int index = 0;
        for (int i = 0; i < array.Length; i++) {
            switch (array[i]) {
                case NPCID.LunarTowerNebula:
                    NPC.TowerActiveNebula = true;
                    NPC.ShieldStrengthTowerNebula = NPC.ShieldStrengthTowerMax;
                    WorldCommon.ActivatedLunarPillars_VanillaLogic[index] = (ushort)NPCID.LunarTowerNebula;
                    index++;
                    break;
                case NPCID.LunarTowerSolar:
                    NPC.TowerActiveSolar = true;
                    NPC.ShieldStrengthTowerSolar = NPC.ShieldStrengthTowerMax;
                    WorldCommon.ActivatedLunarPillars_VanillaLogic[index] = (ushort)NPCID.LunarTowerSolar;
                    index++;
                    break;
                case NPCID.LunarTowerStardust:
                    NPC.TowerActiveStardust = true;
                    NPC.ShieldStrengthTowerStardust = NPC.ShieldStrengthTowerMax;
                    WorldCommon.ActivatedLunarPillars_VanillaLogic[index] = (ushort)NPCID.LunarTowerStardust;
                    index++;
                    break;
                case NPCID.LunarTowerVortex:
                    NPC.TowerActiveVortex = true;
                    NPC.ShieldStrengthTowerVortex = NPC.ShieldStrengthTowerMax;
                    WorldCommon.ActivatedLunarPillars_VanillaLogic[index] = (ushort)NPCID.LunarTowerVortex;
                    index++;
                    break;
            }
            if (array[i] == ModContent.NPCType<FilamentPillar>()) {
                TowerActiveFilament = true;
                WorldCommon.ActivatedLunarPillars_VanillaLogic[index] = (ushort)ModContent.NPCType<FilamentPillar>();
                index++;
            }
        }

        //NPC.TowerActiveVortex = (NPC.TowerActiveNebula = (NPC.TowerActiveSolar = (NPC.TowerActiveStardust = true)));
        NPC.LunarApocalypseIsUp = true;
        //NPC.ShieldStrengthTowerSolar = (NPC.ShieldStrengthTowerVortex = (NPC.ShieldStrengthTowerNebula = (NPC.ShieldStrengthTowerStardust = NPC.ShieldStrengthTowerMax)));

        // TODO: add support
        NetMessage.SendData(101);
        WorldGen.MessageLunarApocalypse();
    }

    private bool On_NPC_DoesntDespawnToInactivity(On_NPC.orig_DoesntDespawnToInactivity orig, NPC self) {
        if (self.type == ModContent.NPCType<FilamentPillar>()) {
            return true;
        }

        return orig(self);
    }

    private void On_NPC_getTenthAnniversaryAdjustments(On_NPC.orig_getTenthAnniversaryAdjustments orig, NPC self) {
        if (self.type == ModContent.NPCType<FilamentPillar>()) {
            float num = self.scale;
            float num2 = 0.5f;

            self.scale *= num2;

            if (self.IsABestiaryIconDummy) {
                self.scale = num;
                return;
            }

            self.width = (int)((float)self.width * self.scale);
            self.height = (int)((float)self.height * self.scale);

            return;
        }

        orig(self);
    }

    public override bool CheckDead() {
        if (NPC.ai[2] != 1f) {
            NPC.ai[2] = 1f;
            NPC.ai[1] = 0f;
            NPC.life = NPC.lifeMax;
            NPC.dontTakeDamage = true;
            NPC.netUpdate = true;
            return false;
        }

        return base.CheckDead();
    }

    public override void SetDefaults() {
        NPC.lifeMax = 20000;
        NPC.defense = 20;
        NPC.damage = 0;
        NPC.width = 130;
        NPC.height = 270;
        NPC.aiStyle = -1;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.value = 0f;
        NPC.knockBackResist = 0f;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.npcSlots = 0f;

        NPC.BossBar = ModContent.GetInstance<FilamentPillarBigProgressBar>();
    }

    public override void AI() {
        if (NPC.ai[2] == 1f) {
            NPC.velocity = Vector2.UnitY * NPC.velocity.Length();
            if (NPC.velocity.Y < 0.25f)
                NPC.velocity.Y += 0.02f;

            if (NPC.velocity.Y > 0.25f)
                NPC.velocity.Y -= 0.02f;

            NPC.dontTakeDamage = true;
            NPC.ai[1]++;
            if (NPC.ai[1] > 120f)
                NPC.Opacity = 1f - (NPC.ai[1] - 120f) / 60f;

            int num1465 = ModContent.DustType<FilamentDust>();
            //switch (type) {
            //    case 517:
            //        num1465 = 127;
            //        break;
            //    case 422:
            //        num1465 = 229;
            //        break;
            //    case 507:
            //        num1465 = 242;
            //        break;
            //    case 493:
            //        num1465 = 135;
            //        break;
            //}

            if (Main.rand.Next(5) == 0 && NPC.ai[1] < 120f) {
                for (int num1466 = 0; num1466 < 3; num1466++) {
                    Dust dust9 = Main.dust[Dust.NewDust(NPC.Left, NPC.width, NPC.height / 2, num1465)];
                    dust9.position = NPC.Center + Vector2.UnitY.RotatedByRandom(4.188790321350098) * new Vector2((float)NPC.width * 1.5f, (float)NPC.height * 1.1f) * 0.8f * (0.8f + Main.rand.NextFloat() * 0.2f);
                    dust9.velocity.X = 0f;
                    dust9.velocity.Y = (0f - Math.Abs(dust9.velocity.Y - (float)num1466 + NPC.velocity.Y - 4f)) * 3f;
                    dust9.noGravity = true;
                    dust9.fadeIn = 1f;
                    dust9.scale = 1f + Main.rand.NextFloat() + (float)num1466 * 0.3f;
                }
            }

            if (NPC.ai[1] < 150f) {
                for (int num1467 = 0; num1467 < 3; num1467++) {
                    if (Main.rand.Next(4) == 0) {
                        Dust dust10 = Main.dust[Dust.NewDust(NPC.Top + new Vector2((float)(-NPC.width) * (0.33f - 0.11f * (float)num1467), -20f), (int)((float)NPC.width * (0.66f - 0.22f * (float)num1467)), 20, num1465)];
                        dust10.velocity.X = 0f;
                        dust10.velocity.Y = (0f - Math.Abs(dust10.velocity.Y - (float)num1467 + NPC.velocity.Y - 4f)) * (1f + NPC.ai[1] / 180f * 0.5f);
                        dust10.noGravity = true;
                        dust10.fadeIn = 1f;
                        dust10.scale = 1f + Main.rand.NextFloat() + (float)num1467 * 0.3f;
                    }
                }
            }

            if (Main.rand.Next(5) == 0 && NPC.ai[1] < 150f) {
                for (int num1468 = 0; num1468 < 3; num1468++) {
                    Vector2 vector283 = NPC.Center + Vector2.UnitY.RotatedByRandom(4.188790321350098) * new Vector2(NPC.width, NPC.height) * 0.7f * Main.rand.NextFloat();
                    float num1469 = 1f + Main.rand.NextFloat() * 2f + NPC.ai[1] / 180f * 4f;
                    for (int num1470 = 0; num1470 < 6; num1470++) {
                        Dust dust11 = Main.dust[Dust.NewDust(vector283, 4, 4, num1465)];
                        dust11.position = vector283;
                        dust11.velocity.X *= num1469;
                        dust11.velocity.Y = (0f - Math.Abs(dust11.velocity.Y)) * num1469;
                        dust11.noGravity = true;
                        dust11.fadeIn = 1f;
                        dust11.scale = 1.5f + Main.rand.NextFloat() + (float)num1470 * 0.13f;
                    }

                    SoundEngine.PlaySound(Main.rand.NextBool() ? SoundID.NPCHit18 : SoundID.NPCHit1, vector283);
                }
            }

            if (Main.rand.Next(3) != 0 && NPC.ai[1] < 150f) {
                Dust dust12 = Main.dust[Dust.NewDust(NPC.Left, NPC.width, NPC.height / 2, 241)];
                dust12.position = NPC.Center + Vector2.UnitY.RotatedByRandom(4.188790321350098) * new Vector2(NPC.width / 2, NPC.height / 2) * (0.8f + Main.rand.NextFloat() * 0.2f);
                dust12.velocity.X = 0f;
                dust12.velocity.Y = Math.Abs(dust12.velocity.Y) * 0.25f;
            }

            if (NPC.ai[1] % 60f == 1f)
                SoundEngine.PlaySound(SoundID.NPCDeath22, NPC.Center);

            if (NPC.ai[1] >= 180f) {
                NPC.life = 0;
                NPC.HitEffect(0, 1337.0);
                NPC.checkDead();
            }

            return;
        }

        if (NPC.ai[3] > 0f) {
            bool flag88 = NPC.dontTakeDamage;
            flag88 = ShieldStrengthTowerFilamentTower != 0;
            //switch (type) {
            //    case 517:
            //        flag88 = ShieldStrengthTowerSolar != 0;
            //        break;
            //    case 422:
            //        flag88 = ShieldStrengthTowerVortex != 0;
            //        break;
            //    case 507:
            //        flag88 = ShieldStrengthTowerNebula != 0;
            //        break;
            //    case 493:
            //        flag88 = ShieldStrengthTowerStardust != 0;
            //        break;
            //}

            if (flag88 != NPC.dontTakeDamage)
                SoundEngine.PlaySound(SoundID.NPCDeath58, NPC.position);
            else if (NPC.ai[3] == 1f)
                SoundEngine.PlaySound(SoundID.NPCDeath3, NPC.position);

            NPC.ai[3]++;
            if (NPC.ai[3] > 120f)
                NPC.ai[3] = 0f;
        }

        //switch (type) {
        //    case 517:
        //        dontTakeDamage = ShieldStrengthTowerSolar != 0;
        //        break;
        //    case 422:
        //        dontTakeDamage = ShieldStrengthTowerVortex != 0;
        //        break;
        //    case 507:
        //        dontTakeDamage = ShieldStrengthTowerNebula != 0;
        //        break;
        //    case 493:
        //        dontTakeDamage = ShieldStrengthTowerStardust != 0;
        //        break;
        //}

        NPC.dontTakeDamage = ShieldStrengthTowerFilamentTower != 0;

        NPC.TargetClosest(faceTarget: false);
        if (Main.player[NPC.target].Distance(NPC.Center) > 2000f)
            NPC.localAI[0]++;

        if (NPC.localAI[0] >= 60f && Main.netMode != 1) {
            NPC.localAI[0] = 0f;
            NPC.netUpdate = true;
            NPC.life = (int)MathHelper.Clamp(NPC.life + 200, 0f, NPC.lifeMax);
        }
        else {
            NPC.localAI[0] = 0f;
        }

        NPC.velocity = new Vector2(0f, (float)Math.Sin((float)Math.PI * 2f * NPC.ai[0] / 300f) * 0.5f);
        Point origin = NPC.Bottom.ToTileCoordinates();
        int maxDistance = 10;
        int num1471 = 20;
        int num1472 = 30;
        if (WorldGen.InWorld(origin.X, origin.Y, 20) && Main.tile[origin.X, origin.Y] != null) {
            if (WorldUtils.Find(origin, Searches.Chain(new Searches.Down(maxDistance), new Terraria.WorldBuilding.Conditions.IsSolid()), out var result)) {
                float num1473 = 1f - (float)Math.Abs(origin.Y - result.Y) / 10f;
                NPC.position.Y -= 1.5f * num1473;
            }
            else if (!WorldUtils.Find(origin, Searches.Chain(new Searches.Down(num1471), new Terraria.WorldBuilding.Conditions.IsSolid()), out result)) {
                float num1474 = 1f;
                if (WorldUtils.Find(origin, Searches.Chain(new Searches.Down(num1472), new Terraria.WorldBuilding.Conditions.IsSolid()), out result))
                    num1474 = Utils.GetLerpValue(num1471, num1472, Math.Abs(origin.Y - result.Y), clamped: true);

                NPC.position.Y += 1.5f * num1474;
            }
        }

        if (!Main.remixWorld && !Main.getGoodWorld && (double)NPC.Bottom.Y > Main.worldSurface * 16.0 - 100.0)
            NPC.position.Y = (float)Main.worldSurface * 16f - (float)NPC.height - 100f;

        NPC.ai[0]++;
        if (NPC.ai[0] >= 300f) {
            NPC.ai[0] = 0f;
            NPC.netUpdate = true;
        }

        if (Main.rand.Next(5) == 0) {
            Dust dust19 = Main.dust[Dust.NewDust(NPC.Left, NPC.width, NPC.height / 2, DustID.MarblePot)];
            dust19.position = NPC.Center + Vector2.UnitY.RotatedByRandom(2.094395160675049) * new Vector2(NPC.width / 2, NPC.height / 2) * (0.8f + Main.rand.NextFloat() * 0.2f);
            dust19.velocity.X = 0f;
            dust19.velocity.Y = Math.Abs(dust19.velocity.Y) * 0.25f;
        }

        for (int num1486 = 0; num1486 < 3; num1486++) {
            if (Main.rand.Next(5) == 0) {
                Dust dust20 = Main.dust[Dust.NewDust(NPC.Top + new Vector2((float)(-NPC.width) * (0.33f - 0.11f * (float)num1486), -20f), (int)((float)NPC.width * (0.66f - 0.22f * (float)num1486)), 20,
                    ModContent.DustType<FilamentDust>())];
                dust20.velocity.X = 0f;
                dust20.velocity.Y = (0f - Math.Abs(dust20.velocity.Y - (float)num1486 + NPC.velocity.Y - 4f)) * 1f;
                dust20.noGravity = true;
                dust20.fadeIn = 1f;
                dust20.scale = 1f + Main.rand.NextFloat() + (float)num1486 * 0.3f;
            }
        }

        if (NPC.ai[1] > 0f)
            NPC.ai[1]--;

        if (Main.netMode != 1 && NPC.ai[1] <= 0f && Main.player[NPC.target].active && !Main.player[NPC.target].dead && NPC.Distance(Main.player[NPC.target].Center) < 1080f && Main.player[NPC.target].position.Y - NPC.position.Y < 700f) {
            //Vector2 vector285 = NPC.Top + new Vector2((float)(-NPC.width) * 0.33f, -20f) + new Vector2((float)NPC.width * 0.66f, 20f) * Utils.RandomVector2(Main.rand, 0f, 1f);
            //Vector2 vector286 = -Vector2.UnitY.RotatedByRandom(0.7853981852531433) * (7f + Main.rand.NextFloat() * 5f);
            //int num1487 = NewNPC(GetSpawnSourceForNPCFromNPCAI(), (int)vector285.X, (int)vector285.Y, 519, whoAmI);
            //Main.npc[num1487].velocity = vector286;
            //Main.npc[num1487].netUpdate = true;
            NPC.ai[1] = 60f;
        }

        //if (type == 493) {
        //    if (Main.rand.Next(5) == 0) {
        //        Dust dust13 = Main.dust[Dust.NewDust(base.Left, width, height / 2, 241)];
        //        dust13.position = base.Center + Vector2.UnitY.RotatedByRandom(2.094395160675049) * new Vector2(width / 2, height / 2) * (0.8f + Main.rand.NextFloat() * 0.2f);
        //        dust13.velocity.X = 0f;
        //        dust13.velocity.Y = Math.Abs(dust13.velocity.Y) * 0.25f;
        //    }

        //    for (int num1475 = 0; num1475 < 3; num1475++) {
        //        if (Main.rand.Next(5) == 0) {
        //            Dust dust14 = Main.dust[Dust.NewDust(base.Top + new Vector2((float)(-width) * (0.33f - 0.11f * (float)num1475), -20f), (int)((float)width * (0.66f - 0.22f * (float)num1475)), 20, 135)];
        //            dust14.velocity.X = 0f;
        //            dust14.velocity.Y = (0f - Math.Abs(dust14.velocity.Y - (float)num1475 + velocity.Y - 4f)) * 1f;
        //            dust14.noGravity = true;
        //            dust14.fadeIn = 1f;
        //            dust14.scale = 1f + Main.rand.NextFloat() + (float)num1475 * 0.3f;
        //        }
        //    }

        //    if (this.ai[1] > 0f)
        //        this.ai[1]--;

        //    if (Main.netMode != 1 && this.ai[1] <= 0f && Main.player[target].active && !Main.player[target].dead && Distance(Main.player[target].Center) < 1080f && Main.player[target].position.Y - position.Y < 400f)
        //        SpawnStardustMark_StardustTower();
        //}

        //if (type == 507) {
        //    if (Main.rand.Next(5) == 0) {
        //        Dust dust15 = Main.dust[Dust.NewDust(base.Left, width, height / 2, 241)];
        //        dust15.position = base.Center + Vector2.UnitY.RotatedByRandom(2.094395160675049) * new Vector2(width / 2, height / 2) * (0.8f + Main.rand.NextFloat() * 0.2f);
        //        dust15.velocity.X = 0f;
        //        dust15.velocity.Y = Math.Abs(dust15.velocity.Y) * 0.25f;
        //    }

        //    for (int num1476 = 0; num1476 < 3; num1476++) {
        //        if (Main.rand.Next(5) == 0) {
        //            Dust dust16 = Main.dust[Dust.NewDust(base.Top + new Vector2((float)(-width) * (0.33f - 0.11f * (float)num1476), -20f), (int)((float)width * (0.66f - 0.22f * (float)num1476)), 20, 242)];
        //            dust16.velocity.X = 0f;
        //            dust16.velocity.Y = (0f - Math.Abs(dust16.velocity.Y - (float)num1476 + velocity.Y - 4f)) * 1f;
        //            dust16.noGravity = true;
        //            dust16.fadeIn = 1f;
        //            dust16.color = Color.Black;
        //            dust16.scale = 1f + Main.rand.NextFloat() + (float)num1476 * 0.3f;
        //        }
        //    }
        //}

        //if (type == 422) {
        //    if (Main.rand.Next(5) == 0) {
        //        Dust dust17 = Main.dust[Dust.NewDust(base.Left, width, height / 2, 241)];
        //        dust17.position = base.Center + Vector2.UnitY.RotatedByRandom(2.094395160675049) * new Vector2(width / 2, height / 2) * (0.8f + Main.rand.NextFloat() * 0.2f);
        //        dust17.velocity.X = 0f;
        //        dust17.velocity.Y = Math.Abs(dust17.velocity.Y) * 0.25f;
        //    }

        //    for (int num1477 = 0; num1477 < 3; num1477++) {
        //        if (Main.rand.Next(5) == 0) {
        //            Dust dust18 = Main.dust[Dust.NewDust(base.Top + new Vector2((float)(-width) * (0.33f - 0.11f * (float)num1477), -20f), (int)((float)width * (0.66f - 0.22f * (float)num1477)), 20, 229)];
        //            dust18.velocity.X = 0f;
        //            dust18.velocity.Y = (0f - Math.Abs(dust18.velocity.Y - (float)num1477 + velocity.Y - 4f)) * 1f;
        //            dust18.noGravity = true;
        //            dust18.fadeIn = 1f;
        //            dust18.color = Color.Black;
        //            dust18.scale = 1f + Main.rand.NextFloat() + (float)num1477 * 0.3f;
        //        }
        //    }

        //    if (this.ai[1] > 0f)
        //        this.ai[1]--;

        //    if (Main.netMode != 1 && this.ai[1] <= 0f && Main.player[target].active && !Main.player[target].dead && Distance(Main.player[target].Center) < 3240f && !Collision.CanHitLine(base.Center, 0, 0, Main.player[target].Center, 0, 0)) {
        //        this.ai[1] = 60 + Main.rand.Next(120);
        //        Point point11 = Main.player[target].Top.ToTileCoordinates();
        //        bool flag89 = CountNPCS(428) + CountNPCS(427) + CountNPCS(426) < 14;
        //        for (int num1478 = 0; num1478 < 10; num1478++) {
        //            if (WorldGen.SolidTile(point11.X, point11.Y))
        //                break;

        //            if (point11.Y <= 10)
        //                break;

        //            point11.Y--;
        //        }

        //        if (flag89)
        //            Projectile.NewProjectile(GetSpawnSource_ForProjectile(), point11.X * 16 + 8, point11.Y * 16 + 24, 0f, 0f, 579, 0, 0f, Main.myPlayer);
        //        else
        //            Projectile.NewProjectile(GetSpawnSource_ForProjectile(), point11.X * 16 + 8, point11.Y * 16 + 17, 0f, 0f, 578, 0, 1f, Main.myPlayer);
        //    }

        //    if (Main.netMode != 1 && this.ai[1] <= 0f && Main.player[target].active && !Main.player[target].dead && Distance(Main.player[target].Center) < 1080f && Main.player[target].position.Y - position.Y < 400f && CountNPCS(427) + CountNPCS(426) * 3 + CountNPCS(428) < 20) {
        //        this.ai[1] = 420 + Main.rand.Next(360);
        //        Point point12 = base.Center.ToTileCoordinates();
        //        Point point13 = Main.player[target].Center.ToTileCoordinates();
        //        Vector2 vector284 = Main.player[target].Center - base.Center;
        //        int num1479 = 20;
        //        int num1480 = 3;
        //        int num1481 = 8;
        //        int num1482 = 2;
        //        int num1483 = 0;
        //        bool flag90 = false;
        //        if (vector284.Length() > 2000f)
        //            flag90 = true;

        //        while (!flag90 && num1483 < 100) {
        //            num1483++;
        //            int num1484 = Main.rand.Next(point13.X - num1479, point13.X + num1479 + 1);
        //            int num1485 = Main.rand.Next(point13.Y - num1479, point13.Y + num1479 + 1);
        //            if ((num1485 < point13.Y - num1481 || num1485 > point13.Y + num1481 || num1484 < point13.X - num1481 || num1484 > point13.X + num1481) && (num1485 < point12.Y - num1480 || num1485 > point12.Y + num1480 || num1484 < point12.X - num1480 || num1484 > point12.X + num1480) && !Main.tile[num1484, num1485].nactive()) {
        //                bool flag91 = true;
        //                if (flag91 && Main.tile[num1484, num1485].lava())
        //                    flag91 = false;

        //                if (flag91 && Collision.SolidTiles(num1484 - num1482, num1484 + num1482, num1485 - num1482, num1485 + num1482))
        //                    flag91 = false;

        //                if (flag91 && !Collision.CanHitLine(base.Center, 0, 0, Main.player[target].Center, 0, 0))
        //                    flag91 = false;

        //                if (flag91) {
        //                    Projectile.NewProjectile(GetSpawnSource_ForProjectile(), num1484 * 16 + 8, num1485 * 16 + 8, 0f, 0f, 579, 0, 0f, Main.myPlayer);
        //                    flag90 = true;
        //                    break;
        //                }
        //            }
        //        }
        //    }
        //}

        //if (type != 517)
        //    return;

        //if (Main.rand.Next(5) == 0) {
        //    Dust dust19 = Main.dust[Dust.NewDust(base.Left, width, height / 2, 241)];
        //    dust19.position = base.Center + Vector2.UnitY.RotatedByRandom(2.094395160675049) * new Vector2(width / 2, height / 2) * (0.8f + Main.rand.NextFloat() * 0.2f);
        //    dust19.velocity.X = 0f;
        //    dust19.velocity.Y = Math.Abs(dust19.velocity.Y) * 0.25f;
        //}

        //for (int num1486 = 0; num1486 < 3; num1486++) {
        //    if (Main.rand.Next(5) == 0) {
        //        Dust dust20 = Main.dust[Dust.NewDust(base.Top + new Vector2((float)(-width) * (0.33f - 0.11f * (float)num1486), -20f), (int)((float)width * (0.66f - 0.22f * (float)num1486)), 20, 6)];
        //        dust20.velocity.X = 0f;
        //        dust20.velocity.Y = (0f - Math.Abs(dust20.velocity.Y - (float)num1486 + velocity.Y - 4f)) * 1f;
        //        dust20.noGravity = true;
        //        dust20.fadeIn = 1f;
        //        dust20.scale = 1f + Main.rand.NextFloat() + (float)num1486 * 0.3f;
        //    }
        //}

        //if (this.ai[1] > 0f)
        //    this.ai[1]--;

        //if (Main.netMode != 1 && this.ai[1] <= 0f && Main.player[target].active && !Main.player[target].dead && Distance(Main.player[target].Center) < 1080f && Main.player[target].position.Y - position.Y < 700f) {
        //    Vector2 vector285 = base.Top + new Vector2((float)(-width) * 0.33f, -20f) + new Vector2((float)width * 0.66f, 20f) * Utils.RandomVector2(Main.rand, 0f, 1f);
        //    Vector2 vector286 = -Vector2.UnitY.RotatedByRandom(0.7853981852531433) * (7f + Main.rand.NextFloat() * 5f);
        //    int num1487 = NewNPC(GetSpawnSourceForNPCFromNPCAI(), (int)vector285.X, (int)vector285.Y, 519, whoAmI);
        //    Main.npc[num1487].velocity = vector286;
        //    Main.npc[num1487].netUpdate = true;
        //    this.ai[1] = 60f;
        //}
    }

    public override void ModifyNPCLoot(NPCLoot npcLoot) {
        DropOneByOne.Parameters parameters = default(DropOneByOne.Parameters);
        parameters.MinimumItemDropsCount = 12;
        parameters.MaximumItemDropsCount = 20;
        parameters.ChanceNumerator = 1;
        parameters.ChanceDenominator = 1;
        parameters.MinimumStackPerChunkBase = 1;
        parameters.MaximumStackPerChunkBase = 3;
        parameters.BonusMinDropsPerChunkPerPlayer = 0;
        parameters.BonusMaxDropsPerChunkPerPlayer = 0;
        DropOneByOne.Parameters parameters2 = parameters;
        DropOneByOne.Parameters parameters3 = parameters2;
        parameters3.BonusMinDropsPerChunkPerPlayer = 1;
        parameters3.BonusMaxDropsPerChunkPerPlayer = 1;
        parameters3.MinimumStackPerChunkBase = (int)((float)parameters2.MinimumStackPerChunkBase * 1.5f);
        parameters3.MaximumStackPerChunkBase = (int)((float)parameters2.MaximumStackPerChunkBase * 1.5f);
        npcLoot.Add(new DropBasedOnExpertMode(new DropOneByOne(ModContent.ItemType<FilamentFragment>(), parameters2), new DropOneByOne(ModContent.ItemType<FilamentFragment>(), parameters3)));
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        SpriteBatch mySpriteBatch = spriteBatch;
        NPC rCurrentNPC = NPC;

        int num230 = ShieldStrengthTowerFilamentTower;
        string key = ShaderLoader.Filament;

        SpriteEffects spriteEffects = SpriteEffects.None;

        int type = NPC.type;

        Color npcColor = drawColor;

        float num35 = 0f;
        float num36 = Main.NPCAddHeight(rCurrentNPC);

        Vector2 halfSize = new Vector2(TextureAssets.Npc[type].Width() / 2, TextureAssets.Npc[type].Height() / Main.npcFrameCount[type] / 2);

        Texture2D value60 = TextureAssets.Npc[type].Value;
        Vector2 vector65 = rCurrentNPC.Center - screenPos;
        Vector2 vector66 = vector65 - new Vector2(300f, 310f);
        vector65 -= new Vector2(value60.Width, value60.Height / Main.npcFrameCount[type]) * rCurrentNPC.scale / 2f;
        vector65 += halfSize * rCurrentNPC.scale + new Vector2(0f, num35 + num36 + rCurrentNPC.gfxOffY);
        mySpriteBatch.Draw(value60, vector65, rCurrentNPC.frame, rCurrentNPC.GetAlpha(npcColor), rCurrentNPC.rotation, halfSize, rCurrentNPC.scale, spriteEffects, 0f);

        value60 = _glowMaskTexture.Value;
        float num226 = 4f + (rCurrentNPC.GetAlpha(npcColor).ToVector3() - new Vector3(0.5f)).Length() * 4f;
        for (int num227 = 0; num227 < 4; num227++) {
            mySpriteBatch.Draw(value60, vector65 + rCurrentNPC.velocity.RotatedBy((float)num227 * ((float)Math.PI / 2f)) * num226, rCurrentNPC.frame, new Microsoft.Xna.Framework.Color(64, 64, 64, 0) * rCurrentNPC.Opacity, rCurrentNPC.rotation, halfSize, rCurrentNPC.scale, spriteEffects, 0f);
        }

        float num231 = (float)num230 / (float)NPC.ShieldStrengthTowerMax;
        if (rCurrentNPC.IsABestiaryIconDummy)
            return false;

        if (num230 > 0) {
            mySpriteBatch.End();
            mySpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.Default, RasterizerState.CullNone, null, Main.Transform);
            float num232 = 0f;
            if (rCurrentNPC.ai[3] > 0f && rCurrentNPC.ai[3] <= 30f)
                num232 = 1f - rCurrentNPC.ai[3] / 30f;

            Terraria.Graphics.Effects.Filters.Scene[key].GetShader().UseIntensity(1f + num232).UseProgress(0f);
            DrawData value61 = new DrawData(ResourceManager.Perlin, vector66 + new Vector2(300f, 300f), new Microsoft.Xna.Framework.Rectangle(0, 0, 600, 600), Microsoft.Xna.Framework.Color.White * (num231 * 0.8f + 0.2f), rCurrentNPC.rotation, new Vector2(300f, 300f), rCurrentNPC.scale * (1f + num232 * 0.05f), spriteEffects);
            GameShaders.Misc["ForceField"].UseColor(new Vector3(1f + num232 * 0.5f));
            GameShaders.Misc["ForceField"].Apply(value61);
            value61.Draw(mySpriteBatch);
            mySpriteBatch.End();
            mySpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
        }
        else if (rCurrentNPC.ai[3] > 0f) {
            mySpriteBatch.End();
            mySpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.Default, RasterizerState.CullNone, null, Main.Transform);
            float num233 = rCurrentNPC.ai[3] / 120f;
            float num234 = Math.Min(rCurrentNPC.ai[3] / 30f, 1f);
            Terraria.Graphics.Effects.Filters.Scene[key].GetShader().UseIntensity(Math.Min(5f, 15f * num233) + 1f).UseProgress(num233);
            DrawData value62 = new DrawData(ResourceManager.Perlin, vector66 + new Vector2(300f, 300f), new Microsoft.Xna.Framework.Rectangle(0, 0, 600, 600), new Microsoft.Xna.Framework.Color(new Vector4(1f - (float)Math.Sqrt(num234))), rCurrentNPC.rotation, new Vector2(300f, 300f), rCurrentNPC.scale * (1f + num234), spriteEffects);
            GameShaders.Misc["ForceField"].UseColor(new Vector3(2f));
            GameShaders.Misc["ForceField"].Apply(value62);
            value62.Draw(mySpriteBatch);
            mySpriteBatch.End();
            mySpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
        }
        else {
            Terraria.Graphics.Effects.Filters.Scene[key].GetShader().UseIntensity(0f).UseProgress(0f);
        }

        return false;
    }
}
