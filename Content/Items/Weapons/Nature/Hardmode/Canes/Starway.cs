using Microsoft.Xna.Framework;

using RoA.Common.Druid;
using RoA.Common.GlowMasks;
using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Core.Defaults;

using Terraria;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Nature.Hardmode.Canes;

[AutoloadGlowMask()]
sealed class Starway : CaneBaseItem<Starway.StarwayBase> {
    protected override void SafeSetDefaults() {
        Item.SetSizeValues(42, 46);
        Item.SetWeaponValues(200, 4f);
        Item.SetUsableValues(ItemUseStyleID.None, 30, useSound: SoundID.Item7);
        Item.SetShopValues(ItemRarityColor.StrongRed10, Item.sellPrice());

        NatureWeaponHandler.SetPotentialDamage(Item, 400);
        NatureWeaponHandler.SetFillingRateModifier(Item, 0.5f);

        Item.autoReuse = true;
    }

    protected override ushort ProjectileTypeToCreate() => (ushort)ModContent.ProjectileType<StarwayWormhole>();

    public sealed class StarwayBase : CaneBaseProjectile {
        protected override Vector2 CorePositionOffsetFactor() => new(0f, 0f);

        protected override bool ShouldWaitUntilProjDespawn() => false;

        protected override void SetSpawnProjectileSettings(Player player, ref Vector2 spawnPosition, ref Vector2 velocity, ref ushort count, ref float ai0, ref float ai1, ref float ai2) {

        }

        protected override void AfterProcessingCane() {

        }
    }
}
