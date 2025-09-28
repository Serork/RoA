using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common;
using RoA.Common.Projectiles;
using RoA.Core.Defaults;
using RoA.Core.Utility;
using RoA.Core.Utility.Vanilla;

using System.Collections.Generic;

using Terraria;
using Terraria.ID;

namespace RoA.Content.Projectiles.Friendly.Nature.Forms;

sealed class HallowedZone : FormProjectile_NoTextureLoad {
    protected override void SafeSetDefaults() {
        Projectile.SetSizeValues(10);

        Projectile.friendly = true;
        Projectile.penetrate = 1;
        Projectile.alpha = 0;

        Projectile.timeLeft = 180;

        Projectile.tileCollide = false;

        Projectile.penetrate = -1;
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        if (Vector2.Distance(targetHitbox.Center.ToVector2(), projHitbox.Center.ToVector2()) < 100) {
            return true;
        }

        return false;
    }

    public override bool? CanDamage() => Projectile.ai[0] >= 30f;

    public override void AI() {
        Projectile.ai[0]++;
        if (Projectile.ai[0] >= 30f) {
            if (Projectile.ai[0] > 60f) {
                Projectile.ai[0] = 0f;
            }

            int num67 = 4;
            for (int m = 0; m < num67; m++) {
                float num68 = MathHelper.Lerp(1.3f, 0.7f, Main.rand.NextFloat()) * Utils.GetLerpValue(0f, 120f, Main.rand.NextFloat() * 120f, clamped: true);
                Color newColor2 = Color.Yellow;
                int num69 = Dust.NewDust(Projectile.Center + Main.rand.NextVector2Circular(100, 100), 0, 0, DustID.TintableDustLighted, 0f, 0f, 0, newColor2);
                Main.dust[num69].position = Projectile.Center + Main.rand.NextVector2Circular(100, 100);
                Main.dust[num69].velocity *= Main.rand.NextFloat() * 0.8f;
                Main.dust[num69].noGravity = true;
                Main.dust[num69].fadeIn = 1.35f - Main.rand.NextFloat(0.4f);
                Main.dust[num69].velocity -= Vector2.UnitY * 3f * Main.rand.NextFloat(0.5f, 1f);
                Main.dust[num69].scale = 0.35f;
            }
        }
    }
}
