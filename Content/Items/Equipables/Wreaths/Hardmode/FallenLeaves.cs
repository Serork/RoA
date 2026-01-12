using Microsoft.Xna.Framework;

using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Core.Defaults;
using RoA.Core.Utility;
using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.Enums;

namespace RoA.Content.Items.Equipables.Wreaths.Hardmode;

sealed class FallenLeaves : WreathItem, WreathItem.IWreathGlowMask {
    Color IWreathGlowMask.GlowColor => Color.White;

    protected override void SafeSetDefaults() {
        Item.SetSizeValues(38, 40);

        Item.SetShopValues(ItemRarityColor.Lime7, Item.buyPrice());
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        if (Main.mouseRight && Main.mouseRightRelease) {
            int countToMake = 2;
            bool direction = false;
            for (int i = 0; i < countToMake; i++) {
                Vector2 position = player.Center;
                ProjectileUtils.SpawnPlayerOwnedProjectile<FallenLeavesBranch>(new ProjectileUtils.SpawnProjectileArgs(player, player.GetSource_FromThis()) {
                    Position = position,
                    AI0 = direction.ToInt(),
                    AI1 = Main.rand.NextFloat(0.5f, 1.5f),
                    AI2 = 0f
                });
                direction = !direction;
            }
        }
    }
}
