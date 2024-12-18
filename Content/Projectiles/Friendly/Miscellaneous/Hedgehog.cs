using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Miscellaneous;

sealed class Hedgehog : ModProjectile {
    public override void SetDefaults() {
        Projectile.CloneDefaults(ProjectileID.SpikyBall);
        AIType = ProjectileID.SpikyBall;
        Projectile.friendly = true;
        Projectile.penetrate = -1;
        Projectile.tileCollide = true;
        Projectile.timeLeft = 300;

        Projectile.DamageType = DamageClass.Default;
        int width = 20; int height = width;
        Projectile.Size = new Vector2(width, height);
    }

    public override void OnKill(int timeLeft) {
        if (Main.netMode == NetmodeID.MultiplayerClient) {
            return;
        }

        NPC.NewNPC(Projectile.GetSource_Death(), (int)Projectile.Center.X, (int)Projectile.Center.Y + 2, ModContent.NPCType<NPCs.Friendly.Hedgehog>());
    }
}