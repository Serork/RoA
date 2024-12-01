using Microsoft.Xna.Framework.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;
using System;
using Terraria.DataStructures;

namespace RoA.Content.Projectiles.Friendly.Druidic;

sealed class SacrificialSickle : NatureProjectile {
    public override void SetStaticDefaults() {
        //DisplayName.SetDefault("Sacrificial Sickle");
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
    }

    protected override void SafeSetDefaults() {
        int width = 38; int height = width;
        Projectile.Size = new Vector2(width, height);

        //Projectile.CloneDefaults(ProjectileID.LightDisc);
        //AIType = 106;
        Projectile.aiStyle = AIType = -1;

        Projectile.penetrate = -1;
        Projectile.scale = 1f;

        Projectile.timeLeft = 200;

        Projectile.tileCollide = false;
    }

    protected override void SafeOnSpawn(IEntitySource source) {
        base.SafeOnSpawn(source);

        Projectile.ai[2] = Main.player[Projectile.owner].direction;
    }

    public override void AI() {
        Projectile.rotation += 0.3f * Projectile.ai[2];
        if (Projectile.ai[0] == 0f) {
            Projectile.velocity *= 0.975f;
            bool flag = true;

            if (flag)
                Projectile.ai[1] += 1f;
            
            if (Projectile.ai[1] >= 35f) {
                Projectile.ai[0] = 1f;
                Projectile.ai[1] = 0f;
                Projectile.netUpdate = true;
            }
        }
        else {
            //tileCollide = false;
            float num63 = 7f;
            float num64 = 0.2f;
            //if (type == 1000)
            //    num63 = 9.5f;

            //if (type == 19) {
            //    num63 = 20f;
            //    num64 = 1.5f;
            //}
            //else if (type == 33) {
            //    num63 = 18f;
            //    num64 = 1.2f;
            //}
            //else if (type == 182) {
            //    num63 = 16f;
            //    num64 = 1.2f;
            //}
            //else if (type == 866) {
            //    num63 = 16f;
            //    num64 = 1.2f;
            //}
            //else if (type == 106) {
            //    num63 = 16f;
            //    num64 = 1.2f;
            //}
            //else if (type == 272) {
            //    num63 = 20f;
            //    num64 = 1.5f;
            //}
            //else if (type == 333) {
            //    num63 = 12f;
            //    num64 = 0.6f;
            //}
            //else if (type == 301) {
            //    num63 = 15f;
            //    num64 = 3f;
            //}
            //else if (type == 320) {
            //    num63 = 15f;
            //    num64 = 3f;
            //}
            //else if (type == 383) {
            //    num63 = 16f;
            //    num64 = 4f;
            //}

            Vector2 vector6 = new Vector2(Projectile.position.X + (float)Projectile.width * 0.5f, Projectile.position.Y + (float)Projectile.height * 0.5f);
            float num65 = Main.player[Projectile.owner].position.X + (float)(Main.player[Projectile.owner].width / 2) - vector6.X;
            float num66 = Main.player[Projectile.owner].position.Y + (float)(Main.player[Projectile.owner].height / 2) - vector6.Y;
            float num67 = (float)Math.Sqrt(num65 * num65 + num66 * num66);
            if (num67 > 3000f)
                Projectile.Kill();

            num67 = num63 / num67;
            num65 *= num67;
            num66 *= num67;
            Vector2 vector7 = new Vector2(num65, num66) - Projectile.velocity;
            if (vector7 != Vector2.Zero) {
                Vector2 vector8 = vector7;
                vector8.Normalize();
                Projectile.velocity += vector8 * Math.Min(num64, vector7.Length());
            }
            if (Type == 383) {

            }
            else {
                if (Projectile.velocity.X < num65) {
                    Projectile.velocity.X += num64;
                    if (Projectile.velocity.X < 0f && num65 > 0f)
                        Projectile.velocity.X += num64;
                }
                else if (Projectile.velocity.X > num65) {
                    Projectile.velocity.X -= num64;
                    if (Projectile.velocity.X > 0f && num65 < 0f)
                        Projectile.velocity.X -= num64;
                }

                if (Projectile.velocity.Y < num66) {
                    Projectile.velocity.Y += num64;
                    if (Projectile.velocity.Y < 0f && num66 > 0f)
                        Projectile.velocity.Y += num64;
                }
                else if (Projectile.velocity.Y > num66) {
                    Projectile.velocity.Y -= num64;
                    if (Projectile.velocity.Y > 0f && num66 < 0f)
                        Projectile.velocity.Y -= num64;
                }
            }

            if (Main.myPlayer == Projectile.owner) {
                Rectangle rectangle = new Rectangle((int)Projectile.position.X, (int)Projectile.position.Y, Projectile.width, Projectile.height);
                Rectangle value = new Rectangle((int)Main.player[Projectile.owner].position.X, (int)Main.player[Projectile.owner].position.Y, Main.player[Projectile.owner].width, Main.player[Projectile.owner].height);
                if (rectangle.Intersects(value))
                    Projectile.Kill();
            }
        }

        Lighting.AddLight(Projectile.Center, 0.4f, 0.4f, 0.2f);
        Projectile.localAI[2]++;
        if (Projectile.localAI[2] >= 20) {
            int dust = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), 1, 1, DustID.AncientLight, 0f, 0f, 100, new Color(180, 165, 5), Main.rand.NextFloat(0.8f, 1.6f));
            Main.dust[dust].noGravity = true;
            Main.dust[dust].noLight = false;
            Main.dust[dust].velocity.Y = -1f;
            Main.dust[dust].velocity.X *= 0.1f;
            int dust2 = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y + 24), 1, 1, DustID.AncientLight, 0f, 0f, 100, new Color(180, 165, 5), Main.rand.NextFloat(0.8f, 1.6f));
            Main.dust[dust2].noGravity = true;
            Main.dust[dust2].noLight = false;
            Main.dust[dust2].velocity.Y = -1f;
            Main.dust[dust2].velocity.X *= 0.1f;
        }
    }

    public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
        width = height = 10;
        return true;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        base.OnHitNPC(target, hit, damageDone);

        Player player = Main.player[Projectile.owner];
        if (Projectile.localAI[2] >= 35) {
            player.HealEffect(3);
            player.statLife += 3;
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        SpriteBatch spriteBatch = Main.spriteBatch;
        Texture2D texture = (Texture2D)ModContent.Request<Texture2D>(Texture);
        if (Projectile.localAI[2] >= 35) {
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
            for (int k = 0; k < Projectile.oldPos.Length; k++) {
                Vector2 drawPos = Projectile.oldPos[k] - Main.screenPosition + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
                Color color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
                spriteBatch.Draw(texture, drawPos, null, color, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0f);
            }
        }
        return true;
    }

    public override Color? GetAlpha(Color lightColor)
     => new Color(255, 255, 200, 200);
}

