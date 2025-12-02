using Microsoft.Xna.Framework;

using RoA.Content.Projectiles.LiquidsSpecific;

using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace RoA.Content.Tiles.Danger;

sealed class SolidifiedTarStalactite : StalactiteBase<SolidifiedTarStalactiteTE, SolidifiedTarStalactiteProjectile> {
    protected override void SafeSetStaticDefaults() {
        AddMapEntry(new Color(57, 45, 65));
    }
}

sealed class SolidifiedTarStalactiteTE : StalactiteTE<SolidifiedTarStalactiteProjectile> { }

sealed class SolidifiedTarStalactiteProjectile : StalactiteProjectileBase {
    protected override ushort KillDustType() => (ushort)ModContent.DustType<Dusts.SolidifiedTar>();

    protected override void SafeModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
        modifiers.FinalDamage *= 0.5f;
    }

    protected override void SafeModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) {
        modifiers.FinalDamage *= 0.5f;
    }

    public override void PostAI() {
        if (Collision.LavaCollision(Projectile.position, Projectile.width, Projectile.height)) {
            Projectile.Kill();

            int x = (int)Projectile.Bottom.X / 16, y = (int)Projectile.Bottom.Y / 16;
            Tile tile = Main.tile[x - 1, y];
            Tile tile2 = Main.tile[x + 1, y];
            Tile tile3 = Main.tile[x, y - 1];
            Tile tile4 = Main.tile[x, y + 1];
            Tile tile5 = Main.tile[x, y];
            tile.LiquidAmount = tile2.LiquidAmount = tile3.LiquidAmount = 0;
            Projectile.NewProjectile(null, new Point16(x, y).ToWorldCoordinates(), Vector2.Zero, ModContent.ProjectileType<TarExplosion>(), 100, 0f, Main.myPlayer);
            WorldGen.SquareTileFrame(x, y);
        }
    }
}
