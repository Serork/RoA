using Microsoft.Xna.Framework;

using RoA.Common.Druid;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Accessories;

sealed class FireflyPin : NatureItem {
	public override void SetStaticDefaults() {
        //DisplayName.SetDefault("Firefly Pin");
        //Tooltip.SetDefault("Keeps Wreath full charge bonuses for 2 seconds after discharging");
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
	}

    protected override void SafeSetDefaults() {
        int width = 24; int height = 24;
		Item.Size = new Vector2(width, height);

		Item.rare = ItemRarityID.Blue;
		Item.accessory = true;

		Item.value = Item.buyPrice(gold: 3);
		Item.value = Item.sellPrice(silver: 75);
	}

	public override void UpdateAccessory(Player player, bool hideVisual)  => player.GetModPlayer<DruidStats>().KeepBonusesForTime += 120f;
}