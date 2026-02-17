using Microsoft.Xna.Framework;

using RoA.Common.Druid;
using RoA.Common.Players;
using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Core.Defaults;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;

using Terraria;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;

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
        protected override bool ShouldWaitUntilProjDespawn() => false;

        protected override void SetSpawnProjectileSettings(Player player, ref Vector2 spawnPosition, ref Vector2 velocity, ref ushort count, ref float ai0, ref float ai1, ref float ai2) {
            spawnPosition = GetSpawnPosition(player);
        }

        public static Vector2 GetSpawnPosition(Player player) {
            Vector2 spawnPosition = player.GetPlayerCorePoint();
            int maxChecks = 50;
            while (maxChecks-- > 0 && !WorldGenHelper.SolidTileNoPlatform(spawnPosition.ToTileCoordinates())) {
                spawnPosition += spawnPosition.DirectionTo(player.GetWorldMousePosition()) * WorldGenHelper.TILESIZE;
            }
            return spawnPosition;
        }
    }
}
