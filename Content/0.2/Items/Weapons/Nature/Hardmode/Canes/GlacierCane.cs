using Microsoft.Xna.Framework;

using RoA.Common.Druid;
using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Core.Defaults;
using RoA.Core.Utility;

using Terraria;
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
    }

    protected override ushort ProjectileTypeToCreate() => (ushort)ModContent.ProjectileType<GlacierSpike>();

    public sealed class GlacierCaneBase : CaneBaseProjectile {
        public override void SetStaticDefaults() {
            ProjectileID.Sets.NeedsUUID[Type] = true;
        }

        protected override void SetSpawnProjectileSettings(Player player, ref Vector2 spawnPosition, ref Vector2 velocity, ref ushort count, ref float ai0, ref float ai1, ref float ai2) {
            ai2 = Projectile.GetByUUID(Projectile.owner, Projectile.whoAmI);
        }

        protected override bool ShouldWaitUntilProjDespawn() => false;
        protected override bool ShouldShoot() => false;

        protected override void Initialize() {
            ShootProjectile();
        }

        protected override void AfterProcessingCane() {
            if (AttackProgress01 >= 1f) {
                ReleaseCane();
            }
        }

        protected override void SpawnCoreDustsBeforeShoot(float step, Player player, Vector2 corePosition) {
           
        }

        protected override void SpawnDustsOnShoot(Player player, Vector2 corePosition) {

        }
    }
}
