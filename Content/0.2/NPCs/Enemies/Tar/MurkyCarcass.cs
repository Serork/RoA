using Microsoft.Xna.Framework;

using ModLiquidLib.ModLoader;
using ModLiquidLib.Utils;

using RoA.Common;
using RoA.Common.Crossmod;
using RoA.Content.Items.Placeable.Banners;
using RoA.Content.Liquids;
using RoA.Core.Utility;

using System;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.NPCs.Enemies.Tar;

[Tracked]
sealed class MurkyCarcass : ModNPC {
    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
        bestiaryEntry.Info.AddRange([
            new FlavorTextBestiaryInfoElement($"Mods.RoA.Bestiary.{nameof(MurkyCarcass)}")
        ]);
    }

    private static Point16 _spawnPosition;

    public override void SetStaticDefaults() {
        NPC.SetFrameCount(6);
    }

    public override void SetDefaults() {
        NPC.SetSizeValues(28, 22);
        NPC.DefaultToEnemy(new NPCExtensions.NPCHitInfo(500, 40, 16, 0f));

        NPC.aiStyle = -1;
        NPC.noGravity = true;
        NPC.npcSlots = 0.5f;

        Banner = Type;
        BannerItem = ModContent.ItemType<MurkyCarcassBanner>();

        NPC.Opacity = 0f;
    }

    public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) {
        if (!NPC.chaseable) {
            modifiers.FinalDamage *= 0f;
        }
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
        if (!NPC.chaseable) {
            modifiers.FinalDamage *= 0f;
        }
    }

    public override int SpawnNPC(int tileX, int tileY) {
        Vector2 spawnPosition = _spawnPosition.ToWorldCoordinates();
        return NPC.NewNPC(new EntitySource_SpawnNPC(), (int)spawnPosition.X, (int)spawnPosition.Y, Type);
    }

    public static bool FindTarLiquid(int landX, int landY, out int liquidX, out int liquidY) {
        liquidX = landX;
        liquidY = landY;
        if (!WorldGen.InWorld(landX, landY, 101))
            return false;

        int num = 1;
        for (int i = landX - 100; i <= landX + 100; i++) {
            for (int j = landY - 100; j <= landY + 100; j++) {
                bool result = false;
                if (Main.tile[i, j - 1].LiquidAmount > 0 && Main.tile[i, j - 2].LiquidAmount > 0 && Main.tile[i, j - 1].LiquidType == LiquidLoader.LiquidType<Liquids.Tar>()) {
                    result = true;
                }
                if (result && Main.rand.Next(num) == 0) {
                    for (int checkX = -1; checkX < 2; checkX++) {
                        for (int checkY = -1; checkY < 2; checkY++) {
                            if (Main.tile[i + checkX, j + checkY].HasTile) {
                                result = false;
                            }
                        }
                    }
                    if (result) {
                        for (int i2 = i - 5; i2 <= i + 5; i2++) {
                            for (int j2 = j - 5; j2 <= j + 5; j2++) {
                                foreach (NPC activeMurkyCarcass in TrackedEntitiesSystem.GetTrackedNPC<MurkyCarcass>()) {
                                    if (activeMurkyCarcass.Center.ToTileCoordinates16() == new Point16(i2, j2)) {
                                        result = false;
                                    }
                                }
                            }
                        }
                        if (result) {
                            liquidX = i;
                            liquidY = j;
                            num++;
                        }
                    }
                }
            }
        }

        if (liquidX != landX || liquidY != landY) {
            return true;
        }

        return false;
    }

    public override float SpawnChance(NPCSpawnInfo spawnInfo) {
        if (spawnInfo.Invasion)
            return 0f;

        if (FindTarLiquid(spawnInfo.SpawnTileX, spawnInfo.SpawnTileY, out int liquidX, out int liquidY)) {
            _spawnPosition = new Point16(liquidX, liquidY);
            return 0.5f;
        }
        return 0f;
    }

    public override void UpdateLifeRegen(ref int damage) {
        if (NPC.wet && !NPC.GetModdedWetArray()[LiquidLoader.LiquidType<Liquids.Tar>() - LiquidID.Count]) {
            if (NPC.lifeRegen > 0)
                NPC.lifeRegen = 0;

            NPC.lifeRegen -= 50;
        }
    }

    private ref float Spawn => ref NPC.ai[2];

    public override void AI() {
        if (NPC.direction == 0)
            NPC.TargetClosest();

        //if (Spawn <= 0f) {
        //    Spawn = 1f;
        //    if (Main.netMode != NetmodeID.MultiplayerClient) {
        //        for (int i = 0; i < 2; i++) {
        //            int npc = NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y, Type, NPC.whoAmI, ai2: 1f);
        //            if (Main.netMode == NetmodeID.Server && npc < Main.maxNPCs) {
        //                NetMessage.SendData(MessageID.SyncNPC, number: npc);
        //            }
        //        }
        //    }
        //}

        //if (type == 615) {
        //    if (this.ai[2] == 0f) {
        //        int num261 = Main.rand.Next(300, 1200);
        //        if ((this.ai[3] += 1f) >= (float)num261) {
        //            this.ai[2] = Main.rand.Next(1, 3);
        //            if (this.ai[2] == 1f && !Collision.CanHitLine(position, width, height, new Vector2(position.X, position.Y - 128f), width, height))
        //                this.ai[2] = 2f;

        //            if (this.ai[2] == 2f)
        //                TargetClosest();

        //            this.ai[3] = 0f;
        //            netUpdate = true;
        //        }
        //    }

        //    if (this.ai[2] == 1f) {
        //        if (collideY || collideX) {
        //            this.ai[2] = 0f;
        //            this.ai[3] = 0f;
        //            netUpdate = true;
        //        }
        //        else if (wet) {
        //            velocity.Y -= 0.4f;
        //            if (velocity.Y < -6f)
        //                velocity.Y = -6f;

        //            rotation = velocity.Y * (float)direction * 0.3f;
        //            if (rotation < (float)Math.PI * -2f / 5f)
        //                rotation = (float)Math.PI * -2f / 5f;

        //            if (rotation > (float)Math.PI * 2f / 5f)
        //                rotation = (float)Math.PI * 2f / 5f;

        //            if (this.ai[3] == 1f) {
        //                this.ai[2] = 0f;
        //                this.ai[3] = 0f;
        //                netUpdate = true;
        //            }
        //        }
        //        else {
        //            rotation += (float)direction * 0.2f;
        //            this.ai[3] = 1f;
        //            velocity.Y += 0.3f;
        //            if (velocity.Y > 10f)
        //                velocity.Y = 10f;
        //        }

        //        return;
        //    }

        //    if (this.ai[2] == 2f) {
        //        if (collideY || collideX) {
        //            this.ai[2] = 0f;
        //            this.ai[3] = 0f;
        //            netUpdate = true;
        //        }
        //        else if (wet) {
        //            velocity.Y -= 0.4f;
        //            if (velocity.Y < -6f)
        //                velocity.Y = -6f;

        //            rotation = velocity.Y * (float)direction * 0.3f;
        //            if (rotation < (float)Math.PI * -2f / 5f)
        //                rotation = (float)Math.PI * -2f / 5f;

        //            if (rotation > (float)Math.PI * 2f / 5f)
        //                rotation = (float)Math.PI * 2f / 5f;

        //            if (Collision.GetWaterLine(base.Top.ToTileCoordinates(), out var waterLineHeight)) {
        //                float y2 = waterLineHeight + 0f - position.Y;
        //                velocity.Y = y2;
        //                velocity.Y = MathHelper.Clamp(velocity.Y, -2f, 0.5f);
        //                rotation = -(float)Math.PI / 5f * (float)direction;
        //                velocity.X *= 0.95f;
        //                if (this.ai[3] == 0f)
        //                    netUpdate = true;

        //                this.ai[3]++;
        //                if (this.ai[3] >= 300f) {
        //                    this.ai[2] = 0f;
        //                    this.ai[3] = 0f;
        //                    netUpdate = true;
        //                    velocity.Y = 4f;
        //                }

        //                if (this.ai[3] == 60f && Main.rand.Next(2) == 0)
        //                    SoundEngine.PlaySound(45, (int)position.X, (int)position.Y);
        //            }
        //        }
        //        else {
        //            this.ai[2] = 0f;
        //            this.ai[3] = 0f;
        //            netUpdate = true;
        //            velocity.Y += 0.3f;
        //            if (velocity.Y > 10f)
        //                velocity.Y = 10f;
        //        }

        //        return;
        //    }
        //}

        //NPC.friendly = NPC.chaseable;
        NPC.ShowNameOnHover = NPC.chaseable;
        NPC.chaseable = NPC.Opacity > 0.5f;
        NPC.Opacity = Helper.Approach(NPC.Opacity, NPC.localAI[2], 0.1f);

        if (NPC.HasValidTarget) {
            float neededDistance = 100f;
            float distance = NPC.GetTargetPlayer().Distance(NPC.Center);
            bool flag = distance < neededDistance;
            bool flag2 = false;
            for (int checkY = -1; checkY <= 0; checkY++) {
                Point16 positionInTiles = NPC.Bottom.ToTileCoordinates16();
                if (WorldGenHelper.GetTileSafely(positionInTiles.X, positionInTiles.Y + checkY).LiquidAmount < 32) {
                    flag2 = true;
                    break;
                }
            }
            if (NPC.wet && !flag2) {
                if (flag) {
                    NPC.localAI[2] = 1f;
                }
                else {
                    NPC.localAI[2] = Utils.Remap(distance, 100f, 200f, 1f, 0f, true);
                }
            }
            else {
                NPC.localAI[2] = 1f;
            }
        }
        else {
            NPC.localAI[2] = NPC.wet ? 0f : 1f;
        }

        if (NPC.wet) {
            NPC.OffsetTheSameNPC(0.1f);

            //if (!RoALiquidsCompat.IsTarWet(NPC)) {
            //    NPC.velocity *= 0.7f;
            //}

            bool flag11 = false;
            if (NPC.type != 55 && NPC.type != 592 && NPC.type != 607 && NPC.type != 615) {
                NPC.TargetClosest(faceTarget: false);
                if ((Main.player[NPC.target].wet || (Main.expertMode && Vector2.Distance(NPC.Center, Main.player[NPC.target].Center) < 300f)) && !Main.player[NPC.target].dead && Collision.CanHit(NPC.position, NPC.width, NPC.height, Main.player[NPC.target].position, Main.player[NPC.target].width, Main.player[NPC.target].height))
                    flag11 = true;
            }

            int num262 = (int)NPC.Center.X / 16;
            int num263 = (int)(NPC.position.Y + (float)NPC.height) / 16;
            if (Main.tile[num262, num263].TopSlope) {
                if (Main.tile[num262, num263].LeftSlope) {
                    NPC.direction = -1;
                    NPC.velocity.X = Math.Abs(NPC.velocity.X) * -1f;
                }
                else {
                    NPC.direction = 1;
                    NPC.velocity.X = Math.Abs(NPC.velocity.X);
                }
            }
            else if (Main.tile[num262, num263 + 1].TopSlope) {
                if (Main.tile[num262, num263 + 1].LeftSlope) {
                    NPC.direction = -1;
                    NPC.velocity.X = Math.Abs(NPC.velocity.X) * -1f;
                }
                else {
                    NPC.direction = 1;
                    NPC.velocity.X = Math.Abs(NPC.velocity.X);
                }
            }

            if (!flag11) {
                if (NPC.collideX) {
                    NPC.velocity.X *= -1f;
                    NPC.direction *= -1;
                    NPC.netUpdate = true;
                }

                if (NPC.collideY) {
                    NPC.netUpdate = true;
                    if (NPC.velocity.Y > 0f) {
                        NPC.velocity.Y = Math.Abs(NPC.velocity.Y) * -1f;
                        NPC.directionY = -1;
                        NPC.ai[0] = -1f;
                    }
                    else if (NPC.velocity.Y < 0f) {
                        NPC.velocity.Y = Math.Abs(NPC.velocity.Y);
                        NPC.directionY = 1;
                        NPC.ai[0] = 1f;
                    }
                }
            }

            if (NPC.type == 102)
                Lighting.AddLight((int)(NPC.position.X + (float)(NPC.width / 2) + (float)(NPC.direction * (NPC.width + 8))) / 16, (int)(NPC.position.Y + 2f) / 16, 0.07f, 0.04f, 0.025f);

            if (!NPC.GetModdedWetArray()[LiquidLoader.LiquidType<Liquids.Tar>() - LiquidID.Count]) {
                flag11 = false;
            }

            if (flag11) {
                NPC.TargetClosest();
                if (true/*NPC.type == 157*/) {
                    if (NPC.velocity.X > 0f && NPC.direction < 0)
                        NPC.velocity.X *= 0.95f;

                    if (NPC.velocity.X < 0f && NPC.direction > 0)
                        NPC.velocity.X *= 0.95f;

                    NPC.velocity.X += (float)NPC.direction * 0.25f;
                    NPC.velocity.Y += (float)NPC.directionY * 0.2f;
                    if (NPC.velocity.X > 8f)
                        NPC.velocity.X = 7f;

                    if (NPC.velocity.X < -8f)
                        NPC.velocity.X = -7f;

                    if (NPC.velocity.Y > 5f)
                        NPC.velocity.Y = 4f;

                    if (NPC.velocity.Y < -5f)
                        NPC.velocity.Y = -4f;
                }
                //else if (NPC.type == 65 || NPC.type == 102) {
                //    NPC.velocity.X += (float)NPC.direction * 0.15f;
                //    NPC.velocity.Y += (float)NPC.directionY * 0.15f;
                //    if (NPC.velocity.X > 5f)
                //        NPC.velocity.X = 5f;

                //    if (NPC.velocity.X < -5f)
                //        NPC.velocity.X = -5f;

                //    if (NPC.velocity.Y > 3f)
                //        NPC.velocity.Y = 3f;

                //    if (NPC.velocity.Y < -3f)
                //        NPC.velocity.Y = -3f;
                //}
                //else {
                //    NPC.velocity.X += (float)NPC.direction * 0.1f;
                //    NPC.velocity.Y += (float)NPC.directionY * 0.1f;
                //    if (NPC.velocity.X > 3f)
                //        NPC.velocity.X = 3f;

                //    if (NPC.velocity.X < -3f)
                //        NPC.velocity.X = -3f;

                //    if (NPC.velocity.Y > 2f)
                //        NPC.velocity.Y = 2f;

                //    if (NPC.velocity.Y < -2f)
                //        NPC.velocity.Y = -2f;
                //}
            }
            else {
                if (true/*NPC.type == 157*/) {
                    if (Main.player[NPC.target].position.Y > NPC.position.Y)
                        NPC.directionY = 1;
                    else
                        NPC.directionY = -1;

                    NPC.velocity.X += (float)NPC.direction * 0.2f;
                    if (NPC.velocity.X < -2f || NPC.velocity.X > 2f)
                        NPC.velocity.X *= 0.95f;

                    if (NPC.ai[0] == -1f) {
                        float num264 = -0.6f;
                        //if (NPC.directionY < 0)
                        //    num264 = -1f;

                        //if (NPC.directionY > 0)
                        //    num264 = -0.2f;

                        NPC.velocity.Y -= 0.02f;
                        if (NPC.velocity.Y < num264 || !NPC.wet || (NPC.velocity.Length() > 1f && (NPC.collideX || NPC.collideY)))
                            NPC.ai[0] = 1f;
                    }
                    else {
                        float num265 = 0.6f;
                        //if (NPC.directionY < 0)
                        //    num265 = 0.2f;

                        //if (NPC.directionY > 0)
                        //    num265 = 1f;

                        NPC.velocity.Y += 0.02f;
                        if (NPC.velocity.Y > num265 || !NPC.wet || (NPC.velocity.Length() > 1f && (NPC.collideX || NPC.collideY)))
                            NPC.ai[0] = -1f;
                    }
                }
                //else {
                //    NPC.velocity.X += (float)NPC.direction * 0.1f;
                //    float num266 = 1f;
                //    if (NPC.type == 615)
                //        num266 = 3f;

                //    if (NPC.velocity.X < 0f - num266 || NPC.velocity.X > num266)
                //        NPC.velocity.X *= 0.95f;

                //    if (NPC.ai[0] == -1f) {
                //        NPC.velocity.Y -= 0.01f;
                //        if ((double)NPC.velocity.Y < -0.3)
                //            NPC.ai[0] = 1f;
                //    }
                //    else {
                //        NPC.velocity.Y += 0.01f;
                //        if ((double)NPC.velocity.Y > 0.3)
                //            NPC.ai[0] = -1f;
                //    }
                //}

                int num267 = (int)(NPC.position.X + (float)(NPC.width / 2)) / 16;
                int num268 = (int)(NPC.position.Y + (float)(NPC.height / 2)) / 16;
                //if (Main.tile[num267, num268 - 1] == null)
                //    Main.tile[num267, num268 - 1] = new Tile();

                //if (Main.tile[num267, num268 + 1] == null)
                //    Main.tile[num267, num268 + 1] = new Tile();

                //if (Main.tile[num267, num268 + 2] == null)
                //    Main.tile[num267, num268 + 2] = new Tile();

                if (Main.tile[num267, num268 - 1].LiquidAmount > 128) {
                    if (Main.tile[num267, num268 + 1].HasTile)
                        NPC.ai[0] = -1f;
                    else if (Main.tile[num267, num268 + 2].HasTile)
                        NPC.ai[0] = -1f;
                }

                //if (NPC.type != 157 && ((double)NPC.velocity.Y > 0.4 || (double)NPC.velocity.Y < -0.4))
                //    NPC.velocity.Y *= 0.95f;
            }
        }
        else {
            if (NPC.IsGrounded()) {
                if (NPC.type == 65) {
                    NPC.velocity.X *= 0.94f;
                    if ((double)NPC.velocity.X > -0.2 && (double)NPC.velocity.X < 0.2)
                        NPC.velocity.X = 0f;
                }
                else if (Main.netMode != 1) {
                    NPC.velocity.Y = (float)Main.rand.Next(-50, -20) * 0.1f;
                    NPC.velocity.X = (float)Main.rand.Next(-20, 20) * 0.1f;
                    NPC.netUpdate = true;
                }
            }

            NPC.velocity.Y += 0.3f;
            if (NPC.velocity.Y > 10f)
                NPC.velocity.Y = 10f;

            NPC.ai[0] = 1f;
        }

        NPC.rotation = NPC.velocity.Y * (float)NPC.direction * 0.1f;
        if ((double)NPC.rotation < -0.2)
            NPC.rotation = -0.2f;

        if ((double)NPC.rotation > 0.2)
            NPC.rotation = 0.2f;

        if (Main.netMode != 2)
            return;

        NPC.netSpam = 0;
    }

    public override void OnKill() {
        if (Main.netMode != 2)
            return;

        NPC.netSpam = 0;
    }

    public override void FindFrame(int frameHeight) {
        NPC.spriteDirection = NPC.direction;
        NPC.frameCounter += 1.0;
        if (!NPC.wet)
            NPC.frameCounter += 1.0;

        int num56 = 5;
        if (NPC.frameCounter < (double)num56)
            NPC.frame.Y = 0;
        else if (NPC.frameCounter < (double)(num56 * 2))
            NPC.frame.Y = frameHeight;
        else if (NPC.frameCounter < (double)(num56 * 3))
            NPC.frame.Y = frameHeight * 2;
        else if (NPC.frameCounter < (double)(num56 * 4))
            NPC.frame.Y = frameHeight;
        else if (NPC.frameCounter < (double)(num56 * 5))
            NPC.frame.Y = frameHeight * 3;
        else if (NPC.frameCounter < (double)(num56 * 6))
            NPC.frame.Y = frameHeight * 4;
        else if (NPC.frameCounter < (double)(num56 * 7))
            NPC.frame.Y = frameHeight * 5;
        else if (NPC.frameCounter < (double)(num56 * 8))
            NPC.frame.Y = frameHeight * 4;
        else
            NPC.frameCounter = 0.0;

    }
}
