using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Content.Projectiles.Friendly;

using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Core.Utility;

static partial class ProjectileExtensions {
    public static bool IsModded(this Projectile projectile) => projectile.ModProjectile is not null;
    public static bool IsModded(this Projectile projectile, out ModProjectile modProjectile) {
        modProjectile = projectile.ModProjectile;
        return modProjectile is not null;
    }

    public static Texture2D GetTexture(this Projectile projectile) => TextureAssets.Projectile[projectile.type].Value;

    public static void SetTrail(this Projectile projectile, int trailingMode = 2, int length = -1) {
        if (length > 0) {
            ProjectileID.Sets.TrailCacheLength[projectile.type] = length;
        }
        ProjectileID.Sets.TrailingMode[projectile.type] = trailingMode;
    }

    public static int GetTrailCount(this Projectile projectile) => ProjectileID.Sets.TrailCacheLength[projectile.type];

    public static bool IsDamageable(this Projectile projectile) => projectile.damage > 0;

    public static bool IsNature(this Projectile projectile) => projectile.ModProjectile is NatureProjectile;

    public static bool IsNature(this Projectile projectile, out NatureProjectile result) {
        if (projectile.ModProjectile is NatureProjectile natureProjectile) {
            result = natureProjectile;
            return true;
        }

        result = null;
        return false;
    }

    public static Player GetOwnerAsPlayer(this Projectile projectile) => Main.player[projectile.owner];
    public static bool IsOwnerLocal(this Projectile projectile) => projectile.owner == Main.myPlayer;

    public static T As<T>(this Projectile projectile) where T : ModProjectile => projectile.ModProjectile as T;

    public static void ApplyFlaskEffects(this Projectile projectile, NPC target) {
        Player player = Main.player[projectile.owner];
        if (player.meleeEnchant > 0 && !projectile.noEnchantments) {
            byte meleeEnchant = player.meleeEnchant;
            if (meleeEnchant == 1)
                target.AddBuff(70, 60 * Main.rand.Next(5, 10));

            if (meleeEnchant == 2)
                target.AddBuff(39, 60 * Main.rand.Next(3, 7));

            if (meleeEnchant == 3)
                target.AddBuff(24, 60 * Main.rand.Next(3, 7));

            if (meleeEnchant == 5)
                target.AddBuff(69, 60 * Main.rand.Next(10, 20));

            if (meleeEnchant == 6)
                target.AddBuff(31, 60 * Main.rand.Next(1, 4));

            if (meleeEnchant == 8)
                target.AddBuff(20, 60 * Main.rand.Next(5, 10));

            if (meleeEnchant == 4)
                target.AddBuff(72, 120);
        }
    }

    public static void ApplyFlaskEffects(this Projectile projectile, Player target) {
        Player player = Main.player[projectile.owner];
        if (player.meleeEnchant > 0 && !projectile.noEnchantments) {
            byte meleeEnchant = player.meleeEnchant;
            if (meleeEnchant == 1)
                target.AddBuff(70, 60 * Main.rand.Next(5, 10));

            if (meleeEnchant == 2)
                target.AddBuff(39, 60 * Main.rand.Next(3, 7));

            if (meleeEnchant == 3)
                target.AddBuff(24, 60 * Main.rand.Next(3, 7));

            if (meleeEnchant == 5)
                target.AddBuff(69, 60 * Main.rand.Next(10, 20));

            if (meleeEnchant == 6)
                target.AddBuff(31, 60 * Main.rand.Next(1, 4));

            if (meleeEnchant == 8)
                target.AddBuff(20, 60 * Main.rand.Next(5, 10));

            if (meleeEnchant == 4)
                target.AddBuff(72, 120);
        }
    }

