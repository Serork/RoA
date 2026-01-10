using Microsoft.Xna.Framework;

using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Core.Utility;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class Tulip : ModDust {
    private Vector2 _velocity;

    public const byte COLUMNCOUNT = 8;
    public const byte ROWCOUNT = 3;
    public const byte SOULORANGE = 5;
    public const byte SOULBLUE = 6;
    public const byte SOULGREEN = 7;

    public override Color? GetAlpha(Dust dust, Color lightColor) => dust.alpha >= SOULORANGE ? TulipPetalSoul.SoulColor : lightColor;

    public override void OnSpawn(Dust dust) {
        dust.frame = (Texture2D?.Value?.Frame(COLUMNCOUNT, ROWCOUNT, frameX: dust.alpha, frameY: Main.rand.Next(ROWCOUNT))).GetValueOrDefault();

        dust.noGravity = true;
        dust.noLight = false;
    }

    public override bool Update(Dust dust) {
        if (dust.customData is null) {
            return false;
        }

        float randomness = (float)dust.customData;

        _velocity = Vector2.SmoothStep(_velocity, dust.velocity *= 0.9f, 1f);
        dust.position += _velocity *= 0.99f;

        if (dust.alpha >= SOULORANGE) {
            Vector2 position = dust.position;
            if (dust.alpha == SOULBLUE) {
                float num6 = (float)Main.rand.Next(90, 111) * 0.01f;
                num6 *= Main.essScale;
                num6 *= MathUtils.Clamp01(dust.scale * 1.5f);
                Lighting.AddLight((int)position.X / 16, (int)position.Y / 16, 0.1f * num6, 0.1f * num6, 0.6f * num6);
            }
            else if (dust.alpha == SOULORANGE) {
                float num5 = (float)Main.rand.Next(90, 111) * 0.01f;
                num5 *= Main.essScale;
                num5 *= MathUtils.Clamp01(dust.scale * 1.5f);
                Lighting.AddLight((int)position.X / 16, (int)position.Y / 16, 0.5f * num5, 0.3f * num5, 0.05f * num5);
            }
            else if (dust.alpha == SOULGREEN) {
                float num8 = (float)Main.rand.Next(90, 111) * 0.01f;
                num8 *= Main.essScale;
                num8 *= MathUtils.Clamp01(dust.scale * 1.5f);
                Lighting.AddLight((int)position.X / 16, (int)position.Y / 16, 0.1f * num8, 0.5f * num8, 0.2f * num8);
            }
        }

        if (dust.alpha >= 4) {
            dust.scale *= 0.95f;
            if (dust.scale <= 0.1f) {
                dust.active = false;
            }
        }

        if (!Collision.SolidCollision(dust.position - Vector2.One * 2, 4, 4)) {
            Helper.ApplyWindPhysics(dust.position, ref dust.velocity);

            dust.rotation += Helper.Wave(-0.1f, 0.1f, 0.5f, randomness);

            dust.position.X += Helper.Wave(-1f, 1f, 4f, randomness);
            dust.position.Y += Helper.Wave(0f, 0.5f, 2f, randomness);

            dust.position.Y += 0.2f;
        }
        else {
            if ((dust.scale *= 0.99f) <= 0f) {
                dust.active = false;
            }
        }

        return false;
    }
}