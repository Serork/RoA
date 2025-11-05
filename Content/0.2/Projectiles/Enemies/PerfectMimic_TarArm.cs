using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common;
using RoA.Content.Dusts;
using RoA.Content.NPCs.Enemies.Tar;
using RoA.Core;
using RoA.Core.Defaults;
using RoA.Core.Graphics.Data;
using RoA.Core.Utility;

using System;
using System.Collections.Generic;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ModLoader;

using static RoA.Content.NPCs.Enemies.Tar.PerfectMimic;

namespace RoA.Content.Projectiles.Enemies;

[Tracked]
sealed class TarArm : ModProjectile {
    public static ushort ATTACKTIME => 180;

    private GeometryUtils.BezierCurve _bezierCurve = null!;
    private Vector2[] _bezierControls = null!;

    public NPC Owner => Main.npc[(int)Projectile.ai[0]];

    public bool Small => Projectile.ai[2] != 0f;
    public float AttackProgress => (float)Projectile.localAI[0] / ATTACKTIME;
    private int PointCount => 30;

    public float DeathProgress => MathUtils.Clamp01(Owner.ai[3]);
    public bool MimicIsDead => Owner.damage == 0;

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

    private float GetDeathScale() {
        float result = 1f;
        if (MimicIsDead) {
            result += Helper.Wave(0.5f, 1f, 10f, Projectile.whoAmI) * DeathProgress * Utils.GetLerpValue(1.5f, 1f, DeathProgress, true);
        }
        return result;
    }

    public override void AI() {
        Projectile.timeLeft = 10;

        if (!Owner.active || Owner.type != ModContent.NPCType<PerfectMimic>()) {
            Projectile.Kill();
            return;
        }

        Vector2 center = Owner.GetTargetPlayer().Center;
        if (MimicIsDead) {
            center = Vector2.Lerp(center, Owner.Center - Vector2.UnitY * 10f, DeathProgress);
        }
        if (Projectile.Opacity == 0f) {
            if (Small && Projectile.whoAmI % 2 == 0) {
                Projectile.localAI[0] = -ATTACKTIME / 2;
            }
            Projectile.velocity = Projectile.Center.DirectionTo(center);
        }

        Projectile.Opacity = MathHelper.Lerp(Projectile.Opacity, Small ? 0.8f : 1f, 0.2f);

        if (Small) {
            if (Owner.IsGrounded()) {
                Projectile.localAI[0]++;
                if (Projectile.localAI[0] == (int)(ATTACKTIME * 0.95f)) {
                    float num15 = 0.1f;
                    float num16 = 0.5f;
                    float num10 = 6f;
                    float num18 = 0f;
                    Vector2 vector4 = _bezierCurve.GetPoints(PointCount)[PointCount - 1];
                    Vector2 v = Owner.GetTargetData().Center - vector4;
                    v.Y -= Math.Abs(v.X) * num15;
                    Vector2 vector5 = v.SafeNormalize(-Vector2.UnitY) * num10;
                    Vector2 vector6 = vector5;
                    Vector2 vector7 = vector4;
                    vector6 += Utils.RandomVector2(Main.rand, 0f - num16, num16);

                    vector7 += vector5 * num18;
                    SoundEngine.PlaySound(Main.rand.NextBool() ? PerfectMimic.Shoot1Sound : PerfectMimic.Shoot2Sound, vector7);
                    if (Main.netMode != 1)
                        Projectile.NewProjectile(Projectile.GetSource_FromAI(), vector7, vector6, ModContent.ProjectileType<TarMass>(), Projectile.damage, Projectile.knockBack, Main.myPlayer);

                    for (int num615 = 0; num615 < 10; num615++) {
                        int num616 = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, ModContent.DustType<TarMetaball>(),
                            vector6.X, vector6.Y, 0, default, Main.rand.NextFloat(1f, 1.2f));
                        Main.dust[num616].noGravity = true;
                        Dust dust2 = Main.dust[num616];
                        dust2.scale *= 1.25f;
                        dust2 = Main.dust[num616];
                        dust2.velocity *= 0.5f;
                        dust2.customData = 1f;
                    }
                }
                if (Projectile.localAI[0] > ATTACKTIME) {
                    Projectile.localAI[0] = 0f;
                }
            }
            if (MimicIsDead) {
                Projectile.localAI[0] = 0f;
            }
        }

