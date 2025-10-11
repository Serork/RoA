using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common;
using RoA.Common.Projectiles;
using RoA.Core.Utility.Extensions;

using System;
using System.Collections.Generic;

using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.LiquidsSpecific;

[Tracked]
sealed class FloatingRemains : ModProjectile {
    public override void SetStaticDefaults() {
        Projectile.SetFrameCount(6);
    }

    public override void SetDefaults() {
        Projectile.width = 20;
        Projectile.height = 16;
        Projectile.friendly = true;
        Projectile.aiStyle = -1;
        Projectile.tileCollide = true;
        Projectile.netImportant = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 86400;
        Projectile.manualDirectionChange = true;
        Projectile.hide = true;
    }

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
        if ((int)Projectile.ai[1] != 0) {
            overWiresUI.Add(index);
        }
        else {
            behindProjectiles.Add(index);
        }
    }

    public override bool OnTileCollide(Vector2 oldVelocity) => false;

    public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
        width = height = 10;

        return base.TileCollideStyle(ref width, ref height, ref fallThrough, ref hitboxCenterFrac);
    }

    public override void AI() {
        if (Projectile.ai[1] == 0f) {
            Projectile.ai[1] = Main.rand.NextBool().ToDirectionInt();
        }
        if (Projectile.ai[0] == 0f) {
            Projectile.ai[0] = 1f;

            for (int i = 0; i < 15; i++) {
                Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Projectile.velocity, Type, 0, 0f, Projectile.owner, 1f);
            }
        }

        float num = 0.25f;
        float num2 = Projectile.width / 2;
        for (int i = 0; i < 1000; i++) {
            if (i != Projectile.whoAmI && Main.projectile[i].active && Main.projectile[i].type == Projectile.type && Math.Abs(Projectile.position.X - Main.projectile[i].position.X) + Math.Abs(Projectile.position.Y - Main.projectile[i].position.Y) < num2) {
                if (Projectile.position.X < Main.projectile[i].position.X)
                    Projectile.velocity.X -= num;
                else
                    Projectile.velocity.X += num;

                if (Projectile.position.Y < Main.projectile[i].position.Y)
                    Projectile.velocity.Y -= num;
                else
                    Projectile.velocity.Y += num;
            }
        }

        if (Projectile.frameCounter == 0) {
            Projectile.frameCounter = 1;
            Projectile.frame = Main.rand.Next(Main.projFrames[Type]);
            Projectile.SetDirection(Main.rand.NextBool().ToDirectionInt());
        }

        if (Projectile.wet) {
            Projectile.velocity.X *= 0.9f;
            int num3 = (int)(Projectile.Center.X + (float)((Projectile.width / 2 + 8) * Projectile.direction)) / 16;
            int num4 = (int)(Projectile.Center.Y / 16f);
            _ = Projectile.position.Y / 16f;
            int num5 = (int)((Projectile.position.Y + (float)Projectile.height) / 16f);

            if (Projectile.velocity.Y > 0f)
                Projectile.velocity.Y *= 0.5f;

            num3 = (int)(Projectile.Center.X / 16f);
            num4 = (int)(Projectile.Center.Y / 16f);
            float num6 = AI_061_FishingBobber_GetWaterLine(num3, num4) - 2f;
            if (Projectile.Center.Y > num6) {
                Projectile.velocity.Y -= 0.1f;
                if (Projectile.velocity.Y < -8f)
                    Projectile.velocity.Y = -8f;

                if (Projectile.Center.Y + Projectile.velocity.Y < num6)
                    Projectile.velocity.Y = num6 - Projectile.Center.Y;
            }
            else {
                Projectile.velocity.Y = num6 - Projectile.Center.Y;
            }
        }
        else {
            if (Projectile.velocity.Y == 0f)
                Projectile.velocity.X *= 0.95f;

            Projectile.velocity.X *= 0.98f;
            Projectile.velocity.Y += 0.3f;
            if (Projectile.velocity.Y > 15.9f)
                Projectile.velocity.Y = 15.9f;
        }
    }

    private float AI_061_FishingBobber_GetWaterLine(int X, int Y) {
        float result = Projectile.position.Y + (float)Projectile.height;

        if (Main.tile[X, Y - 1].LiquidAmount > 0) {
            result = Y * 16;
            result -= (float)((int)Main.tile[X, Y - 1].LiquidAmount / 16);
        }
        else if (Main.tile[X, Y].LiquidAmount > 0) {
            result = (Y + 1) * 16;
            result -= (float)((int)Main.tile[X, Y].LiquidAmount / 16);
        }
        else if (Main.tile[X, Y + 1].LiquidAmount > 0) {
            result = (Y + 2) * 16;
            result -= (float)((int)Main.tile[X, Y + 1].LiquidAmount / 16);
        }

        return result;
    }
}
