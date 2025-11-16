using Microsoft.Xna.Framework;

using RoA.Common.Druid;
using RoA.Common.Druid.Claws;
using RoA.Common.Networking;
using RoA.Common.Networking.Packets;
using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Core.Defaults;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;

using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Nature.Hardmode.Claws;

[WeaponOverlay(WeaponType.Claws)]
sealed class HandOfGod : ClawsBaseItem {
    public override bool IsHardmodeClaws => true;

    public override float BrightnessModifier => 1f;
    public override bool HasLighting => true;

    public override float FirstAttackSpeedModifier => 1.5f;
    public override float SecondAttackSpeedModifier => 0.9f;
    public override float ThirdAttackSpeedModifier => 0.9f;

    public override float FirstAttackScaleModifier => 1.5f;
    public override float SecondAttackScaleModifier => 1.1f;
    public override float ThirdAttackScaleModifier => 1.1f;

    protected override void SafeSetDefaults() {
        Item.SetSizeValues(34, 36);
        Item.SetWeaponValues(40, 4.2f);

        Item.rare = ItemRarityID.Pink;

        Item.value = Item.sellPrice(0, 2, 50, 0);

        Item.SetUsableValues(ItemUseStyleID.Swing, 18, false, autoReuse: true);

        NatureWeaponHandler.SetPotentialDamage(Item, 60);
        NatureWeaponHandler.SetFillingRateModifier(Item, 1f);
    }

    protected override void SetSpecialAttackData(Player player, ref ClawsHandler.AttackSpawnInfoArgs args) {
        ushort type = (ushort)ModContent.ProjectileType<GodFeather>();
        bool hasShieldAlready = player.HasProjectile<GodFeather>();
        args.ShouldSpawn = !hasShieldAlready;
        args.SpawnPosition = player.Center;
        args.ProjectileTypeToSpawn = type;
        args.ShouldReset = !hasShieldAlready;
    }

    protected override (Color, Color) SetSlashColors(Player player) 
        => (new Color(229, 214, 127), new Color(203, 179, 73));
}
