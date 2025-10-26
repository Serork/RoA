using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common.Cache;
using RoA.Common.Druid.Claws;
using RoA.Common.Networking;
using RoA.Common.Networking.Packets;
using RoA.Common.VisualEffects;
using RoA.Content.Dusts;
using RoA.Content.Items.Weapons.Nature;
using RoA.Content.Items.Weapons.Nature.PreHardmode.Claws;
using RoA.Content.VisualEffects;
using RoA.Core;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using System;
using System.IO;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Nature;

class ClawsSlash : NatureProjectile {
    private float _clawsExtraScale;
    private Color? _firstSlashColor = null, _secondSlashColor = null;
    private bool _soundPlayed;

    private static Asset<Texture2D> _secondSlashTexture;

    protected Player Owner => Main.player[Projectile.owner];

    protected bool ShouldFullBright => AttachedNatureWeapon != null && AttachedNatureWeapon.IsNatureClaws(out ClawsBaseItem clawsBaseItem) && clawsBaseItem.BrightnessModifier > 0f;

    protected Color? FirstSlashColor => _firstSlashColor;
    protected Color? SecondSlashColor => _secondSlashColor;

    protected virtual bool CanFunction => Projectile.localAI[0] >= Projectile.ai[1] * 0.5f;

    public sealed override string Texture => ResourceManager.NatureProjectileTextures + nameof(ClawsSlash);

    public override void SetStaticDefaults() {
        ProjectileID.Sets.AllowsContactDamageFromJellyfish[Type] = true;

        if (!Main.dedServ) {
            _secondSlashTexture = ModContent.Request<Texture2D>(Texture + "2");
        }
    }

    protected override void SafeSetDefaults() {
        Projectile.width = Projectile.height = 0;
        Projectile.aiStyle = -1;
        Projectile.friendly = true;
        Projectile.penetrate = -1;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.localNPCHitCooldown = -1;
        Projectile.ownerHitCheck = true;
        Projectile.ownerHitCheckDistance = 300f;
        Projectile.usesOwnerMeleeHitCD = true;
        Projectile.stopsDealingDamageAfterPenetrateHits = true;
        Projectile.noEnchantmentVisuals = true;
    }

    protected override void SafeSendExtraAI(BinaryWriter writer) {
        writer.WriteRGBA(_firstSlashColor!.Value);
        writer.WriteRGBA(_secondSlashColor!.Value);
    }

    protected override void SafeReceiveExtraAI(BinaryReader reader) {
        _firstSlashColor = reader.ReadRGBA();
        _secondSlashColor = reader.ReadRGBA();
    }

    protected override void SafeOnSpawn(IEntitySource source) {
        if (Projectile.owner == Main.myPlayer) {
            var colorHandler = Owner.GetModPlayer<ClawsHandler>().SlashColors;
            _firstSlashColor = colorHandler.Item1;
            _secondSlashColor = colorHandler.Item2;

            Projectile.netUpdate = true;
        }
    }

    public sealed override void OnHitPlayer(Player target, Player.HurtInfo info) {
        Projectile.ApplyFlaskEffects(target);

        SafeOnHitPlayer(target, info);
    }

    public sealed override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        Projectile.ApplyFlaskEffects(target);

