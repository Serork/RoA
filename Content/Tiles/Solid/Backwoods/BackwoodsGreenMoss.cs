using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common.Tiles;
﻿using RoA.Common.World;
using RoA.Content.Dusts;
using RoA.Content.Items.Placeable;
using RoA.Content.Items.Placeable.Miscellaneous;
using RoA.Content.Tiles.Ambient;
using RoA.Core.Utility;

using System;
using System.Linq;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Solid.Backwoods;

sealed class BackwoodsGreenMoss : ModTile, IPostSetupContent {
    private static Asset<Texture2D> _glowTexture = null!;

    private class TealMossPlacementOnGrimstone : GlobalItem {
        public override bool? UseItem(Item item, Player player) {
            if (item.type == ModContent.ItemType<TealMoss>()) {
                if (Main.netMode != NetmodeID.Server && player.ItemAnimationJustStarted) {
                    Tile tile = Framing.GetTileSafely(Player.tileTargetX, Player.tileTargetY);
                    if (tile.TileType == 1 || tile.TileType == 38) {
                        return false;
                    }
                    bool flag = tile.TileType == ModContent.TileType<BackwoodsStoneBrick>();
                    if (tile.HasTile &&
                        (tile.TileType == ModContent.TileType<BackwoodsStone>() || flag)
                        && player.WithinPlacementRange(Player.tileTargetX, Player.tileTargetY)) {
                        tile.TileType = flag ? (ushort)ModContent.TileType<BackwoodsGreenMossBrick>() : (ushort)ModContent.TileType<BackwoodsGreenMoss>();
                        WorldGen.TileFrame(Player.tileTargetX, Player.tileTargetY);
                        SoundEngine.PlaySound(SoundID.Dig, new Point(Player.tileTargetX, Player.tileTargetY).ToWorldCoordinates());
                        NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 1, Player.tileTargetX, Player.tileTargetY, tile.TileType, 0);
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

            if (objData.AnchorValidTiles.Any(tileId => tileId == TileID.GreenMoss)) {
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

        On_WorldGen.SpreadGrass += On_WorldGen_SpreadGrass;
        On_WorldGen.PlaceTile += On_WorldGen_PlaceTile;
    }

    private bool On_WorldGen_PlaceTile(On_WorldGen.orig_PlaceTile orig, int i, int j, int Type, bool mute, bool forced, int plr, int style) {
        Tile tile = WorldGenHelper.GetTileSafely(i, j);
        if ((tile.TileType == 1 || tile.TileType == 38) && Type == ModContent.TileType<BackwoodsGreenMoss>()) {
            return false;
        }

        if (Type == 184) {
            for (int x = -1; x < 2; x++) {
                for (int y = -1; y < 2; y++) {
                    if (x != 0 && y != 0) {
                        int moss = ModContent.TileType<BackwoodsGreenMoss>();
                        int moss2 = ModContent.TileType<BackwoodsGreenMossBrick>();
                        if (WorldGenHelper.ActiveTile(i + x, j + y, moss) || WorldGenHelper.ActiveTile(i + x, j + y, moss2)) {
                            return false;
                        }
                    }
                }
            }
        }

        return orig(i, j, Type, mute, forced, plr, style);
    }

    private static void On_WorldGen_SpreadGrass(On_WorldGen.orig_SpreadGrass orig, int i, int j, int dirt, int grass, bool repeat, TileColorCache color) {
        if ((grass == ModContent.TileType<BackwoodsGreenMoss>() || grass == ModContent.TileType<BackwoodsGreenMossBrick>()) && (dirt == 1 || dirt == 38)) {
            return;
        }

        orig(i, j, dirt, grass, repeat, color);
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
            int type = ModContent.ItemType<TealMoss>();
            int number = Item.NewItem(new EntitySource_ItemUse(self, self.HeldItem), x * 16, y * 16, 16, 16, type);
            NetMessage.SendData(21, -1, -1, null, number, 1f);
        }
    }

    public override void SetStaticDefaults() {
        if (!Main.dedServ) {
            _glowTexture = ModContent.Request<Texture2D>(Texture + "_Glow");
        }

        ushort stoneType = (ushort)ModContent.TileType<BackwoodsStone>();
        TileHelper.Solid(Type, blendAll: true);
        TileHelper.MergeWith(Type, stoneType);

        Main.tileLighted[Type] = true;
        Main.tileMoss[Type] = true;

        TileID.Sets.Conversion.Stone[Type] = true;
        TileID.Sets.Conversion.Moss[Type] = true;

        TileID.Sets.Grass[Type] = true;
        TileID.Sets.NeedsGrassFraming[Type] = true;
        TileID.Sets.NeedsGrassFramingDirt[Type] = stoneType;
        TileID.Sets.GeneralPlacementTiles[Type] = false;

        TileID.Sets.ResetsHalfBrickPlacementAttempt[Type] = true;

        TransformTileSystem.ReplaceToTypeOnKill[Type] = stoneType;

        DustType = ModContent.DustType<TealMossDust>();
        HitSound = SoundID.Dig;
        AddMapEntry(new Color(49, 134, 114));
    }

    public override bool CanDrop(int i, int j) => false;

    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) => SetupLight(ref r, ref g, ref b);

    public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) {
        //bool flag = true;
        //for (int x = -1; x < 2; x++) {
        //    for (int y = -1; y < 2; y++) {
        //        if (x != 0 && y != 0) {
        //            if (!WorldGen.SolidTile(i + x, j + y)) {
        //                flag = false;
        //            }
        //        }
        //    }
        //}
        //if (flag) {
        //    Tile tile = WorldGenHelper.GetTileSafely(i, j);
        //    tile.TileFrameX = 144;
        //    tile.TileFrameY = 198;
        //    return false;
        //}

        return base.TileFrame(i, j, ref resetFrame, ref noBreak);
    }

