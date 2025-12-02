using Microsoft.Xna.Framework;

using RoA.Content.Buffs;
using RoA.Core.Utility;

using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Miscellaneous;

[AutoloadEquip(EquipType.Head)]
sealed class ElderwoodHelmet : ModItem {
    public override void SetStaticDefaults() {
        // DisplayName.SetDefault("Elderwood Helmet");
        Item.ResearchUnlockCount = 1;
    }

    public override void SetDefaults() {
        int width = 22; int height = 22;
        Item.Size = new Vector2(width, height);

        Item.rare = ItemRarityID.White;
        Item.value = Item.sellPrice(silver: 10);

        Item.defense = 2;
    }

    public override bool IsArmorSet(Item head, Item body, Item legs)
        => body.type == ModContent.ItemType<ElderwoodChestplate>() && legs.type == ModContent.ItemType<ElderwoodLeggings>();

    public override void UpdateArmorSet(Player player) {
        player.setBonus = Language.GetText("Mods.RoA.Items.Tooltips.ElderwoodSetBonus").Value;

        player.GetModPlayer<ElderwoodSetBonusHandler>().IsSetBonusActive = true;
    }

    private class ElderwoodSetBonusHandler : ModPlayer {
        public bool IsSetBonusActive;

        public override void ResetEffects() {
            IsSetBonusActive = false;
        }

        public override void ModifyHurt(ref Player.HurtModifiers modifiers) {
            if (!IsSetBonusActive) {
                return;
            }

            if (Main.rand.NextChance(0.2)) {
                modifiers.FinalDamage *= 0.5f;
                Player.AddBuff(ModContent.BuffType<ElderwoodSetBonusBuff>(), ElderwoodSetBonusBuff.TIME);
            }
        }
    }
}