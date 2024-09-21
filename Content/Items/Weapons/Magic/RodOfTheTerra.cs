using Microsoft.Xna.Framework;

using RoA.Common.GlowMasks;
using RoA.Content.Projectiles.Friendly.Magic;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Magic;

[AutoloadGlowMask]
sealed class RodOfTheTerra : Rod {
    public override void SetStaticDefaults() {
        // DisplayName.SetDefault("Rod of the Terra");
        // Tooltip.SetDefault("Casts a damaging beam of earth energy\n'Forged with Terra'");

        Item.ResearchUnlockCount = 1;
    }

    public override void SetDefaults() {
        base.SetDefaults();

        int width = 36; int height = 36;
        Item.Size = new Vector2(width, height);

        Item.useTime = Item.useAnimation = 8;
        Item.autoReuse = false;

        Item.damage = 30;

        Item.mana = 4;

        Item.value = Item.buyPrice(gold: 1, silver: 10);
        Item.rare = ItemRarityID.Orange;
        Item.UseSound = SoundID.Item69;

        //Item.channel = true;

        Item.noUseGraphic = true;

        Item.shoot = ModContent.ProjectileType<TerraProj>();
        Item.shootSpeed = 10f;
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
        Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, player.whoAmI, 0f, -50f);
        return false;
    }
}