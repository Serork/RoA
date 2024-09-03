using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Tiles;
using RoA.Common.WorldEvents;
using RoA.Core;
using RoA.Core.Utility;

using System;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Tiles.Solid.Backwoods;

sealed class BackwoodsGreenMoss : ModTile {
    public override void SetStaticDefaults() {
        ushort stoneType = (ushort)ModContent.TileType<BackwoodsStone>();
        TileHelper.Solid(Type, false, false);
        TileHelper.MergeWith(Type, stoneType);

        Main.tileLighted[Type] = true;

        TileID.Sets.Grass[Type] = true;
        TileID.Sets.NeedsGrassFraming[Type] = true;
        TileID.Sets.NeedsGrassFramingDirt[Type] = stoneType;
        TileID.Sets.GeneralPlacementTiles[Type] = false;

        TransformTileSystem.OnKillNormal[Type] = false;
        TransformTileSystem.ReplaceToOnKill[Type] = stoneType;

        DustType = DustID.GreenMoss;
        HitSound = SoundID.Dig;
        AddMapEntry(new Color(49, 134, 114));
    }

    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) => SetupLight(ref r, ref g, ref b);

    public static void SetupLight(ref float r, ref float g, ref float b) {
        if (!BackwoodsFogHandler.IsFogActive) {
            return;
        }

        r = 49 / 255f;
        g = 134 / 255f;
        b = 114 / 255f;
    }
}