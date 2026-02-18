using Microsoft.Xna.Framework;

using RoA.Common;
using RoA.Content.Dusts;
using RoA.Core.Defaults;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using System;
using System.Collections.Generic;

using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class CottonFiber : NatureProjectile {
    private static ushort TIMELEFT => MathUtils.SecondsToFrames(5);

    protected override void SafeSetDefaults() {
        SetNatureValues(Projectile, shouldChargeWreath: true, shouldApplyAttachedItemDamage: false);

        Projectile.SetSizeValues(24);
        Projectile.friendly = true;
        Projectile.penetrate = 1;
        Projectile.tileCollide = false;

        Projectile.timeLeft = TIMELEFT;

        Projectile.aiStyle = -1;

        Projectile.manualDirectionChange = true;

        Projectile.hide = true;
    }

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
        behindProjectiles.Add(index);
    }

    public override void AI() {
        void pushOthers() {
            foreach (Projectile projectile in TrackedEntitiesSystem.GetTrackedProjectile<CottonBollSmall>()) {
                if (Projectile.Distance(projectile.Center) > Projectile.width * 1.25f) {
                    continue;
                }
                projectile.velocity += projectile.DirectionFrom(Projectile.Center) * TimeSystem.LogicDeltaTime * 2f;
            }
        }

        pushOthers();
        pushOthers();

        if (Main.rand.NextBool(15)) {
            for (int i = 0; i < 1; i++) {
                Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2CircularEdge(Projectile.width, Projectile.height) * 0.5f, ModContent.DustType<CottonDust>(), Main.rand.NextVector2CircularEdge(3f, 3f) * (Main.rand.NextFloat() * 0.5f + 0.5f), 0);
                dust.scale *= 1.2f;
                dust.noGravity = true;
                dust.alpha = Projectile.alpha;
            }
        }

        Projectile.velocity *= 0.97f;

        if (Projectile.velocity.Length() > 0.1f) {
            Projectile.SetDirection(Projectile.velocity.X.GetDirection());
        }

        Projectile.rotation += 0.5f * Projectile.spriteDirection * MathUtils.Clamp01(Projectile.SpeedX());

        if (Projectile.SpeedX() < 1f) {
            Projectile.Opacity = Helper.Approach(Projectile.Opacity, 0f, 0.025f);
            if (Projectile.Opacity <= 0f) {
                Projectile.Kill();
            }
        }
    }

    public override void OnKill(int timeLeft) {

    }

    public override bool PreDraw(ref Color lightColor) {
        Projectile.QuickDraw(lightColor * Projectile.Opacity);

        return false;
    }
}
