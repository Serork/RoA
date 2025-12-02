using Microsoft.Xna.Framework;

using RoA.Core;
using RoA.Core.Utility;

using System;

using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Nature.Forms;

sealed class FlederBomb : FormProjectile {
    private bool initialize = false;
    private int bounceDirection;

    public override void SetStaticDefaults() {
        //DisplayName.SetDefault("Acorn Bomb");
        Main.projFrames[Projectile.type] = 5;
    }
    protected override void SafeSetDefaults() {
        Projectile.timeLeft = 300;
        Projectile.width = 12;
        Projectile.height = 12;
        Projectile.friendly = true;
        Projectile.tileCollide = true;
        Projectile.penetrate = -1;
        DrawOffsetX = -11;
        DrawOriginOffsetY = -11;
    }

    public override void ModifyDamageHitbox(ref Rectangle hitbox) {
        hitbox.Inflate(6, 6);
    }

    public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
        width = height = 10;

        return base.TileCollideStyle(ref width, ref height, ref fallThrough, ref hitboxCenterFrac);
    }

    public override void AI() {
        if (!initialize) {
            switch (Projectile.ai[0]) {
                case 0:
                    break;
                case 1:
                    Projectile.scale = 1.1f;
                    break;
                case 2:
                    Projectile.scale = 1.3f;
                    break;
            }
            bounceDirection = (Main.rand.Next(2) + 1) * 2 - 3; // -1 or 1
            initialize = true;
        }
        if (Projectile.owner == Main.myPlayer && Projectile.timeLeft <= 10) {
            Projectile.velocity = Vector2.Zero;
            Projectile.tileCollide = false;
            if (Projectile.timeLeft == 10) {
                if (Projectile.ai[0] != 0) {
                    if (!Main.dedServ) {
                        Gore.NewGore(Projectile.GetSource_FromThis(), Projectile.Center - new Vector2(5f, 0f), default(Vector2), Main.rand.Next(61, 64), 0.8f);
                        Gore.NewGore(Projectile.GetSource_FromThis(), Projectile.Center - new Vector2(5f, 0f), default(Vector2), Main.rand.Next(61, 64), 0.8f);
                        Gore.NewGore(Projectile.GetSource_FromThis(), Projectile.Center - new Vector2(5f, 0f), default(Vector2), Main.rand.Next(61, 64), 0.8f);
                    }
                }
                for (int i = 0; i < 12 + 6 * Projectile.ai[0]; i++) {
                    int dust1 = Dust.NewDust(Projectile.position, Projectile.width - 6, Projectile.height - 6, 59, 0f, 0f, 0, default(Color), Main.rand.NextFloat(0.9f, 1.5f));
                    Main.dust[dust1].fadeIn = 1.4f;
                    Main.dust[dust1].velocity *= 5f;
                    Main.dust[dust1].noGravity = true;
                }
                SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);

                if (Projectile.ai[0] == 2) {
                    for (int index = 0; index < 3; ++index) {
                        float num3 = -Projectile.velocity.X * Main.rand.Next(10, 30) * 0.01f + Main.rand.Next(-20, 21) * 0.06f;
                        float num4 = -Math.Abs(Projectile.velocity.Y) * Main.rand.Next(10, 30) * 0.01f + Main.rand.Next(-20, 21) * 0.06f;
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center.X, Projectile.Center.Y, num3, num4, ModContent.ProjectileType<FlederFlame>(), (int)(Projectile.damage * 0.3f), Projectile.knockBack * 0.3f, Projectile.owner, 0f, 0f);
                    }
                }
            }
            if (Projectile.timeLeft % 4 == 0) {
                Projectile.frame++;
            }
            if (Projectile.frame > 5) {
                Projectile.frame = 5;
                Projectile.alpha = 255;
            }
            if (Projectile.timeLeft < 6) {
                Projectile.friendly = false;
            }
            Projectile.position = Projectile.Center;
            if (Projectile.ai[0] == 0) {
                Projectile.width = 48;
                Projectile.height = 48;
                DrawOffsetX = 7;
                DrawOriginOffsetY = 7;
            }
            else {
                Projectile.width = 96;
                Projectile.height = 96;
                DrawOffsetX = 31;
                DrawOriginOffsetY = 31;
            }
            Projectile.Center = Projectile.position;
            Projectile.knockBack = 5f;
        }
        Projectile.velocity.Y = Projectile.velocity.Y + 0.2f;
        Projectile.velocity.X *= 0.9f;
        if (Projectile.velocity.Y > 12f) {
            Projectile.velocity.Y = 12f;
        }
        if (Projectile.ai[0] != 0) {
            double chance = Projectile.velocity.Y > 0.2f && Projectile.localAI[0] != 1f ? 0.5f : MathHelper.Clamp(Ease.CubeIn(1f - (float)Projectile.timeLeft / 300), 0f, 0.75f);
            if (Main.rand.NextChance(chance)) {
                int trail = Dust.NewDust(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height, 59, Projectile.velocity.X * 0.4f, Projectile.velocity.Y * 0.4f, 0, default(Color), Main.rand.NextFloat(1f, 1.5f));
                Main.dust[trail].fadeIn = 1.3f;
                Main.dust[trail].velocity *= 1.5f;
                Main.dust[trail].noGravity = true;
                Projectile.rotation += Projectile.velocity.Y * 0.05f * bounceDirection;
            }
        }
        Lighting.AddLight(Projectile.Center, 0.1f, 0.2f, 0.6f);
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        if (Projectile.timeLeft > 11) {
            Projectile.timeLeft = 11;
        }
    }

    public override bool OnTileCollide(Vector2 oldVelocity) {
        if (Projectile.ai[0] == 0 && Projectile.timeLeft > 11) {
            Projectile.timeLeft = 11;
        }
        if (Projectile.velocity.Y > 0.2f) {
            Projectile.velocity.X += Projectile.velocity.Y * bounceDirection * 0.25f;
            Projectile.velocity.Y *= -0.6f;
        }
        Projectile.localAI[0] = 1f;
        return false;
    }
}
