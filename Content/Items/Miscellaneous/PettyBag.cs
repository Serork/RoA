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
        //Item.UseSound = SoundID.Item59;
        Item.useAnimation = 28;
        Item.useTime = 28;
        Item.rare = ItemRarityID.Orange;
        Item.value = Item.sellPrice(0, 2);
    }
}
