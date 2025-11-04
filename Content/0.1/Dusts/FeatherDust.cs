using Microsoft.Xna.Framework;

using RoA.Core.Utility;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class FeatherDust : ModDust {
    private Vector2 _velocity;

    public override Color? GetAlpha(Dust dust, Color lightColor) => Color.White * 0.9f;

    public override void OnSpawn(Dust dust) {
        int maxFramesY = 3;
        dust.frame = (Texture2D?.Value?.Frame(4, maxFramesY, frameX: dust.alpha, frameY: Main.rand.Next(maxFramesY))).GetValueOrDefault();

        dust.noGravity = true;
        dust.noLight = false;
    }

    public override bool Update(Dust dust) {
        Helper.ApplyWindPhysics(dust.position, ref dust.velocity);

        bool flag = dust.customData is null || dust.customData is not float v;
        float randomness = 0f;
        if (!flag) {
            randomness = (float)dust.customData;
        }

        Color color = new(170, 252, 134);
        if (dust.alpha == 2) {
            color = new(251, 234, 94);
        }
        if (dust.alpha == 1) {
            color = new(248, 119, 119);
        }
        Lighting.AddLight(dust.position, color.ToVector3() * dust.scale * 0.5f);

        _velocity = Vector2.SmoothStep(_velocity, dust.velocity *= 0.9f, 1f);
        dust.position += _velocity *= 0.99f;

        if (!Collision.SolidCollision(dust.position - Vector2.One * 2, 4, 4)) {
            dust.rotation += Helper.Wave(-0.1f, 0.1f, 0.5f, randomness);

            dust.position.X += Helper.Wave(-1f, 1f, 4f, randomness);
            dust.position.Y += Helper.Wave(0f, 0.5f, 2f, randomness);

            dust.position.Y += 0.2f;
        }
        if ((dust.scale *= 0.99f) <= 0.4f) {
            dust.active = false;
        }

        return false;
    }
}