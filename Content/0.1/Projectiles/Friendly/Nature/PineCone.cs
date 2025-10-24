using Microsoft.Xna.Framework;

using RoA.Common;
using RoA.Core.Utility;

using System;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class PineCone : NatureProjectile {
    protected override void SafeSetDefaults() {
        Projectile.Size = new Vector2(22, 30);

        Projectile.ignoreWater = true;
        Projectile.friendly = true;

        Projectile.tileCollide = true;

        Projectile.penetrate = 1;

        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 40;

        Projectile.aiStyle = -1;
        Projectile.timeLeft = 240;
    }

    protected override void SafeOnSpawn(IEntitySource source) {
        Player player = Projectile.GetOwnerAsPlayer();
        if (player.whoAmI != Main.myPlayer) {
            return;
        }
        Vector2 center = Helper.GetLimitedPosition(player.Center, player.GetViableMousePosition(), 200f) + new Vector2(Projectile.width * 0.2f, Projectile.Size.Y / 2f + Projectile.height * 0.1f) - new Vector2(2f, 2f);
        Projectile.Center = center;
        Projectile.netUpdate = true;
    }

    public override bool ShouldUpdatePosition() => true;

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
        modifiers.FinalDamage *= 0.5f + 0.5f * Math.Min(10f, Projectile.velocity.Y) * 0.2f;
    }

    public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) {
        modifiers.FinalDamage *= 0.5f + 0.5f * Math.Min(10f, Projectile.velocity.Y) * 0.2f;
    }

    public override bool? CanDamage() => Projectile.velocity.Y >= 2f && (Math.Abs(Projectile.rotation) < 0.1f && Math.Abs(Projectile.ai[0]) < 0.5f) || Projectile.ai[1] != 0f;

    public override void AI() {
        Projectile.Opacity = Utils.GetLerpValue(240, 235, Projectile.timeLeft, true);

        Projectile.velocity.X = 0f;

        Player player = Projectile.GetOwnerAsPlayer();
        if (Projectile.localAI[0] > 0f) {
            if (Projectile.ai[0] == 0f && Projectile.ai[1] == 0f) {
                if (Projectile.IsOwnerLocal()) {
                    Projectile.ai[0] = -(10f + Main.rand.NextFloat() * 5f) * Projectile.GetOwnerAsPlayer().direction;
                    Projectile.netUpdate = true;
                }
            }
            else {
                float angle = Math.Sign(Projectile.rotation) == Math.Sign(Projectile.ai[0]) ? 4f : 3f;
                if (Math.Abs(Projectile.rotation) < 0.5f) {
                    angle *= 0.5f;
                }
                if (Math.Abs(Projectile.rotation) < 0.25f) {
                    angle *= 0.25f;
                }
                Projectile.ai[0] += (float)-Math.Sign(Projectile.rotation) * angle;
                Projectile.ai[0] *= 0.95f;
                Projectile.rotation += Projectile.ai[0] * TimeSystem.LogicDeltaTime;
                float maxAngle = 0.6f;
                Projectile.rotation = MathHelper.Clamp(Projectile.rotation, -maxAngle, maxAngle);
                bool flag = Projectile.Center.Distance(player.GetViableMousePosition()) > 15f && player.whoAmI == Main.myPlayer;
                bool flag2 = Math.Abs(Projectile.rotation) < 0.1f && Math.Abs(Projectile.ai[0]) < 0.5f;
                if (flag2) {
                    Projectile.ai[1] = 1f;
                }
                if (flag2 || Projectile.ai[1] != 0f) {
                    if (flag || Projectile.ai[1] != 0f) {
                        Projectile.rotation *= 0.975f;

                        Projectile.ai[1] = 1f;
                        Projectile.velocity.Y += 0.35f;
                        Projectile.velocity.Y = Math.Min(10f, Projectile.velocity.Y);

                        //Projectile.position.Y += Projectile.velocity.Y;

                        //Projectile.netUpdate = true;
                    }
                }
            }

            return;
        }
        else {
            if (Projectile.ai[0] == 0f) {
                if (Projectile.IsOwnerLocal()) {
                    Projectile.ai[0] = -10f * Main.rand.NextBool().ToDirectionInt();
                    Projectile.netUpdate = true;
                }
            }
        }
        Projectile.localAI[0] += 1f;
    }

    public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
        width = height = 6;

        return base.TileCollideStyle(ref width, ref height, ref fallThrough, ref hitboxCenterFrac);
    }

    public override bool OnTileCollide(Vector2 oldVelocity) {
        if (Main.netMode != NetmodeID.Server) {
            Point point = Projectile.TopLeft.ToTileCoordinates();
            Point point2 = Projectile.BottomRight.ToTileCoordinates();
            int num2 = point.X / 2 + point2.X / 2;
            int num3 = Projectile.width / 2;
            int num4 = (int)Projectile.ai[0] / 3;
            for (int i = point.X; i <= point2.X; i++) {
                for (int j = point.Y; j <= point2.Y; j++) {
                    if (Vector2.Distance(Projectile.Center, new Vector2(i * 16, j * 16)) > num3)
                        continue;
                    Tile tileSafely = Framing.GetTileSafely(i, j);
                    if (!tileSafely.HasTile || !Main.tileSolid[tileSafely.TileType] || Main.tileSolidTop[tileSafely.TileType] || Main.tileFrameImportant[tileSafely.TileType]) {
                        continue;
                    }
                    Tile tileSafely2 = Framing.GetTileSafely(i, j - 1);
                    if (tileSafely2.HasTile && Main.tileSolid[tileSafely2.TileType] && !Main.tileSolidTop[tileSafely2.TileType]) {
                        continue;
                    }
                    int num5 = WorldGen.KillTile_GetTileDustAmount(fail: true, tileSafely, i, j);
                    for (int k = 0; k < num5; k++) {
                        Dust obj = Main.dust[WorldGen.KillTile_MakeTileDust(i, j, tileSafely)];
                        obj.velocity.Y -= 1f + num4 * 1.5f;
                        obj.velocity.Y *= Main.rand.NextFloat();
                        obj.velocity.Y *= 0.75f;
                        obj.scale += num4 * 0.03f;
                    }
                }
            }
        }

        SoundEngine.PlaySound(SoundID.Dig with { Pitch = Main.rand.NextFloat(0.8f, 1.2f) }, Projectile.Center);

        return base.OnTileCollide(oldVelocity);
    }

    public override void ModifyDamageHitbox(ref Rectangle hitbox) {
        hitbox.Inflate(4, 0);
    }

    public override void OnKill(int timeLeft) {
        if (Main.netMode == NetmodeID.Server)
            return;

        int type = 76;
        for (int i = 0; i < Main.rand.Next(4, 8); i++) {
            if (Main.rand.Next(2) == 0)
                type = ModContent.DustType<Dusts.PineCone>();
            Dust dust = Dust.NewDustDirect(Projectile.Center - Projectile.velocity * 0.1f, 8, 8, type, Projectile.velocity.X / 10f * 0.25f, Projectile.velocity.Y / 10f * 0.25f, 0, default, Main.rand.NextFloat(0.8f, 1f) * Main.rand.NextFloat(0.75f, 0.9f) * 0.85f);
            dust.fadeIn = (float)(1.3 + (double)Main.rand.NextFloat() * 0.2);
            dust.noGravity = true;
            Dust dust2 = dust;
            dust2.position += dust.velocity;
        }
    }
}