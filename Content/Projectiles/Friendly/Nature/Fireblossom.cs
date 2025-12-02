using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common;
using RoA.Common.Cache;
using RoA.Common.Druid.Wreath;
using RoA.Content.Buffs;
using RoA.Content.Items.Equipables.Wreaths;
using RoA.Core;
using RoA.Core.Utility;
using RoA.Core.Utility.Vanilla;

using System;
using System.IO;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Graphics.Renderers;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class FireblossomExplosion : NatureProjectile {
    public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.Explosives}";

    protected override void SafeSetDefaults() {
        int width = 150; int height = 150;
        Projectile.Size = new Vector2(width, height);

        Projectile.penetrate = -1;
        Projectile.hostile = false;

        Projectile.friendly = true;

        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;

        Projectile.timeLeft = 25;

        Projectile.alpha = 255;

        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;

        ShouldChargeWreathOnDamage = false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        if (target.immortal) {
            return;
        }
        Player player = Main.player[Projectile.owner];
        WreathHandler handler = player.GetWreathHandler();
        int type = ModContent.ProjectileType<Fireblossom>();
        if (handler.IsFull1
            && player.GetModPlayer<FenethsBlazingWreath.FenethsBlazingWreathHandler>().IsEffectActive
            && target.FindBuffIndex(ModContent.BuffType<Buffs.Fireblossom>()) == -1 &&
            player.ownedProjectileCounts[type] < 10) {
            foreach (Projectile projectile in Main.ActiveProjectiles) {
                if (projectile.owner == Projectile.owner && projectile.type == type && projectile.ai[0] == target.whoAmI) {
                    return;
                }
            }
            if (player.whoAmI == Main.myPlayer) {
                Vector2 center = target.Center + (new Vector2(Projectile.ai[0], Projectile.ai[1]) - target.Center).SafeNormalize(Vector2.Zero) * target.width / 2f;
                int projectile2 = Projectile.NewProjectile(target.GetSource_OnHit(target), target.Center, Vector2.Zero, type, Projectile.damage, Projectile.knockBack,
                    Projectile.owner, target.whoAmI, center.X, center.Y);
            }
        }
    }
}

sealed class Fireblossom : NatureProjectile {
    private static Asset<Texture2D> _glowTexture = null!;

    private Vector2 _position;

    public override void SetStaticDefaults() {
        Main.projFrames[Projectile.type] = 3;

        ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true;

        if (Main.dedServ) {
            return;
        }

        _glowTexture = ModContent.Request<Texture2D>(Texture + "_Glow");
    }

    public override void Load() {
        On_LegacyPlayerRenderer.DrawPlayerFull += On_LegacyPlayerRenderer_DrawPlayerFull;
    }

