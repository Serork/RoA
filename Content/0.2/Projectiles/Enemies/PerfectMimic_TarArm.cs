using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common;
using RoA.Content.NPCs.Enemies.Tar;
using RoA.Core;
using RoA.Core.Defaults;
using RoA.Core.Graphics.Data;
using RoA.Core.Utility;

using System;
using System.Collections.Generic;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

using static RoA.Content.NPCs.Enemies.Tar.PerfectMimic;

namespace RoA.Content.Projectiles.Enemies;

[Tracked]
sealed class TarArm : ModProjectile {
    private GeometryUtils.BezierCurve _bezierCurve = null!;
    private Vector2[] _bezierControls = null!;

    public NPC Owner => Main.npc[(int)Projectile.ai[0]];

    public override string Texture => ResourceManager.EmptyTexture;

    public override void SetDefaults() {
        Projectile.SetSizeValues(10);

        Projectile.hostile = true;
        Projectile.tileCollide = false;
        Projectile.hide = true;
        Projectile.Opacity = 0f;
        Projectile.aiStyle = -1;
    }

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
        behindNPCs.Add(index);
    }

    public override void AI() {
        Projectile.timeLeft = 2;

        Projectile.Opacity = MathHelper.Lerp(Projectile.Opacity, 1f, 0.1f);

        if (!Owner.active || Owner.type != ModContent.NPCType<PerfectMimic>()) {
            Projectile.Kill();
            return;
        }

        Projectile.direction = Owner.direction;
        Projectile.Center = Owner.Center + Vector2.UnitY * Owner.gfxOffY;
        float distance = 350f;
        Vector2 center = Owner.GetTargetPlayer().Center;
        Projectile.ai[1] += TimeSystem.LogicDeltaTime * 2f;
        Projectile.localAI[1] = MathF.Sin(Projectile.ai[1]);
        Projectile.localAI[2] = Helper.Approach(Projectile.localAI[2], 1f, 0.1f);
        Projectile.rotation = MathHelper.PiOver2 * Projectile.localAI[1];
        Vector2 midControlDir = Projectile.Center.DirectionTo(center).RotatedBy(Projectile.rotation);
        float wave = Helper.Wave(Projectile.ai[1] * 0.1f, MathHelper.PiOver2, -MathHelper.PiOver2, 5f, Projectile.identity);
        Vector2 projCenterAfterVelocity = Projectile.Center + Projectile.velocity.RotatedBy(wave * 0.1f);
        Projectile.velocity = Vector2.Lerp(Projectile.velocity, midControlDir * distance, TimeSystem.LogicDeltaTime);
        _bezierControls = [
            Projectile.Center,
            Projectile.Center + (midControlDir * Projectile.Center.Distance(projCenterAfterVelocity) / 1.5f).RotatedBy(wave),
            projCenterAfterVelocity + new Vector2(-100f, 0).RotatedBy(wave),
            projCenterAfterVelocity
        ];
        _bezierCurve = new GeometryUtils.BezierCurve(_bezierControls);
    }

    public override void OnKill(int timeLeft) {

    }

    public override bool PreDraw(ref Color lightColor) => false;

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        List<Vector2> points = _bezierCurve.GetPoints(30);
        for (int i = 0; i < points.Count; i++) {
            if (GeometryUtils.CenteredSquare(points[i], (int)(10 * (1f - (float)i / (points.Count * 3f)) * Projectile.Opacity)).Intersects(targetHitbox)) {
                return true;
            }
        }

        return false;
    }

    public void DrawFluidSelf() {
        if (!AssetInitializer.TryGetRequestedTextureAssets<PerfectMimic>(out Dictionary<byte, Asset<Texture2D>> indexedTextureAssets)) {
            return;
        }

        List<Vector2> points = _bezierCurve.GetPoints(30);
        SpriteBatch batch = Main.spriteBatch;
        Texture2D armTexture = indexedTextureAssets[(byte)PerfectMimicRequstedTextureType.PlayerArm].Value;
        Texture2D headTexture = indexedTextureAssets[(byte)PerfectMimicRequstedTextureType.PlayerHead].Value;
        for (int i = 0; i < points.Count; i++) {
            Texture2D texture = indexedTextureAssets[(byte)PerfectMimicRequstedTextureType.Part1].Value;
            bool flag = false;
            bool flag2 = false;
            if (Projectile.whoAmI % 2 == 0) {
                flag = true;
            }
            if (Projectile.whoAmI % 3 == 0) {
                flag2 = true;
            }
            if (i % 2 == 0) {
                texture = indexedTextureAssets[(byte)PerfectMimicRequstedTextureType.Part2].Value;
            }
            if (i % 3 == 0) {
                texture = indexedTextureAssets[(byte)PerfectMimicRequstedTextureType.Part3].Value;
            }
            Vector2 current = points[i];
            Vector2 position = current;
            Rectangle clip = texture.Bounds;
            Vector2 origin = clip.Centered();
            Color color = Lighting.GetColor(position.ToTileCoordinates()) * Projectile.Opacity;
            Vector2 scale = Vector2.One * (1f - (float)i / (points.Count * 3f)) * Projectile.Opacity;
            float rotation = Helper.Wave(-MathHelper.Pi, MathHelper.Pi, 1f, i) * Projectile.direction;
            batch.DrawWithSnapshot(() => {
                batch.Draw(texture, position, DrawInfo.Default with {
                    Clip = clip,
                    Origin = origin,
                    Color = color,
                    Scale = scale,
                    Rotation = rotation
                });
            });
            if (i == points.Count - 1) {
                texture = flag2 ? headTexture : armTexture;
                SpriteFrame frame = new(1, 2, 0, (byte)(!flag).ToInt());
                clip = frame.GetSourceRectangle(texture);
                if (flag2) {
                    clip = texture.Bounds;
                }
                origin = clip.Centered();
                color = Color.Lerp(Color.White, SkinColor, 0.25f).MultiplyRGB(Lighting.GetColor(position.ToTileCoordinates())) * Projectile.Opacity;
                scale = Vector2.One * (1f - (float)i / (points.Count * 3f)) * Projectile.Opacity * 2f;
                //rotation += MathHelper.Pi;
                batch.DrawWithSnapshot(() => {
                    batch.Draw(texture, position, DrawInfo.Default with {
                        Clip = clip,
                        Origin = origin,
                        Color = color,
                        Scale = scale,
                        Rotation = rotation
                    });
                });
            }
        }
    }
}
