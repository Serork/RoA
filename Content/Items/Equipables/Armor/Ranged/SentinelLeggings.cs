using Microsoft.Xna.Framework;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Armor.Ranged;

[AutoloadEquip(EquipType.Legs)]
sealed class SentinelLeggings : ModItem {
    public override void SetStaticDefaults() {
        //DisplayName.SetDefault("Sentinel Leggings");
        //Tooltip.SetDefault("Increases arrow damage by 10%");
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
    }

    public override void SetDefaults() {
        int width = 22; int height = 16;
        Item.Size = new Vector2(width, height);

        Item.rare = ItemRarityID.Green;
        Item.defense = 4;

        Item.value = Item.sellPrice(0, 0, 40, 0);
    }

    public override void UpdateEquip(Player player) => player.GetDamage(DamageClass.Ranged) += 0.07f;
}
