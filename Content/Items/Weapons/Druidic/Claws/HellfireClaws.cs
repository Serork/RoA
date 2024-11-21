using Microsoft.Xna.Framework;

using RoA.Common.Druid;
using RoA.Common.Druid.Claws;
using RoA.Content.Projectiles.Friendly.Druidic;
using RoA.Core;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Druidic.Claws;

[WeaponOverlay(WeaponType.Claws, 0xffffff)]
sealed class HellfireClaws : BaseClawsItem {
    public override Color? GetAlpha(Color lightColor) => Color.White;

    protected override void SafeSetDefaults() {
        Item.SetSize(26);
        Item.SetWeaponValues(16, 4.2f);
    }

    public override bool CanUseItem(Player player) => base.CanUseItem(player) && player.ownedProjectileCounts[ModContent.ProjectileType<HellfireClawsSlash>()] < 1;

    protected override (Color, Color) SlashColors(Player player) => (new Color(255, 150, 20), new Color(200, 80, 10));

    public override void SafeOnUse(Player player, ClawsHandler clawsStats) {
        clawsStats.SetSpecialAttackData(new ClawsHandler.AttackSpawnInfoArgs() {
            Owner = Item,
            SpawnProjectile = (Player player) => {
                Projectile.NewProjectile(player.GetSource_ItemUse(Item), player.Center, new Vector2(player.direction, 0f), ModContent.ProjectileType<HellfireClawsSlash>(),
                    player.GetWeaponDamage(Item), player.GetWeaponKnockback(Item), player.whoAmI, player.direction/* * player.gravDir*/, NatureWeaponHandler.GetUseSpeed(Item, player));
            }
        });
    }
}
