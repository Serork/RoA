using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Magic;

abstract class Rod : ShootingWeapon {
    protected virtual Color? LightingColor { get; } = null;

    public override void SetDefaults() {
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.noMelee = true;
        Item.DamageType = DamageClass.Magic;

        Item.staff[Type] = true;
    }

    public override void UseStyle(Player player, Rectangle heldItemFrame) {
        if (!Main.dedServ && LightingColor != null) {
            Lighting.AddLight(player.itemLocation + Utils.SafeNormalize(player.itemLocation.DirectionFrom(player.Center), Vector2.Zero) * Item.width, LightingColor.Value.ToVector3() * 0.75f);
        }
    }

    //public override bool? UseItem(Player player) {
    //    if (player.whoAmI == Main.myPlayer) {
    //        if (!Main.dedServ && LightingColor != null) {
    //            Lighting.AddLight(player.itemLocation, LightingColor.Value.ToVector3());
    //        }
    //    }

    //    return base.UseItem(player);
    //}
}