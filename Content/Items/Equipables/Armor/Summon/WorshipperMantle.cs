using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Armor.Summon;

[AutoloadEquip(EquipType.Body)]
sealed class WorshipperMantle : ModItem {
    public override void SetStaticDefaults() {
        // DisplayName.SetDefault("Worshipper Mantle");
        // Tooltip.SetDefault("18% increased minion damage");

        Item.ResearchUnlockCount = 1;
    }

    public override void SetDefaults() {
        int width = 30; int height = 22;
        Item.Size = new Vector2(width, height);

        Item.value = Item.sellPrice(silver: 60);
        Item.rare = ItemRarityID.Orange;

        Item.defense = 6;
    }

    public override void UpdateEquip(Player player) => player.GetDamage(DamageClass.Summon) += 0.18f;
	
	 public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.Leather, 5)
			.AddIngredient(ItemID.Bone, 50)
            .AddTile(TileID.WorkBenches)
            .Register();
    }
}