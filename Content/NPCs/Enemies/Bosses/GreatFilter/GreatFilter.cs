using Microsoft.Xna.Framework;

using RoA.Content.Dusts;

using System;
using System.Collections.Generic;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace RoA.Content.NPCs.Enemies.Bosses.GreatFilter;

[AutoloadBossHead]
sealed class GreatFilter : ModNPC {
    public static int ShieldStrengthTowerGreatFilter = 0;
    public static bool TowerActiveGreatFilter;

    public override void Load() {
        On_NPC.getTenthAnniversaryAdjustments += On_NPC_getTenthAnniversaryAdjustments;
        On_NPC.DoesntDespawnToInactivity += On_NPC_DoesntDespawnToInactivity;

        On_WorldGen.TriggerLunarApocalypse += On_WorldGen_TriggerLunarApocalypse;
    }

    private void On_WorldGen_TriggerLunarApocalypse(On_WorldGen.orig_TriggerLunarApocalypse orig) {
        List<int> list = new List<int> {
            517,
            422,
            507,
            493,
            ModContent.NPCType<GreatFilter>()
        };

        int[] array = new int[5];
        for (int i = 0; i < 5; i++) {
            array[i] = list[Main.rand.Next(list.Count)];
            list.Remove(array[i]);
        }

        int sectionCount = 7;
        int step = Main.maxTilesX / sectionCount;
        HashSet<Point16> possibleTowerPositions = [];
        for (int currentSection = 0; currentSection < sectionCount; currentSection++) {
            if (currentSection == 3 || currentSection == 6) {
                continue;
            }

            int x = step * (1 + currentSection) - step / 2;

            x += step / 2;

            //if (currentSection == 4 || currentSection == 5) {
            //    x += step;
            //}

            possibleTowerPositions.Add(new Point16(x, (int)Main.worldSurface));
        }
        foreach (Point16 pillarPosition in possibleTowerPositions) {
            int num3 = pillarPosition.X;
            int num2 = pillarPosition.Y;
            bool flag = false;

            int pillarNPCType = ModContent.NPCType<GreatFilter>();

            for (int k = 0; k < 30; k++) {
                int num4 = Main.rand.Next(-100, 101);
                if (Main.remixWorld && Main.getGoodWorld) {
                    int num5 = Main.rand.Next((int)Main.worldSurface, Main.maxTilesY - 350);
                    if (!WorldGen.PlayerLOS(num3 + num4 - 10, num5) && !WorldGen.PlayerLOS(num3 + num4 + 10, num5) && !WorldGen.PlayerLOS(num3 + num4 - 10, num5 - 20) && !WorldGen.PlayerLOS(num3 + num4 + 10, num5 - 20)) {
                        int num6 = NPC.NewNPC(new EntitySource_WorldEvent(), (num3 + num4) * 16, num5 * 16, pillarNPCType);
                        if (Main.netMode == 2 && num6 < 200)
                            NetMessage.SendData(23, -1, -1, null, num6);

                        flag = true;
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
                        break;
                    }
                }

                if (flag)
                    break;
            }

            if (!flag)
                NPC.NewNPC(new EntitySource_WorldEvent(), num3 * 16, (num2 - 40) * 16, pillarNPCType);
        }

        //int num = Main.maxTilesX / 5;
        //int num2 = (int)Main.worldSurface;
        //for (int j = 0; j < 5; j++) {
        //    int num3 = num * (1 + j);
        //    num3 -= num / 2;
        //    bool flag = false;
        //    for (int k = 0; k < 30; k++) {
        //        int num4 = Main.rand.Next(-100, 101);
        //        if (Main.remixWorld && Main.getGoodWorld) {
        //            int num5 = Main.rand.Next((int)Main.worldSurface, Main.maxTilesY - 350);
        //            if (!WorldGen.PlayerLOS(num3 + num4 - 10, num5) && !WorldGen.PlayerLOS(num3 + num4 + 10, num5) && !WorldGen.PlayerLOS(num3 + num4 - 10, num5 - 20) && !WorldGen.PlayerLOS(num3 + num4 + 10, num5 - 20)) {
        //                int num6 = NPC.NewNPC(new EntitySource_WorldEvent(), (num3 + num4) * 16, num5 * 16, array[j]);
        //                if (Main.netMode == 2 && num6 < 200)
        //                    NetMessage.SendData(23, -1, -1, null, num6);

        //                flag = true;
        //                break;
        //            }

        //            continue;
        //        }

        //        for (int num7 = num2; num7 > 100; num7--) {
        //            if (!Collision.SolidTiles(num3 + num4 - 10, num3 + num4 + 10, num7 - 20, num7 + 15) && !WorldGen.PlayerLOS(num3 + num4 - 10, num7) && !WorldGen.PlayerLOS(num3 + num4 + 10, num7) && !WorldGen.PlayerLOS(num3 + num4 - 10, num7 - 20) && !WorldGen.PlayerLOS(num3 + num4 + 10, num7 - 20)) {
        //                int num8 = NPC.NewNPC(new EntitySource_WorldEvent(), (num3 + num4) * 16, num7 * 16, array[j]);
        //                if (Main.netMode == 2 && num8 < 200)
        //                    NetMessage.SendData(23, -1, -1, null, num8);

        //                flag = true;
        //                break;
        //            }
        //        }

        //        if (flag)
        //            break;
        //    }

        //    if (!flag)
        //        NPC.NewNPC(new EntitySource_WorldEvent(), num3 * 16, (num2 - 40) * 16, array[j]);
        //}

        TowerActiveGreatFilter = NPC.TowerActiveVortex = (NPC.TowerActiveNebula = (NPC.TowerActiveSolar = (NPC.TowerActiveStardust = true)));
        NPC.LunarApocalypseIsUp = true;
        ShieldStrengthTowerGreatFilter = NPC.ShieldStrengthTowerSolar = (NPC.ShieldStrengthTowerVortex = (NPC.ShieldStrengthTowerNebula = (NPC.ShieldStrengthTowerStardust = NPC.ShieldStrengthTowerMax)));
        
        // TODO: add support
        NetMessage.SendData(101);
        WorldGen.MessageLunarApocalypse();
    }

    private bool On_NPC_DoesntDespawnToInactivity(On_NPC.orig_DoesntDespawnToInactivity orig, NPC self) {
        if (self.type == ModContent.NPCType<GreatFilter>()) {
            return true;
        }

        return orig(self);
    }

    private void On_NPC_getTenthAnniversaryAdjustments(On_NPC.orig_getTenthAnniversaryAdjustments orig, NPC self) {
        if (self.type == ModContent.NPCType<GreatFilter>()) {
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

            int num1465 = ModContent.DustType<GreatFilterDust>();
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
            flag88 = ShieldStrengthTowerGreatFilter != 0;
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

        NPC.dontTakeDamage = ShieldStrengthTowerGreatFilter != 0;

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
                    ModContent.DustType<GreatFilterDust>())];
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
}
