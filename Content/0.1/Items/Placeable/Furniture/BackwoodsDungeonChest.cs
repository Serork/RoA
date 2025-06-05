using RoA.Content.Items.Miscellaneous;

using Terraria;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Placeable.Furniture;

sealed class BackwoodsDungeonChest : ModItem {
    public override void Load() {
        On_Player.PlaceThing_LockChest += On_Player_PlaceThing_LockChest;
    }

    private void On_Player_PlaceThing_LockChest(On_Player.orig_PlaceThing_LockChest orig, Player self) {
        Tile tile = Main.tile[Player.tileTargetX, Player.tileTargetY];
        Item item = self.inventory[self.selectedItem];
        if (!tile.HasTile || item.type != 5328 || !TileID.Sets.IsAContainer[tile.TileType] ||
            !(self.position.X / 16f - (float)Player.tileRangeX - (float)item.tileBoost - (float)self.blockRange <= (float)Player.tileTargetX) ||
            !((self.position.X + (float)self.width) / 16f + (float)Player.tileRangeX + (float)item.tileBoost - 1f + (float)self.blockRange >= (float)Player.tileTargetX) ||
            !(self.position.Y / 16f - (float)Player.tileRangeY - (float)item.tileBoost - (float)self.blockRange <= (float)Player.tileTargetY) ||
            !((self.position.Y + (float)self.height) / 16f + (float)Player.tileRangeY + (float)item.tileBoost - 2f + (float)self.blockRange >= (float)Player.tileTargetY) ||
            !self.ItemTimeIsZero || self.itemAnimation <= 0 || !self.controlUseItem)
            return;

        Tile tileSafely = Framing.GetTileSafely(Player.tileTargetX, Player.tileTargetY);
        int type = tileSafely.TileType;
        int num = tileSafely.TileFrameX / 36;
        bool flag = false;
        if (type == ModContent.TileType<Tiles.Furniture.BackwoodsDungeonChest>()) {
            if (num == 0) {
                return;
            }
            else {
                flag = true;
            }
        }
        if (!flag) {
            switch (type) {
                case 21:
                    switch (num) {
                        default:
                            return;
                        case 1:
                        case 3:
                        case 18:
                        case 19:
                        case 20:
                        case 21:
                        case 22:
                        case 35:
                        case 37:
                        case 39:
                            break;
                    }
                    break;
                case 467:
                    if (num != 12)
                        return;
                    break;
            }
        }

        if (self.inventory[self.selectedItem].stack <= 0)
            return;


        int num2;
        for (num2 = Main.tile[Player.tileTargetX, Player.tileTargetY].TileFrameX / 18; num2 > 1; num2 -= 2) {
        }

        num2 = Player.tileTargetX - num2;
        int num3 = Player.tileTargetY - Main.tile[Player.tileTargetX, Player.tileTargetY].TileFrameY / 18;
        if (Chest.Lock(num2, num3)) {
            self.inventory[self.selectedItem].stack--;
            if (self.inventory[self.selectedItem].stack <= 0)
                self.inventory[self.selectedItem] = new Item();

            if (Main.netMode == 1)
                NetMessage.SendData(52, -1, -1, null, self.whoAmI, 3f, num2, num3);
        }
    }

    public override void SetStaticDefaults() {
        ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<BackwoodsDungeonKey>();
    }

    public override void SetDefaults() {
        Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.Furniture.BackwoodsDungeonChest>(), 1);
        Item.SetShopValues(ItemRarityColor.White0, Item.buyPrice(0, 0, 25));
        Item.maxStack = Item.CommonMaxStack;
        Item.width = 32;
        Item.height = 32;
    }
}