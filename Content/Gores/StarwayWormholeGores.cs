using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Gores;
using RoA.Content.NPCs.Enemies.Backwoods;
using RoA.Core.Utility;

using System;

using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace RoA.Content.Gores;

class StarwayWormholeGore4 : StarwayWormholeGore1 { }
class StarwayWormholeGore3 : StarwayWormholeGore1 { }
class StarwayWormholeGore2 : StarwayWormholeGore1 { }

class StarwayWormholeGore1 : ModGore, ICustomGoreDraw {
    public override Color? GetAlpha(Gore gore, Color lightColor) => Color.Transparent;

    public override bool Update(Gore gore) {
        GoreHelper.FadeOutOverTime(gore);

        float opacity = 1f - gore.alpha / 255f;
        Vector3 lightColor = new Color(127, 153, 22).ToVector3() * 0.75f * opacity;
        Lighting.AddLight(gore.position, lightColor);

        return base.Update(gore);
    }

    void ICustomGoreDraw.Draw(SpriteBatch spriteBatch, Gore gore) {
        float opacity = 1f - gore.alpha / 255f;
        Color baseColor = Color.White;
        baseColor = baseColor.MultiplyAlpha(0.75f);
        baseColor *= opacity;
        Color color = baseColor * Helper.Wave(0.5f, 0.75f, 5f, 0f);
        spriteBatch.Draw(TextureAssets.Gore[gore.type].Value, new Vector2(gore.position.X - Main.screenPosition.X + (float)(TextureAssets.Gore[gore.type].Width() / 2), gore.position.Y - Main.screenPosition.Y + (float)(TextureAssets.Gore[gore.type].Height() / 2)) + gore.drawOffset, new Microsoft.Xna.Framework.Rectangle(0, 0, TextureAssets.Gore[gore.type].Width(), TextureAssets.Gore[gore.type].Height()),
            color, gore.rotation, new Vector2(TextureAssets.Gore[gore.type].Width() / 2, TextureAssets.Gore[gore.type].Height() / 2), gore.scale, SpriteEffects.None, 0f);

        float num184 = Helper.Wave(2f, 6f, 1f, 0f);
        for (int num185 = 0; num185 < 4; num185++) {
            spriteBatch.Draw(TextureAssets.Gore[gore.type].Value, Vector2.UnitX.RotatedBy((float)num185 * ((float)Math.PI / 4f) - Math.PI) * num184 + new Vector2(gore.position.X - Main.screenPosition.X + (float)(TextureAssets.Gore[gore.type].Width() / 2), gore.position.Y - Main.screenPosition.Y + (float)(TextureAssets.Gore[gore.type].Height() / 2)) + gore.drawOffset, new Microsoft.Xna.Framework.Rectangle(0, 0, TextureAssets.Gore[gore.type].Width(), TextureAssets.Gore[gore.type].Height()),
                new Microsoft.Xna.Framework.Color(64, 64, 64, 0) * 0.25f * opacity,
                gore.rotation, new Vector2(TextureAssets.Gore[gore.type].Width() / 2, TextureAssets.Gore[gore.type].Height() / 2), gore.scale, SpriteEffects.None, 0f);
        }
    }
}
