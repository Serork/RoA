﻿using Microsoft.Xna.Framework;

using RoA.Common.GlowMasks;
using RoA.Content.Projectiles.Friendly.Magic;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Magic;

[AutoloadGlowMask]
sealed class RodOfTheDragonfire : Rod {
    protected override Color? LightingColor => new(255, 154, 116);

    public override void SetStaticDefaults() {
        Item.ResearchUnlockCount = 1;
    }

    public override void SetDefaults() {
        base.SetDefaults();

        int width = 42; int height = width;
        Item.Size = new Vector2(width, height);

        Item.useTime = Item.useAnimation = 6;
        Item.autoReuse = true;

        Item.damage = 11;
        Item.knockBack = 2f;

        Item.mana = 14;

        Item.value = Item.buyPrice(gold: 1, silver: 10);
        Item.rare = ItemRarityID.Orange;
        Item.UseSound = SoundID.Item73;

        Item.shoot = ModContent.ProjectileType<Hellbat>();
        Item.shootSpeed = 5f;
    }

    public override void ModifyShootCustom(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
        Vector2 newVelocity = Utils.SafeNormalize(new Vector2(velocity.X, velocity.Y), Vector2.Zero);
        position -= newVelocity * 10f;
        position += new Vector2(-newVelocity.Y, newVelocity.X) * (10f * player.direction);
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
        if (base.Shoot(player, source, position, velocity, type, damage, knockback)) {
            int amount = Main.rand.Next(2, 4);
            for (int i = -amount + 1; i < amount; i++) {
                Vector2 vector2 = Utils.RotatedBy(velocity, (double)(i / 10f)) * Main.rand.NextFloat(0.75f, 1.35f);
                Projectile.NewProjectileDirect(source, position + vector2, vector2, type, damage, knockback, player.whoAmI);
            }
        }

        return false;
    }

    //public override void AddRecipes() {
    //    CreateRecipe()
    //        .AddIngredient<Elderwood>(10)
    //        .AddRecipeGroup(CustomRecipes.GoldRecipeGroup, 5)
    //        .AddIngredient(ItemID.Meteorite, 10)
    //        .AddIngredient(ItemID.Ruby, 7)
    //        .AddIngredient(ItemID.ManaCrystal)
    //        .AddTile(TileID.DemonAltar)
    //        .Register();
    //}
}