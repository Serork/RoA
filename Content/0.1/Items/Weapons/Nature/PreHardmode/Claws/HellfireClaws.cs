using Microsoft.Xna.Framework;

using RoA.Common.Druid;
using RoA.Common.Druid.Claws;
using RoA.Common.Druid.Wreath;
using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Core.Defaults;
using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Nature.PreHardmode.Claws;

[WeaponOverlay(WeaponType.Claws, 0xffffff)]
sealed class HellfireClaws : ClawsBaseItem {
    public override Color? GetAlpha(Color lightColor) => Color.White;

    public override float BrightnessModifier => 1f;
    public override bool HasLighting => true;

    protected override void SafeSetDefaults() {
        Item.SetSizeValues(26);
        Item.SetWeaponValues(30, 4.2f);

        Item.rare = ItemRarityID.Orange;

        Item.value = Item.sellPrice(0, 2, 50, 0);

        Item.SetUsableValues(ItemUseStyleID.Swing, 18, false, autoReuse: true);

        NatureWeaponHandler.SetPotentialDamage(Item, 34);
        NatureWeaponHandler.SetFillingRateModifier(Item, 1f);
    }

    public override bool CanUseItem(Player player) => base.CanUseItem(player) && player.ownedProjectileCounts[ModContent.ProjectileType<HellfireClawsSlash>()] < 1;

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
        WreathHandler handler = player.GetWreathHandler();
        bool flag = handler.WillBeFull(handler.CurrentResource, true);
        Projectile.NewProjectile(player.GetSource_ItemUse(Item), position, new Vector2(player.direction, 0f), flag ? ModContent.ProjectileType<HellfireClawsSlash>() : type, damage, knockback, player.whoAmI, player.direction, NatureWeaponHandler.GetUseSpeed(Item, player));

        return false;
    }

    protected override (Color, Color) SlashColors(Player player) => (new Color(255, 150, 20), new Color(200, 80, 10));
}
