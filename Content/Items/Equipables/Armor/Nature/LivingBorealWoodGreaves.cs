using Microsoft.Xna.Framework;

using RoA.Common.Druid;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Armor.Nature;

[AutoloadEquip(EquipType.Legs)]
sealed class LivingBorealWoodGreaves : NatureItem {
	public override void SetStaticDefaults() {
		//DisplayName.SetDefault("Living Boreal Wood Greaves");
		CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
	}

    protected override void SafeSetDefaults() {
        int width = 22; int height = 18;
        Item.Size = new Vector2(width, height);

        Item.rare = ItemRarityID.Blue;
        Item.value = Item.sellPrice(silver: 10);

        Item.defense = 2;
    }

    public override void UpdateEquip(Player player) {
        player.maxRunSpeed += 0.02f;
        player.GetModPlayer<DruidStats>().DruidPotentialUseTimeMultiplier -= 0.04f;
    }

    //public override void AddRecipes() {
    //	CreateRecipe()
    //		.AddIngredient(ItemID.BorealWood, 15)
    //		.AddIngredient<Galipot>(2)
    //		.AddTile(TileID.LivingLoom)
    //		.Register();
    //}
}