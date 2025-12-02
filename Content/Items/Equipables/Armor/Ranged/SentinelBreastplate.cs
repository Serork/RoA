using Microsoft.Xna.Framework;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Armor.Ranged;

[AutoloadEquip(EquipType.Body)]
sealed class SentinelBreastplate : ModItem {
    public override void SetStaticDefaults() {
        //DisplayName.SetDefault("Sentinel Breastplate");
        //Tooltip.SetDefault("Increases arrow damage by 10%");
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
    }

    public override void SetDefaults() {
        int width = 26; int height = 20;
        Item.Size = new Vector2(width, height);

        Item.rare = ItemRarityID.Green;
        Item.defense = 6;

        Item.value = Item.sellPrice(0, 0, 50, 0);
    }

    public override void UpdateEquip(Player player) => player.GetDamage(DamageClass.Ranged) += 0.07f;
}

