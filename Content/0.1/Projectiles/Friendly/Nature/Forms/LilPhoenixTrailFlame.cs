using Microsoft.Xna.Framework;

using RoA.Content.Forms;
using RoA.Core;

using System;
using System.Collections.Generic;

using Terraria;

namespace RoA.Content.Projectiles.Friendly.Nature.Forms;

sealed class LilPhoenixTrailFlame : FormProjectile {
    private readonly List<Vector2> _positions = [];

    public override string Texture => ResourceManager.EmptyTexture;
    public override bool PreDraw(ref Color lightColor) => false;

    protected override void SafeSetDefaults() {
        Projectile.width = 8;
        Projectile.height = 8;
        Projectile.friendly = true;
        Projectile.aiStyle = -1;
        Projectile.penetrate = 6;
        Projectile.timeLeft = 260;
        Projectile.extraUpdates = 1;
        Projectile.tileCollide = false;
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        foreach (Vector2 trailPosition in _positions) {
            var checkHitbox = targetHitbox;
            checkHitbox.Inflate(4, 4);
            if (checkHitbox.Contains(trailPosition.ToPoint())) {
                return true;
            }
        }

        return base.Colliding(projHitbox, targetHitbox);
    }

    public override void AI() {
        Player player = Main.player[Projectile.owner];
        if (!player.active || player.dead)
            Projectile.Kill();
        Projectile.ai[1]++;
        float offset;
        if (Projectile.ai[0] == 1f)
            offset = (float)Math.Cos((Projectile.ai[1] / 30) * Math.PI);
        else
            offset = (float)Math.Cos((Projectile.ai[1] / 30 + 1) * Math.PI);
        Vector2 pos = player.Center + player.fullRotationOrigin - player.Size / 2f;
        Vector2 vector2 = pos + new Vector2(-10f * player.direction, 5f * offset).RotatedBy(player.velocity.SafeNormalize(Vector2.Zero).ToRotation()) * 2f * player.direction;
        vector2 = vector2.RotatedBy(player.fullRotation, vector2);
        if (!player.GetModPlayer<LilPhoenixFormHandler>()._dashed) {
            Projectile.ai[2] = 1f;
            Projectile.netUpdate = true;
        }
        if (Projectile.ai[2] != 1f) {
            Projectile.timeLeft = 260;
            Projectile.position = vector2;
            bool flag = true;
            foreach (Vector2 trailPosition in _positions) {
                if (trailPosition.Distance(Projectile.position) <= 1.5f) {
                    flag = false;
                    break;
                }
            }
            if (flag) {
                _positions.Add(Projectile.position);
            }
            Projectile.netUpdate = true;
        }
        foreach (Vector2 trailPosition in _positions) {
            if (Main.rand.NextBool(8)) {
                int dust = Dust.NewDust(trailPosition, 2, 2, 6, 0f, -0.5f, Projectile.alpha, default, Main.rand.NextFloat(1f, 2f));
                Main.dust[dust].noGravity = true;
                Main.dust[dust].fadeIn = Main.rand.NextFloat(0.5f, 1.5f);
            }
        }
    }
}
