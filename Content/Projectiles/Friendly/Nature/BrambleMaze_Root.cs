using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Core;
using RoA.Core.Defaults;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using System;
using System.Collections.Generic;

using Terraria;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class BrambleMazeRoot : NatureProjectile {
    private static ushort TIMELEFT => MathUtils.SecondsToFrames(2);

    private bool _shouldDisappear;

    public override void SetStaticDefaults() {
        Projectile.SetFrameCount(4);
    }

    protected override void SafeSetDefaults() {
        Projectile.SetSizeValues(24, 16);

        Projectile.friendly = true;
        Projectile.penetrate = -1;
        Projectile.tileCollide = true;

        Projectile.timeLeft = TIMELEFT;

        Projectile.manualDirectionChange = true;

        Projectile.Opacity = 0f;

        Projectile.hide = true;
    }

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
        behindProjectiles.Add(index);
    }

    public override bool OnTileCollide(Vector2 oldVelocity) {
        return false;
    }

    public override void AI() {
        if (Projectile.GetOwnerAsPlayer().GetCommon().IsBrambleMazePlaced && !_shouldDisappear) {
            _shouldDisappear = true;
        }
        if (!_shouldDisappear) {
            Projectile.timeLeft++;
        }

        if (Projectile.localAI[1] == 0f) {
            Projectile.localAI[1] = 1f;

            if (Main.rand.NextChance(0.75f)) {
                Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
            }

            int frame = (int)Projectile.ai[2];
            Projectile.frame = Projectile.ai[0] == 0f ? 0 : frame;

            Projectile.SetDirection(Projectile.ai[1] == 0f ? Projectile.GetOwnerAsPlayer().direction : (int)Projectile.ai[1]);

            int attempts = 16;
            while (attempts-- > 0) {
                if (!WorldGenHelper.SolidTile(Projectile.BottomLeft.ToTileCoordinates())) {
                    break;
                }
                Projectile.position.Y -= 1f;
            }
            attempts = 16;
            while (attempts-- > 0) {
                if (WorldGenHelper.SolidTile(Projectile.BottomLeft.ToTileCoordinates())) {
                    break;
                }
                Projectile.position.Y += 1f;
            }
            Projectile.position += Helper.OffsetPerSolidTileSlope_Bottom(WorldGenHelper.GetTileSafely(Projectile.BottomLeft.ToTileCoordinates()));
            Projectile.position += Helper.OffsetPerSolidTileSlope_Bottom(WorldGenHelper.GetTileSafely(Projectile.Bottom.ToTileCoordinates()));
        }

        Projectile.velocity *= 0f;

        Projectile.Opacity = Helper.Approach(Projectile.Opacity, 1f, 0.2f);

        if (Projectile.Opacity < 1f) {
            return;
        }
        float max = 15;
        if (Projectile.IsOwnerLocal() && Projectile.localAI[0] == 0f && Projectile.ai[0] < max) {
            Projectile.localAI[0] = 1f;

            Projectile.ai[0]++;

            Player player = Projectile.GetOwnerAsPlayer();
            Vector2 mousePosition = player.GetViableMousePosition();
            float xDif = mousePosition.X - Projectile.Center.X;
            int direction = xDif.GetDirection();
            if (MathF.Abs(xDif) <= TileHelper.TileSize) {
                Projectile.ai[0] = max;
                direction = Projectile.direction;
            }

            int frame = Projectile.frame;
            frame++;
            if (frame > Projectile.GetFrameCount() - 1) {
                frame = 0;
            }

            Vector2 position = Projectile.Center + Vector2.UnitX * (Projectile.width + 2) * Projectile.direction;
            int damage = Projectile.damage;
            float knockBack = Projectile.knockBack;
            if (Projectile.ai[0] == max) {
                ProjectileUtils.SpawnPlayerOwnedProjectile<BrambleMazeTrap>(new ProjectileUtils.SpawnProjectileArgs(player, Projectile.GetSource_FromThis()) {
                    Position = position,
                    Damage = damage,
                    KnockBack = knockBack,
                    AI1 = direction
                });
                return;
            }
            ProjectileUtils.SpawnPlayerOwnedProjectile<BrambleMazeRoot>(new ProjectileUtils.SpawnProjectileArgs(player, Projectile.GetSource_FromThis()) {
                Position = position,
                Damage = damage,
                KnockBack = knockBack,
                AI0 = Projectile.ai[0],
                AI1 = direction,
                AI2 = frame
            });
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D texture = Projectile.GetTexture();
        Rectangle clip = Utils.Frame(texture, 1, Projectile.GetFrameCount(), frameY: Projectile.frame);
        float opacity = Projectile.Opacity;
        float opacity3 = opacity + 0.075f;
        float borderColorRGBFactor = 0.5f;
        Vector2 position = Projectile.position;
        bool facedRight = Projectile.FacedRight();
        int width = clip.Width;
        if (!facedRight) {
            Projectile.position.X += width;
        }
        Projectile.position.Y += 2f;
        clip.Width = (int)(width * opacity3);
        Projectile.position.X -= width * (1f - opacity3 * facedRight.ToDirectionInt() + (!facedRight).ToInt()) / 2f;
        float opacity2 = Ease.QuadOut(opacity);
        opacity2 *= Utils.GetLerpValue(0, 30, Projectile.timeLeft, true);
        Projectile.QuickDrawAnimated(lightColor.ModifyRGB(borderColorRGBFactor) * opacity2, frameBox: clip);
        Projectile.position = position;
        if (!facedRight) {
            Projectile.position.X += width;
        }
        Projectile.position.X -= width * (1f - opacity * facedRight.ToDirectionInt() + (!facedRight).ToInt()) / 2f;
        clip.Width = (int)(width * opacity);
        Projectile.QuickDrawAnimated(lightColor * opacity2, frameBox: clip);
        Projectile.position = position;

        return false;
    }

    public override void OnKill(int timeLeft) {
        
    }
}
