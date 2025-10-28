using Microsoft.Xna.Framework;

using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Miscellaneous;

[AutoloadEquip(EquipType.Head, EquipType.Face)]
sealed class DeerSkull : ModItem {
    public override void SetStaticDefaults() {
        ArmorIDs.Head.Sets.DrawsBackHairWithoutHeadgear[Item.headSlot] = true;

        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;

        ArmorIDs.Face.Sets.PreventHairDraw[Item.faceSlot] = true;
    }

    public override void SetDefaults() {
        int width = 32; int height = 24;
        Item.Size = new Vector2(width, height);

        Item.defense = 3;

        Item.rare = ItemRarityID.Orange;

        Item.value = Item.sellPrice(0, 0, 30, 0);

        Item.accessory = true;
    }

    public override void UpdateEquip(Player player) {
        player.GetCritChance(DruidClass.Nature) += 4;

        if (player.GetCommon().PerfectClotActivated) {
        }
    }

    public override bool IsArmorSet(Item head, Item body, Item legs) => body.type == ModContent.ItemType<CarcassChestguard>() && legs.type == ModContent.ItemType<CarcassSandals>();

    public override void UpdateArmorSet(Player player) {
        
    }

    public override bool CanEquipAccessory(Player player, int slot, bool modded) => player.GetCommon().PerfectClotActivated;

    public override void UpdateAccessory(Player player, bool hideVisual) {
        
    }
}
