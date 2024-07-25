using RoA.Core;
using RoA.Core.Utility;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly;

abstract class NatureProjectile : ModProjectile {
    public sealed override void SetDefaults() {
        SafeSetDefaults();

        if (Projectile.IsDamageable()) {
            Projectile.SetDefaultToDruidicProjectile();
        }

        SafeSetDefaults2();
    }

    protected virtual void SafeSetDefaults() { }
    protected virtual void SafeSetDefaults2() { }
}
