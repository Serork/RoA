using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Tiles;
using RoA.Content.Dusts;
using RoA.Content.Items.Placeable.Seeds;
using RoA.Content.Tiles.Solid.Backwoods;
using RoA.Core.Utility;

using System;
using System.Linq;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Plants;

sealed class MiracleMint : PlantBase, TileHooks.IGetTileDrawData {
    protected override void SafeSetStaticDefaults() {
        Main.tileLighted[Type] = true;

        AddMapEntry(new Color(102, 243, 205), CreateMapEntryName());

        HitSound = SoundID.Grass;

        DropItem = (ushort)ModContent.ItemType<Items.Materials.MiracleMint>();
    }

    protected override int PlantDrop => DropItem;

    protected override int SeedsDrop => (ushort)ModContent.ItemType<MiracleMintSeeds>();

    protected override void PreAddNewTile() {
        TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<MiracleMintTE>().Hook_AfterPlacement, -1, 0, false);
    }

    protected override int[] AnchorValidTiles => [ModContent.TileType<BackwoodsGrass>()];

    public override void PlaceInWorld(int i, int j, Item item) => ModContent.GetInstance<MiracleMintTE>().Place(i, j);
    public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem) => ModContent.GetInstance<MiracleMintTE>().Kill(i, j);

    public override bool CreateDust(int i, int j, ref int type) {
        if (IsGrown(i, j) && Main.rand.NextBool(3)) {
            type = ModContent.DustType<MiracleMintDust>();
        }
        else {
            type = ModContent.DustType<Dusts.Backwoods.Grass>();
        }

        return true;
    }

    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
        float counting = TileHelper.GetTE<MiracleMintTE>(i, j).Counting;
        float factor = Math.Max(0.1f, (double)counting < 1.0 ? 1f - (float)Math.Pow(2.0, -10.0 * (double)counting) : 1f);
        float lightValue = (factor > 0.5f ? 1f - factor : factor) + 0.5f;
        lightValue *= 0.85f;
        if (IsGrown(i, j)) {
            r = 0.3f * lightValue;
            g = 0.6f * lightValue;
            b = 1.2f * lightValue;

            if (Main.rand.NextBool(50)) {
                Dust.NewDust(new Vector2(i * 16, j * 16), 16, 16, ModContent.DustType<MiracleMintDust>());
            }
        }
    }

    public void GetTileDrawData(TileDrawing self, int x, int y, Tile tileCache, ushort typeCache, ref short tileFrameX, ref short tileFrameY, ref int tileWidth, ref int tileHeight, ref int tileTop, ref int halfBrickHeight, ref int addFrX, ref int addFrY, ref SpriteEffects tileSpriteEffect, ref Texture2D glowTexture, ref Rectangle glowSourceRect, ref Color glowColor) {
        if (IsGrown(x, y) && !AnchorValidTiles.Contains(WorldGenHelper.GetTileSafely(x, y + 1).TileType)) {
            tileHeight += 2;
            addFrY -= 2;
        }
    }
}
