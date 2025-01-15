using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Utilities;

using System;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Summon;

sealed class LittleFleder : ModProjectile {
    public override void SetStaticDefaults() {
        Main.projFrames[Projectile.type] = 4;
        Main.projPet[Projectile.type] = true;

        ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
        ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
        ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
    }

    public override void SetDefaults() {
        int width = 10; int height = width;
        Projectile.Size = new Vector2(width, height);

        Projectile.penetrate = -1;
        Projectile.minion = true;
        Projectile.DamageType = DamageClass.Summon;

        Projectile.friendly = true;
        Projectile.tileCollide = false;
        Projectile.minionSlots = 1;
    }

    public override bool PreAI() {
        Projectile.frameCounter++;
        if (Projectile.frameCounter > 4) {
            Projectile.frame++;
            Projectile.frameCounter = 0;
        }
        if (Projectile.frame > 3)
            Projectile.frame = 0;
        return true;
    }

    public override bool PreDraw(ref Color lightColor) {
        SpriteBatch spriteBatch = Main.spriteBatch;
        Texture2D texture = (Texture2D)ModContent.Request<Texture2D>(Texture);
        Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, texture.Height * 0.5f);
        SpriteEffects spriteEffects = (SpriteEffects)(Projectile.spriteDirection != 1).ToInt();
        Vector2 position = Projectile.Center - Main.screenPosition;
        int height = texture.Height / Main.projFrames[Projectile.type];
        Rectangle sourceRectangle = new(0, height * Projectile.frame, texture.Width, height);
        Color color = Lighting.GetColor(Projectile.Center.ToTileCoordinates()) * Projectile.Opacity;
        Vector2 origin = sourceRectangle.Size() / 2f;
        Main.EntitySpriteDraw(texture, position, sourceRectangle, color, Projectile.rotation, origin, Projectile.scale, spriteEffects);

        return false;
    }

    public override void AI() {
        Player player = Main.player[Projectile.owner];
        if (player.dead || !player.active)
            player.ClearBuff(ModContent.BuffType<Buffs.LittleFleder>());
        if (player.HasBuff(ModContent.BuffType<Buffs.LittleFleder>()))
            Projectile.timeLeft = 2;

        Projectile.rotation = Projectile.velocity.X * 0.085f;
        Projectile.rotation = MathHelper.Clamp(Projectile.rotation, -0.2f, 0.2f);
        Projectile.direction = -(Projectile.Center.X - player.Center.X).GetDirection();
        Projectile.spriteDirection = -Projectile.direction;

        Projectile.ai[0] += 1f;

        Vector2 offset = new Vector2(-MathHelper.Lerp(5f, 15f, Utils.Clamp((float)Math.Sin(Projectile.ai[0] * 0.25f), 0, 1)) * Projectile.direction).RotatedBy(MathHelper.ToRadians(Projectile.ai[0] * Projectile.direction));
        Vector2 positionTo = player.Center + new Vector2(-(35f + 50f * Projectile.minionPos) * player.direction, -25f) + Vector2.UnitY * offset.Y + Vector2.UnitX * offset.X * 0.25f;
        float distance = Vector2.Distance(Projectile.Center, positionTo);
        Vector2 dif = positionTo - Projectile.Center;
        if (dif.Length() < 0.0001f) {
            dif = Vector2.Zero;
        }
        else {
            float speed = 35f;
            if (distance < 1000f) {
                speed = MathHelper.Lerp(5f, 10f, distance / 1000f);
            }
            if (distance < 100f) {
                speed = MathHelper.Lerp(0.1f, 5f, distance / 100f);
            }
            dif.Normalize();
            dif *= speed;
        }
        float inertia = 15f;
        Projectile.velocity = (Projectile.velocity * (inertia - 1) + dif) / inertia;
        if (Projectile.velocity.Length() > 5f) {
        }
        else {
            Projectile.velocity *= (float)Math.Pow(0.99, inertia * 2.0 / inertia);
            if (distance > 50f) {
                Projectile.velocity += Projectile.DirectionTo(player.Center) * distance / 100f * 0.1f;
            }
        }

        if (distance > 2000f) {
            Projectile.Center = player.Center;
            Projectile.velocity *= 0.95f;
            Projectile.netUpdate = true;
        }
    }

    public override bool? CanDamage() => Projectile.velocity.Length() > 5f && Projectile.ai[1] > 0f;

    public override bool? CanCutTiles() => false;

    public override bool MinionContactDamage() => true;
}
