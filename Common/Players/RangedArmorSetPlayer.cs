using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace RoA.Common.Players;

public class RangedArmorSetPlayer : ModPlayer {
    public float ArrowConsumptionReduce { get; internal set; }
    public bool TaurusArmorSet { get; internal set; }

    public override void ResetEffects() {
        ArrowConsumptionReduce = 0;
        TaurusArmorSet = false;
    }

    private class ExtraArrowLogicHandler : GlobalItem {
        public override void ModifyShootStats(Item item, Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
            if (player.GetModPlayer<RangedArmorSetPlayer>().TaurusArmorSet && (item.useAmmo == AmmoID.Arrow || item.useAmmo == AmmoID.Stake || AmmoID.Sets.IsArrow[item.useAmmo])) {
                if ((player.velocity.X > 1f && player.direction > 0) || (player.velocity.X < 0f && player.direction < 0)) velocity.X *= 1.8f;
                else velocity.X *= 1f;
            }
        }

        public override bool CanConsumeAmmo(Item weapon, Item ammo, Player player) {
            if (ammo.type == AmmoID.Arrow || ammo.type == AmmoID.Stake || AmmoID.Sets.IsArrow[ammo.type]) {
                return Main.rand.NextFloat() > player.GetModPlayer<RangedArmorSetPlayer>().ArrowConsumptionReduce;
            }

            return base.CanConsumeAmmo(weapon, ammo, player);
        }
    }
}
