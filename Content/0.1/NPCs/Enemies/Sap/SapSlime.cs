using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Content.Items.Consumables;
using RoA.Content.Items.Materials;
using RoA.Content.Items.Placeable.Banners;
using RoA.Content.Tiles.Crafting;
using RoA.Core.Utility;

using System;

using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;
using Terraria.WorldBuilding;

namespace RoA.Content.NPCs.Enemies.Sap;

sealed class SapSlime : ModNPC {
    public override void Load() {
        On_Player.ApplyEquipFunctional += On_Player_ApplyEquipFunctional;
    }

    private void On_Player_ApplyEquipFunctional(On_Player.orig_ApplyEquipFunctional orig, Player self, Item currentItem, bool hideVisual) {
        orig(self, currentItem, hideVisual);

        if ((currentItem.expertOnly && !Main.expertMode) || (currentItem.masterOnly && !Main.masterMode))
            return;

        int type = ModContent.NPCType<SapSlime>();
        if (currentItem.type == ItemID.RoyalGel) {
            self.npcTypeNoAggro[type] = true;
        }
    }

    public override void SetStaticDefaults() {
        //DisplayName.SetDefault("Sap Slime");
        Main.npcFrameCount[NPC.type] = 3;

        NPCID.Sets.ShimmerTransformToNPC[Type] = NPCID.ShimmerSlime;

        NPCID.Sets.ImmuneToRegularBuffs[Type] = true;
        NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
    }

    public override void SetDefaults() {
        NPC.lifeMax = 50;
        NPC.defense = 4;
        NPC.damage = 12;

        NPC.width = 34;
        NPC.height = 24;
        NPC.npcSlots = 0.4f;

        NPC.value = Item.buyPrice(0, 0, 0, 75);
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.aiStyle = 1;
        NPC.alpha = 100;

        Banner = Type;
        BannerItem = ModContent.ItemType<SapSlimeBanner>();
    }

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
        bestiaryEntry.AddTags(BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,
            new FlavorTextBestiaryInfoElement("Mods.RoA.Bestiary.SapSlime"));
    }

    private int GenerateItemInsideBody(bool isBallooned) {
        //if (Main.rand.NextBool(3)) {
        WeightedRandom<int> weightedRandom = new(Main.rand);
        weightedRandom.Add(ModContent.ItemType<SlipperyBomb>());
        //weightedRandom.Add(ModContent.ItemType<SlipperyDynamite>(), 0.5);
        weightedRandom.Add(ModContent.ItemType<SlipperyGrenade>());
        weightedRandom.Add(ModContent.ItemType<SlipperyGlowstick>());
        return weightedRandom.Get();
        //}
        //else {
        //    return ModContent.ItemType<Galipot>();
        //}
        //switch (Main.rand.Next(1)) {
        //    case 0:
        //        return ModContent.ItemType<Galipot>();
        //    //case 1:
        //    //    return 72;
        //    //default:
        //    //    return 73;
        //}
    }
    private static void DrawNPC_SlimeItem(NPC rCurrentNPC, int typeCache, Microsoft.Xna.Framework.Color npcColor, float addedRotation) {
        int num = (int)rCurrentNPC.ai[1];
        float num2 = 1f;
        float num3 = 34 * rCurrentNPC.scale * 0.575f;
        float num4 = 24 * rCurrentNPC.scale * 0.575f;
        Main.GetItemDrawFrame(num, out var itemTexture, out var rectangle);
        float num5 = rectangle.Width;
        float num6 = rectangle.Height;
        bool num7 = (int)rCurrentNPC.ai[0] == -999;
        if (num7) {
            num3 = 14f * rCurrentNPC.scale;
            num4 = 14f * rCurrentNPC.scale;
        }

        if (num5 > num3) {
            num2 *= num3 / num5;
            num5 *= num2;
            num6 *= num2;
        }

        if (num6 > num4) {
            num2 *= num4 / num6;
            num5 *= num2;
            num6 *= num2;
        }

        float num8 = -1f;
        float num9 = 1f;
        int num10 = rCurrentNPC.frame.Y / (TextureAssets.Npc[typeCache].Height() / Main.npcFrameCount[typeCache]);
        num9 -= (float)num10;
        num9 += 2f;
        num8 += (float)(num10 * 2);
        float num11 = 0.2f;
        num11 -= 0.3f * (float)num10;
        if (num7) {
            num11 = 0f;
            num9 -= 6f;
            num8 -= num5 * addedRotation;
        }

        if (num == 75) {
            npcColor = new Microsoft.Xna.Framework.Color(255, 255, 255, 0);
            num11 *= 0.3f;
            num9 -= 2f;
        }

        npcColor = rCurrentNPC.GetShimmerColor(npcColor);
        Main.spriteBatch.Draw(itemTexture, new Vector2(rCurrentNPC.Center.X - Main.screenPosition.X + num8, rCurrentNPC.Center.Y - Main.screenPosition.Y + rCurrentNPC.gfxOffY + num9), rectangle, npcColor, num11, rectangle.Size() / 2f, num2, SpriteEffects.None, 0f);
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        if (NPC.ai[1] > 0f)
            DrawNPC_SlimeItem(NPC, NPC.type, drawColor, 0f);
        return true;
    }

    public override void OnKill() {
        if (NPC.ai[1] > 0f) {
            int type = (int)NPC.ai[1];
            int stack = 1;
            int bomb = ModContent.ItemType<SlipperyBomb>();
            int stick = ModContent.ItemType<SlipperyGlowstick>();
            int grenade = ModContent.ItemType<SlipperyGrenade>();
            if (type == bomb || type == stick || type == grenade) {
                stack = Main.rand.Next(2, 7);
            }
            Item.NewItem(NPC.GetSource_Loot(), NPC.getRect(), (int)NPC.ai[1], stack);
        }
    }

    public override void AI() {
        if (NPC.IsGrounded()) {
            NPC.ai[0] += 1f + (Main.expertMode ? 0.5f : 0f);
        }
        if (NPC.ai[1] == 0f && Main.netMode != 1) {
            NPC.ai[1] = -1f;
            if (Main.remixWorld && NPC.ai[0] != -999f && Main.rand.Next(3) == 0) {
                NPC.ai[1] = 75f;
                NPC.netUpdate = true;
            }
            else if (Main.rand.NextChance(0.05f)) {
                int num2 = GenerateItemInsideBody(NPC.ai[0] == -999f);
                NPC.ai[1] = num2;
                NPC.netUpdate = true;
            }
        }
    }

    public override void FindFrame(int frameHeight) {
        NPC.frameCounter += 0.2;
        NPC.frameCounter %= Main.npcFrameCount[NPC.type];
        int frame = (int)NPC.frameCounter;
        NPC.frame.Y = frame * frameHeight;
    }

    public override void HitEffect(NPC.HitInfo hit) {
        int num39 = 8;
        float num40 = 1.1f;
        short num41 = (short)ModContent.DustType<Dusts.Galipot>();
        if (NPC.life <= 0) {
            num40 = 1.5f;
            num39 = 40;
        }

        for (int num42 = 0; num42 < num39; num42++) {
            Dust dust6 = Main.dust[Dust.NewDust(NPC.position, NPC.width, NPC.height, num41, 2 * hit.HitDirection, -1f, 80, default(Color), num40)];
            if (Main.rand.Next(3) != 0)
                dust6.noGravity = true;

            Dust dust = dust6;
            dust.velocity *= 1.5f;
            dust = dust6;
            dust.velocity += NPC.velocity * 0.1f;
        }
    }

    /*public override void NPCLoot() {
        if (Main.rand.Next(2) == 1)
            Item.NewItem((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height, mod.ItemType("Galipot"), Main.rand.Next(1, 3));
    }*/

    public override float SpawnChance(NPCSpawnInfo spawnInfo) {
        int y = spawnInfo.SpawnTileY;
        return ModContent.GetInstance<SapTileCount>().isSapActive ?
            MathHelper.Clamp((float)ModContent.GetInstance<SapTileCount>().tapperTilesCount / 6f, 0f, 2f) / 5f : 0f;
    }
}

