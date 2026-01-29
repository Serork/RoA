using Microsoft.Xna.Framework;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Magic;

abstract class Rod : ModItem {
    protected virtual Color? LightingColor { get; } = null;

    public override void SetStaticDefaults() {
    }

    public override void SetDefaults() {
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.noMelee = true;
        Item.DamageType = DamageClass.Magic;

        Item.staff[Item.type] = true;
    }

    public override void UseStyle(Player player, Rectangle heldItemFrame) {
        if (!Main.dedServ && LightingColor != null) {
            Lighting.AddLight(player.itemLocation + Utils.SafeNormalize(player.itemLocation.DirectionFrom(player.Center), Vector2.Zero) * Item.width, LightingColor.Value.ToVector3() * 0.75f);
        }
    }

    public override void PostUpdate() {
        if (!Main.dedServ && LightingColor != null) {
            Lighting.AddLight(Item.getRect().TopRight(), LightingColor.Value.ToVector3() * 0.75f);
        }
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
        return Collision.CanHit(player.Center, 0, 0, position + Vector2.Normalize(velocity) * (Item.height - 10f) / 2f, 0, 0);
    }

    public sealed override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
        Vector2 newVelocity = new Vector2(velocity.X, velocity.Y).SafeNormalize(Vector2.Zero);
        position += newVelocity * 50f;
        ModifyShootCustom(player, ref position, ref velocity, ref type, ref damage, ref knockback);
    }

    public virtual void ModifyShootCustom(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) { }

    public override Vector2? HoldoutOrigin() => Vector2.UnitY * 4f;

    //public override bool? UseItem(Player player) {
    //    if (player.whoAmI == Main.myPlayer) {
    //        if (!Main.dedServ && LightingColor != null) {
    //            Lighting.AddLight(player.itemLocation, LightingColor.Value.ToVector3());
    //        }
    //    }

    //    return base.UseItem(player);
    //}
}