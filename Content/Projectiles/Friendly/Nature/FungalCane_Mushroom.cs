using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common;
using RoA.Core;
using RoA.Core.Defaults;
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

[Tracked]
sealed class FungalCaneMushroom : NatureProjectile {
    private static ushort TIMELEFT => MathUtils.SecondsToFrames(5);

    private Vector2 _scale;

    public ref float InitValue => ref Projectile.localAI[0];

    public bool Init {
        get => InitValue != 0f;
        set => InitValue = value.ToInt();
    }

    public override void SetStaticDefaults() {
        Projectile.SetFrameCount(2);
    }

    protected override void SafeSetDefaults() {
        Projectile.SetSizeValues(16);
        Projectile.aiStyle = -1;
        Projectile.friendly = true;
        Projectile.timeLeft = TIMELEFT;
        Projectile.penetrate = -1;
        Projectile.hide = true;

        Projectile.tileCollide = false;

        Projectile.netImportant = true;

        Projectile.manualDirectionChange = true;

        Projectile.Opacity = 0f;
    }

    public override bool? CanDamage() => false;
    public override bool? CanCutTiles() => false;

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        => behindNPCsAndTiles.Add(index);

    protected override void SafeOnSpawn(IEntitySource source) {
        if (Projectile.owner != Main.myPlayer) {
            return;
        }

        Player player = Main.player[Projectile.owner];
        EvilBranch.GetPos(player, out Point point, out Point point2, maxDistance: 800f, random: false);
        Projectile.Center = point2.ToWorldCoordinates();
        while (Collision.SolidCollision(Projectile.position, Projectile.width, Projectile.height)) {
            Projectile.position.Y -= 1f;
        }
        Projectile.position.Y -= 5f;

        Projectile.netUpdate = true;
    }

    public override void AI() {
        if (Init) {
            float targetY = 1f;
            float targetX = 1f;
            if (Projectile.ai[0] > 0f) {
                targetY = 0.625f;
                targetX = 1.375f;
                Projectile.rotation = 0f;
            }
            float lerpValue = 0.1f;
            _scale.Y = Helper.Approach(_scale.Y, targetY, lerpValue);
            _scale.X = Helper.Approach(_scale.X, targetX, lerpValue);
        }

        if (!Init) {
            Init = true;

            Projectile.SetDirection(Main.rand.NextBool().ToDirectionInt());

            Projectile.frame = Main.rand.Next(2);

            Projectile.localAI[2] = Main.rand.NextFloat(10f);

            _scale = Vector2.One;
        }

        if (Main.rand.NextBool(100)) {
            for (int i = 0; i < 1; i++) {
                Vector2 position = Projectile.Center + Main.rand.RandomPointInArea(20f, 10f + 20f);
                Vector2 velocity = -Vector2.UnitY * Main.rand.NextFloat(1f, 2f) + Vector2.UnitX * Main.rand.NextFloat(-1f, 1f);
                velocity.Y *= 0.5f;
                Dust dust = Dust.NewDustPerfect(position, ModContent.DustType<Dusts.GlowingMushroom>(), velocity, Alpha: 25);
                dust.scale = Main.rand.NextFloat(0.8f, 1.2f);
                dust.scale *= 0.75f;
                dust.noLight = dust.noLightEmittence = true;
                if (Main.rand.NextBool(5)) {
                    dust.noGravity = false;
                }
            }
        }

        if (Projectile.timeLeft > 20) {
            Projectile.ai[1] = Helper.Approach(Projectile.ai[1], 1f, 0.1f);
            Projectile.Opacity = Helper.Approach(Projectile.Opacity, 1f, 0.1f);
        }
        else {
            float lerpValue = 0.15f;
            Projectile.ai[1] = Helper.Approach(Projectile.ai[1], 0f, lerpValue);
            Projectile.Opacity = Helper.Approach(Projectile.Opacity, 0f, lerpValue * 0.8f);
            if (Projectile.Opacity <= 0f) {
                Projectile.Kill();
            }
        }

        float num11 = (float)Main.rand.Next(28, 42) * 0.005f;
        num11 += (float)(270 - Main.mouseTextColor) / 1000f;
        float R = 0f;
        float G = 0.2f + num11 / 2f;
        float B = 1f;
        Lighting.AddLight(Projectile.Center, new Vector3(R, G, B) * Projectile.Opacity * 0.875f);

        float preparationTime = 5f;
        float delayTime = 30f;
        if (Projectile.ai[2] == 1f) {
            for (int i = 0; i < 1; i++) {
                if (Projectile.IsOwnerLocal()) {
                    Vector2 offset = Main.rand.RandomPointInArea(20f, 10f);
                    ProjectileUtils.SpawnPlayerOwnedProjectile<FungalCaneFumes>(new ProjectileUtils.SpawnProjectileArgs(Projectile.GetOwnerAsPlayer(), Projectile.GetSource_FromThis()) {
                        Position = Projectile.Center + offset - Vector2.UnitY * 5f,
                        Velocity = new Vector2(-4f * Main.rand.NextFloat(0.875f, 1f) * offset.X.GetDirection(), -2f * Main.rand.NextFloat(0.875f, 1f)).RotatedBy(MathHelper.PiOver4 * 0.5f * Main.rand.NextFloatDirection()) * Main.rand.NextFloat(0.15f, 0.325f),
                        Damage = Projectile.damage,
                        KnockBack = Projectile.knockBack
                    });
                }
            }
            Projectile.ai[0] = Helper.Approach(Projectile.ai[0], preparationTime, 1f);
        }
        else {
            Projectile.ai[0] = Helper.Approach(Projectile.ai[0], 0f, 1f);
        }

        if (Projectile.ai[0] >= preparationTime) {
            Projectile.ai[2] = 0f;
            Projectile.ai[0] = -delayTime;
        }

        if (Projectile.ai[0] > 0f) {
            return;
        }

        float maxRotation = 0.05f;
        Projectile.localAI[2] += TimeSystem.LogicDeltaTime;
        Projectile.rotation = Helper.Wave(Projectile.localAI[2] * Projectile.ai[1], -maxRotation, maxRotation, 1f, 0f) * Projectile.ai[1];
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D texture = Projectile.GetTexture();
        Rectangle sourceRectangle = Utils.Frame(texture, 1, 2, frameY: Projectile.frame);
        float progress = Projectile.ai[1];
        progress = Ease.QuadOut(progress);
        int baseHeight = sourceRectangle.Height;
        int height = (int)(baseHeight * progress);
        Vector2 position = Projectile.position;
        Projectile.position.Y += (int)(baseHeight * (1f - progress));
        sourceRectangle.Height = Math.Clamp(height, 0, baseHeight);

        Projectile.position.Y += baseHeight / 2 * (progress);

        Vector2 baseScale = _scale;
        Vector2 scale = baseScale * (Projectile.timeLeft > 20 ? progress : 1f);

        lightColor = Lighting.GetColor(Projectile.Center.ToTileCoordinates());

        Projectile.QuickDrawAnimated(lightColor * Projectile.Opacity, scale: scale, frameBox: sourceRectangle, origin: sourceRectangle.BottomCenter());

        Projectile.position = position;

        return false;
    }
}
