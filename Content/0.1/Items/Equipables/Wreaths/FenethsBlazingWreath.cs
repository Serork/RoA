using Microsoft.Xna.Framework;

using RoA.Common.Druid.Wreath;
using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Core.Utility;
using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Wreaths;

sealed class FenethsBlazingWreath : WreathItem, WreathItem.IWreathGlowMask {
    Color IWreathGlowMask.GlowColor => Color.White;

    protected override void SafeSetDefaults() {
        int width = 30; int height = 30;
        Item.Size = new Vector2(width, height);

        Item.maxStack = 1;
        Item.rare = ItemRarityID.Orange;

        Item.value = Item.sellPrice(0, 0, 75, 0);
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        WreathHandler handler = player.GetWreathHandler();
        if (handler.IsFull1) {
            player.GetKnockback(DruidClass.Nature) += 0.5f;
        }
        player.GetModPlayer<FenethsBlazingWreathHandler>().IsEffectActive = true;
    }

    internal class FenethsBlazingWreathHandler : ModPlayer {
        public bool IsEffectActive;

        public override void ResetEffects() {
            IsEffectActive = false;
        }

        public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers) {
            if (target.immortal) {
                return;
            }

            if (!proj.IsNature()) {
                return;
            }

            int type = ModContent.ProjectileType<Fireblossom>();
            if (proj.type != ModContent.ProjectileType<FireblossomExplosion>() && proj.type != ModContent.ProjectileType<Fireblossom>() &&
                IsEffectActive && Main.rand.NextChance(0.25 * Player.GetWreathHandler().ActualProgress4) && target.FindBuffIndex(ModContent.BuffType<Buffs.Fireblossom>()) == -1 &&
                Player.ownedProjectileCounts[type] < 10) {
                Vector2 center = proj.Center;
                if (center.Distance(Player.Center) < 100f || center.Distance(Player.RotatedRelativePoint(Player.MountedCenter, true)) < 100f) {
                    center = target.Center + (Player.Center - target.Center).SafeNormalize(Vector2.Zero) * target.width / 2f;
                }
                int projectile = Projectile.NewProjectile(target.GetSource_OnHit(target), target.Center, Vector2.Zero, type, (int)Player.GetTotalDamage(DruidClass.Nature).ApplyTo(10) + proj.damage / 2, proj.knockBack,
                    Player.whoAmI, target.whoAmI, center.X, center.Y);

                center = Player.Center + (target.Center - Player.Center).SafeNormalize(Vector2.Zero) * Player.width;
                projectile = Projectile.NewProjectile(target.GetSource_OnHit(target), target.Center, Vector2.Zero, type, (int)Player.GetTotalDamage(DruidClass.Nature).ApplyTo(10) + + proj.damage / 2, proj.knockBack,
                    Player.whoAmI, Player.whoAmI, center.X, center.Y);
            }
        }

        public override void ModifyHitNPCWithItem(Item item, NPC target, ref NPC.HitModifiers modifiers) {
            if (target.immortal) {
                return;
            }

            if (!item.IsANatureWeapon()) {
                return;
            }

            int type = ModContent.ProjectileType<Fireblossom>();
            if (IsEffectActive && Main.rand.NextChance(0.25 * Player.GetWreathHandler().ActualProgress4) && target.FindBuffIndex(ModContent.BuffType<Buffs.Fireblossom>()) == -1 &&
                Player.ownedProjectileCounts[type] < 10) {
                int projectile = Projectile.NewProjectile(target.GetSource_OnHit(target), target.Center, Vector2.Zero, type, (int)Player.GetTotalDamage(DruidClass.Nature).ApplyTo(10) + + item.damage / 2, item.knockBack,
                    Player.whoAmI, target.whoAmI, Player.itemLocation.X, Player.itemLocation.Y);

                Vector2 center = Player.Center + (target.Center - Player.Center).SafeNormalize(Vector2.Zero) * Player.width;
                projectile = Projectile.NewProjectile(target.GetSource_OnHit(target), target.Center, Vector2.Zero, type, (int)Player.GetTotalDamage(DruidClass.Nature).ApplyTo(10) + + item.damage / 2, item.knockBack,
                    Player.whoAmI, Player.whoAmI, center.X, center.Y);
            }
        }
    }
}
