using Microsoft.Xna.Framework;

using RoA.Common;
using RoA.Common.VisualEffects;
using RoA.Content.Dusts;
using RoA.Content.VisualEffects;
using RoA.Core.Defaults;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using System;
using System.Collections.Generic;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Miscellaneous;

[Tracked]
sealed class HornetSpear : ModProjectile {
    private static ushort STARTDISAPPEARINGTIME => 5;

    public override void SetStaticDefaults() => Projectile.SetTrail(0, 3);

    public override void SetDefaults() {
        Projectile.SetSizeValues(10);

        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Melee;
        Projectile.aiStyle = -1;

        Projectile.tileCollide = false;
        Projectile.penetrate = -1;

        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;

        //Projectile.hide = true;

        Projectile.ownerHitCheck = true;
        Projectile.ownerHitCheckDistance = 300f;

        Projectile.manualDirectionChange = true;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        if (Projectile.ai[2] == 0f) {
            Vector2 position = target.Center;
            float rotation = Projectile.rotation + MathHelper.PiOver2;
            if (Projectile.direction > 0) {
                rotation += MathHelper.Pi;
            }
            for (int i = 0; i < 4; i++) {
                int type = ModContent.DustType<Slash>();
                Dust dust = Dust.NewDustPerfect(position, type, Vector2.UnitY.RotatedBy(rotation + Main.rand.NextFloatRange(MathHelper.PiOver4)) * 5f, newColor: Color.White);
                dust.noGravity = true;
                dust.noLight = true;
                dust.customData = 1f;
                dust.scale = 1f + 0.5f * Main.rand.NextFloatDirection();
                dust = Dust.NewDustPerfect(position, type, Vector2.UnitY.RotatedBy(MathHelper.Pi + rotation + Main.rand.NextFloatRange(MathHelper.PiOver4)) * 5f, newColor: Color.White);
                dust.noGravity = true;
                dust.noLight = true;
                dust.customData = 1f;
                dust.scale = 1f + 0.5f * Main.rand.NextFloatDirection();
                Vector2 vector3 = ((float)Math.PI / 4f * i).ToRotationVector2() * 4f;
                dust = Dust.NewDustPerfect(position, type, vector3.RotatedBy(rotation + Main.rand.NextFloatDirection() * ((float)Math.PI * 2f) * 0.025f) * 5f * Main.rand.NextFloat(), newColor: Color.White);
                dust.noGravity = true;
                dust.noLight = true;
                dust.scale = 0.8f;
                dust.customData = 1f;
                Dust dust2 = Dust.NewDustPerfect(position, type, -vector3.RotatedBy(rotation + Main.rand.NextFloatDirection() * ((float)Math.PI * 2f) * 0.025f) * 5f * Main.rand.NextFloat(), newColor: Color.White);
                dust2.noGravity = true;
                dust2.noLight = true;
                dust2.scale = 1.4f;
                dust.customData = 1f;
            }

            int layer = VisualEffectLayer.ABOVEPLAYERS;
            VisualEffectSystem.New<HornetHit>(layer).
                Setup(position,
                      Vector2.Zero,
                      Color.White,
                      rotation: rotation);

            Projectile.ai[2] = 1f;
            Projectile.ai[1] = 1f;
            Projectile.netUpdate = true;
        }
    }

    public bool CanFunction => Projectile.timeLeft > STARTDISAPPEARINGTIME - 1;
    public override bool? CanDamage() => CanFunction;
    public override bool? CanCutTiles() => false;

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) => behindProjectiles.Add(index);

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        float f = Projectile.rotation - MathHelper.PiOver2;
        float collisionPoint2 = 0f;
        float num10 = 70f;
        if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, Projectile.Center + f.ToRotationVector2() * num10, 20f * Projectile.scale, ref collisionPoint2)) {
            return true;
        }

        return false;
    }

    public override void AI() {
        if (Projectile.ai[0] != 0f) {
            Projectile.timeLeft = (int)Projectile.ai[0];
            Projectile.localAI[1] = Projectile.timeLeft;
            Projectile.ai[0] = 0f;
        }
        if (Projectile.ai[1] == 1f && Projectile.timeLeft > STARTDISAPPEARINGTIME * 2) {
            Projectile.timeLeft = STARTDISAPPEARINGTIME * 2;
            Projectile.ai[1] = 0f;
        }

        Player owner = Projectile.GetOwnerAsPlayer();
        if (!owner.IsAlive()) {
            return;
        }

        owner.UseBodyFrame(Core.Data.PlayerFrame.Use4);

        owner.heldProj = Projectile.whoAmI;

        Projectile.SetDirection(owner.direction);
        if (Projectile.localAI[0] != Projectile.direction) {
            Projectile.velocity.X = MathF.Abs(Projectile.velocity.X) * Projectile.direction;
            Projectile.localAI[0] = Projectile.direction;
        }
        Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        if (Projectile.ai[2] == 1f) {
            Projectile.rotation += owner.fullRotation;
        }
        int maxTimeLeft = (int)Projectile.localAI[1];
        Projectile.scale = Utils.GetLerpValue(maxTimeLeft, maxTimeLeft - STARTDISAPPEARINGTIME / 2, Projectile.timeLeft, true);

        Projectile.Center = owner.Center;
        Projectile.Center = Utils.Floor(Projectile.Center) + Vector2.UnitY * owner.gfxOffY + -Vector2.UnitY.RotatedBy(Projectile.rotation) * 20f;
    }

    public override bool ShouldUpdatePosition() => false;

    public override void OnKill(int timeLeft) {
        base.OnKill(timeLeft);
    }

    public override bool PreDraw(ref Color lightColor) {
        //int timeLeft = Projectile.timeLeft;
        //int maxTimeLeft = (int)Projectile.localAI[1];
        //float colorLerpProgress = Utils.GetLerpValue(0, STARTDISAPPEARINGTIME, timeLeft, true);
        //Color baseColor = Color.Lerp(Color.Black, Color.White, colorLerpProgress);
        //lightColor = baseColor.MultiplyRGB(lightColor) * colorLerpProgress;

        if (CanFunction) {
            Projectile.QuickDrawShadowTrails(lightColor * Projectile.Opacity, 0.25f, 1, Projectile.rotation);
        }
        Projectile.QuickDraw(lightColor);

        return false;
    }
}
