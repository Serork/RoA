using Microsoft.Xna.Framework;

using RoA.Content.Buffs;
using RoA.Core.Utility;

using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Miscellaneous;

[AutoloadEquip(EquipType.Head)]
sealed class DynastyWoodHelmet : ModItem {
    public override void SetStaticDefaults() {
        // DisplayName.SetDefault("Dynasty Wood Helmet");
        Item.ResearchUnlockCount = 1;
    }

    public override void SetDefaults() {
        int width = 24; int height = 20;
        Item.Size = new Vector2(width, height);

        Item.rare = ItemRarityID.White;
        Item.value = Item.sellPrice(0, 0, 1, 50);

        Item.defense = 2;
    }

    public override bool IsArmorSet(Item head, Item body, Item legs)
        => body.type == ModContent.ItemType<DynastyWoodBreastplate>() && legs.type == ModContent.ItemType<DynastyWoodLeggings>();

    public override void UpdateArmorSet(Player player) {
        player.setBonus = Language.GetText("Mods.RoA.Items.Tooltips.DynastySetBonus").Value;

        player.GetModPlayer<DynastyWoodSetBonusHandler>().IsSetBonusActive = true;
    }

    private class DynastyWoodSetBonusHandler : ModPlayer {
        public bool IsSetBonusActive;

        public override void ResetEffects() {
            IsSetBonusActive = false;
        }

        public override void OnHitAnything(float x, float y, Entity victim) {
            if (victim is NPC npc && !npc.CanActivateOnHitEffect()) {
                return;
            }

            if (!IsSetBonusActive) {
                return;
            }

            byte blocksAmount = 10;
            float neededDistance = blocksAmount * 16f;
            if (Player.Distance(new Vector2(x, y)) < neededDistance && !Player.HasBuff<DynastySetBonusBuff>()) {
                Player.AddBuff(ModContent.BuffType<DynastySetBonusBuff>(), DynastySetBonusBuff.TIME);
            }
        }
    }
}