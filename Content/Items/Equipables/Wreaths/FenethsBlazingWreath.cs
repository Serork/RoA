using Microsoft.Xna.Framework;

using RoA.Common.Druid.Wreath;
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

	private class FenethsBlazingWreathHandler : ModPlayer {
		public bool IsEffectActive;

        public override void ResetEffects() {
			IsEffectActive = false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
            if (IsEffectActive && /*Main.rand.NextChance(0.2) && */target.FindBuffIndex(ModContent.BuffType<Buffs.Fireblossom>()) == -1) {
                int type = ModContent.ProjectileType<Projectiles.Friendly.Druidic.Fireblossom>();
                Projectile.NewProjectile(target.GetSource_OnHit(target), target.Center, Vector2.Zero, type, damageDone, hit.Knockback, Player.whoAmI, target.whoAmI);
            }
        }

        //public override void OnHitNPC(Item item, NPC target, int damage, float knockback, bool crit) {
        //    if (blazingWreath && Main.rand.NextChance(0.2) && target.FindBuffIndex(ModContent.BuffType<Buffs.Fireblossom>()) == -1) {
        //        int _type = ModContent.ProjectileType<Projectiles.Friendly.Druid.Fireblossom>();
        //        Projectile.NewProjectile(target.GetProjectileSpawnSource(), target.Center, Vector2.Zero, _type, damage, knockback, Player.whoAmI, target.whoAmI);
        //    }
        //}

        //public override void OnHitNPCWithProj(Projectile proj, NPC target, int damage, float knockback, bool crit) {
        //    if (blazingWreath && Main.rand.NextChance(0.2) && target.FindBuffIndex(ModContent.BuffType<Buffs.Fireblossom>()) == -1) {
        //        int _type = ModContent.ProjectileType<Projectiles.Friendly.Druid.Fireblossom>();
        //        Projectile.NewProjectile(target.GetProjectileSpawnSource(), target.Center, Vector2.Zero, _type, damage, knockback, proj.owner, target.whoAmI);
        //    }
        //}
    }
}
