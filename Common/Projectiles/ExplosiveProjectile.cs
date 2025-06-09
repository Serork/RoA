using RoA.Core.Utility;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Common.Projectiles;

interface IExplosiveProjectile {

}

sealed class ExplosiveProjectile : GlobalProjectile {
    public override bool AppliesToEntity(Projectile entity, bool lateInstantiation) => entity.IsModded(out ModProjectile modProjectile) && modProjectile is IExplosiveProjectile;
}
