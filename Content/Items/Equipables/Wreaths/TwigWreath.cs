using Microsoft.Xna.Framework;

using RoA.Common.Druid.Wreath;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Wreaths;

sealed class TwigWreath : BaseWreathItem {
    private sealed class TwigWreathInDryadShopSystem : GlobalNPC {
        public override void ModifyShop(NPCShop shop) {
            if (shop.NpcType != NPCID.Dryad) {
                return;
            }

            shop.InsertAfter(ItemID.DirtRod, ModContent.ItemType<TwigWreath>());
        }
    }
    
    protected override void SafeSetDefaults() {
		int width = 20; int height = width; 
		Item.Size = new Vector2(width, height);

		Item.maxStack = 1;
		Item.value = Item.buyPrice(gold: 3);
		Item.rare = ItemRarityID.Blue;
	}

    public override void UpdateAccessory(Player player, bool hideVisual) {
        if (player.GetModPlayer<WreathHandler>().IsFull) {
            player.endurance += 0.1f;
        }
    }
}
