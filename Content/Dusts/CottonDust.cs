using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class CottonDust : ModDust {
    public override void SetStaticDefaults() => UpdateType = DustID.Cloud;

    public override bool PreDraw(Dust dust) {
        dust.QuickDraw(Texture2D.Value, Lighting.GetColor(dust.position.ToTileCoordinates()) * 0.9f * (1f - dust.alpha / 255f));

        return false;
    }
}
