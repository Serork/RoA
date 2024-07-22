using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class BackwoodsFurniture : ModDust {
	public override void OnSpawn(Dust dust) => UpdateType = DustID.WoodFurniture;
}