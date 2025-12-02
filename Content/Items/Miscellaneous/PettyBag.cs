using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Miscellaneous;

sealed class PettyBag : ModItem {
    public override void SetStaticDefaults() {
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
    }

    public override void SetDefaults() {
        Item.useStyle = ItemUseStyleID.Swing;
        Item.shootSpeed = 4f;
        Item.shoot = ModContent.ProjectileType<Projectiles.Friendly.Miscellaneous.PettyBag>();
        Item.width = 22;
        Item.height = 24;
        Item.UseSound = SoundID.Item7;
        Item.useAnimation = Item.useTime = 28;
        Item.noUseGraphic = true;

        Item.rare = ItemRarityID.Orange;
        Item.value = Item.sellPrice(0, 3, 0, 0);
    }

    public override bool CanUseItem(Player player) {
        if (player.ownedProjectileCounts[Item.shoot] > 0 &&
            player.GetModPlayer<Projectiles.Friendly.Miscellaneous.PettyBag.PettyBagHandler>().BagItems.Count != 0) {
            return false;
        }

        return base.CanUseItem(player);
    }
}
