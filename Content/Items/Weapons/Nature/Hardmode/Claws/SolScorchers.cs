using Microsoft.Xna.Framework;

using RoA.Common.Druid;
using RoA.Common.Druid.Claws;
using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Core.Defaults;
using RoA.Core.Utility;

using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace RoA.Content.Items.Weapons.Nature.Hardmode.Claws;

[WeaponOverlay(WeaponType.Claws)]
sealed class SolScorchers : ClawsBaseItem {
    public override bool IsHardmodeClaws => true;

    public override float BrightnessModifier => 1f;
    public override bool HasLighting => true;
    public override float HitEffectOpacity => 0.75f;

    public override float FirstAttackSpeedModifier => 0.75f;
    public override float SecondAttackSpeedModifier => 0.75f;
    public override float ThirdAttackSpeedModifier => 1f;

    public override float FirstAttackScaleModifier => 1f;
    public override float SecondAttackScaleModifier => 1.375f;
    public override float ThirdAttackScaleModifier => 1.75f;

    protected override void SafeSetDefaults() {
        Item.SetSizeValues(34, 36);
        Item.SetWeaponValues(65, 4.2f);

        Item.rare = ItemRarityID.Lime;

        Item.value = Item.sellPrice(0, 2, 50, 0);

        Item.SetUsableValues(ItemUseStyleID.Swing, 18, false, autoReuse: true);

        NatureWeaponHandler.SetPotentialDamage(Item, 70);
        NatureWeaponHandler.SetFillingRateModifier(Item, 1f);
    }

    public override bool ResetOnHit => false;

    protected override (Color, Color) SetSlashColors(Player player) => (new Color(255, 221, 71), new Color(175, 152, 49));

    public override void SafeOnUse(Player player, ClawsHandler clawsStats) {
        clawsStats.SetSpecialAttackData<SunSigil>(new ClawsHandler.AttackSpawnInfoArgs() {
            Owner = Item
        });
    }
}
