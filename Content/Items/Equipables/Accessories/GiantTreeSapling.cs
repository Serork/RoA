using Microsoft.Xna.Framework;

using RoA.Common.Druid.Wreath;
using RoA.Common.Druid;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Accessories;

sealed class GiantTreeSapling : NatureItem {
	public override void SetStaticDefaults() {
		//DisplayName.SetDefault("Giant Tree Sapling");
		//Tooltip.SetDefault("Increases wreath filling rate by 10%\nIncreases maximum life by 20 while wreath is charged");
		CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
	}

    protected override void SafeSetDefaults() {
        int width = 20; int height = 34;
		Item.Size = new Vector2(width, height);

		Item.rare = ItemRarityID.Blue;
		Item.accessory = true;
		Item.value = Item.sellPrice(gold: 1);
	}

	public override void UpdateAccessory(Player player, bool hideVisual) {
        player.GetModPlayer<DruidStats>().DruidDamageExtraIncreaseValueMultiplier += 0.1f;
        if (player.GetModPlayer<WreathHandler>().IsFull) {
            player.statLifeMax2 += 20;
        }
    }
}