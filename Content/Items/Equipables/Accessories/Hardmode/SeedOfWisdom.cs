using Microsoft.Xna.Framework;

using RoA.Content.Projectiles.Friendly.Miscellaneous;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.Enums;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Accessories.Hardmode;

sealed class SeedOfWisdom : ModItem {
    public override void SetDefaults() {
        Item.DefaultToAccessory(28, 34);

        Item.SetShopValues(ItemRarityColor.LightRed4, Item.sellPrice(0, 1));
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        ushort attackTime = MathUtils.SecondsToFrames(0.25f);
        if (player.GetCommon().StandingStillTimer < attackTime) {
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
            Position = position
        });
    }
}
