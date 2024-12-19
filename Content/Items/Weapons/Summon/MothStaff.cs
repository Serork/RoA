using Microsoft.Xna.Framework;

using RoA.Content.Buffs;

using System.Collections.Generic;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Summon;

sealed class MothStaff : ModItem {
	public override void SetStaticDefaults() {
		//DisplayName.SetDefault("Moth Staff");
		//Tooltip.SetDefault("Summöns a moth to fight for you");
		//ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true;
		//ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;
		CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
	}

	//public override void ModifyTooltips(List<TooltipLine> tooltips) {
	//	foreach (TooltipLine toolTipLine in tooltips) {
	//		if (toolTipLine.Mod == "Terraria" && toolTipLine.Name == "Damage") {
	//			_ = toolTipLine.Text.Replace("summon", "summön");
	//		}
	//	}
	//}

	public override void SetDefaults() {
		int width = 40; int height = 36;
		Item.Size = new Vector2(width, height);

		Item.useStyle = ItemUseStyleID.HoldUp;
		Item.holdStyle = ItemHoldStyleID.HoldFront;
		Item.useTime = Item.useAnimation = 20;
		Item.autoReuse = false;

		Item.mana = 10;
		Item.channel = true;
		Item.noMelee = true;

		Item.DamageType = DamageClass.Summon;
		Item.damage = 15;
		Item.knockBack = 4f;

		Item.value = Item.sellPrice(gold: 1, silver: 25);
		Item.rare = ItemRarityID.Green;
		Item.UseSound = SoundID.Item77;

		//Item.shoot = ModContent.ProjectileType<Projectiles.Friendly.Summon.Moth>();
	}

	public override void HoldStyle(Player player, Rectangle heldItemFrame) {
		player.itemLocation.X = player.MountedCenter.X + 10f * player.direction;
		player.itemLocation.Y = player.MountedCenter.Y;
		player.itemRotation = 0f;
	}

	public override void HoldItem(Player player) {
		Vector2 position = new Vector2(player.MountedCenter.X + 36f * player.direction - (player.direction != 1 ? 8f : 0f), player.itemLocation.Y + (16f - 32f) * player.gravDir);
		Lighting.AddLight(position, 0.4f, 0.2f, 0f);
		if (Main.rand.Next(4) == 0) {
			Dust dust = Dust.NewDustDirect(position, 4, 4, DustID.Flare, 0f, -10f * player.gravDir, 0, new Color(255, 255, 255), Main.rand.NextFloat(0.8f, 1.2f));
			dust.noGravity = true;
		}
	}
}
