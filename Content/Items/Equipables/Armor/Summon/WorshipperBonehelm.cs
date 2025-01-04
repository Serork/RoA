using Microsoft.Xna.Framework;

using RoA.Common.Players;
using RoA.Content.Buffs;
using RoA.Content.Projectiles.Friendly.Summon;
using RoA.Core.Utility;
using RoA.Utilities;

using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Armor.Summon;

[AutoloadEquip(EquipType.Head)]
sealed class WorshipperBonehelm : ModItem {
    internal class BoneHarpyOptions : ModPlayer, IDoubleTap {
        internal const float FLYTIME = 350f;

        private const int STATETIME = 600;

        internal bool IsInIdle = true;
        internal int HarpyThatRideWhoAmI = -1;

        internal int StateTimer;
        internal float FlyTime;

        public void OnDoubleTap(Player player, IDoubleTap.TapDirection direction) {
            if (player.HasBuff(ModContent.BuffType<BoneHarpyAttackDebuff>())) {
                return;
            }
            if (direction != IDoubleTap.TapDirection.Down) {
                return;
            }

            BoneHarpyOptions handler = player.GetModPlayer<BoneHarpyOptions>();
            if (handler.RodeHarpy) {
                return;
            }
            if (!handler.ShouldBeActive()) {
                return;
            }

            SoundEngine.PlaySound(SoundID.Item25, player.Center);

            handler.IsInIdle = !handler.IsInIdle;
            if (handler.StateTimer <= 0) {
                handler.StateTimer = STATETIME;
            }
        }

        public void RideHarpy(int harpyWhoAmI) {
            if (RodeHarpy) {
                return;
            }
            Projectile projectile = Main.projectile[harpyWhoAmI];
            if (!projectile.active) {
                return;
            }
            if (projectile.ModProjectile is not BoneHarpy) {
                return;
            }
            if (Player.mount.Active) {
                Player.mount.Dismount(Player);
            }
            HarpyThatRideWhoAmI = harpyWhoAmI;
            IsInIdle = true;
            Player.velocity = Vector2.Zero;
            if (StateTimer <= 0) {
                StateTimer = STATETIME;
            }
        }

        public void JumpOffHarpy() {
            if (!RodeHarpy) {
                return;
            }
            Player.ClearBuff(ModContent.BuffType<BoneHarpyMountBuff>());
            HarpyThatRideWhoAmI = -1;
            //AddCD();
        }

        public void ToggleState(int harpyWhoAmI = -1) {
            if (RodeHarpy) {
                JumpOffHarpy();
                return;
            }
            if (harpyWhoAmI != -1) {
                RideHarpy(harpyWhoAmI);
            }
        }

        public override void ResetEffects() {
            if (!ShouldBeActive()) {
                IsInIdle = true;
            }
        }

        private void AddCD() => Player.AddBuff(ModContent.BuffType<BoneHarpyAttackDebuff>(), 900);

        public bool RodeHarpy => !(HarpyThatRideWhoAmI == -1 || !Main.projectile[HarpyThatRideWhoAmI].active);

        public bool ShouldBeActive() => Player.HasSetBonusFrom<WorshipperBonehelm>();

        public override void PostUpdateBuffs() {
            if (StateTimer > 0) {
                StateTimer--;
            }
            else if (!IsInIdle) {
                IsInIdle = true;

                AddCD();
            }

            if (!RodeHarpy) {
                if (FlyTime < FLYTIME) {
                    FlyTime += 1f;
                }

                return;
            }

            Player.AddBuff(ModContent.BuffType<BoneHarpyMountBuff>(), 2);
        }
    }

    public override void SetStaticDefaults() {
        // DisplayName.SetDefault("Worshipper Bonehelm");
        // Tooltip.SetDefault("Increases your max number of minions by 1" + "\n8% increased minion damage");

        Item.ResearchUnlockCount = 1;
    }

    public override void SetDefaults() {
        int width = 28; int height = 22;
        Item.Size = new Vector2(width, height);

        Item.value = Item.sellPrice(silver: 75);
        Item.rare = ItemRarityID.Orange;
        Item.defense = 5;
    }

    public override void UpdateEquip(Player player) {
        player.GetDamage(DamageClass.Summon) += 0.08f;
        player.maxMinions += 1;
    }

    public override bool IsArmorSet(Item head, Item body, Item legs) => body.type == ModContent.ItemType<WorshipperMantle>() && legs.type == ModContent.ItemType<WorshipperGarb>();

    public override void UpdateArmorSet(Player player) {
        player.setBonus = Language.GetText("Mods.RoA.Items.Tooltips.WorshipperSetBonus").WithFormatArgs(Helper.ArmorSetBonusKey, Language.GetTextValue("Controls.RightClick")).Value;

        SummonHarpy(player);
    }

    private static void SummonHarpy(Player player) {
        int type = ModContent.ProjectileType<BoneHarpy>();
        if (player.ownedProjectileCounts[type] < 1) {
            Projectile.NewProjectile(player.GetSource_Misc("worshipperarmorset"), player.MountedCenter, Vector2.Zero, type,
                (int)player.GetTotalDamage(DamageClass.Summon).ApplyTo(30),
                player.GetTotalKnockback(DamageClass.Summon).ApplyTo(2.5f),
                player.whoAmI);
        }
    }
}