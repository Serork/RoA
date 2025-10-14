using Microsoft.Xna.Framework;

using RoA.Common.GlowMasks;
using RoA.Content.Projectiles.Friendly.Melee;
using RoA.Core.Utility;

using System;

using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Melee;

[AutoloadGlowMask(shouldApplyItemAlpha: true)]
sealed class StarFusion : ModItem {
    public override void SetStaticDefaults() {
        // Tooltip.SetDefault("Creates magical constellation on hit");
        Item.ResearchUnlockCount = 1;
    }

    public override void SetDefaults() {
        int width = 58; int height = width;
        Item.Size = new Vector2(width, height);

        Item.useStyle = ItemUseStyleID.Swing;
        Item.useTime = Item.useAnimation = 40;
        Item.autoReuse = false;
        Item.useTurn = true;

        Item.DamageType = DamageClass.Melee;
        Item.damage = 23;
        Item.knockBack = 5.5f;

        Item.rare = ItemRarityID.Orange;
        Item.UseSound = SoundID.Item1;

        Item.value = Item.sellPrice(0, 2, 0, 0);

        //Item.glowMask = RiseofAgesGlowMask.Get(nameof(StarFusion));
    }

    private static void GetPointOnSwungItemPath(Player player, float spriteWidth, float spriteHeight, float normalizedPointOnPath, float itemScale, out Vector2 location, out Vector2 outwardDirection) {
        float num = (float)Math.Sqrt(spriteWidth * spriteWidth + spriteHeight * spriteHeight);
        float num2 = (float)(player.direction == 1).ToInt() * ((float)Math.PI / 2f);
        if (player.gravDir == -1f)
            num2 += (float)Math.PI / 2f * (float)player.direction;

        outwardDirection = player.itemRotation.ToRotationVector2().RotatedBy(3.926991f + num2);
        location = player.RotatedRelativePoint(player.itemLocation + outwardDirection * num * normalizedPointOnPath * itemScale);
    }

    public override bool? UseItem(Player player) {
        if (player.whoAmI == Main.myPlayer && Main.rand.NextChance(0.5)) {
            GetPointOnSwungItemPath(player, 58f, 58f, 0.3f + Main.rand.NextFloat(0.6f), 1f, out var location, out var outwardDirection);
            Vector2 vector = outwardDirection.RotatedBy((float)Math.PI / 2f * (float)player.direction * player.gravDir);
            Dust dust = Dust.NewDustPerfect(location, Main.rand.NextBool(3) ? DustID.Enchanted_Gold : DustID.YellowStarDust, vector * 4f, 255, default(Color), 1.2f);
            dust.noGravity = true;
            dust.noLightEmittence = true;
        }

        return base.UseItem(player);
    }

    public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone) {
        if (!target.CanActivateOnHitEffect()) {
            return;
        }

        int index = 0;
        void spawnStar() {
            index++;
            Vector2 velocity = Vector2.One.RotatedByRandom(MathHelper.TwoPi) * 0.5f;
            Vector2 offset = new Vector2(0f, 200f).RotatedBy(velocity.ToRotation());
            ushort type = (ushort)ModContent.ProjectileType<MeltingStar>();
            Vector2 spawnPos = target.Center + offset;
            Vector2 projectileVelocity = Helper.VelocityToPoint(spawnPos, target.Center, 1f);
            Projectile.NewProjectile(target.GetSource_OnHit(target), spawnPos.X, spawnPos.Y, projectileVelocity.X, projectileVelocity.Y, type,
                Item.damage - Item.damage / 4, Item.knockBack / 3f, player.whoAmI, 0f, (float)index, target.whoAmI);
        }
        for (int i = 0; i < 3; i++) {
            spawnStar();
        }
        SoundEngine.PlaySound(SoundID.Item4 with { Volume = 0.3f, Pitch = -0.2f }, target.Center);
    }
}