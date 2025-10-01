using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Projectiles;
using RoA.Common.VisualEffects;
using RoA.Content.VisualEffects;
using RoA.Core;
using RoA.Core.Defaults;
using RoA.Core.Utility;

using Terraria;

namespace RoA.Content.Projectiles.Enemies;

sealed class WardenPurification : ModProjectile_NoTextureLoad {
    private static ushort TIMELEFT => 60;

    private Color _areaColor;

    public override void SetDefaults() {
        Projectile.SetSizeValues(80);

        Projectile.hostile = true;
        Projectile.timeLeft = TIMELEFT;
    }

    public float Circle2Progress => Utils.GetLerpValue(1.25f, 1.75f, Projectile.localAI[1], true);

    public override void AI() {
        float extraModifier = Projectile.ai[1];
        extraModifier = MathUtils.Clamp01(extraModifier);
        extraModifier *= 0.02f;
        Projectile.ai[2] += 0.02f + extraModifier;

        if (Projectile.localAI[0] == 0f) {
            Projectile.localAI[0] = 1f;
            switch (Projectile.ai[0]) {
                case 0f:
                    _areaColor = new Color(5, 220, 135);
                    break;
                case 1f:
                    _areaColor = Color.White;
                    break;
            }
        }
        Projectile.localAI[1] += 0.05f + extraModifier;

        if (Circle2Progress >= 1f) {
            Projectile.Kill();
        }
    }

    public override bool? CanDamage() => Circle2Progress >= 0.5f;

    public override void OnKill(int timeLeft) {
        int num67 = 20;
        for (int m = 0; m < num67; m++) {
            Color newColor2 = _areaColor;
            Vector2 position = Projectile.Center;
            position = position + Vector2.UnitX * 4f + Vector2.UnitY * 20f + Vector2.UnitX * Projectile.width / 2f * Main.rand.NextFloatDirection() + Vector2.UnitY * Projectile.height / 2f * Main.rand.NextFloatDirection();
            Vector2 velocity = -Vector2.UnitY * 5f * Main.rand.NextFloat(0.25f, 1f);
            WardenDust? leafParticle = VisualEffectSystem.New<WardenDust>(VisualEffectLayer.ABOVEPLAYERS)?.Setup(position, velocity,
                scale: Main.rand.NextFloat(0.2f, num67 * 0.6f) / 7f);
            if (leafParticle != null) {
                leafParticle.AI0 = Projectile.ai[2];
            }
        }
    }

    protected override void Draw(ref Color lightColor) {
        Texture2D circle = ResourceManager.Circle;
        Texture2D circle2 = ResourceManager.Circle2;
        Texture2D Texture = circle;
        SpriteBatch spritebatch = Main.spriteBatch;
        Vector2 Position = Projectile.Center;
        int extra = 3;
        Rectangle clip = circle.Bounds;
        Rectangle clip2 = circle2.Bounds;
        Vector2 origin = clip.Centered();
        Vector2 origin2 = clip2.Centered();
        float fadeOutProgress = 1f;
        Color color2 = _areaColor;
        float waveMin = MathHelper.Lerp(0.75f, 1f, 1f - fadeOutProgress), waveMax = MathHelper.Lerp(1.25f, 1f, 1f - fadeOutProgress);
        float wave = Helper.Wave(Projectile.ai[2], waveMin, waveMax, 3f, Projectile.whoAmI) * fadeOutProgress;
        float opacity = wave * fadeOutProgress;
        color2 *= opacity;
        float disappearValue = 1f - Utils.GetLerpValue(0.75f, 1f, Circle2Progress, true);
        disappearValue = Ease.CircOut(disappearValue);
        color2 *= disappearValue;
        Color color3 = color2;
        color3.A = 100;
        for (int i = 0; i < extra; i++) {
            Vector2 scale = Ease.SineInOut(MathUtils.Clamp01(Projectile.localAI[1])) * Vector2.One *
                Utils.Remap(fadeOutProgress, 0f, 1f, 0.75f, 1f, true) * 
                (i != 0 ? (Utils.Remap(i, 0, extra, 0.75f, 1f) * 
                Utils.Remap(wave, waveMin, waveMax, waveMin * 1.5f, waveMax, true)) : 1f) *
                disappearValue;
            spritebatch.DrawWithSnapshot(() => {
                spritebatch.Draw(Texture, Position - Main.screenPosition, null, color2 * 0.625f * fadeOutProgress, Projectile.rotation, origin, Projectile.scale * scale, SpriteEffects.None, 0f);
            }, blendState: BlendState.Additive);

            spritebatch.DrawWithSnapshot(() => {
                spritebatch.Draw(circle2, Position - Main.screenPosition, null, color3 * 0.625f * fadeOutProgress * Circle2Progress, Projectile.rotation, origin2, Projectile.scale * scale * Circle2Progress * 0.7f, SpriteEffects.None, 0f);
            }, blendState: BlendState.Additive);
        }
    }
}
