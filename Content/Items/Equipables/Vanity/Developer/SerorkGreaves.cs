﻿using Microsoft.Xna.Framework;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Vanity.Developer;

[AutoloadEquip(EquipType.Legs)]
sealed class SerorkGreaves : ModItem {
    public override void SetStaticDefaults() {
        //DisplayName.SetDefault("Peegeon's Greaves");
        //Tooltip.SetDefault("'Great for impersonating RoA devs?' Sure!");
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
    }

    public override void SetDefaults() {
        int width = 22; int height = 20;
        Item.Size = new Vector2(width, height);

        Item.sellPrice(gold: 5);
        Item.rare = ItemRarityID.Cyan;

        Item.vanity = true;
    }
}