    private void On_LegacyPlayerRenderer_DrawPlayerFull(On_LegacyPlayerRenderer.orig_DrawPlayerFull orig, LegacyPlayerRenderer self, Terraria.Graphics.Camera camera, Player drawPlayer) {
        orig(self, camera, drawPlayer);

        Player player = drawPlayer;
        foreach (Projectile projectile in Main.ActiveProjectiles) {
            if (projectile.type != ModContent.ProjectileType<Fireblossom>()) {
                continue;
            }
            if ((int)projectile.ai[0] == player.whoAmI) {
                SpriteBatch spriteBatch = camera.SpriteBatch;
                SamplerState samplerState = camera.Sampler;
                if (drawPlayer.mount.Active && drawPlayer.fullRotation != 0f) {
                    samplerState = LegacyPlayerRenderer.MountedSamplerState;
                }
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, samplerState, DepthStencilState.None, camera.Rasterizer, null, camera.GameViewMatrix.TransformationMatrix);
                Main.instance.DrawProjDirect(projectile);
                spriteBatch.End();
            }
        }
    }

    protected override void SafeSetDefaults() {
        int width = 28; int height = 24;
        Projectile.Size = new Vector2(width, height);

        Projectile.penetrate = -1;
        Projectile.hostile = false;

        Projectile.friendly = true;

        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;

        Projectile.timeLeft = int.MaxValue;

        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;

        ShouldChargeWreathOnDamage = false;
    }

    private float Speed => MathHelper.Clamp(Projectile.localAI[2], 0f, 1.4f);

    private bool CanExplode => Projectile.ai[2] == 1f;

    protected override void SafeSendExtraAI(BinaryWriter writer) {
        base.SafeSendExtraAI(writer);

        writer.WriteVector2(_position);
    }

    protected override void SafeReceiveExtraAI(BinaryReader reader) {
        base.SafeReceiveExtraAI(reader);

        _position = reader.ReadVector2();
    }

    public void SetPosition(Vector2 position) {
        _position = position;
        Vector2 center = Main.player[Projectile.owner].Center;
        bool flag = (int)Projectile.ai[0] == Projectile.owner;
        Entity entity = flag ? Main.player[(int)Projectile.ai[0]] : Main.npc[(int)Projectile.ai[0]];
        Projectile.rotation = Helper.VelocityAngle((entity.Center - center).SafeNormalize(Vector2.Zero)) + (flag ? MathHelper.PiOver2 : MathHelper.Pi);
        _position -= entity.Center;
    }

    public override bool ShouldUpdatePosition() => false;

    public override void AI() {
        if (Projectile.localAI[0] <= 0f) {
            Projectile.localAI[0] = 1f;

            SetPosition(new Vector2(Projectile.ai[1], Projectile.ai[2]));

            Projectile.frame = Main.rand.Next(3);
            Projectile.spriteDirection = Projectile.direction = 1 - Main.rand.Next(2) == 0 ? 2 : 0;

            Projectile.ai[1] = Projectile.ai[2] = 0f;
            Projectile.velocity = Vector2.Zero;
        }

        bool flag = (int)Projectile.ai[0] == Projectile.owner;
        NPC targetNPC = flag ? null : Main.npc[(int)Projectile.ai[0]];
        Player targetPlayer = !flag ? null : Main.player[(int)Projectile.ai[0]];
        int onFireForEnemiesType = ModContent.BuffType<OnFire>();
        if (Main.player[Projectile.owner].GetWreathHandler().IsFull1) {
            if (Projectile.ai[2] == 0f) {
                float num2 = (float)Main.rand.Next(90, 180) * 0.025f;
                int time = (int)(60f * num2 * 2f);
                if (flag) {
                    targetPlayer.AddBuff(ModContent.BuffType<OnFire>(), time);
                }
                else {
                    targetNPC.AddBuff(onFireForEnemiesType, time);
                }
                Projectile.ai[2] = 1f;
            }

            Projectile.ai[2] = 1f;
        }
        else {
            if (Projectile.ai[2] == 0f) {
                float num2 = (float)Main.rand.Next(90, 180) * 0.025f;
                int time = (int)(60f * num2 * 2f);
                if (flag) {
                    targetPlayer.AddBuff(ModContent.BuffType<OnFire>(), time);
                }
                else {
                    targetNPC.AddBuff(onFireForEnemiesType, time);
                }
                Projectile.ai[2] = time;
            }
        }
        if (Projectile.localAI[2] >= (!CanExplode ? Projectile.ai[2] * 0.015f : 2f)) {
            if (CanExplode) {
                Projectile.Kill();
            }
            else if (Projectile.Opacity > 0f) {
                Projectile.Opacity -= 0.1f;
            }
            else {
                Projectile.Kill();
            }
        }
        else {
            Projectile.localAI[2] += TimeSystem.LogicDeltaTime;

            Projectile.Opacity = Projectile.timeLeft > int.MaxValue - 40 ? (float)(int.MaxValue - Projectile.timeLeft) / 40f : 1f;
        }
        if (!flag) {
            if (!targetNPC.HasBuff(onFireForEnemiesType)) {
                Projectile.localAI[2] = !CanExplode ? Projectile.ai[2] * 0.015f : 2f;
            }
            targetNPC.AddBuff(ModContent.BuffType<Buffs.Fireblossom>(), 180);
            Vector2 center = targetNPC.Center + _position + new Vector2(0f, targetNPC.gfxOffY);
            while (!targetNPC.getRect().Contains(center.ToPoint())) {
                center += (targetNPC.Center - center).SafeNormalize(Vector2.Zero);
            }
            Projectile.Center = new Vector2((int)center.X, (int)center.Y);
        }
        else {
            if (!targetPlayer.HasBuff(ModContent.BuffType<OnFire>())) {
                Projectile.localAI[2] = !CanExplode ? Projectile.ai[2] * 0.015f : 2f;
            }
            targetPlayer.onFire = false;
            Projectile.Center = new Vector2((int)targetPlayer.Center.X, (int)targetPlayer.Center.Y) + new Vector2(0f, targetPlayer.gfxOffY);
        }
        float rate = (float)(Speed % 5.0);
        Color color = Color.Lerp(Color.Orange, Color.DarkOrange, Ease.QuartOut(MathHelper.Clamp(1f - (rate - 0.5f) / 0.5f, 0f, 1f))) * 0.75f;
        if (CanExplode) {
            float value = Projectile.ai[1];
            value *= Projectile.Opacity;
            Lighting.AddLight(Projectile.Center, new Vector3(color.R / 255f * value, color.G / 255f * value, color.B / 255f * value));
        }
        else {
            float value = Projectile.Opacity;
            Lighting.AddLight(Projectile.Center, new Vector3(color.R / 255f * value, color.G / 255f * value, color.B / 255f * value));
        }
        if (Projectile.localAI[0] >= 1f) {
            if (Projectile.localAI[1] <= 1f)
                Projectile.localAI[1] += 0.01f;
            if (!flag && (targetNPC.life <= 0 || !targetNPC.active)) Projectile.Kill();
            if (flag && (targetPlayer.dead || !targetPlayer.active)) {
                Projectile.Kill();
            }
            Projectile.ai[1] = FireblossomWave(0f, 1f, speed: Speed) * Projectile.localAI[1];
            if (!flag) {
                Projectile.scale = targetNPC.scale;
            }
            if (CanExplode) {
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
                if (Main.rand.NextBool(3)) {
                    int dust1 = Dust.NewDust(new Vector2(Projectile.position.X + Main.rand.NextFloat(0f, (float)Projectile.width * Projectile.scale), Projectile.Center.Y - (float)(Projectile.height / 2)), 4, 4, Main.rand.Next(2) == 0 ? ModContent.DustType<Dusts.Fireblossom>() : DustID.Asphalt);
                    Dust dust2 = Main.dust[dust1];
                    dust2.velocity = Vector2.One.RotatedByRandom(MathHelper.TwoPi) * Main.rand.NextFloat(2f);
                    dust2.scale = (1f * (5f - dust2.velocity.Y)) / 5f * Projectile.scale;
                    dust2.alpha = dust2.type == ModContent.DustType<Dusts.Fireblossom>() ? 255 - (int)MathHelper.Lerp(25f, 255f, Projectile.ai[1]) : Main.rand.Next(25, 101);
                    dust2.noGravity = true;
                    dust2.noLight = true;
                }
            }
        }
        else {
            Projectile.localAI[0] += 0.025f;
            Projectile.alpha = 255 - (int)(255 * Projectile.localAI[0]);
        }
    }

    private float FireblossomWave(float minimum, float maximum, float speed = 1f, float offset = 0f) => Helper.Wave((float)(Speed % 5.0), minimum, maximum, speed, offset);

    public override void OnKill(int timeLeft) {
        bool flag = (int)Projectile.ai[0] == Projectile.owner;
        NPC targetNPC = flag ? null : Main.npc[(int)Projectile.ai[0]];
        Player targetPlayer = !flag ? null : Main.player[(int)Projectile.ai[0]];
        if (flag) {
            targetPlayer.ClearBuff(ModContent.BuffType<OnFire>());
        }
        else {
            targetNPC.ClearBuff(ModContent.BuffType<OnFire>());
        }
        if (CanExplode) {
            Projectile.localAI[0] = 0f;
            Projectile.localAI[1] = 0f;
            if (Projectile.owner == Main.myPlayer) {
                Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<FireblossomExplosion>(), Projectile.damage, Projectile.knockBack, Projectile.owner,
                    Projectile.Center.X,
                    Projectile.Center.Y);
            }
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
    }

    public override void PostDraw(Color lightColor) {
        if (Projectile.ai[2] == 0f) {
            return;
        }
        Asset<Texture2D> glowMaskTexture = _glowTexture;
        Texture2D texture = glowMaskTexture.Value;
        SpriteBatch sb = Main.spriteBatch;
        Vector2 offset = new((float)(Projectile.width * 0.5f), (float)(Projectile.height * 0.5f));
        SpriteEffects effects = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
        SpriteFrame frame = new(1, 3);
        Rectangle rectangle = frame.GetSourceRectangle(texture);
        int height = texture.Height / 3;
        rectangle.Y = height * Projectile.frame;
        float scale = Projectile.scale;
        sb.Draw(glowMaskTexture.Value, Projectile.Center - new Vector2(0f, Projectile.height) + new Vector2(0f, (float)Projectile.height) - Main.screenPosition,
            rectangle,
            Color.White * 0.5f
    * Projectile.Opacity, Projectile.rotation, offset, scale, effects, 0);
        if (CanExplode) {
            SpriteBatchSnapshot snapshot = sb.CaptureSnapshot();
            sb.Begin(snapshot with { blendState = BlendState.Additive, samplerState = SamplerState.LinearWrap }, true);
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
            sb.Begin(snapshot, true);
        }
    }

    public override bool? CanDamage() => false;
}
