using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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

        if (dust.customData != null && dust.customData is Player) {
            Player player9 = (Player)dust.customData;
            dust.position += player9.position - player9.oldPosition;
        }

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
        DustHelper.KillDustThatOutOfScreen(dust);

        dust.fadeIn += 0.05f;
        dust.velocity = dust.velocity.SafeNormalize() * MathF.Sin(dust.fadeIn) * 1f;
        dust.velocity += Vector2.UnitX * MathF.Sin(dust.fadeIn) * 1f;

        if (Collision.SolidCollision(dust.position - Vector2.One * 2, 4, 4)) {
            dust.alpha += 3;
            if (dust.alpha >= 255) {
                dust.active = false;
            }
            dust.velocity *= 0.25f;
        }
        else {
            if (Helper.CanApplyWindPhysics(dust.position, dust.scale)) {
                dust.scale = Main.WindForVisuals * 1.5f;
            }
            dust.position += dust.velocity;
            dust.position.X += dust.scale;
            dust.position.Y += 1f;
            dust.rotation = dust.velocity.X * 0.25f + MathHelper.Pi;
        }

        return false;
    }

    public override bool PreDraw(Dust dust) {
        Microsoft.Xna.Framework.Color newColor = Lighting.GetColor((int)((double)dust.position.X + 4.0) / 16, (int)((double)dust.position.Y + 4.0) / 16);
        if (dust.type == 6 || dust.type == 15 || (dust.type >= 59 && dust.type <= 64))
            newColor = Microsoft.Xna.Framework.Color.White;

        newColor = dust.GetAlpha(newColor);

        Main.spriteBatch.Draw(Texture2D.Value, dust.position - Main.screenPosition, dust.frame, newColor, dust.GetVisualRotation(), new Vector2(4f, 4f), 1f, SpriteEffects.None, 0f);
        if (dust.color.PackedValue != 0) {
            Microsoft.Xna.Framework.Color color6 = dust.GetColor(newColor);
            if (color6.PackedValue != 0)
                Main.spriteBatch.Draw(Texture2D.Value, dust.position - Main.screenPosition, dust.frame, color6, dust.GetVisualRotation(), new Vector2(4f, 4f), 1f, SpriteEffects.None, 0f);
        }

        return false;
    }
}

sealed class CarrionCane3 : ModDust {
    public override void SetStaticDefaults() => UpdateType = DustID.Plantera_Green;
}

