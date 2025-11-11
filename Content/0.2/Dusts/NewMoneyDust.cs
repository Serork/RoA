using Microsoft.Xna.Framework;

using RoA.Content.Projectiles.Friendly.Ranged;
using RoA.Core.Utility;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class NewMoneyDust : ModDust {
    public override Color? GetAlpha(Dust dust, Color lightColor) {
        Color color = NewMoneyBullet.BulletColor * MathHelper.Min(dust.scale, 1f);
        color.A /= 2;
        return color;
    }

    public override void OnSpawn(Dust dust) {
        dust.velocity.Y = (float)Main.rand.Next(-10, 6) * 0.1f;
        dust.velocity.X *= 0.3f;
        dust.scale *= 0.7f;
    }

    public override bool Update(Dust dust) {
        dust.BasicDust();

        if (!dust.noGravity)
            dust.velocity.Y += 0.05f;

        if (dust.customData != null && dust.customData is NPC) {
            NPC nPC = (NPC)dust.customData;
            dust.position += nPC.position - nPC.oldPos[1];
        }
        else if (dust.customData != null && dust.customData is Player) {
            Player player5 = (Player)dust.customData;
            dust.position += player5.position - player5.oldPosition;
        }
        else if (dust.customData != null && dust.customData is Vector2) {
            Vector2 vector4 = (Vector2)dust.customData - dust.position;
            if (vector4 != Vector2.Zero)
                vector4.Normalize();

            dust.velocity = (dust.velocity * 4f + vector4 * dust.velocity.Length()) / 5f;
        }

        if (!dust.noLight && !dust.noLightEmittence) {
            Lighting.AddLight(dust.position, NewMoneyBullet.BulletColor.ToVector3() * dust.scale * 0.25f);
        }

        return false;
    }
}
