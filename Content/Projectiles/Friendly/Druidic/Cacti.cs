using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Content.Dusts;
using RoA.Core;
using RoA.Core.Utility;
using RoA.Utilities;

using System;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Druidic;

sealed class Cacti : NatureProjectile {
    private enum State {
        Normal,
        Enchanted
    }

    private State _state = State.Normal;

    public override void SetStaticDefaults() => Projectile.SetTrail(length: 6);

    protected override void SafeSetDefaults() {
        Projectile.Size = 24 * Vector2.One;
        Projectile.aiStyle = -1;
        Projectile.friendly = true;
        Projectile.timeLeft = 200;
        Projectile.penetrate = -1;
    }

    //public override bool ShouldUpdatePosition() => false;

    protected override void SafeOnSpawn(IEntitySource source) {
        if (Projectile.owner != Main.myPlayer) {
            return;
        }

        Player player = Main.player[Projectile.owner];
        Vector2 mousePoint = player.GetViableMousePosition();
        float y = player.MountedCenter.Y - player.height * 5f;
        float lastY = Math.Abs(y - Main.screenPosition.Y + Main.screenHeight);
        Vector2 pointPosition = new(mousePoint.X, y);
        Projectile.Center = pointPosition + Vector2.UnitY * lastY;
        Vector2 dif = pointPosition - Projectile.Center;
        Projectile.velocity.X = 0f;
        Projectile.velocity.Y = -dif.Length() * 0.05f;
        Projectile.ai[1] = Main.player[Projectile.owner].direction;

        Projectile.netUpdate = true;
    }

    public override bool PreDraw(ref Color lightColor) {
        if (_state == State.Enchanted) {
            Texture2D texture = Projectile.GetTexture(),
                      trailTexture = ModContent.Request<Texture2D>(ResourceManager.ProjectileTextures + "CactiTrail").Value;

            Vector2 origin = texture.Size() / 2f;

            Color color = lightColor;

            int length = Projectile.oldPos.Length;
            for (int i = 0; i < length - 1; i++) {
                float progress = (length - i) / (float)length * 1.25f;
                color *= Utils.Remap(progress, 0f, 1f, 0.5f, 1f);

                float scale = Projectile.scale * Math.Clamp(progress, 0.5f, 1f);

                float offsetYBetween = Projectile.Size.Y * 0.15f;

                Vector2 dif = (Projectile.position - Projectile.oldPos[i]).SafeNormalize(Vector2.UnitY);
                Main.EntitySpriteDraw(trailTexture,
                                      Projectile.oldPos[i] + dif * offsetYBetween / 2f + origin - dif * offsetYBetween * i - Main.screenPosition,
                                      null,
                                      color,
                                      Projectile.rotation,
                                      origin,
                                      scale,
                                      default);
            }

            int offset = 1, size = offset * 2;
            for (int k = 0; k < 2; k++) {
                for (int i = -size; i <= size; i += offset) {
                    for (int j = -size; j <= size; j += offset) {
                        if (Math.Abs(i) + Math.Abs(j) == size) {
                            Main.EntitySpriteDraw(trailTexture, Projectile.Center + new Vector2(i, j) * 2f - Main.screenPosition,
                                                  null,
                                                  color * 0.5f,
                                                  Projectile.rotation,
                                                  origin,
                                                  Projectile.scale,
                                                  default);
                        }
                    }
                }
            }
        }

        return true;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        if (_state == State.Enchanted) {
            Projectile.Kill();
        }
    }

