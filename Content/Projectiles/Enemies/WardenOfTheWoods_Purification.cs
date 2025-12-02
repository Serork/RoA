using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Projectiles;
using RoA.Common.VisualEffects;
using RoA.Content.NPCs.Enemies.Backwoods.Hardmode;
using RoA.Content.VisualEffects;
using RoA.Core;
using RoA.Core.Defaults;
using RoA.Core.Utility;

using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace RoA.Content.Projectiles.Enemies;

sealed class Purification : ModProjectile_NoTextureLoad {
    private static ushort TIMELEFT => 60;

    private Color _areaColor;

    public ref float VisualEffectTimer => ref Projectile.ai[2];
    public ref float InitValue => ref Projectile.localAI[0];
    public ref float CurrentAttackTime => ref Projectile.localAI[1];
    public ref float FinalScaleFactor => ref Projectile.localAI[2];
    public ref float AreaColorFactor => ref Projectile.ai[0];

    public override void SetDefaults() {
        Projectile.SetSizeValues(80);

        Projectile.hostile = true;
        Projectile.timeLeft = TIMELEFT;

        Projectile.tileCollide = false;
    }

    public float CircleProgress => Utils.GetLerpValue(1.25f, 1.75f, CurrentAttackTime, true);

    public override void AI() {
        VisualEffectTimer += 0.05f;

        if (InitValue == 0f) {
            InitValue = 1f;
            switch (AreaColorFactor) {
                case 0f:
                    _areaColor = WardenOfTheWoods.Color;
                    break;
                case 1f:
                    _areaColor = WardenOfTheWoods.AltColor;
                    break;
            }
        }
        CurrentAttackTime += 0.05f;

        FinalScaleFactor = 1.35f;

        int type = DustID.TintableDustLighted;
        for (int i = 0; i < 2; i++) {
            if (Main.rand.NextBool(8)) {
                Vector2 spinningpoint = Vector2.UnitX.RotatedBy((double)Main.rand.NextFloat() * MathHelper.TwoPi);
                Vector2 center = Projectile.Center + new Vector2(Projectile.direction == 1 ? 3f : -3f, 0f) + Projectile.velocity + spinningpoint * (Projectile.width * 0.8f * Projectile.scale);
                Vector2 rotationPoint = spinningpoint.RotatedBy(0.785) * Projectile.direction;
                Vector2 position = center + rotationPoint * 5f;
                int dust = Dust.NewDust(position, 0, 0, type, newColor: _areaColor, Alpha: 200);
                Main.dust[dust].position = position;
                Main.dust[dust].noGravity = true;
                Main.dust[dust].fadeIn = Main.rand.NextFloat() * 1.2f * Projectile.scale;
                Main.dust[dust].velocity = rotationPoint * Projectile.scale * -2f;
                Main.dust[dust].scale = Main.rand.NextFloat() * Main.rand.NextFloat(1f, 1.25f);
                Main.dust[dust].scale *= Projectile.scale * 1.5f;
                Main.dust[dust].velocity += Projectile.velocity * 1.25f;
                Main.dust[dust].position += Main.dust[dust].velocity * -5f;
                Main.dust[dust].noLight = true;
            }
        }

        if (CircleProgress >= 1f) {
            SoundEngine.PlaySound(SoundID.Item66 with { Pitch = 0.5f, Volume = 0.75f }, Projectile.Center);
            Projectile.Kill();
        }

        if (CurrentAttackTime == 0.25f) SoundEngine.PlaySound(new SoundStyle(ResourceManager.ItemSounds + "PurifySmall") with { PitchVariance = 0.1f, Volume = 0.75f }, Projectile.Center);

    }

    public override void ModifyDamageHitbox(ref Rectangle hitbox) {
        int size = (int)(40 * Projectile.ai[1]);
        hitbox.Inflate(size, size);
    }

    public override bool? CanDamage() => CircleProgress >= 0.75f;

    public override void OnKill(int timeLeft) {
        int num67 = 20;
        for (int m = 0; m < num67; m++) {
            Color newColor2 = _areaColor;
            Vector2 position = Projectile.Center;
            position = position + Vector2.UnitX * 4f + Vector2.UnitY * 20f + Vector2.UnitX * Projectile.width / 2f * Main.rand.NextFloatDirection() + Vector2.UnitY * Projectile.height / 2f * Main.rand.NextFloatDirection();
            Vector2 velocity = -Vector2.UnitY * 5f * Main.rand.NextFloat(0.25f, 1f);
            WardenDust? wardenParticle = VisualEffectSystem.New<WardenDust>(VisualEffectLayer.ABOVEPLAYERS)?.Setup(position, velocity,
                scale: Main.rand.NextFloat(0.2f, num67 * 0.6f) / 7f);
            if (wardenParticle != null) {
                wardenParticle.AI0 = VisualEffectTimer;
                wardenParticle.Alt = AreaColorFactor != 0f;
            }
        }
    }

    protected override void Draw(ref Color lightColor) {
        //Texture2D circle = ResourceManager.Circle;
        Texture2D circle = ResourceManager.Circle3;
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
        float wave = Helper.Wave(VisualEffectTimer, waveMin, waveMax, 3f, Projectile.whoAmI) * fadeOutProgress;
        float opacity = wave * fadeOutProgress;
        color2 *= opacity;
        float disappearValue = 1f - Utils.GetLerpValue(0.5f, 1f, CircleProgress, true);
        disappearValue = Ease.CircOut(disappearValue);
        color2 *= disappearValue;
        Color color3 = color2;
        color3.A = 200;
        for (int i = 0; i < extra; i++) {
            Vector2 scale = Ease.SineInOut(MathUtils.Clamp01(CurrentAttackTime)) * Vector2.One *
                Utils.Remap(fadeOutProgress, 0f, 1f, 0.75f, 1f, true) * 
                (i != 0 ? (Utils.Remap(i, 0, extra, 0.75f, 1f) * 
                Utils.Remap(wave, waveMin, waveMax, waveMin * 1.5f, waveMax, true)) : 1f) *
                disappearValue * (Projectile.ai[1] + 1f);
            spritebatch.DrawWithSnapshot(() => {
                spritebatch.Draw(Texture, Position - Main.screenPosition, null, color2 * 0.625f * fadeOutProgress, Projectile.rotation, origin, Projectile.scale * scale/* * 0.45f*/, SpriteEffects.None, 0f);
            }, blendState: BlendState.Additive);

            spritebatch.DrawWithSnapshot(() => {
                spritebatch.Draw(circle2, Position - Main.screenPosition, null, color3 * 0.625f * fadeOutProgress * CircleProgress, Projectile.rotation, origin2, Projectile.scale * scale * CircleProgress * FinalScaleFactor * 0.9f, SpriteEffects.None, 0f);
            }, blendState: BlendState.Additive);
        }
    }
}
