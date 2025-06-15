using Microsoft.Xna.Framework;

using RoA.Common.Druid;
using RoA.Common.Players;
using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Core.Defaults;
using RoA.Core.Utility;

using Terraria;
using Terraria.Enums;
using Terraria.ID;

namespace RoA.Content.Items.Weapons.Nature.Hardmode.Canes;

sealed class CavernCane : CaneBaseItem<CavernCane.CavernCaneBase> {
    protected override void SafeSetDefaults() {
        Item.SetSizeValues(42);
        Item.SetWeaponValues(60, 4f);
        Item.SetUsageValues(ItemUseStyleID.HiddenAnimation, 30, useSound: SoundID.Item7);
        Item.SetShopValues(ItemRarityColor.LightRed4, Item.sellPrice());

        NatureWeaponHandler.SetPotentialDamage(Item, 100);
        NatureWeaponHandler.SetFillingRateModifier(Item, 0.5f);
    }

    protected override ushort GetUseTime(Player player) => (ushort)(NatureWeaponHandler.GetUseSpeed(Item, player) * 4);

    public sealed class CavernCaneBase : CaneBaseProjectile {
        public override bool IsInUse => base.IsInUse && Owner.controlUseItem && AttackProgress01 < 1f;

        protected override bool ShouldShoot() => false;
        protected override bool ShouldPlayShootSound() => false;

        protected override ushort TimeAfterShootToExist(Player player) {
            byte result = (byte)(UseTime * 0.75f);
            float cubicBezierEaseIn(float t, float control) => 3f * control * t * t * (1f - t) + t * t * t;
            float attackProgress = cubicBezierEaseIn(AttackProgress01, 0.7f);
            result = (byte)(result * (1f - Utils.Clamp(attackProgress, 0.3f, 0.8f)));
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
                        ProjectileHelper.SpawnPlayerOwnedProjectile<Rocks>(new ProjectileHelper.SpawnProjectileArgs(owner, Projectile.GetSource_Misc("caneattack")) {
                            Position = Owner.GetMousePosition(),
                            AI1 = UseTime
                        });
                    }
                }
                float timeNeededToCastAnAttack = 0f/*0.33f*/;
                if (caneAttackProgress >= timeNeededToCastAnAttack && !Shot2) {
                    shootRocks();
                    Shot2 = true;
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
    }
}
