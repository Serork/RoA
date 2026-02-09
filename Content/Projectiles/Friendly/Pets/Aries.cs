using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using System;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Pets;

sealed class Aries : ModProjectile {
    private static Asset<Texture2D> _glowTexture = null!;

    public int Frame;
    public int FrameCounter;

    public override void SetStaticDefaults() {
        Main.projPet[Type] = true;

        Projectile.SetFrameCount(4);

        ProjectileID.Sets.CharacterPreviewAnimations[Type] = ProjectileID.Sets
            .SimpleLoop(2, 2, 8)
            .WithOffset(0, 0)
            .WithSpriteDirection(-1)
            .WithCode(DelegateMethods.CharacterPreview.Float);

        if (!Main.dedServ) {
            _glowTexture = ModContent.Request<Texture2D>(Texture + "_Glow");
        }
    }

    public override bool? CanDamage() => false;
    public override bool? CanCutTiles() => false;

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
        Projectile.QuickDrawAnimated(Color.White * 0.9f * Projectile.Opacity, spriteEffects: (-Projectile.spriteDirection).ToSpriteEffects(), texture: _glowTexture.Value);

        return false;
    }

    public override void PostAI() {
    }

    public override void AI() {
        int walkFrameCounter = 20;
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
