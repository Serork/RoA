using Microsoft.Xna.Framework;

using RoA.Common.Druid;
using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Core.Defaults;
using RoA.Core.Utility;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Nature.PreHardmode.Canes;

sealed class FungalCane : CaneBaseItem<FungalCane.FungalCaneBase> {
    protected override ushort ProjectileTypeToCreate() => (ushort)ModContent.ProjectileType<FungalCaneMushroom>();

    protected override void SafeSetDefaults() {
        Item.SetSizeValues(38, 40);
        Item.SetUsableValues(-1, 40, useSound: SoundID.Item7);
        Item.SetWeaponValues(12, 4f);

        NatureWeaponHandler.SetPotentialDamage(Item, 30);
        NatureWeaponHandler.SetFillingRateModifier(Item, 0.15f);

        Item.rare = ItemRarityID.Blue;
        Item.value = Item.sellPrice(0, 0, 1, 0);
        //NatureWeaponHandler.SetPotentialUseSpeed(Item, 20);
    }

    public sealed class FungalCaneBase : CaneBaseProjectile {
        protected override bool ShouldWaitUntilProjDespawn() => false;

        protected override void SpawnDustsOnShoot(Player player, Vector2 corePosition) {

        }

        protected override void SpawnCoreDustsBeforeShoot(float step, Player player, Vector2 corePosition) {

        }
    }
}
