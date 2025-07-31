using Microsoft.Xna.Framework;

using RoA.Common;
using RoA.Common.Druid;
using RoA.Common.Players;
using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Core.Defaults;

using System.Linq;

using Terraria;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Nature.Hardmode;

sealed class ClingerStaff : NatureItem {
    private static float SPAWNOFFSET => 100f;

    protected override void SafeSetDefaults() {
        Item.SetSizeValues(38, 38);
        Item.SetWeaponValues(40, 2f);
        Item.SetUsableValues(ItemUseStyleID.Swing, 40, useSound: SoundID.Item100);
        Item.SetShootableValues((ushort)ModContent.ProjectileType<ClingerHideway>());
        Item.SetShopValues(ItemRarityColor.Pink5, Item.sellPrice(gold: 8));

        NatureWeaponHandler.SetPotentialDamage(Item, 80);
        NatureWeaponHandler.SetFillingRateModifier(Item, 0.2f);
    }

    public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
        Vector2 getOffset() => Vector2.One.RotatedByRandom(MathHelper.TwoPi) * SPAWNOFFSET;
        Vector2 mousePosition = player.GetMousePosition();
        position = mousePosition + getOffset();
        foreach (Projectile projectile in TrackedEntitiesSystem.GetTrackedProjectile<ClingerHideway>((checkProjectile) => checkProjectile.owner != player.whoAmI)) {
            while (projectile.Center.Distance(position) < SPAWNOFFSET) {
                position = mousePosition + getOffset();
            }
        }
    }
}
