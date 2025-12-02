using Microsoft.Xna.Framework;

using ModLiquidLib.ModLoader;

using RoA.Content.Liquids;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.LiquidsSpecific;

sealed class TarRocket : ModProjectile {
    public override void SetDefaults() {
        Projectile.CloneDefaults(799);
        AIType = 799;
    }

    public override bool OnTileCollide(Vector2 oldVelocity) {
        Projectile.Kill();
        return true;
    }

    public override void OnKill(int timeLeft) {
        TarLiquidExplosiveKill(Projectile);
    }

    public static void TarLiquidExplosiveKill(Projectile proj, bool isGrenade = false) {
        proj.Resize(22, 22);
        if (isGrenade) {
            SoundEngine.PlaySound(in SoundID.Item62, proj.position);
        }
        else {
            SoundEngine.PlaySound(in SoundID.Item14, proj.position);
        }
        Color color = Color.Transparent;
        int type = ModContent.DustType<Dusts.Tar>();
        for (int i = 0; i < 30; i++) {
            Dust dust = Dust.NewDustDirect(proj.position, proj.width, proj.height, DustID.Smoke, 0f, 0f, 100, color, 1.5f);
            dust.velocity *= 1.4f;
        }
        for (int i = 0; i < 80; i++) {
            Dust dust = Dust.NewDustDirect(proj.position, proj.width, proj.height, type, 0f, 0f, 100, color, 2.2f);
            dust.velocity *= 7f;
            Dust dust2 = Dust.NewDustDirect(proj.position, proj.width, proj.height, type, 0f, 0f, 100, color, 1.3f);
            dust2.velocity *= 4f;
        }
        for (int i = 1; i <= 2; i++) {
            for (int x = -1; x <= 1; x += 2) {
                for (int y = -1; y <= 1; y += 2) {
                    Gore gore = Gore.NewGoreDirect(new EntitySource_Death(proj), proj.position, Vector2.Zero, Main.rand.Next(61, 64));
                    gore.velocity *= ((i == 1) ? 0.4f : 0.8f);
                    gore.velocity += new Vector2(x, y);
                }
            }
        }
        if (Main.netMode != NetmodeID.MultiplayerClient) {
            Point projPos = proj.Center.ToTileCoordinates();
            proj.Kill_DirtAndFluidProjectiles_RunDelegateMethodPushUpForHalfBricks(projPos, 3f, SpreadTarLiquid);
        }
    }

    public static bool SpreadTarLiquid(int x, int y) {
        if (Vector2.Distance(DelegateMethods.v2_1, new Vector2(x, y)) > DelegateMethods.f_1)
            return false;

        if (WorldGen.PlaceLiquid(x, y, (byte)LiquidLoader.LiquidType<Tar>(), byte.MaxValue)) {
            Vector2 position = new Vector2(x * 16, y * 16);
            int type = ModContent.DustType<Dusts.Tar>();
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
