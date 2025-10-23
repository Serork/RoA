using RoA.Common.Dusts;
using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class WoodFurniture : ModDust, IDrawDustPreNPCs {
    public override void SetStaticDefaults() => UpdateType = DustID.WoodFurniture;

    void IDrawDustPreNPCs.DrawPreNPCs(Dust dust) {
        dust.QuickDraw(Texture2D.Value);
    }

    public override bool PreDraw(Dust dust) => false;
}
