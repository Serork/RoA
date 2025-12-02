using Microsoft.Xna.Framework;

using RoA.Content.Projectiles.Friendly;
using RoA.Core;

using Terraria.ModLoader;

namespace RoA.Common.Projectiles;

abstract class ModProjectile_NoTextureLoad : ModProjectile, IProjectileVanillaTextureLoadBypass {
    public sealed override string Texture => ResourceManager.EmptyTexture;

    public sealed override void AutoStaticDefaults() { }

    public sealed override bool PreDraw(ref Color lightColor) {
        Draw(ref lightColor);

        return false;
    }

    protected virtual void Draw(ref Color lightColor) { }
}

abstract class NatureProjectile_NoTextureLoad : NatureProjectile, IProjectileVanillaTextureLoadBypass {
    public sealed override string Texture => ResourceManager.EmptyTexture;

    public sealed override void AutoStaticDefaults() { }

    public sealed override bool PreDraw(ref Color lightColor) {
        Draw(ref lightColor);

        return false;
    }

    protected virtual void Draw(ref Color lightColor) { }
}

abstract class FormProjectile_NoTextureLoad : NatureProjectile_NoTextureLoad {
    protected override void SafeSetDefaults3() {
        ShouldChargeWreathOnDamage = false;
    }
}
