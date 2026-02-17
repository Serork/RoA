using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class EvilStaff1 : ModDust {
    public override void SetStaticDefaults() => UpdateType = DustID.CrimsonPlants;
}

sealed class EvilStaff2 : ModDust {
    public override void SetStaticDefaults() => UpdateType = DustID.CorruptPlants;
}