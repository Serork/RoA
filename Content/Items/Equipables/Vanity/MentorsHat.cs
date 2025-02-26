using Microsoft.Xna.Framework;

using RoA.Content.Items.Materials;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Vanity;

[AutoloadEquip(EquipType.Head)]
sealed class MentorsHat : ModItem {
	private sealed class MentorsHatDrop : GlobalNPC {
        public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot) {
            if (npc.type != NPCID.ArmoredViking) {
				return;
			}

			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<MentorsHat>(), 50));
        }
    }

    public override void SetStaticDefaults() {
		//DisplayName.SetDefault("Mentor's Hat");
		//Tooltip.SetDefault("");
		ArmorIDs.Head.Sets.DrawHatHair[Item.headSlot] = true;
		CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
	}

	public override void SetDefaults() {
		int width = 26; int height = 18;
		Item.Size = new Vector2(width, height);

		Item.rare = ItemRarityID.Blue;
		Item.value = Item.sellPrice(silver: 50);
		Item.vanity = true;
	}
}
