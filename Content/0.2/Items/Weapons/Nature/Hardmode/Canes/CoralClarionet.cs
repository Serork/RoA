using Microsoft.Xna.Framework;

using RoA.Common.Druid;
using RoA.Common.Players;
using RoA.Content.Dusts;
using RoA.Core;
using RoA.Core.Defaults;
using RoA.Core.Utility;

using System;

using Terraria;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Nature.Hardmode.Canes;

sealed class CoralClarionet : CaneBaseItem<CoralClarionet.CoralClarionetBase> {
    protected override void SafeSetDefaults() {
        Item.SetSizeValues(34, 40);
        Item.SetWeaponValues(60, 4f);
        Item.SetUsableValues(ItemUseStyleID.None, 30, useSound: SoundID.Item7);
        Item.SetShopValues(ItemRarityColor.LightRed4, Item.sellPrice());

        NatureWeaponHandler.SetPotentialDamage(Item, 100);
        NatureWeaponHandler.SetFillingRateModifier(Item, 0.5f);

        Item.autoReuse = true;
    }

    protected override ushort ProjectileTypeToCreate() => (ushort)ModContent.ProjectileType<Projectiles.Friendly.Nature.CoralClarionet>();

    public sealed class CoralClarionetBase : CaneBaseProjectile {
        protected override bool ShouldWaitUntilProjDespawn() => false;

        protected override void SetSpawnProjectileSettings(Player player, ref Vector2 spawnPosition, ref Vector2 velocity, ref ushort count, ref float ai0, ref float ai1, ref float ai2) {
            spawnPosition = player.Center;
            int maxChecks = 50;
            while (maxChecks-- > 0 && !WorldGenHelper.SolidTileNoPlatform(spawnPosition.ToTileCoordinates())) {
                spawnPosition += spawnPosition.DirectionTo(Owner.GetWorldMousePosition()) * WorldGenHelper.TILESIZE;
            }
        }

        protected override Vector2 CorePositionOffsetFactor() => new(0.3f, 0.025f);

        protected override void SpawnCoreDustsBeforeShoot(float step, Player player, Vector2 corePosition) {
            float baseStep = step;
            step = Ease.CubeIn(step);
            int num548 = 2;
            int width = 25;
            int height = 25;
            width = (int)(width * MathF.Max(0.5f, step));
            height = (int)(height * MathF.Max(0.5f, step));
            for (int i = 0; i < 2; i++) {
                for (int num549 = 0; num549 < num548; num549++) {
                    if (Main.rand.NextChance(0.5f - Utils.Remap(baseStep, 0f, 1f, 0f, 0.5f))) {
                        continue;
                    }
                    if (Main.rand.NextChance(Utils.Remap(baseStep, 0f, 1f, 0.5f, 0f))) {
                        continue;
                    }
                    if (Main.rand.NextBool()) {
                        continue;
                    }
                    Vector2 velocity = Vector2.One.RotatedByRandom(MathHelper.TwoPi);
                    Vector2 spinningpoint = Vector2.Normalize(velocity) * new Vector2((float)width / 2f, height) * 0.75f;
                    spinningpoint = spinningpoint.RotatedBy((double)(num549 - (num548 / 2 - 1)) * Math.PI / (double)num548) + corePosition;
                    Vector2 vector42 = ((float)(Main.rand.NextDouble() * 3.1415927410125732) - (float)Math.PI / 2f).ToRotationVector2();
                    Vector2 circleSize = Vector2.UnitY * 10f;
                    Vector2 vector422 = vector42;
                    vector42 += circleSize.RotatedBy(((i == 0 ? (1f - AttackProgress01) : AttackProgress01) * MathHelper.Pi + MathHelper.PiOver4) * -player.direction)
                        .RotatedBy(corePosition.DirectionTo(player.GetWorldMousePosition()).ToRotation() + MathHelper.Pi + (player.direction > 0).ToInt() * MathHelper.Pi) * 1f;
                    int num550 = Dust.NewDust(spinningpoint + vector422 + Vector2.UnitY * 3f - new Vector2(width / 4f, height / 4f),
                        0, 0, Main.rand.NextBool() ? DustID.Water : ModContent.DustType<Water>(), vector42.X * 2f, vector42.Y * 2f,
                        Main.rand.Next(100, 200), default(Color), 1f + 0.4f * Main.rand.NextFloat());
                    Main.dust[num550].noGravity = Main.rand.NextBool();
                    Main.dust[num550].noLight = true;
                    Dust dust2 = Main.dust[num550];
                    dust2.velocity /= 4f;
                    dust2 = Main.dust[num550];
                    dust2.velocity -= velocity;
                    dust2.velocity *= Main.rand.NextFloat(0.75f, 1f);
                    dust2.velocity *= baseStep;
                }
            }
        }

        protected override void SpawnDustsOnShoot(Player player, Vector2 corePosition) {

        }
    }
}
