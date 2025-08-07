using Microsoft.Xna.Framework;

using System;

using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Miscellaneous;

sealed class TarBomb : ModProjectile {
    public override bool IsLoadingEnabled(Mod mod) => RoA.HasRoALiquidMod();

    private static ushort TARDUSTTYPE => (ushort)RoA.RoALiquidMod.Find<ModDust>("Tar").Type;

    public override void SetStaticDefaults() {
        ProjectileID.Sets.PlayerHurtDamageIgnoresDifficultyScaling[Type] = true;
        ProjectileID.Sets.Explosive[Type] = true;
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
        if (Main.expertMode) {
            if (target.type >= NPCID.EaterofWorldsHead && target.type <= NPCID.EaterofWorldsTail) {
                modifiers.FinalDamage /= 5;
            }
        }
    }

    public override bool OnTileCollide(Vector2 oldVelocity) {
        if (Projectile.velocity.X != oldVelocity.X) {
            Projectile.velocity.X = oldVelocity.X * -0.4f;
        }
        if (Projectile.velocity.Y != oldVelocity.Y && (double)oldVelocity.Y > 0.7) {
            Projectile.velocity.Y = oldVelocity.Y * -0.4f;
        }

        return false;
    }

    public override void SetDefaults() {
        Projectile.width = 22;
        Projectile.height = 22;
        Projectile.aiStyle = -1;
        Projectile.friendly = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 180;
    }

    public override void AI() {
        if (Projectile.wet) {
            Projectile.timeLeft = 1;
        }

        if (Projectile.owner == Main.myPlayer && Projectile.timeLeft <= 3) {
            Projectile.PrepareBombToBlow(); // Get ready to explode.
        }
        else if (Main.rand.NextBool()) {
            int num28 = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, 31, 0f, 0f, 100);
            Main.dust[num28].scale = 0.1f + (float)Main.rand.Next(5) * 0.1f;
            Main.dust[num28].fadeIn = 1.5f + (float)Main.rand.Next(5) * 0.1f;
            Main.dust[num28].noGravity = true;
            Main.dust[num28].position = Projectile.Center + new Vector2(0f, -Projectile.height / 2).RotatedBy(Projectile.rotation) * 1.1f;
            int num29 = TARDUSTTYPE;
            Dust dust8 = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, num29, 0f, 0f, 100);
            dust8.scale = 1f + (float)Main.rand.Next(5) * 0.1f;
            dust8.noGravity = true;
            dust8.position = Projectile.Center + new Vector2(0f, -Projectile.height / 2 - 6).RotatedBy(Projectile.rotation) * 1.1f;
        }

        Projectile.ai[0] += 1f;
        if (Projectile.ai[0] > 10f) {
            Projectile.ai[0] = 10f;
            // Roll speed dampening. 
            if (Projectile.velocity.Y == 0f && Projectile.velocity.X != 0f) {
                Projectile.velocity.X = Projectile.velocity.X * 0.96f;

                if (Projectile.velocity.X > -0.01 && Projectile.velocity.X < 0.01) {
                    Projectile.velocity.X = 0f;
                    Projectile.netUpdate = true;
                }
            }
            // Delayed gravity
            Projectile.velocity.Y = Projectile.velocity.Y + 0.2f;
        }
        // Rotation increased by velocity.X 
        Projectile.rotation += Projectile.velocity.X * 0.1f;
    }

    public override void PrepareBombToBlow() {
        Projectile.tileCollide = false; 
        Projectile.alpha = 255;

        Projectile.damage = 100;
        Projectile.Resize(48, 48);
        Projectile.knockBack = 12f;
    }

    public override void OnKill(int timeLeft) {
        Projectile.Resize(22, 22);
        //if (type == 791)
        //    SoundEngine.PlaySound(SoundID.Item62, position);
        //else
            SoundEngine.PlaySound(SoundID.Item14, Projectile.position);

        Color transparent5 = Color.Transparent;
        int num866 = TARDUSTTYPE;
        for (int num867 = 0; num867 < 30; num867++) {
            Dust dust57 = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, 0f, 0f, 100, transparent5, 1.5f);
            Dust dust2 = dust57;
            dust2.velocity *= 1.4f;
        }

        for (int num868 = 0; num868 < 80; num868++) {
            Dust dust58 = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, num866, 0f, 0f, 100, transparent5, 2.2f);
            Dust dust2 = dust58;
            dust2.velocity *= 7f;
            dust58 = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, num866, 0f, 0f, 100, transparent5, 1.3f);
            dust2 = dust58;
            dust2.velocity *= 4f;
        }

        for (int num869 = 1; num869 <= 2; num869++) {
            for (int num870 = -1; num870 <= 1; num870 += 2) {
                for (int num871 = -1; num871 <= 1; num871 += 2) {
                    Gore gore9 = Gore.NewGoreDirect(null, Projectile.position, Vector2.Zero, Main.rand.Next(61, 64));
                    Gore gore2 = gore9;
                    gore2.velocity *= ((num869 == 1) ? 0.4f : 0.8f);
                    gore2 = gore9;
                    gore2.velocity += new Vector2(num870, num871);
                }
            }
        }

        if (Main.netMode != 1) {
            Point pt4 = Projectile.Center.ToTileCoordinates();
            Projectile.Kill_DirtAndFluidProjectiles_RunDelegateMethodPushUpForHalfBricks(pt4, 3f, SpreadTar);
        }
    }

    public static bool SpreadTar(int x, int y) {
        if (Vector2.Distance(DelegateMethods.v2_1, new Vector2(x, y)) > DelegateMethods.f_1)
            return false;

        if (WorldGen.PlaceLiquid(x, y, 5, byte.MaxValue)) {
            Vector2 position = new Vector2(x * 16, y * 16);
            int type = TARDUSTTYPE;
            for (int i = 0; i < 3; i++) {
                Dust dust = Dust.NewDustDirect(position, 16, 16, type, 0f, 0f, 100, Color.Transparent, 2.2f);
                dust.velocity.Y -= 1.2f;
                dust.velocity *= 7f;
                Dust dust2 = Dust.NewDustDirect(position, 16, 16, type, 0f, 0f, 100, Color.Transparent, 1.3f);
                dust2.velocity.Y -= 1.2f;
                dust2.velocity *= 4f;
            }

            return true;
        }

        return false;
    }
}
