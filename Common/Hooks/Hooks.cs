using RoA.Content.Items.Equipables.Accessories;
using RoA.Content.Items.Miscellaneous;
using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.Hooks;

sealed partial class Hooks : ModSystem {
    public override void Load() {
        LoadPlayerHooks();

        On_NPC.NPCLoot_DropCommonLifeAndMana += On_NPC_NPCLoot_DropCommonLifeAndMana;
    }


    private void On_NPC_NPCLoot_DropCommonLifeAndMana(On_NPC.orig_NPCLoot_DropCommonLifeAndMana orig, NPC self, Player closestPlayer) {
        int getHerb() {
            int rand = Main.rand.Next(3);
            return rand switch {
                0 => ModContent.ItemType<MagicHerb1>(),
                1 => ModContent.ItemType<MagicHerb2>(),
                _ => ModContent.ItemType<MagicHerb3>(),
            };
        }

        bool dropHerb = false;
        if (closestPlayer.GetModPlayer<HerbariumPlayer>().healingHerb && !NPCID.Sets.NeverDropsResourcePickups[self.type] && closestPlayer.RollLuck(6) == 0 && self.lifeMax > 1 && self.damage > 0) {
            if (Main.rand.Next(2) == 0 && closestPlayer.statLife < closestPlayer.statLifeMax2)
                Item.NewItem(self.GetSource_Loot(), (int)self.position.X, (int)self.position.Y, self.width, self.height, getHerb());

            dropHerb = true;
        }

        int heartType = ItemID.Heart;

        if (closestPlayer.GetCommon().IsBansheesGuardEffectActive && Main.rand.NextBool()) {
            heartType = ModContent.ItemType<AvengingSoul>();
        }

        //if (dropHerb) {
        //    return;
        //}

        //orig(self, closestPlayer);

        if (!NPCID.Sets.NeverDropsResourcePickups[self.type] && closestPlayer.RollLuck(6) == 0 && self.lifeMax > 1 && self.damage > 0) {
            if (Main.rand.Next(2) == 0 && closestPlayer.statMana < closestPlayer.statManaMax2)
                Item.NewItem(self.GetSource_Loot(), (int)self.position.X, (int)self.position.Y, self.width, self.height, 184);
            else if (Main.rand.Next(2) == 0 && closestPlayer.statLife < closestPlayer.statLifeMax2)
                Item.NewItem(self.GetSource_Loot(), (int)self.position.X, (int)self.position.Y, self.width, self.height, heartType);
        }

        if (!NPCID.Sets.NeverDropsResourcePickups[self.type] && closestPlayer.RollLuck(2) == 0 && self.lifeMax > 1 && self.damage > 0 && closestPlayer.statMana < closestPlayer.statManaMax2)
            Item.NewItem(self.GetSource_Loot(), (int)self.position.X, (int)self.position.Y, self.width, self.height, 184);
    }

    public partial void LoadPlayerHooks();
}
