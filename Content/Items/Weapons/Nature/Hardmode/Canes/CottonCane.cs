using Humanizer;

using Microsoft.Xna.Framework;

using RoA.Common.Druid;
using RoA.Common.Players;
using RoA.Common.VisualEffects;
using RoA.Content.AdvancedDusts;
using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Core.Defaults;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using System;
using System.Collections.Generic;

using Terraria;
using Terraria.Audio;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;

using static System.Net.Mime.MediaTypeNames;
using static tModPorter.ProgressUpdate;

namespace RoA.Content.Items.Weapons.Nature.Hardmode.Canes;

sealed class CottonCane : CaneBaseItem<CottonCane.CottonCaneBase> {
    protected override void SafeSetDefaults() {
        Item.SetSizeValues(38, 38);
        Item.SetWeaponValues(60, 4f);
        Item.SetUsableValues(ItemUseStyleID.None, 30, useSound: SoundID.Item7);
        Item.SetShopValues(ItemRarityColor.LightRed4, Item.sellPrice());

        NatureWeaponHandler.SetPotentialDamage(Item, 100);
        NatureWeaponHandler.SetFillingRateModifier(Item, 0.5f);

        Item.autoReuse = true;
    }

    protected override ushort ProjectileTypeToCreate() => (ushort)ModContent.ProjectileType<CottonBoll>();

    public sealed class CottonCaneBase : CaneBaseProjectile {
        private static ushort SMALLBOUNTCOUNT => 5;

        private bool[] _smallBollSpawned = null!;
        private List<float> _anglesTaken = null!;

        protected override void Initialize() {
            _smallBollSpawned = new bool[SMALLBOUNTCOUNT];
            _anglesTaken = [];
        }

        protected override bool ShouldWaitUntilProjDespawn() => false;

        protected override void SetSpawnProjectileSettings(Player player, ref Vector2 spawnPosition, ref Vector2 velocity, ref ushort count, ref float ai0, ref float ai1, ref float ai2) {
            spawnPosition = GetSpawnPosition(player);
            velocity = Owner.GetPlayerCorePoint().DirectionTo(spawnPosition) * 1f;
        }

        public static Vector2 GetSpawnPosition(Player player) {
            Vector2 spawnPosition = player.GetPlayerCorePoint();
            int maxChecks = 25;
            while (maxChecks-- > 0 && !WorldGenHelper.SolidTileNoPlatform(spawnPosition.ToTileCoordinates())) {
                spawnPosition += spawnPosition.DirectionTo(player.GetWorldMousePosition()) * WorldGenHelper.TILESIZE;
            }
            return spawnPosition;
        }

        protected override void SpawnCoreDustsBeforeShoot(float step, Player player, Vector2 corePosition) {
            Vector2 to = GetSpawnPosition(player);
            Vector2 to2 = to;
            Vector2 velocity = Owner.GetPlayerCorePoint().DirectionTo(to);

            if (Main.rand.NextBool(10)) {
                for (int i = 0; i < 1; i++) {
                    Vector2 position = corePosition + Main.rand.RandomPointInArea(10f) * 0.75f;
                    Vector2 velocity2 = -Vector2.UnitY * Main.rand.NextFloat(1f, 2f) + Vector2.UnitX * Main.rand.NextFloat(-1f, 1f);
                    velocity2.Y *= 0.5f;
                    Dust dust = Dust.NewDustPerfect(position, ModContent.DustType<Dusts.CottonDust>(), velocity2, Alpha: 25);
                    dust.scale = Main.rand.NextFloat(0.8f, 1.2f);
                    dust.scale *= 1.05f;
                    dust.alpha = Projectile.alpha;
                    dust.velocity += velocity * 0.5f;
                }
            }
        }

        protected override void AfterProcessingCane() {
            int index = 0;
            for (float i = 0f; i < 1f;) {
                i += 1f / SMALLBOUNTCOUNT;
                if (AttackProgress01 > i && !_smallBollSpawned[index]) {
                    _smallBollSpawned[index] = true;
                    Projectile.localAI[0] = 1f;

                    SoundEngine.PlaySound(SoundID.Item20 with { Pitch = 1.75f, Volume = 0.5f }, CorePosition);

                    Vector2 spawnPosition = CorePosition;
                    Vector2 velocity = Vector2.Zero;
                    ushort count = 1;
                    int damage = Projectile.damage;
                    float knockBack = Projectile.knockBack;
                    float ai0 = 0f, ai1 = 0f, ai2 = 0f;
                    SetSpawnProjectileSettings(Owner, ref spawnPosition, ref velocity, ref count, ref ai0, ref ai1, ref ai2);
                    SetSpawnProjectileSettings2(Owner, ref damage, ref knockBack);
                    if (Owner.IsLocal()) {
                        int tiles = 3;
                        float offset = TileHelper.TileSize * tiles;
                        float angle = MathHelper.TwoPi * Main.rand.NextFloat();
                        int attempts = 10;
                        while (attempts-- > 0) {
                            bool shouldBreak = true;
                            foreach (float checkAngle in _anglesTaken) {
                                if (MathF.Abs(checkAngle - angle) < 0.5f) {
                                    angle = MathHelper.TwoPi * Main.rand.NextFloat();
                                    shouldBreak = false;
                                }
                            }
                            if (shouldBreak) {
                                break;
                            }
                        }
                        spawnPosition += Vector2.One.RotatedBy(angle) * offset * Main.rand.NextFloat(0.5f, 0.75f);
                        velocity = Owner.GetPlayerCorePoint().DirectionTo(spawnPosition) * 1f;
                        _anglesTaken.Add(angle);
                        ProjectileUtils.SpawnPlayerOwnedProjectile<Projectiles.Friendly.Nature.CottonBollSmall>(new ProjectileUtils.SpawnProjectileArgs(Owner, Projectile.GetSource_FromThis()) {
                            Position = spawnPosition,
                            Velocity = velocity,
                            Damage = damage,
                            KnockBack = knockBack
                        });
                    }
                }
                index++;
            }

            if (Shot) {
                Projectile.localAI[2]++;
            }

            {
                if (Main.rand.NextChance(Projectile.localAI[2] / 10f)) {
                    return;
                }
                if (Main.rand.NextBool()) {
                    return;
                }
                Vector2 to = GetSpawnPosition(Owner);
                Vector2 to2 = to;
                Vector2 velocity = Owner.GetPlayerCorePoint().DirectionTo(to);
                to -= velocity * 20f;
                velocity = velocity.RotatedBy(MathHelper.PiOver2 * 0.75f * Main.rand.NextFloat(0.5f, 1f) * Main.rand.NextBool().ToDirectionInt());
                velocity *= Main.rand.NextFloat(2.5f, 5f);
                CottonDust2? cottonDust = AdvancedDustSystem.New<CottonDust2>(AdvancedDustLayer.ABOVEDUSTS)?
                    .Setup(to + Main.rand.RandomPointInArea(35f),
                           velocity,
                           scale: 1f);
                cottonDust?.CorePosition = to2;
            }
        }
    }
}
