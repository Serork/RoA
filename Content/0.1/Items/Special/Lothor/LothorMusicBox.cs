using RoA.Core;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Special.Lothor;

sealed class LothorMusicBox : ModItem {
    public override void SetStaticDefaults() {
        // DisplayName.SetDefault("Music Box (Lothor)");
        ItemID.Sets.CanGetPrefixes[Type] = false;
        ItemID.Sets.ShimmerTransformToItem[Type] = ItemID.MusicBox;

        Item.ResearchUnlockCount = 1;

        MusicLoader.AddMusicBox(Mod, MusicLoader.GetMusicSlot(RoA.MusicMod, ResourceManager.Music + "Lothor"), ModContent.ItemType<LothorMusicBox>(), ModContent.TileType<Tiles.Miscellaneous.LothorMusicBox>());
    }

    public override void SetDefaults() {
        Item.useStyle = 1;
        Item.useTurn = true;
        Item.useAnimation = 15;
        Item.useTime = 10;
        Item.autoReuse = true;
        Item.consumable = true;
        Item.createTile = ModContent.TileType<Tiles.Miscellaneous.LothorMusicBox>();
        Item.width = 32;
        Item.height = 28;
        Item.rare = 4;
        Item.value = 100000;
        Item.accessory = true;
        Item.hasVanityEffects = true;
    }
}