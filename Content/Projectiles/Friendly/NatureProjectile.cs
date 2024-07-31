using RoA.Core;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly;

abstract class NatureProjectile : ModProjectile {
    public bool ShouldApplyWreathPoints { get; protected set; } = true;

    public sealed override void SetDefaults() {
        SafeSetDefaults();

        if (Projectile.friendly) {
            Projectile.SetDefaultToDruidicProjectile();
        }

        SafeSetDefaults2();
    }

    protected virtual void SafeSetDefaults() { }
    protected virtual void SafeSetDefaults2() { }
}
