using Microsoft.Xna.Framework;

using RoA.Common.Druid;
using RoA.Common.Players;
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
        private bool[] _smallBollSpawned = null!;
        private List<float> _anglesTaken = null!;

        protected override void Initialize() {
            _smallBollSpawned = new bool[10];
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

        protected override void AfterProcessingCane() {
            int index = 0;
            for (float i = 0f; i < 1f;) {
                i += 0.1f;
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
                        spawnPosition += Vector2.One.RotatedBy(angle) * offset * Main.rand.NextFloat(0.5f, 1.25f);
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
        }
    }
}