    public override void OnKill(int timeLeft) {
        if (Main.netMode != NetmodeID.Server) {
            for (int num559 = 0; num559 < 10; num559++) {
                int num560 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.JunglePlants);
                Dust dust2 = Main.dust[num560];
                dust2.noLight = true;
                if (Main.rand.NextBool(2)) {
                    dust2.scale *= 1.2f;
                }
            }

            for (float num17 = 0f; num17 < 1f; num17 += 0.025f) {
                Dust dust8 = Dust.NewDustPerfect(Projectile.Center + Vector2.UnitY * Projectile.Size.Y + Main.rand.NextVector2Circular(80, 40f) * Projectile.scale + Projectile.velocity.SafeNormalize(Vector2.UnitY) * num17, ModContent.DustType<CactiCasterDust>(), Main.rand.NextVector2Circular(3f, 3f), Scale: Main.rand.NextFloat(1.25f, 1.5f));
                dust8.velocity.Y -= 4f;
                dust8.noGravity = true;
                Dust dust2 = dust8;
                dust2.velocity += Projectile.velocity * 0.2f;
                dust2.velocity *= 1.01f;
            }
        }

        //Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<CactiExplosion>(), Projectile.damage * 2, Projectile.knockBack * 2.25f, Projectile.owner);
    }

    public override void AI() {
        Projectile.tileCollide = _state == State.Enchanted;

        switch (_state) { 
            case State.Normal:
                if (Main.netMode != NetmodeID.Server && Main.rand.NextBool(2)) {
                    int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.JunglePlants, Projectile.velocity.X * 0.5f, Projectile.velocity.Y * 0.5f, 150, default, 1.2f);
                    Main.dust[dust].noGravity = true;
                }

                Projectile.velocity.Y *= 0.95f;
                Projectile.rotation = Projectile.velocity.Y * Projectile.ai[1];
                if (Math.Abs(Projectile.velocity.Y) <= 0.5f) {
                    Player player = Main.player[Projectile.owner];
                    if (player.whoAmI == Main.myPlayer) {
                        Vector2 mousePoint = player.GetViableMousePosition();
                        float speed = MathHelper.Clamp((mousePoint - Projectile.Center).Length() * 0.0375f, 9.25f, 11f);
                        Projectile.velocity = Helper.VelocityToPoint(Projectile.Center, mousePoint, speed);
                        Projectile.netUpdate = true;
                    }

                    _state = State.Enchanted;

                    if (Main.netMode != NetmodeID.Server) {
                        for (int i = 0; i < 30; i++) {
                            int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<CactiCasterDust>(), 0f, -2f, 0, default, 1.5f);
                            Main.dust[dust].position.X += Main.rand.Next(-50, 51) * 0.05f - 1.5f;
                            Main.dust[dust].position.Y += Main.rand.Next(-50, 51) * 0.05f - 1.5f;
                            Main.dust[dust].noGravity = true;
                            if (Main.dust[dust].position != Projectile.Center) {
                                Main.dust[dust].velocity = Projectile.DirectionTo(Main.dust[dust].position) * 2f;
                            }
                        }
                    }
                }
                break;
            case State.Enchanted:
                Projectile.rotation = (Projectile.velocity.SafeNormalize(Vector2.One) * 2f).ToRotation() - MathHelper.PiOver2;
                if (Main.netMode != NetmodeID.Server) {
                    for (int i = 0; i < 2; i++) {
                        int direction = i != 0 ? 1 : -1;
                        Vector2 vector32 = new(Projectile.Size.X * 0.4f * direction, Projectile.Size.Y * 0.15f);
                        vector32 = vector32.RotatedBy(Projectile.rotation);
                        int type = Dust.NewDust(Projectile.Center - Vector2.One * 2f + Projectile.velocity + vector32, 4, 4, ModContent.DustType<CactiCasterDust>());
                        Dust dust = Main.dust[type];
                        dust.scale = Main.rand.NextFloat(0.8f, 1f) * 1.4f;
                        dust.noGravity = true;
                        dust.velocity = (dust.velocity * 0.25f + Vector2.Normalize(vector32)).SafeNormalize(new Vector2(10f * direction, -1f)) * Main.rand.NextFloat(1f, 1.5f) * 3f;
                        dust.velocity -= new Vector2(3f * direction, 5f).RotatedBy(Projectile.rotation);
                        dust.fadeIn = 1f;
                        dust.scale *= 0.95f;
                    }
                }
                break;
        }
    }
}