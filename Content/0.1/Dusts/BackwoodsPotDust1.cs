using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

class BackwoodsPotDust2 : BackwoodsPotDust1 {
}

class BackwoodsPotDust1 : ModDust {
    public override void OnSpawn(Dust dust) => UpdateType = DustID.Pot;
}