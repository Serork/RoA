using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common.VisualEffects;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;

using Terraria;
using Terraria.Graphics.Renderers;
using Terraria.ModLoader;

namespace RoA.Content.VisualEffects;

class WardenDust : VisualEffect<WardenDust> {
    private static Asset<Texture2D>? _altTexture;

    public bool Alt;

    protected override void SetDefaults() {
        TimeLeft = 100;

        SetFramedTexture(3, Main.rand.Next(1));
        AI0 = 0f;

        Alt = false;

        DontEmitLight = true;

        Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
    }

    public override void OnLoad(Mod mod) {
        if (Main.dedServ) {
            return;
        }

        _altTexture = ModContent.Request<Texture2D>(TexturePath + "2");
    }

    protected override float ScaleDecreaseModifier() => 0.75f;

    public override void Update(ref ParticleRendererSettings settings) {
        if (Velocity.IsWithinRange(1f)) {
            base.Update(ref settings);
        }
        else {
            Velocity *= 0.9f;
            Position += Velocity;
        }
        AI0 += 0.02f;
    }

    public override void Draw(ref ParticleRendererSettings settings, SpriteBatch spritebatch) {
        int extra = 3;
        Color color2 = Alt ? Color.CadetBlue : Color.LightGreen;
        float fadeOutProgress = CustomData == null ? 1f : (float)CustomData!;
        float waveMin = MathHelper.Lerp(0.75f, 1f, 1f - fadeOutProgress), waveMax = MathHelper.Lerp(1.25f, 1f, 1f - fadeOutProgress);
        float wave = Helper.Wave(AI0, waveMin, waveMax, 3f, 0f) * fadeOutProgress;
        float opacity = wave * fadeOutProgress;
        color2 *= opacity;
        Texture2D texture = Alt ? _altTexture!.Value : Texture;
        spritebatch.Draw(texture, Position - Main.screenPosition, Frame, Lighting.GetColor(Position.ToTileCoordinates()) * Utils.Remap(fadeOutProgress, 0f, 1f, 0.5f, 1f, true), Rotation, Origin, Scale, SpriteEffects.None, 0f);
        for (int i = 0; i < extra; i++) {
            spritebatch.DrawWithSnapshot(() => {
                spritebatch.Draw(texture, Position - Main.screenPosition, Frame, color2 * 0.625f * fadeOutProgress, Rotation, Origin, Scale, SpriteEffects.None, 0f);
            }, blendState: BlendState.Additive);
        }
    }
}
