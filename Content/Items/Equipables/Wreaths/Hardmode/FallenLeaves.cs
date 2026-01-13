using Microsoft.Xna.Framework;

using RoA.Common.Druid.Wreath;
using RoA.Common.Items;
using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Core.Defaults;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.Enums;

namespace RoA.Content.Items.Equipables.Wreaths.Hardmode;

sealed class FallenLeaves : WreathItem, WreathItem.IWreathGlowMask {
    public static ushort ATTACKTIME => MathUtils.SecondsToFrames(10);

    Color IWreathGlowMask.GlowColor => Color.White;

    public override void Load() {
        ItemCommon.UseItemEvent += ItemCommon_UseItemEvent;
    }

    private void ItemCommon_UseItemEvent(Item item, Player player) {
        if (!player.IsLocal()) {
            return;
        }

        if (!player.GetCommon().IsFallenLeavesEffectActive) {
            return;
        }

        if (!player.GetCommon().CanSpawnFallenLeavesBranch) {
            return;
        }

        if (!player.ItemAnimationJustStarted) {
            return;
        }

        if (!item.IsANatureWeapon()) {
            return;
        }

        //if (player.HasProjectile<FallenLeavesBranch>()) {
        //    return;
        //}

        //float chance = 1f * player.GetWreathHandler().ActualProgress4;
        //if (Main.rand.NextChance(chance))

        int damage = 75;
        if (Main.masterMode) {
            damage *= 3;
        }
        else if (Main.expertMode) {
            damage *= 2;
        }
        float knockBack = 7f;

        int denom = 1;
        damage /= denom;
        knockBack /= denom;

        {
            int countToMake = 2;
            bool direction = false;
            bool wreathIsFull = WreathHandler.IsWreathCharged(player);
            for (int i = 0; i < countToMake; i++) {
                Vector2 position = player.Center;
                ProjectileUtils.SpawnPlayerOwnedProjectile<FallenLeavesBranch>(new ProjectileUtils.SpawnProjectileArgs(player, player.GetSource_FromThis()) {
                    Position = position,
                    AI0 = direction.ToInt(),
                    AI1 = Main.rand.NextFloat(0.5f, 1.5f),
                    AI2 = wreathIsFull.ToInt(),
                    Damage = damage,
                    KnockBack = knockBack
                });
                direction = !direction;
            }
        }

        player.GetCommon().FallenLeavesCounter = 0f;
    }

    protected override void SafeSetDefaults() {
        Item.SetSizeValues(38, 40);

        Item.SetShopValues(ItemRarityColor.Lime7, Item.buyPrice());
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        player.GetCommon().IsFallenLeavesEffectActive = true;
    }
}
