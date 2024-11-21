using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common;
using RoA.Core;
using RoA.Core.Utility;
using RoA.Utilities;

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using Terraria;
using Terraria.ID;
using Terraria.Utilities;

namespace RoA.Content.Projectiles.Friendly.Druidic;

sealed class HellfireFracture : NatureProjectile {
    private Vector2 _first, _last;
    private Color _color;
    private float _timer;
    private List<Vector2> _collisionPoints = [];

    public override string Texture => ResourceManager.EmptyTexture;

    public override bool ShouldUpdatePosition() => false;

    protected override void SafeSetDefaults() {
        Projectile.Size = Vector2.One * 5f;
        Projectile.tileCollide = false;
        Projectile.friendly = true;
        Projectile.ignoreWater = true;

        Projectile.aiStyle = -1;
        Projectile.penetrate = -1;

        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 15;
        Projectile.knockBack = 0f;

        Projectile.timeLeft = 300;

        ShouldIncreaseWreathPoints = false;
    }

    public override void AI() {
        Player player = Main.player[Projectile.owner];
        float max = 4.5f;
        bool flag = Projectile.ai[0] < max;
        //if (flag && Projectile.ai[1] > 0f) {
        //    Projectile.ai[1] -= TimeSystem.LogicDeltaTime * max;
        //    Projectile.ai[0] += 0.15f;
        //}
        Projectile.ai[0] = Math.Min(Projectile.ai[0], max);
        float height = 100f;
        Vector2 baseValue = Projectile.position;
        _first = _last = baseValue;
        _last.Y = _first.Y + Projectile.ai[0] * 0.2f * height;
        Projectile? proj = GetParent();
        if (flag && proj != null && Projectile.ai[1] == 1f) {
            var slash = proj.As<HellfireClawsSlash>();
            if (slash != null) {
                Projectile.ai[1] = 0f;
                Projectile.position = proj.As<HellfireClawsSlash>().GetPos(MathHelper.PiOver4 * 0.5f);
                Projectile.position += Vector2.UnitY * 10f * -player.direction;
                Projectile.position += Helper.VelocityToPoint(Projectile.position, player.Center, Projectile.ai[0]) * 10f;
                Projectile.velocity = Helper.VelocityToPoint(player.Center, Projectile.position, 1f).SafeNormalize(Vector2.Zero);
            }
        }
        else if (Projectile.ai[0] < 1f) {
            Projectile.Kill();
        }
    }

    private Projectile? GetParent() {
        Projectile proj = Main.projectile[(int)Projectile.ai[2]];
        if (proj != null && proj.active) {
            var slash = proj.As<HellfireClawsSlash>();
            if (slash != null) {
                return proj;
            }
        }
        return null;
    }

    private static uint PseudoRand(ref uint seed) {
        seed ^= seed << 13;
        seed ^= seed >> 17;
        return seed;
    }

    private static float PseudoRandRange(ref uint seed, float min, float max) => min + (float)((double)(PseudoRand(ref seed) & 1023U) / 1024.0 * ((double)max - (double)min));

    public override bool PreDraw(ref Color lightColor) {
        Projectile? parent = GetParent();
        if (parent != null) {
            float fromValue = 1f/* - Ease.QuintIn(parent.localAI[0] / parent.ai[1])*/;
            Color color1 = Color.Lerp(new Color(255, 150, 20), new Color(137, 54, 6), fromValue),
                  color2 = Color.Lerp(new Color(200, 80, 10), new Color(96, 36, 4), fromValue);
            _color = Color.Lerp(color1, color2, fromValue) * 0.9f;
        }
        //_color = Color.White;
        _timer++;
        _timer += Main.rand.NextFloatRange(0.5f);
        float count = _timer * 0.75f;
        float num13 = ((float)count / 75f * ((float)Math.PI * 2f)).ToRotationVector2().X * 1f + 0f;
        num13 = Utils.Remap(num13, -1f, 1f, 1.5f, 2f);
        SpriteBatch spriteBatch = Main.spriteBatch;
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.AnisotropicClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);
        DrawSlash(_color * (num13 / 2f * 0.3f + 0.85f), lightColor);
        for (float num14 = -4f; num14 < 4f; num14 += 1f) {
            DrawSlash(_color * (num13 / 2f * 0.3f + 0.85f) * 0.35f, lightColor, posExtra: num14 * ((float)Math.PI / 2f).ToRotationVector2() * 0.35f * num13);
        }
        spriteBatch.EndBlendState();

