using RoA.Content.Tiles.Solid.Backwoods;
using RoA.Core.Utility;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Placeable.Seeds;

sealed class BackwoodsGrassSeeds : ModItem {
    public override void SetStaticDefaults() {
        ItemID.Sets.GrassSeeds[Type] = true;
    }

    public override void SetDefaults() {
        Item.autoReuse = true;
        Item.useTurn = true;
        Item.useStyle = 1;
        Item.useAnimation = 15;
        Item.useTime = 10;
        Item.maxStack = Item.CommonMaxStack;
        Item.consumable = true;
        Item.width = 14;
        Item.height = 14;
        Item.value = 150;
        //Item.createTile = ModContent.TileType<BackwoodsGrass>();
    }

    public override bool? UseItem(Player player) {
        if (Main.netMode == NetmodeID.Server) {
            return false;
        }

        Tile tile = Framing.GetTileSafely(Player.tileTargetX, Player.tileTargetY);
        if (tile.HasTile && tile.TileType == TileID.Dirt && player.WithinPlacementRange(Player.tileTargetX, Player.tileTargetY)) {
            WorldGen.PlaceTile(Player.tileTargetX, Player.tileTargetY, ModContent.TileType<BackwoodsGrass>(), forced: true);
            player.inventory[player.selectedItem].stack--;
        }

        return true;
    }
}