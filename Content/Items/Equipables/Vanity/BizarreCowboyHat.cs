using Microsoft.Xna.Framework;

using RoA.Common.CustomConditions;

using System.Linq;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Vanity;

[AutoloadEquip(EquipType.Head)]
sealed class BizarreCowboyHat : ModItem {
	 private class BizarreCowboyHatInMerchantShop : GlobalNPC {
        public override void ModifyShop(NPCShop shop) {
            if (shop.NpcType != NPCID.Merchant) {
				return;
			}

            shop.InsertAfter(ItemID.MiningHelmet, ModContent.ItemType<BizarreCowboyHat>(), RoAConditions.HasAnySaddle);
        }
    }


    public override void SetStaticDefaults() {
        //DisplayName.SetDefault("Bizarre Cowboy Hat");
        //Tooltip.SetDefault("'Eat shit, asshole! Fall of your horse!'\n'Nyo ho ho ho'");
        ArmorIDs.Head.Sets.DrawHatHair[Item.headSlot] = true;
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
	}

	public override void SetDefaults() {
		int width = 30; int height = 18;
		Item.Size = new Vector2(width, height);

		Item.rare = ItemRarityID.Blue;
		Item.vanity = true;

        Item.value = Item.sellPrice(0, 3, 0, 0);
    }
}
