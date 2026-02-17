using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Dusts.Backwoods;

sealed class Furniture : ModDust {
    public override void SetStaticDefaults() => UpdateType = DustID.WoodFurniture;
}