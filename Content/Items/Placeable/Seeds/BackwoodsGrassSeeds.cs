using RoA.Content.Tiles.Solid.Backwoods;
using RoA.Core.Utility;

using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Placeable.Seeds;

sealed class BackwoodsGrassSeeds : ModItem {
    public override void SetStaticDefaults() {
        //ItemTrader.ChlorophyteExtractinator.AddOption_OneWay(Type, 1, ItemID.DirtBlock, 1);

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
        if (Main.netMode != NetmodeID.Server && player.ItemAnimationJustStarted) {
            Tile tile = Framing.GetTileSafely(Player.tileTargetX, Player.tileTargetY);
            if (tile.HasTile && tile.TileType == TileID.Dirt && player.WithinPlacementRange(Player.tileTargetX, Player.tileTargetY)) {
                WorldGen.PlaceTile(Player.tileTargetX, Player.tileTargetY, ModContent.TileType<BackwoodsGrass>(), forced: true);
                NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 1, Player.tileTargetX, Player.tileTargetY, ModContent.TileType<BackwoodsGrass>(), 0);

                return true;
            }
        }

        return base.UseItem(player);
    }
}