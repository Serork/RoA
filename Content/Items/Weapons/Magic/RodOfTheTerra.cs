using Microsoft.Xna.Framework;

using RoA.Common.GlowMasks;
using RoA.Common.Items;
using RoA.Content.Items.Materials;
using RoA.Content.Items.Special;
using RoA.Content.Projectiles.Friendly.Magic;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Magic;

[AutoloadGlowMask]
sealed class RodOfTheTerra : Rod {
    protected override Color? LightingColor => new(73, 170, 104);

    public override void SetStaticDefaults() {
        // DisplayName.SetDefault("Rod of the Terra");
        // Tooltip.SetDefault("Casts a damaging beam of earth energy\n'Forged with Terra'");

        Item.ResearchUnlockCount = 1;

        ItemID.Sets.ShimmerTransformToItem[Type] = (ushort)ModContent.ItemType<SphereOfQuake>();

        //ItemSwapSystem.SwapToOnRightClick[Type] = (ushort)ModContent.ItemType<SphereOfQuake>();
    }

    public override void SetDefaults() {
        base.SetDefaults();

        int width = 42; int height = 42;
        Item.Size = new Vector2(width, height);

        Item.useTime = Item.useAnimation = 8;
        Item.autoReuse = false;

        Item.damage = 29;

        Item.mana = 7;

        Item.value = Item.buyPrice(gold: 1, silver: 10);
        Item.rare = ItemRarityID.Orange;
        Item.UseSound = SoundID.Item69;

        //Item.channel = true;

        Item.noUseGraphic = true;

        Item.shoot = ModContent.ProjectileType<TerraProj>();
        Item.shootSpeed = 10f;
    }

    public override Vector2? HoldoutOrigin() => null;

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
        Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, player.whoAmI, 0f, -50f);
        return false;
    }
}