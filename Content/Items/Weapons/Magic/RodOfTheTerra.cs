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
sealed class RodOfTheTerra : Rod {
    protected override Color? LightingColor => new(73, 170, 104);

    public override void SetStaticDefaults() {
        // DisplayName.SetDefault("Rod of the TerraDye");
        // Tooltip.SetDefault("Casts a damaging beam of earth energy\n'Forged with TerraDye'");

        Item.ResearchUnlockCount = 1;

        //ItemID.Sets.ShimmerTransformToItem[Type] = (ushort)ModContent.ItemType<SphereOfQuake>();

        //ItemSwapSystem.SwapToOnRightClick[Type] = (ushort)ModContent.ItemType<SphereOfQuake>();
    }

    public override void SetDefaults() {
        base.SetDefaults();

        int width = 42; int height = 42;
        Item.Size = new Vector2(width, height);

        Item.useTime = Item.useAnimation = 60;
        Item.autoReuse = false;

        Item.damage = 29;
        Item.knockBack = 0f;

        Item.mana = 3;

        Item.value = Item.sellPrice(0, 3, 50, 0);
        Item.rare = ItemRarityID.Orange;
        Item.UseSound = new SoundStyle(ResourceManager.ItemSounds + "SpellCast") { Volume = 0.9f };

        //Item.channel = true;

        Item.noUseGraphic = true;

        Item.shoot = ModContent.ProjectileType<TerraProj>();
        Item.shootSpeed = 10f;
    }

    public override void ModifyManaCost(Player player, ref float reduce, ref float mult) {
        mult *= 0f;
    }

    public override Vector2? HoldoutOrigin() => null;

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
        Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, player.whoAmI, 0f, -50f);
        return false;
    }
}