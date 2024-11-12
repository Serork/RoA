using Microsoft.Xna.Framework;

using RoA.Content.Items.Materials;
using RoA.Content.Tiles.Crafting;

using System;

using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.NPCs.Enemies.Sap;

sealed class SapSlime : ModNPC {
    public override void SetStaticDefaults() {
        //DisplayName.SetDefault("Sap Slime");
        Main.npcFrameCount[NPC.type] = 3;
    }

    public override void SetDefaults() {
        NPC.lifeMax = 50;
        NPC.defense = 3;
        NPC.damage = 5;
        NPC.width = 34;
        NPC.height = 24;
        NPC.npcSlots = 0.1f;
        NPC.value = Item.buyPrice(silver: 20);
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.aiStyle = 1;
        NPC.alpha = 100;
    }

    public override void FindFrame(int frameHeight) {
        NPC.frameCounter += 0.2;
        NPC.frameCounter %= Main.npcFrameCount[NPC.type];
        int frame = (int)NPC.frameCounter;
        NPC.frame.Y = frame * frameHeight;
    }

    public override void HitEffect(NPC.HitInfo hit) {
        //if (NPC.life <= 0) {
        //    for (int j = 0; j < 12; j++)
        //        Dust.NewDust(NPC.position, NPC.width, NPC.height, 3, hitDirection, -1f, 0, new Color(252, 193, 45), 1f);
        //}
        //else
        //    for (int i = 0; i < 3; i++)
        //        Dust.NewDust(NPC.position, NPC.width, NPC.height, 3, hitDirection, -1f, 0, new Color(252, 193, 45), 1f);
    }

    /*public override void NPCLoot() {
        if (Main.rand.Next(2) == 1)
            Item.NewItem((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height, mod.ItemType("Galipot"), Main.rand.Next(1, 3));
    }*/

    public override float SpawnChance(NPCSpawnInfo spawnInfo) {
        int y = spawnInfo.SpawnTileY;
        return ModContent.GetInstance<Sap>().isSapActive ? MathHelper.Clamp((float)ModContent.GetInstance<Sap>().tapperTilesCount / 10f, 0f, 1f) : 0f;
    }
}

sealed class Sap : ModSystem {
    public int tapperTilesCount;
    public bool isSapActive;
    public override void TileCountsAvailable(ReadOnlySpan<int> tileCounts)
        => tapperTilesCount = tileCounts[ModContent.TileType<Tapper>()];

		public override void PostUpdateWorld()
        => isSapActive = tapperTilesCount >= 3;
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
        for (int i = 0; i < Main.maxNPCs; i++)  {
				NPC npc = Main.npc[i];
            if (npc.active && npc.type == (ushort)ModContent.NPCType<SapSlime>())  {
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
