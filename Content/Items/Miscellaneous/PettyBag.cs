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
        Item.useStyle = 1;
        Item.shootSpeed = 4f;
        //Item.shoot = 525;
        Item.width = 22;
        Item.height = 24;
        Item.UseSound = SoundID.Item59;
        Item.useAnimation = 28;
        Item.useTime = 28;
        Item.rare = 3;
        Item.value = Item.sellPrice(0, 2);
    }
}
