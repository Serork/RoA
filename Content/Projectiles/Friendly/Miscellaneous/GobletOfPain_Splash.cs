using Microsoft.Xna.Framework;

using RoA.Core.Defaults;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using System.Collections.Generic;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Miscellaneous;

sealed class GobletOfPainSplash : ModProjectile {
    public override void SetStaticDefaults() {
        Projectile.SetFrameCount(3);
    }

    public override void SetDefaults() {
        Projectile.SetSizeValues(10);

        Projectile.friendly = true;
        Projectile.tileCollide = false;

        Projectile.Opacity = 0f;

        Projectile.ignoreWater = true;

        Projectile.hide = true;

        Projectile.manualDirectionChange = true;
    }

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
        overPlayers.Add(index);
    }

    public override bool? CanDamage() => false;
    public override bool? CanCutTiles() => false;

    public override void AI() {
        if (Projectile.localAI[0] == 0f) {
            Projectile.localAI[0] = 1f;

            Projectile.SetDirection(Main.rand.NextBool().ToDirectionInt());
        }

        if (Projectile.frame < 2) {
            Projectile.Opacity = Helper.Approach(Projectile.Opacity, 1f, 0.2f);
        }
        else {
            Projectile.Opacity = Helper.Approach(Projectile.Opacity, 0f, 0.2f);
            if (Projectile.Opacity <= 0f) {
                Projectile.Kill();
            }
        }

        Projectile.Center = Projectile.GetOwnerAsPlayer().GetPlayerCorePoint();

        if (Projectile.Opacity >= 1f && Projectile.frameCounter++ > 4) {
            Projectile.frameCounter = 0;
            Projectile.frame++;
        }
        if (Projectile.frame >= 2) {
            Projectile.frame = 2;
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        Projectile.QuickDrawAnimated(lightColor * Projectile.Opacity);

        return false;
    }
}
