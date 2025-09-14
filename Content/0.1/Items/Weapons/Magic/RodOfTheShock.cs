using Microsoft.Xna.Framework;

using RoA.Common.GlowMasks;
using RoA.Content.Items.Special;
using RoA.Content.Projectiles.Friendly.Magic;
using RoA.Core;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Magic;

[AutoloadGlowMask(shouldApplyItemAlpha: true)]
sealed class RodOfTheShock : Rod {
    protected override Color? LightingColor => new(86, 173, 177);

    public override void SetStaticDefaults() {
        Item.ResearchUnlockCount = 1;

        ItemID.Sets.ShimmerTransformToItem[Type] = (ushort)ModContent.ItemType<SphereOfShock>();

        //ItemSwapSystem.SwapToOnRightClick[Type] = (ushort)ModContent.ItemType<SphereOfShock>();
    }

    public override void SetDefaults() {
        base.SetDefaults();

        int width = 42; int height = 42;
        Item.Size = new Vector2(width, height);

        Item.useTime = Item.useAnimation = 10;
        Item.autoReuse = true;

        Item.damage = 15;
        Item.knockBack = 1.5f;

        Item.mana = 11;

        Item.crit = 6;

        Item.value = Item.sellPrice(0, 3, 50, 0);
        Item.rare = ItemRarityID.Orange;
        Item.UseSound = new SoundStyle(ResourceManager.ItemSounds + "Zap") { Volume = 0.7f, PitchVariance = 0.2f };

        Item.channel = true;

        Item.shoot = ModContent.ProjectileType<ShockLightning>();
        Item.shootSpeed = 22f;
    }

    public override void ModifyShootCustom(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
        position += velocity.SafeNormalize(Vector2.Zero) * -2f;
        position += new Vector2(player.direction == 1 ? 2f : 0f, 0f * player.direction).RotatedBy(velocity.ToRotation());
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
        Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, player.whoAmI);

        return false;
    }
}