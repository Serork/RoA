using Microsoft.Xna.Framework;

using RoA.Common;
using RoA.Common.Druid;
using RoA.Core;
using RoA.Core.Utility;

using System;
using System.IO;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace RoA.Content.Projectiles.Friendly.Druidic;

sealed class ShepherdLeaves : NatureProjectile {
    private Color _color;

    protected override void SafeSendExtraAI(BinaryWriter writer) {
        base.SafeSendExtraAI(writer);

        writer.WriteRGB(_color);
    }

    protected override void SafeReceiveExtraAI(BinaryReader reader) {
        base.SafeReceiveExtraAI(reader);

        _color = reader.ReadRGB();
    }

    public override Color? GetAlpha(Color lightColor) => _color.MultiplyRGB(lightColor) * Projectile.Opacity;

    public override string Texture => ResourceManager.ProjectileTextures + "ShepherdLeaves2";

    public override bool PreDraw(ref Color lightColor) => true;

    public override void SetStaticDefaults() => Main.projFrames[Type] = 3;

    protected override void SafeSetDefaults() {
        Projectile.Size = 8 * Vector2.One;
        Projectile.aiStyle = -1;
        Projectile.friendly = true;
        Projectile.tileCollide = false;
        Projectile.timeLeft = 100;
        Projectile.netImportant = true;
    }

    public override bool ShouldUpdatePosition() => false;

    protected override void SafeOnSpawn(IEntitySource source) {
        if (Projectile.ai[0] != 0f) {
            _color = Helper.FromHexRgb(Convert.ToUInt32(Helper.ToHexString(Projectile.ai[0]), 16));

            Projectile.ai[0] = 0.2f;
        }
    }

    public override void AI() {
        Player player = Main.player[Projectile.owner];
        if (Projectile.IsOwnerMyPlayer(player)) {
            Vector2 pointPosition = Main.player[Projectile.owner].GetViableMousePosition();
            Projectile.ai[1] = pointPosition.X;
            Projectile.ai[2] = pointPosition.Y;
            Projectile.netUpdate = true;
        }
        if (Projectile.ai[0] > 0.1f) {
            Projectile.ai[0] -= TimeSystem.LogicDeltaTime * NatureWeaponHandler.GetUseSpeedMultiplier(player.GetSelectedItem(), player);
        }
        Vector2 mousePosition = new(Projectile.ai[1], Projectile.ai[2]);
        Projectile.velocity = Vector2.SmoothStep(Projectile.velocity, Helper.VelocityToPoint(Projectile.Center, mousePosition, 6f), Projectile.ai[0]);
        Projectile.position += Projectile.velocity;

        Projectile.Opacity = Utils.GetLerpValue(0, 10, Projectile.timeLeft, true);
    }

    public override void SafePostAI() {
        //if (Main.netMode != NetmodeID.Server) {
        //    Dust dust = Main.dust[Dust.NewDust(Projectile.Center + Main.rand.RandomPointInArea(3f, 3f), 0, 0, DustID.AmberBolt, Scale: Main.rand.NextFloat(1f, 1.3f))];
        //    dust.velocity *= 0.4f;
        //    dust.noGravity = true;
        //}

        ProjectileHelper.Animate(Projectile, 4);
    }
}