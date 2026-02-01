using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common.VisualEffects;
using RoA.Content.Dusts;
using RoA.Content.Items.Weapons.Ranged.Hardmode;
using RoA.Core;
using RoA.Core.Defaults;
using RoA.Core.Graphics.Data;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using System;

using Terraria;
using Terraria.Audio;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Ranged;

sealed class SatchelChargeProjectile : ModProjectile {
    private static float EXPLOSIONSCALE => 1.25f;

    private static Asset<Texture2D> _glowTexture = null!;

    public override string Texture => ResourceManager.RangedProjectileTextures + "SatchelCharge";

    public static SoundStyle ExplosionSound { get; private set; } = new SoundStyle(ResourceManager.ItemSounds + "SatchelChargeExplosion");

    public override void SetStaticDefaults() {
        ProjectileID.Sets.PlayerHurtDamageIgnoresDifficultyScaling[Type] = true;

        ProjectileID.Sets.Explosive[Type] = true;

        Projectile.SetFrameCount(2);

        if (Main.dedServ) {
            return;
        }

        _glowTexture = ModContent.Request<Texture2D>(Texture + "_Glow");
    }

    public override void SetDefaults() {
        Projectile.SetSizeValues(22, 28);
        Projectile.DamageType = DamageClass.Ranged;
        Projectile.friendly = true;
        Projectile.tileCollide = true;

        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;

        Projectile.penetrate = -1;
    }

    public override bool? CanDamage() => Projectile.timeLeft <= 3;

    public override bool? CanCutTiles() => Projectile.timeLeft <= 3;

    public override bool OnTileCollide(Vector2 oldVelocity) {
        Projectile.velocity *= 0.9f;

        return false;
    }

    public override void PrepareBombToBlow() {
        Projectile.tileCollide = false; // This is important or the explosion will be in the wrong place if the bomb explodes on slopes.
        Projectile.alpha = 255; // Set to transparent. This projectile technically lives as transparent for about 3 frames

        // Change the hitbox size, centered about the original projectile center. This makes the projectile damage enemies during the explosion.
        int size = (int)(250 * EXPLOSIONSCALE);
        Projectile.Resize(size, size);

        //Projectile.damage = 250; // Bomb: 100, Dynamite: 250
        //Projectile.knockBack = 10f; // Bomb: 8f, Dynamite: 10f
    }

    public override void OnKill(int timeLeft) {
        // Play explosion sound
        SoundEngine.PlaySound(ExplosionSound, Projectile.Center);

        // Smoke Dust spawn
        for (int i = 0; i < 50; i++) {
            Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, 0f, 0f, 100, default, 2f);
            dust.position = Projectile.Center + Main.rand.NextVector2Circular(Projectile.width, Projectile.height) * 0.5f;
            dust.velocity *= 1.4f;
        }

