using Microsoft.Xna.Framework;

using System.Collections.Generic;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Accessories;

sealed class JewellersBelt : ModItem {
    public override void SetDefaults() {
        int width = 32; int height = 24;
        Item.Size = new Vector2(width, height);

        Item.rare = ItemRarityID.Blue;
        Item.accessory = true;

        Item.value = Item.sellPrice(0, 0, 10, 0);
    }

    private class JewellersBeltHandler : ModPlayer {
        public bool ShouldHideLargeGems, StopDropppingLargeGems;

        public override void Load() {
            On_Player.KillMe += On_Player_KillMe;
            On_PlayerDrawLayers.DrawPlayer_36_CTG += On_PlayerDrawLayers_DrawPlayer_36_CTG;
        }

        private void On_PlayerDrawLayers_DrawPlayer_36_CTG(On_PlayerDrawLayers.orig_DrawPlayer_36_CTG orig, ref PlayerDrawSet drawinfo) {
            if (drawinfo.drawPlayer.GetModPlayer<JewellersBeltHandler>().ShouldHideLargeGems) {
                return;
            }

            orig(ref drawinfo);
        }

        private void On_Player_KillMe(On_Player.orig_KillMe orig, Player self, Terraria.DataStructures.PlayerDeathReason damageSource, double dmg, int hitDirection, bool pvp) {
            bool flag = (self.difficulty == 0 || self.difficulty == 3) && self.GetModPlayer<JewellersBeltHandler>().StopDropppingLargeGems;
            Dictionary<int, int> savedIndexes = [];
            if (flag) {
                for (int i = 0; i < 59; i++) {
                    if (self.inventory[i].stack > 0 && ((self.inventory[i].type >= 1522 && self.inventory[i].type <= 1527) || self.inventory[i].type == 3643)) {
                        savedIndexes.TryAdd(i, self.inventory[i].type);
                    }
                }
                for (int i = 0; i < 59; i++) {
                    if (savedIndexes.ContainsKey(i)) {
                        self.inventory[i].type = ItemID.None;
                    }
                }
            }
            orig(self, damageSource, dmg, hitDirection, pvp);
            if (flag) {
                for (int i = 0; i < 59; i++) {
                    if (savedIndexes.ContainsKey(i)) {
                        self.inventory[i].type = savedIndexes[i];
                    }
                }
            }
        }

        public override void ResetEffects() => ShouldHideLargeGems = StopDropppingLargeGems = false;
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        var handler = player.GetModPlayer<JewellersBeltHandler>();
        handler.ShouldHideLargeGems = hideVisual;
        handler.StopDropppingLargeGems = true;
    }
}
