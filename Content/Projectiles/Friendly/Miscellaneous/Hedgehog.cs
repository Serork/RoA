using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Core.Utility;

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

    public override bool? CanCutTiles() => false;

    public override bool? CanDamage() => false;

    public override bool PreDraw(ref Color lightColor) {
        Texture2D texture = Projectile.GetTexture();
        Vector2 origin = Projectile.Size / 2f;
        Main.spriteBatch.Draw(texture, Projectile.position - Main.screenPosition + origin, new Rectangle(0, Projectile.height * Projectile.frame, Projectile.width, Projectile.height), lightColor, Projectile.rotation, origin, Projectile.scale, Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);

        return false;
    }

    public override void OnKill(int timeLeft) {
        if (Main.netMode == NetmodeID.MultiplayerClient) {
            return;
        }

        NPC.NewNPC(Projectile.GetSource_Death(), (int)Projectile.Center.X, (int)Projectile.Center.Y + 10, ModContent.NPCType<NPCs.Friendly.Hedgehog>(), ai3: Projectile.rotation);
    }
}