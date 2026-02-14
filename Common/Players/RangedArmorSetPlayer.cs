using Microsoft.Xna.Framework;

using RoA.Common.CombatTexts;
using RoA.Content.Items.Weapons.Ranged;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;

using System.Collections.Generic;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.Players;

public class RangedArmorSetPlayer : ModPlayer {
    public float ArrowConsumptionReduce { get; internal set; }
    public float AllAmmoConsumptionReduce { get; internal set; }
    public bool TaurusArmorSet { get; internal set; }

    public byte ExtraCustomAmmoAmount;
    public float ExtraCustomAmmoConsumptionReduce;

    public Item UsedRangedWeaponWithCustomAmmo = null!;
    public List<(Item, bool)> CanReceiveCustomAmminition = [];

    public override void Unload() {
        CanReceiveCustomAmminition.Clear();
    }

    public void ReceiveCustomAmmunition(Item weaponWithCustomAmmo) {
        if (weaponWithCustomAmmo.IsEmpty() || !weaponWithCustomAmmo.IsModded()) {
            return;
        }
        if (weaponWithCustomAmmo.ModItem is not RangedWeaponWithCustomAmmo item) {
            return;
        }

        if (item.HasMaxAmmo(Player)) {
            return;
        }

        Color color = Color.White;

        switch (item.GetCurrentMaxAmmoAmount2(Player)) {
            case RangedWeaponWithCustomAmmo.BaseMaxAmmoAmount.Two:
                switch (item.GetCurrentAmmoAmount(Player)) {
                    case 0:
                        color = CustomCombatText.AmmorReceiveLightOrange;
                        break;
                    case 1:
                        color = CustomCombatText.AmmoReceiveGreen;
                        break;
                }
                break;
            case RangedWeaponWithCustomAmmo.BaseMaxAmmoAmount.Three:
                switch (item.GetCurrentAmmoAmount(Player)) {
                    case 0:
                        color = CustomCombatText.AmmoReceiveOrange;
                        break;
                    case 1:
                        color = CustomCombatText.AmmoReceiveYellow;
                        break;
                    case 2:
                        color = CustomCombatText.AmmoReceiveGreen;
                        break;
                }
                break;
            case RangedWeaponWithCustomAmmo.BaseMaxAmmoAmount.Four:
                switch (item.GetCurrentAmmoAmount(Player)) {
                    case 0:
                        color = CustomCombatText.AmmoReceiveRed;
                        break;
                    case 1:
                        color = CustomCombatText.AmmorReceiveLightOrange;
                        break;
                    case 2:
                        color = CustomCombatText.AmmoReceiveYellow;
                        break;
                    case 3:
                        color = CustomCombatText.AmmoReceiveGreen;
                        break;
                }
                break;
        }

        CustomCombatText.NewText(new Rectangle((int)Player.position.X, (int)Player.position.Y, Player.width, Player.height),
            color, "+1", dramatic: false, customAmmoReceive: true);

        item.RecoverAmmo(Player, true);
    }

    public override void ResetEffects() {
        ArrowConsumptionReduce = 0;
        AllAmmoConsumptionReduce = 0;
        TaurusArmorSet = false;

        ExtraCustomAmmoAmount = 0;
        ExtraCustomAmmoConsumptionReduce = 0f;
    }

    private class ExtraArrowLogicHandler : GlobalItem {
        public override void ModifyShootStats(Item item, Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
            if (player.GetModPlayer<RangedArmorSetPlayer>().TaurusArmorSet && (item.useAmmo == AmmoID.Arrow || item.useAmmo == AmmoID.Stake || AmmoID.Sets.IsArrow[item.useAmmo])) {
                if ((player.velocity.X > 1f && player.direction > 0) || (player.velocity.X < 0f && player.direction < 0)) velocity.X *= 1.8f;
                else velocity.X *= 1f;
            }
        }

        public override bool CanConsumeAmmo(Item weapon, Item ammo, Player player) {
            var handler = player.GetModPlayer<RangedArmorSetPlayer>();
            if (ammo.type == AmmoID.Arrow || ammo.type == AmmoID.Stake || AmmoID.Sets.IsArrow[ammo.type]) {
                return Main.rand.NextFloat() > handler.ArrowConsumptionReduce;
            }

            if (handler.AllAmmoConsumptionReduce > 0f) {
                return Main.rand.NextFloat() > handler.AllAmmoConsumptionReduce;
            }

            return base.CanConsumeAmmo(weapon, ammo, player);
        }
    }
}
