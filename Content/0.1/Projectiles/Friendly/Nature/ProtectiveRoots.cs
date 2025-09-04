using Microsoft.Xna.Framework;

using RoA.Content.Dusts.Backwoods;
using RoA.Core;
using RoA.Core.Utility;

using System;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class ProtectiveRoots : NatureProjectile {
    private float rotationTimer = 3.14f;
    private float rotationSpeed = 0.8f;

    public override void SetStaticDefaults() {
        Main.projFrames[Type] = 4;
    }

    protected override void SafeSetDefaults() {
        int width = 40; int height = width;
        Projectile.Size = new Vector2(width, height);

        Projectile.penetrate = 4;

        Projectile.tileCollide = false;
        Projectile.friendly = true;

        Projectile.ignoreWater = true;
        AIType = ProjectileID.Flames;

        DrawOffsetX = -4;
        DrawOriginOffsetY = -4;

        Projectile.alpha = 0;
        Projectile.timeLeft = 10;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.immune[Projectile.owner] = 15;
    }

    protected override void SafeOnSpawn(IEntitySource source) {
        if (Projectile.ai[2] == -1f) {
            return;
        }
        Player player = Main.player[Projectile.owner];
        float distY = Projectile.ai[2];
        Projectile.spriteDirection = player.direction;
        Vector2 pos = player.Center;
        Vector2 velocity = Helper.VelocityToPoint(pos, new Vector2(Projectile.ai[0], Projectile.ai[1]), 1f).SafeNormalize(Vector2.Zero);
        Vector2 muzzleOffset = Vector2.Normalize(new Vector2(velocity.X, velocity.Y)) * distY;
        pos += muzzleOffset;
        float rejection = (float)Math.PI / 10f;
        int projectileCount = 4;
        float betweenProjs = distY * 1.75f;
        Vector2 vector2 = new(velocity.X, velocity.Y);
        vector2.Normalize();
        vector2 *= betweenProjs;
        for (int i = 0; i < projectileCount; i++) {
            float factor = (float)i - ((float)projectileCount - 1f) / 2f;
            Vector2 newPos = vector2.RotatedBy(rejection * factor);
            Projectile.NewProjectile(source, pos.X + newPos.X, pos.Y + newPos.Y, 0f, 0f, Type, Projectile.damage, Projectile.knockBack, player.whoAmI, (float)i, Projectile.ai[2], -1f);
        }
        Projectile.Kill();
        Projectile.netUpdate = true;
    }

    public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
        return base.TileCollideStyle(ref width, ref height, ref fallThrough, ref hitboxCenterFrac);
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        float value3 = Ease.QuadIn(Math.Min(1f, Projectile.ai[1] * 0.012f));
        int width = Math.Min(40, Math.Max(14, (int)(60 * value3))); int height = width;
        Vector2 size = new(width, height);
        return Collision.CheckAABBvAABBCollision(Projectile.Center - Vector2.One * size / 2f, size, targetHitbox.Location.ToVector2(), targetHitbox.Size());
    }

    public override void AI() {
        float value3 = Ease.QuadIn(Math.Min(1f, Projectile.ai[1] * 0.012f));
        int width = Math.Min(40, Math.Max(14, (int)(60 * value3))); int height = width;
        Vector2 size = new Vector2(width, height) * 0.5f;
        if (Collision.SolidTiles(Projectile.Center - size / 2, (int)size.X, (int)size.Y)) {
            Projectile.Kill();
        }
        float value4 = Ease.CubeIn(Math.Min(1f, Projectile.ai[1] * 0.02f));
        int maxFrame = (int)Math.Min(Main.projFrames[Type] - 1, value3 * (Main.projFrames[Type] - 1));
        if (Projectile.localAI[1] == 0) {
            int timeLeft = 225;
            Projectile.timeLeft = (int)Math.Min(timeLeft, timeLeft * value4);
            Projectile.penetrate = Projectile.maxPenetrate = (maxFrame + 1) * 2;
            //Projectile.scale = Math.Min(1f, value2 * 2.5f);
            Projectile.rotation = Projectile.ai[2] / 100f * 2f * MathHelper.TwoPi * Main.player[Projectile.owner].direction;
        }
        Projectile.velocity *= 0;
        Projectile.localAI[1]++;
        float value = Math.Min(1f, Projectile.ai[2] * 0.025f);
        float time = 30f;
        switch (Projectile.ai[0]) {
            case 0:
            case 3:
                if (Projectile.localAI[1] < time * 2) {
                    Projectile.frameCounter++;
                    if (Projectile.frameCounter % 6 == 0 && Projectile.frame < maxFrame) Projectile.frame++;
                    Projectile.rotation += rotationSpeed / rotationTimer * Main.player[Projectile.owner].direction;
                    rotationTimer += 0.01f * value;
                    rotationSpeed *= 0.93f;
                }
                break;
            case 1:
            case 2:
                if (Projectile.localAI[1] < time) {
                    Projectile.frameCounter++;
                    if (Projectile.frameCounter % 6 == 0 && Projectile.frame < maxFrame) Projectile.frame++;
                    Projectile.rotation += rotationSpeed / rotationTimer * Main.player[Projectile.owner].direction;
                    rotationTimer += 0.01f * value;
                    rotationSpeed *= 0.93f;
                }
                break;
        }
    }

    public override void OnKill(int timeLeft) {
        if (Projectile.ai[2] != -1f) {
            SoundEngine.PlaySound(SoundID.Item80, Projectile.Center);
        }

        Vector2 radius = Projectile.frame switch {
            0 => new(4f, 4f),
            1 => new(8f, 8f),
            2 => new(12f, 12f),
            _ => new(16f, 16f),
        };
        for (float i = -MathHelper.Pi; i < MathHelper.Pi; i += MathHelper.PiOver4 / 3f) {
            Vector2 pos = Projectile.Center + new Vector2(0f, radius.X + 4f).RotatedBy(i);
            pos -= Vector2.One * 5f;
            int dust = Dust.NewDust(pos, 2, 2, ModContent.DustType<WoodTrash>(), 0f, 0f, 0, default(Color), 1f);
            Main.dust[dust].velocity = Helper.VelocityToPoint(pos, Projectile.Center, 1f) * Main.rand.NextFloat(0.75f, 1.25f);
            Main.dust[dust].noGravity = true;
            Main.dust[dust].scale *= Main.rand.NextFloat(0.75f, 1.25f);
            Main.dust[dust].velocity *= 0.35f + Main.rand.NextFloatRange(0.2f);
            Main.dust[dust].velocity *= 1.15f + Main.rand.NextFloatRange(0.2f);
        }
        if (Projectile.ai[2] == -1f) {
            SoundEngine.PlaySound(SoundID.Item48 with { Volume = 0.6f, Pitch = -0.2f, MaxInstances = 1 }, Projectile.Center);
            SoundEngine.PlaySound(SoundID.Dig, Projectile.Center);
        }
    }

    public override bool? CanDamage() => Projectile.alpha != 255;
}
