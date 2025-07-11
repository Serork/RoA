using Microsoft.Xna.Framework;

using RoA.Core.Utility;

using System;

using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class IceBlock : NatureProjectile {
    protected override void SafeSetDefaults() {
        Projectile.width = 16;
        Projectile.height = 16;
        Projectile.aiStyle = 22;
        Projectile.friendly = true;
        //Projectile.magic = true;
        Projectile.tileCollide = false;
        //Projectile.light = 0.5f;
        Projectile.coldDamage = true;
    }

    public override void AI() {
        if (Projectile.velocity.X == 0f && Projectile.velocity.Y == 0f)
            Projectile.alpha = 255;

        Dust dust2;
        if (Projectile.ai[1] < 0f) {
            if (Projectile.timeLeft > 60)
                Projectile.timeLeft = 60;

            if (Projectile.velocity.X > 0f)
                Projectile.rotation += 0.3f;
            else
                Projectile.rotation -= 0.3f;

            int num184 = (int)(Projectile.position.X / 16f) - 1;
            int num185 = (int)((Projectile.position.X + (float)Projectile.width) / 16f) + 2;
            int num186 = (int)(Projectile.position.Y / 16f) - 1;
            int num187 = (int)((Projectile.position.Y + (float)Projectile.height) / 16f) + 2;
            if (num184 < 0)
                num184 = 0;

            if (num185 > Main.maxTilesX)
                num185 = Main.maxTilesX;

            if (num186 < 0)
                num186 = 0;

            if (num187 > Main.maxTilesY)
                num187 = Main.maxTilesY;

            int num188 = (int)Projectile.position.X + 4;
            int num189 = (int)Projectile.position.Y + 4;
            Vector2 vector23 = default(Vector2);
            for (int num190 = num184; num190 < num185; num190++) {
                for (int num191 = num186; num191 < num187; num191++) {
                    if (Main.tile[num190, num191] != null && Main.tile[num190, num191].HasTile && Main.tile[num190, num191].TileType != 127 && Main.tileSolid[Main.tile[num190, num191].TileType] && !Main.tileSolidTop[Main.tile[num190, num191].TileType]) {
                        vector23.X = num190 * 16;
                        vector23.Y = num191 * 16;
                        if ((float)(num188 + 8) > vector23.X && (float)num188 < vector23.X + 16f && (float)(num189 + 8) > vector23.Y && (float)num189 < vector23.Y + 16f)
                            Projectile.Kill();
                    }
                }
            }

            int num192 = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, 67);
            Main.dust[num192].noGravity = true;
            dust2 = Main.dust[num192];
            dust2.velocity *= 0.3f;
            return;
        }

        if (Projectile.ai[0] < 0f) {
            if (Projectile.ai[0] == -1f) {
                for (int num193 = 0; num193 < 10; num193++) {
                    int num194 = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, 67, 0f, 0f, 0, default(Color), 1.1f);
                    Main.dust[num194].noGravity = true;
                    dust2 = Main.dust[num194];
                    dust2.velocity *= 1.3f;
                }
            }
            else if (Main.rand.Next(30) == 0) {
                int num195 = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, 67, 0f, 0f, 100);
                dust2 = Main.dust[num195];
                dust2.velocity *= 0.2f;
            }

            int num196 = (int)Projectile.position.X / 16;
            int num197 = (int)Projectile.position.Y / 16;
            if (Main.tile[num196, num197] == null || !Main.tile[num196, num197].HasTile)
                Projectile.Kill();

            Projectile.ai[0] -= 1f;
            if (Projectile.ai[0] <= -900f && (Main.myPlayer == Projectile.owner || Main.netMode == 2) && Main.tile[num196, num197].HasTile && Main.tile[num196, num197].TileType == 127) {
                WorldGen.KillTile(num196, num197);
                if (Main.netMode == 1)
                    NetMessage.SendData(17, -1, -1, null, 0, num196, num197);

                Projectile.Kill();
            }

            return;
        }

        int num198 = (int)(Projectile.position.X / 16f) - 1;
        int num199 = (int)((Projectile.position.X + (float)Projectile.width) / 16f) + 2;
        int num200 = (int)(Projectile.position.Y / 16f) - 1;
        int num201 = (int)((Projectile.position.Y + (float)Projectile.height) / 16f) + 2;
        if (num198 < 0)
            num198 = 0;

        if (num199 > Main.maxTilesX)
            num199 = Main.maxTilesX;

        if (num200 < 0)
            num200 = 0;

        if (num201 > Main.maxTilesY)
            num201 = Main.maxTilesY;

        int num202 = (int)Projectile.position.X + 4;
        int num203 = (int)Projectile.position.Y + 4;
        Vector2 vector24 = default(Vector2);
        for (int num204 = num198; num204 < num199; num204++) {
            for (int num205 = num200; num205 < num201; num205++) {
                if (Main.tile[num204, num205] != null && Main.tile[num204, num205].HasUnactuatedTile && Main.tile[num204, num205].TileType != 127 && Main.tileSolid[Main.tile[num204, num205].TileType] && !Main.tileSolidTop[Main.tile[num204, num205].TileType]) {
                    vector24.X = num204 * 16;
                    vector24.Y = num205 * 16;
                    if ((float)(num202 + 8) > vector24.X && (float)num202 < vector24.X + 16f && (float)(num203 + 8) > vector24.Y && (float)num203 < vector24.Y + 16f)
                        Projectile.Kill();
                }
            }
        }

        if (Projectile.lavaWet)
            Projectile.Kill();

        int num206 = (int)(Projectile.Center.X / 16f);
        int num207 = (int)(Projectile.Center.Y / 16f);
        if (WorldGen.InWorld(num206, num207) && Main.tile[num206, num207] != null && Main.tile[num206, num207].LiquidAmount > 0 && TileHelper.IsShimmer(num206, num207))
            Projectile.Kill();

        if (!Projectile.active)
            return;

        int num208 = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, 67);
        Main.dust[num208].noGravity = true;
        dust2 = Main.dust[num208];
        dust2.velocity *= 0.3f;
        int num209 = (int)Projectile.ai[0];
        int num210 = (int)Projectile.ai[1];
        if (WorldGen.InWorld(num209, num210) && WorldGen.SolidTile(num209, num210)) {
            if (Math.Abs(Projectile.velocity.X) > Math.Abs(Projectile.velocity.Y)) {
                if (Projectile.Center.Y < (float)(num210 * 16 + 8) && WorldGen.InWorld(num209, num210 - 1) && !WorldGen.SolidTile(num209, num210 - 1))
                    num210--;
                else if (WorldGen.InWorld(num209, num210 + 1) && !WorldGen.SolidTile(num209, num210 + 1))
                    num210++;
                else if (WorldGen.InWorld(num209, num210 - 1) && !WorldGen.SolidTile(num209, num210 - 1))
                    num210--;
                else if (Projectile.Center.X < (float)(num209 * 16 + 8) && WorldGen.InWorld(num209 - 1, num210) && !WorldGen.SolidTile(num209 - 1, num210))
                    num209--;
                else if (WorldGen.InWorld(num209 + 1, num210) && !WorldGen.SolidTile(num209 + 1, num210))
                    num209++;
                else if (WorldGen.InWorld(num209 - 1, num210) && !WorldGen.SolidTile(num209 - 1, num210))
                    num209--;
            }
            else if (Projectile.Center.X < (float)(num209 * 16 + 8) && WorldGen.InWorld(num209 - 1, num210) && !WorldGen.SolidTile(num209 - 1, num210)) {
                num209--;
            }
            else if (WorldGen.InWorld(num209 + 1, num210) && !WorldGen.SolidTile(num209 + 1, num210)) {
                num209++;
            }
            else if (WorldGen.InWorld(num209 - 1, num210) && !WorldGen.SolidTile(num209 - 1, num210)) {
                num209--;
            }
            else if (Projectile.Center.Y < (float)(num210 * 16 + 8) && WorldGen.InWorld(num209, num210 - 1) && !WorldGen.SolidTile(num209, num210 - 1)) {
                num210--;
            }
            else if (WorldGen.InWorld(num209, num210 + 1) && !WorldGen.SolidTile(num209, num210 + 1)) {
                num210++;
            }
            else if (WorldGen.InWorld(num209, num210 - 1) && !WorldGen.SolidTile(num209, num210 - 1)) {
                num210--;
            }
        }

        if (Projectile.velocity.X > 0f)
            Projectile.rotation += 0.3f;
        else
            Projectile.rotation -= 0.3f;

        if (Main.myPlayer != Projectile.owner)
            return;

        int num211 = (int)((Projectile.position.X + (float)(Projectile.width / 2)) / 16f);
        int num212 = (int)((Projectile.position.Y + (float)(Projectile.height / 2)) / 16f);
        bool flag8 = false;
        if (num211 == num209 && num212 == num210)
            flag8 = true;

        if (((Projectile.velocity.X <= 0f && num211 <= num209) || (Projectile.velocity.X >= 0f && num211 >= num209)) && ((Projectile.velocity.Y <= 0f && num212 <= num210) || (Projectile.velocity.Y >= 0f && num212 >= num210)))
            flag8 = true;

        if (!flag8)
            return;

        if (WorldGen.PlaceTile(num209, num210, 127, mute: false, forced: false, Projectile.owner)) {
            if (Main.netMode == 1)
                NetMessage.SendData(17, -1, -1, null, 1, num209, num210, 127f);

            Projectile.damage = 0;
            Projectile.ai[0] = -1f;
            Projectile.velocity *= 0f;
            Projectile.alpha = 255;
            Projectile.position.X = num209 * 16;
            Projectile.position.Y = num210 * 16;
            Projectile.netUpdate = true;
        }
        else {
            Projectile.ai[1] = -1f;
        }

        float num = 0.5f;
        float num2 = 0.5f;
        float num3 = 0.5f;
        num *= 0f;
        num2 *= 0.8f;
        num3 *= 1f;
        Lighting.AddLight((int)((Projectile.position.X + (float)(Projectile.width / 2)) / 16f), (int)((Projectile.position.Y + (float)(Projectile.height / 2)) / 16f), num, num2, num3);

        if (Projectile.lavaWet) {
            Projectile.wet = false;
            if (Projectile.ai[0] >= 0f) {
                Projectile.Kill();
            }
        }
    }

    public override void OnKill(int timeLeft) {
        if (Projectile.ai[0] >= 0f) {
            SoundEngine.PlaySound(SoundID.Item27, Projectile.position);
            for (int num624 = 0; num624 < 10; num624++) {
                Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, DustID.IceRod);
            }
        }

        int num625 = (int)Projectile.position.X / 16;
        int num626 = (int)Projectile.position.Y / 16;
        if (Main.tile[num625, num626].TileType == 127 && Main.tile[num625, num626].HasTile)
            WorldGen.KillTile(num625, num626);
    }
}
