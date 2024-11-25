using Microsoft.Xna.Framework;

using RoA.Content.Dusts.Backwoods;
using RoA.Core.Utility;
using RoA.Utilities;

using System;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Druidic;

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

        Projectile.tileCollide = true;
        Projectile.friendly = true;

        Projectile.ignoreWater = true;
        AIType = ProjectileID.Flames;

        DrawOffsetX = -4;
        DrawOriginOffsetY = -4;

        Projectile.alpha = 0;
        Projectile.timeLeft = 450;
    }

    protected override void SafeOnSpawn(IEntitySource source) {
        if (Projectile.ai[2] == -1f) {
            return;
        }
        Player player = Main.player[Projectile.owner];
        if (player.whoAmI != Main.myPlayer) {
            return;
        }
        float distY = 100f;
        Vector2 pos = player.Center;
        Vector2 velocity = Helper.VelocityToPoint(pos, player.GetViableMousePosition(), 1f).SafeNormalize(Vector2.Zero);
        Vector2 muzzleOffset = Vector2.Normalize(new Vector2(velocity.X, velocity.Y)) * distY;
        pos += muzzleOffset;
        float rejection = (float)Math.PI / 10f;
        int projectileCount = 4;
        float betweenProjs = distY * 1.5f;
        Vector2 vector2 = new(velocity.X, velocity.Y);
        vector2.Normalize();
        vector2 *= betweenProjs;
        for (int i = 0; i < projectileCount; i++) {
            float factor = (float)i - ((float)projectileCount - 1f) / 2f;
            Vector2 newPos = vector2.RotatedBy(rejection * factor);
            Projectile.NewProjectile(source, pos.X + newPos.X, pos.Y + newPos.Y, 0f, 0f, Type, Projectile.damage, Projectile.knockBack, player.whoAmI, (float)i, 0f, -1f);
        }
        Projectile.Kill();
    }

    public override void AI() {
        if (Projectile.ai[1] == 0) Projectile.rotation = Main.rand.Next(360);
        Projectile.velocity *= 0;
        Projectile.ai[1]++;
        float time = 30f;
        switch (Projectile.ai[0]) {
            case 0:
            case 3:
                if (Projectile.ai[1] < time * 2) {
                    Projectile.frameCounter++;
                    if (Projectile.frameCounter % 6 == 0 && Projectile.frame < 3) Projectile.frame++;
                    Projectile.rotation += rotationSpeed / rotationTimer;
                    rotationTimer += 0.01f;
                    rotationSpeed *= 0.93f;
                }
                break;
            case 1:
            case 2:
                if (Projectile.ai[1] < time) {
                    Projectile.frameCounter++;
                    if (Projectile.frameCounter % 6 == 0 && Projectile.frame < 3) Projectile.frame++;
                    Projectile.rotation += rotationSpeed / rotationTimer;
                    rotationTimer += 0.01f;
                    rotationSpeed *= 0.93f;
                }
                break;
        }
    }

    public override void Kill(int timeLeft) {
        Vector2 radius = new(16f, 16f);
        for (float i = -MathHelper.Pi; i < MathHelper.Pi; i += MathHelper.PiOver4 / 3f) {
            Vector2 pos = Projectile.Center + new Vector2(0f, 16f).RotatedBy(i);
            pos -= Vector2.One * 5f;
            int dust = Dust.NewDust(pos, 2, 2, ModContent.DustType<WoodTrash>(), 0f, 0f, 0, default(Color), 1f);
            Main.dust[dust].velocity = Helper.VelocityToPoint(pos, Projectile.Center, 1f) * Main.rand.NextFloat(0.75f, 1.25f);
            Main.dust[dust].noGravity = true;
            Main.dust[dust].scale *= Main.rand.NextFloat(0.75f, 1.25f);
            Main.dust[dust].velocity *= 0.35f + Main.rand.NextFloatRange(0.2f);
            Main.dust[dust].velocity *= 1.15f + Main.rand.NextFloatRange(0.2f);
        }
        if (Main.rand.Next(3) == 0) SoundEngine.PlaySound(SoundID.Item48, Projectile.Center);
        SoundEngine.PlaySound(SoundID.Dig, Projectile.Center);
    }

    public override bool? CanDamage() => Projectile.alpha != 255;
}
