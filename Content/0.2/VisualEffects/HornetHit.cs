using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.VisualEffects;
using RoA.Core;
using RoA.Core.Utility;

using System;

using Terraria;
using Terraria.GameContent;
using Terraria.Graphics.Renderers;

namespace RoA.Content.VisualEffects;

sealed class HornetHit : VisualEffect<HornetHit> {
    public override Texture2D Texture => ResourceManager.DefaultSparkle;

    protected override void SetDefaults() {
        TimeLeft = 7;

        SetFramedTexture(1);

        DontEmitLight = true;
    }

    public override void Update(ref ParticleRendererSettings settings) {
        if (--TimeLeft <= 0) {
            RestInPool();

            return;
        }

        Scale -= 0.025f;
    }

    public override void Draw(ref ParticleRendererSettings settings, SpriteBatch spritebatch) {
        for (int i = 0; i < 2; i++) {
            Color color = Color.Lerp(Lighting.GetColor(Position.ToTileCoordinates()), Color.White, 0.75f) * 0.9f;
            color.A /= 2;
            Texture2D value = Texture;
            Vector2 origin = value.Size() / 2f;
            Color color3 = color * 0.5f;
            float t = ((float)TimeLeft / MaxTimeLeft);
            Vector2 position = Position - Main.screenPosition;
            spritebatch.Draw(value, position, new Rectangle(0, 0, Frame.Width, (int)(Frame.Height * (i == 0 ? 1f : MathUtils.Clamp01(2f * t)))), color * (i == 0 ? t * 0.5f : 1f), Rotation, Origin, 
                Scale * new Vector2(2f, 10f) * MathHelper.Clamp(1f, 0.75f, t) * 0.5f, SpriteEffects.None, 0f);
        }
    }
}
