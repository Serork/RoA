using Microsoft.Xna.Framework;

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
sealed class DreadheartCorruptionHelmet : NatureItem, IDoubleTap, IPostSetupContent {
	public override void SetStaticDefaults() {
		//DisplayName.SetDefault("Dreadheart Helmet");
		//Tooltip.SetDefault("4% increased nature critical strike chance");
		CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
	}

    protected override void SafeSetDefaults() {
        int width = 26; int height = 20;
		Item.Size = new Vector2(width, height);

		Item.rare = ItemRarityID.Green;
		Item.value = Item.sellPrice(silver: 75);

		Item.defense = 3;
	}

    public override void UpdateEquip(Player player) => player.GetCritChance(DruidClass.NatureDamage) += 4;

    public override bool IsArmorSet(Item head, Item body, Item legs) => body.type == ModContent.ItemType<DreadheartCorruptionChestplate>() && legs.type == ModContent.ItemType<DreadheartCorruptionLeggings>();

	//public override void DrawArmorColor(Player drawPlayer, float shadow, ref Color color, ref int glowMask, ref Color glowMaskColor) {
	//	if (drawPlayer.active && drawPlayer.GetModPlayer<WreathPlayer>().isCharged) {
	//		glowMask = RoAGlowMask.Get(nameof(DreadheartCorruptionHelmet));
	//		glowMaskColor = Color.White;
	//	}
	//}

	public override void UpdateArmorSet(Player player) {
		string tapDir = Language.GetTextValue(Main.ReversedUpDownArmorSetBonuses ? "Key.UP" : "Key.DOWN");
		player.setBonus = "Taking damage while Wreath is charged depletes it, releasing insect swarm" + $"\nDouble tap {tapDir} to take Insect form";

        BaseFormHandler.KeepFormActive(player);
    }

    void IDoubleTap.OnDoubleTap(Player player, IDoubleTap.TapDirection direction) {
        if (player.HasSetBonusFrom<DreadheartCorruptionHelmet>() && direction == IDoubleTap.TapDirection.Down) {
            BaseFormHandler.ToggleForm<CorruptionInsectForm>(player);
        }
    }

    void IPostSetupContent.PostSetupContent() {
        BaseFormHandler.RegisterForm<CorruptionInsectForm>();
    }

    //public override void AddRecipes() {
    //	CreateRecipe()
    //		.AddIngredient(ItemID.ShadowScale, 10)
    //		.AddIngredient<NaturesHeart>()
    //		.AddTile<OvergrownAltar>()
    //		.Register();
    //}
}