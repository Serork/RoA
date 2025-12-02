using RoA.Core;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Common.Projectiles;

sealed class StopVanillaFromLoadingProjectileTexture : IInitializer {
    void ILoadable.Load(Mod mod) {
        On_Main.LoadProjectile += On_Main_LoadProjectile;
    }

    private void On_Main_LoadProjectile(On_Main.orig_LoadProjectile orig, Main self, int i) {
        ModProjectile? modProjectile = ProjectileLoader.GetProjectile(i);
        if (modProjectile != null && modProjectile is IProjectileVanillaTextureLoadBypass) {
            return;
        }

        orig(self, i);
    }
}
