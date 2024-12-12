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
sealed class AshwalkerHood : NatureItem, IDoubleTap, IPostSetupContent {
	public override void SetStaticDefaults() {
		//DisplayName.SetDefault("Ashwalker Hood");
		//Tooltip.SetDefault("6% increased nature potential damage");
		CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
	}

    protected override void SafeSetDefaults() {
        int width = 26; int height = 20;
		Item.Size = new Vector2(width, height);

		Item.rare = ItemRarityID.Orange;
		Item.value = Item.sellPrice(silver: 80);

		Item.defense = 4;
	}

	public override void UpdateEquip(Player player) => player.GetModPlayer<DruidStats>().DruidPotentialDamageMultiplier += 0.06f;

    public override bool IsArmorSet(Item head, Item body, Item legs) => body.type == ModContent.ItemType<AshwalkerRobe>() && legs.type == ModContent.ItemType<AshwalkerLeggings>();

	public override void UpdateArmorSet(Player player) {
		string tapDir = Language.GetTextValue(Main.ReversedUpDownArmorSetBonuses ? "Key.UP" : "Key.DOWN");
		player.setBonus = "On Fire! debuff regenerates life instead of doing damage" + $"\nDouble tap {tapDir} to take Firebird form";
        //player.GetModPlayer<DruidArmorSetPlayer>().ashwalkerArmor = true;
        //Lighting.AddLight(player.Center, new Vector3(0.2f, 0.1f, 0.1f));

        BaseFormHandler.KeepFormActive(player);
    }

    void IDoubleTap.OnDoubleTap(Player player, IDoubleTap.TapDirection direction) {
        if (player.HasSetBonusFrom<AshwalkerHood>() && direction == IDoubleTap.TapDirection.Down) {
            BaseFormHandler.ToggleForm<LilPhoenixForm>(player);
        }
    }

    void IPostSetupContent.PostSetupContent() {
        BaseFormHandler.RegisterForm<LilPhoenixForm>();
    }

    //public override void AddRecipes() {
    //	CreateRecipe()
    //		.AddIngredient<FlamingFabric>(15)
    //		.AddTile(TileID.Loom)
    //		.Register();
    //}
}