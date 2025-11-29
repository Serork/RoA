using RoA.Core;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Placeable.MusicBoxes;

// backwoods music box
sealed class BackwoodsDayMusicBox : ModItem {
    public override void SetStaticDefaults() {
        ItemID.Sets.CanGetPrefixes[Type] = false;
        ItemID.Sets.ShimmerTransformToItem[Type] = ItemID.MusicBox;

        Item.ResearchUnlockCount = 1;

        MusicLoader.AddMusicBox(Mod, MusicLoader.GetMusicSlot(RoA.MusicMod, ResourceManager.Music + "BackwoodsDay"), ModContent.ItemType<BackwoodsDayMusicBox>(), ModContent.TileType<Tiles.Miscellaneous.BackwoodsDayMusicBox>());
    }

    public override void SetDefaults() {
        Item.useStyle = 1;
        Item.useTurn = true;
        Item.useAnimation = 15;
        Item.useTime = 10;
        Item.autoReuse = true;
        Item.consumable = true;
        Item.createTile = ModContent.TileType<Tiles.Miscellaneous.BackwoodsDayMusicBox>();
        Item.width = 30;
        Item.height = 20;
        Item.rare = 4;
        Item.value = 100000;
        Item.accessory = true;
        Item.hasVanityEffects = true;
    }
}