using RoA.Content.Items.Weapons.Druidic.Rods;
using RoA.Content.Projectiles.Friendly.Melee;

using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace RoA.Common.Players;

sealed class DruidPlayerShouldersFix : ILoadable {
    void ILoadable.Load(Mod mod) {
        On_PlayerDrawSet.CreateCompositeData += On_PlayerDrawSet_CreateCompositeData;
    }

    private void On_PlayerDrawSet_CreateCompositeData(On_PlayerDrawSet.orig_CreateCompositeData orig, ref PlayerDrawSet self) {
        orig(ref self);
        if (self.drawPlayer.velocity.Y != 0f) {
            foreach (Projectile projectile in Main.ActiveProjectiles) {
                if (projectile.owner != self.drawPlayer.whoAmI) {
                    continue;
                }
                if (projectile.ModProjectile is BaseRodProjectile) {
                    self.drawPlayer.bodyFrame.Y = 0;
                    self.compShoulderOverFrontArm = true;
                    self.hideCompositeShoulders = false;
                    break;
                }
            }
        }
    }

    void ILoadable.Unload() { }
}
