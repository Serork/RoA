﻿using Terraria.ID;
using Terraria;
using Terraria.ModLoader;

using Microsoft.Xna.Framework;
using RoA.Core;

namespace RoA.Content.Projectiles.Friendly;

sealed class JudgementCut : ModProjectile {
    public override string Texture => ResourceManager.EmptyTexture;

    public override void SetDefaults() {
        Projectile.width = 8;
        Projectile.height = 8;
        Projectile.friendly = true;
        Projectile.aiStyle = 1;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 25;
        Projectile.extraUpdates = 1;

        Projectile.DamageType = DamageClass.Melee;

        AIType = ProjectileID.Bullet;
    }

    public override bool? CanDamage() => false;

    public override bool? CanCutTiles() => false;

    public override void AI() {
        Projectile.localAI[0] += 1f;
        Projectile.tileCollide = Projectile.localAI[0] >= 10f;
        if (Projectile.localAI[0] % 3f == 0f) {
            Vector2 velocity = new Vector2(0, 14 * Main.rand.NextFloat(0.65f, 1.8f)).RotatedByRandom(MathHelper.TwoPi);
            float duration = 0.01f;
            Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center/* - Vector2.UnitX * 1080f*/ + new Vector2((60 + duration * 2.5f) * Projectile.ai[1], 0) - velocity * 8, velocity, ModContent.ProjectileType<JudgementSlash>(), Projectile.damage, Projectile.knockBack);
        }
    }
}
