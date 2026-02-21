using Microsoft.Xna.Framework;

using RoA.Common.Druid;
using RoA.Common.Players;
using RoA.Common.VisualEffects;
using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Content.AdvancedDusts;
using RoA.Core.Defaults;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;

using System;

using Terraria;
using Terraria.Audio;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Nature.Hardmode.Canes;

sealed class GlacierCane : CaneBaseItem<GlacierCane.GlacierCaneBase> {
    protected override void SafeSetDefaults() {
        Item.SetSizeValues(42, 44);
        Item.SetWeaponValues(60, 4f);
        Item.SetUsableValues(ItemUseStyleID.None, 30, useSound: SoundID.Item7);
        Item.SetShopValues(ItemRarityColor.LightRed4, Item.sellPrice());

        NatureWeaponHandler.SetPotentialDamage(Item, 100);
        NatureWeaponHandler.SetFillingRateModifier(Item, 0.5f);

        Item.autoReuse = true;
    }

    protected override ushort ProjectileTypeToCreate() => (ushort)ModContent.ProjectileType<GlacierSpike>();

    public sealed class GlacierCaneBase : CaneBaseProjectile {
        public bool ShouldReleaseCane => AttackProgress01 >= 1f || !IsInUse;

        public override bool IsInUse => Owner.IsAliveAndFree() && Owner.controlUseItem;

        public override void SetStaticDefaults() {
            ProjectileID.Sets.NeedsUUID[Type] = true;
        }

        protected override void SetSpawnProjectileSettings(Player player, ref Vector2 spawnPosition, ref Vector2 velocity, ref ushort count, ref float ai0, ref float ai1, ref float ai2) {
            ai2 = Projectile.GetByUUID(Projectile.owner, Projectile.whoAmI);
        }

        protected override bool ShouldWaitUntilProjDespawn() => false;
        protected override bool ShouldShoot() => false;

        protected override Vector2 CorePositionOffsetFactor() => new(0.075f, -0.165f);

        protected override void Initialize() {
            ShootProjectile();
        }

        protected override void AfterProcessingCane() {
            if (ShouldReleaseCane) {
                if (!ShotWhenEndedAttackAnimation) {
                    SoundEngine.PlaySound(SoundID.Item28, CorePosition);
                    SpawnDustsOnShoot(Owner, CorePosition);
                    ReleaseCane();
                }
                ShotWhenEndedAttackAnimation = true;
            }
        }

        protected override void SpawnCoreDustsBeforeShoot(float step, Player player, Vector2 corePosition) {
            player.SyncMousePosition();
            Vector2 mousePosition = player.GetViableMousePosition();
            if (Main.rand.Next(10) < 5) {
                if (Main.rand.NextBool(6)) {
                    return;
                }
                float progress = 1f - MathHelper.Clamp(step, 0f, 1f);
                float offset = 40f * progress;
                Vector2 randomOffset = Main.rand.RandomPointInArea(offset, offset), spawnPosition = corePosition + randomOffset;
                float velocityFactor = MathHelper.Clamp(Vector2.Distance(spawnPosition, corePosition) / offset, 0.25f, 1f) * 2f * Math.Max(step, 0.25f) + 0.25f;
                if (Main.rand.NextChance(progress)) {
                    Snowflake? snowFlake = AdvancedDustSystem.New<Snowflake>(AdvancedDustLayer.ABOVEPLAYERS)?.Setup(spawnPosition, (corePosition - spawnPosition).SafeNormalize(Vector2.One) * velocityFactor * 0.9f, scale: MathHelper.Clamp(velocityFactor * 1.4f, 1.2f, 1.75f) * 0.75f);
                    if (snowFlake != null) {
                        snowFlake.CorePosition = corePosition;
                    }
                }
                if (Main.rand.NextChance(0.4f)) {
                    Vector2 velocity = Helper.VelocityToPoint(CorePosition - Vector2.One * 1f, mousePosition, 2.5f + Main.rand.NextFloatRange(1f));
                    Vector2 vector2 = velocity.RotatedBy(MathHelper.Pi * Main.rand.NextFloatDirection() * 0.75f);
                    Dust dust = Dust.NewDustDirect(CorePosition, 5, 5, Main.rand.NextBool(3) ? DustID.Ice : DustID.BubbleBurst_Blue, Scale: Main.rand.NextFloat(1.05f, 1.35f) * 1.25f);
                    dust.velocity = vector2 * Main.rand.NextFloat(0.8f, 1.1f);
                    dust.noGravity = true;
                    dust.alpha = Main.rand.Next(50, 100);
                }
            }
        }

        protected override void SpawnDustsOnShoot(Player player, Vector2 corePosition) {
            player.SyncMousePosition();
            Vector2 mousePosition = player.GetViableMousePosition();
            int count = 10 + (int)(AttackProgress01 * 4);
            for (int i = 0; i < count; i++) {
                if (Main.rand.NextBool()) {
                    continue;
                }
                if (Main.rand.NextBool(5)) {
                    continue;
                }
                float progress = (float)i / count;
                Vector2 velocity = corePosition.DirectionTo(mousePosition).RotatedByRandom(MathHelper.Pi * Main.rand.NextFloatDirection() * 0.75f).SafeNormalize(Vector2.One) * Main.rand.NextFloat(5f, 10f) * MathHelper.Lerp(0.5f, 0.75f, AttackProgress01);
                AdvancedDustSystem.New<Snowflake>(AdvancedDustLayer.ABOVEPLAYERS)?.
                    Setup(
                    corePosition + Main.rand.RandomPointInArea(10f, 10f),
                    velocity,
                    scale: Main.rand.NextFloat(0.9f, 1.1f) * 1.25f * 0.9f);
                velocity = corePosition.DirectionTo(mousePosition).RotatedByRandom(MathHelper.Pi * Main.rand.NextFloatDirection() * 0.75f).SafeNormalize(Vector2.One) * Main.rand.NextFloat(5f, 10f) * MathHelper.Lerp(0.5f, 0.75f, AttackProgress01);
                Dust dust = Dust.NewDustDirect(corePosition + Main.rand.RandomPointInArea(10f, 10f), 0, 0, Main.rand.NextBool(3) ? DustID.Ice : DustID.BubbleBurst_Blue, 0f, 0f, 0, default, Scale: Main.rand.NextFloat(1.05f, 1.35f) * 0.9f * 1.25f);
                dust.noGravity = true;
                dust.fadeIn = 0.9f;
                dust.alpha = Main.rand.Next(50, 100);
                dust.velocity = velocity;
            }
            for (int i = 0; i < count; i++) {
                if (Main.rand.NextBool()) {
                    continue;
                }
                Vector2 size = new(24f, 24f);
                Rectangle r = Utils.CenteredRectangle(corePosition, size);
                Dust dust = Dust.NewDustDirect(r.TopLeft(), r.Width, r.Height, Main.rand.NextBool(3) ? DustID.Ice : DustID.BubbleBurst_Blue, 0f, 0f, 0, default, Scale: Main.rand.NextFloat(1.05f, 1.35f) * 0.9f * 1.5f);
                dust.noGravity = true;
                dust.fadeIn = 0.9f;
                dust.velocity = new Vector2(Main.rand.Next(-50, 51) * 0.05f, Main.rand.Next(-50, 51) * 0.05f);
                dust.alpha = Main.rand.Next(50, 100);
            }
        }
    }
}
