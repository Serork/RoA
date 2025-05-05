using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Content.Buffs;
using RoA.Content.Dusts;
using RoA.Core;
using RoA.Core.Utility;

using System;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Druidic;

sealed class InfectedWave : Wave {
    public override Color UsedColor() => new(66, 54, 112, 255);
    public override (int, int, int) UsedDustTypes() => (ModContent.DustType<CorruptedFoliage>(), ModContent.DustType<EbonwoodLeaves>(), DustID.CorruptPlants);

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => target.AddBuff(ModContent.BuffType<Infected>(), 180);
    public override void OnHitPlayer(Player target, Player.HurtInfo info) => target.AddBuff(ModContent.BuffType<Infected>(), 180);
}

sealed class HemorrhageWave : Wave {
    public override Color UsedColor() => new(85, 0, 15, 255);
    public override (int, int, int) UsedDustTypes() => (ModContent.DustType<CrimsonFoliage>(), ModContent.DustType<ShadewoodLeaves>(), DustID.CrimsonPlants);

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => target.AddBuff(ModContent.BuffType<Hemorrhage>(), 180);
    public override void OnHitPlayer(Player target, Player.HurtInfo info) => target.AddBuff(ModContent.BuffType<Hemorrhage>(), 180);
}

abstract class Wave : DruidicProjectile {
    public abstract Color UsedColor();
    public abstract (int, int, int) UsedDustTypes();

    public override Color? GetAlpha(Color lightColor) => UsedColor().Bright(2f).MultiplyRGB(lightColor);

    public override string Texture => ResourceManager.ProjectileTextures + "EvilBiomeClawsSpecialAttack";

    protected override void SafeSetDefaults() {
        int width = 62; int height = width;
        Projectile.Size = new Vector2(width, height);

        Projectile.penetrate = -1;
        Projectile.aiStyle = -1;

        Projectile.timeLeft = 180;
        Projectile.tileCollide = false;

        Projectile.friendly = true;
        Projectile.Opacity = 0f;

        Projectile.usesLocalNPCImmunity = true;
        Projectile.ignoreWater = true;
        Projectile.localNPCHitCooldown = 100;

        ShouldChargeWreath = false;
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        Vector2 lineEnd = Projectile.Center + Vector2.Normalize(Projectile.velocity) * Projectile.height * 2f;
        return Helper.DeathrayHitbox(Projectile.Center, lineEnd, targetHitbox, 26f);
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
        //modifiers.SourceDamage += 2f;
        modifiers.SetCrit();
    }

    protected override void SafeOnSpawn(IEntitySource source) {
        Player player = Main.player[Projectile.owner];
        if (player.whoAmI == Main.myPlayer) {
            Vector2 pointPosition = player.GetViableMousePosition();
            Vector2 velocity = Vector2.Subtract(pointPosition, player.RotatedRelativePoint(player.MountedCenter, true));
            velocity.Normalize();
            Projectile.velocity = velocity;
            Projectile.netUpdate = true;
        }
        (int, int, int) dustTypes = UsedDustTypes();
        for (int i = 0; i < Main.rand.Next(2, 4); i++) {
            int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, dustTypes.Item1, Projectile.velocity.X * 10f * Main.rand.NextFloat(0.9f, 1.2f), Projectile.velocity.Y * 5f * Main.rand.NextFloat(0.9f, 1.2f), 0, default, 1f);
            Main.dust[dust].fadeIn = 1f;
            Main.dust[dust].noGravity = false;
            Main.dust[dust].noLight = true;
            Main.dust[dust].rotation = Main.rand.NextFloat(5f);
            Main.dust[dust].velocity += player.velocity;
        }
        for (int i = 0; i < Main.rand.Next(2, 4); i++) {
            int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, dustTypes.Item2, Projectile.velocity.X * 10f * Main.rand.NextFloat(0.9f, 1.2f), Projectile.velocity.Y * 5f * Main.rand.NextFloat(0.9f, 1.2f), 0, default, Main.rand.NextFloat(0.9f, 1.2f));
            Main.dust[dust].fadeIn = 1f;
            Main.dust[dust].noGravity = true;
            Main.dust[dust].noLight = true;
            Main.dust[dust].rotation = Main.rand.NextFloat(5f);
            Main.dust[dust].velocity += player.velocity;
        }
        for (int i = 0; i < Main.rand.Next(2, 4); i++) {
            int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, dustTypes.Item3, Projectile.velocity.X * 8f * Main.rand.NextFloat(0.9f, 1.2f), Projectile.velocity.Y * 5f * Main.rand.NextFloat(0.9f, 1.2f), 0, default, Main.rand.NextFloat(0.9f, 1.2f));
            Main.dust[dust].noGravity = true;
            Main.dust[dust].noLight = true;
            Main.dust[dust].rotation = Main.rand.NextFloat(5f);
            Main.dust[dust].velocity += player.velocity;
        }
    }

    public override void AI() {
        Player player = Main.player[Projectile.owner];
        Projectile.Center = player.MountedCenter + Projectile.velocity * 50f;
        Projectile.spriteDirection = Math.Sign(Projectile.velocity.X);
        Projectile.rotation = Projectile.velocity.ToRotation() + 1.57f;
        if (Projectile.Opacity == 0f) {
            Projectile.Opacity = 1f;
        }
        player.heldProj = Projectile.whoAmI;
        if (Main.myPlayer == Projectile.owner) {
            if (player.noItems || player.CCed) {
                Projectile.Kill();
                return;
            }
        }
        if (Projectile.Opacity > 0.05f) {
            Projectile.Opacity -= 0.05f;
            Projectile.Opacity *= 0.975f;
        }
        else {
            Projectile.Kill();
        }
        //Projectile.netUpdate = true;
    }

    //public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => target.AddBuff(ModContent.BuffType<Infected>(), 180);

    public override bool PreDraw(ref Color lightColor) {
        SpriteBatch spriteBatch = Main.spriteBatch;
        Texture2D texture = (Texture2D)ModContent.Request<Texture2D>(Texture);
        Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
        Vector2 position = Projectile.Center - Main.screenPosition + Projectile.velocity * 50f;
        Color color = Projectile.GetAlpha(lightColor) * Projectile.Opacity;
        spriteBatch.BeginBlendState(BlendState.NonPremultiplied, SamplerState.PointClamp);
        spriteBatch.Draw(texture, position, null, color, Projectile.rotation, drawOrigin, 1f, SpriteEffects.None, 0f);
        spriteBatch.EndBlendState();
        spriteBatch.BeginBlendState(BlendState.Additive);
        spriteBatch.Draw(texture, position, null, color.MultiplyAlpha((float)(1.0 - (double)(Projectile.Opacity - 0.5f))), Projectile.rotation, drawOrigin, 1f, SpriteEffects.None, 0f);
        spriteBatch.EndBlendState();
        return false;
    }
}