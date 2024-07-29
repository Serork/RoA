using Microsoft.Xna.Framework;
using RoA.Common.Druid.Claws;
using RoA.Content.Projectiles.Friendly.Druidic;
using RoA.Core;
using RoA.Utilities;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Druidic.Claws;

[WeaponOverlay(WeaponType.Claws)]
abstract class BaseClawsItem : NatureItem {
    protected sealed override void SafeSetDefaults2() {
        Item.noMelee = true;

        Item.shoot = ModContent.ProjectileType<ClawsSlash>();
        Item.shootSpeed = 1.2f;

        Item.SetDefaultToUsable(ItemUseStyleID.Swing, 18, 18, false, autoReuse: false);
    }

    protected abstract (Color, Color) SlashColors();

    public override bool? UseItem(Player player) {
        if (Main.netMode != NetmodeID.Server && player.ItemAnimationJustStarted) {
            (Color, Color) slashColors = SlashColors();
            player.GetModPlayer<ClawsStats>().SetColors(slashColors.Item1, slashColors.Item2);
        }

        return base.UseItem(player);
    }

    public sealed override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
        Vector2 point = Helper.VelocityToPoint(player.Center, Main.MouseWorld, 1f);
        position += point * 15f;
        velocity = point;
    }

    public sealed override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
        Projectile.NewProjectile(player.GetSource_ItemUse(Item), position, new Vector2(player.direction, 0f), type, damage, knockback, player.whoAmI, player.direction * player.gravDir, player.itemAnimationMax);
        NetMessage.SendData(MessageID.PlayerControls, number: player.whoAmI);
        return false;
    }
}
