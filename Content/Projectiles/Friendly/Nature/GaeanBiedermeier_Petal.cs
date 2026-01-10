using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common;
using RoA.Common.Projectiles;
using RoA.Core;
using RoA.Core.Defaults;
using RoA.Core.Graphics.Data;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using System;
using System.Collections.Generic;

using Terraria;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class BiedermeierPetal : NatureProjectile_NoTextureLoad, IRequestAssets, ISpawnCopies {
    private static byte BASESIZE => 32;
    private static ushort MAXTIMELEFT => 360;
    private static byte LOOPANIMATIONFRAMECOUNT => 8;

    public enum BiedermeierPetalTextureType : byte {
        Petal
    }

    (byte, string)[] IRequestAssets.IndexedPathsToTexture =>
        [((byte)BiedermeierPetalTextureType.Petal, ResourceManager.NatureProjectileTextures + "BiedermeierFlower_Petal")];

    float ISpawnCopies.CopyDeathFrequency => 0.1f;

    public ref float InitOnSpawnValue => ref Projectile.localAI[0];
    public ref float RotationValue => ref Projectile.localAI[1];
    public ref float RotationValue2 => ref Projectile.localAI[2];
    public ref int Frame => ref Projectile.frame;

    private Vector2 _mousePosition;
    private float _copyCounter;

    public byte FlowerType => (byte)Projectile.ai[0];
    public float StartRotation => Projectile.ai[1];

    public bool Init {
        get => InitOnSpawnValue == 1f;
        set => InitOnSpawnValue = value.ToInt();
    }

    public bool Falling {
        get => Projectile.ai[2] == 1f;
        set => Projectile.ai[2] = value.ToInt();
    }

    public override void SetStaticDefaults() {
        Projectile.SetTrail(2, 4);
    }

    protected override void SafeSetDefaults() {
        SetNatureValues(Projectile, shouldChargeWreath: true, shouldApplyAttachedItemDamage: true);

        Projectile.SetSizeValues(BASESIZE);

        Projectile.aiStyle = -1;
        Projectile.tileCollide = true;
        Projectile.timeLeft = MAXTIMELEFT;

        Projectile.friendly = true;
        Projectile.penetrate = 1 + 3;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;

        Projectile.manualDirectionChange = true;

        Projectile.hide = true;
    }

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
        overPlayers.Add(index);
    }

    public override void AI() {
        if (!Init) {
            Init = true;

            if (Projectile.IsOwnerLocal()) {
                _mousePosition = Projectile.GetOwnerAsPlayer().GetViableMousePosition();
            }

            float flySpeed = 9f;
            Projectile.velocity = Projectile.velocity.SafeNormalize() * flySpeed;
            Projectile.rotation = StartRotation;

            RotationValue = RotationValue2 = MathHelper.Pi;

            Projectile.SetDirection(Projectile.velocity.X.GetDirection());

            CopyHandler.InitializeCopies(Projectile, 10);

            return;
        }

        if (_copyCounter++ % 4 == 0) {
            CopyHandler.MakeCopy(Projectile);
        }

        if (Vector2.Distance(Projectile.Center, _mousePosition) < 100f) {
            if (!Falling) {
                _mousePosition += Main.rand.RandomPointInArea(50f);
            }
            Falling = true;
        }

        if (Falling) {
            Projectile.timeLeft--;

            Projectile.velocity.Y += 0.1f;

            Vector2 destination = _mousePosition;
            float distanceToDestination = Vector2.Distance(Projectile.position, destination);
            float minDistance = 60f;
            float inertiaValue = 30, extraInertiaValue = inertiaValue * 5;
            float extraInertiaFactor = 1f - MathUtils.Clamp01(distanceToDestination / minDistance);
            float inertia = inertiaValue + extraInertiaValue * extraInertiaFactor;
            Helper.InertiaMoveTowards(ref Projectile.velocity, Projectile.position, destination, inertia: inertia);

        }

        ref float rotation = ref Projectile.rotation;
        ref float extraRotation = ref RotationValue;
        ref float extraRotation2 = ref RotationValue2;
        float velocityFactor = Utils.Remap(Projectile.velocity.Length(), 0f, 3f, 0.5f, 1f, true);
        rotation += extraRotation / extraRotation2 * Projectile.direction * velocityFactor * 0.25f;
        float timerValue = 0.01f;
        extraRotation2 += timerValue;
        float minFactor = 0.75f;
        float deceleration = 0.96f;
        if (extraRotation > minFactor) {
            extraRotation *= deceleration;
        }

        Projectile.Animate(6, LOOPANIMATIONFRAMECOUNT);
    }

    protected override void Draw(ref Color lightColor) {
        if (!AssetInitializer.TryGetRequestedTextureAssets<BiedermeierPetal>(out Dictionary<byte, Asset<Texture2D>> indexedTextureAssets)) {
            return;
        }

        if (!Init) {
            return;
        }

        SpriteBatch batch = Main.spriteBatch;
        Texture2D texture = indexedTextureAssets[(byte)BiedermeierPetalTextureType.Petal].Value;
        Vector2 position = Projectile.Center;
        byte frameXCount = (byte)BiedermeierFlower.FlowerType.Count;
        int frame = Projectile.frame;
        Rectangle clip = Utils.Frame(texture, frameXCount, LOOPANIMATIONFRAMECOUNT, frameX: FlowerType, frameY: frame);

        var handler = Projectile.GetGlobalProjectile<CopyHandler>();
        var copyData = handler.CopyData;
        for (int i = 0; i < 10; i++) {
            CopyHandler.CopyInfo copyInfo = copyData![i];
            if (MathUtils.Approximately(copyInfo.Position, Projectile.Center, 2f)) {
                continue;
            }
            var clip2 = Utils.Frame(texture, frameXCount, LOOPANIMATIONFRAMECOUNT, frameX: FlowerType, frameY: copyInfo.UsedFrame);
            batch.Draw(texture, copyInfo.Position, DrawInfo.Default with {
                Color = lightColor * MathUtils.Clamp01(copyInfo.Opacity) * Projectile.Opacity * 0.5f * 0.5f,
                Rotation = copyInfo.Rotation,
                Scale = Vector2.One * MathF.Max(copyInfo.Scale, 1f),
                Origin = clip2.Centered(),
                Clip = clip2
            });
        }

        Projectile.QuickDrawShadowTrails(lightColor * Projectile.Opacity * 0.25f, 0.5f, 1, 0f, texture: texture, clip: clip);

        Vector2 origin = clip.Centered();
        float rotation = Projectile.rotation;
        SpriteEffects flip = Projectile.spriteDirection.ToSpriteEffects();
        DrawInfo drawInfo = new() {
            Clip = clip,
            Origin = origin,
            Rotation = rotation,
            ImageFlip = flip
        };
        batch.Draw(texture, position, drawInfo);
    }
}
