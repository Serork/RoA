using Microsoft.Xna.Framework;

using RoA.Common.Druid;
using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Core.Defaults;
using RoA.Core.Utility;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Nature.PreHardmode;

sealed class MushroomStaff : NatureItem {
    protected override void SafeSetDefaults() {
        Item.SetSizeValues(38);
        Item.SetUsableValues(ItemUseStyleID.Swing, 55, useSound: SoundID.Item156);
        Item.SetWeaponValues(3, 2f);
        Item.SetShootableValues((ushort)ModContent.ProjectileType<MushroomSpore>(), 4f);
        Item.SetOtherValues(Item.sellPrice(silver: 10), ItemRarityID.Blue);

        NatureWeaponHandler.SetPotentialDamage(Item, 16);
        NatureWeaponHandler.SetFillingRateModifier(Item, 0.2f);

        Item.value = Item.sellPrice(0, 0, 20, 0);
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
        int count = 3;
        for (int i = 0; i < count; i++) {
            Vector2 newVelocity = new Vector2(velocity.X, velocity.Y).RotatedByRandom(MathHelper.ToRadians(30)) * 1.6f;
            Projectile.NewProjectile(source, position.X + Main.rand.NextFloatRange(0.05f), position.Y, newVelocity.X, newVelocity.Y, type, damage, knockback, player.whoAmI);
        }

        return false;
    }

    public override Vector2? HoldoutOffset() => new Vector2(-18f, -4f);
}