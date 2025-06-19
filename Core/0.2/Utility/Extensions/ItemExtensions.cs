using Terraria;
using Terraria.ModLoader;

namespace RoA.Core.Utility.Extensions;

static partial class ItemExtensions {
    public static bool IsModded(this Item item) => item.ModItem is not null;
    public static bool IsModded(this Item item, out ModItem modItem) {
        modItem = item.ModItem;
        return modItem is not null;
    }
}
