using Microsoft.Xna.Framework;

using RoA.Common.GlowMasks;
using RoA.Common.Items;
using RoA.Content.Items.Materials;
using RoA.Content.Items.Special;
using RoA.Content.Projectiles.Friendly.Magic;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Magic;

[AutoloadGlowMask]
sealed class RodOfTheShock : Rod {
    protected override Color? LightingColor => new(86, 173, 177);

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ModContent.ItemType<MercuriumNugget>(), 15)
            .AddIngredient(ItemID.GoldBar, 10)
            .AddIngredient(ModContent.ItemType<SphereOfShock>())
            .AddTile(TileID.Anvils)
            .Register();

        CreateRecipe()
            .AddIngredient(ModContent.ItemType<MercuriumNugget>(), 15)
            .AddIngredient(ItemID.PlatinumBar, 10)
            .AddIngredient(ModContent.ItemType<SphereOfShock>())
            .AddTile(TileID.Anvils)
            .Register();
    }

    public override void SetStaticDefaults() {
        Item.ResearchUnlockCount = 1;

        ItemID.Sets.ShimmerTransformToItem[Type] = (ushort)ModContent.ItemType<SphereOfShock>();

        //ItemSwapSystem.SwapToOnRightClick[Type] = (ushort)ModContent.ItemType<SphereOfShock>();
    }

    public override void SetDefaults() {
        base.SetDefaults();

        int width = 42; int height = 42;
        Item.Size = new Vector2(width, height);

        Item.useTime = Item.useAnimation = 8;
        Item.autoReuse = true;

        Item.damage = 3;

        Item.mana = 11;

        Item.value = Item.buyPrice(gold: 1, silver: 10);
        Item.rare = ItemRarityID.Orange;
        Item.UseSound = SoundID.Item81;

        Item.channel = true;

        Item.shoot = ModContent.ProjectileType<ShockLightning>();
        Item.shootSpeed = 22f;
    }

    public override void ModifyShootCustom(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
        position += velocity.SafeNormalize(Vector2.Zero) * -2f;
        position += new Vector2(player.direction == 1 ? 2f : 0f, 0f * player.direction).RotatedBy(velocity.ToRotation());
    }
}