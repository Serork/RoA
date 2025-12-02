using Microsoft.Xna.Framework;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Armor.Summon;

[AutoloadEquip(EquipType.Head)]
sealed class FlinxFurUshanka : ModItem {
    public override void SetStaticDefaults() {
        //DisplayName.SetDefault("Flinx Fur Ushanka");
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
    }

    public override void SetDefaults() {
        int width = 20; int height = 18;
        Item.Size = new Vector2(width, height);

        Item.rare = ItemRarityID.Green;
        Item.value = Item.sellPrice(0, 0, 50, 0);

        Item.defense = 1;
    }

    public override void UpdateEquip(Player player) {
        player.whipRangeMultiplier += 0.1f;
    }

    public override bool IsArmorSet(Item head, Item body, Item legs) => body.type == ItemID.FlinxFurCoat;

    public override void UpdateArmorSet(Player player) {
        player.setBonus = Language.GetText("Mods.RoA.Items.Tooltips.FlinxFurUshankaSetBonus").Value;
        player.buffImmune[BuffID.Chilled] = true;
        player.buffImmune[BuffID.Frozen] = true;
    }
}