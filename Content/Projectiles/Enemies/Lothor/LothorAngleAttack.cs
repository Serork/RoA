using Microsoft.Xna.Framework;

using RoA.Core;

using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Enemies.Lothor;

sealed class LothorAngleAttack : ModProjectile {
    public override string Texture => ResourceManager.EmptyTexture;

    public override bool PreDraw(ref Color lightColor) => false;
}