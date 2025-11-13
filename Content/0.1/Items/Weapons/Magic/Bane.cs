using Microsoft.Xna.Framework;

using RoA.Common.GlowMasks;

using System.Collections.Generic;
using System.Linq;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Magic;

[AutoloadGlowMask(185, 185, 185, shouldApplyItemAlpha: true)]
sealed class Bane : ModItem {
    public override void SetStaticDefaults() {
        // DisplayName.SetDefault("Bane");
        // Tooltip.SetDefault("Drains life out of your enemies");
        Item.ResearchUnlockCount = 1;
        //ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<MothStaff>();
    }

    public override void SetDefaults() {
        int width = 28; int height = 30;
        Item.Size = new Vector2(width, height);

        Item.useStyle = ItemUseStyleID.Shoot;
        Item.useTime = Item.useAnimation = 48;
        Item.autoReuse = false;
        Item.useTurn = false;

        Item.DamageType = DamageClass.Magic;
        Item.damage = 1;
        Item.knockBack = 0;

        Item.noMelee = true;
        Item.mana = 35;

        Item.crit = 0;

        Item.rare = ItemRarityID.Green;
        Item.UseSound = SoundID.Item45;

        Item.shoot = ModContent.ProjectileType<Projectiles.Friendly.Magic.BaneSpell>();
        Item.shootSpeed = 6f;

        Item.value = Item.sellPrice(0, 1, 50, 0);
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips) {
        TooltipLine tt = tooltips.FirstOrDefault(x => x.Name == "CritChance" && x.Mod == "Terraria");
        if (tt != null) {
            tooltips.Remove(tt);
        }
    }

    public override Vector2? HoldoutOffset() => new Vector2(2f, 0f);

    public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
        Vector2 newVelo = new Vector2(velocity.X, velocity.Y).SafeNormalize(Vector2.Zero);
        position += newVelo * 10;
        position += new Vector2(-newVelo.Y, newVelo.X) * (-5f * player.direction);
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
        Vector2 funnyOffset = Vector2.Normalize(new Vector2(velocity.X, velocity.Y)) * 15f;
        if (Collision.CanHit(position, 0, 0, position + funnyOffset, 0, 0)) position += funnyOffset;
        Vector2 perturbedSpeed = new Vector2(velocity.X, velocity.Y).RotatedByRandom(MathHelper.ToRadians(5));
        velocity.X = perturbedSpeed.X;
        velocity.Y = perturbedSpeed.Y;
        return true;
    }
}