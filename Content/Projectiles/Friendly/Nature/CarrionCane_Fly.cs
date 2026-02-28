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
    private static ushort MAXTIMELEFT => 360;

    (byte, string)[] IRequestAssets.IndexedPathsToTexture => [(0, ResourceManager.NatureProjectileTextures + "Fly")];

    private Color _color;

    public override void SetStaticDefaults() {
        ProjectileID.Sets.CultistIsResistantTo[Type] = true;

        Projectile.SetTrail(0, 5);
    }

    protected override void SafeSetDefaults() {
        SetNatureValues(Projectile, shouldChargeWreath: true, shouldApplyAttachedItemDamage: true);

        Projectile.SetSizeValues(10);

        Projectile.aiStyle = -1;
        Projectile.tileCollide = false;
        Projectile.timeLeft = MAXTIMELEFT;

        Projectile.friendly = true;
        Projectile.penetrate = 3;

        Projectile.Opacity = 1f;

        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 30;
    }

    public override void AI() {
        Projectile parent = Main.projectile[(int)Projectile.ai[0]];

        //float timeToAppear = 30f;
        //projectile.Opacity = Utils.GetLerpValue(0f, timeToAppear / 2f, projectile.timeLeft, true)/* * Utils.GetLerpValue(MAXTIMELEFT, MAXTIMELEFT - timeToAppear, projectile.timeLeft, true)*/;

        bool flag = Projectile.ai[1] == 1f;
        if (!flag) {
            if (!parent.active || parent.type != ModContent.ProjectileType<Rafflesia>()) {
                Projectile.Kill();
                return;
            }
        }

        Projectile.OffsetTheSameProjectile(0.1f);

        Vector2 destination = Projectile.Center;

        Vector2 getParentGoToPosition() {
            Vector2 result = parent.Center - Vector2.UnitY.RotatedBy(parent.rotation) * 10f;
            result -= Projectile.Size / 2f;
            return result;
        }
        if (flag) {
            NPC? target = NPCUtils.FindClosestNPC(Projectile.Center, 300);
            if (target != null) {
                destination = target.Center;
                destination -= Projectile.Size / 2f;
            }
            else {
                destination = getParentGoToPosition();
            }
        }
        else {
            destination = getParentGoToPosition();
        }
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
            Projectile.ai[1] = 1f;
        }

        Projectile.rotation = Projectile.velocity.X * 0.1f;

        if (Projectile.localAI[0] == 0f) {
            Projectile.localAI[0] = 1f;

            _color = new Color(0.4f, 0.25f, 0.25f);
        }

        ProjectileUtils.Animate(Projectile, 4);
    }

    public static void CreateFlyDust(Vector2 position, Vector2 velocity, int width, int height) {
        Dust dust3 = Dust.NewDustDirect(position, width, height, DustID.BorealWood, 0f, 0f, 0, default, 0.8f + Main.rand.NextFloatRange(0.1f));
        dust3.noGravity = true;
        dust3.velocity += velocity;
        Dust dust2 = dust3;
        dust2.velocity *= Main.rand.NextFloat(0.75f, 1f);
        dust3 = Dust.NewDustDirect(position, width, height, DustID.BorealWood, 0f, 0f, 0, default, 0.8f + Main.rand.NextFloatRange(0.1f));
        dust2 = dust3;
        dust2.velocity += velocity;
        dust2.velocity *= Main.rand.NextFloat(0.75f, 1f);
        dust3.noGravity = true;
    }

    public override void OnKill(int timeLeft) {
        for (int j = 0; j < 1; j++) {
            CreateFlyDust(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
        }
    }

    protected override void Draw(ref Color lightColor) {
        if (!AssetInitializer.TryGetRequestedTextureAssets<Fly>(out Dictionary<byte, Asset<Texture2D>> indexedTextureAssets)) {
            return;
        }

        Texture2D wingsTexture = indexedTextureAssets[0].Value;
        Rectangle clip = new SpriteFrame(1, 4, 0, (byte)Projectile.frame).GetSourceRectangle(wingsTexture);
        Vector2 origin = clip.Size() / 2f;
		
		Projectile.frame++;

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
            Color color = _color.MultiplyRGB(lightColor) * Projectile.Opacity;
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
            Color = lightColor * Projectile.Opacity,
            ImageFlip = Projectile.direction > 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
            Scale = Vector2.One * 0.75f,
            Rotation = Projectile.rotation
        });
    }
}
