using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Core.Utility;

using System;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class SacrificialSickle : NatureProjectile {
    private Vector2 _to;
    private float _rotation;
    private int _direction;

    public override string Texture => base.Texture + "_Projectile";

    public override void SetStaticDefaults() {
        //DisplayName.SetDefault("Sacrificial Sickle");
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5;
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

        Projectile.alpha = 255;

        Projectile.friendly = true;
    }

    public override bool PreDraw(ref Color lightColor) {
        SpriteBatch spriteBatch = Main.spriteBatch;
        Player player = Main.player[Projectile.owner];
        bool flag = player.direction != 1;
        Item _item = player.HeldItem;
        Texture2D _texture = (Texture2D)ModContent.Request<Texture2D>(base.Texture);
        Vector2 _origin = new(_texture.Width * 0.5f * (1 - player.direction), (player.gravDir == -1f) ? 0 : _texture.Height);
        int x = -(int)_origin.X;
        ItemLoader.HoldoutOrigin(player, ref _origin);
        Vector2 _offset = new(_origin.X + x, 0);
        float _rotOffset = 0.785f * player.direction;
        if (player.gravDir == -1f)
            _rotOffset -= 1.57f * player.direction;
        SpriteEffects effects = flag ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        if (player.gravDir == -1f) {
            if (player.direction == 1) {
                effects = SpriteEffects.FlipVertically;
            }
            else {
                effects = SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically;
            }
        }
        SpriteBatch _spriteBatch = Main.spriteBatch;
        Vector2 pos = player.MountedCenter;
        pos = Utils.Floor(pos) + Vector2.UnitY * player.gfxOffY;
        if (player.direction == -1) {
            pos.X -= 2f;
        }
        pos += (player.itemRotation.ToRotationVector2() * 6f * player.direction).Floor();
        _spriteBatch.Draw(_texture, pos - Main.screenPosition + new Vector2(0f, 6f) * player.gravDir + _offset, _texture.Bounds, new Color(255, 255, 200, 255) * 0.9f * (1f - Projectile.alpha / 255f), player.itemRotation + _rotOffset, _origin, _item.scale, effects, 0);

        void draw(Texture2D texture, Color lightColor, float opacity = 1f) {
            Vector2 drawOrigin = new(texture.Width * 0.5f, texture.Height * 0.5f);
            for (int k = 0; k < Projectile.oldPos.Length; k++) {
                Vector2 drawPos = Projectile.oldPos[k] - Main.screenPosition;
                Color color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length) * MathHelper.Clamp(Projectile.localAI[0] * 2f / 30f, 0f, 1f);
                color *= 1f - Projectile.localAI[2] / 25f;
                spriteBatch.Draw(texture, drawPos, null, color * opacity, Projectile.oldRot[k] - MathHelper.PiOver2, drawOrigin, Projectile.scale, SpriteEffects.FlipHorizontally, 0f);
            }
            if (Projectile.ai[0] == 2f) {
                int length = Projectile.oldPos.Length / 2 + Projectile.oldPos.Length / 5;
                for (int k = 0; k < length; k++) {
                    Vector2 drawPos = Projectile.oldPos[k] - Main.screenPosition;
                    Color color = Projectile.GetAlpha(lightColor) * ((length - k) / (float)length * 0.75f) * MathHelper.Clamp(Projectile.localAI[0] * 2f / 30f, 0f, 1f);
                    spriteBatch.Draw(texture, drawPos, null, color * opacity, Projectile.oldRot[k] - MathHelper.PiOver2, drawOrigin, Projectile.scale, SpriteEffects.FlipHorizontally, 0f);
                }
            }
            spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, Projectile.GetAlpha(lightColor) * opacity, Projectile.rotation - MathHelper.PiOver2, drawOrigin, Projectile.scale, SpriteEffects.FlipHorizontally, 0f);
        }
        Texture2D texture = (Texture2D)ModContent.Request<Texture2D>(Texture);
        float dist = player.Distance(Projectile.Center);
        draw(texture, lightColor);
        texture = (Texture2D)ModContent.Request<Texture2D>(Texture + "2");
        float value = MathHelper.Clamp(Utils.GetLerpValue(250f, 0f, dist, true) * 1.25f, 0f, 1f);
        draw(texture, lightColor);
        return false;
    }


    protected override void SafeOnSpawn(IEntitySource source) {
        base.SafeOnSpawn(source);

        Player player = Main.player[Projectile.owner];
        Vector2 pos = player.MountedCenter;
        pos = Utils.Floor(pos) + Vector2.UnitY * player.gfxOffY;
        Projectile.Center = pos;
        float randomness = Main.rand.NextFloatDirection();
        float min = 0.75f;
        if (randomness > 0f && randomness < min) {
            randomness = min;
        }
        if (randomness > -min && randomness < 0f) {
            randomness = -min;
        }
        Projectile.ai[2] = player.direction * randomness * Main.rand.NextFromList(-1, 1);
        if (Projectile.owner == Main.myPlayer) {
            _to = player.GetViableMousePosition();
        }
        Vector2 dif = _to - pos;
        int dir = dif.X.GetDirection();
        _direction = dir;
        player.itemRotation = (float)Math.Atan2(dif.Y * dir, dif.X * dir);
        player.itemRotation = MathHelper.WrapAngle(player.itemRotation);
        _rotation = player.itemRotation;
        Projectile.velocity = Helper.VelocityToPoint(player.Center, _to, 15f);
        Projectile.rotation = Helper.VelocityAngle(Projectile.velocity) + MathHelper.PiOver2 / 2f;

        for (int num615 = 0; num615 < 10; num615++) {
            int num616 = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, DustID.AncientLight, Projectile.velocity.X, Projectile.velocity.Y, 100, new Color(180, 165, 5), Main.rand.NextFloat(0.8f, 1.6f));
            Main.dust[num616].noGravity = true;
            Dust dust2 = Main.dust[num616];
            dust2.scale *= 1.25f;
            dust2 = Main.dust[num616];
            dust2.velocity *= 0.5f;
        }
    }

    public override void SafePostAI() {
        if (Projectile.alpha > 0) {
            Projectile.alpha -= 35;
        }
        else {
            Projectile.alpha = 0;
        }
        for (int num28 = Projectile.oldPos.Length - 1; num28 > 0; num28--) {
            Projectile.oldPos[num28] = Projectile.oldPos[num28 - 1];
            Projectile.oldRot[num28] = Projectile.oldRot[num28 - 1];
        }

        Projectile.oldPos[0] = Projectile.Center;
        Projectile.oldRot[0] = Projectile.rotation;
    }

    public override void OnKill(int timeLeft) {
        for (int i = 0; i < 5; i++) {
            int dust3 = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), 24, 24, DustID.AncientLight, 0f, 0f, 0, new Color(180, 165, 5), Main.rand.NextFloat(0.8f, 1.6f));
            Main.dust[dust3].noGravity = true;
            Main.dust[dust3].noLight = false;
            Main.dust[dust3].velocity.Y -= 1f;
            Main.dust[dust3].velocity.X *= 0.1f;
        }
    }

    public override void AI() {
        Lighting.AddLight(Projectile.Center, 0.4f, 0.4f, 0.2f);

        Player player = Main.player[Projectile.owner];
        player.heldProj = Projectile.whoAmI;
        player.itemTime = Projectile.timeLeft > 180 ? player.itemTimeMax : 2;
        player.itemAnimation = 2;

        player.ChangeDir(_direction);

        Projectile.localAI[0] += Projectile.velocity.Length() > 5f ? 1f : -1f;
        if (Projectile.ai[0] == 0f) {
            Projectile.velocity *= 0.975f;
            bool flag = true;

            if (flag)
                Projectile.ai[1] += 1f;

            if (Projectile.ai[1] >= 10f) {
                Projectile.ai[0] = 1f;
                Projectile.ai[1] = 0f;
                Projectile.netUpdate = true;
            }
            Vector2 pos = player.MountedCenter;
            pos = Utils.Floor(pos) + Vector2.UnitY * player.gfxOffY;
            Vector2 dif = _to - pos;
            int dir = dif.X.GetDirection();
            _direction = dir;
            player.itemRotation = (float)Math.Atan2(dif.Y * dir, dif.X * dir);
            player.itemRotation = MathHelper.WrapAngle(player.itemRotation);
            _rotation = player.itemRotation;

            Projectile.rotation = Helper.VelocityAngle(Projectile.velocity) + MathHelper.PiOver2 / 2f;
        }
        else {
            player.itemRotation = _rotation;
            Projectile.ai[1] += 0.5f;
            if (Projectile.velocity.Length() < 7f || Projectile.ai[0] == 2f) {
                Projectile.rotation += 0.3f * MathHelper.Clamp(Projectile.ai[1] / 20f, 0f, 1f) * -Projectile.ai[2];
                Projectile.ai[0] = 2f;
            }
            float num63 = 7f;
            float num64 = 0.2f;

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
                Projectile.velocity += vector8 * Math.Min(num64 / 2f, vector7.Length());
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
                if (rectangle.Intersects(value)) {
                    for (int num615 = 0; num615 < 5; num615++) {
                        int num616 = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y) - Projectile.velocity.SafeNormalize(Vector2.Zero) * 15f, Projectile.width, Projectile.height, DustID.AncientLight, Projectile.velocity.X, Projectile.velocity.Y, 100, new Color(180, 165, 5), Main.rand.NextFloat(0.8f, 1.6f));
                        Main.dust[num616].noGravity = true;
                        Dust dust2 = Main.dust[num616];
                        dust2.scale *= 1.25f;
                        dust2 = Main.dust[num616];
                        dust2.velocity *= 0.75f;
                    }
                    Projectile.Kill();
                }
            }
        }

        Lighting.AddLight(Projectile.Center, 0.4f, 0.4f, 0.2f);
        Projectile.localAI[2]++;
        if (Projectile.localAI[2] >= 20 && Projectile.velocity.Length() > 3f) {
            if (Main.rand.NextChance(0.5)) {
                int dust = Dust.NewDust(new Vector2(Projectile.Center.X, Projectile.Center.Y - 10).RotatedBy(Projectile.rotation - MathHelper.PiOver2, Projectile.Center), 1, 1, DustID.AncientLight, 0f, 0f, 100, new Color(180, 165, 5), Main.rand.NextFloat(0.8f, 1.6f));
                Main.dust[dust].noGravity = true;
                Main.dust[dust].noLight = false;
                Main.dust[dust].velocity.Y = -1f;
                Main.dust[dust].velocity.X *= 0.1f;
                Main.dust[dust].scale = 0.7f + Main.rand.NextFloat() * 1.2f;
                Main.dust[dust].fadeIn = Main.rand.NextFloat() * 1.2f * Main.rand.NextFloat(0.75f, 1f);
            }
            if (Main.rand.NextChance(0.5)) {
                int dust2 = Dust.NewDust(new Vector2(Projectile.Center.X, Projectile.Center.Y + 10).RotatedBy(Projectile.rotation - MathHelper.PiOver2, Projectile.Center), 1, 1, DustID.AncientLight, 0f, 0f, 100, new Color(180, 165, 5), Main.rand.NextFloat(0.8f, 1.6f));
                Main.dust[dust2].noGravity = true;
                Main.dust[dust2].noLight = false;
                Main.dust[dust2].velocity.Y = -1f;
                Main.dust[dust2].velocity.X *= 0.1f;
                Main.dust[dust2].scale = 0.7f + Main.rand.NextFloat() * 1.2f;
                Main.dust[dust2].fadeIn = Main.rand.NextFloat() * 1.2f * Main.rand.NextFloat(0.75f, 1f);
            }
            if (Main.rand.NextChance(0.5)) {
                int dust = Dust.NewDust(new Vector2(Projectile.Center.X, Projectile.Center.Y - 10).RotatedBy(Projectile.velocity.ToRotation() - MathHelper.PiOver2 * Main.rand.NextFloat(0.5f, 1f), Projectile.Center), 1, 1, DustID.AncientLight, 0f, 0f, 100, new Color(180, 165, 5), Main.rand.NextFloat(0.8f, 1.6f));
                Main.dust[dust].noGravity = true;
                Main.dust[dust].noLight = false;
                Main.dust[dust].velocity.Y = -1f;
                Main.dust[dust].velocity.X *= 0.1f;
                Main.dust[dust].scale = 0.7f + Main.rand.NextFloat() * 1.2f;
                Main.dust[dust].fadeIn = Main.rand.NextFloat() * 1.2f * Main.rand.NextFloat(0.75f, 1f);
            }
            if (Main.rand.NextChance(0.5)) {
                int dust2 = Dust.NewDust(new Vector2(Projectile.Center.X, Projectile.Center.Y + 10).RotatedBy(Projectile.velocity.ToRotation() - MathHelper.PiOver2 * Main.rand.NextFloat(0.5f, 1f), Projectile.Center), 1, 1, DustID.AncientLight, 0f, 0f, 100, new Color(180, 165, 5), Main.rand.NextFloat(0.8f, 1.6f));
                Main.dust[dust2].noGravity = true;
                Main.dust[dust2].noLight = false;
                Main.dust[dust2].velocity.Y = -1f;
                Main.dust[dust2].velocity.X *= 0.1f;
                Main.dust[dust2].scale = 0.7f + Main.rand.NextFloat() * 1.2f;
                Main.dust[dust2].fadeIn = Main.rand.NextFloat() * 1.2f * Main.rand.NextFloat(0.75f, 1f);
            }
        }
    }

    public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
        width = height = 10;
        return true;
    }

    public override bool OnTileCollide(Vector2 oldVelocity) {
        if (Projectile.ai[0] == 0f) {
            Projectile.ai[0] = 1f;
            Projectile.ai[1] = 0f;
            Projectile.netUpdate = true;

            SoundEngine.PlaySound(SoundID.Dig with { Pitch = Main.rand.NextFloat(0.8f, 1.2f) }, Projectile.Center);

            for (int num615 = 0; num615 < 10; num615++) {
                int num616 = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, DustID.AncientLight, Projectile.velocity.X, Projectile.velocity.Y, 100, new Color(180, 165, 5), Main.rand.NextFloat(0.8f, 1.6f));
                Main.dust[num616].noGravity = true;
                Dust dust2 = Main.dust[num616];
                dust2.scale *= 1.25f;
                dust2 = Main.dust[num616];
                dust2.velocity *= 0.5f + 0.5f * Main.rand.NextFloat();
            }
        }

        return false;
    }

    public override Color? GetAlpha(Color lightColor) => new Color(255, 255, 200, 200) * (1f - Projectile.alpha / 255f);
}

