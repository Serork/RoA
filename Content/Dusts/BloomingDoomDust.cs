using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace RoA.Content.Dusts;

sealed class BloomingDoomDust : ModDust {
    public override Color? GetAlpha(Dust dust, Color lightColor) => lightColor;

    public override void OnSpawn(Dust dust) {
        dust.velocity.Y = (float)Main.rand.Next(-10, 6) * 0.1f;
        dust.velocity.X *= 0.3f;
        dust.scale *= 0.7f;

        WeightedRandom<int> random = new();
        random.Clear();
        random.needsRefresh = true;
        random.Add(0);
        random.Add(1, 0.25);
        random.Add(2, 0.25);
        int frame = random.Get();
        dust.frame = (Texture2D?.Value?.Frame(3, 3, frameX: frame, frameY: Main.rand.Next(3))).GetValueOrDefault();
    }

    public override bool Update(Dust dust) {
        if (!dust.noGravity)
            dust.velocity.Y += 0.05f;

        if (dust.customData != null && dust.customData is Player) {
            Player player9 = (Player)dust.customData;
            dust.position += player9.position - player9.oldPosition;
        }

        if (dust.frame.X < 10 && !dust.noLight && !dust.noLightEmittence) {
            float num56 = dust.scale * 1.4f;
            if (num56 > 0.5f)
                num56 = 0.5f;

            float r = 75 / 255f;
            float g = 115 / 255f;
            float b = 190 / 255f;
            Lighting.AddLight(dust.position, new Vector3(r * num56, g * num56, b * num56));
        }

        return true;
    }
}