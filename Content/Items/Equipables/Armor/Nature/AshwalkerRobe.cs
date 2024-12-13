using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Druid;
using RoA.Common.Druid.Wreath;
using RoA.Common.GlowMasks;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Armor.Nature;

[AutoloadEquip(EquipType.Body)]
sealed class AshwalkerRobe : NatureItem, ItemGlowMaskHandler.ISetGlowMask {
	public override void SetStaticDefaults() {
		//DisplayName.SetDefault("Ashwalker Robe");
		//Tooltip.SetDefault("10% increased nature base damage");
		CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;

        ItemGlowMaskHandler.RegisterArmorGlowMask(Item.bodySlot, this);
    }

    protected override void SafeSetDefaults() {
        int width = 26; int height = 20;
		Item.Size = new Vector2(width, height);

		Item.rare = ItemRarityID.Orange;
		Item.value = Item.sellPrice(silver: 80);

		Item.defense = 6;
	}

	public override void UpdateEquip(Player player) => player.GetModPlayer<DruidStats>().DruidBaseDamageMultiplier += 0.1f;

    public void SetDrawSettings(Player player, ref Texture2D texture, ref Color color) {
        color = Color.White * player.GetModPlayer<WreathHandler>().ActualProgress5;
    }

    //public override void AddRecipes() {
    //	CreateRecipe()
    //		.AddIngredient<FlamingFabric>(25)
    //		.AddTile(TileID.Loom)
    //		.Register();
    //}
}