using Microsoft.Xna.Framework;

using RoA.Common.Druid.Wreath;
using RoA.Content.Projectiles.Friendly.Druidic;
using RoA.Core.Utility;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Wreaths;

sealed class FenethsBlazingWreath : BaseWreathItem {
	protected override void SafeSetDefaults() {
		int width = 30; int height = 30;
		Item.Size = new Vector2(width, height);

		Item.maxStack = 1;
		Item.value = Item.sellPrice(gold: 2);
		Item.rare = ItemRarityID.Orange;
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        WreathHandler handler = player.GetModPlayer<WreathHandler>();
        player.GetKnockback(DruidClass.NatureDamage) += 0.5f;
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

            if (Player.whoAmI != Main.myPlayer) {
                return;
            }

            if (!proj.IsDruidic()) {
                return;
            }

            if (proj.type != ModContent.ProjectileType<FireblossomExplosion>() && proj.type != ModContent.ProjectileType<Fireblossom>() &&
                IsEffectActive && Main.rand.NextChance(0.2) && target.FindBuffIndex(ModContent.BuffType<Buffs.Fireblossom>()) == -1) {
                Vector2 center = proj.Center;
                if (center.Distance(Player.Center) < 100f || center.Distance(Player.RotatedRelativePoint(Player.MountedCenter, true)) < 100f) {
                    center = target.Center + (Player.Center - target.Center).SafeNormalize(Vector2.Zero) * target.width / 2f;
                }
                int type = ModContent.ProjectileType<Fireblossom>();
                int projectile = Projectile.NewProjectile(target.GetSource_OnHit(target), target.Center, Vector2.Zero, type, proj.damage * 2, proj.knockBack, Player.whoAmI, target.whoAmI);
                Main.projectile[projectile].As<Fireblossom>().SetPosition(center, target);
                
                projectile = Projectile.NewProjectile(target.GetSource_OnHit(target), target.Center, Vector2.Zero, type, proj.damage * 2, proj.knockBack, Player.whoAmI, Player.whoAmI);
                Main.projectile[projectile].As<Fireblossom>().SetPosition(Player.Center + (target.Center - Player.Center).SafeNormalize(Vector2.Zero) * Player.width, Player);
            }
        }

        public override void ModifyHitNPCWithItem(Item item, NPC target, ref NPC.HitModifiers modifiers) {
            if (target.immortal) {
                return;
            }

            if (Player.whoAmI != Main.myPlayer) {
                return;
            }

            if (!item.IsADruidicWeapon()) {
                return;
            }

            if (IsEffectActive && Main.rand.NextChance(0.2) && target.FindBuffIndex(ModContent.BuffType<Buffs.Fireblossom>()) == -1) {
                int type = ModContent.ProjectileType<Fireblossom>();
                int projectile = Projectile.NewProjectile(target.GetSource_OnHit(target), target.Center, Vector2.Zero, type, item.damage * 2, item.knockBack, Player.whoAmI, target.whoAmI);
                Main.projectile[projectile].As<Fireblossom>().SetPosition(Player.itemLocation, target);

                projectile = Projectile.NewProjectile(target.GetSource_OnHit(target), target.Center, Vector2.Zero, type, item.damage * 2, item.knockBack, Player.whoAmI, Player.whoAmI);
                Main.projectile[projectile].As<Fireblossom>().SetPosition(Player.Center + (target.Center - Player.Center).SafeNormalize(Vector2.Zero) * Player.width, Player);
            }
        }
    }
}
