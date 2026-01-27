using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common;
using RoA.Common.Cache;
using RoA.Common.Druid;
using RoA.Common.Druid.Wreath;
using RoA.Core;
using RoA.Core.Utility;
using RoA.Core.Utility.Vanilla;

using System;
using System.IO;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class RootRing : NatureProjectile {
    private int _alpha = 255;

    private static float _factor;

    public override Color? GetAlpha(Color lightColor) => new Color(Color.White.R, Color.White.G, Color.White.B, 0) * 0.9f;

    protected override void SafeSetDefaults() {
        int width = 296; int height = width;
        Projectile.Size = new Vector2(width, height);

        Projectile.penetrate = -1;
        Projectile.friendly = true;

        Projectile.hostile = false;

        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;

        Projectile.aiStyle = -1;

        //Projectile.light = 0.25f;

        Projectile.appliesImmunityTimeOnSingleHits = true;
        Projectile.usesIDStaticNPCImmunity = true;
        Projectile.idStaticNPCHitCooldown = 30;

        ShouldChargeWreathOnDamage = false;
    }

    protected override void SafeSendExtraAI(BinaryWriter writer) {
        base.SafeSendExtraAI(writer);

        writer.Write(_alpha);
    }

    protected override void SafeReceiveExtraAI(BinaryReader reader) {
        base.SafeReceiveExtraAI(reader);

        _alpha = reader.ReadInt32();
    }

    public override void AI() {
        Projectile.timeLeft = 2;

        Player player = Main.player[Projectile.owner];

        float projOpacity = 1f - (float)_alpha / 255f;
        Lighting.AddLight(Projectile.Center, new Color(248, 119, 119).ToVector3() * projOpacity * 1.125f);

        Vector2 position = new((float)(player.Center.X - (Projectile.width / 2)), (float)(player.Center.Y - 90 + (float)(player.gfxOffY - 60.0 +
            (player.gravDir == -1f ? -120f : 0f))));
        Projectile.position = new Vector2((int)position.X, (int)position.Y);
        if (player.gravDir == -1.0) Projectile.position.Y += 120f;
        Projectile.rotation += 0.03f;

        if (Projectile.owner == Main.myPlayer) {
            var stats = player.GetWreathHandler();
            Projectile.ai[0] = stats.PulseIntensity;
            Projectile.ai[2] = stats.ActualProgress2;

            bool flag = false;
            if (!player.GetModPlayer<DruidStats>().SoulOfTheWoods || !player.GetWreathHandler().IsFull2 || player.dead) {
                flag = true;
                _alpha += 15;
                if (_alpha >= 255) {
                    _alpha = 255;
                    Projectile.Kill();
                    return;
                }
            }

            if (!flag) {
                FadeIn();
            }

            Projectile.netUpdate = true;
        }
    }

    private void FadeIn() {
        if (_alpha >= 10) {
            _alpha -= 10;
        }
    }

    public override bool? CanHitNPC(NPC target)
        => target.CanBeChasedBy() && Vector2.Distance(target.Center, Main.player[Projectile.owner].Center) >= 110f && Vector2.Distance(target.Center, Main.player[Projectile.owner].Center) <= 170f;

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
    }

    public override bool? CanCutTiles()
        => false;

    public override bool PreDraw(ref Color lightColor) {
        SpriteBatch batch = Main.spriteBatch;
        SpriteBatchSnapshot snapshot = batch.CaptureSnapshot();
        batch.End();
        batch.Begin(SpriteSortMode.Deferred, snapshot.blendState, SamplerState.PointClamp, snapshot.depthStencilState, snapshot.rasterizerState, snapshot.effect, Main.GameViewMatrix.ZoomMatrix);
        Texture2D texture = Projectile.GetTexture();
        Vector2 position = Projectile.Center - Main.screenPosition;
        Player player = Main.player[Projectile.owner];
        var stats = player.GetWreathHandler();
        float projOpacity = 1f - (float)_alpha / 255f;
        Color color = WreathHandler.SoulOfTheWoodsBaseColor * 0.9f * (0.5f + 0.5f * (1f - Projectile.ai[0]));
        float multiplier = 0.035f;
        //for (int i = 0; i < 2; i++)
        //batch.DrawSelf(texture, position, null, color * _fading2, Projectile._rotation, new Vector2(Projectile.width / 2, Projectile.height / 2), Projectile.scale + (i < 1 ? multiplier : -multiplier) * _fading2, SpriteEffects.None, 0);

        batch.Draw(texture, position, null, color * projOpacity, Projectile.rotation, new Vector2(Projectile.width / 2, Projectile.height / 2), Projectile.scale, SpriteEffects.None, 0);

        float progress = MathHelper.Clamp(Projectile.ai[2], 0f, 1f);
        float opacity = 1f;
        float factor = Ease.CircOut((float)(TimeSystem.TimeForVisualEffects % 1.0) / 12f) * Math.Min(opacity > 0.75f ? 0.75f - opacity * (1f - opacity) : 0.925f, 0.925f);
        _factor = MathHelper.Lerp(_factor, factor, _factor < factor ? 0.1f : 0.025f);
        factor = _factor * Projectile.ai[0];
        color *= 1.4f;
        color.A = 70;
        color *= opacity;
        opacity = progress < 1f ? Ease.CubeInOut(progress) : 1f;
        color *= factor * opacity * 2f;
        color *= 2f;
        float scale = Projectile.scale + factor * 0.035f;
        for (int i = 0; i < 3; i++) {
            batch.Draw(texture, position, null, color * projOpacity, Projectile.rotation, new Vector2(Projectile.width / 2, Projectile.height / 2), scale, SpriteEffects.None, 0);
        }

        float scale2 = Projectile.scale + (float)(0.15f * Math.Sin(Main.timeForVisualEffects / 10.0));
        batch.Draw(texture, position, null,
            (color * projOpacity).MultiplyAlpha(scale) * 0.75f,
            Projectile.rotation, new Vector2(Projectile.width / 2, Projectile.height / 2),
            scale2, SpriteEffects.None, 0);

        batch.Begin(in snapshot, true);
        return false;
    }
}
