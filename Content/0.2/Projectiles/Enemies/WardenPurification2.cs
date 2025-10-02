
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Projectiles;
using RoA.Content.NPCs.Enemies.Backwoods.Hardmode;
using RoA.Core;
using RoA.Core.Defaults;
using RoA.Core.Utility;

using Terraria;

using static RoA.Content.NPCs.Enemies.Backwoods.Hardmode.WardenOfTheWoods;

namespace RoA.Content.Projectiles.Enemies;

sealed class WardenPurification2 : ModProjectile_NoTextureLoad {
    private static ushort TIMELEFT => (ushort)WardenOfTheWoods.ATTACKTIME;

    private Color _areaColor;
    private float _currentAttackTime;

    public ref float FinalScale => ref Projectile.localAI[2];
    public ref float AttackTime => ref Projectile.localAI[1];
    public ref float OwnerWhoAmI => ref Projectile.ai[1];
    public ref float InitValue => ref Projectile.localAI[0];
    public ref float VisualEffectTimer => ref Projectile.ai[2];
    public ref float AreaColorFactor => ref Projectile.ai[0];

    public override void SetDefaults() {
        Projectile.SetSizeValues(80);

        Projectile.hostile = true;
        Projectile.timeLeft = TIMELEFT;

        Projectile.tileCollide = false;
    }

    public float Circle2Progress => 1f - Utils.GetLerpValue(0f, AttackTime, _currentAttackTime, true);

    public override void AI() {
        NPC owner = Main.npc[(int)OwnerWhoAmI];
        if (!owner.active) {
            Projectile.Kill();
            return;
        }

        if (AttackTime == 0f) {
            _currentAttackTime = AttackTime = new WardenOfTheWoodsValues(owner).StateTimer;
        }

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

        if (_currentAttackTime > 0f) {
            _currentAttackTime -= 1f;
        }

        FinalScale = 2f;
    }

    public override void ModifyDamageHitbox(ref Rectangle hitbox) {
        int size = (int)(45 * FinalScale * Circle2Progress);
        hitbox.Inflate(size, size);
    }

    public override bool? CanDamage() => Circle2Progress >= 0.5f;

    public override void OnKill(int timeLeft) { }

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
        float wave = Helper.Wave(VisualEffectTimer, waveMin, waveMax, 3f, Projectile.whoAmI) * fadeOutProgress;
        float opacity = wave * fadeOutProgress;
        color2 *= opacity;
        Color color3 = color2;
        color3.A = 200;
        for (int i = 0; i < extra; i++) {
            Vector2 scale = Ease.SineInOut(MathUtils.Clamp01(AttackTime)) * Vector2.One *
                Utils.Remap(fadeOutProgress, 0f, 1f, 0.75f, 1f, true) *
                (i != 0 ? (Utils.Remap(i, 0, extra, 0.75f, 1f) *
                Utils.Remap(wave, waveMin, waveMax, waveMin * 1.5f, waveMax, true)) : 1f) *
                (FinalScale + 1f);

            spritebatch.DrawWithSnapshot(() => {
                spritebatch.Draw(circle2, Position - Main.screenPosition, null, color3 * 0.625f * fadeOutProgress * Circle2Progress, Projectile.rotation, origin2, Projectile.scale * scale * Circle2Progress * 0.7f, SpriteEffects.None, 0f);
            }, blendState: BlendState.Additive);
        }
    }
}
