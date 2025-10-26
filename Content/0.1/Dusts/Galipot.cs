using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class Galipot2 : ModDust {
    public override string Texture => DustLoader.GetDust(ModContent.DustType<Galipot>()).Texture;

    public override bool Update(Dust dust) {
        if (dust.velocity.X == 0f) {
            dust.rotation += 0.5f;
            dust.scale -= 0.01f;
        }

        dust.alpha += 2;
        dust.scale -= 0.005f;
        if (dust.alpha > 255)
            dust.scale = 0f;

        if (dust.velocity.Y > 4f)
            dust.velocity.Y = 4f;

        if (dust.noGravity) {
            if (dust.velocity.X < 0f)
                dust.rotation -= 0.2f;
            else
                dust.rotation += 0.2f;

            dust.scale += 0.03f;
            dust.velocity.X *= 1.05f;
            dust.velocity.Y += 0.15f;
        }

        return true;
    }
}

sealed class Galipot : ModDust {
    public override bool Update(Dust dust) {
        if (dust.velocity.X == 0f) {
            if (Collision.SolidCollision(dust.position - Vector2.One, 2, 2))
                dust.scale = 0f;

            dust.rotation += 0.5f;
            dust.scale -= 0.01f;
        }

        dust.alpha += 2;
        dust.scale -= 0.005f;
        if (dust.alpha > 255)
            dust.scale = 0f;

        if (dust.velocity.Y > 4f)
            dust.velocity.Y = 4f;

        if (dust.noGravity) {
            if (dust.velocity.X < 0f)
                dust.rotation -= 0.2f;
            else
                dust.rotation += 0.2f;

            dust.scale += 0.03f;
            dust.velocity.X *= 1.05f;
            dust.velocity.Y += 0.15f;
        }

        if (Collision.SolidCollision(dust.position - Vector2.One * 5f, 10, 10) || Collision.WetCollision(dust.position - Vector2.One * 5f, 10, 10)) {
            dust.alpha += 20;
            dust.scale -= 0.1f;
            dust.scale *= 0.9f;
            dust.velocity *= 0.25f;
        }

        return true;
    }
}