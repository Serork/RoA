using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common;
using RoA.Common.Players;
using RoA.Common.Projectiles;
using RoA.Content.Items.Weapons.Nature;
using RoA.Core;
using RoA.Core.Defaults;
using RoA.Core.Graphics.Data;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;

using System.Collections.Generic;

using Terraria;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class GlacierSpike : NatureProjectile_NoTextureLoad, IRequestAssets {
    private static ushort TIMELEFT => 300;

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

    public ref float IcicleTypeValue => ref Projectile.ai[0];

    public bool Init {
        get => InitValue == 1f;
        set => InitValue = value.ToInt();
    }

    public GlacierSpikeType IcicleType {
        get => (GlacierSpikeType)IcicleTypeValue;
        set => IcicleTypeValue = Utils.Clamp((byte)value, (byte)GlacierSpikeType.Small, (byte)GlacierSpikeType.Large);
    }

    public int ParentId => (int)Projectile.ai[2];
    public int ParentUUID => Projectile.GetByUUID(Projectile.owner, ParentId);
    public Projectile Parent => Main.projectile[ParentUUID];
    public bool IsParentActive => ParentUUID != -1 && Main.projectile.IndexInRange(ParentUUID) && Parent.active;
    public CaneBaseProjectile ParentAsCane => Parent.As<CaneBaseProjectile>();
    public float CaneAttackProgress => ParentAsCane.AttackProgress01;
    public Vector2 CaneCorePosition => ParentAsCane.CorePosition;

    protected override void SafeSetDefaults() {
        SetNatureValues(Projectile);

        Projectile.SetSizeValues(10);

        Projectile.aiStyle = -1;
        Projectile.tileCollide = false;
        Projectile.timeLeft = TIMELEFT;

        Projectile.friendly = true;
    }

    public override void AI() {
        Projectile.timeLeft = 2;

        if (!IsParentActive) {
            Projectile.Kill();
            return;
        }

        Player owner = Projectile.GetOwnerAsPlayer();
        owner.SyncMousePosition();
        float lerpRotationTo = Projectile.DirectionTo(owner.GetWorldMousePosition()).ToRotation() - MathHelper.PiOver2;

        if (!Init) {
            Init = true;

            IcicleType = GlacierSpikeType.Small;

            Projectile.rotation = lerpRotationTo;
        }

        if (CaneAttackProgress >= 0.66f) {
            IcicleType = GlacierSpikeType.Large;
        }
        else if (CaneAttackProgress >= 0.33f) {
            IcicleType = GlacierSpikeType.Medium;
        }

        float lerpValue = 0.1f;
        Projectile.rotation = Utils.AngleLerp(Projectile.rotation, lerpRotationTo, lerpValue);

        Projectile.SetDirection(Parent.direction);

        Vector2 lerpPositionTo = CaneCorePosition + new Vector2(-100f * Projectile.direction, -100f);
        Projectile.Center = Vector2.Lerp(Projectile.Center, lerpPositionTo, lerpValue);
    }

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
            ImageFlip = effects
        });
    }
}
