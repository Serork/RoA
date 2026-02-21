using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common;
using RoA.Common.Players;
using RoA.Common.Projectiles;
using RoA.Content.Items.Weapons.Nature;
using RoA.Core;
using RoA.Core.Defaults;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using System;
using System.Collections.Generic;

using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class CoralBubble : NatureProjectile_NoTextureLoad, IRequestAssets {
    private static ushort TIMELEFT => 120;

    public enum CoralBubbleRequstedTextureType : byte {
        Base,
        Outline
    }

    (byte, string)[] IRequestAssets.IndexedPathsToTexture =>
        [((byte)CoralBubbleRequstedTextureType.Base, ResourceManager.NatureProjectileTextures + "CoralBubble"),
         ((byte)CoralBubbleRequstedTextureType.Outline, ResourceManager.NatureProjectileTextures + "CoralBubble_Outline")];

    private float _wave;
    private Vector2 _bubbleSquish;

    public ref float MousePositionX => ref Projectile.ai[1];
    public ref float MousePositionY => ref Projectile.ai[2];
    public ref float SquishVelocityX => ref Projectile.localAI[1];
    public ref float SquishVelocityY => ref Projectile.localAI[2];

    public Vector2 SquishVelocity {
        get => new(SquishVelocityX, SquishVelocityY);
        set {
            SquishVelocityX = value.X;
            SquishVelocityY = value.Y;
        } 
    }

    public Vector2 BubbleSquish {
        get => _bubbleSquish;
        set {
            _bubbleSquish = value;
        }
    }

    public Vector2 MousePosition {
        get => new(MousePositionX, MousePositionY);
        set {
            MousePositionX = value.X;
            MousePositionY = value.Y;
        }
    }

    protected override void SafeSetDefaults() {
        Projectile.SetSizeValues(10);

        Projectile.aiStyle = -1;
        Projectile.tileCollide = false;

        Projectile.friendly = true;

        Projectile.timeLeft = TIMELEFT;

        Projectile.ignoreWater = true;
    }

    public override void AI() {
        if (Collision.SolidCollision(Projectile.position, Projectile.width, Projectile.height)) {
            Projectile.timeLeft -= 1;
        }

        Player owner = Projectile.GetOwnerAsPlayer();
        CaneBaseProjectile? heldCane = owner.GetWreathHandler().GetHeldCane();
        bool flag = false;
        if (heldCane is not null && heldCane.PreparingAttack) {
            flag = true;
        }
        if (!flag) {
            owner.SyncMousePosition();
            MousePosition = owner.GetWorldMousePosition();
        }
        Vector2 destination = MousePosition;

        int num606 = -1;
        Vector2 vector52 = Projectile.Center;
        float num607 = 250;
        if (Projectile.localAI[0] > 0f)
            Projectile.localAI[0]--;

        if (Projectile.ai[0] == 0f && Projectile.localAI[0] == 0f) {
            for (int num608 = 0; num608 < 200; num608++) {
                NPC nPC7 = Main.npc[num608];
                if (nPC7.CanBeChasedBy(this) && (Projectile.ai[0] == 0f || Projectile.ai[0] == (float)(num608 + 1))) {
                    Vector2 center7 = nPC7.Center;
                    float num609 = Vector2.Distance(center7, vector52);
                    if (num609 < num607 && Collision.CanHit(Projectile.Center - Vector2.One * 60, 30, 30, nPC7.position, nPC7.width, nPC7.height)) {
                        num607 = num609;
                        vector52 = center7;
                        num606 = num608;
                    }
                }
            }

            if (num606 >= 0) {
                Projectile.ai[0] = num606 + 1;
                Projectile.netUpdate = true;
            }

            num606 = -1;
        }

        if (Projectile.localAI[0] == 0f && Projectile.ai[0] == 0f)
            Projectile.localAI[0] = 30f;

        bool flag33 = false;
        if (Projectile.ai[0] != 0f) {
            int num610 = (int)(Projectile.ai[0] - 1f);
            if (Main.npc[num610].active && !Main.npc[num610].dontTakeDamage && Main.npc[num610].immune[Projectile.owner] == 0) {
                float num611 = Main.npc[num610].position.X + (float)(Main.npc[num610].width / 2);
                float num612 = Main.npc[num610].position.Y + (float)(Main.npc[num610].height / 2);
                float num613 = Math.Abs(Projectile.position.X + (float)(Projectile.width / 2) - num611) + Math.Abs(Projectile.position.Y + (float)(Projectile.height / 2) - num612);
                if (num613 < 1000f) {
                    flag33 = true;
                    vector52 = Main.npc[num610].Center;
                }
            }
            else {
                Projectile.ai[0] = 0f;
                flag33 = false;
                Projectile.netUpdate = true;
            }
        }
        if (flag33) {
            destination = vector52;
        }

        float distanceToDestination = Vector2.Distance(Projectile.position, destination);
        float minDistance = 60f;
        float inertiaValue = 5f, extraInertiaValue = inertiaValue * 5f;
        float extraInertiaFactor = 1f - MathUtils.Clamp01(distanceToDestination / minDistance);
        float inertia = inertiaValue + extraInertiaValue * extraInertiaFactor;
        float speed = 7.5f;
        Helper.InertiaMoveTowards(ref Projectile.velocity, Projectile.position, destination, speed: speed, inertia: inertia);
        float offsetSpeed = 2.5f;
        Projectile.OffsetTheSameProjectile(offsetSpeed);
        if (Projectile.NearestTheSame(out Projectile projectile)) {
            if (Projectile.position.X < projectile.position.X) {
                Projectile.position.X -= offsetSpeed;
            }
            else {
                Projectile.position.X += offsetSpeed;
            }
            if (Projectile.position.Y < projectile.position.Y) {
                Projectile.position.Y -= offsetSpeed;
            }
            else {
                Projectile.position.Y += offsetSpeed;
            }
        }
        Projectile.rotation = Projectile.velocity.X * 0.025f;
        float squishVelocityBlend = 0.1f;
        float interpolationBlend = 0.15f;
        float movementInfluence = 0.5f;
        float maxSquishDeformation = 0.1f;
        float velocityChangeThreshold = 10f;
        Vector2 targetSquish = Vector2.One;
        Vector2 absoluteVelocity = new(Math.Abs(Projectile.velocity.X), Math.Abs(Projectile.velocity.Y));
        float movementRatio = MathHelper.Clamp((1f + absoluteVelocity.X * movementInfluence) / (1f + absoluteVelocity.Y * movementInfluence), 1f - maxSquishDeformation, 1f + maxSquishDeformation);
        targetSquish.X *= movementRatio;
        targetSquish.Y /= movementRatio;
        Vector2 squishDirection = (targetSquish - BubbleSquish).SafeNormalize(Vector2.One);
        if (Vector2.Distance(targetSquish, BubbleSquish) > 0.05f) {
            SquishVelocity = Vector2.Lerp(SquishVelocity, squishDirection, interpolationBlend);
        }
        if (Math.Abs(Projectile.velocity.Length() - Projectile.oldVelocity.Length()) > velocityChangeThreshold) {
            SquishVelocity += squishDirection * 0.5f;
        }
        BubbleSquish += SquishVelocity * squishVelocityBlend;
        Vector2 clampedSquish = new(MathHelper.Clamp(BubbleSquish.X, 1f - maxSquishDeformation, 1f + maxSquishDeformation), MathHelper.Clamp(BubbleSquish.Y, 1f - maxSquishDeformation, 1f + maxSquishDeformation));
        BubbleSquish = Vector2.Lerp(BubbleSquish, clampedSquish, interpolationBlend);

        _wave += 0.15f;
    }

    protected override void Draw(ref Color lightColor) {
        if (!AssetInitializer.TryGetRequestedTextureAssets<CoralBubble>(out Dictionary<byte, Asset<Texture2D>> indexedTextureAssets)) {
            return;
        }

        Texture2D baseTexture = indexedTextureAssets[(byte)CoralBubbleRequstedTextureType.Base].Value,
                  outlineTexture = indexedTextureAssets[(byte)CoralBubbleRequstedTextureType.Outline].Value;
        Vector2 scale = Vector2.One * BubbleSquish;
        float hslFactor = _wave + Projectile.whoAmI;
        Color color = Color.Lerp(Color.White, Main.hslToRgb(hslFactor / 15f % 1f, 1f, 0.5f), 0.5f);
        color = color.MultiplyRGB(lightColor);
        color.A /= 2;
        color *= 0.75f;
        Projectile.QuickDraw(color, scale: scale, texture: baseTexture);
        color = Color.Lerp(Color.White, Main.hslToRgb((hslFactor / 15f + 0.5f) % 1f, 1f, 0.5f), 0.5f);
        color = color.MultiplyRGB(lightColor);
        color *= 0.75f;
        Projectile.QuickDraw(color, scale: scale, texture: outlineTexture);
    }

    public override void OnKill(int timeLeft) {
        SoundEngine.PlaySound(SoundID.Item54, Projectile.position);

        float sizeFactor = 4;
        for (int num269 = 0; num269 < 5 + 8 * sizeFactor; num269++) {
            int type = Utils.IsPowerOfTwo(num269) ? DustID.Water_Desert : DustID.BubbleBurst_Blue;
            int num270 = (int)(3f * sizeFactor);
            int num271 = Dust.NewDust(Projectile.Center - Vector2.One * num270, num270 * 2, num270 * 2, type);
            Dust dust41 = Main.dust[num271];
            Vector2 vector35 = Vector2.Normalize(dust41.position - Projectile.Center);
            dust41.position = Projectile.Center + vector35 * num270 * Projectile.scale;
            if (num269 < 30)
                dust41.velocity = vector35 * dust41.velocity.Length();
            else
                dust41.velocity = vector35 * Main.rand.Next(45, 91) / 10f;

            dust41.velocity += Projectile.velocity * 0.5f;

            dust41.color = Main.hslToRgb((float)(0.4000000059604645 + Main.rand.NextDouble() * 0.45000000298023224), (float)(0.8000000059604645 + Main.rand.NextFloatDirection() * 0.20000000298023224), 0.5f);
            dust41.color = Color.Lerp(dust41.color, Color.White, 0.3f);
            dust41.alpha = Main.rand.Next(100, 200);
            dust41.noGravity = true;
            dust41.scale = 0.7f + 0.1f * sizeFactor;
        }
    }
}
