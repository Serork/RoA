using Microsoft.Xna.Framework;

using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using System;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Pets;

sealed class Aries : ModProjectile {
    public int Frame;
    public int FrameCounter;

    public override void SetStaticDefaults() {
        Main.projPet[Type] = true;

        Projectile.SetFrameCount(4);

        ProjectileID.Sets.CharacterPreviewAnimations[Projectile.type] = ProjectileID.Sets.SimpleLoop(2, Main.projFrames[Projectile.type], 6)
            .WithOffset(0, 0)
            .WithSpriteDirection(-1)
            .WithCode(DelegateMethods.CharacterPreview.Float);
    }

    public override void SetDefaults() {
        Projectile.CloneDefaults(ProjectileID.JunimoPet); // Copy the stats of the Zephyr Fish

        AIType = ProjectileID.JunimoPet; // Mimic as the Zephyr Fish during AI.

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
        Main.player[owner].petFlagJunimoPet = false; // Relic from AIType

        if (Main.player[owner].dead)
            Main.player[owner].GetCommon().IsAriesActive = false;

        if (Main.player[owner].GetCommon().IsAriesActive)
            Projectile.timeLeft = 2;

        return base.PreAI();
    }

    public override bool PreDraw(ref Color lightColor) {
        Projectile.QuickDrawAnimated(lightColor * Projectile.Opacity, spriteEffects: (-Projectile.spriteDirection).ToSpriteEffects());

        return false;
    }

    public override void PostAI() {
    }

    public override void AI() {
        int walkFrameCounter = 16;
        int flyFrameCoutner = 8;

        Projectile.frameCounter = 0;
        if (Projectile.velocity.Y == 0.4f) {
            if ((double)Projectile.velocity.X < -0.1 || (double)Projectile.velocity.X > 0.1) {
                FrameCounter += (int)Math.Abs(Projectile.velocity.X);
                FrameCounter++;
                if (FrameCounter > walkFrameCounter) {
                    Frame++;
                    FrameCounter = 0;
                }

                if (Frame >= 2)
                    Frame = 0;
            }
            else {
                Frame = 0;
                FrameCounter = 0;
            }
        }
        else {
            FrameCounter++;
            if (FrameCounter > flyFrameCoutner) {
                Frame++;
                FrameCounter = 0;
            }

            if (Frame > 3)
                Frame = 2;

            if (Frame < 2)
                Frame = 2;
        }

        Projectile.frame = Frame;
    }
}
