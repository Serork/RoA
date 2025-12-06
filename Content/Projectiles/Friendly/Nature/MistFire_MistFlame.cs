using Microsoft.Xna.Framework;

using RoA.Core.Defaults;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using Terraria;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class MistFlame : NatureProjectile {
    private static byte FRAMECOUNT => 4;

    public override void SetStaticDefaults() {
        Projectile.SetFrameCount(FRAMECOUNT);
    }

    protected override void SafeSetDefaults() {
        Projectile.SetSizeValues(10);

        Projectile.friendly = true;
        Projectile.aiStyle = -1;

        Projectile.tileCollide = false;

        Projectile.manualDirectionChange = true;

        Projectile.timeLeft = 40;
    }

    public override void AI() {
        Projectile.Animate(4);

        Projectile.scale = 1.25f;
    }

    public override void OnKill(int timeLeft) {

    }

    public override bool PreDraw(ref Color lightColor) {
        Projectile.QuickDrawAnimated(Color.White);

        return false;
    }
}
