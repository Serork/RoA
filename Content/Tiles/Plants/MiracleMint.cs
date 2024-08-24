using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.ObjectData;

using RoA.Content.Tiles.Solid.Backwoods;
using RoA.Core.Utility;
using RoA.Common;

using System;
using System.Collections.Generic;
using RoA.Common.Tiles;

namespace RoA.Content.Tiles.Plants;

sealed class MiracleMint : Plant {
    protected override void SafeStaticDefaults() {
        Main.tileLighted[Type] = true;

        AddMapEntry(new Color(102, 243, 205), CreateMapEntryName());

        HitSound = SoundID.Grass;

        TileObjectData.newTile.CopyFrom(TileObjectData.StyleAlch);
        TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<MiracleMintTE>().Hook_AfterPlacement, -1, 0, false);
        TileObjectData.addTile(Type);
    }

    public override IEnumerable<Item> GetItemDrops(int i, int j) {
        if (IsGrown(i, j)) {
            yield return new Item(ModContent.ItemType<Items.Materials.MiracleMint>());
        }
    }

    protected override int[] AnchorValidTiles => [ModContent.TileType<BackwoodsGrass>()];

    public override void PlaceInWorld(int i, int j, Item item) => ModContent.GetInstance<MiracleMintTE>().Place(i, j);
    public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem) => ModContent.GetInstance<MiracleMintTE>().Kill(i, j);

    public override void NumDust(int i, int j, bool fail, ref int num) => num = IsGrown(i, j) ? Main.rand.Next(3, 6) : IsGrowing(i, j) ? Main.rand.Next(2, 5) : Main.rand.Next(1, 3);

    public override bool CreateDust(int i, int j, ref int type) {
        if (IsGrown(i, j)) {
            type = DustID.BlueTorch;
        }
        else {
            type = DustID.Grass;
        }

        return true;
    }

    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
        float counting = TileHelper.GetTE<MiracleMintTE>(i, j).Counting;
        float factor = Math.Max(0.1f, (double)counting < 1.0 ? 1f - (float)Math.Pow(2.0, -10.0 * (double)counting) : 1f);
        float lightValue = (factor > 0.5f ? 1f - factor : factor) + 0.5f;
        if (IsGrown(i, j)) {
            r = 0.3f * lightValue;
            g = 0.6f * lightValue;
            b = 1.2f * lightValue;
        }
    }
}
