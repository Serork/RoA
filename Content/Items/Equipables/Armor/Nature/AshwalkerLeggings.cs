using Microsoft.Xna.Framework;

using RoA.Common.Druid;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Armor.Nature;

[AutoloadEquip(EquipType.Legs)]
sealed class AshwalkerLeggings : NatureItem {
	public override void SetStaticDefaults() {
		//DisplayName.SetDefault("Ashwalker Leggings");
		//Tooltip.SetDefault("6% increased nature potential damage");
		CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
	}

    protected override void SafeSetDefaults() {
        int width = 22; int height = 18;
		Item.Size = new Vector2(width, height);

		Item.rare = ItemRarityID.Orange;
		Item.value = Item.sellPrice(silver: 70);

		Item.defense = 4;
	}

	public override void UpdateEquip(Player player) => player.GetModPlayer<DruidStats>().DruidPotentialDamageMultiplier += 0.06f;

    //public override void AddRecipes() {
    //	CreateRecipe()
    //		.AddIngredient<FlamingFabric>(20)
    //		.AddTile(TileID.Loom)
    //		.Register();
    //}
}