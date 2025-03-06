using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using MonoMod.Cil;

using RoA.Common.Sets;
using RoA.Common.Tiles;
using RoA.Content.Emotes;
using RoA.Content.NPCs.Friendly;
using RoA.Core.Utility;

using System;
using System.IO;

using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.ObjectInteractions;
using Terraria.GameContent.UI;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Miscellaneous;

sealed class TreeDryad : ModTile {
    private sealed class DryadAIChanges : GlobalNPC {
        public override bool? CanChat(NPC npc) {
            if (npc.type == NPCID.Dryad) {
                return npc.ai[0] != -20f;
            }

            return base.CanChat(npc);
        }

        public override void FindFrame(NPC npc, int frameHeight) {
            if (npc.type != NPCID.Dryad) {
                return;
            }
            if (npc.ai[0] != -25f) {
                return;
            }
            int num237 = Main.npcFrameCount[npc.type] - NPCID.Sets.AttackFrameCount[npc.type];
            npc.ai[2]++;
            int num269 = 0;
            num269 = (npc.ai[2] % 16.0 < 8.0) ? (num237 - 2) : 0;
            npc.frame.Y = frameHeight * num269;
        }

        public override void AI(NPC npc) {
            if (npc.type != NPCID.Dryad) {
                return;
            }
            if (npc.ai[0] == -25f) {
                if (npc.ai[1] > 0f) {
                    npc.ai[1] -= 1f;

                    npc.velocity.X *= 0.8f;

                    int dir = (Main.player[Player.FindClosest(npc.position, npc.width, npc.height)].Center.X - npc.Center.X).GetDirection();
                    npc.direction = npc.spriteDirection = dir;
                }
                else {
                    npc.ai[1] = npc.ai[0] = 0f;
                    npc.frameCounter = 0.0;
                    npc.ai[2] = 0;
                    npc.netUpdate = true;
                }
            }
            if (npc.ai[0] != -20f) {
                return;
            }
            if (npc.ai[1] > 0f) {
                npc.ai[1] -= 1;

                npc.velocity.X *= 0.8f;

                if (npc.ai[1] > 50f) {
                    npc.ai[1] -= 1;
                }
                else {
                    if (npc.ai[1] == 49f) {
                        npc.direction *= -1;
                        npc.spriteDirection = npc.direction;
                    }
                }
            }
            else {
                npc.ai[1] = 80;
                npc.ai[0] = -25f;
                npc.ai[2] = 0f;
                npc.frameCounter = 0.0;
                npc.netUpdate = true;

                int emoteType = ModContent.EmoteBubbleType<BackwoodsEmote>();
                EmoteBubble.NewBubble(emoteType, new WorldUIAnchor(npc), (int)npc.ai[1]);
            }
        }
    }

    private static bool AbleToBeDestroyed => NPC.downedBoss1 || NPC.downedBoss2 || NPC.downedBoss3;

    private sealed class DryadAwakeHandler : ModSystem {
        public static bool DryadAwake;

        public override void ClearWorld() {
            DryadAwake = false;
        }

        public override void SaveWorldData(TagCompound tag) {
            if (DryadAwake) {
                tag["DryadAwake"] = true;
            }
        }

        public override void LoadWorldData(TagCompound tag) {
            DryadAwake = tag.ContainsKey("DryadAwake");
        }

        public override void NetSend(BinaryWriter writer) {
            var flags = new BitsByte();
            flags[0] = DryadAwake;
            writer.Write(flags);
        }

        public override void NetReceive(BinaryReader reader) {
            BitsByte flags = reader.ReadByte();
            DryadAwake = flags[0];
        }
    }

    private sealed class ExtraDruidQuote : GlobalNPC {
        public override void GetChat(NPC npc, ref string chat) {
            if (npc.type != NPCID.Dryad) {
                return;
            }
            if (!DryadAwakeHandler.DryadAwake) {
                return;
            }

            chat = Language.GetTextValue($"Mods.RoA.NPC.Quotes.Dryad.AwakeQuote{Main.rand.NextBool().ToInt() + 1}");
            DryadAwakeHandler.DryadAwake = false;
        }
    }

    public override void Load() {
        On_Main.UpdateTime_SpawnTownNPCs += On_Main_UpdateTime_SpawnTownNPCs;
    }

