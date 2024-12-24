using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common;
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

sealed class Fireblossom : NatureProjectile {
    public override void SetStaticDefaults() {
        Main.projFrames[Projectile.type] = 3;
    }

    protected override void SafeSetDefaults() {
        int width = 28; int height = 24;
        Projectile.Size = new Vector2(width, height);

        Projectile.penetrate = -1;
        Projectile.hostile = false;

        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;

        Projectile.timeLeft = int.MaxValue;
    }

    private float Speed => MathHelper.Clamp(Projectile.localAI[2], 0f, 1.4f);

    public override void AI() {
        Projectile.Opacity = Projectile.timeLeft > int.MaxValue - 40 ? (float)(int.MaxValue - Projectile.timeLeft) / 40f : 1f;
        NPC targetNPC = Main.npc[(int)Projectile.ai[0]];
        if (Projectile.localAI[0] <= 0f) {
            Projectile.localAI[0] = 1f;

            Projectile.frame = Main.rand.Next(3);
            Projectile.direction = 1 - Main.rand.Next(2) == 0 ? 2 : 0;
        }
        if (Projectile.localAI[2] >= 2f) {
            Projectile.Kill();
        }
        else {
            Projectile.localAI[2] += TimeSystem.LogicDeltaTime;
        }
        Projectile.Center = targetNPC.Center + new Vector2(0f, targetNPC.gfxOffY);
        targetNPC.AddBuff(ModContent.BuffType<Buffs.Fireblossom>(), 2);
        float rate = (float)(Speed % 5.0);
        Color color = Color.Lerp(Color.Orange, Color.DarkOrange, Ease.QuartOut(MathHelper.Clamp(1f - (rate - 0.5f) / 0.5f, 0f, 1f))) * 0.75f;
        float value = Projectile.ai[1];
        value *= Projectile.Opacity;
        Lighting.AddLight(Projectile.Center, new Vector3(color.R / 255f * value, color.G / 255f * value, color.B / 255f * value));
        if (Projectile.localAI[0] >= 1f) {
            if (Projectile.localAI[1] <= 1f)
                Projectile.localAI[1] += 0.01f;
            if (targetNPC.life <= 0 || !targetNPC.active) Projectile.Kill();
            Projectile.ai[1] = FireblossomWave(0f, 1f, speed: Speed) * Projectile.localAI[1];
            Projectile.scale = targetNPC.scale;
            if (Main.rand.NextChance((double)Projectile.ai[1])) {
                int dust1 = Dust.NewDust(new Vector2(Projectile.position.X + Main.rand.NextFloat(0f, (float)Projectile.width * Projectile.scale), Projectile.Center.Y - (float)(Projectile.height / 2)), 4, 4, Main.rand.Next(2) == 0 ? ModContent.DustType<Dusts.Fireblossom>() : DustID.Asphalt);
                Dust dust2 = Main.dust[dust1];
                dust2.velocity.Y -= -dust2.velocity.Y + 0.01f;
                dust2.scale = (1f * (5f - dust2.velocity.Y)) / 5f * Projectile.scale;
                dust2.alpha = dust2.type == ModContent.DustType<Dusts.Fireblossom>() ? 255 - (int)MathHelper.Lerp(25f, 255f, Projectile.ai[1]) : Main.rand.Next(25, 101);
                dust2.noGravity = true;
                dust2.noLight = true;
            }
        }
        else {
            Projectile.localAI[0] += 0.025f;
            Projectile.alpha = 255 - (int)(255 * Projectile.localAI[0]);
        }
    }

    private float FireblossomWave(float minimum, float maximum, float speed = 1f, float offset = 0f) => Helper.Wave((float)(Speed % 5.0), minimum, maximum, speed, offset);

