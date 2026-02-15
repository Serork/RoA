using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Players;
using RoA.Core;
using RoA.Core.Defaults;
using RoA.Core.Graphics.Data;
using RoA.Core.Utility;

using System;

using Terraria;
using Terraria.GameContent;
using Terraria.ID;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class Spore : NatureProjectile {
    private static ushort MAXTIMELEFT => 200;

    public ref struct SporeValues(Projectile projectile) {
        public ref float InitOnSpawnValue = ref projectile.localAI[0];
        public ref float DirectionValue = ref projectile.ai[1];

        public bool Init {
            readonly get => InitOnSpawnValue == 1f;
            set => InitOnSpawnValue = value.ToInt();
        }

        public bool FacedRight {
            readonly get => DirectionValue != 0f;
            set => DirectionValue = value.ToInt();
        }
    }

    public override Color? GetAlpha(Color lightColor) => new Color(255 - Projectile.alpha, 255 - Projectile.alpha, 255 - Projectile.alpha, 0) * 0.9f;

    protected override void SafeSetDefaults() {
        SetNatureValues(Projectile, shouldChargeWreath: true, shouldApplyAttachedItemDamage: true);

        Projectile.SetSizeValues(20);

        Projectile.aiStyle = -1;
        Projectile.tileCollide = false;
        Projectile.timeLeft = MAXTIMELEFT;

        Projectile.friendly = true;
    }

    public override void AI() {
        Player owner = Projectile.GetOwnerAsPlayer();

        void init() {
            //SporeValues sporeValues = new(Projectile);
            //if (!sporeValues.Init) {
            //    sporeValues.Init = true;

            //    if (owner.IsLocal()) {
            //        sporeValues.IsFacingRight = Main.rand.NextBool();
            //    }
            //}
        }
        void moveTowardsCursor() {
            Vector2 destination = owner.GetWorldMousePosition();
            float distanceToDestination = Vector2.Distance(Projectile.position, destination);
            float minDistance = 60f;
            float inertiaValue = 30, extraInertiaValue = inertiaValue * 5;
            float extraInertiaFactor = 1f - MathUtils.Clamp01(distanceToDestination / minDistance);
            float inertia = inertiaValue + extraInertiaValue * extraInertiaFactor;
            Helper.InertiaMoveTowards(ref Projectile.velocity, Projectile.position, destination, inertia: inertia);

            owner.SyncMousePosition();

            SporeValues sporeValues = new(Projectile);
            sporeValues.FacedRight = Projectile.velocity.X < 0f;
        }
        void fadeOut() {
            Projectile.alpha = (byte)Utils.Remap(Projectile.timeLeft, MAXTIMELEFT / 3, 0, 0, 255);
            if (Projectile.alpha >= 255) {
                Projectile.Kill();
            }
        }
        void rotate() {
            Projectile.rotation = Projectile.velocity.X * 0.1f;
        }
        void lightUp() {
            Lighting.AddLight(Projectile.Center, new Vector3(0.1f, 0.4f, 1f) * 0.5f * Projectile.Opacity);
        }

        init();
        moveTowardsCursor();
        fadeOut();
        rotate();
        lightUp();
    }

    public override void ModifyDamageHitbox(ref Rectangle hitbox) {
        hitbox = hitbox.AdjustY(4);
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D sporeTexture = TextureAssets.Projectile[Type].Value;
        Vector2 position = Projectile.Center;
        Rectangle clip = sporeTexture.Bounds;
        Color color = Projectile.GetAlpha(Color.White * 0.9f);
        float rotation = Projectile.rotation;
        SporeValues sporeValues = new(Projectile);
        SpriteEffects flip = sporeValues.FacedRight ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        Vector2 origin = Utils.Top(clip);
        Main.spriteBatch.Draw(sporeTexture, position, DrawInfo.Default with {
            Clip = clip,
            Origin = origin,
            Color = color,
            Rotation = rotation,
            ImageFlip = flip
        });

        return false;
    }

    //public override bool? CanDamage() => Projectile.Opacity > 0.1f;

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        Vector2 hitPoint = Projectile.Center + Projectile.DirectionTo(target.Center) * 10f;
        Vector2 normal = Projectile.velocity.SafeNormalize(Vector2.UnitX);
        Vector2 spinningpoint = Vector2.Reflect(Projectile.velocity, normal);
        float scale = 2.5f - Vector2.Distance(target.Center, Projectile.position) * 0.01f;
        scale = MathHelper.Clamp(scale, 0.75f, 1.15f);
        int dustType = DustID.MushroomSpray;
        for (int i = 0; i < 4; i++) {
            Vector2 dustVelocity = spinningpoint.RotatedBy((float)Math.PI / 4f * Main.rand.NextFloatDirection()) * 0.6f * Main.rand.NextFloat();
            Dust dust = Dust.NewDustPerfect(hitPoint, dustType, dustVelocity, 0, default, 0.5f + 0.3f * Main.rand.NextFloat());
            dust.scale = Main.rand.NextFloat(1f, 2f) * 0.75f;
            dust.scale *= scale;
            dust.noGravity = true;
            dust.noLightEmittence = true;
        }
    }
}
