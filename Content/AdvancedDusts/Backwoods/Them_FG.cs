using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common;
using RoA.Common.VisualEffects;
using RoA.Core;
using RoA.Core.Utility;

using Terraria;
using Terraria.Graphics.Renderers;
using Terraria.ModLoader;

namespace RoA.Content.AdvancedDusts.Backwoods;

sealed class Them_FG : AdvancedDust<Them_FG> {
    private float _brightness;
    private byte _textureIndex;

    public float FadeIn { get; private set; }
    public int Alpha { get; private set; }

    public override Texture2D Texture => (Texture2D)ModContent.Request<Texture2D>(ResourceManager.AmbienceTextures + $"them_FG_{_textureIndex + 1}");

    public override void Draw(ref ParticleRendererSettings settings, SpriteBatch spritebatch) {
        Color lightColor = Lighting.GetColor((int)(Position.X / 16f), (int)(Position.Y / 16f));
        float brightness = (lightColor.R / 255f + lightColor.G / 255f + lightColor.B / 255f) / 3f;
        float brightness2 = MathHelper.Clamp((brightness - 0.6f) * 5f, 0f, 1f);
        _brightness = MathHelper.Lerp(_brightness, 1f - brightness2, 0.15f);
        float opacity = _brightness * (1f - Alpha / 255f);
        opacity *= 5f;
        opacity *= Utils.GetLerpValue(0f, 1f, FadeIn, true);
        spritebatch.Draw(Texture, Position - Main.screenPosition + Main.rand.RandomPointInArea(0.075f), null, Color.White * opacity, Rotation, Texture.Size() / 2f, Scale, ShouldFullBright.ToInt().ToSpriteEffects(), 0f);
        spritebatch.Draw(Texture, Position - Main.screenPosition + Main.rand.RandomPointInArea(7.5f), null, Color.White * 0.1f * opacity, Rotation, Texture.Size() / 2f, Scale, ShouldFullBright.ToInt().ToSpriteEffects(), 0f);
    }

    protected override void SetDefaults() {
        _textureIndex = (byte)Main.rand.Next(2);

        _brightness = 0f;

        Alpha = 255;
        FadeIn = 10f;

        Velocity *= 0f;

        DontEmitLight = true;

        ShouldFullBright = Main.rand.NextBool();
    }

    public override void Update(ref ParticleRendererSettings settings) {
        TimeLeft = 2;

        foreach (Player player in Main.ActivePlayers) {
            if (player.dead) {
                continue;
            }

            FadeIn *= Ease.CubeOut(MathUtils.Clamp01(1f - MathUtils.ClampedDistanceProgress(Position, player.Center, TileHelper.TileSize * 5f, TileHelper.TileSize * 20f)));
        }

        FadeIn -= TimeSystem.LogicDeltaTime;
        if (FadeIn <= 0) {
            RestInPool();
            return;
        }

        bool flag = false;

        if (FadeIn <= 0.3f)
            flag = true;

        if (!flag) {
            if (Alpha > 200) {
                Alpha--;
            }
        }
        else {
            Alpha++;
            if (Alpha >= 255) {
                FadeIn -= TimeSystem.LogicDeltaTime;
                if (FadeIn <= 0f) {
                    RestInPool();
                    return;
                }
            }
        }
    }
}
