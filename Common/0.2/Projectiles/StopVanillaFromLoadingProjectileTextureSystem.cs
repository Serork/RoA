using Microsoft.Xna.Framework;

using RoA.Common.Projectiles;
using RoA.Core;
using RoA.Core.Utility;

using System.Runtime.CompilerServices;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Common.Projectiles;

sealed class StopVanillaFromLoadingProjectileTextureSystem : IInitializer {
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
