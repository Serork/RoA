using Terraria.ModLoader;

namespace RoA.Core.Utility.Vanilla;

static class ItemUtils {
    public static string GetTexturePath<T>() where T : ModItem => ItemLoader.GetItem(ModContent.ItemType<T>()).Texture;
}
