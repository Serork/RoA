using RoA.Common.Druid;
using RoA.Content.Projectiles.Friendly;
using RoA.Core;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Druidic.Rods;

sealed class TectonicCane : BaseRodItem<TectonicCane.TectonicCaneBase> {
    protected override ushort ShootType() => (ushort)ModContent.ProjectileType<TectonicCaneProjectile>();

    protected override void SafeSetDefaults() {
        Item.SetSize(36, 38);
        Item.SetDefaultToUsable(-1, 30, useSound: SoundID.Item7);
        Item.SetWeaponValues(5, 4f);

        NatureWeaponHandler.SetPotentialDamage(Item, 15);
        NatureWeaponHandler.SetFillingRate(Item, 0.35f);
        //NatureWeaponHandler.SetPotentialUseSpeed(Item, 20);
    }

    public sealed class TectonicCaneBase : BaseRodProjectile {
        protected override bool ShouldWaitUntilProjDespawns() => false;
    }
}

sealed class TectonicCaneProjectile : NatureProjectile {
    public override string Texture => ResourceManager.EmptyTexture;
}