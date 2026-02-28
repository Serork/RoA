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
using RoA.Core.Utility.Extensions;

using System;
using System.Collections.Generic;

using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

using static RoA.Content.Items.Weapons.Nature.Hardmode.Canes.GlacierCane;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class GlacierSpike : NatureProjectile_NoTextureLoad, IRequestAssets {
    private static ushort TIMELEFT => 300;
    private static float MINPROGRESS => 30f;

    private Vector2 _lerpPositionTo, _stickingPosition;
    private float _lerpRotationTo;
    private float _hitProgress;
    private bool _prepared;

    public enum GlacierSpikeType : byte {
        Small,
        Medium,
        Large
    }

    public enum GlacierSpikeRequstedTextureType : byte {
        Small,
        Medium,
        Large
    }

    (byte, string)[] IRequestAssets.IndexedPathsToTexture =>
        [((byte)GlacierSpikeRequstedTextureType.Small, ResourceManager.NatureProjectileTextures + "IcicleSmall"),
         ((byte)GlacierSpikeRequstedTextureType.Medium, ResourceManager.NatureProjectileTextures + "IcicleMedium"),
         ((byte)GlacierSpikeRequstedTextureType.Large, ResourceManager.NatureProjectileTextures + "IcicleLarge")];

    public ref float InitValue => ref Projectile.localAI[0];
    public ref float ShotValue => ref Projectile.localAI[1];
    public ref float AIProgress => ref Projectile.localAI[2];

    public ref float IcicleTypeValue => ref Projectile.ai[0];
    public ref float IsStickingToTargetValue => ref Projectile.ai[1];
    public ref float TargetWhoAmI => ref Projectile.ai[2];

    public bool Init {
        get => InitValue == 1f;
        set => InitValue = value.ToInt();
    }

    public bool Shot {
        get => ShotValue == 1f;
        set => ShotValue = value.ToInt();
    }

    public bool IsStickingToTarget {
        get => IsStickingToTargetValue == 1f;
        set => IsStickingToTargetValue = value.ToInt();
    }

    public GlacierSpikeType IcicleType {
        get => (GlacierSpikeType)IcicleTypeValue;
        set => IcicleTypeValue = Utils.Clamp((byte)value, (byte)GlacierSpikeType.Small, (byte)GlacierSpikeType.Large);
    }

    public int ParentId => (int)Projectile.ai[2];
    public int ParentUUID => Projectile.GetByUUID(Projectile.owner, ParentId);
    public Projectile Parent => Main.projectile[ParentUUID];
    public bool IsParentActive => ParentUUID != -1 && Main.projectile.IndexInRange(ParentUUID) && Parent.active;
    public GlacierCaneBase ParentAsCane => Parent.As<GlacierCaneBase>();
    public float CaneAttackProgress => ParentAsCane.AttackProgress01;
    public Vector2 CaneCorePosition => ParentAsCane.CorePosition;
    public int IcicleSize {
        get {
            int result = 0;
            switch (IcicleType) {
                case GlacierSpikeType.Medium:
                    result = 10;
                    break;
                case GlacierSpikeType.Large:
                    result = 14;
                    break;
            }
            return result;
        }
    }
    public bool IsSmall => IcicleType == GlacierSpikeType.Small;
    public bool IsMedium => IcicleType == GlacierSpikeType.Medium;
    public bool IsLarge => IcicleType == GlacierSpikeType.Large;

    public override void SetStaticDefaults() => Projectile.SetTrail(0, 6);

    protected override void SafeSetDefaults() {
        SetNatureValues(Projectile);

        Projectile.SetSizeValues(10);

        Projectile.aiStyle = -1;
        Projectile.tileCollide = false;
        Projectile.timeLeft = TIMELEFT;

        Projectile.friendly = true;

        Projectile.penetrate = -1;

        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;
    }

    public override void AI() {
        if (!Shot) {
            Projectile.timeLeft = 2;
        }

        Player owner = Projectile.GetOwnerAsPlayer();

        if (!IsSmall) {
            Projectile.localNPCHitCooldown = !IsMedium ? 3 : 5;
        }

        if (IsStickingToTarget) {
            Projectile.timeLeft = 100;

            _stickingPosition += Projectile.velocity.SafeNormalize() * 0.5f;
            Projectile.Center = _stickingPosition;

            _hitProgress = Helper.Approach(_hitProgress, 1f, TimeSystem.LogicDeltaTime * 5f);
            if (_hitProgress >= 1f) {
                Projectile.Kill();
            }

            for (int i = 0; i < (IsMedium ? 4 : 6); i++) {
                float spawnOffsetValue = IsMedium ? 15 : 20;
                float spawnOffsetValue2 = IsMedium ? 15 : 20;
                Vector2 dustOrigin = Vector2.One * 1f;
                Vector2 velocity3 = Projectile.velocity.SafeNormalize().RotatedByRandom(MathHelper.PiOver4 * Main.rand.NextFloatDirection());
                if (Main.rand.NextBool(3)) {
                    continue;
                }
                if (!Main.rand.NextBool(3)) {
                    Vector2 vector80 = velocity3.RotatedBy(1.5707963705062866) * ((float)Main.rand.NextDouble() - 0.5f) * spawnOffsetValue * Main.rand.NextFloat(1f, 2f);
                    int num746 = Dust.NewDust(Projectile.Center + velocity3 * spawnOffsetValue2 * 2.5f + vector80 - Vector2.One * 4f - dustOrigin, 8, 8, DustID.BubbleBurst_Blue, 0f, 0f,
                        Main.rand.Next(50, 100), default(Color), 1.25f + 0.25f * Main.rand.NextFloat());
                    Dust dust2 = Main.dust[num746];
                    dust2.noGravity = true;
                    Main.dust[num746].velocity = -velocity3;
                    Main.dust[num746].velocity *= Main.rand.NextFloat(0f, 5f);
                }
                if (Main.rand.NextBool(3)) {
                    continue;
                }
                if (!Main.rand.NextBool(3)) {
                    Vector2 vector80 = velocity3.RotatedBy(1.5707963705062866) * ((float)Main.rand.NextDouble() - 0.5f) * spawnOffsetValue * Main.rand.NextFloat(1f, 1.5f);
                    int num746 = Dust.NewDust(Projectile.Center + velocity3 * spawnOffsetValue2 * 2.5f + vector80 - Vector2.One * 4f - dustOrigin, 8, 8, DustID.BubbleBurst_Blue, 0f, 0f,
                        Main.rand.Next(50, 100), default(Color), 1.25f + 0.25f * Main.rand.NextFloat());
                    Dust dust2 = Main.dust[num746];
                    dust2.noGravity = true;
                    Main.dust[num746].velocity = -velocity3;
                    Main.dust[num746].velocity *= Main.rand.NextFloat(0f, 2.5f);
                }
                if (!Main.dedServ) {
                    Vector2 vector80 = velocity3.RotatedBy(1.5707963705062866) * ((float)Main.rand.NextDouble() - 0.5f) * spawnOffsetValue * Main.rand.NextFloat(1f, 1.5f);
                    if (Main.rand.NextBool(6)) {
                        velocity3 = (Projectile.rotation + MathHelper.PiOver2).ToRotationVector2().RotatedByRandom(MathHelper.PiOver4 * Main.rand.NextFloatDirection());
                        Gore.NewGore(Projectile.GetSource_FromAI(), Projectile.Center - dustOrigin + velocity3 * spawnOffsetValue2 * 2.5f + vector80 + Main.rand.RandomPointInArea(8),
                            velocity3 * Main.rand.NextFloat(-2.5f, 2.5f), $"IceGore{Main.rand.Next(3) + 1}".GetGoreType(),
                            Scale: 0.75f + 0.25f * Main.rand.NextFloat() - IsMedium.ToInt() * 0.15f);
                    }
                    if (Main.rand.NextBool(6)) {
                        velocity3 = (Projectile.rotation + MathHelper.PiOver2).ToRotationVector2().RotatedByRandom(MathHelper.PiOver4 * Main.rand.NextFloatDirection());
                        Gore.NewGore(Projectile.GetSource_FromAI(), Projectile.Center - dustOrigin + velocity3 * spawnOffsetValue2 * 2.5f + vector80 + Main.rand.RandomPointInArea(8),
                            velocity3 * Main.rand.NextFloat(-2.5f, 2.5f), $"IceGore{Main.rand.Next(3) + 1}".GetGoreType(),
                            Scale: 0.75f + 0.25f * Main.rand.NextFloat() - IsMedium.ToInt() * 0.15f);
                    }
                }
            }

            return;
        }

        owner.SyncMousePosition();
        Vector2 velocity = Projectile.DirectionTo(owner.GetWorldMousePosition());
        float lerpValue = 0.1f;

        if (!Shot) {
            _lerpRotationTo = velocity.ToRotation() - MathHelper.PiOver2;

            if (!Init) {
                Init = true;

                IcicleType = GlacierSpikeType.Small;

                Projectile.rotation = _lerpRotationTo;
            }

            if (!IsParentActive) {
                Projectile.Kill();
                return;
            }

            if (CaneAttackProgress < 0.5f) {
                float step = Utils.Remap(CaneAttackProgress, 0f, 0.5f, 0f, 1f, true);
                step = Ease.CubeIn(step);
                for (int i = 0; i < 2; i++) {
                    Vector2 corePosition = Projectile.Center;
                    float offsetX = 30 * 2f * (1f - MathHelper.Clamp(step, 0.4f, 1f));
                    float offsetY = 64 * 1.5f * (1f - MathHelper.Clamp(step, 0.4f, 1f));
                    Vector2 spawnPosition = corePosition + Main.rand.NextVector2Circular(offsetX, offsetY).RotatedBy(Projectile.rotation) * 1.5f * step * 1.5f;
                    spawnPosition -= Vector2.UnitY.RotatedBy(Projectile.rotation) * 10;
                    int dustType = Main.rand.NextBool(3) ? ModContent.DustType<Ice>() : ModContent.DustType<Ice2>();
                    float velocityFactor = MathHelper.Clamp(Vector2.Distance(spawnPosition, corePosition) / (offsetX + offsetY) / 2f, 0.25f, 1f) * 2f * Math.Max(step, 0.25f) + 0.25f;
                    Dust dust = Dust.NewDustPerfect(spawnPosition, dustType,
                        Scale: MathHelper.Clamp(velocityFactor * 1.4f, 1.2f, 1.75f));
                    dust.velocity = (corePosition - spawnPosition).SafeNormalize(Vector2.One) * velocityFactor;
                    dust.velocity *= 4f;
                    dust.velocity *= Main.rand.NextFloat(0.75f, 1f);
                    dust.noGravity = true;
                    dust.scale *= Main.rand.NextFloat(1f, 1.25f);
                    dust.alpha = Main.rand.Next(50, 100);
                    if (dust.position.Distance(corePosition) < (offsetX + offsetY) / 4f) {
                        dust.active = false;
                    }
                }
            }
            else if (CaneAttackProgress < 1f) {
                float step = Utils.Remap(CaneAttackProgress, 0.5f, 1f, 0f, 1f, true);
                step = Ease.CubeIn(step);
                for (int i = 0; i < 2; i++) {
                    Vector2 corePosition = Projectile.Center;
                    float offsetX = 42 * 2f * (1f - MathHelper.Clamp(step, 0.4f, 1f));
                    float offsetY = 86 * 1.5f * (1f - MathHelper.Clamp(step, 0.4f, 1f));
                    Vector2 spawnPosition = corePosition + Main.rand.NextVector2Circular(offsetX, offsetY).RotatedBy(Projectile.rotation) * 1f * step * 3f;
                    spawnPosition -= Vector2.UnitY.RotatedBy(Projectile.rotation) * 15;
                    int dustType = Main.rand.NextBool(3) ? ModContent.DustType<Ice>() : ModContent.DustType<Ice2>();
                    float velocityFactor = MathHelper.Clamp(Vector2.Distance(spawnPosition, corePosition) / (offsetX + offsetY) / 2f, 0.25f, 1f) * 2f * Math.Max(step, 0.25f) + 0.25f;
                    Dust dust = Dust.NewDustPerfect(spawnPosition, dustType,
                        Scale: MathHelper.Clamp(velocityFactor * 1.4f, 1.2f, 1.75f));
                    dust.velocity = (corePosition - spawnPosition).SafeNormalize(Vector2.One) * velocityFactor;
                    dust.velocity *= 5f;
                    dust.velocity *= Main.rand.NextFloat(0.75f, 1f);
                    dust.noGravity = true;
                    dust.scale *= Main.rand.NextFloat(1f, 1.25f);
                    dust.scale *= 1.25f;
                    dust.alpha = Main.rand.Next(50, 100);
                    if (dust.position.Distance(corePosition) < (offsetX + offsetY) / 4f) {
                        dust.active = false;
                    }
                }
            }

            if (CaneAttackProgress >= 1f) {
                if (!IsLarge) {
                    SoundEngine.PlaySound(SoundID.MaxMana with { Pitch = 0.5f }, Projectile.Center);
                }
                IcicleType = GlacierSpikeType.Large;
            }
            else if (CaneAttackProgress >= 0.5f) {
                if (!IsMedium) {
                    SoundEngine.PlaySound(SoundID.MaxMana with { Pitch = 0.25f }, Projectile.Center);
                }
                IcicleType = GlacierSpikeType.Medium;
            }

            Projectile.rotation = Utils.AngleLerp(Projectile.rotation, _lerpRotationTo, lerpValue);
            Projectile.velocity = velocity;

            _lerpPositionTo = CaneCorePosition + new Vector2(-100f * Projectile.direction, -100f);
            Projectile.SetDirection(Parent.direction);

            Projectile.Center = Vector2.Lerp(Projectile.Center, _lerpPositionTo, lerpValue);

            if (ParentAsCane.ShouldReleaseCane) {
                Shot = true;
            }

            return;
        }

        bool notEnoughDistance = Projectile.Distance(_lerpPositionTo) > 5f;
        float timeToPrepareAttack = MINPROGRESS / 2f;
        bool preparing = AIProgress <= timeToPrepareAttack;
        if (preparing || !_prepared) {
            Projectile.timeLeft = 80;

            if (!_prepared && !notEnoughDistance) {
                _prepared = true;
            }
            if (preparing) {
                AIProgress++;
            }
            Projectile.Center = Vector2.Lerp(Projectile.Center, _lerpPositionTo, lerpValue);
            Projectile.rotation = Utils.AngleLerp(Projectile.rotation, _lerpRotationTo, lerpValue);

            return;
        }

        AIProgress++;
        float minProgress = MINPROGRESS;
        Vector2 velocity2 = Projectile.velocity;
        _lerpRotationTo = velocity2.ToRotation() - MathHelper.PiOver2;
        float aiProgress = AIProgress - timeToPrepareAttack;
        if (aiProgress < minProgress) {
            AIProgress += 0.25f;

            float progress = aiProgress / minProgress;
            velocity2 *= Utils.GetLerpValue(0f, 0.4f, progress, true) * Utils.GetLerpValue(1f, 0.5f, progress, true);
            Projectile.Center -= velocity2 * 4f;

            progress = 1f - progress;
            Projectile.rotation = Utils.AngleLerp(Projectile.rotation, _lerpRotationTo, lerpValue * progress);

            //if (aiProgress == 5f) {
            //    SoundEngine.PlaySound(SoundID.Item28, Projectile.Center);
            //}
        }
        else {
            AIProgress++;
            float progress = (aiProgress - minProgress) / (minProgress / 3.5f);
            Vector2 velocity3 = velocity2 * MathHelper.Lerp(2.5f, 7.5f, progress);
            velocity3 = velocity3.NormalizeWithMaxLength(20f);
            Projectile.Center += velocity3;
        }

        int size = 10;
        if (!IsStickingToTarget && Projectile.timeLeft < 25
            && Collision.SolidCollision(Projectile.Center + Vector2.UnitY.RotatedBy(Projectile.rotation) * MathF.Max(2, MathF.Min(IcicleSize, 10)) * 1.75f - Vector2.One * size / 2f, size, size)) {
            DustsOnKill();

            SoundEngine.PlaySound(SoundID.Item27, Projectile.Center);

            Projectile.Kill();
        }
    }

    private void DustsOnKill() {
        Vector2 vector48 = Projectile.position;
        Vector2 vector49 = Projectile.oldVelocity;
        vector49.Normalize();
        vector48 += vector49 * 16f;
        for (int num443 = 0; num443 < 10 + (byte)(IcicleSize * 0.8f); num443++) {
            for (int i = 0; i < (byte)IcicleType + 1; i++) {
                float sizeModifier = 4f;
                int num444 = Dust.NewDust(vector48 - Vector2.One * IcicleSize * (sizeModifier / 2f), (int)(MathF.Max(4, IcicleSize) * sizeModifier), (int)(MathF.Max(4, IcicleSize) * sizeModifier), DustID.BubbleBurst_Blue, Alpha: Main.rand.Next(50, 100));
                Main.dust[num444].position = (Main.dust[num444].position + Projectile.Center) / 2f;
                Dust dust2 = Main.dust[num444];
                dust2.velocity += Projectile.oldVelocity * 0.4f;
                dust2 = Main.dust[num444];
                dust2.velocity *= 0.5f;
                Main.dust[num444].noGravity = true;
            }
            vector48 -= vector49 * 8f;
        }
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        int size = Math.Max(10, IcicleSize * 2);
        if (GeometryUtils.CenteredSquare(Projectile.Center + Vector2.UnitY.RotatedBy(Projectile.rotation) * size, size).Intersects(targetHitbox)) {
            return true;
        }

        return false;
    }

    public override bool? CanDamage() => Shot && AIProgress >= MINPROGRESS;

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        SoundEngine.PlaySound(SoundID.Item27, Projectile.Center);

        if (IsSmall) {
            Projectile.Kill();
            return;
        }

        if (IsStickingToTarget) {
            return;
        }

        IsStickingToTarget = true; 
        TargetWhoAmI = target.whoAmI;
        _stickingPosition = Projectile.Center;

        Projectile.netUpdate = true; 
    }

    public override bool ShouldUpdatePosition() => false;

    public override void OnKill(int timeLeft) {
        if (IsSmall || !IsStickingToTarget) {
            DustsOnKill();
            SoundEngine.PlaySound(SoundID.Item27, Projectile.Center);
        }
    }

    protected override void Draw(ref Color lightColor) {
        if (!AssetInitializer.TryGetRequestedTextureAssets<GlacierSpike>(out Dictionary<byte, Asset<Texture2D>> indexedTextureAssets)) {
            return;
        }

        Texture2D smallIcicleTexture = indexedTextureAssets[(byte)GlacierSpikeRequstedTextureType.Small].Value,
                  mediumIcicleTexture = indexedTextureAssets[(byte)GlacierSpikeRequstedTextureType.Medium].Value,
                  largeIcicleTexture = indexedTextureAssets[(byte)GlacierSpikeRequstedTextureType.Large].Value;
        Texture2D texture = IcicleType switch {
            GlacierSpikeType.Medium => mediumIcicleTexture,
            GlacierSpikeType.Large => largeIcicleTexture,
            _ => smallIcicleTexture,
        };
        SpriteBatch batch = Main.spriteBatch;
        Vector2 position = Projectile.Center;
        Rectangle clip = texture.Bounds;
        int height = texture.Height;
        clip.Height = (int)(height * (1f - _hitProgress));
        float rotation = Projectile.rotation;
        Vector2 extraPosition = Vector2.UnitY.RotatedBy(rotation) * (int)(height * _hitProgress) / 2f;
        position += Vector2.UnitY.RotatedBy(rotation) * (int)(height * _hitProgress) / 2f;
        Vector2 origin = clip.Centered();
        Color color = lightColor;
        SpriteEffects effects = Projectile.spriteDirection.ToSpriteEffects();

        int length = ProjectileID.Sets.TrailCacheLength[Type];
        for (int i = 0; i < length - 2; i++) {
            Vector2 vector6 = Projectile.oldPos[i];
            if (vector6 == Vector2.Zero || i == 0) {
                continue;
            }
            Color color2 = Color.SkyBlue.MultiplyRGB(color);
            Vector2 position2 = vector6 + extraPosition + Projectile.Size / 2f;
            batch.Draw(texture, position2, DrawInfo.Default with {
                Clip = clip,
                Origin = origin,
                Color = color2 * (_prepared ? 0.5f : 0.25f) * (1f - (float)i / length),
                Rotation = rotation,
                ImageFlip = effects,
            });
        }

        batch.Draw(texture, position, DrawInfo.Default with {
            Clip = clip,
            Origin = origin,
            Color = color,
            Rotation = rotation,
            ImageFlip = effects,
        });

        if (Shot) {
            return;
        }
        float scaleModifier = 1f;
        if (CaneAttackProgress < 0.5f) {
            scaleModifier += CaneAttackProgress * 0.5f;
        }
        else if (CaneAttackProgress < 1f) {
            scaleModifier += (CaneAttackProgress - 0.5f) * 0.75f;
        }
        float opacity = 0f;
        if (CaneAttackProgress < 0.5f) {
            opacity = Utils.GetLerpValue(0.35f, 0.5f, CaneAttackProgress, true);
        }
        else if (CaneAttackProgress < 1f) {
            opacity = Utils.GetLerpValue(0.85f, 1f, CaneAttackProgress, true);
        }
        color.A /= 2;
        Vector2 scale = Vector2.One * scaleModifier;
        batch.Draw(texture, position, DrawInfo.Default with {
            Clip = clip,
            Origin = origin,
            Color = color * opacity,
            Rotation = rotation,
            ImageFlip = effects,
            Scale = scale
        });
    }
}