        SafeOnHitNPC(target, hit, damageDone);
    }

    protected virtual void SafeOnHitPlayer(Player target, Player.HurtInfo info) { }
    protected virtual void SafeOnHitNPC(NPC target, NPC.HitInfo info, int damageDone) { }

    public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) {
        if (Main.player[Projectile.owner].ownedProjectileCounts[ModContent.ProjectileType<InfectedWave>()] > 0 ||
            Main.player[Projectile.owner].ownedProjectileCounts[ModContent.ProjectileType<HemorrhageWave>()] > 0) {
            modifiers.Knockback *= 0f;
        }

        SpawnVisualHitEffect(target);
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
        if (Main.player[Projectile.owner].ownedProjectileCounts[ModContent.ProjectileType<InfectedWave>()] > 0 ||
            Main.player[Projectile.owner].ownedProjectileCounts[ModContent.ProjectileType<HemorrhageWave>()] > 0) {
            modifiers.Knockback *= 0f;
        }

        SpawnVisualHitEffect(target);
    }

    protected virtual void SpawnVisualHitEffect(Entity target) {
        var selectedClaws = Owner.GetSelectedItem().As<ClawsBaseItem>();
        if (FirstSlashColor != null && SecondSlashColor != null) {
            float angle = MathHelper.PiOver2;
            Vector2 offset = new(0.2f);
            Vector2 velocity = 1.5f * offset;
            Vector2 position = Main.rand.NextVector2Circular(4f, 4f) * offset;
            Color color = Lighting.GetColor(Projectile.Center.ToTileCoordinates()).MultiplyRGB(GetSlashColor());
            if (ShouldFullBright) {
                color = Color.Lerp(color, GetSlashColor(), selectedClaws.BrightnessModifier);
            }
            color.A = 25;
            if (!ShouldFullBright) {
                Point pos = Projectile.Center.ToTileCoordinates();
                float brightness = MathHelper.Clamp(Lighting.Brightness(pos.X, pos.Y), 0.8f, 1f);
                color *= brightness;
            }
            color *= selectedClaws.HitEffectOpacity;
            position = target.Center + target.velocity + position + Main.rand.NextVector2Circular(target.width / 3f, target.height / 3f);
            velocity = angle.ToRotationVector2() * velocity * 0.5f;
            float scale = Projectile.scale;
            int layer = VisualEffectLayer.ABOVENPCS;
            VisualEffectSystem.New<ClawsSlashHit>(layer).
                Setup(position,
                      velocity,
                      color,
                      scale: scale).DontEmitLight = !selectedClaws.HasLighting;
            if (Main.netMode == NetmodeID.MultiplayerClient) {
                MultiplayerSystem.SendPacket(new VisualEffectSpawnPacket(VisualEffectSpawnPacket.VisualEffectPacketType.ClawsHit, Owner, layer, position, velocity, color, 1f, 0f));
            }
        }
    }

    public override bool? CanDamage() => CanFunction;

    public override void CutTiles() {
        DelegateMethods.tilecut_0 = TileCuttingContext.AttackProjectile;
        float coneLength = 55f * Projectile.scale;
        SetCollisionScale(ref coneLength);
        Utils.PlotTileLine(Projectile.Center + (Projectile.rotation - 0.7853982f).ToRotationVector2() * coneLength, Projectile.Center + (Projectile.rotation + 0.7853982f).ToRotationVector2() * coneLength, coneLength, DelegateMethods.CutTiles);
    }

    public override bool? CanCutTiles() => CanFunction;

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        float coneLength = 75f * Projectile.scale;
        SetCollisionScale(ref coneLength);
        float num1 = 0.5105088f * Projectile.ai[0];
        float maximumAngle = (float)Math.PI / 4f;
        Player player = Projectile.GetOwnerAsPlayer();
        float coneRotation = Projectile.rotation + num1;
        bool result = targetHitbox.IntersectsConeSlowMoreAccurate(Projectile.Center, coneLength, coneRotation, maximumAngle);
        return result;
    }

    protected virtual void SetCollisionScale(ref float coneLength) { }

    public override bool PreDraw(ref Color lightColor) {
        DrawItself(ref lightColor);
        return false;
    }

    protected void DrawItself(ref Color lightColor) {
        float rot = GetRotation();
        lightColor *= 2f;
        lightColor.A = 100;
        Vector2 position = Projectile.Center - Main.screenPosition;
        Asset<Texture2D> asset = TextureAssets.Projectile[Type];
        Asset<Texture2D> asset2 = _secondSlashTexture;
        Rectangle r = asset.Frame(verticalFrames: 2);
        Rectangle r2 = asset2.Frame(verticalFrames: 2);
        Vector2 origin = r.Size() / 2f;
        Vector2 origin2 = r2.Size() / 2f;
        float scale = Projectile.scale * 1.1f;
        SpriteEffects effects = Projectile.ai[0] >= 0.0 ? SpriteEffects.None : SpriteEffects.FlipVertically;
        Player player = Projectile.GetOwnerAsPlayer();
        if (player.gravDir < 0) {
            effects = Projectile.ai[0] >= 0.0 ? SpriteEffects.FlipVertically : SpriteEffects.None;
        }
        float num1 = (Projectile.localAI[0] + 0.5f) / (Projectile.ai[1] + Projectile.ai[1] * 0.5f);
        float num2 = Utils.Remap(num1, 0.0f, 0.6f, 0.0f, 1f) * Utils.Remap(num1, 0.6f, 1f, 1f, 0.0f);
        float num3 = 0.975f;
        Point tilePosition = Projectile.Center.ToTileCoordinates();
        float num4 = Utils.Remap((Lighting.GetColor(tilePosition) * 1.5f).ToVector3().Length() / (float)Math.Sqrt(3.0), 0.6f, 1f, 0.4f, 1f) * Projectile.Opacity;
        if (FirstSlashColor != null && SecondSlashColor != null) {
            Color color1 = FirstSlashColor.Value;
            Color color2 = SecondSlashColor.Value;
            if (ShouldFullBright) {
                num4 = Owner.GetSelectedItem().As<ClawsBaseItem>().BrightnessModifier;
            }
            if (!ShouldFullBright) {
                color1 = color1.MultiplyRGB(lightColor);
                color2 = color2.MultiplyRGB(lightColor);
            }

            float num12 = MathHelper.Clamp(Projectile.timeLeft / 2, 0f, 5f);
            if (CanFunction) {
                SpriteBatch spriteBatch = Main.spriteBatch;
                SpriteBatchSnapshot snapshot = SpriteBatchSnapshot.Capture(spriteBatch);
                spriteBatch.Begin(snapshot with { blendState = BlendState.NonPremultiplied }, true);
                spriteBatch.Draw(asset.Value, position, new Rectangle?(r), FirstSlashColor.Value * num4 * num2 * 0.35f, rot + (float)(Projectile.ai[0] * 0.785398185253143 * -1.0 * (1.0 - (double)num1)), origin, scale, effects, 0.0f);
                spriteBatch.Begin(snapshot with { blendState = BlendState.Additive, samplerState = SamplerState.LinearClamp }, true);

                Color shineColor = new Color(255, 200, 150);
                Color color3 = lightColor * num2 * 0.5f;
                color3.A = (byte)(color3.A * (1.0 - (double)num4));
                Color color4 = color3 * num4 * 0.5f;
                color4.G = (byte)(color4.G * (double)num4);
                color4.B = (byte)(color4.R * (0.25 + (double)num4 * 0.75));
                spriteBatch.Draw(asset.Value, position, new Rectangle?(r), color4 * 0.15f, rot + Projectile.ai[0] * 0.01f, origin, scale, effects, 0.0f);
                spriteBatch.Draw(asset.Value, position, new Rectangle?(r), shineColor * num4 * num2 * 0.3f, rot, origin, scale, effects, 0.0f);
                spriteBatch.Draw(asset.Value, position, new Rectangle?(r), color2 * num4 * num2 * 0.5f, rot, origin, scale * num3, effects, 0.0f);
                spriteBatch.Draw(asset.Value, position, new Rectangle?(asset.Frame(verticalFrames: 2, frameY: 1)), lightColor * 0.6f * num2, rot + Projectile.ai[0] * 0.01f, origin, scale, effects, 0.0f);
                spriteBatch.Draw(asset.Value, position, new Rectangle?(asset.Frame(verticalFrames: 2, frameY: 1)), lightColor * 0.5f * num2, rot + Projectile.ai[0] * -0.05f, origin, scale * 0.8f, effects, 0.0f);
                spriteBatch.Draw(asset.Value, position, new Rectangle?(asset.Frame(verticalFrames: 2, frameY: 1)), lightColor * 0.4f * num2, rot + Projectile.ai[0] * -0.1f, origin, scale * 0.6f, effects, 0.0f);
                spriteBatch.Begin(snapshot with { blendState = BlendState.Additive, samplerState = Main.DefaultSamplerState }, true);
                spriteBatch.Draw(asset.Value, position, new Rectangle?(r), color4 * 0.15f, rot + Projectile.ai[0] * 0.01f, origin, scale, effects, 0.0f);
                spriteBatch.Draw(asset.Value, position, new Rectangle?(r), color2 * num4 * num2 * 0.5f, rot, origin, scale * num3, effects, 0.0f);
                spriteBatch.Draw(asset.Value, position, new Rectangle?(r), color1 * num4 * num2, rot + (float)(Projectile.ai[0] * 0.785398185253143 * -1.0 * (1.0 - (double)num1)), origin, scale, effects, 0.0f);
                for (int i = 0; i < 5; i++) {
                    spriteBatch.Draw(asset.Value, position + Utils.ToRotationVector2((float)(Projectile.timeLeft * 0.1 + i * Math.PI / 5.0)) * num12, new Rectangle?(r), color1 * num4 * num2 * 0.25f, rot + (float)(Projectile.ai[0] * 0.785398185253143 * -1.0 * (1.0 - (double)num1)), origin, scale, effects, 0.0f);
                }
                spriteBatch.Draw(asset.Value, position, new Rectangle?(r), color4 * 0.15f * 0.15f, rot + Projectile.ai[0] * 0.01f, origin, scale, effects, 0.0f);
                spriteBatch.Draw(asset.Value, position, new Rectangle?(r), color2 * 0.15f * num4 * num2 * 0.5f, rot, origin, scale * num3, effects, 0.0f);
                spriteBatch.Draw(asset.Value, position, new Rectangle?(r), color1 * 0.15f * num4 * num2, rot + (float)(Projectile.ai[0] * 0.785398185253143 * -1.0 * (1.0 - (double)num1)), origin, scale, effects, 0.0f);

                Color lightColor2 = ShouldFullBright ? Color.Lerp(Color.White, lightColor, 1f - num4) : lightColor;
                lightColor2 *= MathUtils.Clamp01(num4 * 3f);
                Color lightColor3 = ShouldFullBright ? Color.Lerp(Color.White, lightColor, 1f - num4) : lightColor;
                spriteBatch.Draw(asset.Value, position, new Rectangle?(asset.Frame(verticalFrames: 2, frameY: 1)), Color.Lerp(FirstSlashColor.Value, SecondSlashColor.Value, 0.333f).MultiplyRGB(lightColor2) * 0.25f * num2, rot + Projectile.ai[0] * 0.01f, origin, scale * 1.075f, effects, 0.0f);
                spriteBatch.Draw(asset.Value, position, new Rectangle?(asset.Frame(verticalFrames: 2, frameY: 1)), Color.Lerp(FirstSlashColor.Value, SecondSlashColor.Value, 0.666f).MultiplyRGB(lightColor2) * 0.25f * num2, rot + Projectile.ai[0] * -0.05f, origin, scale * 0.8f * 1.075f, effects, 0.0f);
                spriteBatch.Draw(asset.Value, position, new Rectangle?(asset.Frame(verticalFrames: 2, frameY: 1)), Color.Lerp(FirstSlashColor.Value, SecondSlashColor.Value, 0.333f).MultiplyRGB(lightColor2) * 0.25f * num2, rot + Projectile.ai[0] * -0.1f, origin, scale * 0.6f * 1.075f, effects, 0.0f);

                spriteBatch.Draw(asset.Value, position, new Rectangle?(asset.Frame(verticalFrames: 2, frameY: 1)), Color.Lerp(Color.White, FirstSlashColor.Value, 0.75f).MultiplyRGB(lightColor2) * 0.15f * num2, rot + Projectile.ai[0] * 0.01f, origin, scale * 0.575f, effects, 0.0f);

                spriteBatch.Draw(asset2.Value, position, new Rectangle?(asset2.Frame(verticalFrames: 2, frameY: 0)), Color.Lerp(Color.White, FirstSlashColor.Value, 0.75f).MultiplyRGB(lightColor2) * 0.45f * num2, rot + Projectile.ai[0] * -0.15f, origin2, scale * 1.125f, effects, 0.0f);
                spriteBatch.Draw(asset2.Value, position, new Rectangle?(asset2.Frame(verticalFrames: 2, frameY: 1)), Color.Lerp(Color.White, FirstSlashColor.Value, 0.75f).MultiplyRGB(lightColor2) * 0.45f * num2, rot + Projectile.ai[0] * -0.2f, origin2, scale * 0.6f * 1.075f, effects, 0.0f);

                spriteBatch.Begin(snapshot, true);

                if (player.gravDir < 0) {
                    rot -= MathHelper.PiOver4 * 0.3725f * Projectile.direction;
                }

                Vector2 drawpos2 = position + (rot + Utils.Remap(num2, 0f, 1f, 0f, (float)Math.PI / 4f) * Projectile.ai[0] - MathHelper.PiOver4 * 0.5f * Projectile.direction).ToRotationVector2() * ((float)asset.Width() * 0.5f - 10f) * scale;
                DrawPrettyStarSparkle(Projectile.Opacity, effects, drawpos2, (Color.Lerp(Color.White, GetSlashColor(), Main.rand.NextFloat()) with { A = 0 }).MultiplyRGB(lightColor3) * num3 * num4, Color.Lerp(FirstSlashColor.Value, SecondSlashColor.Value, 0.666f).MultiplyRGB(lightColor2), num1, 0f, 0.5f, 0.5f, 1f, 0f, new Vector2(2f, Utils.Remap(num2, 0f, 1f, 4f, 1f)) * scale, Vector2.One * scale * 1.5f);

                drawpos2 = position + (rot + Utils.Remap(num2, 0f, 1f, 0f, (float)Math.PI / 4f) * Projectile.ai[0] - MathHelper.PiOver4 * 1.25f * Projectile.direction).ToRotationVector2() * ((float)asset.Width() * 0.5f - 10f) * scale;
                DrawPrettyStarSparkle(Projectile.Opacity, effects, drawpos2, (Color.Lerp(Color.White, GetSlashColor(), Main.rand.NextFloat()) with { A = 0 }).MultiplyRGB(lightColor3) * num3 * num4, Color.Lerp(FirstSlashColor.Value, SecondSlashColor.Value, 0.666f).MultiplyRGB(lightColor2), num1, 0f, 0.5f, 0.5f, 1f, 0f, new Vector2(2f, Utils.Remap(num2, 0f, 1f, 4f, 1f)) * scale, Vector2.One * scale * 1.5f);
            }
        }
    }

    protected float GetRotation() {
        float rot = Projectile.rotation;
        Player player = Projectile.GetOwnerAsPlayer();
        if (player.gravDir < 0) {
            rot = MathHelper.TwoPi - rot;
            rot += MathHelper.PiOver4 * 0.3725f * Projectile.direction;
        }
        return rot;
    }

    protected virtual void SpawnSlashDusts(float num1, float num4) {
        //if (Main.gamePaused || !Main.instance.IsActive || Lighting.UpdateEveryFrame && !Main.rand.NextBool(4)) {
        //    return;
        //}
        Player player = Projectile.GetOwnerAsPlayer();
        float rot = GetRotation();
        var selectedClaws = Owner.GetSelectedItem().As<ClawsBaseItem>();
        if (FirstSlashColor != null && SecondSlashColor != null) {
            if (OnSlashDustSpawn(num1)) {
                float max = Projectile.ai[1] + Projectile.ai[1] * 0.5f;
                if (Projectile.localAI[0] >= Projectile.ai[1] * 0.5f && Projectile.localAI[0] < max) {
                    float startProgress = Utils.Remap(Utils.GetLerpValue(Projectile.ai[1] * 0.5f, Projectile.ai[1] * 0.7f, Projectile.localAI[0], true), 0f, 1f, 0.5f, 1f, true);
                    float endProgress = Utils.GetLerpValue(max, max * 0.75f, Projectile.localAI[0], true);
                    startProgress *= endProgress;
                    if (!Main.rand.NextChance(startProgress * 0.9f)) {
                        return;
                    }
                    Color color1 = FirstSlashColor.Value;
                    Color color2 = SecondSlashColor.Value;
                    //Point pos = Projectile.Center.ToTileCoordinates();
                    //float brightness = MathHelper.Clamp(Lighting.Brightness(pos.X, pos.Y), 0.5f, 1f);
                    //color1 *= brightness;
                    //color2 *= brightness;
                    float num12 = (Projectile.localAI[0] + 0.5f) / (Projectile.ai[1] + Projectile.ai[1] * 0.5f);
                    float num22 = Utils.Remap(num12, 0.0f, 0.6f, 0.0f, 1f) * Utils.Remap(num12, 0.6f, 1f, 1f, 0.0f);
                    float num42 = num4;
                    if (ShouldFullBright) {
                        num42 = selectedClaws.BrightnessModifier;
                    }
                    color1 *= num42 * num22;
                    color2 *= num42 * num22;

                    float offset = 0f;
                    float f = rot + (float)((double)Main.rand.NextFloatDirection() * MathHelper.PiOver2 * 0.7);
                    Vector2 rotationVector2 = (f + Projectile.ai[0] * 1.25f * MathHelper.PiOver2).ToRotationVector2();
                    Vector2 position = Projectile.Center + (f - offset).ToRotationVector2() * (float)((double)Main.rand.NextFloat() * 80.0 * Projectile.scale + 20.0 * Projectile.scale);
                    if (position.Distance(player.Center) >= 10f/*45f*/) {
                        int type = ModContent.DustType<Slash>();
                        Dust dust = Dust.NewDustPerfect(position, type, new Vector2?(rotationVector2 * player.gravDir), 0, Color.Lerp(color1, color2, Main.rand.NextFloat(0.5f, 1f) * 0.3f) * 2f, Main.rand.NextFloat(0.75f, 0.9f) * 1.3f);
                        dust.fadeIn = (float)(0.4 + (double)Main.rand.NextFloat() * 0.15);
                        dust.noLight = dust.noLightEmittence = !selectedClaws.HasLighting;
                        dust.scale *= Projectile.scale;
                        dust.scale *= Utils.GetLerpValue(0f, 1f, position.Distance(player.Center) / 50f, true) * startProgress;
                        dust.noGravity = true;
                        if (ShouldFullBright) {
                            dust.customData = num42;
                        }
                        AdjustBaseSlashDust(ref dust);
                    }
                }
            }
        }
    }

    protected virtual bool OnSlashDustSpawn(float progress) => true;
    protected virtual void AdjustBaseSlashDust(ref Dust dust) { }

    protected virtual void DrawPrettyStarSparkle(float opacity, SpriteEffects dir, Vector2 drawpos, Microsoft.Xna.Framework.Color drawColor, Microsoft.Xna.Framework.Color shineColor, float flareCounter, float fadeInStart, float fadeInEnd, float fadeOutStart, float fadeOutEnd, float rotation, Vector2 scale, Vector2 fatness) {
        Texture2D value = TextureAssets.Extra[98].Value;
        Microsoft.Xna.Framework.Color color = shineColor * opacity * 0.5f;
        color.A = 0;
        Vector2 origin = value.Size() / 2f;
        Microsoft.Xna.Framework.Color color2 = drawColor * 0.5f;
        float num = Utils.GetLerpValue(fadeInStart, fadeInEnd, flareCounter, clamped: true) * Utils.GetLerpValue(fadeOutEnd, fadeOutStart, flareCounter, clamped: true);
        num *= 0.95f;
        Vector2 vector = new Vector2(fatness.X * 0.5f, scale.X) * num * 1f;
        Vector2 vector2 = new Vector2(fatness.Y * 0.5f, scale.Y) * num * 1f;
        color *= num;
        color2 *= num;
        Main.EntitySpriteDraw(value, drawpos, null, color, (float)Math.PI / 2f + rotation, origin, vector, dir);
        Main.EntitySpriteDraw(value, drawpos, null, color, 0f + rotation, origin, vector2, dir);
        Main.EntitySpriteDraw(value, drawpos, null, color2, (float)Math.PI / 2f + rotation, origin, vector * 0.6f, dir);
        Main.EntitySpriteDraw(value, drawpos, null, color2, 0f + rotation, origin, vector2 * 0.6f, dir);
    }

    //public override void PostAI() {
    //    Player player = Owner;

    //    float value = 1f - Projectile.localAI[0] / Projectile.ai[1];
    //    value = Ease.ExpoInSinOut(value);
    //    if (value < 0.25)
    //        player.bodyFrame.Y = player.bodyFrame.Height * 3;
    //    else if (value < 0.5)
    //        player.bodyFrame.Y = player.bodyFrame.Height * 2;
    //    else if (value < 0.75)
    //        player.bodyFrame.Y = player.bodyFrame.Height * 1;
    //    else
    //        player.bodyFrame.Y = 0;
    //}

    protected virtual void UpdateMainCycle() {
        Projectile.localAI[0] += 1f;
        Update();
    }

    protected virtual void Update(float extraRotation = 0f) {
        Player player = Owner;
        float fromValue = Projectile.localAI[0] / Projectile.ai[1];
        float num1 = Projectile.ai[0];
        float num2 = 0.2f;
        float num3 = 1f;

        float rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver4 / 2f * Math.Sign(Projectile.velocity.X) + extraRotation;
        Projectile.rotation = (float)(MathHelper.Pi * (double)num1 * (double)fromValue + (double)rotation + (double)num1 * MathHelper.Pi) + player.fullRotation;

        Projectile.scale = num3 + fromValue * num2;
        Projectile.scale *= _clawsExtraScale;

        ClawsBaseItem selectedClaws = Owner.GetSelectedItem().As<ClawsBaseItem>();
        bool hasLighting = selectedClaws.HasLighting;
        if (hasLighting) {
            Lighting.AddLight(Projectile.Center, GetLightingColor().ToVector3() * 0.75f);
        }

        if (CanFunction) {
            for (float i = -MathHelper.PiOver4; i <= MathHelper.PiOver4; i += MathHelper.PiOver2) {
                Vector2 position = Projectile.Center + (Projectile.rotation * player.gravDir + i).ToRotationVector2() * 60f * Projectile.scale;
                if (hasLighting) {
                    Lighting.AddLight(position, GetLightingColor().ToVector3() * 0.75f);
                }
                Rectangle rectangle = Utils.CenteredRectangle(position, new Vector2(55f * Projectile.scale, 55f * Projectile.scale));
                Projectile.EmitEnchantmentVisualsAtForNonMelee(rectangle.TopLeft(), rectangle.Width, rectangle.Height);
            }
        }

        num1 = (Projectile.localAI[0] + 0.5f) / (Projectile.ai[1] + Projectile.ai[1] * 0.5f);
        num2 = Utils.Remap(num1, 0.0f, 0.6f, 0.0f, 1f) * Utils.Remap(num1, 0.6f, 1f, 1f, 0.0f);
        num3 = 0.975f;
        Point tilePosition = Projectile.Center.ToTileCoordinates();
        float num4 = Utils.Remap((Lighting.GetColor(tilePosition) * 1.5f).ToVector3().Length() / (float)Math.Sqrt(3.0), 0.6f, 1f, 0.4f, 1f) * Projectile.Opacity;
        SpawnSlashDusts(num1, num4);
    }

    protected virtual Color GetLightingColor() {
        Color color = Color.Transparent;
        if (FirstSlashColor != null && SecondSlashColor != null) {
            ClawsBaseItem selectedClaws = Owner.GetSelectedItem().As<ClawsBaseItem>();
            color = Lighting.GetColor(Projectile.Center.ToTileCoordinates()).MultiplyRGB(GetSlashColor());
            if (ShouldFullBright) {
                color = Color.Lerp(color, GetSlashColor(), selectedClaws.BrightnessModifier);
            }
        }
        return color;
    }

    protected virtual Color GetSlashColor(float progress = 0f) {
        if (FirstSlashColor != null && SecondSlashColor != null) {
            return Color.Lerp(FirstSlashColor.Value, SecondSlashColor.Value, progress == 0f ? Main.rand.NextFloat() : MathUtils.Clamp01(progress));
        }

        return Color.Transparent;
    }

    public override void AI() {
        Player player = Owner;
        if (!_soundPlayed) {
            _soundPlayed = true;
            SoundEngine.PlaySound(new SoundStyle(ResourceManager.ItemSounds + "ClawsAttack"), player.Center);
        }

        player.ChangeDir((int)Projectile.ai[0]);

        if (player.inventory[player.selectedItem].ModItem is not ClawsBaseItem clawsBaseItem) {
            Projectile.Kill();

            return;
        }

        if (_clawsExtraScale == 0f) {
            _clawsExtraScale = 1f;
            float scale = Main.player[Projectile.owner].CappedMeleeOrDruidScale();
            GetExtraSizeByAttackType(ref scale, clawsBaseItem);
            if (scale != 1f) {
                SetScaleOnSpawn(ref scale);
                _clawsExtraScale *= scale;
                Projectile.scale *= scale;
            }
        }

        float fromValue = 1f - Projectile.localAI[0] / Projectile.ai[1] * 0.9f;
        player.itemAnimation = player.itemTime = (int)(Projectile.ai[1] * fromValue);

        Projectile.Center = player.RotatedRelativePoint(player.MountedCenter) - Projectile.velocity + 
            Vector2.UnitX * player.direction * (Owner.GetClawsHandler().AttackCount * 20f);

        UpdateMainCycle();

        if (Projectile.localAI[0] < (double)(Projectile.ai[1] + Projectile.ai[1] * 0.3f)) {
            return;
        }
        Projectile.Kill();
    }

    protected virtual void SetScaleOnSpawn(ref float scale) { }

    protected virtual void GetExtraSizeByAttackType(ref float scale, ClawsBaseItem clawsBaseItem) {
        ClawsHandler.ClawsAttackType clawsAttackType = Owner.GetClawsHandler().AttackType;
        switch (clawsAttackType) {
            case ClawsHandler.ClawsAttackType.Back:
                scale *= clawsBaseItem.FirstAttackScaleModifier;
                break;
            case ClawsHandler.ClawsAttackType.Front:
                scale *= clawsBaseItem.SecondAttackScaleModifier;
                break;
            case ClawsHandler.ClawsAttackType.Both:
                scale *= clawsBaseItem.ThirdAttackScaleModifier;
                break;
        }
    }
}
