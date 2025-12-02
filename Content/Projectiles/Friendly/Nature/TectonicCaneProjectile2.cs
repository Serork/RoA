using Microsoft.Xna.Framework;

using RoA.Content.Dusts;
using RoA.Core.Utility;

using System;

using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class TectonicCaneProjectile2 : NatureProjectile {
    public override void SetStaticDefaults() {
        Main.projFrames[Type] = 2;
    }

    protected override void SafeSetDefaults() {
        Projectile.Size = new Vector2(14, 14);
        Projectile.aiStyle = 0;
        Projectile.friendly = true;
        Projectile.timeLeft = 220 + (int)(20 * Main.rand.NextFloat());
        Projectile.penetrate = 1;

        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 20;
    }

    public override void AI() {
        if (Projectile.localAI[2] == 0f) {
            Projectile.localAI[2] = 1f;

            Projectile.frame = Main.rand.NextBool().ToInt();
        }

        //Helper.ApplyWindPhysics(Projectile.Center, ref Projectile.velocity);

        float value = Projectile.velocity.Length();
        Projectile.direction = (int)Projectile.ai[0];
        Projectile.rotation += value * 0.05f * Projectile.direction;
        Projectile.velocity.Y += 0.1f;
        Projectile.velocity.Y = Math.Min(10f, Projectile.velocity.Y);
    }

    public override bool OnTileCollide(Vector2 oldVelocity) {
        if (Projectile.ai[1] != 1f) {
            float max = Math.Max(oldVelocity.X, oldVelocity.Y);
            if (oldVelocity.X == max) {
                oldVelocity.X = 0f;
            }
            else {
                oldVelocity.Y = 0f;
            }
            Projectile.ai[1] = 1f;
        }
        else {
            Projectile.velocity *= 0.97f;
        }

        return false;
    }

    public override void OnKill(int timeLeft) {
        for (int i = 0; i < 6; i++) {
            Dust dust = Dust.NewDustDirect(Projectile.position - Vector2.One * 2f, Projectile.width + 2, Projectile.height + 2, ModContent.DustType<TectonicDust>(),
                Scale: Main.rand.NextFloat(0.95f, 1.05f) * 1.2f);
            dust.velocity *= Main.rand.NextFloat();
            dust.velocity *= 0.5f;
            dust.customData = 1;
        }
    }
}