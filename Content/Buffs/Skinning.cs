using Microsoft.Xna.Framework;

using RoA.Content.Items.Miscellaneous;
using RoA.Content.Tiles.Crafting;

using System;

using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Buffs;

sealed class Skinning : ModBuff {
	public override void SetStaticDefaults() {
		//DisplayName.SetDefault("Skinning");
		//Description.SetDefault("Enemies have a chance to drop rawhides, which will spoil when the effect ends");
	}

	public override void Update(Player player, ref int buffIndex) => player.GetModPlayer<SkinningPlayer>().skinning = true;
}

sealed class SkinningPlayer : ModPlayer {
	public bool skinning;

	public override void ResetEffects()
		=> skinning = false;

	public override void PostUpdateBuffs() {
		int type = (ushort)ModContent.BuffType<Skinning>();
		if (Player.FindBuffIndex(type) != -1)
			return;
		for (int i = 0; i < Player.inventory.Length; i++) {
			Item item = Player.inventory[i];
			if (item != new Item() && (item.type == (ushort)ModContent.ItemType<AnimalLeather>() || item.type == (ushort)ModContent.ItemType<RoughLeather>())) {
				int stack = item.stack;
				Player.inventory[i].SetDefaults((ushort)ModContent.ItemType<SpoiledRawhide>());
				Player.inventory[i].stack = stack;
			}
		}
	}

	public override void PostItemCheck() {
		Item item = Player.inventory[Player.selectedItem];
		if ((item.type == (ushort)ModContent.ItemType<AnimalLeather>() || item.type == (ushort)ModContent.ItemType<RoughLeather>())
			&& Main.tile[Player.tileTargetX, Player.tileTargetY].HasTile && Main.tile[Player.tileTargetX, Player.tileTargetY].TileType == (ushort)ModContent.TileType<TanningRack>()
			&& Player.position.X / 16f - (float)Player.tileRangeX - (float)item.tileBoost - (float)Player.blockRange <= (float)Player.tileTargetX
			&& (Player.position.X + (float)Player.width) / 16f + (float)Player.tileRangeX + (float)item.tileBoost - 1f + (float)Player.blockRange >= (float)Player.tileTargetX && Player.position.Y / 16f - (float)Player.tileRangeY - (float)item.tileBoost - (float)Player.blockRange <= (float)Player.tileTargetY && (Player.position.Y + (float)Player.height) / 16f + (float)Player.tileRangeY + (float)item.tileBoost - 2f + (float)Player.blockRange >= (float)Player.tileTargetY) {
			if (Player.ItemTimeIsZero
				&& Player.itemAnimation > 0
				&& Player.controlUseItem) {
				Player.ApplyItemTime(item);
				foreach (Item inventoryItem in Player.inventory)
					if (inventoryItem.type == item.type) {
						int removed = Math.Min(inventoryItem.stack, 1);
						inventoryItem.stack -= removed;
						if (inventoryItem.stack <= 0)
							inventoryItem.SetDefaults();
						break;
					}
				Vector2 vector = Main.ReverseGravitySupport(Main.MouseScreen) + Main.screenPosition;
				if (Main.SmartCursorIsUsed || PlayerInput.UsingGamepad)
					vector = Player.Center;
				int item2 = Item.NewItem(Player.GetSource_ItemUse(item), (int)vector.X, (int)vector.Y, 1, 1, ItemID.Leather, 1, noBroadcast: false, -1);
				if (Main.netMode == NetmodeID.MultiplayerClient && item2 >= 0)
					NetMessage.SendData(21, -1, -1, null, item2, 1f);
			}
		}
	}
}

sealed class SkinningDropCondition : IItemDropRuleCondition {
	public bool CanDrop(DropAttemptInfo info) {
		if (!info.IsInSimulation)
			return info.player.FindBuffIndex((ushort)ModContent.BuffType<Skinning>()) != -1;
		return false;
	}

	public bool CanShowItemDropInUI()
		=> true;

	public string GetConditionDescription()
		=> "Drops if the player has the buff \"Skinning\"";
}

sealed class SkinningNPC : GlobalNPC {
	public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot) {
		SkinningDropCondition dropCondition = new SkinningDropCondition();
		IItemDropRule conditionalRule = new LeadingConditionRule(dropCondition);
		int npcType = npc.type;
		bool critters = NPCID.Sets.CountsAsCritter[npcType] && !NPCID.Sets.GoldCrittersCollection.Contains(npcType) &&
			!NPCID.Sets.IsDragonfly[npcType]/* && !NPCID.Sets.TownCritter[npcType]*/ && !npc.FullName.Contains("utterfly");
		bool enemies = (NPCID.Sets.Zombies[npcType] || npc.DeathSound == SoundID.NPCDeath39 || npc.HitSound == SoundID.NPCHit27) && !NPCID.Sets.Skeletons[npcType] &&
			npc.aiStyle != 22;
		NPCsType type;
		type = critters ? NPCsType.Critters : enemies ? NPCsType.Enemies : NPCsType.None;
		if (type == NPCsType.None)
			return;
		int itemType = type switch {
			NPCsType.Critters => (ushort)ModContent.ItemType<AnimalLeather>(),
			NPCsType.Enemies => (ushort)ModContent.ItemType<RoughLeather>()
		};
		IItemDropRule rule = ItemDropRule.Common(itemType, chanceDenominator: 7);
		conditionalRule.OnSuccess(rule);
		npcLoot.Add(conditionalRule);
	}

	enum NPCsType {
		Critters,
		Enemies,
		None
	}
}