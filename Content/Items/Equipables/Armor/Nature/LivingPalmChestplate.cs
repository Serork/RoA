using Microsoft.Xna.Framework;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Armor.Nature;

[AutoloadEquip(EquipType.Body)]
sealed class LivingPalmChestplate : NatureItem {
	public override void SetStaticDefaults() {
		//DisplayName.SetDefault("Living Boreal Wood Chestplate");
		//Tooltip.SetDefault("4% increased nature potential damage");
		CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
	}

    protected override void SafeSetDefaults() {
        int width = 30; int height = 22;
        Item.Size = new Vector2(width, height);

        Item.rare = ItemRarityID.Blue;
        Item.value = Item.sellPrice(silver: 15);

        Item.defense = 2;
    }

    public override void AddRecipes() {
    	CreateRecipe()
    		.AddIngredient(ItemID.PalmWood, 20)
    		.AddIngredient<Materials.Galipot>(5)
    		.AddTile(TileID.LivingLoom)
    		.Register();
    }
}