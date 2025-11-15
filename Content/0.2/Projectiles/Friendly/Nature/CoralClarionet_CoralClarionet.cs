using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common;
using RoA.Common.Players;
using RoA.Common.Projectiles;
using RoA.Content.Dusts;
using RoA.Core;
using RoA.Core.Defaults;
using RoA.Core.Graphics.Data;
using RoA.Core.Utility;
using RoA.Core.Utility.Vanilla;

using System;
using System.Collections.Generic;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class CoralClarionet : NatureProjectile_NoTextureLoad, IRequestAssets {
    private static float SPAWNTIMEINTICKS => 10f;
    private static float ATTACKTIME => 20f;

    public enum CoralClarionetRequstedTextureType : byte {
        Base,
        Part1,
        Part2,
        Part3,
        Part4,
        Water
    }

    (byte, string)[] IRequestAssets.IndexedPathsToTexture =>
        [((byte)CoralClarionetRequstedTextureType.Base, ResourceManager.NatureProjectileTextures + "CoralClarionet"),
         ((byte)CoralClarionetRequstedTextureType.Part1, ResourceManager.NatureProjectileTextures + "CoralClarionet_1"),
         ((byte)CoralClarionetRequstedTextureType.Part2, ResourceManager.NatureProjectileTextures + "CoralClarionet_2"),
         ((byte)CoralClarionetRequstedTextureType.Part3, ResourceManager.NatureProjectileTextures + "CoralClarionet_3"),
         ((byte)CoralClarionetRequstedTextureType.Part4, ResourceManager.NatureProjectileTextures + "CoralClarionet_4"),
         ((byte)CoralClarionetRequstedTextureType.Water, ResourceManager.NatureProjectileTextures + "CoralClarionet_Water")];

    public ref float WaveValue => ref Projectile.localAI[0];
    public ref float WaveValue2 => ref Projectile.localAI[1];
    public ref float DesiredRotation => ref Projectile.localAI[2];

    public ref float SpawnValue => ref Projectile.ai[0];
    public ref float AttackValue => ref Projectile.ai[1];
    public ref float AttackSpawnTypeValue => ref Projectile.ai[2];

    public float Opacity => Utils.GetLerpValue(0f, 0.25f, SpawnValue / SPAWNTIMEINTICKS, true);

    protected override void SafeSetDefaults() {
        Projectile.SetSizeValues(10);

        Projectile.aiStyle = -1;
        Projectile.tileCollide = false;

        Projectile.friendly = true;
    }

    public override bool? CanDamage() => false;
    public override bool? CanCutTiles() => false;

    public override void AI() {
        WaveValue2 = (float)(Projectile.whoAmI + Main.timeForVisualEffects) * TimeSystem.LogicDeltaTime;
        WaveValue = Projectile.whoAmI + (float)(Main.timeForVisualEffects / 10 % MathHelper.TwoPi);

        Player owner = Projectile.GetOwnerAsPlayer();
        owner.SyncMousePosition();
        Vector2 mousePosition = owner.GetViableMousePosition();
        if (SpawnValue == 0f) {
            Projectile.Center = mousePosition + Vector2.UnitY * 50f;
        }

        SpawnValue = Helper.Approach(SpawnValue, SPAWNTIMEINTICKS, 1f);

        ref float rotation = ref Projectile.rotation;
        float lerpValue = 0.075f;
        float desiredRotation = Projectile.Center.AngleTo(mousePosition) + MathHelper.PiOver2;
        float maxRotation = 0.1f;
        float desiredRotationValue = MathF.Abs(desiredRotation);
        if (desiredRotationValue > maxRotation) {
            desiredRotation = Utils.AngleLerp(desiredRotation, maxRotation * desiredRotation.GetDirection(), 0.75f);
        }
        float lerpValue2 = lerpValue;
        if (mousePosition.Y > Projectile.Center.Y) {
            lerpValue2 *= 0.5f;
        }
        DesiredRotation = Utils.AngleLerp(DesiredRotation, desiredRotation, lerpValue2);
        rotation = Utils.AngleLerp(rotation, DesiredRotation, lerpValue);

        if (AttackValue++ > ATTACKTIME) {
            AttackValue = 0;

            if (owner.IsLocal()) {
                AttackSpawnTypeValue = Main.rand.Next(4);
                Projectile.netUpdate = true;

                Vector2 position = Projectile.Center;
                switch (AttackSpawnTypeValue) {
                    case 0:
                        position += new Vector2(36, -90).RotatedBy(rotation);
                        break;
                    case 1:
                        position += new Vector2(-12, -118).RotatedBy(rotation);
                        break;
                    case 2:
                        position += new Vector2(36, -36).RotatedBy(rotation);
                        break;
                    case 3:
                        position += new Vector2(-32, -66).RotatedBy(rotation);
                        break;
                }
                ProjectileUtils.SpawnPlayerOwnedProjectile<CoralBubble>(new ProjectileUtils.SpawnProjectileArgs(owner, Projectile.GetSource_FromAI()) {
                    Position = position,
                    Damage = Projectile.damage,
                    KnockBack = Projectile.knockBack
                });
            }
        }

        float scale = Projectile.scale;
        Vector2 xVelocity = new Vector2(scale, scale) * 0.5f,
                yVelocity = new Vector2(scale, -scale) * 0.75f;
        ushort dustType = (ushort)DustID.Water;
        for (int i = 0; i < 10; i++) {
            if (!Main.rand.NextBool(10)) {
                continue;
            }
            float rotation2 = 0f;
            Vector2 center = Projectile.Center - Vector2.One * 3f;
            Vector2 position = center - xVelocity.RotatedBy(rotation2) * i;
            Dust dust = Dust.NewDustDirect(position - yVelocity, 0, 0, dustType);
            dust.noGravity = true;
            dust.velocity = dust.position.DirectionTo(center) * Main.rand.NextFloat(-2.5f, 5f);
            dust.alpha = Main.rand.Next(150, 225);
            if (Main.rand.NextBool()) {
                dust.scale = 0.5f;
                dust.fadeIn = Main.rand.NextFloat(1f, 2f);
            }
            if (Main.rand.NextBool()) {
                dust.velocity *= Main.rand.NextFloat(0.5f, 1f);
            }
            if (Main.rand.NextBool()) {
                dust.velocity.Y -= Main.rand.NextFloat(0.5f, 1f);
            }
            dust.velocity.X *= 1.5f;
            Dust dust2 = Dust.NewDustDirect(position + yVelocity, 0, 0, dustType);
            dust2.noGravity = true;
            dust2.velocity = dust2.position.DirectionTo(center) * Main.rand.NextFloat(-2.5f, 5f);
            dust2.alpha = Main.rand.Next(150, 225);
            if (Main.rand.NextBool()) {
                dust2.scale = 0.5f;
                dust2.fadeIn = Main.rand.NextFloat(1f, 2f);
            }
            if (Main.rand.NextBool()) {
                dust2.velocity *= Main.rand.NextFloat(0.5f, 1f);
            }
            if (Main.rand.NextBool()) {
                dust2.velocity.Y -= Main.rand.NextFloat(0.5f, 1f);
            }
            dust2.velocity.X *= 1.5f;
        }
    }

    protected override void Draw(ref Color lightColor) {
        if (!AssetInitializer.TryGetRequestedTextureAssets<CoralClarionet>(out Dictionary<byte, Asset<Texture2D>> indexedTextureAssets)) {
            return;
        }

        Texture2D baseTexture = indexedTextureAssets[(byte)CoralClarionetRequstedTextureType.Base].Value,
                  part1Texture = indexedTextureAssets[(byte)CoralClarionetRequstedTextureType.Part1].Value,
                  part2Texture = indexedTextureAssets[(byte)CoralClarionetRequstedTextureType.Part2].Value,
                  part3Texture = indexedTextureAssets[(byte)CoralClarionetRequstedTextureType.Part3].Value,
                  part4Texture = indexedTextureAssets[(byte)CoralClarionetRequstedTextureType.Part4].Value,
                  waterTexture = indexedTextureAssets[(byte)CoralClarionetRequstedTextureType.Water].Value;
        Vector2 position = Projectile.Center + Vector2.UnitY * 14f;
        Rectangle clip = part1Texture.Bounds;
        Vector2 origin = clip.BottomCenter();
        SpriteBatch batch = Main.spriteBatch;
        float spawnProgress = SpawnValue / SPAWNTIMEINTICKS;
        float opacity = Opacity;
        Color color = Color.White * opacity;
        Vector2 scale = Vector2.One * Ease.QuartOut(spawnProgress);
        scale.X *= Utils.Remap(spawnProgress * Ease.CubeOut(spawnProgress), 0f, 1f, 2f, 1f);
        scale.Y *= Utils.Remap(spawnProgress * Ease.CubeIn(spawnProgress), 0f, 1f, 0.75f, 1f);
        DrawInfo drawInfo = DrawInfo.Default with {
            Clip = clip,
            Origin = origin,
            Color = color,
            Scale = scale
        };
        SpriteFrame waterFrame = new(1, 3,  0, (byte)((WaveValue * 2) % 3));
        Rectangle waterClip = waterFrame.GetSourceRectangle(waterTexture);
        Vector2 waterOrigin = waterClip.Centered();
        Vector2 waterScale = scale * 0.875f;
        waterScale.Y *= 0.375f;
        Color waterColor = color * 0.75f;
        waterColor = waterColor.MultiplyAlpha(Helper.Wave(WaveValue2, 0.625f, 1f, 10f, Projectile.whoAmI));
        float maxRotation = 0.05f;
        float rotation = Projectile.rotation;
        DrawInfo waterDrawInfo = DrawInfo.Default with {
            Clip = waterClip,
            Origin = waterOrigin,
            Color = waterColor,
            Scale = waterScale,
            Rotation = rotation * 0.5f
        };
        Vector2 waterPosition = position;
        waterPosition.Y -= 20f;
        waterPosition.X -= 2f;
        batch.Draw(waterTexture, waterPosition, waterDrawInfo);
        for (float num5 = 0f; num5 < 1f; num5 += 0.25f) {
            Color waterColor2 = color * 0.75f;
            waterColor2 = waterColor.MultiplyAlpha(Helper.Wave(WaveValue2, 0.625f, 1f, 10f, Projectile.whoAmI + num5 * 5f));
            waterColor2 *= 0.5f;
            Vector2 vector2 = (num5 * ((float)Math.PI * 2f)).ToRotationVector2() * 4f * scale * MathF.Sin(WaveValue2 * 10f + num5);
            batch.Draw(waterTexture, waterPosition + vector2, waterDrawInfo with { Color = waterColor2 });
        }
        ShaderLoader.WavyShader.WaveFactor = WaveValue;
        ShaderLoader.WavyShader.StrengthX = 0.15f;
        ShaderLoader.WavyShader.StrengthY = 1.5f;
        ShaderLoader.WavyShader.DrawColor = color;
        ShaderLoader.WavyShader.Apply(batch, () => {
            drawInfo = drawInfo with { Rotation = rotation + Helper.Wave(WaveValue2, -maxRotation, maxRotation, 5f, 3f) };
            batch.Draw(baseTexture, position, drawInfo);
            drawInfo = drawInfo with { Rotation = rotation + Helper.Wave(WaveValue2, -maxRotation, maxRotation, 5f, 3f) };
            batch.Draw(part3Texture, position, drawInfo);
            drawInfo = drawInfo with { Rotation = rotation + Helper.Wave(WaveValue2, -maxRotation, maxRotation, 5f, 2.5f) };
            batch.Draw(part1Texture, position, drawInfo);
            drawInfo = drawInfo with { Rotation = rotation + Helper.Wave(WaveValue2, -maxRotation, maxRotation, 5f, 2.75f) };
            batch.Draw(part4Texture, position, drawInfo);
            drawInfo = drawInfo with { Rotation = rotation + Helper.Wave(WaveValue2, -maxRotation, maxRotation, 5f, 2.25f) };
            batch.Draw(part2Texture, position, drawInfo);
        });
    }
}
