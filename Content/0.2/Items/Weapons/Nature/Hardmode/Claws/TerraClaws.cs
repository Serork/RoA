using Microsoft.Xna.Framework;

using RoA.Common.Druid;
using RoA.Common.Druid.Claws;
using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Core.Defaults;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Nature.Hardmode.Claws;

[WeaponOverlay(WeaponType.Claws)]
sealed class TerraClaws : ClawsBaseItem {
    public override bool IsHardmodeClaws => true;

    public override float BrightnessModifier => 1f;
    public override bool HasLighting => true;

    public override float FirstAttackSpeedModifier => 0.75f;
    public override float SecondAttackSpeedModifier => 0.75f;
    public override float ThirdAttackSpeedModifier => 0.75f;

    public override float FirstAttackScaleModifier => 1.25f;
    public override float SecondAttackScaleModifier => 1.35f;
    public override float ThirdAttackScaleModifier => 1.45f;

    protected override void SafeSetDefaults() {
        Item.SetSizeValues(38, 36);
        Item.SetWeaponValues(40, 4.2f);

        Item.rare = ItemRarityID.Yellow;

        Item.value = Item.sellPrice(0, 2, 50, 0);

        Item.SetUsableValues(ItemUseStyleID.Swing, 18, false, autoReuse: true);

        NatureWeaponHandler.SetPotentialDamage(Item, 60);
        NatureWeaponHandler.SetFillingRateModifier(Item, 1f);
    }

    protected override void SetSpecialAttackData(Player player, ref ClawsHandler.AttackSpawnInfoArgs args) {
        args.ShouldSpawn = false;
    }

    protected override (Color, Color) SetSlashColors(Player player)
        => (new Color(47, 239, 102), new Color(181, 230, 29));

    protected override void SpawnClawsSlash(Player player, Vector2 position, int type, int damage, float knockback, int attackTime) {
        Projectile.NewProjectile(player.GetSource_ItemUse(Item), position, new Vector2(player.direction, 0f), SpawnClawsProjectileType(player) ?? type, damage, knockback, player.whoAmI, 
            player.direction, attackTime);
        Projectile.NewProjectile(player.GetSource_ItemUse(Item), position, new Vector2(player.direction, 0f), SpawnClawsProjectileType(player) ?? type, damage, knockback, player.whoAmI,
            player.direction, attackTime, ai2: 30f);
    }
}
