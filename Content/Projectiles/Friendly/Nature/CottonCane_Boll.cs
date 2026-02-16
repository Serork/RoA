using Microsoft.Xna.Framework;

using RoA.Core.Defaults;
using RoA.Core.Utility;
using RoA.Core.Utility.Vanilla;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class CottonBoll : NatureProjectile {
    private static ushort TIMELEFT => MathUtils.SecondsToFrames(15);

    protected override void SafeSetDefaults() {
        Projectile.SetSizeValues(10);
        Projectile.aiStyle = -1;
        Projectile.friendly = true;
        Projectile.timeLeft = TIMELEFT;
        Projectile.penetrate = -1;
        Projectile.hide = true;

        Projectile.tileCollide = false;

        Projectile.netImportant = true;

        Projectile.manualDirectionChange = true;
    }

    public override bool? CanDamage() => false;
    public override bool? CanCutTiles() => false;

    public override bool PreDraw(ref Color lightColor) {
        Projectile.QuickDraw(lightColor);

        return false;
    }
}
