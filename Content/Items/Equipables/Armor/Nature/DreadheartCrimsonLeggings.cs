using Microsoft.Xna.Framework;

using RoA.Common.Items;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Armor.Nature;

[AutoloadEquip(EquipType.Legs)]
sealed class DreadheartCrimsonLeggings : NatureItem {
    public override void SetStaticDefaults() {
        //DisplayName.SetDefault("Dreadheart Leggings");
        //Tooltip.SetDefault("4% increased nature critical strike chance");
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
    }

    protected override void SafeSetDefaults() {
        int width = 22; int height = 18;
        Item.Size = new Vector2(width, height);

        Item.rare = ItemRarityID.Green;
        Item.value = Item.sellPrice(0, 0, 60, 0);

        Item.defense = 5;
    }

    public override void UpdateEquip(Player player) => player.GetCritChance(DruidClass.Nature) += 5;
}