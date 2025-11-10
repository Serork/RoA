using Microsoft.Xna.Framework;

using RoA.Common.Dusts;
using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class WoodFurniture : ModDust, IDrawDustPreNPCs {
    public override void SetStaticDefaults() => UpdateType = DustID.WoodFurniture;

    public override Color? GetAlpha(Dust dust, Color lightColor) => lightColor.MultiplyRGB(dust.color);

    void IDrawDustPreNPCs.DrawPreNPCs(Dust dust) {
        dust.QuickDraw(Texture2D.Value);
    }

    public override bool PreDraw(Dust dust) => false;
}
