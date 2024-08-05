using Microsoft.Xna.Framework;

using RoA.Common;
using RoA.Common.Druid;
using RoA.Content.Projectiles.Friendly;
using RoA.Core;
using RoA.Core.Utility;
using RoA.Utilities;

using System;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Druidic.Staffs;

sealed class PastoralRod : BaseRodItem<PastoralRodBase> {
    protected override ushort ShootType() => (ushort)ModContent.ProjectileType<ShepherdLeaves>();

    protected override void SafeSetDefaults() {
        Item.SetSize(38);
        Item.SetDefaultToUsable(ItemUseStyleID.Swing, 40, useSound: SoundID.Item7);
        Item.SetWeaponValues(4, 2f);

        NatureWeaponHandler.SetPotentialDamage(Item, 12);
        NatureWeaponHandler.SetFillingRate(Item, 0.75f);
    }
}

sealed class PastoralRodBase : BaseRodProjectile {
    protected override void SetSpawnProjectileSettings(Player player, ref Vector2 spawnPosition, ref Vector2 velocity, ref ushort count) {
        count = 2;
        Vector2 direction = (Main.MouseWorld - Projectile.Center).SafeNormalize(Vector2.One);
        velocity = direction.RotatedBy(Main.rand.NextFloatRange(MathHelper.PiOver2) + MathHelper.Pi) * 4f;
    }

    protected override void SpawnCoreDusts(float step, Player player, Vector2 corePosition) {
        for (int i = 0; i < 2; i++) {
            Vector2 randomPosition = Main.rand.NextVector2Unit();
            float areaSize = step * 2f;
            float speed = step;
            float scale = Math.Clamp(step, 0.1f, 0.8f);
            Dust dust = Dust.NewDustPerfect(corePosition + randomPosition * Main.rand.NextFloat(areaSize, areaSize + 2f),
                                            DustID.AmberBolt,
                                            randomPosition.RotatedBy(player.direction * -MathHelper.PiOver2) * speed,
                                            Scale: scale);
            dust.noGravity = true;
            dust.velocity *= 0.4f;
        }
    }
}

sealed class ShepherdLeaves : NatureProjectile {
    public override string Texture => ResourceManager.EmptyTexture;

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

    public override void AI() {
        if (Projectile.ai[0] == 0f) {
            Projectile.ai[0] = 0.2f;
        }
        Player player = Main.player[Projectile.owner];
        if (Projectile.ai[0] > 0.1f) {
            Projectile.ai[0] -= TimeSystem.LogicDeltaTime;
            if (Projectile.IsOwnerMyPlayer(player)) {
                Projectile.ai[1] = Main.MouseWorld.X;
                Projectile.ai[2] = Main.MouseWorld.Y;
            }
            Projectile.netUpdate = true;
        }
        Vector2 mousePosition = new(Projectile.ai[1], Projectile.ai[2]);
        Projectile.velocity = Vector2.SmoothStep(Projectile.velocity, Helper.VelocityToPoint(Projectile.Center, mousePosition, 6f), Projectile.ai[0]);
        Projectile.position += Projectile.velocity;
    }

    public override void PostAI() {
        if (Main.netMode != NetmodeID.Server) {
            Dust dust = Main.dust[Dust.NewDust(Projectile.Center + Main.rand.RandomPointInArea(3f, 3f), 0, 0, DustID.AmberBolt, Scale: Main.rand.NextFloat(1f, 1.3f))];
            dust.velocity *= 0.4f;
            dust.noGravity = true;
        }

        ProjectileHelper.Animate(Projectile, 4);
    }
}