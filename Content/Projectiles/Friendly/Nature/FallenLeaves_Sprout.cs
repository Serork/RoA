using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Content.Dusts;
using RoA.Core;
using RoA.Core.Defaults;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using System.Collections.Generic;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class FallenLeavesSprout : ModProjectile {
    public override string Texture => ResourceManager.NatureProjectileTextures + "FallenLeaves_Sprout";

    public override void SetStaticDefaults() {
        Projectile.SetFrameCount(2);
    }

    public override void SetDefaults() {
        Projectile.SetSizeValues(10);

        Projectile.friendly = true;
        Projectile.tileCollide = false;

        Projectile.hide = true;

        Projectile.Opacity = 0f;

        Projectile.manualDirectionChange = true;
    }

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
        overPlayers.Add(index);
    }

    public override bool? CanDamage() => false;
    public override bool? CanCutTiles() => false;

    public override bool ShouldUpdatePosition() => false;

    public override void AI() {
        Projectile.Opacity = Helper.Approach(Projectile.Opacity, 1f, 0.125f);

        if (Projectile.localAI[0] == 0f) {
            Projectile.localAI[0] = 1f;

            Projectile.frame = Main.rand.Next(2);

            Projectile.SetDirection(Main.rand.NextBool().ToDirectionInt());
        }

        Player player = Projectile.GetOwnerAsPlayer();
        if (!player.GetCommon().IsFallenLeavesEffectActive) {
            Projectile.Kill();
        }
        if (!player.IsAlive()) {
            Projectile.Kill();
        }
        if (player.HasProjectile<FallenLeavesBranch>()) {
            Projectile.Kill();
        }

        Projectile.timeLeft = 2;

        Vector2 velocity = Projectile.velocity.SafeNormalize();
        velocity.X *= player.direction * player.gravDir;
        Projectile.rotation = velocity.ToRotation() + MathHelper.PiOver2;
        Projectile.Center = player.MovementOffset() + player.RotatedRelativePoint(player.GetPlayerCorePoint(), false, false) + velocity.RotatedBy(player.fullRotation) * 25f * player.gravDir;
    }

    public override void OnKill(int timeLeft) {
        float offset = 20f;
        int count = 4;
        Vector2 startPosition = Projectile.Center,
                endPosition = Projectile.Center + Vector2.UnitY.RotatedBy(Projectile.rotation) * offset;
        startPosition -= startPosition.DirectionTo(endPosition) * offset / 2f;
        for (int i = 0; i < count; i++) {
            for (int k = 0; k < 4; k++) {
                int size = 6;
                float dustScale = 0.915f + 0.15f * Main.rand.NextFloat();
                Dust dust = Main.dust[Dust.NewDust(startPosition - Vector2.One * size * 0.5f, size, size, ModContent.DustType<FallenLeavesBranchDust>(), 0f, 0f, Main.rand.Next(100), default, dustScale)];
                dust.noGravity = true;
                dust.fadeIn = 0.5f;
                dust.noLight = true;
                dust.velocity *= 0.5f;
                dust.velocity *= Main.rand.NextFloat(0.5f, 1f);
            }

            startPosition += startPosition.DirectionTo(endPosition) * offset / count;
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        Player player = Projectile.GetOwnerAsPlayer();
        SpriteEffects spriteEffects = (Projectile.spriteDirection * player.direction).ToSpriteEffects();
        if (player.gravDir < 0f) {
            spriteEffects |= SpriteEffects.FlipVertically;
        }
        Projectile.QuickDrawAnimated(lightColor, scale: new Vector2(Ease.CubeOut(Projectile.Opacity), MathUtils.Clamp01(Projectile.Opacity * 3f)), spriteEffects: spriteEffects);

        return false;
    }
}
