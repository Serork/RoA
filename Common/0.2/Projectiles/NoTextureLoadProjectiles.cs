using Microsoft.Xna.Framework;

using RoA.Content.Projectiles.Friendly;
using RoA.Core;
using RoA.Core.Utility;

using System.Runtime.CompilerServices;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Common.Projectiles;

abstract class NatureProjectile_NoTextureLoad : NatureProjectile, IProjectileVanillaTextureLoadBypass {
    public sealed override string Texture => ResourceManager.EmptyTexture;

    public sealed override void AutoStaticDefaults() { }

    public sealed override bool PreDraw(ref Color lightColor) {
        Draw(ref lightColor);

        return false;
    }

    protected virtual void Draw(ref Color lightColor) { }
}