    private void On_Main_UpdateTime_SpawnTownNPCs(On_Main.orig_UpdateTime_SpawnTownNPCs orig) {
        orig();
        Main.townNPCCanSpawn[NPCID.Dryad] = false;
    }

    public override void SetStaticDefaults() {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileLavaDeath[Type] = false;
        Main.tileLighted[Type] = true;
        Main.tileHammer[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
        TileObjectData.newTile.DrawYOffset = 2;
        TileObjectData.newTile.Width = 2;
        TileObjectData.newTile.Height = 3;
        TileObjectData.newTile.LavaDeath = false;
        TileObjectData.newTile.CoordinateHeights = [16, 16, 16];
        TileObjectData.newTile.CoordinateWidth = 16;
        TileObjectData.newTile.CoordinatePadding = 2;
        TileObjectData.addTile(Type);

        TileID.Sets.PreventsTileRemovalIfOnTopOfIt[Type] = true;
        TileID.Sets.PreventsSandfall[Type] = true;
        TileSets.ShouldKillTileBelow[Type] = false;
        TileSets.PreventsSlopesBelow[Type] = true;
        CanBeSlopedTileSystem.Included[Type] = true;

        AddMapEntry(new Color(191, 143, 111), CreateMapEntryName());

        DustType = DustID.WoodFurniture;
    }

    public override bool CreateDust(int i, int j, ref int type) {
        if (Main.rand.NextBool(5)) {
            type = DustID.Grass;
        }

        return base.CreateDust(i, j, ref type);
    }

    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) {
        Tile tile = Main.tile[i, j];
        Vector2 zero = new(Main.offScreenRange, Main.offScreenRange);
        if (Main.drawToScreen) {
            zero = Vector2.Zero;
        }
        bool flag = tile.TileFrameY == 0;
        int frameY = !flag ? tile.TileFrameY + 4 : 0; 
        int height = flag ? 22 : 16;
        int frameX = tile.TileFrameX;
        if (AbleToBeDestroyed) {
            frameX += 36;
        }
        Main.spriteBatch.Draw(TextureAssets.Tile[Type].Value,
                              new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y - (flag ? 4 : 0)) + zero,
                              new Rectangle(frameX, frameY, 16, height),
                              Lighting.GetColor(i, j), 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

        return false;
    }

    public override void KillMultiTile(int i, int j, int frameX, int frameY) {
        Vector2 position = new Point(i, j).ToWorldCoordinates();
        {
            int dustType = DustID.WoodFurniture;
            for (int k = 0; k < 20; k++) {
                Dust.NewDust(position, 36, 54, dustType, 2.5f * Main.rand.NextFloatDirection(), 2.5f * Main.rand.NextFloatDirection());
            }
            if (!Main.dedServ) {
                Vector2 offset = new(-12f, 4f);
                Gore.NewGore(NPC.GetSource_TownSpawn(), position + offset, Vector2.Zero, "TreeDryadGore1".GetGoreType());
                Gore.NewGore(NPC.GetSource_TownSpawn(), position + offset + new Vector2(8, 0), Vector2.Zero, "TreeDryadGore2".GetGoreType());
                Gore.NewGore(NPC.GetSource_TownSpawn(), position + offset + new Vector2(16, 0), Vector2.Zero, "TreeDryadGore3".GetGoreType());
            }
        }
        int whoAmI = NPC.NewNPC(NPC.GetSource_TownSpawn(), (int)position.X + 10, (int)position.Y + 40, NPCID.Dryad);
        Main.npc[whoAmI].direction = Main.npc[whoAmI].spriteDirection = Main.tile[i, j].TileFrameX < 72 ? -1 : 1;
        Main.npc[whoAmI].ai[0] = -20f;
        Main.npc[whoAmI].ai[1] = 150f;
        Main.npc[whoAmI].netUpdate = true;

        DryadAwakeHandler.DryadAwake = true;

        if (Main.netMode == NetmodeID.Server) {
            NetMessage.SendData(MessageID.WorldData);
        }
    }

    public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => false;

    public override bool CanExplode(int i, int j) => false;

    public override bool CanKillTile(int i, int j, ref bool blockDamaged) => AbleToBeDestroyed;

    public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 2 : 5;
}