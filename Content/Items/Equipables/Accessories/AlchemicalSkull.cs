using Microsoft.Xna.Framework;

using RoA.Content.Buffs;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Accessories;


[AutoloadEquip(EquipType.Face)]
sealed class AlchemicalSkull : ModItem {
	public override void SetStaticDefaults() {
		//DisplayName.SetDefault("Alchemical Skull");
		//Tooltip.SetDefault("Grants immunity to fire blocks, poison and toxic fumes");
		ArmorIDs.Face.Sets.DrawInFaceHeadLayer[Item.faceSlot] = true;
		CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
	}

	public override void SetDefaults() {
		int width = 22; int height = 28;
		Item.Size = new Vector2(width, height);

		Item.value = Item.sellPrice(gold: 1, silver: 60);
		Item.rare = ItemRarityID.Orange;
		Item.accessory = true;
		Item.defense = 1;
	}

	public override void UpdateAccessory(Player player, bool hideVisual) {
		player.fireWalk = true;
		player.buffImmune[BuffID.Poisoned] = true;
		player.buffImmune[ModContent.BuffType<ToxicFumes>()] = true;
        player.buffImmune[ModContent.BuffType<ToxicFumesNoTimeDisplay>()] = true;
    }

	public override void AddRecipes() {
		CreateRecipe()
			.AddIngredient(ItemID.ObsidianSkull)
			.AddIngredient(ItemID.Bezoar)
			.AddIngredient<Materials.NaturesHeart>(1)
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
	}
}