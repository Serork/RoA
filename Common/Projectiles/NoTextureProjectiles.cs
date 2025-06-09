using Microsoft.Xna.Framework;

using RoA.Content.Projectiles.Friendly;
using RoA.Core;

namespace RoA.Common.Projectiles;

abstract class NatureProjectile_NoTextureLoad : NatureProjectile {
    public override string Texture => ResourceManager.EmptyTexture;

    public override bool PreDraw(ref Color lightColor) {
        Draw(ref lightColor);

        return false;
    }

    protected virtual void Draw(ref Color lightColor) { }
}