    public static void SetupLight(ref float r, ref float g, ref float b) {
        float value = BackwoodsFogHandler.Opacity;
        r = 49 / 255f * value;
        g = 134 / 255f * value;
        b = 114 / 255f * value;
    }

    public override void PostDraw(int i, int j, SpriteBatch spriteBatch) {
        if (!TileDrawing.IsVisible(Main.tile[i, j])) {
            return;
        }

        Tile tile = Main.tile[i, j];
        Vector2 zero = new(Main.offScreenRange, Main.offScreenRange);
        if (Main.drawToScreen) {
            zero = Vector2.Zero;
        }
        int height = tile.TileFrameY == 36 ? 18 : 16;
        Main.spriteBatch.Draw(_glowTexture.Value,
                              new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y + 2) + zero,
                              new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, height),
                              TileDrawingExtra.BackwoodsMossGlowColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
    }

    public override void RandomUpdate(int i, int j) {
        Tile me = Main.tile[i, j];
        Tile left = Main.tile[i + 1, j];
        Tile right = Main.tile[i - 1, j];
        Tile down = Main.tile[i, j + 1];
        Tile up = Main.tile[i, j - 1];
        int type = ModContent.TileType<MossGrowth>();
        if (Main.rand.NextBool(6) && me.HasUnactuatedTile && !me.IsHalfBlock && !me.BottomSlope && !me.LeftSlope && !me.RightSlope && !me.TopSlope) {
            short framing = (short)(Main.rand.Next(3) * 18);
            if (!left.HasTile) {
                WorldGen.PlaceTile(i + 1, j, type, mute: true);
                left.TileFrameY = (short)(108 + framing);
                left.CopyPaintAndCoating(me);
            }
            if (!right.HasTile) {
                WorldGen.PlaceTile(i - 1, j, type, mute: true);
                right.TileFrameY = (short)(162 + framing);
                right.CopyPaintAndCoating(me);
            }
            if (!down.HasTile) {
                WorldGen.PlaceTile(i, j + 1, type, mute: true);
                down.TileFrameY = (short)(54 + framing);
                down.CopyPaintAndCoating(me);
            }
            if (!up.HasTile) {
                WorldGen.PlaceTile(i, j - 1, type, mute: true);
                up.TileFrameY = framing;
                up.CopyPaintAndCoating(me);
            }
        }

        if (Main.tile[i, j].HasUnactuatedTile && (j > Main.worldSurface - 1 || Main.rand.NextBool(2))) {
            int num = i - 1;
            int num2 = i + 2;
            int num3 = j - 1;
            int num4 = j + 2;
            int type2 = Main.tile[i, j].TileType;
            bool flag9 = false;
            TileColorCache color = Main.tile[i, j].BlockColorAndCoating();
            for (int num28 = num; num28 < num2; num28++) {
                for (int num29 = num3; num29 < num4; num29++) {
                    bool flag = Main.tile[num28, num29].TileType == ModContent.TileType<BackwoodsStoneBrick>();
                    if ((i != num28 || j != num29) && Main.tile[num28, num29].HasTile && (
                        //Main.tile[num28, num29].TileType == 1 ||
                        Main.tile[num28, num29].TileType == ModContent.TileType<BackwoodsStone>() ||
                        flag
                        /*|| Main.tile[num28, num29].TileType == 38*/)) {
                        int type3 = Main.tile[num28, num29].TileType;
                        int num30 = !flag ? Type : ModContent.TileType<BackwoodsGreenMossBrick>();
                        WorldGen.SpreadGrass(num28, num29, Main.tile[num28, num29].TileType, num30, repeat: false, color);
                        if (Main.tile[num28, num29].TileType == num30) {
                            WorldGen.SquareTileFrame(num28, num29);
                            flag9 = true;
                        }
                    }
                }
            }

            if (Main.netMode == 2 && flag9)
                NetMessage.SendTileSquare(-1, i, j, 3);
        }
    }
}