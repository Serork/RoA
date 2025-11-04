using RoA.Content.Items.Placeable.Banners;
using RoA.Content.NPCs.Enemies.Backwoods;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Miscellaneous;

sealed class MonsterBanners : ModBannerTile {
    public enum StyleID : byte {
        Archdruid,
        Grimdruid,
        Hog,
        Lumberjack,
        BabyFleder,
        Fleder,
        Ent,
        Ravencaller,
        PettyGoblin,
        Hedgehog,
        SapSlime,
        Sentinel,
        SentinelWarrior,
        GrimDefender,
        BackwoodsRaven,
        DeerSkull,
        MurkyCarcass,
        WardenOfTheWoods,
        WardenOfTheWoods_Alt,
        Menhir,
        TarSlime,
        ElderSnail,
        PerfectMimic
    }

    public override void NearbyEffects(int i, int j, bool closer) {
        base.NearbyEffects(i, j, closer);

        if (closer) {
            return;
        }

        // Calculate the tile place style, then map that place style to an ItemID and BannerID.
        int tileStyle = TileObjectData.GetTileStyle(Main.tile[i, j]);
        int itemType = TileLoader.GetItemDropFromTypeAndStyle(Type, tileStyle);
        int bannerID = NPCLoader.BannerItemToNPC(itemType);

        if (itemType != ModContent.ItemType<BackwoodsRavenBanner>()) {
            return;
        }

        //if (bannerID != ModContent.NPCType<SummonedRaven>()) {
        //    return;
        //}

        // Once the BannerID and Item type have been calculated, we apply the banner buff
        if (ItemID.Sets.BannerStrength[itemType].Enabled) {
            Main.SceneMetrics.NPCBannerBuff[ModContent.NPCType<CrowdRaven>()] = true;
            Main.SceneMetrics.hasBanner = true;
        }
    }
}

