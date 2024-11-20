using Microsoft.Xna.Framework;

using RoA.Common;
using RoA.Content.Projectiles.Friendly;
using RoA.Content.Projectiles.Friendly.Druidic;
using RoA.Core;
using RoA.Core.Utility;
using RoA.Utilities;

using System;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace RoA.Content.Items.Weapons;

sealed class HellfireFractureTest : NatureItem {
    public override string Texture => ResourceManager.EmptyTexture;

    protected override void SafeSetDefaults() {
        Item.SetSize(30, 28);
        Item.SetWeaponValues(4, 6f);
        Item.SetDefaultToUsable(ItemUseStyleID.Swing, 20, 20, false);
        Item.SetDefaultToShootable((ushort)ModContent.ProjectileType<HellfireFractureTestProjectile>(), 1f);
    }
}

sealed class HellfireFractureTestProjectile : NatureProjectile {
    private Vector2 _first, _last;

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
        Projectile.localNPCHitCooldown = 30;

        ShouldIncreaseWreathPoints = false;
    }

    public override void AI() {
        Player player = Main.player[Projectile.owner];
        bool flag = Projectile.ai[0] < 5f;
        if (flag && Projectile.ai[1] > 0f) {
            Projectile.ai[1] -= TimeSystem.LogicDeltaTime * 5f;
            Projectile.ai[0] += 0.25f;
        }
        Projectile.ai[0] = Math.Min(Projectile.ai[0], 5f);
        if (player.whoAmI != Main.myPlayer || Projectile.ai[2] == 0f) {
            return;
        }
        Projectile proj = Main.projectile[(int)Projectile.ai[2]];
        if (flag && proj != null && proj.active) {
            Projectile.position = proj.As<HellfireClawsSlash>().GetPos();
            Projectile.position += Vector2.UnitY * 7f * -player.direction;
            Projectile.position += Helper.VelocityToPoint(Projectile.position, player.Center, Projectile.ai[0]) * 7f;
            Projectile.velocity = Helper.VelocityToPoint(player.Center, Projectile.position, 1f).SafeNormalize(Vector2.Zero);
            float height = 100f;
            Vector2 baseValue = Projectile.position;
            _first = _last = baseValue;
            _last.Y = _first.Y + Projectile.ai[0] * 0.2f * height;
            //Projectile.ai[0] += 0.25f;
        }
    }

    private static uint PseudoRand(ref uint seed) {
        seed ^= seed << 13;
        seed ^= seed >> 17;
        return seed;
    }

    private static float PseudoRandRange(ref uint seed, float min, float max) => min + (float)((double)(PseudoRand(ref seed) & 1023U) / 1024.0 * ((double)max - (double)min));

    public override bool PreDraw(ref Color lightColor) {
        DrawSlash();

        return base.PreDraw(ref lightColor);
    }

    private void DrawSlash() {
        uint seed = (uint)(Projectile.position.GetHashCode() + Projectile.velocity.GetHashCode());
        float rot = Helper.VelocityAngle(Projectile.velocity) + MathHelper.PiOver2;
        rot += MathHelper.Pi;
        if (Projectile.direction == 1) {
            rot += 0.2f;
        }
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
        Color color = Color.White;
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
            int max = (int)(4 * Math.Min(2f, progress / gap2) * (progress <= gap1 ? 1f : (1f - (progress - gap1) / gap2)));
            for (int i = 0; i < max; i++) {
                int index = i;
                if (i > max / 2) {
                    index = -(i - max / 2);
                }
                Vector2 posOffset = Projectile.velocity * index;
                Main.spriteBatch.Line(start + posOffset - offset, vector2_4 + posOffset + vec - offset, color, drawSize);
            }
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
                    Main.spriteBatch.Line(start - offset, vector2_4 + vec2 - offset, color, drawSize);
                    width += width2 * (0.04f + random.NextFloatRange(0.02f));
                }
                while (width < width2);
            }
            start = vector2_3;
        }
        while ((double)num4 < (double)num1);
    }
}