    public override void OnKill(int timeLeft) {
        Projectile.localAI[0] = 0f;
        Projectile.localAI[1] = 0f;
        //Projectile.NewProjectile(Projectile.GetProjectileSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<FireblossomExplosion>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
        for (int i = 0; i < 2; i++)
            SoundEngine.PlaySound(i == 0 ? SoundID.Item14 : i == 1 ? SoundID.Item69 : i == 2 ? SoundID.Item74 : SoundID.Item2, Projectile.Center);
        for (int i = 0; i < 50; i++) {
            Vector2 velocity = new(Main.rand.Next(-Projectile.width, Projectile.width + 1), Main.rand.Next(-Projectile.height, Projectile.height + 1));
            Vector2 newVelocity = new(velocity.X * 0.5f, velocity.Y * 0.5f);
            newVelocity.Normalize();
            newVelocity.X = Math.Abs(newVelocity.X) / 2f;
            newVelocity.Y = Math.Abs(newVelocity.Y) / 2f;
            Dust dust1 = Dust.NewDustDirect(Projectile.Center - new Vector2(16, 16), 32, 32, i % 3 == 0 ? ModContent.DustType<Dusts.Fireblossom>() : DustID.Asphalt, velocity.X * newVelocity.X / (float)Math.Log((double)(velocity.X * velocity.X + velocity.Y * velocity.Y)), velocity.Y * newVelocity.Y / (float)Math.Log((double)(velocity.X * velocity.X + velocity.Y * velocity.Y)), 0, default, 1.4f);
            dust1.noGravity = true;
            dust1.velocity *= 0.95f;
            Dust dust2 = Dust.NewDustDirect(Projectile.Center - new Vector2(16, 16), 32, 32, i % 3 == 0 ? ModContent.DustType<Dusts.Fireblossom>() : DustID.Asphalt, velocity.X * Projectile.width / velocity.Length() / (float)Math.Log((double)(velocity.X * velocity.X + velocity.Y * velocity.Y)), velocity.Y * Projectile.width / velocity.Length() / (float)Math.Log((double)(velocity.X * velocity.X + velocity.Y * velocity.Y)), 0, default, 1.4f);
            dust2.noGravity = true;
            dust2.velocity *= 0.95f;
        }
    }

    public override void PostDraw(Color lightColor) {
        Asset<Texture2D> glowMaskTexture = ModContent.Request<Texture2D>(Texture + "_Glow");
        Texture2D texture = glowMaskTexture.Value;
        SpriteBatch sb = Main.spriteBatch;
        Vector2 offset = new((float)(Projectile.width * 0.5f), (float)(Projectile.height * 0.5f));
        SpriteEffects effects = Projectile.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
        SpriteFrame frame = new(1, 3);
        Rectangle rectangle = frame.GetSourceRectangle(texture);
        int height = texture.Height / 3;
        rectangle.Y = height * Projectile.frame;
        float scale = Projectile.scale;
        sb.Draw(glowMaskTexture.Value, Projectile.Center - new Vector2(0f, Projectile.height) + new Vector2(0f, (float)Projectile.height) - Main.screenPosition, 
            rectangle,
            Color.White * 0.5f
    * Projectile.Opacity, Projectile.rotation, offset, scale, effects, 0);
        sb.BeginBlendState(BlendState.Additive, SamplerState.LinearWrap);
        for (float k = -3.14f; k <= 3.14f; k += 1.57f)
            sb.Draw(texture, Projectile.Center - new Vector2(0f, Projectile.height) + new Vector2(0f, (float)Projectile.height) + Utils.RotatedBy(Utils.ToRotationVector2(k), (double)Main.GlobalTimeWrappedHourly, new Vector2())
                * FireblossomWave(0f, 1.5f, speed: Speed) - Main.screenPosition, rectangle,
                (Color.White * Projectile.localAI[1]).MultiplyAlpha(MathHelper.Lerp(0f, 1f, Projectile.ai[1])).MultiplyAlpha(0.35f).MultiplyAlpha(FireblossomWave(0.25f, 0.75f, speed: Speed)) 
                * Projectile.Opacity, Projectile.rotation + Main.rand.NextFloatRange(0.1f * Projectile.ai[1]), offset, scale, effects, 0);
        for (float k = -3.14f; k <= 3.14f; k += 1.57f)
            sb.Draw(texture, Projectile.Center - new Vector2(0f, Projectile.height) + new Vector2(0f, (float)Projectile.height) + Utils.RotatedBy(Utils.ToRotationVector2(k), (double)Main.GlobalTimeWrappedHourly, new Vector2()) 
                * FireblossomWave(0.25f, 1.5f, Speed, 0.5f) - Main.screenPosition, rectangle, 
                (Color.White * Projectile.localAI[1]).MultiplyAlpha(MathHelper.Lerp(0f, 1f, Projectile.ai[1])).MultiplyAlpha(0.35f).MultiplyAlpha(FireblossomWave(0.5f, 0.75f, Speed, 0.5f)) 
                * Projectile.Opacity, Projectile.rotation + Main.rand.NextFloatRange(0.1f * Projectile.ai[1]), offset, scale, effects, 0);
        sb.EndBlendState();
    }

    public override bool? CanDamage() => false;
}