sealed class SapTileCount : ModSystem {
    public int tapperTilesCount;
    public bool isSapActive;

    public override void Load() {
        On_SceneMetrics.ScanAndExportToMain += On_SceneMetrics_ScanAndExportToMain;
    }

    private void On_SceneMetrics_ScanAndExportToMain(On_SceneMetrics.orig_ScanAndExportToMain orig, SceneMetrics self, SceneMetricsScanSettings settings) {
        orig(self, settings);

        int num = 0;
        if (settings.BiomeScanCenterPositionInWorld.HasValue) {
            Point point = settings.BiomeScanCenterPositionInWorld.Value.ToTileCoordinates();
            Rectangle tileRectangle = new Rectangle(point.X - Main.buffScanAreaWidth / 2, point.Y - Main.buffScanAreaHeight / 2, Main.buffScanAreaWidth, Main.buffScanAreaHeight);
            tileRectangle = WorldUtils.ClampToWorld(tileRectangle);
            for (int i = tileRectangle.Left; i < tileRectangle.Right; i++) {
                for (int j = tileRectangle.Top; j < tileRectangle.Bottom; j++) {
                    if (!tileRectangle.Contains(i, j)) {
                        continue;
                    }

                    Tile tile = Main.tile[i, j];
                    if (!tile.HasTile) {
                        continue;
                    }

                    if (tile.TileType != ModContent.TileType<Tapper>()) {
                        continue;
                    }

                    tileRectangle.Contains(i, j);

                    if (tile.TileType == ModContent.TileType<Tapper>()) {
                        TapperTE tapperTE = TileHelper.GetTE<TapperTE>(i, j);
                        if (tapperTE != null && tapperTE.IsReadyToCollectGalipot) {
                            num++;
                        }
                    }
                }
            }
        }

        ModContent.GetInstance<SapTileCount>().tapperTilesCount = num;
    }

