using System;
using Terraria.Enums;
using Terraria.GameContent.Achievements;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.GameContent;

namespace RoA.Core.Utility.Vanilla;

static class ProjectileUtils {
    public static bool CanProjectileCutTiles(Projectile checkProjectile) {
        if (ProjectileLoader.CanCutTiles(checkProjectile) is bool modResult)
            return modResult;

        if (checkProjectile.aiStyle != 45 && checkProjectile.aiStyle != 137 && checkProjectile.aiStyle != 92 && checkProjectile.aiStyle != 105 && checkProjectile.aiStyle != 106 && !ProjectileID.Sets.IsAGolfBall[checkProjectile.type] && checkProjectile.type != 463 && checkProjectile.type != 69 && checkProjectile.type != 70 && checkProjectile.type != 621 && checkProjectile.type != 10 && checkProjectile.type != 11 && checkProjectile.type != 379 && checkProjectile.type != 407 && checkProjectile.type != 476 && checkProjectile.type != 623 && (checkProjectile.type < 625 || checkProjectile.type > 628) && checkProjectile.type != 833 && checkProjectile.type != 834 && checkProjectile.type != 835 && checkProjectile.type != 818 && checkProjectile.type != 831 && checkProjectile.type != 820 && checkProjectile.type != 864 && checkProjectile.type != 970 && checkProjectile.type != 995 && checkProjectile.type != 908)
            return checkProjectile.type != 1020;

        return false;
    }

    public static void CutTiles(Projectile projectileThatCuts) {
        if (CanProjectileCutTiles(projectileThatCuts))
            return;

        int owner = projectileThatCuts.owner;
        AchievementsHelper.CurrentlyMining = true;
        bool[] tileCutIgnorance = Main.player[owner].GetTileCutIgnorance(allowRegrowth: false, projectileThatCuts.trap);
        DelegateMethods.tilecut_0 = TileCuttingContext.AttackProjectile;
        DelegateMethods.tileCutIgnore = tileCutIgnorance;

        CutTilesAt(projectileThatCuts, projectileThatCuts.position, projectileThatCuts.width, projectileThatCuts.height);

        AchievementsHelper.CurrentlyMining = false;
    }

    public static void CutTilesAt(Projectile projectileThatCuts, Vector2 checkBoxPosition, int checkBoxWidth, int checkBoxHeight) { 
        int num = (int)(checkBoxPosition.X / 16f);
        int num2 = (int)((checkBoxPosition.X + (float)checkBoxWidth) / 16f) + 1;
        int num3 = (int)(checkBoxPosition.Y / 16f);
        int num4 = (int)((checkBoxPosition.Y + (float)checkBoxHeight) / 16f) + 1;
        if (num < 0)
            num = 0;

        if (num2 > Main.maxTilesX)
            num2 = Main.maxTilesX;

        if (num3 < 0)
            num3 = 0;

        if (num4 > Main.maxTilesY)
            num4 = Main.maxTilesY;

        bool[] tileCutIgnorance = Main.player[projectileThatCuts.owner].GetTileCutIgnorance(allowRegrowth: false, projectileThatCuts.trap);
        for (int i = num; i < num2; i++) {
            for (int j = num3; j < num4; j++) {
                if (Main.tile[i, j] != null && Main.tileCut[Main.tile[i, j].TileType] && !tileCutIgnorance[Main.tile[i, j].TileType] && WorldGen.CanCutTile(i, j, TileCuttingContext.AttackProjectile)) {
                    WorldGen.KillTile(i, j);
                    if (Main.netMode != 0)
                        NetMessage.SendData(17, -1, -1, null, 0, i, j);
                    // Extra patch context.
                }
            }
        }

        ProjectileLoader.CutTiles(projectileThatCuts);
    }

    public static void DrawSpearProjectile(Projectile projectile, Texture2D? texture = null, Texture2D? glowMaskTexture = null) {
        Projectile proj = projectile;
        texture ??= TextureAssets.Projectile[projectile.type].Value;
        SpriteEffects dir = SpriteEffects.None;
        float num = (float)Math.Atan2(proj.velocity.Y, proj.velocity.X) + 2.355f;
        Player player = Main.player[proj.owner];
        Microsoft.Xna.Framework.Rectangle value = texture.Frame();
        Microsoft.Xna.Framework.Rectangle rect = proj.getRect();
        Vector2 vector = Vector2.Zero;
        if (player.direction > 0) {
            dir = SpriteEffects.FlipHorizontally;
            vector.X = texture.Width;
            num -= (float)Math.PI / 2f;
        }

        if (player.gravDir == -1f) {
            if (proj.direction == 1) {
                dir = SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically;
                vector = new Vector2(texture.Width, texture.Height);
                num -= (float)Math.PI / 2f;
            }
            else if (proj.direction == -1) {
                dir = SpriteEffects.FlipVertically;
                vector = new Vector2(0f, texture.Height);
                num += (float)Math.PI / 2f;
            }
        }

        Vector2.Lerp(vector, value.Center.ToVector2(), 0.25f);
        float num2 = 0f;
        Vector2 vector2 = proj.Center;
        Color color = Lighting.GetColor((int)proj.Center.X / 16, (int)proj.Center.Y / 16);
        Main.EntitySpriteDraw(texture, vector2 - Main.screenPosition, value, color, num, vector, proj.scale, dir);
        color = Color.White * (1f - proj.alpha / 255f);

        if (projectile.type == ProjectileID.MushroomSpear) {
            DelegateMethods.v3_1 = new Vector3(0.1f, 0.4f, 1f);
            Utils.PlotTileLine(vector2, vector2 + Vector2.UnitY.RotatedBy(num) * value.Width, 4, DelegateMethods.CastLightOpen);
        }

        if (glowMaskTexture != null) {
            Main.EntitySpriteDraw(glowMaskTexture, vector2 - Main.screenPosition, value, color, num, vector, proj.scale, dir);
        }
    }
}
