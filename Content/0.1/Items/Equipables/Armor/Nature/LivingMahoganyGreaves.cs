using Microsoft.Xna.Framework;

using RoA.Common.Items;
using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Armor.Nature;

[AutoloadEquip(EquipType.Legs)]
sealed class LivingMahoganyGreaves : NatureItem {
    public override void SetStaticDefaults() {
        //DisplayName.SetDefault("Living Mahogany Greaves");
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
    }

    protected override void SafeSetDefaults() {
        int width = 22; int height = 18;
        Item.Size = new Vector2(width, height);

        Item.rare = ItemRarityID.Blue;
        Item.value = Item.sellPrice(0, 0, 20, 0);

        Item.defense = 2;
    }

    public override void UpdateEquip(Player player) => player.GetDruidStats().ClawsAttackSpeedModifier += 0.04f; //player.GetCritChance(DruidClass.Nature) += 3;
}