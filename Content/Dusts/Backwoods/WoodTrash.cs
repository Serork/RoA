using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Dusts.Backwoods;

sealed class WoodTrash : ModDust {
    public override void OnSpawn(Dust dust) => UpdateType = DustID.RuneWizard;
}