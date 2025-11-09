using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common;
using RoA.Common.Players;
using RoA.Common.Projectiles;
using RoA.Core;
using RoA.Core.Defaults;
using RoA.Core.Graphics.Data;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;

using System.Collections.Generic;

using Terraria;

using static RoA.Content.Items.Weapons.Nature.Hardmode.Canes.GlacierCane;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class GlacierSpike : NatureProjectile_NoTextureLoad, IRequestAssets {
    private static ushort TIMELEFT => 300;
    private static float MINPROGRESS => 30f;

    private Vector2 _lerpPositionTo, _stickingPosition;
    private float _lerpRotationTo;

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

    protected override void SafeSetDefaults() {
        SetNatureValues(Projectile);

        Projectile.SetSizeValues(10);

        Projectile.aiStyle = -1;
        Projectile.tileCollide = false;
        Projectile.timeLeft = TIMELEFT;

        Projectile.friendly = true;

        Projectile.penetrate = -1;
    }

    public override void AI() {
        Projectile.timeLeft = 2;

        if (IsStickingToTarget) {
            Projectile.Center = _stickingPosition;

            return;
        }

        Player owner = Projectile.GetOwnerAsPlayer();
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

            if (CaneAttackProgress >= 1f) {
                IcicleType = GlacierSpikeType.Large;
            }
            else if (CaneAttackProgress >= 0.5f) {
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
        if (AIProgress <= timeToPrepareAttack) {
            AIProgress++;
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
            float progress = aiProgress / minProgress;
            velocity2 *= Utils.GetLerpValue(0f, 0.4f, progress, true) * Utils.GetLerpValue(1f, 0.5f, progress, true);
            Projectile.Center -= velocity2 * 4f;

            progress = 1f - progress;
            Projectile.rotation = Utils.AngleLerp(Projectile.rotation, _lerpRotationTo, lerpValue * progress);
        }
        else {
            AIProgress++;
            float progress = (aiProgress - minProgress) / (minProgress / 3.5f);
            Projectile.Center += velocity2 * MathHelper.Lerp(2.5f, 7.5f, progress);
        }
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        int size = (int)IcicleType * 10;
        if (GeometryUtils.CenteredSquare(Projectile.Center + Vector2.UnitY.RotatedBy(Projectile.rotation) * size, size).Intersects(targetHitbox)) {
            return true;
        }

        return false;
    }

    public override bool? CanDamage() => Shot && AIProgress >= MINPROGRESS;

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        if (IcicleType == GlacierSpikeType.Small) {
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
        Vector2 origin = clip.Centered();
        Color color = lightColor;
        float rotation = Projectile.rotation;
        SpriteEffects effects = Projectile.spriteDirection.ToSpriteEffects();
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
