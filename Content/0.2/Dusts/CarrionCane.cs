using Microsoft.Xna.Framework;

using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;

using System;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class CarrionCane : ModDust {
    public override bool Update(Dust dust) {
        DustHelper.BasicDust(dust);

        dust.noGravity = true;

        return false;
    }
}

sealed class CarrionCane2 : ModDust {
    private Vector2 _windVelocity = Vector2.Zero;

    public override void OnSpawn(Dust dust) {
        dust.frame = (Texture2D?.Value?.Frame(1, 3, frameX: dust.alpha, frameY: Main.rand.Next(3))).GetValueOrDefault();

        dust.fadeIn = Main.rand.NextFloat(10f);

        dust.velocity = Vector2.One.RotateRandom(MathHelper.TwoPi);
    }

    public override bool Update(Dust dust) {
        DustHelper.ApplyDustScale(dust);

        dust.fadeIn += 0.05f;
        float sinOffsetX = 1f;
        if (dust.customData is float value) {
            sinOffsetX += value;
            dust.customData = Helper.Approach((float)dust.customData, 0f, 0.01f);
        }
        dust.velocity = dust.velocity.SafeNormalize() * MathF.Sin(dust.fadeIn) * 1f;
        dust.velocity += Vector2.UnitX * MathF.Sin(dust.fadeIn) * 1f;
        Helper.ApplyWindPhysics(dust.position, ref _windVelocity);

        if (Collision.SolidCollision(dust.position - Vector2.One * 2, 4, 4)) {
            dust.alpha += 2;
            if (dust.alpha >= 255) {
                dust.active = false;
            }
            dust.velocity *= 0.25f;
        }
        else {
            dust.position += dust.velocity;
            dust.position += _windVelocity * 0.00025f;
            dust.position.Y += 1f;
            dust.rotation = dust.velocity.X * 0.25f + MathHelper.Pi;
        }

        return false;
    }
}

sealed class CarrionCane3 : ModDust {
    public override void SetStaticDefaults() => UpdateType = DustID.Plantera_Green;
}

