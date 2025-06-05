using Microsoft.Xna.Framework;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Miscellaneous;

[AutoloadEquip(EquipType.Head)]
sealed class OniMask : ModItem {
    public override void SetStaticDefaults() {
        //Tooltip.SetDefault("Scares away Youkai");
        ArmorIDs.Head.Sets.DrawFullHair[Item.headSlot] = true;
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
    }

    public override void SetDefaults() {
        int width = 26; int height = 32;
        Item.Size = new Vector2(width, height);

        Item.defense = 1;

        Item.rare = ItemRarityID.Blue;
        Item.value = Item.sellPrice(gold: 2);

        Item.useAnimation = 15;
        Item.useTime = 10;
        Item.useStyle = ItemUseStyleID.Swing;

        Item.useTurn = true;
        Item.autoReuse = true;

        Item.consumable = true;
        Item.createTile = ModContent.TileType<Tiles.Decorations.OniMask>();
    }

    public override void UpdateEquip(Player player) => player.AddBuff(ModContent.BuffType<Buffs.OniMask>(), 2);
}

sealed class CalmPlayer : ModPlayer {
    public bool oniMask;

    public override void ResetEffects()
        => oniMask = false;
}

sealed class CalmNPC : GlobalNPC {
    public override bool InstancePerEntity => true;

    public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns) {
        if (player.GetModPlayer<CalmPlayer>().oniMask) {
            spawnRate = (int)(float)(spawnRate * 1.3f);
            maxSpawns = (int)(maxSpawns * 0.7f);
        }
    }
}
