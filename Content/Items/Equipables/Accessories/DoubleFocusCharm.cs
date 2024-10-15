using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Accessories;

sealed class DoubleFocusCharm : NatureItem {
	public override void SetStaticDefaults() {
		Item.ResearchUnlockCount = 1;
	}

    protected override void SafeSetDefaults() {
        int width = 28; int height = 20;
		Item.Size = new Vector2(width, height);

		Item.rare = ItemRarityID.Green;
		Item.accessory = true;
		Item.value = Item.sellPrice(gold: 1, silver: 80);
	}

	public override void UpdateAccessory(Player player, bool hideVisual) => player.GetDamage(DruidClass.NatureDamage) += 0.12f;
}