        Projectile.direction = Owner.direction;
        Projectile.Center = Owner.Center + Vector2.UnitY * Owner.gfxOffY;
        float distance = Small ? 150f : 350f;
        float add = TimeSystem.LogicDeltaTime * 2f * Projectile.direction;
        if (MimicIsDead) {
            float addFactor = MathF.Max(0.5f, 1f - DeathProgress);
            add *= 1f + DeathProgress * 2f;
            distance *= addFactor;
        }
        Projectile.ai[1] += add;
        Projectile.localAI[1] = MathF.Sin(Projectile.ai[1]);
        float opacityFactor = Projectile.Opacity * (Small ? 1.2f : 1f);
        Projectile.localAI[2] = Helper.Approach(Projectile.localAI[2], 1f * opacityFactor, 0.1f);
        Projectile.rotation = MathHelper.PiOver2 * Projectile.localAI[1] * opacityFactor;
        distance *= opacityFactor;
        Vector2 midControlDir = Projectile.Center.DirectionTo(center).RotatedBy(Projectile.rotation);
        float wave = Helper.Wave(Projectile.ai[1] * 0.1f, MathHelper.PiOver2, -MathHelper.PiOver2, 5f, Projectile.identity);
        Vector2 projCenterAfterVelocity = Projectile.Center + Projectile.velocity.RotatedBy(wave * 0.1f);
        Projectile.velocity = Vector2.Lerp(Projectile.velocity, midControlDir * distance, TimeSystem.LogicDeltaTime);
        _bezierControls = [
            Projectile.Center,
            Projectile.Center + (midControlDir * Projectile.Center.Distance(projCenterAfterVelocity) / 1.5f).RotatedBy(wave),
            projCenterAfterVelocity + new Vector2(-distance * 0.285f, 0).RotatedBy(wave),
            projCenterAfterVelocity
        ];
        _bezierCurve = new GeometryUtils.BezierCurve(_bezierControls);
    }

    public override void OnKill(int timeLeft) {
        List<Vector2> points = _bezierCurve.GetPoints(PointCount);
        if (!Small && Helper.SinglePlayerOrServer) {
            Projectile.NewProjectile(Projectile.GetSource_FromAI(), points[points.Count - 1], Vector2.One.RotatedByRandom(MathHelper.TwoPi), ModContent.ProjectileType<PerfectMimicArm>(),
                0, 0, Main.myPlayer);
        }

        for (int i = 0; i < points.Count - 1; i++) {
            for (int num829 = 0; num829 < 4; num829++) {
                int alpha2 = Main.rand.Next(50, 100);
                int size = (int)(10 * (Small ? 0.8f : 1f));
                int dust = Dust.NewDust(points[i] - Vector2.One * 5f, 10, 10, ModContent.DustType<TarDebuff>(), Alpha: alpha2);
                Main.dust[dust].velocity = Vector2.One.RotatedByRandom(MathHelper.TwoPi);
                if (Main.rand.Next(2) == 0)
                    Main.dust[dust].alpha += 25;

                if (Main.rand.Next(2) == 0)
                    Main.dust[dust].alpha += 25;

                Main.dust[dust].noLight = true;
            }
        }
    }

    public override bool PreDraw(ref Color lightColor) => false;

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        List<Vector2> points = _bezierCurve.GetPoints(PointCount);
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

        List<Vector2> points = _bezierCurve.GetPoints(PointCount);
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
            //if (Projectile.whoAmI % 3 == 0) {
            //    flag2 = true;
            //}
            if (i % 2 == 0) {
                texture = indexedTextureAssets[(byte)PerfectMimicRequstedTextureType.Part2].Value;
            }
            //if (i % 3 == 0) {
            //    texture = indexedTextureAssets[(byte)PerfectMimicRequstedTextureType.Part3].Value;
            //}
            Vector2 current = points[i];
            Vector2 position = current;
            Rectangle clip = texture.Bounds;
            Vector2 origin = clip.Centered();
            Color color = Lighting.GetColor(position.ToTileCoordinates()) * Projectile.Opacity;
            Vector2 scale = Vector2.One * (1f - (float)i / (points.Count * 3f)) * Projectile.Opacity;
            if (Small) {
                float min = (float)i / points.Count;
                float max = (float)MathF.Min(points.Count, i + 1) / points.Count;
                scale *= Utils.Remap(Utils.GetLerpValue(min, max, AttackProgress, true) * Utils.GetLerpValue(max * 2f, max * 1.5f, AttackProgress, true)
                    * Utils.GetLerpValue(ATTACKTIME, ATTACKTIME * 0.9f, Projectile.localAI[0], true), 0f, 1f, 1f, 1.5f, true);
            }
            scale *= GetDeathScale();
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
            if (!Small && i == points.Count - 1) {
                texture = flag2 ? headTexture : armTexture;
                SpriteFrame frame = new(3, 2, 1, (byte)(!flag).ToInt());
                clip = frame.GetSourceRectangle(texture);
                if (flag2) {
                    clip = texture.Bounds;
                }
                origin = clip.Centered() + new Vector2(0f, 2f);
                color = Color.Lerp(Owner.As<PerfectMimic>().PlayerCopy.skinColor, SkinColor, 0.375f).MultiplyRGB(Lighting.GetColor(position.ToTileCoordinates())) * Projectile.Opacity;
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

                frame = frame.With(2, frame.CurrentRow);
                clip = frame.GetSourceRectangle(texture);
                batch.DrawWithSnapshot(() => {
                    batch.Draw(texture, position, DrawInfo.Default with {
                        Clip = clip,
                        Origin = origin,
                        Color = Lighting.GetColor(position.ToTileCoordinates()) * Projectile.Opacity,
                        Scale = scale,
                        Rotation = rotation
                    });
                });
            }
        }
    }
}
