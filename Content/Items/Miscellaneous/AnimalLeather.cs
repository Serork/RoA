using Microsoft.Xna.Framework;

using RoA.Content.Buffs;
using RoA.Content.Tiles.Crafting;
using RoA.Core.Utility;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Miscellaneous;

class AnimalLeather : ModItem {
    public override void SetStaticDefaults() {
        // DisplayName.SetDefault("Animal Leather");
        Item.ResearchUnlockCount = 10;

        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[ItemID.Leather] = 50;
    }

    public override bool OnPickup(Player player) {
        var handler = Item.GetGlobalItem<SpoilLeatherHandler>();
        if (handler.StartSpoilingTime == 0) {
            handler.StartSpoilingTime = handler.NeedToSpoilTime;
        }

        return base.OnPickup(player);
    }

    public override void SetDefaults() {
        int width = 26; int height = 24;
        Item.Size = new Vector2(width, height);

        Item.useAnimation = Item.useTime = 18;
        Item.useStyle = ItemUseStyleID.Swing;

        //Item.maxStack = Item.CommonMaxStack;
        Item.maxStack = 1;

        Item.value = Item.sellPrice(0, 0, 0, 25);
    }

    public override bool CanUseItem(Player player)
        => Main.tile[Player.tileTargetX, Player.tileTargetY].HasTile && Main.tile[Player.tileTargetX, Player.tileTargetY].TileType == (ushort)ModContent.TileType<TanningRack>()
        && player.WithinPlacementRange(Player.tileTargetX, Player.tileTargetY);
}