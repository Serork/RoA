using RoA.Content.Items.Weapons.Nature;

using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace RoA.Common.Players;

sealed class DruidPlayerShouldersFix : ILoadable {
    public interface IProjectileFixShoulderWhileActive { }

    void ILoadable.Load(Mod mod) {
        On_PlayerDrawSet.CreateCompositeData += On_PlayerDrawSet_CreateCompositeData;
    }

    private void On_PlayerDrawSet_CreateCompositeData(On_PlayerDrawSet.orig_CreateCompositeData orig, ref PlayerDrawSet self) {
        orig(ref self);
        foreach (Projectile projectile in Main.ActiveProjectiles) {
            if (projectile.owner != self.drawPlayer.whoAmI) {
                continue;
            }
            if (projectile.ModProjectile is CaneBaseProjectile || projectile.ModProjectile is IProjectileFixShoulderWhileActive) {
                self.drawPlayer.bodyFrame.Y = 0;
                self.compShoulderOverFrontArm = true;
                self.hideCompositeShoulders = false;
                break;
            }
        }
    }

    void ILoadable.Unload() { }
}
