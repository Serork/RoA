using Microsoft.Xna.Framework;

using RoA.Content.Items.Miscellaneous;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Accessories;

sealed class Herbarium : NatureItem {
    public override void SetStaticDefaults() {
        //DisplayName.SetDefault("Herbarium");
        //Tooltip.SetDefault("Enemies can drop healing herbs when wreath is fully charged");
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
    }

    protected override void SafeSetDefaults() {
        int width = 28; int height = 32;
        Item.Size = new Vector2(width, height);

        Item.rare = ItemRarityID.Green;
        Item.accessory = true;

        Item.value = Item.sellPrice(0, 0, 3, 0);
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        player.GetModPlayer<HerbariumPlayer>().healingHerb = true;
    }
}

internal class HerbariumPlayer : ModPlayer {
    public bool healingHerb;

    public override void ResetEffects()
        => healingHerb = false;

    public override void Load() {
        On_NPC.NPCLoot_DropCommonLifeAndMana += On_NPC_NPCLoot_DropCommonLifeAndMana;
    }

    private void On_NPC_NPCLoot_DropCommonLifeAndMana(On_NPC.orig_NPCLoot_DropCommonLifeAndMana orig, NPC self, Player closestPlayer) {
        if (self.type != NPCID.MotherSlime && self.type != NPCID.CorruptSlime && self.type != NPCID.Slimer &&
            closestPlayer.RollLuck(6) == 0 && self.lifeMax > 1 && self.damage > 0) {
            if (Main.rand.NextBool(2) && closestPlayer.statMana < closestPlayer.statManaMax2)
                Item.NewItem(self.GetSource_Loot(), (int)self.position.X, (int)self.position.Y, self.width, self.height, 184);
            else if (Main.rand.NextBool(2) && closestPlayer.statLife < closestPlayer.statLifeMax2)
                Item.NewItem(self.GetSource_Loot(), (int)self.position.X, (int)self.position.Y, self.width, self.height, 58);
        }

        if (self.type != NPCID.MotherSlime && self.type != NPCID.CorruptSlime && self.type != NPCID.Slimer &&
            closestPlayer.RollLuck(2) == 0 && self.lifeMax > 1 && self.damage > 0 &&
            closestPlayer.statMana < closestPlayer.statManaMax2)
            Item.NewItem(self.GetSource_Loot(), (int)self.position.X, (int)self.position.Y, self.width, self.height, 184);

        int getHerb() {
            int rand = Main.rand.Next(3);
            return rand switch {
                0 => ModContent.ItemType<MagicHerb1>(),
                1 => ModContent.ItemType<MagicHerb2>(),
                _ => ModContent.ItemType<MagicHerb3>(),
            };
        }
        if (closestPlayer.GetModPlayer<HerbariumPlayer>().healingHerb &&
            self.type != NPCID.MotherSlime && self.type != NPCID.CorruptSlime && self.type != NPCID.Slimer &&
            closestPlayer.RollLuck(2) == 0 && self.lifeMax > 1 && self.damage > 0)
            Item.NewItem(self.GetSource_Loot(), (int)self.position.X, (int)self.position.Y, self.width, self.height, getHerb());
    }
}
