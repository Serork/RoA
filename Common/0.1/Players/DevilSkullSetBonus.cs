using Microsoft.Xna.Framework;

using RoA.Content.Projectiles.Friendly.Miscellaneous;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Common.Players;

sealed partial class PlayerCommon : ModPlayer {
    public bool ApplyDevilSkullSetBonus;

    public partial void DevilSkullLoad() {
    }

    public override void PostUpdateMiscEffects() {
        Player self = Player;
        if (!self.IsLocal()) {
            return;
        }

        if (!self.GetCommon().ApplyDevilSkullSetBonus) {
            return;
        }

        if (self.HasProjectile<SerpentChain>() || !self.HasMinionAttackTargetNPC) {
            return;
        }

        Vector2 center = self.Center;
        ProjectileUtils.SpawnPlayerOwnedProjectile<SerpentChain>(new ProjectileUtils.SpawnProjectileArgs(self, self.GetSource_Misc("serpentchain")) {
            Position = center,
            Velocity = center.DirectionTo(self.GetWorldMousePosition()) * 5f
        });
    }
}