    public override void TileCountsAvailable(ReadOnlySpan<int> tileCounts)
        => tapperTilesCount = tileCounts[ModContent.TileType<Tapper>()];

    public override void PostUpdateWorld()
        => isSapActive = tapperTilesCount >= 2;
}

sealed class SapPlayer : ModPlayer {
    public bool useBottle;

    public override void PreUpdate() {
        if (Player.inventory[Player.selectedItem].type == ItemID.Bottle && Player.itemAnimation >= Player.itemAnimationMax)
            useBottle = true;
        if (Player.itemAnimation <= 0)
            useBottle = false;
    }

    public override void PostUpdateMiscEffects() {
        int itemID = ItemID.Bottle;
        if (Player.inventory[Player.selectedItem].type != itemID || Player.itemAnimation <= 0)
            return;
        Rectangle firstRectangle = new Rectangle((int)Player.itemLocation.X, (int)Player.itemLocation.Y, Player.inventory[Player.selectedItem].width, Player.inventory[Player.selectedItem].height);
        firstRectangle.Width = (int)((float)firstRectangle.Width * Player.inventory[Player.selectedItem].scale);
        firstRectangle.Height = (int)((float)firstRectangle.Height * Player.inventory[Player.selectedItem].scale);
        if (Player.direction == -1)
            firstRectangle.X -= firstRectangle.Width;
        if (Player.gravDir == 1f)
            firstRectangle.Y -= firstRectangle.Height;
        if ((double)Player.itemAnimation < (double)Player.itemAnimationMax * 0.333) {
            if (Player.direction == -1)
                firstRectangle.X -= (int)((double)firstRectangle.Width * 1.4 - (double)firstRectangle.Width);
            firstRectangle.Width = (int)((double)firstRectangle.Width * 1.4);
            firstRectangle.Y += (int)((double)firstRectangle.Height * 0.5 * (double)Player.gravDir);
            firstRectangle.Height = (int)((double)firstRectangle.Height * 1.1);
        }
        else if ((double)Player.itemAnimation >= (double)Player.itemAnimationMax * 0.666) {
            if (Player.direction == 1)
                firstRectangle.X -= (int)((double)firstRectangle.Width * 1.2);
            firstRectangle.Width *= 2;
            firstRectangle.Y -= (int)(((double)firstRectangle.Height * 1.4 - (double)firstRectangle.Height) * (double)Player.gravDir);
            firstRectangle.Height = (int)((double)firstRectangle.Height * 1.4);
        }
        for (int i = 0; i < Main.maxNPCs; i++) {
            NPC npc = Main.npc[i];
            if (npc.active && npc.type == (ushort)ModContent.NPCType<SapSlime>()) {
                Rectangle secondRectangle = new Rectangle((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height);
                if (useBottle && firstRectangle.Intersects(secondRectangle) && (npc.noTileCollide || Collision.CanHit(Player.position, Player.width, Player.height, npc.position, npc.width, npc.height))) {
                    if (Player.CountItem(itemID) >= 1) {
                        useBottle = false;
                        SoundEngine.PlaySound(SoundID.Drip, Player.position);
                        foreach (Item inventoryItem in Player.inventory)
                            if (inventoryItem.type == itemID) {
                                int removed = Math.Min(inventoryItem.stack, 1);
                                inventoryItem.stack -= removed;
                                if (inventoryItem.stack <= 0)
                                    inventoryItem.SetDefaults();
                                break;
                            }
                        int item = Item.NewItem(npc.GetSource_CatchEntity(npc), (int)npc.position.X, (int)npc.position.Y, npc.width, npc.height, ModContent.ItemType<Galipot>(), 1, false, 0, false, false);
                        if (Main.netMode == NetmodeID.MultiplayerClient && item >= 0)
                            NetMessage.SendData(21, -1, -1, null, item, 1f, 0f, 0f, 0, 0, 0);
                        if (npc.life <= npc.lifeMax * 0.2f || npc.scale <= 0.35f)
                            npc.StrikeNPC(new NPC.HitInfo() {
                                Damage = Main.rand.Next(5, 15),
                                Knockback = Main.rand.NextFloat(0.75f, 1.5f),
                                HitDirection = (Player.direction > 0f) ? 1 : -1
                            });
                        npc.scale *= 0.8f;
                        npc.life -= (int)(npc.lifeMax * Main.rand.NextFloat(0.15f, 0.25f));
                    }
                }
            }
        }
    }
}
