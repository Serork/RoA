using Microsoft.Xna.Framework;

using RoA.Content.Projectiles.Friendly.Ranged;
using RoA.Core.Defaults;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Ranged;

sealed class MarineMulcher : RangedWeaponWithCustomAmmo {
    protected override BaseMaxAmmoAmount MaxAmmoAmount => BaseMaxAmmoAmount.Two;

    protected override void SafeSetDefaults() {
        Item.SetSizeValues(48, 24);
        Item.DefaultToRangedWeapon(ModContent.ProjectileType<MarineMulcherBomb>(), AmmoID.None, 30, 7f);
        Item.knockBack = 6.5f;
        Item.UseSound = SoundID.Item36;
        Item.damage = 14;
        Item.value = Item.buyPrice(0, 35);
        Item.rare = ItemRarityID.Orange;
    }

    public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
        position -= velocity.TurnLeft().SafeNormalize() * 6f * -player.direction * player.gravDir;
        velocity = position.DirectionTo(player.GetViableMousePosition()) * velocity.Length();
    }

    public override Vector2? HoldoutOffset() => new Vector2(-4f, 0f);
}
