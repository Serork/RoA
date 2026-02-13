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

        if (WorldGenHelper.GetTileSafely(Projectile.position.ToTileCoordinates()).IsHalfBlock) {
            Projectile.position.Y -= 3f;
        }

        Projectile.netUpdate = true;
    }

    public override void AI() {
        Projectile.hide = Projectile.Opacity < 0.75f;

        Projectile.Opacity = Helper.Approach(Projectile.Opacity, 1f, 0.1f);

        if (!Init) {
            Init = true;

            Projectile.SetDirection(Main.rand.NextBool().ToDirectionInt());

            Projectile.frame = Main.rand.Next(3);

            Projectile.localAI[2] = Main.rand.NextFloat(10f);
        }

        Projectile.ai[1] = Helper.Approach(Projectile.ai[1], 1f, 0.1f);

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
        sourceRectangle.Height = Math.Clamp(height, 2, baseHeight);

        Projectile.position.Y += baseHeight / 2 * (progress);

        Vector2 scale = Vector2.One * progress;

        Projectile.QuickDrawAnimated(lightColor * Projectile.Opacity, scale: scale, frameBox: sourceRectangle, origin: sourceRectangle.BottomCenter());

        Projectile.position = position;

        return false;
    }
}
