using Microsoft.Xna.Framework;

using RoA.Common.Druid;
using RoA.Common.Druid.Forms;
using RoA.Common.Players;
using RoA.Content.Forms;
using RoA.Core.Utility;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Armor.Nature;

[AutoloadEquip(EquipType.Head)]
sealed class LivingElderwoodCrown : NatureItem, IDoubleTap, IPostSetupContent {
	public override void SetStaticDefaults() {
		//DisplayName.SetDefault("Living Elderwood Crown");
		//Tooltip.SetDefault("6% increased nature potential damage");
		ArmorIDs.Head.Sets.DrawFullHair[Item.headSlot] = true;
		CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
	}

    protected override void SafeSetDefaults() {
        int width = 26; int height = 20;
		Item.Size = new Vector2(width, height);

		Item.rare = ItemRarityID.Green;
		Item.value = Item.sellPrice(silver: 25);

		Item.defense = 1;
	}

	public override void UpdateEquip(Player player) {
		player.GetModPlayer<DruidStats>().DruidPotentialDamageMultiplier += 0.06f;
	}

	public override bool IsArmorSet(Item head, Item body, Item legs) => body.type == ModContent.ItemType<LivingElderwoodBreastplate>() && legs.type == ModContent.ItemType<LivingElderwoodGreaves>();

	public override void UpdateArmorSet(Player player) {
		string tapDir = Language.GetTextValue(Main.ReversedUpDownArmorSetBonuses ? "Key.UP" : "Key.DOWN");
		player.setBonus = "Increases Wreath filling rate by 10%" + 
			$"\nDouble tap {tapDir} to take Fleder form";
        player.GetModPlayer<DruidStats>().DruidDamageExtraIncreaseValueMultiplier += 0.1f;

		BaseFormHandler.KeepFormActive(player);
    }

    void IDoubleTap.OnDoubleTap(Player player, IDoubleTap.TapDirection direction) {
		if (player.HasSetBonusFrom<LivingElderwoodCrown>() && direction == IDoubleTap.TapDirection.Down) {
			BaseFormHandler.ToggleForm<FlederForm>(player);
		}
    }

    void IPostSetupContent.PostSetupContent() {
        BaseFormHandler.RegisterForm<FlederForm>();
    }

    //public override void AddRecipes() {
    //	CreateRecipe()
    //		.AddIngredient<Elderwood>(10)
    //		.AddIngredient<Galipot>(5)
    //		.AddTile(TileID.LivingLoom)
    //		.Register();
    //}
}