        // Fire Dust spawn
        for (int i = 0; i < 80; i++) {
            Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, default, 3f);
            dust.position = Projectile.Center + Main.rand.NextVector2Circular(Projectile.width, Projectile.height) * 0.5f;
            dust.noGravity = true;
            dust.velocity *= 5f;
            dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, default, 2f);
            dust.position = Projectile.Center + Main.rand.NextVector2Circular(Projectile.width, Projectile.height) * 0.5f;
            dust.velocity *= 3f;
        }

        // Large Smoke Gore spawn
        for (int g = 0; g < 2; g++) {
            var goreSpawnPosition = new Vector2(Projectile.position.X + Projectile.width / 2 - 24f, Projectile.position.Y + Projectile.height / 2 - 24f);
            Gore gore = Gore.NewGoreDirect(Projectile.GetSource_FromThis(), goreSpawnPosition, default, Main.rand.Next(61, 64), 1f);
            gore.position = Projectile.Center + Main.rand.NextVector2Circular(Projectile.width, Projectile.height) * 0.1f - new Vector2(24, 24);
            gore.scale = 1.5f;
            gore.velocity.X += 1.5f;
            gore.velocity.Y += 1.5f;
            gore = Gore.NewGoreDirect(Projectile.GetSource_FromThis(), goreSpawnPosition, default, Main.rand.Next(61, 64), 1f);
            gore.position = Projectile.Center + Main.rand.NextVector2Circular(Projectile.width, Projectile.height) * 0.1f - new Vector2(24, 24);
            gore.scale = 1.5f;
            gore.velocity.X -= 1.5f;
            gore.velocity.Y += 1.5f;
            gore = Gore.NewGoreDirect(Projectile.GetSource_FromThis(), goreSpawnPosition, default, Main.rand.Next(61, 64), 1f);
            gore.position = Projectile.Center + Main.rand.NextVector2Circular(Projectile.width, Projectile.height) * 0.1f - new Vector2(24, 24);
            gore.scale = 1.5f;
            gore.velocity.X += 1.5f;
            gore.velocity.Y -= 1.5f;
            gore = Gore.NewGoreDirect(Projectile.GetSource_FromThis(), goreSpawnPosition, default, Main.rand.Next(61, 64), 1f);
            gore.position = Projectile.Center + Main.rand.NextVector2Circular(Projectile.width, Projectile.height) * 0.1f - new Vector2(24, 24);
            gore.scale = 1.5f;
            gore.velocity.X -= 1.5f;
            gore.velocity.Y -= 1.5f;
        }

        if (Projectile.IsOwnerLocal() && Main.netMode != NetmodeID.Server) {
            PunchCameraModifier modifier4 = new(Projectile.Center, Vector2.One.RotatedByRandom(MathHelper.TwoPi), 1f, 6f, 15, -1f, "Satchel Charge");
            Main.instance.CameraModifiers.Add(modifier4);
        }

        float modifier = 0.1f;
        int count = 20;
        for (int g = 0; g < count; g++) {
            Vector2 position = Projectile.Center + Main.rand.NextVector2CircularEdge(Projectile.width, Projectile.height) * modifier * 0.375f;
            if (!Main.rand.NextBool(3)) {
                AdvancedDustSystem.New<AdvancedDusts.SatchelChargeExplosion>(Main.rand.NextBool() ? AdvancedDustLayer.ABOVEDUSTS : AdvancedDustLayer.BEHINDPLAYERS)?.Setup(position,
                    Vector2.One.RotatedByRandom(MathHelper.TwoPi) * 5f,
                    scale: Main.rand.NextFloat(1f, 1.5f));
            }
            for (int i = 0; i < 3; i++) {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<Smoke2>(), 0f, 0f, 0,
                    Color.Lerp(Color.Black, Color.Lerp(Color.Black, Color.Brown, 0.5f), 0.75f * Main.rand.NextFloat()) * 0.5f, Main.rand.NextFloat(0.4f, 0.6f) * 1f);
                position = Projectile.Center + Main.rand.NextVector2CircularEdge(Projectile.width, Projectile.height) * modifier * 0.375f;
                dust.position = position;
                dust.velocity = Vector2.UnitY * -2f;
            }
            modifier += 0.05f;
        }

        // reset size to normal width and height.
        Projectile.Resize(22, 28);

        //// Finally, actually explode the tiles and walls. Run this code only for the owner
        //if (Projectile.owner == Main.myPlayer) {
        //    int explosionRadius = 7; // Bomb: 4, Dynamite: 7, Explosives & TNT Barrel: 10
        //    int minTileX = (int)(Projectile.Center.X / 16f - explosionRadius);
        //    int maxTileX = (int)(Projectile.Center.X / 16f + explosionRadius);
        //    int minTileY = (int)(Projectile.Center.Y / 16f - explosionRadius);
        //    int maxTileY = (int)(Projectile.Center.Y / 16f + explosionRadius);

        //    // Ensure that all tile coordinates are within the world bounds
        //    Utils.ClampWithinWorld(ref minTileX, ref minTileY, ref maxTileX, ref maxTileY);

        //    // These 2 methods handle actually mining the tiles and walls while honoring tile explosion conditions
        //    bool explodeWalls = Projectile.ShouldWallExplode(Projectile.Center, explosionRadius, minTileX, maxTileX, minTileY, maxTileY);
        //    Projectile.ExplodeTiles(Projectile.Center, explosionRadius, minTileX, maxTileX, minTileY, maxTileY, explodeWalls);
        //}
    }

    public override void AI() {
        if (Projectile.owner == Main.myPlayer && Projectile.timeLeft <= 3) {
            Projectile.PrepareBombToBlow();
        }
        else {

        }

        Player player = Projectile.GetOwnerAsPlayer();

        if (Projectile.localAI[0] == 1f && player.IsLocal()) {
            if (player.controlUseItem && player.ItemAnimationJustStarted && Projectile.ai[1] != 1f && player.GetSelectedItem().type == ModContent.ItemType<SatchelCharge>()) {
                Projectile.ai[1] = 1f;
                Projectile.netUpdate = true;
            }
        }

        if (Projectile.localAI[0] == 0f) {
            Projectile.localAI[0] = 1f;

            Projectile.velocity = Projectile.velocity.SafeNormalize() * 7.5f;

            Projectile.Center = player.GetPlayerCorePoint();
            int index = 0;
            int max = 15;
            while (!WorldGenHelper.SolidTileNoPlatform(Projectile.Center.ToTileCoordinates())) {
                index++;
                Projectile.Center += player.DirectionTo(player.GetViableMousePosition());
                if (index > max) {
                    break;
                }
            }

            Projectile.Center -= Projectile.velocity;
        }

        Projectile.velocity.X *= 0.95f;
        if ((double)Math.Abs(Projectile.velocity.X) < 0.1)
            Projectile.velocity.X = 0f;
        else {
            Projectile.SetDirection(Projectile.velocity.X.GetDirection());
        }

        if (Projectile.ai[1] == 1f) {
            Projectile.localAI[2]++;
            if (player.ItemAnimationEndingOrEnded && Projectile.timeLeft > 3) {
                Projectile.timeLeft = 3;
                Projectile.PrepareBombToBlow();
            }
            if (Projectile.localAI[2] > 20 / 3) {
                Projectile.frame = 1;
            }
        }
        else {
            Projectile.timeLeft = 100;
        }

        Projectile.velocity.Y = Projectile.velocity.Y + 0.3f;
        if (Projectile.velocity.Y > 16f) {
            Projectile.velocity.Y = 16f;
        }

        Projectile.localAI[1] += 0.03f;
        if (Projectile.localAI[1] > 1.5f) {
            Projectile.localAI[1] = 0f;
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        Color color = lightColor * Projectile.Opacity;
        Projectile.QuickDrawAnimated(color);
        Texture2D glowTexture = _glowTexture.Value;
        if (Projectile.frame == 0) {
            color = color.ModifyRGB(Helper.Wave(0.25f, 1f, 25f, Projectile.whoAmI));
        }
        Projectile.QuickDrawAnimated(color, texture: glowTexture);

        float globalOpacity = 0.875f;
        float baseGlobalScale = Projectile.localAI[1];
        float globalScale = Ease.CubeOut(/*Ease.CubeIn*/(MathUtils.Clamp01(baseGlobalScale))) * 1.25f * EXPLOSIONSCALE;
        float alphaModifier = 0.5f;
        float scaleOpacityFactor = Utils.GetLerpValue(1.5f, 1.1f, baseGlobalScale, true);
        globalScale -= Utils.GetLerpValue(0.75f, 1f, scaleOpacityFactor, true) * 0.05f;
        globalOpacity *= scaleOpacityFactor;
        SpriteBatch batch = Main.spriteBatch;
        Texture2D circleTexture = ResourceManager.Circle2;
        Vector2 circlePosition = Projectile.Center;
        Rectangle circleClip = circleTexture.Bounds;
        Vector2 circleOrigin = circleClip.Centered();
        Color circleColor = new Color(230, 70, 70).MultiplyAlpha(alphaModifier) * 0.625f * globalOpacity * 0.375f;
        DrawInfo circleDrawInfo = new() {
            Clip = circleClip,
            Origin = circleOrigin,
            Color = circleColor
        };
        batch.DrawWithSnapshot(circleTexture, circlePosition, circleDrawInfo.WithScale(1.5f).WithScale(globalScale), blendState: BlendState.Additive);
        circleTexture = ResourceManager.Circle3;
        circlePosition = Projectile.Center;
        circleClip = circleTexture.Bounds;
        circleOrigin = circleClip.Centered();
        circleDrawInfo = new() {
            Clip = circleClip,
            Origin = circleOrigin,
            Color = circleColor
        };
        float opacity = 1f;
        for (int i = 0; i < 3; i++) {
            float scale = 1f + 0.09f * i;
            opacity -= 0.25f;
            circleDrawInfo = circleDrawInfo.WithScale(scale).WithColorModifier(opacity * 1.375f);
            circleColor = Color.Lerp(new Color(255, 255, 86), new Color(230, 70, 70), i / 2f);
            circleDrawInfo = circleDrawInfo.WithColor(circleColor.MultiplyAlpha(alphaModifier) * globalOpacity);
            batch.DrawWithSnapshot(circleTexture, circlePosition, circleDrawInfo.WithScale(1.625f).WithScale(globalScale), blendState: BlendState.Additive);
        }
        circleTexture = ResourceManager.Circle7;
        circlePosition = Projectile.Center;
        circleClip = circleTexture.Bounds;
        circleOrigin = circleClip.Centered();
        circleColor = new Color(255, 150, 86).MultiplyAlpha(alphaModifier) * 0.5f;
        circleDrawInfo = new() {
            Clip = circleClip,
            Origin = circleOrigin,
            Color = circleColor
        };
        opacity = 0.25f;
        for (int i = 0; i < 3; i++) {
            float scale = 1f + 0.0375f * i;
            opacity += 0.45f;
            if (i == 2) {
                scale *= 0.97f;
            }
            else {
                continue;
            }
            circleDrawInfo = circleDrawInfo.WithScale(scale).WithColorModifier(opacity);
            batch.DrawWithSnapshot(circleTexture, circlePosition, circleDrawInfo.WithColorModifier(0.4f * globalOpacity).WithScale(globalScale), blendState: BlendState.Additive);
        }
        circleTexture = ResourceManager.Circle9;
        circlePosition = Projectile.Center;
        circleClip = Utils.Frame(circleTexture, 1, 2, frameY: 1);
        circleOrigin = circleClip.Centered();
        circleColor = new Color(255, 255, 86) * 0.375f * globalOpacity;
        circleDrawInfo = new() {
            Clip = circleClip,
            Origin = circleOrigin,
            Color = circleColor
        };
        batch.DrawWithSnapshot(circleTexture, circlePosition, circleDrawInfo.WithScale(2f).WithScale(globalScale), blendState: BlendState.Additive);

        return false;
    }
}
