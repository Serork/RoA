using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Tiles;
using RoA.Common.WorldEvents;
using RoA.Core.Utility;

using System;
using System.Linq;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Solid.Backwoods;

sealed class BackwoodsGreenMoss : ModTile, IPostSetupContent {
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

    public override void SetStaticDefaults() {
        ushort stoneType = (ushort)ModContent.TileType<BackwoodsStone>();
        TileHelper.Solid(Type, false, false);
        TileHelper.MergeWith(Type, stoneType);

        Main.tileLighted[Type] = true;

        TileID.Sets.Grass[Type] = true;
        TileID.Sets.NeedsGrassFraming[Type] = true;
        TileID.Sets.NeedsGrassFramingDirt[Type] = stoneType;
        TileID.Sets.GeneralPlacementTiles[Type] = false;

        TransformTileSystem.OnKillActNormal[Type] = false;
        TransformTileSystem.ReplaceToTypeOnKill[Type] = stoneType;

        DustType = DustID.GreenMoss;
        HitSound = SoundID.Dig;
        AddMapEntry(new Color(49, 134, 114));
    }

    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) => SetupLight(ref r, ref g, ref b);

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
}