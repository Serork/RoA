using Microsoft.Xna.Framework;

using RoA.Common.CustomConditions;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Vanity;

[AutoloadEquip(EquipType.Head)]
sealed class SailorHat : ModItem {
     private class SailorHatInMerchantShop : GlobalNPC {
        public override void ModifyShop(NPCShop shop) {
            if (shop.NpcType != NPCID.Pirate) {
                return;
            }

            shop.InsertAfter(ItemID.PiratePants, ModContent.ItemType<SailorHat>(), RoAConditions.SailorHatCondition);
        }
    }

    public override void SetStaticDefaults() {
        //DisplayName.SetDefault("Sailor Hat");
        //Tooltip.SetDefault("'Who the hell am I?'");
        ArmorIDs.Head.Sets.DrawHatHair[Item.headSlot] = true;
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
	}

	public override void SetDefaults() {
		int width = 20; int height = 18;
		Item.Size = new Vector2(width, height);

		Item.rare = ItemRarityID.Orange;
		Item.vanity = true;

        Item.value = Item.sellPrice(0, 3, 0, 0);
    }
}
