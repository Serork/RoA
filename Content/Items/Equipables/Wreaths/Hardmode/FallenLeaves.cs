using Microsoft.Xna.Framework;

using RoA.Common.Druid.Wreath;
using RoA.Common.Items;
using RoA.Common.Players;
using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Core.Defaults;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.Enums;

namespace RoA.Content.Items.Equipables.Wreaths.Hardmode;

sealed class FallenLeaves : WreathItem, WreathItem.IWreathGlowMask {
    public static ushort ATTACKTIME => MathUtils.SecondsToFrames(20);

    Color IWreathGlowMask.GlowColor => Color.White;

    public override void Load() {
        ItemCommon.UseItemEvent += ItemCommon_UseItemEvent;
        PlayerCommon.PreItemCheckEvent += PlayerCommon_PreItemCheckEvent;
    }

    private void PlayerCommon_PreItemCheckEvent(Player player) {
        if (!player.GetCommon().IsFallenLeavesEffectActive) {
            return;
        }

        if (!player.GetCommon().CanSpawnFallenLeavesBranch) {
            return;
        }

        if (player.IsLocal() && !player.HasProjectile<FallenLeavesSprout>()) {
            int direction = -1;
            for (int i = 0; i < 2; i++) {
                Vector2 position = player.GetPlayerCorePoint(),
                        velocity = -Vector2.UnitY.RotatedBy(MathHelper.PiOver4 * Main.rand.NextFloat(0.5f, 1f) * direction);
                ProjectileUtils.SpawnPlayerOwnedProjectile<FallenLeavesSprout>(new ProjectileUtils.SpawnProjectileArgs(player, player.GetSource_FromThis()) {
                    Position = position,
                    Velocity = velocity
                });
                direction = 1;
            }
        }
    }

    private void ItemCommon_UseItemEvent(Item item, Player player) {
        if (!player.GetCommon().IsFallenLeavesEffectActive) {
            return;
        }

        if (!player.GetCommon().CanSpawnFallenLeavesBranch) {
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


        if (player.IsLocal() && player.ItemAnimationJustStarted) {
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
                    Vector2 position = player.GetPlayerCorePoint();
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
