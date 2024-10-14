﻿using Microsoft.Xna.Framework;

using RoA.Content.Buffs;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Materials;

sealed class Cloudberry : ModItem {
	public override void SetStaticDefaults() {
        //DisplayName.SetDefault("Cloudberry");
        //Tooltip.SetDefault("Recovers 1 life per second");
        Item.ResearchUnlockCount = 25;

        ItemID.Sets.SortingPriorityMaterials[Item.type] = 58;

        Main.RegisterItemAnimation(Type, new DrawAnimationVertical(int.MaxValue, 3));
        ItemID.Sets.FoodParticleColors[Item.type] = new Color[3] {
            new(235, 150, 12),
            new(209, 102, 36),
            new(202, 66, 32)
        };
		ItemID.Sets.IsFood[Type] = true;

    }

	public override void SetDefaults() {
		int width = 30; int height = 22;
		Item.Size = new Vector2(width, height);

		Item.value = Item.sellPrice(silver: 10);
		Item.rare = ItemRarityID.White;
		Item.maxStack = 999;

		Item.useTime = 10;
		Item.useAnimation = 15;
		Item.useStyle = ItemUseStyleID.EatFood;
		Item.UseSound = SoundID.Item2;

		Item.consumable = true;
		Item.buffType = ModContent.BuffType<ExtraRegen>();
		Item.buffTime = 600;
	}
}
