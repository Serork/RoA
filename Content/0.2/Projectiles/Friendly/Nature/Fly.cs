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
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class Fly : NatureProjectile_NoTextureLoad, IRequestAssets {
    private static ushort MAXTIMELEFT => 1000;

    (byte, string)[] IRequestAssets.IndexedPathsToTexture => [(0, ResourceManager.NatureProjectileTextures + "Fly")];

    private Color _color;

    public override void SetStaticDefaults() {
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 0;

        Main.projFrames[Projectile.type] = 4;
    }

    protected override void SafeSetDefaults() {
        SetNatureValues(Projectile, shouldChargeWreath: true, shouldApplyAttachedItemDamage: true);

        Projectile.SetSizeValues(10);

        Projectile.aiStyle = -1;
        Projectile.tileCollide = false;
        Projectile.timeLeft = MAXTIMELEFT;

        Projectile.friendly = true;
        Projectile.penetrate = 1;
    }

    public override void AI() {
        Projectile parent = Main.projectile[(int)Projectile.ai[0]];

        if (!parent.active || parent.type != ModContent.ProjectileType<Rafflesia>()) {
            Projectile.Kill();
            return;
        }

        Vector2 destination = parent.Center - Vector2.UnitY.RotatedBy(parent.rotation) * 10f;
        float distanceToDestination = Vector2.Distance(Projectile.position, destination);
        float minDistance = 100f;
        float inertiaValue = 30, extraInertiaValue = inertiaValue * 5;
        float extraInertiaFactor = 1f - MathUtils.Clamp01(distanceToDestination / minDistance);
        float inertia = inertiaValue + extraInertiaValue * extraInertiaFactor;
        Helper.InertiaMoveTowards(ref Projectile.velocity, Projectile.position, destination, inertia: inertia);
        float length = Projectile.velocity.Length();
        float minLength = 1f;
        if (length < minLength) {
            Projectile.velocity = Projectile.velocity.SafeNormalize() * minLength;
        }

        if (Projectile.localAI[0] == 0f) {
            Projectile.localAI[0] = 1f;

            _color = new Color((float)(Main.rand.NextDouble() * 0.20000000298023224), (float)(Main.rand.NextDouble() * 0.20000000298023224), (float)(Main.rand.NextDouble() * 0.20000000298023224));
        }

        Lighting.AddLight(Projectile.position, _color.ToVector3() * 0.5f);

        ProjectileUtils.Animate(Projectile, 4);
    }

    public override void OnKill(int timeLeft) {
 
    }

    protected override void Draw(ref Color lightColor) {
        if (!AssetInitializer.TryGetRequestedTextureAssets<Fly>(out Dictionary<byte, Asset<Texture2D>> indexedTextureAssets)) {
            return;
        }

        Texture2D wingsTexture = indexedTextureAssets[0].Value;
        Rectangle clip = new SpriteFrame(1, 4, 0, (byte)Projectile.frame).GetSourceRectangle(wingsTexture);
        Vector2 origin = clip.Size() / 2f;

        Texture2D texture = ResourceManager.Pixel;
        int length = ProjectileID.Sets.TrailCacheLength[Projectile.type];
        for (int i = 0; i < length; i++) {
            Vector2 vector6 = Projectile.oldPos[i];
            if (vector6 == Vector2.Zero) {
                continue;
            }
            Vector2 vector7 = i <= 0 ? Projectile.position : Projectile.oldPos[i - 1];
            int num5 = (int)Vector2.Distance(vector6, vector7) / 3 + 1;
            float progress = 1f - (float)i / length;
            if (Vector2.Distance(vector6, vector7) % 3f != 0f)
                num5++;
            Color color = _color.MultiplyRGB(lightColor);
            //color.A += (byte)(MathF.Abs(MathF.Sin((float)Main.timeForVisualEffects * i) * 10));
            for (float num6 = 1f; num6 <= (float)num5; num6 += 1f) {
                Vector2 position = Vector2.Lerp(vector7, vector6, num6 / (float)num5);
                for (int k = 0; k < 4; k++) {
                    switch (k) {
                        case 0:
                            position.X += 1f * progress;
                            break;
                        case 1:
                            position.X += -1f * progress;
                            break;
                        case 2:
                            position.Y += 1f * progress;
                            break;
                        case 3:
                            position.Y += 1f * progress;
                            break;
                    }
                    Main.spriteBatch.Draw(texture, position, DrawInfo.Default with {
                        Clip = texture.Bounds,
                        Color = color * 0.25f,
                        Scale = Vector2.One * MathHelper.Lerp(2f, 4f, progress)
                    });
                }
            }
        }

        Main.spriteBatch.Draw(wingsTexture, Projectile.Center - origin / 2f, DrawInfo.Default with {
            Clip = clip,
            Origin = origin,
            Color = lightColor,
            ImageFlip = Projectile.direction > 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
            Scale = Vector2.One * 0.875f
        });
    }
}
