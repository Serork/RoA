using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Miscellaneous;

[AutoloadEquip(EquipType.Head)]
sealed class DynastyWoodHelmet : ModItem {
    public override void SetStaticDefaults() {
        // DisplayName.SetDefault("Dynasty Wood Helmet");
        Item.ResearchUnlockCount = 1;
    }

    public override void SetDefaults() {
        int width = 24; int height = 20;
        Item.Size = new Vector2(width, height);

        Item.rare = ItemRarityID.White;
        Item.value = Item.sellPrice(silver: 10);

        Item.defense = 2;
    }

    public override bool IsArmorSet(Item head, Item body, Item legs)
        => body.type == ModContent.ItemType<DynastyWoodBreastplate>() && legs.type == ModContent.ItemType<DynastyWoodLeggings>();

    public override void UpdateArmorSet(Player player) {
        player.statDefense += 1;

        player.setBonus = Language.GetText("Mods.RoA.Items.Tooltips.DynastySetBonus").Value;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.DynastyWood, 20)
            .AddTile(TileID.WorkBenches)
            .Register();
    }
}