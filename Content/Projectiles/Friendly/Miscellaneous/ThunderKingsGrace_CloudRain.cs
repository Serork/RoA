using Microsoft.Xna.Framework;

using System.Collections.Generic;

using Terraria;
using Terraria.ID;

namespace RoA.Content.Projectiles.Friendly.Miscellaneous;

sealed class CloudPlatformAngryRain : NatureProjectile {
    protected override void SafeSetDefaults() {
        Projectile.ignoreWater = true;
        Projectile.width = 4;
        Projectile.height = 20;
        Projectile.aiStyle = -1;
        Projectile.friendly = true;
        Projectile.penetrate = 5;
        Projectile.timeLeft = 300;
        Projectile.scale = 1.1f;
        //Projectile.magic = true;
        Projectile.extraUpdates = 1;
        Projectile.usesIDStaticNPCImmunity = true;
        Projectile.idStaticNPCHitCooldown = 10;

        ShouldApplyAttachedNatureWeaponCurrentDamage = false;

        Projectile.hide = true;
    }

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
        behindProjectiles.Add(index);
    }

    public override void AI() {
        int num359 = (int)(Projectile.Center.X / 16f);
        int num360 = (int)((Projectile.position.Y + (float)Projectile.height) / 16f);
        if (WorldGen.InWorld(num359, num360) && Main.tile[num359, num360] != null && Main.tile[num359, num360].LiquidAmount == byte.MaxValue && Main.tile[num359, num360].LiquidType == LiquidID.Shimmer && Projectile.velocity.Y > 0f) {
            Projectile.velocity.Y *= -1f;
            Projectile.netUpdate = true;
        }

        if (Projectile.type == 239)
            Projectile.alpha = 50;
    }

    public override void OnKill(int timeLeft) {
        if (Projectile.velocity.Y > 0f) {
            int num502 = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y + (float)Projectile.height - 2f), 2, 2, 154);
            Main.dust[num502].position.X -= 2f;
            Main.dust[num502].alpha = 38;
            Dust dust2 = Main.dust[num502];
            dust2.velocity *= 0.1f;
            dust2 = Main.dust[num502];
            dust2.velocity += -Projectile.oldVelocity * 0.25f;
            Main.dust[num502].scale = 0.95f;
        }
    }
}
