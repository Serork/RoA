using Microsoft.Xna.Framework;

using RoA.Common.GlowMasks;
using RoA.Common.Items;
using RoA.Content.Items.Special;
using RoA.Content.Projectiles.Friendly.Magic;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Magic;

[AutoloadGlowMask]
sealed class RodOfTheDragonfire : Rod {
    protected override Color? LightingColor => new(255, 154, 116);

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient<SphereOfPyre>()
            .Register();
    }

    public override void SetStaticDefaults() {
        Item.ResearchUnlockCount = 1;

        ItemID.Sets.ShimmerTransformToItem[Type] = (ushort)ModContent.ItemType<SphereOfPyre>();

        //ItemSwapSystem.SwapToOnRightClick[Type] = (ushort)ModContent.ItemType<SphereOfPyre>();
    }

    public override void SetDefaults() {
        base.SetDefaults();

        int width = 42; int height = 42;
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
        position += newVelocity * -2f;
        position += new Vector2(player.direction == -1 ? 4f : -4f, -6f * player.direction).RotatedBy(newVelocity.ToRotation());
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
        if (base.Shoot(player, source, position, velocity, type, damage, knockback)) {
            int amount = Main.rand.Next(2, 4);
            Vector2 dustPosition = position;
            for (int i = -amount + 1; i < amount; i++) {
                Vector2 vector2 = Utils.RotatedBy(velocity, (double)(i / 10f)) * Main.rand.NextFloat(0.75f, 1.35f);
                if (Main.rand.NextBool()) {
                    Dust dust = Dust.NewDustDirect(dustPosition, 0, 0, 6, velocity.X, velocity.Y, 100);
                    dust.scale = 0.2f;
                    for (int i2 = 0; i2 < 3; i2++) {
                        if (Main.rand.Next(4) == 0) {
                            dust.noGravity = true;
                            dust.scale *= 3f;
                            dust.velocity.X *= 2f;
                            dust.velocity.Y *= 2f;
                        }
                        else {
                            dust.scale *= 1.5f;
                        }
                    }
                    if (dust.scale < 1f) {
                        dust.scale = 1f;
                    }
                    dust.noGravity = true;
                }
                Vector2 spawnPosition = position + vector2.SafeNormalize(Vector2.Zero) * -4f + vector2 + vector2 + new Vector2(0f, 12f * player.direction).RotatedBy(velocity.ToRotation());
                Projectile.NewProjectileDirect(source, spawnPosition, vector2, type, damage, knockback, player.whoAmI);
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