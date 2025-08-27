using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.LiquidsSpecific;

sealed class TarBomb : ModProjectile {
    public override void SetDefaults() {
        Projectile.CloneDefaults(904);
        Projectile.aiStyle = 16;
        AIType = 906;
        Projectile.timeLeft = 180;
    }

    public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
        fallThrough = false;
        return true;
    }

    //This is to make the bomb emit our liquid's splash at its fuse like other liquid bombs
    public override bool PreAI() {
        if (Projectile.owner != Main.myPlayer || Projectile.timeLeft > 3) {
            if (Main.rand.NextBool(2)) {
                int type = ModContent.DustType<Dusts.Tar>();
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, type, 0f, 0f, 100);
                dust.scale = 1f + (float)Main.rand.Next(5) * 0.1f;
                dust.noGravity = true;
                Vector2 center = Projectile.Center;
                Vector2 spinPoint = new Vector2(0f, (float)(-Projectile.height / 2 - 6));
                double rot = Projectile.rotation;
                dust.position = center + Utils.RotatedBy(spinPoint, rot, default(Vector2)) * 1.1f;
            }
        }
        return true;
    }

    public override void OnKill(int timeLeft) {
        TarRocket.TarLiquidExplosiveKill(Projectile);
    }
}
