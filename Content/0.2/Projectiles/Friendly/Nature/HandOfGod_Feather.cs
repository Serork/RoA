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

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class GodFeather : NatureProjectile_NoTextureLoad, IRequestAssets {
    private static ushort TIMELEFT => 300;

    public enum GodFeatherRequstedTextureType : byte {
        Base,
        Glow,
        Eye
    }

    (byte, string)[] IRequestAssets.IndexedPathsToTexture =>
        [((byte)GodFeatherRequstedTextureType.Base, ResourceManager.NatureProjectileTextures + "GodFeather"),
         ((byte)GodFeatherRequstedTextureType.Glow, ResourceManager.NatureProjectileTextures + "GodFeather_Glow"),
         ((byte)GodFeatherRequstedTextureType.Eye, ResourceManager.NatureProjectileTextures + "GodFeather_Eye")];

    public ref float WaveValue => ref Projectile.localAI[1];
    public ref float RotationLerpValue => ref Projectile.localAI[2];

    protected override void SafeSetDefaults() {
        SetNatureValues(Projectile);

        Projectile.SetSizeValues(10);

        Projectile.aiStyle = -1;
        Projectile.tileCollide = false;
        Projectile.timeLeft = TIMELEFT;

        Projectile.friendly = true;

        Projectile.manualDirectionChange = true;

        Projectile.hide = true;
    }

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
        behindNPCs.Add(index);
    }

    public override bool? CanDamage() => false;
    public override bool? CanCutTiles() => false;

    public override void AI() {
        Projectile.timeLeft = 2;

        Player owner = Projectile.GetOwnerAsPlayer();
        owner.SyncMousePosition();
        Vector2 mousePosition = owner.GetWorldMousePosition();
        Vector2 center = owner.Center;
        float distance = 50f;
        Projectile.velocity = Vector2.Lerp(Projectile.velocity, center.DirectionTo(mousePosition) * distance, 0.05f);
        Projectile.Center = Utils.Floor(center) + Projectile.velocity + Vector2.UnitY * owner.gfxOffY;

        int direction = Projectile.DirectionTo(owner.Center).X.GetDirection();
        Projectile.SetDirection(direction);

        RotationLerpValue = Helper.Approach(RotationLerpValue, 0.0375f * Projectile.direction, TimeSystem.LogicDeltaTime);
        float rotationValue = RotationLerpValue;
        Projectile.rotation += rotationValue;

        WaveValue += TimeSystem.LogicDeltaTime;
    }

    public override void OnKill(int timeLeft) {
        
    }

    protected override void Draw(ref Color lightColor) {
        if (!AssetInitializer.TryGetRequestedTextureAssets<GodFeather>(out Dictionary<byte, Asset<Texture2D>> indexedTextureAssets)) {
            return;
        }

        Texture2D baseTexture = indexedTextureAssets[(byte)GodFeatherRequstedTextureType.Base].Value,
                  glowTexture = indexedTextureAssets[(byte)GodFeatherRequstedTextureType.Glow].Value,
                  closedEyeTexture = indexedTextureAssets[(byte)GodFeatherRequstedTextureType.Eye].Value;
        SpriteBatch batch = Main.spriteBatch;
        int count = 8;
        float waveOffset = Projectile.whoAmI;
        for (int i = 0; i < count; i++) {
            float rotation = (float)i / count * MathHelper.TwoPi;
            rotation += Projectile.rotation;
            Vector2 position = Projectile.Center;
            float distance = 6f;
            position += Vector2.UnitY.RotatedBy(rotation) * distance;
            Rectangle clip = baseTexture.Bounds;
            Vector2 origin = clip.BottomCenter();
            Color color = lightColor;
            rotation += MathHelper.Pi;
            Vector2 scale = Vector2.One;
            DrawInfo drawInfo = DrawInfo.Default with {
                Clip = clip,
                Origin = origin,
                Color = color,
                Rotation = rotation,
                Scale = scale
            };

            float maxRotation = 0.3f;

            bool second = i % 2 == 0;
            if (second) {
                batch.Draw(glowTexture, position, drawInfo with {
                    Scale = scale * 1.5f,
                    Color = color.MultiplyAlpha(Helper.Wave(WaveValue, 0.25f, 0.75f, 10f, waveOffset)) * 0.625f,
                    Rotation = rotation + Helper.Wave(WaveValue, -maxRotation, maxRotation, 5f, waveOffset)
                });
            }
            batch.Draw(baseTexture, position, drawInfo with {
                Scale = scale * Helper.Wave(WaveValue, 0.9f, 1.1f, 2.5f, waveOffset + i * count)
            });
            if (!second) {
                batch.Draw(glowTexture, position, drawInfo with {
                    Scale = scale * 1.5f * Helper.Wave(WaveValue, 0.75f, 1.25f, 5f, waveOffset + i * count),
                    Color = color.MultiplyAlpha(Helper.Wave(WaveValue, 0.25f, 0.75f, 10f, waveOffset)) * 0.5f,
                    Rotation = rotation + MathHelper.PiOver4 + Helper.Wave(WaveValue, -maxRotation, maxRotation, 5f, waveOffset)
                });
            }
        }

        for (int i = 0; i < count; i++) {
            float rotation = (float)i / count * MathHelper.TwoPi;
            rotation += Projectile.rotation;
            Vector2 position = Projectile.Center;
            float distance = 6f;
            position += Vector2.UnitY.RotatedBy(rotation) * distance;
            Rectangle clip = baseTexture.Bounds;
            Vector2 origin = clip.BottomCenter();
            Color color = lightColor;
            rotation += MathHelper.Pi;
            Vector2 scale = Vector2.One;
            DrawInfo drawInfo = DrawInfo.Default with {
                Clip = clip,
                Origin = origin,
                Color = color,
                Rotation = rotation,
                Scale = scale
            };
            origin += Vector2.UnitY * -16f;
            position += Vector2.UnitY.RotatedBy(rotation) * 25.5f;
            scale *= Helper.Wave(WaveValue, 1f, 1.5f, 5f, waveOffset + i * count);
            batch.Draw(closedEyeTexture, position, drawInfo with {
                Scale = scale,
                Origin = origin
            });
        }
    }
}