    public static void EmitEnchantmentVisualsAtForNonMelee(this Projectile projectile, Vector2 boxPosition, int boxWidth, int boxHeight) {
        CombinedHooks.EmitEnchantmentVisualsAt(projectile, boxPosition, boxWidth, boxHeight);
        Player player = Main.player[projectile.owner];

        if (player.frostBurn && projectile.friendly && !projectile.hostile && Main.rand.Next(2 * (1 + projectile.extraUpdates)) == 0) {
            int num = Dust.NewDust(boxPosition, boxWidth, boxHeight, 135, projectile.velocity.X * 0.2f + (float)(projectile.direction * 3), projectile.velocity.Y * 0.2f, 100, default(Color), 2f);
            Main.dust[num].noGravity = true;
            Main.dust[num].velocity *= 0.7f;
            Main.dust[num].velocity.Y -= 0.5f;
        }

        if (player.magmaStone && Main.rand.Next(3) != 0) {
            int num2 = Dust.NewDust(new Vector2(boxPosition.X - 4f, boxPosition.Y - 4f), boxWidth + 8, boxHeight + 8, 6, projectile.velocity.X * 0.2f, projectile.velocity.Y * 0.2f, 100, default(Color), 2f);
            if (Main.rand.Next(2) == 0)
                Main.dust[num2].scale = 1.5f;

            Main.dust[num2].noGravity = true;
            Main.dust[num2].velocity.X *= 2f;
            Main.dust[num2].velocity.Y *= 2f;
        }

        if (player.meleeEnchant <= 0)
            return;

        if (player.meleeEnchant == 1 && Main.rand.Next(3) == 0) {
            int num3 = Dust.NewDust(boxPosition, boxWidth, boxHeight, 171, 0f, 0f, 100);
            Main.dust[num3].noGravity = true;
            Main.dust[num3].fadeIn = 1.5f;
            Main.dust[num3].velocity *= 0.25f;
        }

        if (player.meleeEnchant == 1) {
            if (Main.rand.Next(3) == 0) {
                int num4 = Dust.NewDust(boxPosition, boxWidth, boxHeight, 171, 0f, 0f, 100);
                Main.dust[num4].noGravity = true;
                Main.dust[num4].fadeIn = 1.5f;
                Main.dust[num4].velocity *= 0.25f;
            }
        }
        else if (player.meleeEnchant == 2) {
            if (Main.rand.Next(2) == 0) {
                int num5 = Dust.NewDust(boxPosition, boxWidth, boxHeight, 75, projectile.velocity.X * 0.2f + (float)(projectile.direction * 3), projectile.velocity.Y * 0.2f, 100, default(Color), 2.5f);
                Main.dust[num5].noGravity = true;
                Main.dust[num5].velocity *= 0.7f;
                Main.dust[num5].velocity.Y -= 0.5f;
            }
        }
        else if (player.meleeEnchant == 3) {
            if (Main.rand.Next(2) == 0) {
                int num6 = Dust.NewDust(boxPosition, boxWidth, boxHeight, 6, projectile.velocity.X * 0.2f + (float)(projectile.direction * 3), projectile.velocity.Y * 0.2f, 100, default(Color), 2.5f);
                Main.dust[num6].noGravity = true;
                Main.dust[num6].velocity *= 0.7f;
                Main.dust[num6].velocity.Y -= 0.5f;
            }
        }
        else if (player.meleeEnchant == 4) {
            int num7 = 0;
            if (Main.rand.Next(2) == 0) {
                num7 = Dust.NewDust(boxPosition, boxWidth, boxHeight, 57, projectile.velocity.X * 0.2f + (float)(projectile.direction * 3), projectile.velocity.Y * 0.2f, 100, default(Color), 1.1f);
                Main.dust[num7].noGravity = true;
                Main.dust[num7].velocity.X /= 2f;
                Main.dust[num7].velocity.Y /= 2f;
            }
        }
        else if (player.meleeEnchant == 5) {
            if (Main.rand.Next(2) == 0) {
                int num8 = Dust.NewDust(boxPosition, boxWidth, boxHeight, 169, 0f, 0f, 100);
                Main.dust[num8].velocity.X += projectile.direction;
                Main.dust[num8].velocity.Y += 0.2f;
                Main.dust[num8].noGravity = true;
            }
        }
        else if (player.meleeEnchant == 6) {
            if (Main.rand.Next(2) == 0) {
                int num9 = Dust.NewDust(boxPosition, boxWidth, boxHeight, 135, 0f, 0f, 100);
                Main.dust[num9].velocity.X += projectile.direction;
                Main.dust[num9].velocity.Y += 0.2f;
                Main.dust[num9].noGravity = true;
            }
        }
        else if (player.meleeEnchant == 7) {
            Vector2 vector = projectile.velocity;
            if (vector.Length() > 4f)
                vector *= 4f / vector.Length();

            if (Main.rand.Next(20) == 0) {
                int num10 = Main.rand.Next(139, 143);
                int num11 = Dust.NewDust(boxPosition, boxWidth, boxHeight, num10, vector.X, vector.Y, 0, default(Color), 1.2f);
                Main.dust[num11].velocity.X *= 1f + (float)Main.rand.Next(-50, 51) * 0.01f;
                Main.dust[num11].velocity.Y *= 1f + (float)Main.rand.Next(-50, 51) * 0.01f;
                Main.dust[num11].velocity.X += (float)Main.rand.Next(-50, 51) * 0.05f;
                Main.dust[num11].velocity.Y += (float)Main.rand.Next(-50, 51) * 0.05f;
                Main.dust[num11].scale *= 1f + (float)Main.rand.Next(-30, 31) * 0.01f;
            }

            if (Main.rand.Next(40) == 0) {
                if (!Main.dedServ) {
                    int num12 = Main.rand.Next(276, 283);
                    int num13 = Gore.NewGore(projectile.GetSource_FromAI(), projectile.position, vector, num12);
                    Main.gore[num13].velocity.X *= 1f + (float)Main.rand.Next(-50, 51) * 0.01f;
                    Main.gore[num13].velocity.Y *= 1f + (float)Main.rand.Next(-50, 51) * 0.01f;
                    Main.gore[num13].scale *= 1f + (float)Main.rand.Next(-20, 21) * 0.01f;
                    Main.gore[num13].velocity.X += (float)Main.rand.Next(-50, 51) * 0.05f;
                    Main.gore[num13].velocity.Y += (float)Main.rand.Next(-50, 51) * 0.05f;
                }
            }
        }
        else if (player.meleeEnchant == 8 && Main.rand.Next(4) == 0) {
            int num14 = Dust.NewDust(boxPosition, boxWidth, boxHeight, 46, 0f, 0f, 100);
            Main.dust[num14].noGravity = true;
            Main.dust[num14].fadeIn = 1.5f;
            Main.dust[num14].velocity *= 0.25f;
        }
    }
}