        return base.PreDraw(ref lightColor);
    }

    public override void PostAI() {
        uint seed = (uint)(Projectile.position.GetHashCode() + Projectile.velocity.GetHashCode());
        float rot = Helper.VelocityAngle(Projectile.velocity) + MathHelper.PiOver2;
        rot += MathHelper.Pi;
        Vector2 pos1 = _first, pos2 = _last;
        Vector2 dif = pos2 - pos1;
        Vector2 vel = dif.SafeNormalize(Vector2.Zero) * dif.Length() / 2f;
        Vector2 center = Projectile.position + vel;
        Vector2 a = pos1.RotatedBy(rot, center);
        Vector2 b = pos2.RotatedBy(rot, center);
        float num1 = (b - a).Length();
        Vector2 vec = (b - a) / num1;
        Vector2 vector2_1 = new(-vec.Y, vec.X);
        Vector2 start = a;
        int num2 = 1;
        float num4 = 0.0f;
        UnifiedRandom random = new((int)seed);
        _collisionPoints.Clear();
        do {
            bool flag4 = false;
            float value = PseudoRandRange(ref seed, 8f, 11f) / 2f;
            num4 += value;
            float progress = num4 / num1;
            float pi = MathHelper.Pi * 0.9f;
            Vector2 vector2_2 = a + vec.RotatedBy(pi * progress - pi) * 0.6f * num4 + vec * num4 * 0.6f;
            Vector2 vector2_3 = /*(double)num4 >= (double)num1 ? b : */vector2_2 + (random.NextBool() ? (float)num2 * vector2_1 * PseudoRandRange(ref seed, -6f, 6f) : Vector2.Zero);
            Vector2 vector2_4 = vector2_3;
            Vector2 offset = Vector2.UnitY * num1 / 2f;
            Vector2 offset2 = Vector2.Zero;
            Vector2 first = start - offset + offset2;
            if (random.NextBool()) {
                num2 = -num2;
            }
            float num56 = Projectile.ai[0] / 5f;
            if (num56 > 0.6f)
                num56 = 0.6f;
            Lighting.AddLight((int)(first.X / 16f), (int)(first.Y / 16f), num56, num56 * 0.65f, num56 * 0.4f);
            _collisionPoints.Add(first);
            start = vector2_3;
        }
        while ((double)num4 < (double)num1);
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        foreach (Vector2 point in _collisionPoints) {
            if (new Rectangle((int)point.X - 2, (int)point.Y - 2, 4, 4).Intersects(targetHitbox)) {
                return true;
            }
        }
        return false;
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo info) {
        base.OnHitPlayer(target, info);

        float num2 = (float)Main.rand.Next(75, 150) * 0.0075f * (Projectile.ai[0] / 5f);
        target.AddBuff(BuffID.OnFire, (int)(60f * num2));
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        base.OnHitNPC(target, hit, damageDone);

        float num2 = (float)Main.rand.Next(75, 150) * 0.0075f * (Projectile.ai[0] / 5f);
        target.AddBuff(BuffID.OnFire, (int)(60f * num2));
    }

    private void DrawSlash(Color color, Color lightColor, Vector2? posExtra = null) {
        uint seed = (uint)(Projectile.position.GetHashCode() + Projectile.velocity.GetHashCode());
        float rot = Helper.VelocityAngle(Projectile.velocity) + MathHelper.PiOver2;
        rot += MathHelper.Pi;
        if (Projectile.direction == 1) {
            rot += 0.2f;
        }
        //color = color.MultiplyRGB(lightColor);
        Vector2 pos1 = _first, pos2 = _last;
        Vector2 dif = pos2 - pos1;
        Vector2 vel = dif.SafeNormalize(Vector2.Zero) * dif.Length() / 2f;
        Vector2 center = Projectile.position + vel;
        Vector2 a = pos1.RotatedBy(rot, center);
        Vector2 b = pos2.RotatedBy(rot, center);
        float num1 = (b - a).Length();
        Vector2 vec = (b - a) / num1;
        Vector2 vector2_1 = new(-vec.Y, vec.X);
        Vector2 start = a;
        int num2 = 1;
        float num4 = 0.0f;
        float minSize = 1f;
        float size = minSize;
        UnifiedRandom random = new((int)seed);
        bool decrease = false;
        int dir = random.NextBool() ? 1 : -1;
        int count = 0;
        do {
            bool flag4 = false;
            if (count > 2) {
                count = 0;
                flag4 = true;
                dir *= -1;
            }
            float value = PseudoRandRange(ref seed, 8f, 11f) / 2f;
            num4 += value;
            count++;
            float progress = num4 / num1;
            float pi = MathHelper.Pi * 0.9f;
            Vector2 vector2_2 = a + vec.RotatedBy(pi * progress - pi) * 0.6f * num4 + vec * num4 * 0.6f;
            Vector2 vector2_3 = /*(double)num4 >= (double)num1 ? b : */vector2_2 + (random.NextBool() ? (float)num2 * vector2_1 * PseudoRandRange(ref seed, -6f, 6f) : Vector2.Zero);
            Vector2 vector2_4 = vector2_3;
            Vector2 offset = Vector2.UnitY * num1 / 2f;
            Vector2 offset2 = Vector2.Zero;
            if (posExtra.HasValue) {
                offset2 = posExtra.Value;
            }
            if (random.NextBool()) {
                num2 = -num2;
            }
            float gap1 = 0.75f;
            float gap2 = 0.25f;
            bool flag = progress > gap1;
            bool flag2 = progress < gap2;
            bool flag3 = !flag && !flag2;
            float maxSize = 5f;
            if (flag3) {
                if (size >= maxSize) {
                    if (random.NextBool(3) && !decrease) {
                        decrease = true;
                    }
                }
                if (decrease) {
                    float minSize2 = 1.75f;
                    if (size > minSize2) {
                        size -= value * 0.25f;
                        if (size < minSize2) {
                            size = minSize2;
                        }
                    }
                    else {
                        decrease = false;
                    }
                }
                else {
                    size += value * 0.25f;
                    if (size > maxSize) {
                        size = maxSize;
                    }
                }
            }
            else {
                decrease = false;
            }
            if (flag) {
                size -= value * 0.05f;
                if (size < minSize) {
                    size = minSize;
                }
            }
            if (flag2) {
                size += value * 0.05f;
                if (size > maxSize) {
                    size = maxSize;
                }
            }
            float drawSize = size * 1f;
            int max = (int)(5 * Math.Min(2f, progress / gap2) * (progress <= gap1 ? 1f : (1f - (progress - gap1) / gap2)));
            for (int i = 0; i < max; i++) {
                int index = i;
                if (i > max / 2) {
                    index = -(i - max / 2);
                }
                Vector2 posOffset = Projectile.velocity * index;
                Main.spriteBatch.Line(start + posOffset - offset + offset2, vector2_4 + posOffset + vec - offset + offset2, color, drawSize);
            }
            float num = 10f;
            Vector2 first = start - offset + offset2;
            Vector2 second = vector2_4 + vec - offset + offset2;
            float f = Helper.VelocityToPoint(first, second, 1f).SafeNormalize(Vector2.Zero).ToRotation();
            Vector2 vector = f.ToRotationVector2();
            int value5 = (int)Projectile.ai[0];
            bool flag6 = Projectile.ai[0] > value5 - 0.2f && Projectile.ai[0] < value5 + 0.2f;
            if (progress > 0.25f && progress < 0.9f && flag4) {
                float width = 0f;
                float width2 = PseudoRandRange(ref seed, 25f, 40f);
                float size2 = size + random.NextFloat(-size / 4f, size / 5f);
                float rot2 = PseudoRandRange(ref seed, -MathHelper.PiOver4 * 0.75f, MathHelper.PiOver4 * 0.75f);
                do {
                    value = PseudoRandRange(ref seed, 8f, 11f) / 2f;
                    size2 -= value * 0.05f;
                    if (size2 < 0f) {
                        size2 = 0f;
                    }
                    Vector2 vec2 = new Vector2(-vec.Y, vec.X).RotatedBy(rot2) * dir * (width / width2) * width2;
                    drawSize = size2 * 1.475f;
                    Main.spriteBatch.Line(start - offset + offset2, vector2_4 + vec2 - offset + offset2, color, drawSize);
                    width += width2 * (0.04f + random.NextFloatRange(0.02f));
                    f = Helper.VelocityToPoint(start - offset + offset2, vector2_4 + vec2 - offset + offset2, 1f).SafeNormalize(Vector2.Zero).ToRotation();
                    vector = f.ToRotationVector2();
                    if (size2 > 0f && Main.rand.NextBool(80 + (int)(20 * (Projectile.ai[0] / 5f)))) {
                        Dust dust = Dust.NewDustPerfect(Vector2.Lerp(start - offset + offset2 - vector * num, start - offset + offset2 + vector * num, Main.rand.NextFloat()), 6,
                            vector.RotatedBy((float)Math.PI * 2f * Main.rand.NextFloatDirection() * 0.02f + Main.rand.NextFloatRange(MathHelper.PiOver4)) * Main.rand.NextFromList(2f, 4f) * Main.rand.NextFloat(0.35f, 0.6f) * Main.rand.NextFloat(), 0, default, (Math.Max(0.75f, size2) * 0.6f + Main.rand.NextFloatRange(0.25f)) * Main.rand.NextFloat(0.75f, 1.1f));
                        dust.fadeIn = (float)(0.4 + (double)Main.rand.NextFloat() * 0.15);
                        dust.noGravity = !Main.rand.NextBool(40);
                        dust.customData = 0;
                    }
                }
                while (width < width2);
            }
            f = Helper.VelocityToPoint(first, second, 1f).SafeNormalize(Vector2.Zero).ToRotation();
            vector = f.ToRotationVector2();
            if (size > minSize && Main.rand.NextBool(80 + (int)(20 * (Projectile.ai[0] / 5f)))) {
                Dust dust = Dust.NewDustPerfect(Vector2.Lerp(start - offset - vector * num, start - offset + vector * num, Main.rand.NextFloat()), 6,
                    vector.RotatedBy((float)Math.PI * 2f * Main.rand.NextFloatDirection() * 0.02f + Main.rand.NextFloatRange(MathHelper.PiOver4)) * Main.rand.NextFromList(2f, 4f) * Main.rand.NextFloat(0.35f, 0.6f) * Main.rand.NextFloat(), 0, default, (Math.Max(2f, size) * 0.6f + Main.rand.NextFloatRange(0.25f)) * Main.rand.NextFloat(0.75f, 1.1f));
                dust.fadeIn = (float)(0.4 + (double)Main.rand.NextFloat() * 0.15);
                dust.noGravity = !Main.rand.NextBool(40);
                dust.customData = 0;
            }
            start = vector2_3;
        }
        while ((double)num4 < (double)num1);
    }
}
