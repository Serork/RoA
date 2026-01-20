using Microsoft.Xna.Framework;

using RoA.Common.Druid;
using RoA.Common.Items;
using RoA.Content.Projectiles.Friendly.Miscellaneous;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.Enums;
using Terraria.ModLoader;

using static RoA.Content.Items.Equipables.Accessories.RagingBoots;

namespace RoA.Content.Items.Equipables.Accessories.Hardmode;

sealed class SandalwoodStompers : NatureItem {
    protected override void SafeSetDefaults() {
        Item.damage = 34;
        Item.knockBack = 5f;
        Item.defense = 2;

        Item.DefaultToAccessory(34, 30);

        NatureWeaponHandler.SetPotentialDamage(Item, 45);
        NatureWeaponHandler.SetFillingRateModifier(Item, 0.25f);

        Item.SetShopValues(ItemRarityColor.LightRed4, Item.sellPrice(0, 1));
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        var handler = player.GetModPlayer<RagingBootsAttackHandler>();
        if (!handler.IsEffectActive) {
            handler.Boots = Item;
        }
        handler.IsEffectActive = true;

        player.moveSpeed += 0.05f;
        player.runAcceleration += 0.05f;

        if ((player.gravDir == 1 && player.velocity.Y > 0) || (player.gravDir == -1 && player.velocity.Y < 0)) {
            player.gravity *= 1.5f;
        }

        ushort attackTime = MathUtils.SecondsToFrames(1.5f);
        if (player.GetCommon().StandingStillTimer < 1) {
            return;
        }

        if (player.HasProjectile<SeedOfWisdomRoot>()) {
            return;
        }

        if (!player.IsLocal()) {
            return;
        }

        Vector2 position = player.Bottom;
        ProjectileUtils.SpawnPlayerOwnedProjectile<SeedOfWisdomRoot>(new ProjectileUtils.SpawnProjectileArgs(player, player.GetSource_Accessory(Item)) {
            Position = position,
            AI0 = 1f,
            Damage = NatureWeaponHandler.GetNatureDamage(Item, player),
            KnockBack = player.GetTotalKnockback(DruidClass.Nature).ApplyTo(Item.knockBack)
        });
    }
}
