using Microsoft.Xna.Framework;

using RoA.Common.Druid;
using RoA.Common.GlowMasks;
using RoA.Common.Players;
using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Core;
using RoA.Core.Data;
using RoA.Core.Defaults;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using System;
using System.Collections.Generic;
using System.Linq;

using Terraria;
using Terraria.Enums;
using Terraria.ID;

namespace RoA.Content.Items.Weapons.Nature.Hardmode.Canes;

[AutoloadGlowMask]
sealed class CavernCane : CaneBaseItem<CavernCane.CavernCaneBase> {
    protected override void SafeSetDefaults() {
        Item.SetSizeValues(42);
        Item.SetWeaponValues(60, 4f);
        Item.SetUsableValues(ItemUseStyleID.None, 30, useSound: SoundID.Item7);
        Item.SetShopValues(ItemRarityColor.LightRed4, Item.sellPrice());

        NatureWeaponHandler.SetPotentialDamage(Item, 100);
        NatureWeaponHandler.SetFillingRateModifier(Item, 0.5f);
    }

    protected override ushort TimeToCastAttack(Player player) => (ushort)(NatureWeaponHandler.GetUseSpeed(Item, player) * 4);

    public sealed class CavernCaneBase : CaneBaseProjectile {
        private static float GetCubicBezierEaseInForCavernCaneVisuals(float t, float control) => 3f * control * t * t * (1f - t) + t * t * t;

        private Color[]? _gemColor;

        private GemType GeodeType {
            get => (GemType)Projectile.ai[2];
            set => Projectile.ai[2] = Utils.Clamp((byte)value, (byte)GemType.Amethyst, (byte)(GemType.Amber + 1));
        }

        public override bool IsInUse => base.IsInUse && Owner.controlUseItem && AttackProgress01 < 1f;

        protected override bool ShouldShoot() => false;
        protected override bool ShouldPlayShootSound() => false;

        protected override ushort TimeAfterShootToExist(Player player) {
            byte result = (byte)(UseTime * 0.75f);
            float bezierControlValue = 0.7f;
            float attackProgress = GetCubicBezierEaseInForCavernCaneVisuals(AttackProgress01, bezierControlValue);
            float minPenaltyFactor = 0.3f,
                  maxPenaltyFactor = 0.65f;
            result = (byte)(result * (1f - Utils.Clamp(attackProgress, minPenaltyFactor, maxPenaltyFactor)));
            return result;
        }

        protected override void Initialize() { }

        protected override void AfterProcessingCane() {
            Player owner = Owner;
            float caneAttackProgress = AttackProgress01;
            Vector2 caneCorePosition = CorePosition;
            void shotRockProjectileOnSpawn() {
                void shootRocks() {
                    PlayAttackSound();
                    SpawnDustsOnShoot(owner, caneCorePosition);
                    if (owner.IsLocal()) {
                        GemType geodeType = Main.rand.GetRandomEnumValue<GemType>();
                        GeodeType = geodeType;
                        Vector2 spawnPosition = owner.Center;
                        int maxChecks = 40;
                        while (maxChecks-- > 0) {
                            spawnPosition += spawnPosition.DirectionTo(Owner.GetWorldMousePosition()) * WorldGenHelper.TILESIZE;
                            if (WorldGenHelper.SolidTileNoPlatform(spawnPosition.ToTileCoordinates())) {
                                break;
                            }
                        }
                        ProjectileUtils.SpawnPlayerOwnedProjectile<CavernCane_Rocks>(new ProjectileUtils.SpawnProjectileArgs(owner, Projectile.GetSource_Misc("caneattack")) {
                            Position = spawnPosition,
                            AI0 = (byte)geodeType,
                            AI1 = UseTime
                        });
                        Projectile.netUpdate = true;
                    }
                }
                float timeNeededToCastAnAttack = 0f/*0.33f*/;
                if (caneAttackProgress >= timeNeededToCastAnAttack && !ShotWhenEndedAttackAnimation) {
                    shootRocks();
                    ShotWhenEndedAttackAnimation = true;
                }
            }
            void releaseCaneWhenIsNotInUse() {
                if (!IsInUse) {
                    ReleaseCane();
                }
            }

            shotRockProjectileOnSpawn();
            releaseCaneWhenIsNotInUse();
        }

        protected override void SpawnCoreDustsBeforeShoot(float step, Player player, Vector2 corePosition) {
            if (!ShouldBeActive) {
                return;
            }

            float dustSpawnPositionOffsetFactor = 20f * -player.direction;
            float visualProgress = GetCubicBezierEaseInForCavernCaneVisuals(AttackProgress01, 1f);
            float visualProgress2 = 1f - MathF.Min(0.8f, AttackProgress01);
            float dustRotationSpeed = 8f - 3f * visualProgress;
            dustRotationSpeed *= player.direction;
            if (step > 0.025f) {
                byte circleCount = 2;
                _gemColor ??= new Color[circleCount];
                int value = 4;
                for (int k = 0; k < value; k++) {
                    for (int i = 1; i < circleCount; i++) {
                        byte nextCircleIndex = (byte)i;
                        float circleProgress = (float)nextCircleIndex / circleCount;
                        Vector2 circleSize = Vector2.UnitY * dustSpawnPositionOffsetFactor * Ease.CubeOut(circleProgress) * visualProgress2;
                        float dustAngle = step * MathHelper.Pi * dustRotationSpeed;
                        dustAngle += MathHelper.Pi * k;
                        Vector2 dustSpawnPosition = corePosition + circleSize.RotatedBy(dustAngle);
                        Func<float, float> func = k % 2 == 0 ? MathF.Sin : MathF.Cos;
                        dustSpawnPosition += circleSize.RotatedBy(func(1f - Utils.Clamp(AttackProgress01, 0.5f, 1f)) * MathHelper.Pi) * 0.5f;
                        List<GemType> gemTypes = [.. Enum.GetValues(typeof(GemType)).Cast<GemType>()];
                        int gemCount = gemTypes.Count;
                        ref Color gemColor = ref _gemColor[i];
                        gemColor = Color.Lerp(gemColor, CavernCane_Rocks.GetGeodeColor(GeodeType), 0.1f);
                        Color dustColor = gemColor * (1f - visualProgress);
                        int dustType = CavernCane_Rocks.GetGeodeDustType(GeodeType);
                        float dustScale = Main.rand.NextFloat(1.25f, 1.5f) * 1f * MathF.Max(0.6f, 1f - visualProgress2);
                        Vector2 dustVelocity = Vector2.Zero;
                        Dust dust = Dust.NewDustPerfect(dustSpawnPosition - Vector2.One * 1f,
                                                        dustType,
                                                        dustVelocity,
                                                        Scale: dustScale);
                        dust.noGravity = true;
                        //dust.color = gemColor;
                    }
                }
            }
        }

        protected override void SpawnDustsOnShoot(Player player, Vector2 corePosition) {

        }
    }
}
