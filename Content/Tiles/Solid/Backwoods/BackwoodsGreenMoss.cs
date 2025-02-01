using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Tiles;
using RoA.Common.WorldEvents;
using RoA.Content.Tiles.Ambient;
using RoA.Core.Utility;

using System;
using System.Linq;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Solid.Backwoods;

sealed class BackwoodsGreenMoss : ModTile, IPostSetupContent {
    private sealed class GreenMossPlacementOnGrimstone : GlobalItem {
        public override bool? UseItem(Item item, Player player) {
            if (item.type == ItemID.GreenMoss) {
                if (Main.netMode != NetmodeID.Server && player.ItemAnimationJustStarted) {
                    Tile tile = Framing.GetTileSafely(Player.tileTargetX, Player.tileTargetY);
                    if (tile.HasTile && tile.TileType == ModContent.TileType<BackwoodsStone>() && player.WithinPlacementRange(Player.tileTargetX, Player.tileTargetY)) {
                        tile.TileType = (ushort)ModContent.TileType<BackwoodsGreenMoss>();
                        WorldGen.TileFrame(Player.tileTargetX, Player.tileTargetY);
                        SoundEngine.PlaySound(SoundID.Dig, new Point(Player.tileTargetX, Player.tileTargetY).ToWorldCoordinates());
                        //WorldGen.PlaceTile(Player.tileTargetX, Player.tileTargetY, ModContent.TileType<BackwoodsGreenMoss>(), forced: true);
                        return true;
                    }
                }
            }

            return base.UseItem(item, player);
        }
    }

    void IPostSetupContent.PostSetupContent() {
        for (int i = 0; i < TileLoader.TileCount; i++) {
            TileObjectData objData = TileObjectData.GetTileData(i, 0);
            if (objData == null || objData.AnchorValidTiles == null || objData.AnchorValidTiles.Length == 0) {
                continue;
            }

            if (objData.AnchorValidTiles.Any(tileId => tileId == TileID.RainbowMoss)) {
                lock (objData) {
                    int[] anchorAlternates = objData.AnchorValidTiles;
                    Array.Resize(ref anchorAlternates, anchorAlternates.Length + 1);
                    anchorAlternates[^1] = ModContent.TileType<BackwoodsGreenMoss>();
                    objData.AnchorValidTiles = anchorAlternates;
                }
            }
        }
    }

    public override void Load() {
        On_Player.PlaceThing_PaintScrapper_LongMoss += On_Player_PlaceThing_PaintScrapper_LongMoss;
    }

    private void On_Player_PlaceThing_PaintScrapper_LongMoss(On_Player.orig_PlaceThing_PaintScrapper_LongMoss orig, Player self, int x, int y) {
        orig(self, x, y);

        if (Main.tile[x, y].TileType != ModContent.TileType<MossGrowth>())
            return;

        self.cursorItemIconEnabled = true;
        if (!self.ItemTimeIsZero || self.itemAnimation <= 0 || !self.controlUseItem)
            return;

        _ = Main.tile[x, y].TileType;
        WorldGen.KillTile(x, y);
        if (Main.tile[x, y].HasTile)
            return;

        self.ApplyItemTime(self.inventory[self.selectedItem]);
        if (Main.netMode == 1)
            NetMessage.SendData(17, -1, -1, null, 0, x, y);

        if (Main.rand.Next(9) == 0) {
            int type = 4349;
            int number = Item.NewItem(new EntitySource_ItemUse(self, self.HeldItem), x * 16, y * 16, 16, 16, type);
            NetMessage.SendData(21, -1, -1, null, number, 1f);
        }
    }

    public override void SetStaticDefaults() {
        ushort stoneType = (ushort)ModContent.TileType<BackwoodsStone>();
        TileHelper.Solid(Type, blendAll: false);
        TileHelper.MergeWith(Type, stoneType);

        Main.tileLighted[Type] = true;
        Main.tileMoss[Type] = true;

        TileID.Sets.Conversion.Moss[Type] = true;

        TileID.Sets.Grass[Type] = true;
        TileID.Sets.NeedsGrassFraming[Type] = true;
        TileID.Sets.NeedsGrassFramingDirt[Type] = stoneType;
        TileID.Sets.GeneralPlacementTiles[Type] = false;

        TileID.Sets.ResetsHalfBrickPlacementAttempt[Type] = true;

        TransformTileSystem.OnKillActNormal[Type] = false;
        TransformTileSystem.ReplaceToTypeOnKill[Type] = stoneType;

        DustType = DustID.GreenMoss;
        HitSound = SoundID.Dig;
        AddMapEntry(new Color(49, 134, 114));
    }

    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) => SetupLight(ref r, ref g, ref b);

    public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) {
        bool flag = true;
        for (int x = -1; x < 2; x++) {
            for (int y = -1; y < 2; y++) {
                if (x != 0 && y != 0) {
                    if (!WorldGenHelper.GetTileSafely(i + x, j + y).HasTile) {
                        flag = false;
                    }
                }
            }
        }
        if (flag) {
            Tile tile = WorldGenHelper.GetTileSafely(i, j);
            tile.TileFrameX = 144;
            tile.TileFrameY = 198;
            return false;
        }

        return base.TileFrame(i, j, ref resetFrame, ref noBreak);
    }

    public static void SetupLight(ref float r, ref float g, ref float b) {
        float value = BackwoodsFogHandler.Opacity;
        r = 49 / 255f * value;
        g = 134 / 255f * value;
        b = 114 / 255f * value;
    }

    public override void PostDraw(int i, int j, SpriteBatch spriteBatch) {
        Tile tile = Main.tile[i, j];
        Vector2 zero = new(Main.offScreenRange, Main.offScreenRange);
        if (Main.drawToScreen) {
            zero = Vector2.Zero;
        }
        int height = tile.TileFrameY == 36 ? 18 : 16;
        Main.spriteBatch.Draw(this.GetTileGlowTexture(),
                              new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y + 2) + zero,
                              new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, height),
                              TileDrawingExtra.BackwoodsMossGlowColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
    }

    public override void RandomUpdate(int i, int j) {
        if (Main.tile[i, j].HasUnactuatedTile && Main.tile[i, j].Slope == 0 && !Main.tile[i, j].IsHalfBlock && WorldGen.genRand.NextBool(2)) {
            if (WorldGen.genRand.Next(6) == 0) {
                int num20 = i;
                int num21 = j;
                switch (WorldGen.genRand.Next(4)) {
                    case 0:
                        num20--;
                        break;
                    case 1:
                        num20++;
                        break;
                    case 2:
                        num21--;
                        break;
                    default:
                        num21++;
                        break;
                }

                if (!Main.tile[num20, num21].HasTile) {
                    if (WorldGen.PlaceTile(num20, num21, ModContent.TileType<MossGrowth>(), mute: true))
                        Main.tile[num20, num21].CopyPaintAndCoating(Main.tile[i, j]);

                    if (Main.netMode == NetmodeID.Server && Main.tile[num20, num21].HasTile)
                        NetMessage.SendTileSquare(-1, num20, num21);
                }
            }
        }
    }
}