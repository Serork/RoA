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

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class FungalCaneSmallShroom : NatureProjectile {
    private static ushort TIMELEFT => MathUtils.SecondsToFrames(5);

    public ref float InitValue => ref Projectile.localAI[0];

    public bool Init {
        get => InitValue != 0f;
        set => InitValue = value.ToInt();
    }

    public override void SetStaticDefaults() {
        Projectile.SetFrameCount(3);
    }

    protected override void SafeSetDefaults() {
        Projectile.SetSizeValues(16);
        Projectile.aiStyle = -1;
        Projectile.friendly = true;
        Projectile.timeLeft = 300;
        Projectile.penetrate = -1;

        Projectile.tileCollide = false;

        Projectile.netImportant = true;

        Projectile.timeLeft = TIMELEFT;

        //Projectile.hide = true;

        Projectile.manualDirectionChange = true;

        Projectile.Opacity = 0f;
    }

    public override bool? CanDamage() => false;
    public override bool? CanCutTiles() => false;

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
        if (Projectile.hide) {
            behindNPCsAndTiles.Add(index);
        }
    }

    protected override void SafeOnSpawn(IEntitySource source) {
        if (Projectile.owner != Main.myPlayer) {
            return;
        }

        Player player = Main.player[Projectile.owner];
        EvilBranch.GetPos(player, out Point point, out Point point2, maxDistance: 800f);
        Projectile.Center = point2.ToWorldCoordinates();
        Projectile.position.X += Main.rand.NextFloatDirection() * TileHelper.TileSize * 2f;
        while (!WorldGen.SolidTile(Projectile.position.ToTileCoordinates())) {
            Projectile.position.Y++;
        }
        Projectile.position.Y -= Projectile.height;
        Projectile.position.Y += 5f;

        while (Collision.SolidCollision(Projectile.Center, 0, 0)) {
            Projectile.position.Y--;
        }
        Projectile.position.Y -= 0f;

        if (WorldGenHelper.GetTileSafely(Projectile.position.ToTileCoordinates() + new Point(1, 0)).IsHalfBlock) {
            Projectile.position.Y -= 3f;
        }

        Projectile.netUpdate = true;
    }

    public override void AI() {
        Projectile.hide = Projectile.Opacity < 0.75f;

        if (!Init) {
            Init = true;

            Projectile.SetDirection(Main.rand.NextBool().ToDirectionInt());

            Projectile.frame = Main.rand.Next(3);

            Projectile.localAI[2] = Main.rand.NextFloat(10f);
        }

        if (Main.rand.NextBool(200)) {
            for (int i = 0; i < 1; i++) {
                Vector2 position = Projectile.Center + Main.rand.RandomPointInArea(4f, 10f);
                Vector2 velocity = -Vector2.UnitY * Main.rand.NextFloat(1f, 2f) + Vector2.UnitX * Main.rand.NextFloat(-1f, 1f);
                velocity.Y *= 0.25f;
                Dust dust = Dust.NewDustPerfect(position, DustID.GlowingMushroom, velocity, Alpha: 25);
                dust.scale = Main.rand.NextFloat(0.8f, 1.2f);
                dust.scale *= 0.5f;
                dust.noLight = dust.noLightEmittence = true;
                if (Main.rand.NextBool(5)) {
                    dust.noGravity = false;
                }
            }
        }

        float num11 = (float)Main.rand.Next(28, 42) * 0.005f;
        num11 += (float)(270 - Main.mouseTextColor) / 1000f;
        float R = 0f;
        float G = 0.2f + num11 / 2f;
        float B = 1f;
        Lighting.AddLight(Projectile.Center, new Vector3(R, G, B) * 0.75f * Projectile.Opacity);

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

        float maxRotation = 0.05f;
        Projectile.localAI[2] += TimeSystem.LogicDeltaTime;
        Projectile.rotation = Helper.Wave(Projectile.localAI[2] * Projectile.ai[1], -maxRotation, maxRotation, 1f, 0f) * Projectile.ai[1];
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D texture = Projectile.GetTexture();
        Rectangle sourceRectangle = Utils.Frame(texture, 1, 3, frameY: Projectile.frame);
        float progress = Projectile.ai[1];
        progress = Ease.QuadOut(progress);
        int baseHeight = sourceRectangle.Height;
        int height = (int)(baseHeight * progress);
        Vector2 position = Projectile.position;
        Projectile.position.Y += (int)(baseHeight * (1f - progress));
        sourceRectangle.Height = Math.Clamp(height, 0, baseHeight);

        Projectile.position.Y += baseHeight / 2 * (progress);

        Vector2 scale = Vector2.One * (Projectile.timeLeft > 20 ? progress : 1f);

        lightColor = Lighting.GetColor(Projectile.Center.ToTileCoordinates());

        Projectile.QuickDrawAnimated(lightColor * Projectile.Opacity, scale: scale, frameBox: sourceRectangle, origin: sourceRectangle.BottomCenter());

        Projectile.position = position;

        return false;
    }
}
