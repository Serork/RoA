using Microsoft.Xna.Framework;
using RoA.Common.Players;
using RoA.Content.Items.Consumables;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Vanity;

[AutoloadEquip(EquipType.Head)]
sealed class SoapSellersShades : ModItem {
    public override void SetStaticDefaults() {
        Item.ResearchUnlockCount = 1;

        ArmorIDs.Head.Sets.DrawFullHair[Item.headSlot] = true;

        ItemID.Sets.ShimmerTransformToItem[ItemID.FamiliarWig] = Type;
    }

    public override void SetDefaults() {
        int width = 20; int height = 10;
        Item.Size = new Vector2(width, height);

        Item.rare = ItemRarityID.White;
        Item.value = Item.sellPrice(copper: 65);

        Item.vanity = true;
    }
}