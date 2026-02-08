using Microsoft.Xna.Framework;

using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using System;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Pets;

sealed class Aries : ModProjectile {
    public override void SetStaticDefaults() {
        Main.projPet[Type] = true;

        Projectile.SetFrameCount(4);
    }

    public override void SetDefaults() {
        Projectile.netImportant = true;
        Projectile.width = 46;
        Projectile.height = 44;
        Projectile.aiStyle = ProjAIStyleID.Pet;
        Projectile.friendly = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft *= 5;

        Projectile.tileCollide = false;
    }

    public override bool PreAI() {
        int owner = Projectile.owner;
        if (Main.player[owner].dead)
            Main.player[owner].GetCommon().IsAriesActive = false;

        if (Main.player[owner].GetCommon().IsAriesActive)
            Projectile.timeLeft = 2;

        return base.PreAI();
    }

    public override bool PreDraw(ref Color lightColor) {
        Projectile.QuickDrawAnimated(lightColor * Projectile.Opacity);

        return false;
    }

    public override void AI() {
        if (Projectile.velocity.Y == 0f) {
            if ((double)Projectile.velocity.X < -0.1 || (double)Projectile.velocity.X > 0.1) {
                Projectile.frameCounter += (int)Math.Abs(Projectile.velocity.X);
                Projectile.frameCounter++;
                if (Projectile.frameCounter > 6) {
                    Projectile.frame++;
                    Projectile.frameCounter = 0;
                }

                if (Projectile.frame >= 2)
                    Projectile.frame = 0;
            }
            else {
                Projectile.frame = 0;
                Projectile.frameCounter = 0;
            }
        }
        else {
            Projectile.frameCounter++;
            if (Projectile.frameCounter > 6) {
                Projectile.frame++;
                Projectile.frameCounter = 0;
            }

            if (Projectile.frame > 3)
                Projectile.frame = 2;

            if (Projectile.frame < 2)
                Projectile.frame = 2;
        }

        Projectile.velocity.Y += 0.4f;
        if (Projectile.velocity.Y > 14f)
            Projectile.velocity.Y = 14f;
    }
}
